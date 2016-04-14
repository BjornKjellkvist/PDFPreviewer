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
        Navigator _Navigator;
        public PageLoader(MainWindow main) {
            ImageConverter.WorkerReportsProgress = false;
            ImageConverter.WorkerSupportsCancellation = true;
            ImageConverter.DoWork += (obj, e) => RenderPages();
            this.main = main;
            _Navigator = new Navigator();
        }

        private void RenderPages() {
            main.Dispatcher.InvokeAsync((Action)(() => {
                foreach (Window w in System.Windows.Application.Current.Windows) {
                    if (w != System.Windows.Application.Current.MainWindow)
                        w.Close();
                }

                Page[] Pages = PDFToPages();
                if (Screen.AllScreens.Length == 1) {
                    for (int i = Pages.Length; i-- > 0;) {
                        RenderWindow(Pages[i].Window, i);
                    }
                } else {
                    for (int i = 0; i < Pages.Count(); i++) {
                        RenderWindow(Pages[i].Window, i);
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
                Screen s = Screen.PrimaryScreen;
                System.Drawing.Rectangle ScreenArea = s.WorkingArea;
                win.Height = ScreenArea.Height;
                win.Top = ScreenArea.Top;
                win.Width = SettingsManager.PageWidth;
                win.Left = ScreenArea.Right - win.Width;
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

        private Page[] PDFToPages() {
            _lastInstalledVersion =
                GhostscriptVersionInfo.GetLastInstalledVersion(
                        GhostscriptLicense.GPL | GhostscriptLicense.AFPL,
                        GhostscriptLicense.GPL);
            _rasterizer = new GhostscriptRasterizer();
            try {
                _rasterizer.Open(SettingsManager.FilePath, _lastInstalledVersion, false);
            } catch (FileNotFoundException) {
                ((MainWindow)System.Windows.Application.Current.MainWindow).TextBox_FilePath.Text = ErrorManager.InvalidFile();
                return new Page[0];
            }

            int PagesToRender = SettingsManager.NumOfPages;
            if (PagesToRender == 0) {
                PagesToRender = _rasterizer.PageCount;
            }
            if (PagesToRender > _rasterizer.PageCount) {
                PagesToRender = _rasterizer.PageCount;
            }
            int Index = SettingsManager.StartOnPage;
            Page[] Pages = new Page[PagesToRender - Index + 1];
            int PageIndex = 0;
            for (; Index <= PagesToRender; Index++) {
                try {
                    if (_rasterizer.PageCount >= Index) {
                        System.Drawing.Image img = _rasterizer.GetPage(desired_x_dpi, desired_y_dpi, Index);
                        Page Page = new Page(img, Index, main);
                        Pages[PageIndex] = Page;
                    }
                } catch (OutOfMemoryException) {
                    _rasterizer.Close();
                    return Pages;
                }
                PageIndex++;
            }
            _rasterizer.Close();
            return Pages;
        }

        internal class Page {
            public Page(System.Drawing.Image image, int pageNumber, MainWindow main) {
                Image = image;
                PageNumber = pageNumber;
                Main = main;
                Window = CreateWindow();
            }

            protected Window CreateWindow() {
                Window win = new Window();
                win.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/PDFPreview;component/Gfx/PDF.ico"));
                win.Title = $"Page {PageNumber} | PDFViewer";
                win.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 204, 204));
                StackPanel can = new StackPanel();
                can.VerticalAlignment = VerticalAlignment.Top;
                win.Content = can;
                win.Owner = Main;
                win.KeyUp += new System.Windows.Input.KeyEventHandler(_Navigator.MoveToPage);
                using (var stream = new MemoryStream(ImageToByteArray(Image))) {
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
                return win;
            }
            public Window Window { get; }
            protected int PageNumber;
            protected System.Drawing.Image Image;
            protected MainWindow Main;
            protected Navigator _Navigator = new Navigator();
        }
    }
}
