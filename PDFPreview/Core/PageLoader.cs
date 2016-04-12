using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PDFPreview.Core {
    class PageLoader {
        public BackgroundWorker ImageConverter = new BackgroundWorker();
        MainWindow main;
        public PageLoader(MainWindow main) {
            ImageConverter.WorkerReportsProgress = false;
            ImageConverter.WorkerSupportsCancellation = true;
            ImageConverter.DoWork += (obj, e) => RenderPages();
            this.main = main;
        }

        private void RenderPages() {
            main.Dispatcher.InvokeAsync((Action)(() => {
                foreach (Window w in System.Windows.Application.Current.Windows) {
                    if (w != System.Windows.Application.Current.MainWindow)
                        w.Close();
                }

                System.Drawing.Image[] pages = PDFToIamges(SettingsManager.NumOfPages);
                for (int i = 0; i < pages.Count(); i++) {
                    System.Drawing.Image page = pages[i];
                    Window win = new Window();
                    win.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/PDFPreview;component/Gfx/PDF.ico"));
                    win.Title = $"Page {i + 1} | PDFViewer";
                    win.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 204, 204));
                    StackPanel can = new StackPanel();
                    can.VerticalAlignment = VerticalAlignment.Top;
                    win.Content = can;
                    win.Owner = main;
                    RenderWindow(win, i);
                    win.KeyUp += new System.Windows.Input.KeyEventHandler(main.MoveToPage);
                    using (var stream = new MemoryStream(ImageToByteArray(page))) {
                        BitmapImage imageIn = new BitmapImage();
                        imageIn.BeginInit();
                        imageIn.StreamSource = stream;
                        imageIn.CacheOption = BitmapCacheOption.OnLoad;
                        imageIn.EndInit();
                        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                        image.Source = imageIn;
                        image.Stretch = Stretch.None;
                        can.Children.Add(image);
                    }
                }
            }));
        }

        private void RenderWindow(Window win, int Iteration) {
            //The main window should be rendered on the main screen always
            if (Screen.AllScreens.Length > 1) {
                Screen s = Screen.AllScreens[1];
                System.Drawing.Rectangle ScreenArea = s.WorkingArea;
                win.Top = ScreenArea.Top;
                win.Width = SettingsManager.PageWidth;
                if (Iteration < 2) {
                    win.Left = ScreenArea.Left + (win.Width * Iteration);
                } else {
                    //Trying to prevent the windows from going of screen
                    win.Left = (ScreenArea.Left + win.Width) + 50 * Iteration;
                    if (!ScreenArea.Contains(new System.Drawing.Point((int)win.Left + 50, (int)win.Top))) {
                        win.Left = ScreenArea.Right - win.Width;
                    }
                }
                win.Height = ScreenArea.Height;
                win.Show();
            } else {
                win.Show();
            }
        }
        public static byte[] ImageToByteArray(System.Drawing.Image x) {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(x, typeof(byte[]));
            return xByte;
        }

        private static GhostscriptVersionInfo _lastInstalledVersion = null;
        private static GhostscriptRasterizer _rasterizer = null;
        int desired_x_dpi = 96;
        int desired_y_dpi = 96;

        private System.Drawing.Image[] PDFToIamges(int pagesToRender) {
            _lastInstalledVersion =
                GhostscriptVersionInfo.GetLastInstalledVersion(
                        GhostscriptLicense.GPL | GhostscriptLicense.AFPL,
                        GhostscriptLicense.GPL);
            _rasterizer = new GhostscriptRasterizer();
            try {
                _rasterizer.Open(SettingsManager.FilePath, _lastInstalledVersion, false);
            } catch (FileNotFoundException) {
                ((MainWindow)System.Windows.Application.Current.MainWindow).TextBox_FilePath.Text = ErrorManager.InvalidFile();
                return new System.Drawing.Image[0];
            }


            if (pagesToRender == 0) {
                pagesToRender = _rasterizer.PageCount;
            }
            if (pagesToRender > _rasterizer.PageCount) {
                pagesToRender = _rasterizer.PageCount;
            }
            System.Drawing.Image[] images = new System.Drawing.Image[pagesToRender];
            for (int pageNumber = 1; pageNumber <= pagesToRender; pageNumber++) {
                try {
                    System.Drawing.Image img = _rasterizer.GetPage(desired_x_dpi, desired_y_dpi, pageNumber);
                    images[pageNumber - 1] = img;
                } catch (OutOfMemoryException) {
                    _rasterizer.Close();
                    return images;
                }
            }
            _rasterizer.Close();
            return images;
        }
    }
}
