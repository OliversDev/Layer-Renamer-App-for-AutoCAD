using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using DataTable = System.Data.DataTable;

namespace AutoCADLayerRenamer
{
    public partial class LayerRenameForm : Form
    {
        private static readonly Regex RenameCommandPattern = new Regex(
            @"^\(\s*command\s+""(?<command>[^""]+)""\s+""(?<type>[^""]+)""\s+""(?<old>(?:\\.|[^""])*)""\s+""(?<new>(?:\\.|[^""])*)""\s*\)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private readonly DataTable layerTable = new DataTable();
        private readonly List<LayerInfo> allLayers = new List<LayerInfo>();
        private readonly HashSet<string> selectedLayerNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ObjectId> existingLayers =
            new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);
        private bool restoringSelection;

        public LayerRenameForm()
        {
            InitializeComponent();
            ApplyApplicationIcon();
            InitializeLayerTable();
            LayerRenamerTheme.Apply(this, btnRename, footerPanel, Logo, GitHub, LinkedIn);
            ConfigureGrid();
            LoadLayers();
            UpdateScriptDisplay();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            try
            {
                var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (document != null)
                    document.Window.Focus();
                else
                    Autodesk.AutoCAD.ApplicationServices.Application.MainWindow.Focus();
            }
            catch
            {
                // Focus recovery must never prevent the form from closing.
            }
        }

        private void ApplyApplicationIcon()
        {
            using (Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("AutoCADLayerRenamer.LayerRenamerIcon.ico"))
            {
                if (stream == null) return;
                using (Icon source = new Icon(stream))
                    Icon = (Icon)source.Clone();
            }
        }

        private void InitializeLayerTable()
        {
            layerTable.Columns.Add("LayerId", typeof(ObjectId));
            layerTable.Columns.Add("LayerName", typeof(string));
            layerTable.Columns.Add("Color", typeof(string));
            layerTable.Columns.Add("Linetype", typeof(string));
            layerTable.Columns.Add("IsFrozen", typeof(bool));
            layerTable.Columns.Add("IsLocked", typeof(bool));
            dataGridViewLayers.DataSource = layerTable;
        }

        private void ConfigureGrid()
        {
            dataGridViewLayers.Columns["LayerId"].Visible = false;
            dataGridViewLayers.Columns["LayerName"].HeaderText = "Layer Name";
            dataGridViewLayers.Columns["Linetype"].HeaderText = "Line Type";
            dataGridViewLayers.Columns["IsFrozen"].HeaderText = "Frozen";
            dataGridViewLayers.Columns["IsLocked"].HeaderText = "Locked";
            dataGridViewLayers.Columns["LayerName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewLayers.Columns["Color"].Width = 90;
            dataGridViewLayers.Columns["Linetype"].Width = 110;
            dataGridViewLayers.Columns["IsFrozen"].Width = 64;
            dataGridViewLayers.Columns["IsLocked"].Width = 64;

            dataGridViewLayers.ColumnHeadersDefaultCellStyle.Font =
                new System.Drawing.Font(dataGridViewLayers.Font, System.Drawing.FontStyle.Bold);
            dataGridViewLayers.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;
            dataGridViewLayers.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            dataGridViewLayers.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            foreach (string name in new[] { "IsFrozen", "IsLocked" })
            {
                var column = dataGridViewLayers.Columns[name] as DataGridViewCheckBoxColumn;
                if (column == null) continue;

                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.ReadOnly = true;
                column.FlatStyle = FlatStyle.Flat;
                column.ThreeState = false;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.DefaultCellStyle.Padding = new Padding(0);
                column.DefaultCellStyle.ForeColor = SystemColors.GrayText;
            }

            dataGridViewLayers.CellPainting += dataGridViewLayers_CellPainting;
        }

        private void dataGridViewLayers_CellPainting(
            object sender,
            DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string columnName = dataGridViewLayers.Columns[e.ColumnIndex].Name;
            if (!string.Equals(columnName, "IsFrozen", StringComparison.Ordinal) &&
                !string.Equals(columnName, "IsLocked", StringComparison.Ordinal))
            {
                return;
            }

            e.PaintBackground(e.CellBounds, true);
            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);

            bool isChecked = e.FormattedValue != null && Convert.ToBoolean(e.FormattedValue);
            LayerRenamerTheme.DrawReadOnlyCheckBox(e.Graphics, e.CellBounds, isChecked);
            e.Handled = true;
        }

