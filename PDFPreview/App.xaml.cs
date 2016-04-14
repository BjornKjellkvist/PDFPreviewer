using PDFPreview.Core;
using System.Windows;

namespace PDFPreview {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            if (e.Args.Length > 1) {
                if (!string.IsNullOrWhiteSpace(e.Args[0])) {
                    SettingsManager.FilePath = e.Args[0];
                }
            }
            base.OnStartup(e);
        }
    }
}
