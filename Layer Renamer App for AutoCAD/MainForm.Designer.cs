using System.Drawing;
using System.Windows.Forms;

namespace AutoCADLayerRenamer
{
    partial class LayerRenameForm
    {
        private System.ComponentModel.IContainer components;
        private DataGridView dataGridViewLayers;
        private TextBox txtFilter;
        private TextBox txtPrefix;
        private TextBox txtSuffix;
        private TextBox txtFind;
        private TextBox txtReplace;
        private CheckBox chkFindReplace;
        private CheckBox chkMatchCase;
        private Label lblSelection;
        private TableLayoutPanel footerPanel;
        private PictureBox Logo;
        private PictureBox GitHub;
        private PictureBox LinkedIn;
        private Button btnRename;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 1, RowCount = 4 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var search = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4, Margin = new Padding(0, 0, 0, 10) };
            search.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            search.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            search.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            search.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            search.Controls.Add(new Label { Text = "Filter", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 7, 10, 0) }, 0, 0);
            txtFilter = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 3, 10, 3) };
            txtFilter.TextChanged += txtFilter_TextChanged;
            search.Controls.Add(txtFilter, 1, 0);
            var clearFilter = new Button { Text = "Clear Filter", Width = 90, Margin = new Padding(0, 0, 10, 0) };
            clearFilter.Click += btnClearFilter_Click;
            search.Controls.Add(clearFilter, 2, 0);
            lblSelection = new Label { Text = "0 selected", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 7, 0, 0) };
            search.Controls.Add(lblSelection, 3, 0);

            dataGridViewLayers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = true,
                MultiSelect = true,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Margin = new Padding(0, 0, 0, 10),
                ColumnHeadersHeight = 30,
                RowTemplate = { Height = 26 }
            };
            dataGridViewLayers.SelectionChanged += dataGridViewLayers_SelectionChanged;

            var options = new GroupBox { Text = "Rename Options", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10), Margin = new Padding(0, 0, 0, 10) };
            var optionGrid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 6, RowCount = 2 };
            optionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            optionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            optionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            optionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            optionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            optionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            optionGrid.Controls.Add(InputLabel("Prefix"), 0, 0);
            txtPrefix = InputBox(); optionGrid.Controls.Add(txtPrefix, 1, 0);
            optionGrid.Controls.Add(InputLabel("Suffix"), 2, 0);
            txtSuffix = InputBox(); optionGrid.Controls.Add(txtSuffix, 3, 0);
            chkFindReplace = new CheckBox { Text = "Find and Replace", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(10, 5, 10, 0) };
            chkFindReplace.CheckedChanged += RenameOptionChanged;
            optionGrid.Controls.Add(chkFindReplace, 4, 0);
            chkMatchCase = new CheckBox { Text = "Match case", AutoSize = true, Enabled = false, Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 0, 0) };
            chkMatchCase.CheckedChanged += RenameOptionChanged;
            optionGrid.Controls.Add(chkMatchCase, 5, 0);
            optionGrid.Controls.Add(InputLabel("Find"), 0, 1);
            txtFind = InputBox(); txtFind.Enabled = false; optionGrid.Controls.Add(txtFind, 1, 1);
            optionGrid.Controls.Add(InputLabel("Replace"), 2, 1);
            txtReplace = InputBox(); txtReplace.Enabled = false; optionGrid.Controls.Add(txtReplace, 3, 1);
            txtPrefix.TextChanged += RenameOptionChanged;
            txtSuffix.TextChanged += RenameOptionChanged;
            txtFind.TextChanged += RenameOptionChanged;
            txtReplace.TextChanged += RenameOptionChanged;
            options.Controls.Add(optionGrid);

            footerPanel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 10, Margin = new Padding(0) };
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Logo = IconBox(Layer_Renamer_App_for_AutoCAD.Properties.Resources.Logo_BW_NOBG, 54); footerPanel.Controls.Add(Logo, 0, 0);
            GitHub = IconBox(Layer_Renamer_App_for_AutoCAD.Properties.Resources.github_mark_white, 30); GitHub.Cursor = Cursors.Hand; GitHub.Click += GitHub_Click; footerPanel.Controls.Add(GitHub, 1, 0);
            LinkedIn = IconBox(Layer_Renamer_App_for_AutoCAD.Properties.Resources.LI_In_Bug, 30); LinkedIn.Cursor = Cursors.Hand; LinkedIn.Click += LinkedIn_Click; footerPanel.Controls.Add(LinkedIn, 2, 0);
            var author = FooterLink("Created by Oliver Wackenreuther, v2.0"); author.LinkClicked += linkLblFootnote_LinkClicked; footerPanel.Controls.Add(author, 3, 0);
            var license = FooterLink("License"); license.LinkClicked += linkLblLicense_LinkClicked; footerPanel.Controls.Add(license, 4, 0);
            var privacy = FooterLink("Privacy"); privacy.LinkClicked += linkLblPrivacy_LinkClicked; footerPanel.Controls.Add(privacy, 5, 0);
            var help = FooterLink("Help"); help.LinkClicked += linkLblHelp_LinkClicked; footerPanel.Controls.Add(help, 7, 0);
            btnRename = new Button { Text = "RENAME", Width = 100, Margin = new Padding(10, 0, 10, 0) }; btnRename.Click += btnRename_Click; footerPanel.Controls.Add(btnRename, 8, 0);
            var close = new Button { Text = "Close", Width = 100, DialogResult = DialogResult.Cancel }; close.Click += btnClose_Click; footerPanel.Controls.Add(close, 9, 0);

            root.Controls.Add(search, 0, 0);
            root.Controls.Add(dataGridViewLayers, 0, 1);
            root.Controls.Add(options, 0, 2);
            root.Controls.Add(footerPanel, 0, 3);
            Controls.Add(root);
            AcceptButton = btnRename;
            CancelButton = close;
            ClientSize = new Size(980, 650);
            MinimumSize = new Size(820, 560);
            Name = "LayerRenameForm";
            Text = "Layer Renamer";
            ResumeLayout(false);
        }

        private static Label InputLabel(string text) { return new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 7, 10, 0) }; }
        private static TextBox InputBox() { return new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 3, 10, 3) }; }
        private static PictureBox IconBox(Image image, int width) { return new PictureBox { Image = image, SizeMode = PictureBoxSizeMode.Zoom, Width = width, Height = 30, Margin = new Padding(0, 0, 8, 0) }; }
        private static LinkLabel FooterLink(string text) { return new LinkLabel { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 7, 10, 0) }; }
    }
}
