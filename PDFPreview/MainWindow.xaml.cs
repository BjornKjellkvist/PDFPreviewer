using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Ghostscript.NET.Rasterizer;
using Ghostscript.NET;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace PDFPreview {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            Top = Screen.PrimaryScreen.WorkingArea.Top;
            Left = Screen.PrimaryScreen.WorkingArea.Left;
            InitializeComponent();
            SetWatcher();
            RenderImage();
            InitFields();
        }
        private void RerenderImage(object source, FileSystemEventArgs e) {
            RenderImage();
        }

        private void InitFields() {
            TextBox_FilePath.Text = SettingsManager.FilePath;
            TextBox_NumOfPages.Text = Convert.ToString(SettingsManager.NumOfPages);
        }

        private void SetWatcher() {
            FileSystemWatcher watcher = new FileSystemWatcher();
            try {
                watcher.Path = System.IO.Path.GetDirectoryName(SettingsManager.FilePath);
                watcher.Filter = System.IO.Path.GetFileName(SettingsManager.FilePath);
                watcher.EnableRaisingEvents = true;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
                watcher.Changed += new FileSystemEventHandler(RerenderImage);
            } catch (Exception ex) when (ex is FormatException || ex is ArgumentException) {
                TextBox_FilePath.Text = ErrorManager.InvalidFile();
            }
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
                    win.Left = (ScreenArea.Left + SettingsManager.PageWidth) + (70 * Iteration / 2);
                }
                win.Height = ScreenArea.Height;
                win.Show();
            } else {
                win.Show();
            }
        }

        public void RenderImage() {
            Dispatcher.Invoke((Action)(() => {
                foreach (Window w in System.Windows.Application.Current.Windows) {
                    if (w != this)
                        w.Close();
                }
            }));
            int NumberOfPagesToShow;
            int UsersNumberOfPages = SettingsManager.NumOfPages;
            if (SettingsManager.DefinedPages) {
                if (UsersNumberOfPages > PDFToIamges().Count()) {
                    NumberOfPagesToShow = PDFToIamges().Count();
                } else {
                    NumberOfPagesToShow = UsersNumberOfPages;
                }
            } else {
                NumberOfPagesToShow = PDFToIamges().Count();
            }
            for (int i = 0; i < NumberOfPagesToShow; i++) {
                System.Drawing.Image page = PDFToIamges()[i];
                Dispatcher.Invoke((Action)(() => {
                    Window win = new Window();
                    win.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/PDFPreview;component/Gfx/PDF.ico"));
                    win.Title = $"Page {i + 1} | PDFViewer";
                    win.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 204, 204));
                    StackPanel can = new StackPanel();
                    can.VerticalAlignment = VerticalAlignment.Top;
                    win.Content = can;
                    RenderWindow(win, i);
                    using (var stream = new MemoryStream(ImageToByteArray(page))) {
                        Dispatcher.Invoke((Action)(() => {
                            BitmapImage imageIn = new BitmapImage();
                            imageIn.BeginInit();
                            imageIn.StreamSource = stream;
                            imageIn.CacheOption = BitmapCacheOption.OnLoad;
                            imageIn.EndInit();
                            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                            image.Source = imageIn;
                            image.Stretch = Stretch.None;
                            can.Children.Add(image);
                        }));
                    }
                }));
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


        public System.Drawing.Image[] PDFToIamges() {
            _lastInstalledVersion =
                GhostscriptVersionInfo.GetLastInstalledVersion(
                        GhostscriptLicense.GPL | GhostscriptLicense.AFPL,
                        GhostscriptLicense.GPL);
            _rasterizer = new GhostscriptRasterizer();
            try {
                _rasterizer.Open(SettingsManager.FilePath, _lastInstalledVersion, true);
            } catch (FileNotFoundException) {
                TextBox_FilePath.Text = ErrorManager.InvalidFile();
                return new System.Drawing.Image[0];
            }


            System.Drawing.Image[] images = new System.Drawing.Image[_rasterizer.PageCount];
            for (int pageNumber = 1; pageNumber <= _rasterizer.PageCount; pageNumber++) {
                System.Drawing.Image img = _rasterizer.GetPage(desired_x_dpi, desired_y_dpi, pageNumber);
                images[pageNumber - 1] = img;
            }
            _rasterizer.Close();
            return images;
        }

        private void TextBox_NumOfPages_TextChanged(object sender, TextChangedEventArgs e) {
            string text = TextBox_NumOfPages.Text;
            if (!text.Equals("")) {
                TextBox_NumOfPages.Text = Regex.Replace(text, "[^0-9]", "");
                SettingsManager.NumOfPages = Convert.ToInt32(TextBox_NumOfPages.Text);
                if (text != TextBox_NumOfPages.Text) {
                    TextBox_NumOfPages.Select(TextBox_NumOfPages.Text.Count(), 0);
                }
            }
        }

        private void TextBox_NumOfPages_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Equals(Key.Enter) || e.Equals(Key.Escape)) {
                //TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                //MoveFocus(request);
                Keyboard.ClearFocus();
            }
        }

        private void Button_Select_File_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog FilePicker = new OpenFileDialog();
            FilePicker.Filter = "PDF Files | *.pdf";
            if (FilePicker.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                TextBox_FilePath.Text = FilePicker.FileName;
                SettingsManager.FilePath = FilePicker.FileName;
                SetWatcher();
                RenderImage();
            }
        }
        private void Button_Select_File_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key.Equals(Key.Return)) {
                SettingsManager.FilePath = TextBox_FilePath.Text;
                SetWatcher();
                RenderImage();
            }
        }
        private void Button_Refresh_Click(object sender, RoutedEventArgs e) {
            RenderImage();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            System.Windows.Application.Current.Shutdown();

        }
        public void Close(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.Shutdown();
        }
        private void TextBox_PageWidth_TextChanged(object sender, TextChangedEventArgs e) {
            string text = TextBox_PageWidth.Text;
            if (!text.Equals("")) {
                TextBox_PageWidth.Text = Regex.Replace(text, "[^0-9]", "");
                SettingsManager.PageWidth = Convert.ToInt32(TextBox_PageWidth.Text);
                if (text != TextBox_PageWidth.Text) {
                    TextBox_PageWidth.Select(TextBox_PageWidth.Text.Count(), 0);
                }
            }
        }
    }
}
