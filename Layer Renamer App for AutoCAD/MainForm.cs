using System;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Data;
using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;

namespace AutoCADLayerRenamer
{
    public partial class LayerRenameForm : Form
    {
        // Constructor for initializing the form
        public LayerRenameForm()
        {
            InitializeComponent();  // Initialize form components
            InitializeDataTable();  // Initialize the data table for layer data
            LoadLayers();           // Load layers into the data table
            ApplyDarkTheme();       // Apply dark theme to the form
        }

        // DataTable to store layer information
        private System.Data.DataTable layerDataTable;

        // Initialize the DataTable with necessary columns
        private void InitializeDataTable()
        {
            layerDataTable = new System.Data.DataTable();

            // Add columns for layer information
            layerDataTable.Columns.Add("LayerName", typeof(string));     // Layer name
            layerDataTable.Columns.Add("Color", typeof(string));         // Layer color
            layerDataTable.Columns.Add("Linetype", typeof(string));      // Layer linetype
            layerDataTable.Columns.Add("IsFrozen", typeof(bool));        // Layer frozen status
            layerDataTable.Columns.Add("IsLocked", typeof(bool));        // Layer lock status
            layerDataTable.Columns.Add("Lineweight", typeof(string));    // Layer lineweight

            // Bind the DataTable to the DataGridView control
            dataGridViewLayers.DataSource = layerDataTable;
        }

        // HashSet to store selected layers by their names
        private HashSet<string> selectedLayers = new HashSet<string>();

        // Event handler for text change in the filter textbox
        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            SaveSelectedLayers();  // Save current layer selection before filtering

            // If the filter textbox is empty, clear the filter
            if (string.IsNullOrWhiteSpace(txtFilter.Text))
            {
                layerDataTable.DefaultView.RowFilter = string.Empty;  // Reset filter to show all layers
            }
            else
            {
                string filter = txtFilter.Text;  // Get the filter text
                ApplyFilter(filter);            // Apply the filter to the DataTable
            }

            RestoreSelection();  // Restore the selection of layers after filtering
        }

        // Save the currently selected layers
        private void SaveSelectedLayers()
        {
            // Iterate over all rows in the DataGridView
            foreach (DataGridViewRow row in dataGridViewLayers.Rows)
            {
                string layerName = row.Cells["LayerName"].Value?.ToString();  // Get the layer name

                // Check if the layer name is valid (non-empty)
                if (!string.IsNullOrEmpty(layerName))
                {
                    // If the row is selected, add the layer to the selectedLayers HashSet
                    if (row.Selected)
                    {
                        selectedLayers.Add(layerName);
                    }
                    // Otherwise, remove it from the selectedLayers HashSet
                    else
                    {
                        selectedLayers.Remove(layerName);
                    }
                }
            }
        }

        // Apply the filter to the DataTable based on the entered filter text
        private void ApplyFilter(string filter)
        {
            // Construct the row filter string using the entered filter text
            string rowFilter = $"[LayerName] LIKE '%{filter.Replace("'", "''")}%'";  // Escape single quotes for SQL-like filtering

            // Apply the filter to the DefaultView of the DataTable
            layerDataTable.DefaultView.RowFilter = rowFilter;
        }

        // Restore the previously saved layer selection after filtering
        private void RestoreSelection()
        {
            // Iterate over all rows in the DataGridView
            foreach (DataGridViewRow row in dataGridViewLayers.Rows)
            {
                string layerName = row.Cells["LayerName"].Value?.ToString();  // Get the layer name

                // Check if the layer name is valid and if it is in the selectedLayers HashSet
                if (!string.IsNullOrEmpty(layerName) && selectedLayers.Contains(layerName))
                {
                    row.Selected = true;  // Re-select the layer if it was previously selected
                }
                else
                {
                    row.Selected = false;  // Deselect the layer if it was not previously selected
                }
            }
        }

        // Clear the current filter and restore the selection of layers
        private void ClearFilter()
        {
            txtFilter.Text = string.Empty;                      // Clear the filter textbox
            layerDataTable.DefaultView.RowFilter = string.Empty;  // Reset the filter on the DataTable
            RestoreSelection();                                 // Restore the selection of layers
        }

        // Load layers from the AutoCAD drawing into the DataTable
        private void LoadLayers()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;

