using System;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace AutoCADLayerRenamer
{
    public partial class LayerRenameForm : Form
    {
        public LayerRenameForm()
        {
            InitializeComponent();
            LoadLayers();
            ApplyDarkTheme();
        }

        private void LoadLayers()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    foreach (var layerId in layerTable)
                    {
                        var layer = (LayerTableRecord)tr.GetObject(layerId, OpenMode.ForRead);
                        if (IsValidLayerName(layer.Name))
                        {
                            listBoxLayers.Items.Add(layer.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading layers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter(txtFilter.Text);  // Call filter method when text changes
        }

        private void ApplyFilter(string filter)
        {
            listBoxLayers.Items.Clear();

            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            try
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    foreach (var layerId in layerTable)
                    {
                        var layer = (LayerTableRecord)tr.GetObject(layerId, OpenMode.ForRead);
                        if (IsValidLayerName(layer.Name) && MatchesFilter(layer.Name, filter))
                        {
                            listBoxLayers.Items.Add(layer.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filter: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool MatchesFilter(string layerName, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return true;
            }

            // Convert wildcard '*' into regex equivalent '.*'
            string pattern = "^" + Regex.Escape(filter).Replace(@"\*", ".*") + "$";
            return Regex.IsMatch(layerName, pattern, RegexOptions.IgnoreCase);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            var prefix = txtPrefix.Text;
            var suffix = txtSuffix.Text;

            if (ContainsInvalidCharacters(prefix) || ContainsInvalidCharacters(suffix))
            {
                MessageBox.Show("Prefix or suffix contains invalid characters. Please remove any of the following characters: <>/\\\":;?*|,=", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (listBoxLayers.SelectedItems.Count == 0)
            {
                MessageBox.Show("No layers selected. Please select at least one layer.", "No Layers Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var renamedCount = LayerRenamer(prefix, suffix);
                MessageBox.Show($"{renamedCount} layers renamed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renaming layers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int LayerRenamer(string prefix, string suffix)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            int renamedCount = 0;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite);
                foreach (var selectedLayer in listBoxLayers.SelectedItems)
                {
                    var layer = (LayerTableRecord)tr.GetObject(layerTable[selectedLayer.ToString()], OpenMode.ForWrite);
                    layer.Name = prefix + layer.Name + suffix;
                    renamedCount++;
                }
                tr.Commit();
            }

            return renamedCount;
        }

        private bool ContainsInvalidCharacters(string input)
        {
            char[] invalidChars = { '<', '>', '/', '\\', '\"', ':', ';', '?', '*', '|', ',', '=' };
            return input.Any(c => invalidChars.Contains(c));
        }

        private bool IsValidLayerName(string layerName)
        {
            return layerName != "0" && layerName != "Defpoints" && !layerName.Contains("|");
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            this.ForeColor = System.Drawing.Color.White;

            foreach (Control control in this.Controls)
            {
                if (control is Button || control is TextBox || control is ListBox)
                {
                    control.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
                    control.ForeColor = System.Drawing.Color.White;
                }
            }
        }
    }
}
