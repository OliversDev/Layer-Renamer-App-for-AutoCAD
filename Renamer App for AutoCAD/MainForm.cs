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

namespace AutoCADRenamer
{
    public partial class RenamerForm : Form
    {
        private static readonly Regex RenameCommandPattern = new Regex(
            @"^\(\s*command\s+""(?<command>[^""]+)""\s+""(?<type>[^""]+)""\s+""(?<old>(?:\\.|[^""])*)""\s+""(?<new>(?:\\.|[^""])*)""\s*\)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private readonly DataTable objectTable = new DataTable();
        private readonly List<NamedObjectInfo> allObjects = new List<NamedObjectInfo>();
        private readonly Dictionary<string, ObjectId> existingObjects =
            new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<ObjectKind, HashSet<string>> selectedNames =
            Enum.GetValues(typeof(ObjectKind)).Cast<ObjectKind>().ToDictionary(
                kind => kind,
                kind => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        private readonly Dictionary<ObjectKind, string> filters =
            Enum.GetValues(typeof(ObjectKind)).Cast<ObjectKind>().ToDictionary(kind => kind, kind => string.Empty);

        private ObjectKind activeKind = ObjectKind.Layers;
        private bool restoringSelection;
        private bool switchingCategory;

        public RenamerForm()
        {
            InitializeComponent();
            ApplyApplicationIcon();
            InitializeObjectTable();
            RenamerTheme.Apply(this, btnRename, footerPanel, Logo, GitHub, LinkedIn);
            ConfigureGrid();
            UpdateCategoryTabSelection();
            LoadObjects();
            UpdateScriptDisplay();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            try
            {
                var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (document != null) document.Window.Focus();
                else Autodesk.AutoCAD.ApplicationServices.Application.MainWindow.Focus();
            }
            catch
            {
                // Focus recovery must never prevent the form from closing.
            }
        }

        private void ApplyApplicationIcon()
        {
            using (Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("AutoCADRenamer.RenamerIcon.ico"))
            {
                if (stream == null) return;
                using (Icon source = new Icon(stream)) Icon = (Icon)source.Clone();
            }
        }

        private void InitializeObjectTable()
        {
            objectTable.Columns.Add("ObjectId", typeof(ObjectId));
            objectTable.Columns.Add("Name", typeof(string));
            objectTable.Columns.Add("Details", typeof(string));
            objectTable.Columns.Add("Color", typeof(string));
            objectTable.Columns.Add("Linetype", typeof(string));
            objectTable.Columns.Add("IsFrozen", typeof(bool));
            objectTable.Columns.Add("IsLocked", typeof(bool));
            dataGridViewObjects.DataSource = objectTable;
        }

        private void ConfigureGrid()
        {
            dataGridViewObjects.Columns["ObjectId"].Visible = false;
            dataGridViewObjects.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridViewObjects.AllowUserToResizeColumns = true;
            dataGridViewObjects.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewObjects.Columns["Details"].Width = 560;
            dataGridViewObjects.Columns["Details"].HeaderText = "Details";
            dataGridViewObjects.ColumnHeadersDefaultCellStyle.Font =
                new System.Drawing.Font(dataGridViewObjects.Font, System.Drawing.FontStyle.Bold);
            dataGridViewObjects.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewObjects.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            dataGridViewObjects.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            foreach (string name in new[] { "IsFrozen", "IsLocked" })
            {
                var column = dataGridViewObjects.Columns[name] as DataGridViewCheckBoxColumn;
                if (column == null) continue;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.ReadOnly = true;
                column.FlatStyle = FlatStyle.Flat;
                column.ThreeState = false;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.DefaultCellStyle.Padding = new Padding(0);
                column.DefaultCellStyle.ForeColor = SystemColors.GrayText;
            }

            dataGridViewObjects.CellPainting += dataGridViewObjects_CellPainting;
            ConfigureGridForActiveKind();
        }

        private void ConfigureGridForActiveKind()
        {
            bool layers = activeKind == ObjectKind.Layers;
            DataGridViewColumn name = dataGridViewObjects.Columns["Name"];
            DataGridViewColumn details = dataGridViewObjects.Columns["Details"];
            DataGridViewColumn color = dataGridViewObjects.Columns["Color"];
            DataGridViewColumn linetype = dataGridViewObjects.Columns["Linetype"];
            DataGridViewColumn frozen = dataGridViewObjects.Columns["IsFrozen"];
            DataGridViewColumn locked = dataGridViewObjects.Columns["IsLocked"];

            name.HeaderText = layers ? "Layer Name" : ActiveDefinition.SingularName + " Name";
            name.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            name.Width = layers ? 520 : 350;
            name.MinimumWidth = 180;
            name.Resizable = DataGridViewTriState.True;

            details.Visible = !layers;
            details.HeaderText = "Details";
            details.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            details.Width = 560;
            details.MinimumWidth = 250;
            details.Resizable = DataGridViewTriState.True;

            color.Visible = layers;
            color.HeaderText = "Color";
            color.Width = 90;
            color.MinimumWidth = 70;
            color.Resizable = DataGridViewTriState.True;

            linetype.Visible = layers;
            linetype.HeaderText = "Line Type";
            linetype.Width = 110;
            linetype.MinimumWidth = 90;
            linetype.Resizable = DataGridViewTriState.True;

            frozen.Visible = layers;
            frozen.HeaderText = "Frozen";
            frozen.Width = 64;
            frozen.MinimumWidth = 60;
            frozen.Resizable = DataGridViewTriState.True;

            locked.Visible = layers;
            locked.HeaderText = "Locked";
            locked.Width = 64;
            locked.MinimumWidth = 60;
            locked.Resizable = DataGridViewTriState.True;

            string[] order = layers
                ? new[] { "Name", "Color", "Linetype", "IsFrozen", "IsLocked", "Details", "ObjectId" }
                : new[] { "Name", "Details", "Color", "Linetype", "IsFrozen", "IsLocked", "ObjectId" };
            for (int index = 0; index < order.Length; index++)
                dataGridViewObjects.Columns[order[index]].DisplayIndex = index;
        }

        private void dataGridViewObjects_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (activeKind != ObjectKind.Layers || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string columnName = dataGridViewObjects.Columns[e.ColumnIndex].Name;
            if (!string.Equals(columnName, "IsFrozen", StringComparison.Ordinal) &&
                !string.Equals(columnName, "IsLocked", StringComparison.Ordinal))
            {
                return;
            }

            e.PaintBackground(e.CellBounds, true);
            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
            bool isChecked = e.FormattedValue != null && Convert.ToBoolean(e.FormattedValue);
            RenamerTheme.DrawReadOnlyCheckBox(e.Graphics, e.CellBounds, isChecked);
            e.Handled = true;
        }

        private ObjectDefinition ActiveDefinition { get { return ObjectDefinition.For(activeKind); } }
        private HashSet<string> ActiveSelection { get { return selectedNames[activeKind]; } }

        private void LoadObjects(bool saveCurrentSelection = true)
        {
            if (saveCurrentSelection) SaveSelectedObjects();
            allObjects.Clear();
            existingObjects.Clear();

            var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (document == null)
            {
                ShowMessageDialog("No active drawing is available.", "Renamer", MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    switch (activeKind)
                    {
                        case ObjectKind.Layers: LoadLayers(document.Database, transaction); break;
                        case ObjectKind.Blocks: LoadBlocks(document.Database, transaction); break;
                        case ObjectKind.DimensionStyles:
                            LoadSymbolTable(document.Database.DimStyleTableId, transaction, IsStandardRecordRenameable, null);
                            break;
                        case ObjectKind.Linetypes: LoadLinetypes(document.Database, transaction); break;
                        case ObjectKind.TableStyles: LoadTableStyles(document.Database, transaction); break;
                        case ObjectKind.TextStyles: LoadTextStyles(document.Database, transaction); break;
                    }
                    transaction.Commit();
                }

                allObjects.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name));
                var validNames = new HashSet<string>(allObjects.Select(item => item.Name), StringComparer.OrdinalIgnoreCase);
                ActiveSelection.RemoveWhere(name => !validNames.Contains(name));
                ApplyFilter();
            }
            catch (Exception ex)
            {
                ShowMessageDialog(ActiveDefinition.PluralName + " could not be loaded.\r\n\r\n" + ex.Message, "Renamer", MessageBoxIcon.Error);
            }
        }

