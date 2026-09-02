using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using DataTable = System.Data.DataTable;

namespace AutoCADLayerRenamer
{
    public partial class LayerRenameForm : Form
    {
        private readonly DataTable layerTable = new DataTable();
        private readonly HashSet<ObjectId> selectedLayerIds = new HashSet<ObjectId>();
        private readonly Dictionary<string, ObjectId> existingLayers = new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);
        private bool restoringSelection;

        public LayerRenameForm()
        {
            InitializeComponent();
            ApplyApplicationIcon();
            InitializeLayerTable();
            LayerRenamerTheme.Apply(this, btnRename, footerPanel, Logo, GitHub, LinkedIn);
            ConfigureGrid();
            LoadLayers();
            UpdatePreview();
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
            layerTable.Columns.Add("NewName", typeof(string));
            layerTable.Columns.Add("Color", typeof(string));
            layerTable.Columns.Add("Linetype", typeof(string));
            layerTable.Columns.Add("IsFrozen", typeof(bool));
            layerTable.Columns.Add("IsLocked", typeof(bool));
            layerTable.Columns.Add("Lineweight", typeof(string));
            dataGridViewLayers.DataSource = layerTable;
        }

        private void ConfigureGrid()
        {
            dataGridViewLayers.Columns["LayerId"].Visible = false;
            dataGridViewLayers.Columns["LayerName"].HeaderText = "Layer";
            dataGridViewLayers.Columns["NewName"].HeaderText = "New Name";
            dataGridViewLayers.Columns["IsFrozen"].HeaderText = "Frozen";
            dataGridViewLayers.Columns["IsLocked"].HeaderText = "Locked";
            dataGridViewLayers.Columns["LayerName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewLayers.Columns["NewName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewLayers.Columns["Color"].Width = 90;
            dataGridViewLayers.Columns["Linetype"].Width = 110;
            dataGridViewLayers.Columns["IsFrozen"].Width = 64;
            dataGridViewLayers.Columns["IsLocked"].Width = 64;
            dataGridViewLayers.Columns["Lineweight"].Width = 105;
            foreach (string name in new[] { "IsFrozen", "IsLocked" })
            {
                var column = dataGridViewLayers.Columns[name];
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.DefaultCellStyle.ForeColor = SystemColors.GrayText;
            }
        }

        private void LoadLayers()
        {
            layerTable.Rows.Clear();
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
                    var layers = (LayerTable)transaction.GetObject(document.Database.LayerTableId, OpenMode.ForRead);
                    foreach (ObjectId id in layers)
                    {
                        var layer = (LayerTableRecord)transaction.GetObject(id, OpenMode.ForRead);
                        existingLayers[layer.Name] = id;
                        if (!IsRenameable(layer)) continue;
                        string linetype = "ByLayer";
                        if (!layer.LinetypeObjectId.IsNull && layer.LinetypeObjectId.IsValid)
                        {
                            var record = transaction.GetObject(layer.LinetypeObjectId, OpenMode.ForRead) as LinetypeTableRecord;
                            if (record != null) linetype = record.Name;
                        }
                        layerTable.Rows.Add(id, layer.Name, layer.Name, layer.Color.ToString(), linetype,
                            layer.IsFrozen, layer.IsLocked, layer.LineWeight.ToString());
                    }
                    transaction.Commit();
                }
                ApplyFilter();
                RestoreSelection();
            }
            catch (Exception ex)
            {
                ShowMessageDialog("Layers could not be loaded.\r\n\r\n" + ex.Message, "Layer Renamer", MessageBoxIcon.Error);
            }
        }

        private static bool IsRenameable(LayerTableRecord layer)
        {
            return !layer.IsDependent &&
                   !string.Equals(layer.Name, "0", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(layer.Name, "Defpoints", StringComparison.OrdinalIgnoreCase) &&
                   layer.Name.IndexOf('|') < 0;
        }

        private void dataGridViewLayers_SelectionChanged(object sender, EventArgs e)
        {
            if (restoringSelection) return;
            foreach (DataGridViewRow row in dataGridViewLayers.Rows)
            {
                var id = (ObjectId)row.Cells["LayerId"].Value;
                if (row.Selected) selectedLayerIds.Add(id); else selectedLayerIds.Remove(id);
            }
            UpdateSelectionLabel();
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            restoringSelection = true;
            try { ApplyFilter(); }
            finally { restoringSelection = false; }
            RestoreSelection();
        }

        private void ApplyFilter()
        {
            string text = txtFilter.Text.Trim();
            if (text.Length == 0) { layerTable.DefaultView.RowFilter = string.Empty; return; }
            string pattern = text.IndexOf('*') >= 0 ? text : "*" + text + "*";
            string escaped = pattern.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "%");
            layerTable.DefaultView.RowFilter = "[LayerName] LIKE '" + escaped + "'";
        }

        private void RestoreSelection()
        {
            restoringSelection = true;
            try
            {
                dataGridViewLayers.ClearSelection();
                foreach (DataGridViewRow row in dataGridViewLayers.Rows)
                    if (selectedLayerIds.Contains((ObjectId)row.Cells["LayerId"].Value)) row.Selected = true;
            }
            finally { restoringSelection = false; }
            UpdateSelectionLabel();
        }

        private void RenameOptionChanged(object sender, EventArgs e)
        {
            txtFind.Enabled = chkFindReplace.Checked;
            txtReplace.Enabled = chkFindReplace.Checked;
            chkMatchCase.Enabled = chkFindReplace.Checked;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            foreach (DataRow row in layerTable.Rows) row["NewName"] = BuildNewName((string)row["LayerName"]);
        }

        private string BuildNewName(string oldName)
        {
            string name = oldName;
            if (chkFindReplace.Checked && txtFind.Text.Length > 0)
            {
                name = chkMatchCase.Checked
                    ? name.Replace(txtFind.Text, txtReplace.Text)
                    : Regex.Replace(name, Regex.Escape(txtFind.Text), match => txtReplace.Text, RegexOptions.IgnoreCase);
            }
            return txtPrefix.Text + name + txtSuffix.Text;
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            if (selectedLayerIds.Count == 0)
            {
                ShowMessageDialog("Select at least one layer to rename.", "Layer Renamer", MessageBoxIcon.Warning);
                return;
            }
            if (chkFindReplace.Checked && txtFind.Text.Length == 0)
            {
                ShowMessageDialog("Enter text in Find, or turn off Find and Replace.", "Layer Renamer", MessageBoxIcon.Warning);
                return;
            }
            var plan = BuildPlan();
            string error;
            if (!ValidatePlan(plan, out error))
            {
                ShowMessageDialog(error, "Layer Renamer", MessageBoxIcon.Warning);
                return;
            }
            if (plan.Count == 0)
            {
                ShowMessageDialog("The selected options do not change any layer names.", "Layer Renamer", MessageBoxIcon.Information);
                return;
            }
            if (!ShowConfirmationDialog("Rename " + plan.Count + " layer" + (plan.Count == 1 ? "" : "s") + "?", "Layer Renamer")) return;
            try
            {
                ExecutePlan(plan);
                selectedLayerIds.Clear();
                LoadLayers();
                UpdatePreview();
                ShowMessageDialog(plan.Count + " layer" + (plan.Count == 1 ? " was" : "s were") + " renamed successfully.", "Layer Renamer", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowMessageDialog("No changes were committed.\r\n\r\n" + ex.Message, "Layer Renamer", MessageBoxIcon.Error);
            }
        }

        private List<RenameItem> BuildPlan()
        {
            return layerTable.AsEnumerable()
                .Where(row => selectedLayerIds.Contains(row.Field<ObjectId>("LayerId")))
                .Select(row => new RenameItem(row.Field<ObjectId>("LayerId"), row.Field<string>("LayerName"), BuildNewName(row.Field<string>("LayerName"))))
                .Where(item => !string.Equals(item.OldName, item.NewName, StringComparison.Ordinal))
                .ToList();
        }

        private bool ValidatePlan(IList<RenameItem> plan, out string error)
        {
            foreach (var item in plan)
            {
                if (string.IsNullOrWhiteSpace(item.NewName)) { error = "A layer name cannot be empty."; return false; }
                if (item.NewName.Length > 255)
                {
                    error = "The resulting name for \"" + item.OldName + "\" exceeds AutoCAD's 255-character layer-name limit.";
                    return false;
                }
                if (ContainsInvalidCharacters(item.NewName))
                {
                    error = "The resulting name for \"" + item.OldName + "\" contains an invalid character.\r\n\r\nInvalid characters: < > / \\ \" : ; ? * | , =";
                    return false;
                }
            }
            var duplicate = plan.GroupBy(item => item.NewName, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
            {
                error = "More than one selected layer would be renamed to \"" + duplicate.Key + "\". Change the rename options and try again.";
                return false;
            }
            var selectedIds = new HashSet<ObjectId>(plan.Select(item => item.Id));
            foreach (var existing in existingLayers)
            {
                if (!selectedIds.Contains(existing.Value) && plan.Any(item => string.Equals(item.NewName, existing.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    error = "A layer named \"" + existing.Key + "\" already exists and is not part of this rename operation.";
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

        private static void ExecutePlan(IList<RenameItem> plan)
        {
            var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                foreach (var item in plan)
                {
                    var layer = (LayerTableRecord)transaction.GetObject(item.Id, OpenMode.ForWrite);
                    layer.Name = "__LAYER_RENAMER_" + item.Id.Handle + "_" + Guid.NewGuid().ToString("N");
                }
                foreach (var item in plan)
                {
                    var layer = (LayerTableRecord)transaction.GetObject(item.Id, OpenMode.ForWrite);
                    layer.Name = item.NewName;
                }
                transaction.Commit();
            }
        }

        private void btnClearFilter_Click(object sender, EventArgs e) { txtFilter.Clear(); }
        private void btnClose_Click(object sender, EventArgs e) { Close(); }
        private void UpdateSelectionLabel() { lblSelection.Text = selectedLayerIds.Count + " selected"; }
        private void ShowMessageDialog(string message, string title, MessageBoxIcon icon)
        {
            ThemedMessageBox.Show(this, message, title, MessageBoxButtons.OK, icon);
        }
        private bool ShowConfirmationDialog(string message, string title)
        {
            return ThemedMessageBox.Show(this, message, title, MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }
        private static void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) { ThemedMessageBox.Show(null, "Unable to open the link.\r\n\r\n" + ex.Message, "Layer Renamer", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
        private void GitHub_Click(object sender, EventArgs e) { OpenUrl("https://github.com/OliversDev"); }
        private void LinkedIn_Click(object sender, EventArgs e) { OpenUrl("https://ca.linkedin.com/in/oliverwackenreuther"); }
        private void linkLblFootnote_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { OpenUrl("https://ca.linkedin.com/in/oliverwackenreuther"); }
        private void linkLblLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { OpenBundledDocument("LICENSE.txt", "https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD/blob/master/LICENSE.txt"); }
        private void linkLblPrivacy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { OpenBundledDocument("privacy.html", "https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD/blob/master/PRIVACY.md"); }
        private void linkLblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { OpenBundledDocument("index.html", "https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD"); }

        private sealed class RenameItem
        {
            public RenameItem(ObjectId id, string oldName, string newName) { Id = id; OldName = oldName; NewName = newName; }
            public ObjectId Id { get; private set; }
            public string OldName { get; private set; }
            public string NewName { get; private set; }
        }
    }
}
