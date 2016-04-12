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
using PDFPreview.Core;

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
        private void RerenderImage(object source, FileSystemEventArgs e) {
            imageLoader.ImageConverter.RunWorkerAsync();
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
                SettingsManager.PageWidth = Convert.ToInt32(TextBox_PageWidth.Text);
                if (text != TextBox_PageWidth.Text) {
                    TextBox_PageWidth.Select(TextBox_PageWidth.Text.Count(), 0);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.MainWindow = this;
        }
    }
}