        private void LoadLayers(Database database, Transaction transaction)
        {
            var layers = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);
            foreach (ObjectId id in layers)
            {
                var layer = (LayerTableRecord)transaction.GetObject(id, OpenMode.ForRead);
                existingObjects[layer.Name] = id;
                if (!IsStandardRecordRenameable(layer) ||
                    string.Equals(layer.Name, "0", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(layer.Name, "Defpoints", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string linetype = "ByLayer";
                if (!layer.LinetypeObjectId.IsNull && layer.LinetypeObjectId.IsValid)
                {
                    try
                    {
                        var typeRecord = transaction.GetObject(layer.LinetypeObjectId, OpenMode.ForRead) as LinetypeTableRecord;
                        if (typeRecord != null) linetype = typeRecord.Name;
                    }
                    catch
                    {
                        // Retain the ByLayer fallback if a referenced record cannot be read.
                    }
                }

                allObjects.Add(new NamedObjectInfo(
                    id,
                    layer.Name,
                    layer.Color.ToString(),
                    linetype,
                    layer.IsFrozen,
                    layer.IsLocked));
            }
        }

        private void LoadBlocks(Database database, Transaction transaction)
        {
            LoadSymbolTable(
                database.BlockTableId,
                transaction,
                record =>
                {
                    var block = record as BlockTableRecord;
                    return block != null && IsStandardRecordRenameable(block) &&
                           !block.IsAnonymous && !block.IsLayout &&
                           !block.IsFromExternalReference && !block.IsFromOverlayReference &&
                           !block.Name.StartsWith("*", StringComparison.Ordinal);
                },
                record => ((BlockTableRecord)record).IsDynamicBlock ? "Dynamic block" : "Block definition");
        }

        private void LoadLinetypes(Database database, Transaction transaction)
        {
            LoadSymbolTable(
                database.LinetypeTableId,
                transaction,
                record => IsStandardRecordRenameable(record) &&
                          !string.Equals(record.Name, "BYLAYER", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(record.Name, "BYBLOCK", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(record.Name, "CONTINUOUS", StringComparison.OrdinalIgnoreCase),
                record => ((LinetypeTableRecord)record).AsciiDescription ?? string.Empty);
        }

        private void LoadTextStyles(Database database, Transaction transaction)
        {
            LoadSymbolTable(
                database.TextStyleTableId,
                transaction,
                IsStandardRecordRenameable,
                record =>
                {
                    var style = (TextStyleTableRecord)record;
                    return string.IsNullOrWhiteSpace(style.FileName) ? string.Empty : style.FileName;
                });
        }

        private void LoadSymbolTable(
            ObjectId tableId,
            Transaction transaction,
            Func<SymbolTableRecord, bool> canRename,
            Func<SymbolTableRecord, string> details)
        {
            var table = (SymbolTable)transaction.GetObject(tableId, OpenMode.ForRead);
            foreach (ObjectId id in table)
            {
                var record = (SymbolTableRecord)transaction.GetObject(id, OpenMode.ForRead);
                existingObjects[record.Name] = id;
                if (!canRename(record)) continue;
                allObjects.Add(new NamedObjectInfo(id, record.Name, details == null ? string.Empty : details(record)));
            }
        }

        private void LoadTableStyles(Database database, Transaction transaction)
        {
            var dictionary = (DBDictionary)transaction.GetObject(database.TableStyleDictionaryId, OpenMode.ForRead);
            foreach (DBDictionaryEntry entry in dictionary)
            {
                string name = entry.Key;
                existingObjects[name] = entry.Value;
                if (string.IsNullOrWhiteSpace(name) ||
                    string.Equals(name, "Standard", StringComparison.OrdinalIgnoreCase) ||
                    name.IndexOf('|') >= 0 ||
                    name.StartsWith("*", StringComparison.Ordinal))
                {
                    continue;
                }
                allObjects.Add(new NamedObjectInfo(entry.Value, name, string.Empty));
            }
        }

        private static bool IsStandardRecordRenameable(SymbolTableRecord record)
        {
            return record != null && !string.IsNullOrWhiteSpace(record.Name) && !record.IsDependent && record.Name.IndexOf('|') < 0;
        }

        private void categoryTab_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (switchingCategory || button == null || button.Tag == null) return;

            ObjectKind nextKind = (ObjectKind)Convert.ToInt32(button.Tag);
            if (nextKind == activeKind) return;

            SaveSelectedObjects();
            filters[activeKind] = txtFilter.Text;
            activeKind = nextKind;

            switchingCategory = true;
            try
            {
                txtFilter.Text = filters[activeKind];
                UpdateCategoryTabSelection();
                LoadObjects(false);
            }
            finally { switchingCategory = false; }
        }

        private void UpdateCategoryTabSelection()
        {
            if (categoryTabs == null) return;

            foreach (Control control in categoryTabs.Controls)
            {
                var button = control as Button;
                if (button == null || button.Tag == null) continue;
                bool selected = Convert.ToInt32(button.Tag) == (int)activeKind;
                RenamerTheme.StyleCategoryTab(button, selected);
            }
        }

        private void dataGridViewObjects_SelectionChanged(object sender, EventArgs e)
        {
            if (restoringSelection) return;
            SaveSelectedObjects();
            UpdateSelectionLabel();
        }

        private void SaveSelectedObjects()
        {
            if (dataGridViewObjects == null) return;
            foreach (DataGridViewRow row in dataGridViewObjects.Rows)
            {
                object rawName = row.Cells["Name"].Value;
                if (rawName == null) continue;
                string name = rawName.ToString();
                if (row.Selected) ActiveSelection.Add(name);
                else ActiveSelection.Remove(name);
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            if (switchingCategory) return;
            SaveSelectedObjects();
            filters[activeKind] = txtFilter.Text;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string filter = txtFilter.Text.Trim();
            IEnumerable<NamedObjectInfo> filtered = allObjects.Where(item => MatchesFilter(item.Name, filter));
            restoringSelection = true;
            try
            {
                objectTable.Rows.Clear();
                foreach (NamedObjectInfo item in filtered)
                {
                    objectTable.Rows.Add(
                        item.Id,
                        item.Name,
                        item.Details,
                        item.Color,
                        item.Linetype,
                        item.IsFrozen,
                        item.IsLocked);
                }
                ConfigureGridForActiveKind();
                dataGridViewObjects.ClearSelection();
                foreach (DataGridViewRow row in dataGridViewObjects.Rows)
                {
                    object rawName = row.Cells["Name"].Value;
                    if (rawName != null && ActiveSelection.Contains(rawName.ToString())) row.Selected = true;
                }
            }
            finally { restoringSelection = false; }
            UpdateSelectionLabel();
        }

        private static bool MatchesFilter(string name, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return true;
            if (filter.IndexOf('*') < 0) return string.Equals(name, filter, StringComparison.OrdinalIgnoreCase);
            string pattern = "^" + Regex.Escape(filter).Replace(@"\*", ".*") + "$";
            return Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private void RenameOptionChanged(object sender, EventArgs e)
        {
            bool exact = !string.IsNullOrWhiteSpace(txtExactName.Text);
            txtPrefix.Enabled = !exact;
            txtSuffix.Enabled = !exact;
            txtFind.Enabled = !exact;
            txtReplace.Enabled = !exact;
        }

        private string BuildNewName(string oldName)
        {
            if (!string.IsNullOrWhiteSpace(txtExactName.Text)) return txtExactName.Text.Trim();
            string name = oldName;
            if (txtFind.Text.Length > 0) name = ReplaceOrdinalIgnoreCase(name, txtFind.Text, txtReplace.Text);
            return txtPrefix.Text + name + txtSuffix.Text;
        }

        private static string ReplaceOrdinalIgnoreCase(string input, string find, string replacement)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(find)) return input;
            replacement = replacement ?? string.Empty;
            int startIndex = 0;
            var builder = new StringBuilder();
            while (true)
            {
                int index = input.IndexOf(find, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index < 0) { builder.Append(input.Substring(startIndex)); break; }
                builder.Append(input.Substring(startIndex, index - startIndex));
                builder.Append(replacement);
                startIndex = index + find.Length;
            }
            return builder.ToString();
        }

        private List<RenameItem> BuildCurrentSelectionPlan()
        {
            return allObjects.Where(item => ActiveSelection.Contains(item.Name))
                .Select(item => new RenameItem(activeKind, item.Id, item.Name, BuildNewName(item.Name)))
                .Where(item => !string.Equals(item.OldName, item.NewName, StringComparison.Ordinal))
                .OrderBy(item => item.OldName, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private void btnAddToScriptList_Click(object sender, EventArgs e)
        {
            SaveSelectedObjects();
            if (ActiveSelection.Count == 0)
            {
                ShowMessageDialog("Select one or more " + ActiveDefinition.PluralName.ToLowerInvariant() + " before adding rename commands.", "Renamer", MessageBoxIcon.Information);
                return;
            }
            if (!string.IsNullOrWhiteSpace(txtExactName.Text) && ActiveSelection.Count != 1)
            {
                ShowMessageDialog("Rename To can only be used when exactly one object is selected.", "Renamer", MessageBoxIcon.Information);
                return;
            }

            List<RenameItem> plan = BuildCurrentSelectionPlan();
            if (plan.Count == 0)
            {
                ShowMessageDialog("The selected options do not change any object names.", "Renamer", MessageBoxIcon.Information);
                return;
            }
            string error;
            if (!ValidatePlan(plan, out error))
            {
                ShowMessageDialog(error, "Renamer", MessageBoxIcon.Warning);
                return;
            }

            string commands = BuildGeneratedScript(plan).TrimEnd();
            if (txtGeneratedScript.TextLength > 0 && !txtGeneratedScript.Text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                txtGeneratedScript.AppendText(Environment.NewLine);
            txtGeneratedScript.AppendText(commands + Environment.NewLine);
        }

        private bool ValidatePlan(IList<RenameItem> plan, out string error)
        {
            foreach (RenameItem item in plan)
            {
                ObjectId currentId;
                if (!existingObjects.TryGetValue(item.OldName, out currentId) || currentId != item.Id)
                {
                    error = ActiveDefinition.SingularName + " \"" + item.OldName + "\" is no longer available. Refresh the list and rebuild the script.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(item.NewName)) { error = "An object name cannot be empty."; return false; }
                if (item.NewName.Length > 255)
                {
                    error = "The resulting name for \"" + item.OldName + "\" exceeds AutoCAD's 255-character name limit.";
                    return false;
                }
                if (ContainsInvalidCharacters(item.NewName))
                {
                    error = "The resulting name for \"" + item.OldName + "\" contains an invalid character.\r\n\r\n" +
                            "Invalid characters: < > / \\ \" : ; ? * | , = '";
                    return false;
                }
            }

            IGrouping<string, RenameItem> duplicate = plan.GroupBy(item => item.NewName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
            {
                error = "More than one staged " + ActiveDefinition.SingularName.ToLowerInvariant() +
                        " would be renamed to \"" + duplicate.Key + "\".";
                return false;
            }
            foreach (RenameItem item in plan)
            {
                ObjectId existingId;
                if (existingObjects.TryGetValue(item.NewName, out existingId) && existingId != item.Id)
                {
                    error = "A " + ActiveDefinition.SingularName.ToLowerInvariant() + " named \"" + item.NewName + "\" already exists.";
                    return false;
                }
            }
            error = null;
            return true;
        }

        private static bool ContainsInvalidCharacters(string value)
        {
            return value.IndexOfAny(new[] { '<', '>', '/', '\\', '"', ':', ';', '?', '*', '|', ',', '=', '\'' }) >= 0 || value.Any(char.IsControl);
        }

        private string BuildGeneratedScript(IList<RenameItem> plan)
        {
            return NormalizeScriptText(string.Join(Environment.NewLine, plan.Select(BuildRenameScriptCommand)), true);
        }

        private static string BuildRenameScriptCommand(RenameItem item)
        {
            string keyword = ObjectDefinition.For(item.Kind).CommandKeyword;
            return "(command \"_.-RENAME\" \"_" + keyword + "\" \"" + EscapeAutoLispString(item.OldName) +
                   "\" \"" + EscapeAutoLispString(item.NewName) + "\")";
        }

        private static string EscapeAutoLispString(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string NormalizeScriptText(string script, bool addFinalNewLine)
        {
            if (string.IsNullOrWhiteSpace(script)) return string.Empty;
            string normalized = string.Join(Environment.NewLine,
                script.Replace("\r\n", "\n").Replace("\r", "\n")
                    .Split(new[] { '\n' }, StringSplitOptions.None).Select(line => line.TrimEnd()));
            return addFinalNewLine ? normalized.TrimEnd() + Environment.NewLine : normalized.TrimEnd();
        }

        private void UpdateScriptDisplay()
        {
            bool hasScript = !string.IsNullOrWhiteSpace(txtGeneratedScript.Text);
            btnCopyScript.Enabled = hasScript;
            btnSaveScript.Enabled = hasScript;
            btnClearScript.Enabled = hasScript;
            btnRename.Enabled = hasScript;
        }

        private void txtGeneratedScript_TextChanged(object sender, EventArgs e) { UpdateScriptDisplay(); }

        private void btnRename_Click(object sender, EventArgs e)
        {
            string[] commands = txtGeneratedScript.Lines.Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith(";", StringComparison.Ordinal)).ToArray();
            if (commands.Length == 0)
            {
                ShowMessageDialog("Enter or add one or more script commands first.", "Renamer", MessageBoxIcon.Information);
                return;
            }

            string warning = BuildScriptConflictWarning(commands);
            bool confirmed = string.IsNullOrEmpty(warning)
                ? ShowConfirmationDialog("Run " + commands.Length + " script command" + (commands.Length == 1 ? "?" : "s?"), "Renamer")
                : ShowWarningConfirmationDialog(warning + "\r\n\r\nThese commands may fail or produce incomplete results. Run the script anyway?", "Renamer - Potential Conflicts");
            if (!confirmed) return;

            try
            {
                var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (document == null) throw new InvalidOperationException("No active AutoCAD drawing is available.");
                Hide();
                document.Window.Focus();
                foreach (string command in commands) document.SendStringToExecute(command + "\n", true, false, false);
                BeginInvoke(new Action(Close));
            }
            catch (Exception ex)
            {
                Show();
                ShowMessageDialog("The rename script could not be started.\r\n\r\n" + ex.Message, "Renamer", MessageBoxIcon.Error);
            }
        }

        private static string BuildScriptConflictWarning(IEnumerable<string> scriptLines)
        {
            List<ScriptRenameCommand> commands = scriptLines.Select(ParseRenameCommand).Where(item => item != null).ToList();
            List<string> duplicateTargets = commands
                .GroupBy(item => item.Kind + "\u001f" + item.NewName, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => FormatConflict(group.First().Kind, group.First().NewName)).OrderBy(value => value).ToList();
            List<string> repeatedSources = commands
                .GroupBy(item => item.Kind + "\u001f" + item.OldName, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => FormatConflict(group.First().Kind, group.First().OldName)).OrderBy(value => value).ToList();

            var sections = new List<string>();
            if (duplicateTargets.Count > 0)
                sections.Add("Multiple objects of the same type are being renamed to the same name:\r\n" +
                             string.Join("\r\n", duplicateTargets.Select(value => "  • " + value)));
            if (repeatedSources.Count > 0)
                sections.Add("The same existing object is being renamed more than once:\r\n" +
                             string.Join("\r\n", repeatedSources.Select(value => "  • " + value)));
            return string.Join("\r\n\r\n", sections);
        }

        private static string FormatConflict(ObjectKind kind, string name)
        {
            return ObjectDefinition.For(kind).SingularName + ": " + name;
        }

        private static ScriptRenameCommand ParseRenameCommand(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith(";", StringComparison.Ordinal)) return null;
            Match match = RenameCommandPattern.Match(line.Trim());
            if (!match.Success) return null;
            string commandName = match.Groups["command"].Value.Replace("_", string.Empty).Replace(".", string.Empty);
            if (!string.Equals(commandName, "-RENAME", StringComparison.OrdinalIgnoreCase)) return null;

            ObjectKind kind;
            if (!ObjectDefinition.TryParseCommandKeyword(match.Groups["type"].Value.Replace("_", string.Empty), out kind)) return null;
            return new ScriptRenameCommand(kind, UnescapeAutoLispString(match.Groups["old"].Value), UnescapeAutoLispString(match.Groups["new"].Value));
        }

        private static string UnescapeAutoLispString(string value)
        {
            var builder = new StringBuilder();
            for (int index = 0; index < value.Length; index++)
            {
                if (value[index] == '\\' && index + 1 < value.Length) index++;
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
                ShowMessageDialog("The staged script was copied to the clipboard.", "Renamer", MessageBoxIcon.Information);
            }
            catch (Exception ex) { ShowMessageDialog("The script could not be copied.\r\n\r\n" + ex.Message, "Renamer", MessageBoxIcon.Error); }
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
                    FileName = "Rename.scr",
                    Filter = "AutoCAD scripts (*.scr)|*.scr",
                    OverwritePrompt = true,
                    RestoreDirectory = true,
                    Title = "Save Rename Script"
                })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    File.WriteAllText(dialog.FileName, NormalizeScriptText(txtGeneratedScript.Text, true), Encoding.ASCII);
                }
                ShowMessageDialog("The staged script was saved successfully.", "Renamer", MessageBoxIcon.Information);
            }
            catch (Exception ex) { ShowMessageDialog("The script could not be saved.\r\n\r\n" + ex.Message, "Renamer", MessageBoxIcon.Error); }
        }

        private void btnClearScript_Click(object sender, EventArgs e) { txtGeneratedScript.Clear(); }
        private void btnRefresh_Click(object sender, EventArgs e) { LoadObjects(); }
        private void btnClearFilter_Click(object sender, EventArgs e) { txtFilter.Clear(); }
        private void btnClose_Click(object sender, EventArgs e) { Close(); }
        private void UpdateSelectionLabel() { lblSelection.Text = ActiveSelection.Count + " selected"; }

        private void ShowMessageDialog(string message, string title, MessageBoxIcon icon)
        {
            ThemedMessageBox.Show(this, message, title, MessageBoxButtons.OK, icon);
        }

        private bool ShowConfirmationDialog(string message, string title)
        {
            return ThemedMessageBox.Show(this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private bool ShowWarningConfirmationDialog(string message, string title)
        {
            return ThemedMessageBox.Show(this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private static void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex)
            {
                ThemedMessageBox.Show(null, "Unable to open the link.\r\n\r\n" + ex.Message, "Renamer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void OpenBundledDocument(string fileName, string fallbackUrl)
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] candidates =
            {
                Path.GetFullPath(Path.Combine(directory, "..", "Help", fileName)),
                Path.Combine(directory, fileName)
            };
            OpenUrl(candidates.FirstOrDefault(File.Exists) ?? fallbackUrl);
        }

        private void GitHub_Click(object sender, EventArgs e) { OpenUrl("https://github.com/OliversDev"); }
        private void LinkedIn_Click(object sender, EventArgs e) { OpenUrl("https://ca.linkedin.com/in/oliverwackenreuther"); }
        private void linkLblFootnote_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { OpenUrl("https://ca.linkedin.com/in/oliverwackenreuther"); }
        private void linkLblLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenBundledDocument("license.html", "https://github.com/OliversDev/Renamer-App-for-AutoCAD/blob/master/LICENSE.html");
        }
        private void linkLblPrivacy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenBundledDocument("privacy.html", "https://github.com/OliversDev/Renamer-App-for-AutoCAD/blob/master/PRIVACY.md");
        }
        private void linkLblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenBundledDocument("Renamer.html", "https://github.com/OliversDev/Renamer-App-for-AutoCAD#readme");
        }

        private enum ObjectKind
        {
            Layers = 0,
            Blocks = 1,
            DimensionStyles = 2,
            Linetypes = 3,
            TableStyles = 4,
            TextStyles = 5
        }

        private sealed class ObjectDefinition
        {
            private static readonly Dictionary<ObjectKind, ObjectDefinition> Definitions = new Dictionary<ObjectKind, ObjectDefinition>
            {
                { ObjectKind.Layers, new ObjectDefinition(ObjectKind.Layers, "Layer", "Layers", "Layer") },
                { ObjectKind.Blocks, new ObjectDefinition(ObjectKind.Blocks, "Block", "Blocks", "Block") },
                { ObjectKind.DimensionStyles, new ObjectDefinition(ObjectKind.DimensionStyles, "Dimension Style", "Dimension Styles", "Dimstyle") },
                { ObjectKind.Linetypes, new ObjectDefinition(ObjectKind.Linetypes, "Linetype", "Linetypes", "LType") },
                { ObjectKind.TableStyles, new ObjectDefinition(ObjectKind.TableStyles, "Table Style", "Table Styles", "Tablestyle") },
                { ObjectKind.TextStyles, new ObjectDefinition(ObjectKind.TextStyles, "Text Style", "Text Styles", "textStyle") }
            };

            private ObjectDefinition(ObjectKind kind, string singularName, string pluralName, string commandKeyword)
            {
                Kind = kind;
                SingularName = singularName;
                PluralName = pluralName;
                CommandKeyword = commandKeyword;
            }

            public ObjectKind Kind { get; private set; }
            public string SingularName { get; private set; }
            public string PluralName { get; private set; }
            public string CommandKeyword { get; private set; }
            public static ObjectDefinition For(ObjectKind kind) { return Definitions[kind]; }

            public static bool TryParseCommandKeyword(string value, out ObjectKind kind)
            {
                ObjectDefinition definition = Definitions.Values.FirstOrDefault(item =>
                    string.Equals(item.CommandKeyword, value, StringComparison.OrdinalIgnoreCase));
                if (definition == null) { kind = default(ObjectKind); return false; }
                kind = definition.Kind;
                return true;
            }
        }

        private sealed class NamedObjectInfo
        {
            public NamedObjectInfo(ObjectId id, string name, string details)
            {
                Id = id;
                Name = name;
                Details = details ?? string.Empty;
                Color = string.Empty;
                Linetype = string.Empty;
            }

            public NamedObjectInfo(
                ObjectId id,
                string name,
                string color,
                string linetype,
                bool isFrozen,
                bool isLocked)
            {
                Id = id;
                Name = name;
                Details = string.Empty;
                Color = color ?? string.Empty;
                Linetype = linetype ?? string.Empty;
                IsFrozen = isFrozen;
                IsLocked = isLocked;
            }
            public ObjectId Id { get; private set; }
            public string Name { get; private set; }
            public string Details { get; private set; }
            public string Color { get; private set; }
            public string Linetype { get; private set; }
            public bool IsFrozen { get; private set; }
            public bool IsLocked { get; private set; }
        }

        private sealed class RenameItem
        {
            public RenameItem(ObjectKind kind, ObjectId id, string oldName, string newName)
            {
                Kind = kind;
                Id = id;
                OldName = oldName;
                NewName = newName;
            }
            public ObjectKind Kind { get; private set; }
            public ObjectId Id { get; private set; }
            public string OldName { get; private set; }
            public string NewName { get; private set; }
        }

        private sealed class ScriptRenameCommand
        {
            public ScriptRenameCommand(ObjectKind kind, string oldName, string newName)
            {
                Kind = kind;
                OldName = oldName;
                NewName = newName;
            }
            public ObjectKind Kind { get; private set; }
            public string OldName { get; private set; }
            public string NewName { get; private set; }
        }
    }
}
