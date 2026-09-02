using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

            ApplyForm(form, primaryButton, isDark, palette);

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

            // The footer surface pass must not overwrite the primary action colour.
            ApplyPrimaryButton(primaryButton);

            ApplyBrandLogo(logo, palette.Text, isDark);
            Color socialIconColour = isDark ? Color.White : Color.Black;
            ApplyDazzleIcon(gitHub, "AutoCADLayerRenamer.Dazzle.Github-logo.png", socialIconColour);
            ApplyDazzleIcon(linkedIn, "AutoCADLayerRenamer.Dazzle.Linkedin-logo.png", socialIconColour);
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
                else if (control is TextBox || control is ListBox)
                {
                    control.BackColor = palette.Input;
                    ApplyNativeControlTheme(control, isDark);
                }
                else if (control is DataGridView)
                {
                    DataGridView grid = (DataGridView)control;
                    grid.BackgroundColor = palette.Surface;
                    grid.BorderStyle = BorderStyle.FixedSingle;
                    grid.GridColor = palette.Border;
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = palette.Surface;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = palette.Text;
                    grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = palette.Surface;
                    grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = palette.Text;
                    grid.DefaultCellStyle.BackColor = palette.Input;
                    grid.DefaultCellStyle.ForeColor = palette.Text;
                    grid.DefaultCellStyle.SelectionBackColor = Accent;
                    grid.DefaultCellStyle.SelectionForeColor = Color.White;
                    grid.AlternatingRowsDefaultCellStyle.BackColor = isDark
                        ? Color.FromArgb(42, 42, 44)
                        : Color.FromArgb(250, 250, 250);
                    ApplyNativeControlTheme(grid, isDark);
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

        private static void ApplyDazzleIcon(PictureBox pictureBox, string resourceName, Color colour)
        {
            try
            {
                using (Stream stream = typeof(LayerRenamerTheme).Assembly
                    .GetManifestResourceStream(resourceName))
                using (Bitmap source = stream == null ? null : new Bitmap(stream))
                {
                    if (source == null)
                        return;

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
                    Color.FromArgb(37, 37, 38),
                    Color.FromArgb(245, 245, 245),
                    Color.FromArgb(80, 80, 84),
                    Color.FromArgb(86, 156, 214),
                    Color.FromArgb(230, 174, 16))
                : new ThemePalette(
                    Color.FromArgb(245, 245, 245),
                    Color.White,
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
                Color input,
                Color text,
                Color border,
                Color link,
                Color activeLink)
            {
                Background = background;
                Surface = surface;
                Input = input;
                Text = text;
                Border = border;
                Link = link;
                ActiveLink = activeLink;
            }

            public Color Background { get; }
            public Color Surface { get; }
            public Color Input { get; }
            public Color Text { get; }
            public Color Border { get; }
            public Color Link { get; }
            public Color ActiveLink { get; }
        }
    }
}