        private void LoadLayers()
        {
            SaveSelectedLayers();
            allLayers.Clear();
            existingLayers.Clear();

            var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (document == null)
            {
                ShowMessageDialog("No active drawing is available.", "Layer Renamer", MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var layers = (LayerTable)transaction.GetObject(
                        document.Database.LayerTableId,
                        OpenMode.ForRead);

                    foreach (ObjectId id in layers)
                    {
                        var layer = (LayerTableRecord)transaction.GetObject(id, OpenMode.ForRead);
                        existingLayers[layer.Name] = id;

                        if (!IsRenameable(layer)) continue;

                        string linetype = "ByLayer";
                        if (!layer.LinetypeObjectId.IsNull && layer.LinetypeObjectId.IsValid)
                        {
                            try
                            {
                                var record = transaction.GetObject(
                                    layer.LinetypeObjectId,
                                    OpenMode.ForRead) as LinetypeTableRecord;
                                if (record != null) linetype = record.Name;
                            }
                            catch
                            {
                                // Keep the fallback linetype name.
                            }
                        }

                        allLayers.Add(new LayerInfo(
                            id,
                            layer.Name,
                            layer.Color.ToString(),
                            linetype,
                            layer.IsFrozen,
                            layer.IsLocked));
                    }

                    transaction.Commit();
                }

                allLayers.Sort((left, right) =>
                    StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name));

                var validNames = new HashSet<string>(
                    allLayers.Select(layer => layer.Name),
                    StringComparer.OrdinalIgnoreCase);
                selectedLayerNames.RemoveWhere(name => !validNames.Contains(name));

                ApplyFilter();
            }
            catch (Exception ex)
            {
                ShowMessageDialog(
                    "Layers could not be loaded.\r\n\r\n" + ex.Message,
                    "Layer Renamer",
                    MessageBoxIcon.Error);
            }
        }