                // Start a new transaction
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);  // Get the layer table
                    layerDataTable.Rows.Clear();  // Clear existing rows in the DataTable

                    // Loop through each layer in the layer table
                    foreach (var layerId in layerTable)
                    {
                        var layer = (LayerTableRecord)tr.GetObject(layerId, OpenMode.ForRead);  // Get the layer record

                        // Check if the layer has a valid name
                        if (IsValidLayerName(layer.Name))
                        {
                            // Get layer properties and add them to the DataTable
                            string layerName = layer.Name;               // Layer name
                            string color = layer.Color.ToString();       // Layer color
                            string linetypeName = layer.LinetypeObjectId.IsValid
                                ? (tr.GetObject(layer.LinetypeObjectId, OpenMode.ForRead) as LinetypeTableRecord).Name
                                : "ByLayer";                             // Linetype (or default to "ByLayer")
                            string lineweight = layer.LineWeight.ToString();  // Layer lineweight
                            layerDataTable.Rows.Add(layerName, color, linetypeName, layer.IsFrozen, layer.IsLocked, lineweight);
                        }
                    }
                }

                // Apply the filter if there's text in the filter textbox
                if (!string.IsNullOrWhiteSpace(txtFilter.Text))
                {
                    ApplyFilter(txtFilter.Text);  // Apply filter to DataTable
                }
            }
            catch (Exception ex)  // Handle any errors during the layer loading process
            {
                MessageBox.Show($"Error loading layers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Check if a layer name matches the specified filter using wildcard matching
        private bool MatchesFilter(string layerName, string filter)
        {
            // Return true if the filter is empty (i.e., no filtering)
            if (string.IsNullOrWhiteSpace(filter))
            {
                return true;
            }

            // Convert the filter to a regular expression pattern
            string pattern = "^" + Regex.Escape(filter).Replace(@"\*", ".*") + "$";

            // Perform case-insensitive matching of the layer name against the filter pattern
            return Regex.IsMatch(layerName, pattern, RegexOptions.IgnoreCase);
        }

        // Check if a string contains any invalid characters
        private bool ContainsInvalidCharacters(string input)
        {
            // Define a list of characters that are not allowed in layer names
            char[] invalidChars = { '<', '>', '/', '\\', '\"', ':', ';', '?', '*', '|', ',', '=' };

            // Return true if the input contains any of these invalid characters
            return input.Any(c => invalidChars.Contains(c));
        }

        // Check if a layer name is valid (does not contain reserved or invalid names)
        private bool IsValidLayerName(string layerName)
        {
            // Ensure the layer name is not "0" or "Defpoints" and does not contain a pipe character "|"
            return layerName != "0" && layerName != "Defpoints" && !layerName.Contains("|");
        }

        // Event handler for when the "Apply" button is clicked
        private void btnApply_Click(object sender, EventArgs e)
        {
            // Get the prefix and suffix from the user input
            var prefix = txtPrefix.Text;
            var suffix = txtSuffix.Text;

            // Check if the prefix or suffix contains invalid characters
            if (ContainsInvalidCharacters(prefix) || ContainsInvalidCharacters(suffix))
            {
                MessageBox.Show("Prefix or suffix contains invalid characters. Please remove any of the following characters: <>/\\\":;?*|,=", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if any layers are selected
            if (dataGridViewLayers.SelectedRows.Count == 0)
            {
                MessageBox.Show("No layers selected. Please select at least one layer.", "No Layers Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Save the currently selected layers before clearing the filter
                SaveSelectedLayers();

                // Clear the filter and reset the selection
                txtFilter.Text = string.Empty;
                layerDataTable.DefaultView.RowFilter = string.Empty;

                // Restore the selection after clearing the filter
                RestoreSelection();

                // Rename the selected layers using the specified prefix and suffix
                var renamedCount = LayerRenamer(prefix, suffix);

                // Reload the layers to refresh the DataGridView with updated names
                LoadLayers();

                // Restore the selection again after renaming
                RestoreSelection();

                // Display a success message with the number of renamed layers
                MessageBox.Show($"{renamedCount} layers renamed successfully.", "Rename Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Display an error message if something goes wrong
                MessageBox.Show($"Error renaming layers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to rename layers with the given prefix and suffix
        private int LayerRenamer(string prefix, string suffix)
        {
            // Get the currently active AutoCAD document
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            // Get the database associated with the active document
            var db = doc.Database;

            // Initialize a counter to track how many layers have been renamed
            int renamedCount = 0;

            // Start a transaction to make changes to the AutoCAD database
            using (var tr = db.TransactionManager.StartTransaction())
            {
                // Open the layer table for writing to allow modifications
                var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite);

                // Iterate through all selected rows in the DataGridView
                foreach (DataGridViewRow selectedRow in dataGridViewLayers.SelectedRows)
                {
                    // Retrieve the layer name from the selected row in the DataGridView
                    var layerName = selectedRow.Cells["LayerName"].Value.ToString();

                    // Get the layer table record for the corresponding layer name and open it for writing
                    var layer = (LayerTableRecord)tr.GetObject(layerTable[layerName], OpenMode.ForWrite);

                    // Rename the layer by concatenating the provided prefix and suffix to the existing layer name
                    layer.Name = prefix + layer.Name + suffix;

                    // Increment the counter for each layer that has been renamed
                    renamedCount++;
                }

                // Commit the transaction to apply the changes to the AutoCAD database
                tr.Commit();
            }

            // Return the total number of renamed layers
            return renamedCount;
        }


        private void ApplyDarkTheme()
        {
            this.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            this.ForeColor = System.Drawing.Color.White;

            foreach (Control control in this.Controls)
            {
                if (control is Button || control is TextBox || control is ListBox || control is DataGridView)
                {
                    control.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
                    control.ForeColor = System.Drawing.Color.White;
                }
            }
        }

        private void linkLblFootnote_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://ca.linkedin.com/in/oliverwackenreuther";
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open link. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLblLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD/blob/master/LICENSE.txt";
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open link. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void linkLblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD/blob/master/README.md";
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open link. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GitHub_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/OliversDev";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void LinkedIn_Click(object sender, EventArgs e)
        {
            string url = "https://ca.linkedin.com/in/oliverwackenreuther";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
