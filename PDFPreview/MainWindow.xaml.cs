using PDFPreview.Core;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace PDFPreview {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private PageLoader imageLoader;
        public MainWindow() {
            Top = Screen.PrimaryScreen.WorkingArea.Top;
            Left = Screen.PrimaryScreen.WorkingArea.Left;
            InitializeComponent();
            SetWatcher();
            InitFields();
            imageLoader = new PageLoader(this);
            imageLoader.ImageConverter.RunWorkerAsync();
        }

        private void RerenderPages(object source, FileSystemEventArgs e) {
            if (!imageLoader.ImageConverter.IsBusy) {
                imageLoader.ImageConverter.RunWorkerAsync();
            }
        }

        private void InitFields() {
            TextBox_FilePath.Text = SettingsManager.FilePath;
            TextBox_NumOfPages.Text = Convert.ToString(SettingsManager.NumOfPages);
            TextBox_NumOfPagesStart.Text = Convert.ToString(SettingsManager.StartOnPage);
        }

        private void SetWatcher() {
            FileSystemWatcher watcher = new FileSystemWatcher();
            try {
                watcher.Path = System.IO.Path.GetDirectoryName(SettingsManager.FilePath);
                watcher.Filter = System.IO.Path.GetFileName(SettingsManager.FilePath);
                watcher.EnableRaisingEvents = true;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
                watcher.Changed += new FileSystemEventHandler(RerenderPages);
            } catch (Exception ex) when (ex is FormatException || ex is ArgumentException) {
                TextBox_FilePath.Text = ErrorManager.InvalidFile();
            }
        }

        private void TextBox_NumOfPages_TextChanged(object sender, TextChangedEventArgs e) {
            string text = TextBox_NumOfPages.Text;
            if (!text.Equals("")) {
                TextBox_NumOfPages.Text = Regex.Replace(text, "[^0-9]", "");
                if (!TextBox_NumOfPages.Text.Equals("")) {
                    SettingsManager.NumOfPages = Convert.ToInt32(TextBox_NumOfPages.Text);
                }
                if (text != TextBox_NumOfPages.Text) {
                    TextBox_NumOfPages.Select(TextBox_NumOfPages.Text.Count(), 0);
                }
            }
            TextBox_NumOfPagesStart_TextChanged(sender, e);
        }

        private void TextBox_NumOfPages_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Equals(Key.Enter) || e.Equals(Key.Escape)) {
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
                imageLoader.ImageConverter.RunWorkerAsync();
            }
        }

        private void Button_Select_File_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key.Equals(Key.Return)) {
                SettingsManager.FilePath = TextBox_FilePath.Text;
                SetWatcher();
                imageLoader.ImageConverter.RunWorkerAsync();
            }
        }

        private void Button_Refresh_Click(object sender, RoutedEventArgs e) {
            Dispatcher.InvokeAsync((Action)(() => {
                imageLoader.ImageConverter.RunWorkerAsync();
            }));
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
                if (!TextBox_PageWidth.Text.Equals("")) {
                    SettingsManager.PageWidth = Convert.ToInt32(TextBox_PageWidth.Text);
                }
                if (text != TextBox_PageWidth.Text) {
                    TextBox_PageWidth.Select(TextBox_PageWidth.Text.Count(), 0);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.MainWindow = this;
        }

        private void TextBox_NumOfPagesStart_TextChanged(object sender, TextChangedEventArgs e) {
            string text = TextBox_NumOfPagesStart.Text;
            if (!text.Equals("")) {
                TextBox_NumOfPagesStart.Text = Regex.Replace(text, "[^0-9]", "");
                if (!TextBox_NumOfPagesStart.Text.Equals("")) {
                    if (Convert.ToInt32(TextBox_NumOfPagesStart.Text) > SettingsManager.NumOfPages && SettingsManager.NumOfPages != 0) {
                        TextBox_NumOfPagesStart.Text = Convert.ToString(SettingsManager.NumOfPages);
                    }
                    if (Convert.ToInt32(TextBox_NumOfPagesStart.Text).Equals(0)) {
                        TextBox_NumOfPagesStart.Text = "1";
                    }
                    SettingsManager.StartOnPage = Convert.ToInt32(TextBox_NumOfPagesStart.Text);
                }
                if (text != TextBox_NumOfPagesStart.Text) {
                    TextBox_NumOfPagesStart.Select(TextBox_NumOfPagesStart.Text.Count(), 0);
                }
            }
        }
    }
}
