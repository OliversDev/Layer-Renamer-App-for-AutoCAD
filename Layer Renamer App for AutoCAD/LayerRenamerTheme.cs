using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoCADLayerRenamer
{
    internal static class LayerRenamerTheme
    {
        private const int DwmUseImmersiveDarkMode = 20;
        private const int DwmUseImmersiveDarkModeBefore20H1 = 19;
        private const uint WmThemeChanged = 0x031A;

        private static readonly Color Accent = Color.FromArgb(0, 120, 212);

        public static void Apply(
            Form form,
            Button primaryButton,
            Panel footerPanel,
            PictureBox logo,
            PictureBox gitHub,
            PictureBox linkedIn)
        {
            bool isDark = IsWindowsDarkMode();
            ThemePalette palette = CreatePalette(isDark);

            ApplyForm(form, null, isDark, palette);

            footerPanel.BackColor = palette.Surface;
            footerPanel.ForeColor = palette.Text;
            foreach (Control child in footerPanel.Controls)
            {
                child.BackColor = palette.Surface;
                child.ForeColor = palette.Text;

                LinkLabel link = child as LinkLabel;
                if (link != null)
                {
                    link.LinkColor = palette.Link;
                    link.ActiveLinkColor = palette.ActiveLink;
                    link.VisitedLinkColor = palette.Link;
                }
            }

            // The main action follows the outlined Batch Scripter treatment.
            ApplyOutlinedActionButton(primaryButton, palette);

            ApplyBrandLogo(logo, palette.Text, isDark);
            Color socialIconColour = isDark ? Color.White : Color.Black;
            ApplySocialIcon(gitHub, socialIconColour);
            ApplySocialIcon(linkedIn, socialIconColour);
        }

        public static void ApplyDialog(Form form, Button primaryButton)
        {
            bool isDark = IsWindowsDarkMode();
            ApplyForm(form, primaryButton, isDark, CreatePalette(isDark));
        }

        private static void ApplyForm(
            Form form,
            Button primaryButton,
            bool isDark,
            ThemePalette palette)
        {
            form.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            form.BackColor = palette.Background;
            form.ForeColor = palette.Text;

            ApplyToChildren(form, isDark, palette);
            if (primaryButton != null)
                ApplyPrimaryButton(primaryButton);
            ApplyNativeWindowTheme(form, isDark);
        }

        private static void ApplyToChildren(Control parent, bool isDark, ThemePalette palette)
        {
            foreach (Control control in parent.Controls)
            {
                control.ForeColor = palette.Text;

                if (control is GroupBox)
                {
                    control.BackColor = palette.Background;
                }
                else if (control is TextBoxBase)
                {
                    var textBox = (TextBoxBase)control;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = textBox.Enabled && !textBox.ReadOnly
                        ? palette.Input
                        : palette.AlternateSurface;
                    ApplyNativeControlTheme(control, isDark);
                    textBox.EnabledChanged -= TextBox_StateChanged;
                    textBox.EnabledChanged += TextBox_StateChanged;
                    textBox.ReadOnlyChanged -= TextBox_StateChanged;
                    textBox.ReadOnlyChanged += TextBox_StateChanged;
                }
                else if (control is ListBox)
                {
                    control.BackColor = palette.Input;
                    ApplyNativeControlTheme(control, isDark);
                }
                else if (control is DataGridView)
                {
                    ApplyDataGridViewTheme((DataGridView)control, isDark, palette);
                }
                else if (control is ScrollBar)
                {
                    control.BackColor = palette.Surface;
                    ApplyNativeControlTheme(control, isDark);
                }
                else if (control is Button)
                {
                    Button button = (Button)control;
                    button.UseVisualStyleBackColor = false;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = palette.Border;
                    button.FlatAppearance.BorderSize = 1;
                    button.BackColor = palette.Surface;
                    button.ForeColor = palette.Text;
                }
                else if (!(control is PictureBox) && !(control is LinkLabel))
                {
                    control.BackColor = palette.Background;
                }

                if (control.HasChildren)
                    ApplyToChildren(control, isDark, palette);
            }
        }

        private static void ApplyPrimaryButton(Button button)
        {
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Accent;
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = Accent;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(16, 110, 190);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 90, 158);
        }

        private static void TextBox_StateChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBoxBase;
            if (textBox == null) return;

            ThemePalette palette = CreatePalette(IsWindowsDarkMode());
            textBox.BackColor = textBox.Enabled && !textBox.ReadOnly
                ? palette.Input
                : palette.AlternateSurface;
        }

        private static void ApplyOutlinedActionButton(Button button, ThemePalette palette)
        {
            if (button == null) return;

            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = palette.Surface;
            button.ForeColor = Accent;
            button.FlatAppearance.BorderColor = Accent;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = palette.AlternateSurface;
            button.FlatAppearance.MouseDownBackColor = Accent;

            button.MouseDown -= OutlinedActionButton_MouseDown;
            button.MouseDown += OutlinedActionButton_MouseDown;
            button.MouseUp -= OutlinedActionButton_MouseUp;
            button.MouseUp += OutlinedActionButton_MouseUp;
            button.MouseLeave -= OutlinedActionButton_MouseLeave;
            button.MouseLeave += OutlinedActionButton_MouseLeave;
        }

        private static void OutlinedActionButton_MouseDown(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            button.BackColor = Accent;
            button.ForeColor = Color.White;
        }

        private static void OutlinedActionButton_MouseUp(object sender, MouseEventArgs e)
        {
            RestoreOutlinedActionButton(sender as Button);
        }

        private static void OutlinedActionButton_MouseLeave(object sender, EventArgs e)
        {
            RestoreOutlinedActionButton(sender as Button);
        }

        private static void RestoreOutlinedActionButton(Button button)
        {
            if (button == null) return;
            ThemePalette palette = CreatePalette(IsWindowsDarkMode());
            button.BackColor = palette.Surface;
            button.ForeColor = Accent;
        }

        private static void ApplyDataGridViewTheme(
            DataGridView grid,
            bool isDark,
            ThemePalette palette)
        {
            Color headerSurface = isDark
                ? Color.FromArgb(55, 55, 58)
                : Color.FromArgb(232, 232, 232);

            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = palette.Surface;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.GridColor = palette.Border;
            grid.ForeColor = palette.Text;
            grid.RowHeadersVisible = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = true;

            grid.DefaultCellStyle.BackColor = palette.Surface;
            grid.DefaultCellStyle.ForeColor = palette.Text;
            grid.DefaultCellStyle.SelectionBackColor = Accent;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.NullValue = string.Empty;

            grid.RowsDefaultCellStyle.BackColor = palette.Surface;
            grid.RowsDefaultCellStyle.ForeColor = palette.Text;
            grid.RowsDefaultCellStyle.SelectionBackColor = Accent;
            grid.RowsDefaultCellStyle.SelectionForeColor = Color.White;

            grid.AlternatingRowsDefaultCellStyle.BackColor = palette.AlternateSurface;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = palette.Text;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Accent;
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            grid.ColumnHeadersDefaultCellStyle.BackColor = headerSurface;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = palette.Text;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerSurface;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = palette.Text;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(
                "Segoe UI",
                9F,
                FontStyle.Bold,
                GraphicsUnit.Point);
            grid.RowTemplate.Height = 24;

            ApplyNativeControlTheme(grid, isDark);
            foreach (Control child in grid.Controls)
            {
                if (child is ScrollBar)
                    ApplyNativeControlTheme(child, isDark);
            }
        }

        public static void DrawReadOnlyCheckBox(
            Graphics graphics,
            Rectangle cellBounds,
            bool isChecked)
        {
            if (graphics == null) return;

            bool isDark = IsWindowsDarkMode();
            Color fill = isDark
                ? Color.FromArgb(68, 68, 71)
                : Color.FromArgb(232, 232, 232);
            Color border = isDark
                ? Color.FromArgb(132, 132, 136)
                : Color.FromArgb(150, 150, 150);
            Color check = isDark
                ? Color.FromArgb(188, 188, 188)
                : Color.FromArgb(105, 105, 105);

            const int boxSize = 14;
            var box = new Rectangle(
                cellBounds.Left + (cellBounds.Width - boxSize) / 2,
                cellBounds.Top + (cellBounds.Height - boxSize) / 2,
                boxSize,
                boxSize);

            SmoothingMode originalSmoothing = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var fillBrush = new SolidBrush(fill))
            using (var borderPen = new Pen(border, 1F))
            {
                graphics.FillRectangle(fillBrush, box);
                graphics.DrawRectangle(borderPen, box);
            }

            if (isChecked)
            {
                using (var checkPen = new Pen(check, 2F))
                {
                    checkPen.StartCap = LineCap.Round;
                    checkPen.EndCap = LineCap.Round;
                    graphics.DrawLines(
                        checkPen,
                        new[]
                        {
                            new Point(box.Left + 3, box.Top + 7),
                            new Point(box.Left + 6, box.Top + 10),
                            new Point(box.Left + 11, box.Top + 4)
                        });
                }
            }

            graphics.SmoothingMode = originalSmoothing;
        }

        private static void ApplyNativeWindowTheme(Form form, bool isDark)
        {
            try
            {
                int enabled = isDark ? 1 : 0;
                int result = DwmSetWindowAttribute(
                    form.Handle,
                    DwmUseImmersiveDarkMode,
                    ref enabled,
                    sizeof(int));

                if (result != 0)
                {
                    DwmSetWindowAttribute(
                        form.Handle,
                        DwmUseImmersiveDarkModeBefore20H1,
                        ref enabled,
                        sizeof(int));
                }

                SendMessage(form.Handle, WmThemeChanged, IntPtr.Zero, IntPtr.Zero);
            }
            catch
            {
                // Older Windows builds can ignore the title-bar theme request.
            }
        }

        private static void ApplyNativeControlTheme(Control control, bool isDark)
        {
            try
            {
                SetWindowTheme(control.Handle, isDark ? "DarkMode_Explorer" : "Explorer", null);
                SendMessage(control.Handle, WmThemeChanged, IntPtr.Zero, IntPtr.Zero);
            }
            catch
            {
                // The control colours still provide a usable fallback.
            }
        }

        private static void ApplySocialIcon(PictureBox pictureBox, Color colour)
        {
            if (pictureBox == null || pictureBox.Image == null)
                return;

            try
            {
                using (Bitmap source = new Bitmap(pictureBox.Image))
                {
                    Bitmap themed = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
                    for (int y = 0; y < source.Height; y++)
                    {
                        for (int x = 0; x < source.Width; x++)
                        {
                            Color pixel = source.GetPixel(x, y);
                            themed.SetPixel(x, y, Color.FromArgb(pixel.A, colour.R, colour.G, colour.B));
                        }
                    }

                    pictureBox.Image = themed;
                }
            }
            catch
            {
                // A missing optional footer icon must not block the application.
            }
        }

        private static void ApplyBrandLogo(PictureBox pictureBox, Color foreground, bool isDark)
        {
            if (isDark || pictureBox.Image == null)
                return;

            try
            {
                using (Bitmap source = new Bitmap(pictureBox.Image))
                {
                    Bitmap themed = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
                    for (int y = 0; y < source.Height; y++)
                    {
                        for (int x = 0; x < source.Width; x++)
                        {
                            Color pixel = source.GetPixel(x, y);
                            bool isWhiteArtwork = pixel.R > 225 && pixel.G > 225 && pixel.B > 225;
                            themed.SetPixel(x, y, isWhiteArtwork
                                ? Color.FromArgb(pixel.A, foreground.R, foreground.G, foreground.B)
                                : pixel);
                        }
                    }

                    pictureBox.Image = themed;
                }
            }
            catch
            {
                // The original logo remains available if tinting is unavailable.
            }
        }

        private static bool IsWindowsDarkMode()
        {
            try
            {
                if (SystemInformation.HighContrast)
                    return SystemColors.Window.GetBrightness() < 0.5F;

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object value = key == null ? null : key.GetValue("AppsUseLightTheme");
                    return value != null && Convert.ToInt32(value) == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static ThemePalette CreatePalette(bool isDark)
        {
            return isDark
                ? new ThemePalette(
                    Color.FromArgb(30, 30, 30),
                    Color.FromArgb(45, 45, 48),
                    Color.FromArgb(52, 52, 55),
                    Color.FromArgb(37, 37, 38),
                    Color.FromArgb(245, 245, 245),
                    Color.FromArgb(80, 80, 84),
                    Color.FromArgb(86, 156, 214),
                    Color.FromArgb(230, 174, 16))
                : new ThemePalette(
                    Color.FromArgb(245, 245, 245),
                    Color.White,
                    Color.FromArgb(244, 246, 248),
                    Color.White,
                    Color.FromArgb(32, 32, 32),
                    Color.FromArgb(190, 190, 190),
                    Color.FromArgb(0, 102, 204),
                    Color.FromArgb(0, 78, 156));
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr windowHandle,
            int attribute,
            ref int attributeValue,
            int attributeSize);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(
            IntPtr windowHandle,
            string subApplicationName,
            string subIdList);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr windowHandle,
            uint message,
            IntPtr wordParameter,
            IntPtr longParameter);

        private sealed class ThemePalette
        {
            public ThemePalette(
                Color background,
                Color surface,
                Color alternateSurface,
                Color input,
                Color text,
                Color border,
                Color link,
                Color activeLink)
            {
                Background = background;
                Surface = surface;
                AlternateSurface = alternateSurface;
                Input = input;
                Text = text;
                Border = border;
                Link = link;
                ActiveLink = activeLink;
            }

            public Color Background { get; }
            public Color Surface { get; }
            public Color AlternateSurface { get; }
            public Color Input { get; }
            public Color Text { get; }
            public Color Border { get; }
            public Color Link { get; }
            public Color ActiveLink { get; }
        }
    }
}
