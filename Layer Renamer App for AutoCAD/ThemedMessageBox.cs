using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoCADLayerRenamer
{
    internal sealed class ThemedMessageBox : Form
    {
        private readonly Button defaultButton;

        private ThemedMessageBox(
            string message,
            string caption,
            MessageBoxButtons buttons,
            MessageBoxIcon icon,
            MessageBoxDefaultButton defaultButtonSelection)
        {
            Text = caption;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(560, 190);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 18, 18, 14)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

            PictureBox iconBox = new PictureBox
            {
                Image = GetIcon(icon),
                Location = Point.Empty,
                Margin = new Padding(0, 2, 12, 0),
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.Zoom,
                TabStop = false
            };

            TextBox messageBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 12),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                TabStop = false,
                Text = message
            };

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = new Padding(0),
                Padding = new Padding(0, 6, 0, 0),
                WrapContents = false
            };

            Button primaryButton;
            if (buttons == MessageBoxButtons.YesNo)
            {
                Button noButton = CreateButton("No", DialogResult.No);
                Button yesButton = CreateButton("Yes", DialogResult.Yes);
                buttonPanel.Controls.Add(noButton);
                buttonPanel.Controls.Add(yesButton);
                AcceptButton = yesButton;
                CancelButton = noButton;
                primaryButton = yesButton;
                defaultButton = defaultButtonSelection == MessageBoxDefaultButton.Button2
                    ? noButton
                    : yesButton;
            }
            else
            {
                Button okButton = CreateButton("OK", DialogResult.OK);
                buttonPanel.Controls.Add(okButton);
                AcceptButton = okButton;
                CancelButton = okButton;
                primaryButton = okButton;
                defaultButton = okButton;
            }

            layout.Controls.Add(iconBox, 0, 0);
            layout.Controls.Add(messageBox, 1, 0);
            layout.Controls.Add(buttonPanel, 0, 1);
            layout.SetColumnSpan(buttonPanel, 2);
            Controls.Add(layout);

            LayerRenamerTheme.ApplyDialog(this, primaryButton);
        }

        public static DialogResult Show(
            IWin32Window owner,
            string message,
            string caption,
            MessageBoxButtons buttons,
            MessageBoxIcon icon,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            using (ThemedMessageBox dialog = new ThemedMessageBox(
                message,
                caption,
                buttons,
                icon,
                defaultButton))
            {
                return dialog.ShowDialog(owner);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            defaultButton.Select();
        }

        private static Button CreateButton(string text, DialogResult result)
        {
            return new Button
            {
                DialogResult = result,
                Margin = new Padding(10, 0, 0, 0),
                Size = new Size(90, 30),
                Text = text
            };
        }

        private static Bitmap GetIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Error:
                    return SystemIcons.Error.ToBitmap();
                case MessageBoxIcon.Warning:
                    return SystemIcons.Warning.ToBitmap();
                case MessageBoxIcon.Question:
                    return SystemIcons.Question.ToBitmap();
                case MessageBoxIcon.Information:
                    return SystemIcons.Information.ToBitmap();
                default:
                    return null;
            }
        }
    }
}