        private static bool IsRenameable(LayerTableRecord layer)
        {
            return layer != null &&
                   !string.IsNullOrWhiteSpace(layer.Name) &&
                   !layer.IsDependent &&
                   !string.Equals(layer.Name, "0", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(layer.Name, "Defpoints", StringComparison.OrdinalIgnoreCase) &&
                   layer.Name.IndexOf('|') < 0;
        }

        private void dataGridViewLayers_SelectionChanged(object sender, EventArgs e)
        {
            if (restoringSelection) return;
            SaveSelectedLayers();
            UpdateSelectionLabel();
        }

        private void SaveSelectedLayers()
        {
            if (dataGridViewLayers == null) return;

            foreach (DataGridViewRow row in dataGridViewLayers.Rows)
            {
                object rawName = row.Cells["LayerName"].Value;
                if (rawName == null) continue;

                string layerName = rawName.ToString();
                if (row.Selected)
                    selectedLayerNames.Add(layerName);
                else
                    selectedLayerNames.Remove(layerName);
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            SaveSelectedLayers();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string filter = txtFilter.Text.Trim();
            IEnumerable<LayerInfo> filteredLayers =
                allLayers.Where(layer => MatchesFilter(layer.Name, filter));

            restoringSelection = true;
            try
            {
                layerTable.Rows.Clear();

                foreach (LayerInfo layer in filteredLayers)
                {
                    layerTable.Rows.Add(
                        layer.Id,
                        layer.Name,
                        layer.Color,
                        layer.Linetype,
                        layer.IsFrozen,
                        layer.IsLocked);
                }

                dataGridViewLayers.ClearSelection();
                foreach (DataGridViewRow row in dataGridViewLayers.Rows)
                {
                    object rawName = row.Cells["LayerName"].Value;
                    if (rawName != null && selectedLayerNames.Contains(rawName.ToString()))
                        row.Selected = true;
                }
            }
            finally
            {
                restoringSelection = false;
            }

            UpdateSelectionLabel();
        }

        private static bool MatchesFilter(string layerName, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return true;

            bool containsWildcard = filter.IndexOf('*') >= 0;
            if (!containsWildcard)
                return string.Equals(layerName, filter, StringComparison.OrdinalIgnoreCase);

            string pattern = "^" + Regex.Escape(filter)
                .Replace(@"\*", ".*") + "$";

            return Regex.IsMatch(
                layerName,
                pattern,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private void RenameOptionChanged(object sender, EventArgs e)
        {
            bool useExactName = !string.IsNullOrWhiteSpace(txtExactName.Text);
            txtPrefix.Enabled = !useExactName;
            txtSuffix.Enabled = !useExactName;
            txtFind.Enabled = !useExactName;
            txtReplace.Enabled = !useExactName;
        }

        private string BuildNewName(string oldName)
        {
            if (!string.IsNullOrWhiteSpace(txtExactName.Text))
                return txtExactName.Text.Trim();

            string name = oldName;
            if (txtFind.Text.Length > 0)
                name = ReplaceOrdinalIgnoreCase(name, txtFind.Text, txtReplace.Text);

            return txtPrefix.Text + name + txtSuffix.Text;
        }

        private static string ReplaceOrdinalIgnoreCase(
            string input,
            string find,
            string replacement)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(find)) return input;

            replacement = replacement ?? string.Empty;
            int startIndex = 0;
            var builder = new StringBuilder();

            while (true)
            {
                int index = input.IndexOf(find, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    builder.Append(input.Substring(startIndex));
                    break;
                }

                builder.Append(input.Substring(startIndex, index - startIndex));
                builder.Append(replacement);
                startIndex = index + find.Length;
            }

            return builder.ToString();
        }

        private List<RenameItem> BuildCurrentSelectionPlan()
        {
            return allLayers
                .Where(layer => selectedLayerNames.Contains(layer.Name))
                .Select(layer => new RenameItem(layer.Id, layer.Name, BuildNewName(layer.Name)))
                .Where(item => !string.Equals(item.OldName, item.NewName, StringComparison.Ordinal))
                .OrderBy(item => item.OldName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void btnAddToScriptList_Click(object sender, EventArgs e)
        {
            SaveSelectedLayers();

            if (selectedLayerNames.Count == 0)
            {
                ShowMessageDialog(
                    "Select one or more layers before adding rename commands.",
                    "Layer Renamer",
                    MessageBoxIcon.Information);
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtExactName.Text) && selectedLayerNames.Count != 1)
            {
                ShowMessageDialog(
                    "Rename To can only be used when exactly one layer is selected.",
                    "Layer Renamer",
                    MessageBoxIcon.Information);
                return;
            }

            List<RenameItem> selectionPlan = BuildCurrentSelectionPlan();
            if (selectionPlan.Count == 0)
            {
                ShowMessageDialog(
                    "The selected options do not change any layer names.",
                    "Layer Renamer",
                    MessageBoxIcon.Information);
                return;
            }

            string error;
            if (!ValidatePlan(selectionPlan, out error))
            {
                ShowMessageDialog(error, "Layer Renamer", MessageBoxIcon.Warning);
                return;
            }

            string commands = BuildGeneratedScript(selectionPlan).TrimEnd();
            if (txtGeneratedScript.TextLength > 0 &&
                !txtGeneratedScript.Text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                txtGeneratedScript.AppendText(Environment.NewLine);
            }

            txtGeneratedScript.AppendText(commands + Environment.NewLine);
        }

        private bool ValidatePlan(IList<RenameItem> plan, out string error)
        {
            foreach (RenameItem item in plan)
            {
                ObjectId currentId;
                if (!existingLayers.TryGetValue(item.OldName, out currentId) || currentId != item.Id)
                {
                    error = "Layer \"" + item.OldName + "\" is no longer available. Refresh the layer list and rebuild the script.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(item.NewName))
                {
                    error = "A layer name cannot be empty.";
                    return false;
                }

                if (item.NewName.Length > 255)
                {
                    error = "The resulting name for \"" + item.OldName + "\" exceeds AutoCAD's 255-character layer-name limit.";
                    return false;
                }

                if (ContainsInvalidCharacters(item.NewName))
                {
                    error = "The resulting name for \"" + item.OldName + "\" contains an invalid character.\r\n\r\n" +
                            "Invalid characters: < > / \\ \" : ; ? * | , =";
                    return false;
                }
            }

            IGrouping<string, RenameItem> duplicate = plan
                .GroupBy(item => item.NewName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
            {
                error = "More than one staged layer would be renamed to \"" + duplicate.Key +
                        "\". Change the rename options and add the selection again.";
                return false;
            }

            foreach (RenameItem item in plan)
            {
                ObjectId existingId;
                if (existingLayers.TryGetValue(item.NewName, out existingId) &&
                    existingId != item.Id)
                {
                    error = "A layer named \"" + item.NewName +
                            "\" already exists. Choose a different resulting name.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static bool ContainsInvalidCharacters(string value)
        {
            return value.IndexOfAny(new[] { '<', '>', '/', '\\', '"', ':', ';', '?', '*', '|', ',', '=' }) >= 0 ||
                   value.Any(char.IsControl);
        }

        private void UpdateScriptDisplay()
        {
            bool hasScript = !string.IsNullOrWhiteSpace(txtGeneratedScript.Text);
            btnCopyScript.Enabled = hasScript;
            btnSaveScript.Enabled = hasScript;
            btnClearScript.Enabled = hasScript;
            btnRename.Enabled = hasScript;
        }

        private void txtGeneratedScript_TextChanged(object sender, EventArgs e)
        {
            UpdateScriptDisplay();
        }

        private string BuildGeneratedScript(IList<RenameItem> plan)
        {
            List<string> lines = plan
                .Select(item => BuildRenameScriptCommand(item.OldName, item.NewName))
                .ToList();

            return NormalizeScriptText(string.Join(Environment.NewLine, lines), true);
        }

        private static string BuildRenameScriptCommand(string oldName, string newName)
        {
            return "(command \"_.-RENAME\" \"_Layer\" \"" + EscapeAutoLispString(oldName) +
                   "\" \"" + EscapeAutoLispString(newName) + "\")";
        }

        private static string EscapeAutoLispString(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        private static string NormalizeScriptText(string script, bool addFinalNewLine)
        {
            if (string.IsNullOrWhiteSpace(script)) return string.Empty;

            string normalized = string.Join(
                Environment.NewLine,
                script
                    .Replace("\r\n", "\n")
                    .Replace("\r", "\n")
                    .Split(new[] { '\n' }, StringSplitOptions.None)
                    .Select(line => line.TrimEnd()));

            return addFinalNewLine ? normalized.TrimEnd() + Environment.NewLine : normalized.TrimEnd();
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            string[] commands = txtGeneratedScript.Lines
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line) &&
                               !line.StartsWith(";", StringComparison.Ordinal))
                .ToArray();

            if (commands.Length == 0)
            {
                ShowMessageDialog(
                    "Enter or add one or more script commands first.",
                    "Layer Renamer",
                    MessageBoxIcon.Information);
                return;
            }

            string conflictWarning = BuildScriptConflictWarning(commands);
            bool confirmed = string.IsNullOrEmpty(conflictWarning)
                ? ShowConfirmationDialog(
                    "Run " + commands.Length + " script command" +
                    (commands.Length == 1 ? "?" : "s?"),
                    "Layer Renamer")
                : ShowWarningConfirmationDialog(
                    conflictWarning +
                    "\r\n\r\nThese commands may fail or produce incomplete results. Run the script anyway?",
                    "Layer Renamer - Potential Conflicts");

            if (!confirmed)
            {
                return;
            }

            try
            {
                var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (document == null)
                    throw new InvalidOperationException("No active AutoCAD drawing is available.");

                Hide();
                document.Window.Focus();

                foreach (string command in commands)
                    document.SendStringToExecute(command + "\n", true, false, false);

                BeginInvoke(new Action(Close));
            }
            catch (Exception ex)
            {
                Show();
                ShowMessageDialog(
                    "The rename script could not be started.\r\n\r\n" + ex.Message,
                    "Layer Renamer",
                    MessageBoxIcon.Error);
            }
        }

        private static string BuildScriptConflictWarning(IEnumerable<string> scriptLines)
        {
            List<ScriptRenameCommand> renameCommands = scriptLines
                .Select(ParseRenameCommand)
                .Where(command => command != null)
                .ToList();

            List<string> duplicateTargets = renameCommands
                .GroupBy(command => command.NewName, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            List<string> repeatedSources = renameCommands
                .GroupBy(command => command.OldName, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sections = new List<string>();
            if (duplicateTargets.Count > 0)
            {
                sections.Add(
                    "Multiple layers are being renamed to the same layer name:\r\n" +
                    string.Join("\r\n", duplicateTargets.Select(name => "  • " + name)));
            }

            if (repeatedSources.Count > 0)
            {
                sections.Add(
                    "The same existing layer is being renamed more than once:\r\n" +
                    string.Join("\r\n", repeatedSources.Select(name => "  • " + name)));
            }

            return string.Join("\r\n\r\n", sections);
        }

        private static ScriptRenameCommand ParseRenameCommand(string line)
        {
            if (string.IsNullOrWhiteSpace(line) ||
                line.TrimStart().StartsWith(";", StringComparison.Ordinal))
            {
                return null;
            }

            Match match = RenameCommandPattern.Match(line.Trim());
            if (!match.Success) return null;

            string commandName = match.Groups["command"].Value
                .Replace("_", string.Empty)
                .Replace(".", string.Empty);
            string objectType = match.Groups["type"].Value.Replace("_", string.Empty);
            if (!string.Equals(commandName, "-RENAME", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(objectType, "Layer", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new ScriptRenameCommand(
                UnescapeAutoLispString(match.Groups["old"].Value),
                UnescapeAutoLispString(match.Groups["new"].Value));
        }

        private static string UnescapeAutoLispString(string value)
        {
            var builder = new StringBuilder();
            for (int index = 0; index < value.Length; index++)
            {
                if (value[index] == '\\' && index + 1 < value.Length)
                    index++;

                builder.Append(value[index]);
            }

            return builder.ToString();
        }

        private void btnCopyScript_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGeneratedScript.Text)) return;

            try
            {
                Clipboard.SetText(txtGeneratedScript.Text);
                ShowMessageDialog(
                    "The staged script was copied to the clipboard.",
                    "Layer Renamer",
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowMessageDialog(
                    "The script could not be copied.\r\n\r\n" + ex.Message,
                    "Layer Renamer",
                    MessageBoxIcon.Error);
            }
        }

        private void btnSaveScript_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGeneratedScript.Text)) return;

            try
            {
                using (var dialog = new SaveFileDialog
                {
                    AddExtension = true,
                    DefaultExt = "scr",
                    FileName = "LayerRename.scr",
                    Filter = "AutoCAD scripts (*.scr)|*.scr",
                    OverwritePrompt = true,
                    RestoreDirectory = true,
                    Title = "Save Layer Rename Script"
                })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    File.WriteAllText(
                        dialog.FileName,
                        NormalizeScriptText(txtGeneratedScript.Text, true),
                        Encoding.ASCII);
                }

                ShowMessageDialog(
                    "The staged script was saved successfully.",
                    "Layer Renamer",
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowMessageDialog(
                    "The script could not be saved.\r\n\r\n" + ex.Message,
                    "Layer Renamer",
                    MessageBoxIcon.Error);
            }
        }

        private void btnClearScript_Click(object sender, EventArgs e)
        {
            txtGeneratedScript.Clear();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadLayers();
        }

        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            txtFilter.Clear();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UpdateSelectionLabel()
        {
            lblSelection.Text = selectedLayerNames.Count + " selected";
        }

        private void ShowMessageDialog(string message, string title, MessageBoxIcon icon)
        {
            ThemedMessageBox.Show(this, message, title, MessageBoxButtons.OK, icon);
        }

        private bool ShowConfirmationDialog(string message, string title)
        {
            return ThemedMessageBox.Show(
                this,
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private bool ShowWarningConfirmationDialog(string message, string title)
        {
            return ThemedMessageBox.Show(
                this,
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show(
                    null,
                    "Unable to open the link.\r\n\r\n" + ex.Message,
                    "Layer Renamer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void OpenBundledDocument(string fileName, string fallbackUrl)
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] candidates =
            {
                Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "Help", fileName)),
                Path.Combine(assemblyDirectory, fileName)
            };
            string localFile = candidates.FirstOrDefault(File.Exists);
            OpenUrl(localFile ?? fallbackUrl);
        }

        private void GitHub_Click(object sender, EventArgs e)
        {
            OpenUrl("https://github.com/OliversDev");
        }

        private void LinkedIn_Click(object sender, EventArgs e)
        {
            OpenUrl("https://ca.linkedin.com/in/oliverwackenreuther");
        }

        private void linkLblFootnote_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://ca.linkedin.com/in/oliverwackenreuther");
        }

        private void linkLblLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenBundledDocument(
                "license.html",
                "https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD/blob/master/LICENSE.html");
        }

        private void linkLblPrivacy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenBundledDocument(
                "privacy.html",
                "https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD/blob/master/PRIVACY.md");
        }

        private void linkLblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenBundledDocument(
                "index.html",
                "https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD");
        }

        private sealed class LayerInfo
        {
            public LayerInfo(
                ObjectId id,
                string name,
                string color,
                string linetype,
                bool isFrozen,
                bool isLocked)
            {
                Id = id;
                Name = name;
                Color = color;
                Linetype = linetype;
                IsFrozen = isFrozen;
                IsLocked = isLocked;
            }

            public ObjectId Id { get; private set; }
            public string Name { get; private set; }
            public string Color { get; private set; }
            public string Linetype { get; private set; }
            public bool IsFrozen { get; private set; }
            public bool IsLocked { get; private set; }
        }

        private sealed class RenameItem
        {
            public RenameItem(ObjectId id, string oldName, string newName)
            {
                Id = id;
                OldName = oldName;
                NewName = newName;
            }

            public ObjectId Id { get; private set; }
            public string OldName { get; private set; }
            public string NewName { get; private set; }
        }

        private sealed class ScriptRenameCommand
        {
            public ScriptRenameCommand(string oldName, string newName)
            {
                OldName = oldName;
                NewName = newName;
            }

            public string OldName { get; private set; }
            public string NewName { get; private set; }
        }
    }
}
