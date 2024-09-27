using System.Windows.Forms;

namespace AutoCADLayerRenamer
{
    partial class LayerRenameForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.txtSuffix = new System.Windows.Forms.TextBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.lblPrefix = new System.Windows.Forms.Label();
            this.lblLayerList = new System.Windows.Forms.Label();
            this.lblSuffix = new System.Windows.Forms.Label();
            this.linkLblHelp = new System.Windows.Forms.LinkLabel();
            this.linkLblLicense = new System.Windows.Forms.LinkLabel();
            this.linkLblFootnote = new System.Windows.Forms.LinkLabel();
            this.LinkedIn = new System.Windows.Forms.PictureBox();
            this.GitHub = new System.Windows.Forms.PictureBox();
            this.Logo = new System.Windows.Forms.PictureBox();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.dataGridViewLayers = new System.Windows.Forms.DataGridView();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.LinkedIn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GitHub)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLayers)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPrefix
            // 
            this.txtPrefix.Location = new System.Drawing.Point(93, 34);
            this.txtPrefix.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(200, 22);
            this.txtPrefix.TabIndex = 1;
            // 
            // txtSuffix
            // 
            this.txtSuffix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSuffix.Location = new System.Drawing.Point(569, 34);
            this.txtSuffix.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtSuffix.Name = "txtSuffix";
            this.txtSuffix.Size = new System.Drawing.Size(200, 22);
            this.txtSuffix.TabIndex = 2;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(669, 519);
            this.btnApply.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(100, 28);
            this.btnApply.TabIndex = 3;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lblPrefix
            // 
            this.lblPrefix.AutoSize = true;
            this.lblPrefix.Location = new System.Drawing.Point(13, 38);
            this.lblPrefix.Name = "lblPrefix";
            this.lblPrefix.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPrefix.Size = new System.Drawing.Size(74, 16);
            this.lblPrefix.TabIndex = 4;
            this.lblPrefix.Text = "Enter Prefix";
            // 
            // lblLayerList
            // 
            this.lblLayerList.AutoSize = true;
            this.lblLayerList.Location = new System.Drawing.Point(13, 70);
            this.lblLayerList.Name = "lblLayerList";
            this.lblLayerList.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblLayerList.Size = new System.Drawing.Size(134, 16);
            this.lblLayerList.TabIndex = 5;
            this.lblLayerList.Text = "Select from Layer List";
            // 
            // lblSuffix
            // 
            this.lblSuffix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSuffix.AutoSize = true;
            this.lblSuffix.Location = new System.Drawing.Point(491, 38);
            this.lblSuffix.Name = "lblSuffix";
            this.lblSuffix.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblSuffix.Size = new System.Drawing.Size(72, 16);
            this.lblSuffix.TabIndex = 6;
            this.lblSuffix.Text = "Enter Suffix";
            // 
            // linkLblHelp
            // 
            this.linkLblHelp.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(174)))), ((int)(((byte)(16)))));
            this.linkLblHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLblHelp.AutoSize = true;
            this.linkLblHelp.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.linkLblHelp.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.linkLblHelp.Location = new System.Drawing.Point(500, 532);
            this.linkLblHelp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLblHelp.Name = "linkLblHelp";
            this.linkLblHelp.Size = new System.Drawing.Size(36, 16);
            this.linkLblHelp.TabIndex = 22;
            this.linkLblHelp.TabStop = true;
            this.linkLblHelp.Text = "Help";
            this.linkLblHelp.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.linkLblHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLblHelp_LinkClicked);
            // 
            // linkLblLicense
            // 
            this.linkLblLicense.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(174)))), ((int)(((byte)(16)))));
            this.linkLblLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLblLicense.AutoSize = true;
            this.linkLblLicense.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.linkLblLicense.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.linkLblLicense.Location = new System.Drawing.Point(401, 532);
            this.linkLblLicense.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLblLicense.Name = "linkLblLicense";
            this.linkLblLicense.Size = new System.Drawing.Size(91, 16);
            this.linkLblLicense.TabIndex = 21;
            this.linkLblLicense.TabStop = true;
            this.linkLblLicense.Text = "MIT License ©";
            this.linkLblLicense.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.linkLblLicense.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLblLicense_LinkClicked);
            // 
            // linkLblFootnote
            // 
            this.linkLblFootnote.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(174)))), ((int)(((byte)(16)))));
            this.linkLblFootnote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLblFootnote.AutoSize = true;
            this.linkLblFootnote.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.linkLblFootnote.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.linkLblFootnote.Location = new System.Drawing.Point(157, 532);
            this.linkLblFootnote.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLblFootnote.Name = "linkLblFootnote";
            this.linkLblFootnote.Size = new System.Drawing.Size(235, 16);
            this.linkLblFootnote.TabIndex = 17;
            this.linkLblFootnote.TabStop = true;
            this.linkLblFootnote.Text = "Created by Oliver Wackenreuther, v1.0";
            this.linkLblFootnote.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.linkLblFootnote.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLblFootnote_LinkClicked);
            // 
            // LinkedIn
            // 
            this.LinkedIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LinkedIn.Image = global::Layer_Renamer_App_for_AutoCAD.Properties.Resources.LI_In_Bug;
            this.LinkedIn.Location = new System.Drawing.Point(115, 516);
            this.LinkedIn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.LinkedIn.Name = "LinkedIn";
            this.LinkedIn.Size = new System.Drawing.Size(35, 30);
            this.LinkedIn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LinkedIn.TabIndex = 20;
            this.LinkedIn.TabStop = false;
            this.LinkedIn.Click += new System.EventHandler(this.LinkedIn_Click);
            // 
            // GitHub
            // 
            this.GitHub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GitHub.Image = global::Layer_Renamer_App_for_AutoCAD.Properties.Resources.github_mark_white;
            this.GitHub.Location = new System.Drawing.Point(77, 516);
            this.GitHub.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.GitHub.Name = "GitHub";
            this.GitHub.Size = new System.Drawing.Size(29, 30);
            this.GitHub.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.GitHub.TabIndex = 19;
            this.GitHub.TabStop = false;
            this.GitHub.Click += new System.EventHandler(this.GitHub_Click);
            // 
            // Logo
            // 
            this.Logo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Logo.Image = global::Layer_Renamer_App_for_AutoCAD.Properties.Resources.Logo_BW_NOBG;
            this.Logo.Location = new System.Drawing.Point(16, 516);
            this.Logo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Logo.Name = "Logo";
            this.Logo.Size = new System.Drawing.Size(53, 30);
            this.Logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Logo.TabIndex = 18;
            this.Logo.TabStop = false;
            // 
            // txtFilter
            // 
            this.txtFilter.Location = new System.Drawing.Point(93, 5);
            this.txtFilter.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(200, 22);
            this.txtFilter.TabIndex = 23;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(13, 11);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblSearch.Size = new System.Drawing.Size(50, 16);
            this.lblSearch.TabIndex = 24;
            this.lblSearch.Text = "Search";
            // 
            // dataGridViewLayers
            // 
            this.dataGridViewLayers.AllowUserToAddRows = false;
            this.dataGridViewLayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewLayers.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewLayers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewLayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewLayers.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewLayers.EnableHeadersVisualStyles = false;
            this.dataGridViewLayers.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.dataGridViewLayers.Location = new System.Drawing.Point(16, 89);
            this.dataGridViewLayers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridViewLayers.Name = "dataGridViewLayers";
            this.dataGridViewLayers.ReadOnly = true;
            this.dataGridViewLayers.RowHeadersVisible = false;
            this.dataGridViewLayers.RowHeadersWidth = 51;
            this.dataGridViewLayers.RowTemplate.Height = 24;
            this.dataGridViewLayers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewLayers.Size = new System.Drawing.Size(755, 420);
            this.dataGridViewLayers.TabIndex = 25;
            // 
            // LayerRenameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.dataGridViewLayers);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.linkLblHelp);
            this.Controls.Add(this.linkLblLicense);
            this.Controls.Add(this.LinkedIn);
            this.Controls.Add(this.GitHub);
            this.Controls.Add(this.Logo);
            this.Controls.Add(this.linkLblFootnote);
            this.Controls.Add(this.lblSuffix);
            this.Controls.Add(this.lblLayerList);
            this.Controls.Add(this.lblPrefix);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.txtSuffix);
            this.Controls.Add(this.txtPrefix);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(799, 598);
            this.Name = "LayerRenameForm";
            this.Text = "Layer Renamer";
            ((System.ComponentModel.ISupportInitialize)(this.LinkedIn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GitHub)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLayers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.TextBox txtPrefix;
        private System.Windows.Forms.TextBox txtSuffix;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label lblPrefix;
        private System.Windows.Forms.Label lblLayerList;
        private System.Windows.Forms.Label lblSuffix;
        private System.Windows.Forms.LinkLabel linkLblHelp;
        private System.Windows.Forms.LinkLabel linkLblLicense;
        private System.Windows.Forms.PictureBox LinkedIn;
        private System.Windows.Forms.PictureBox GitHub;
        private System.Windows.Forms.PictureBox Logo;
        private System.Windows.Forms.LinkLabel linkLblFootnote;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.DataGridView dataGridViewLayers;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}
