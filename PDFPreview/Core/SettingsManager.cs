using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFPreview.Core {
    public static class SettingsManager {

        private static Properties.Settings Settings { get { return Properties.Settings.Default; } }

        public static int NumOfPages {
            get { return Settings.NumOfPages; }
            set {
                Settings.NumOfPages = value;
                Settings.Save();
            }
        }

        public static double PageWidth {
            get { return Settings.PageWidth; }
            set { Settings.PageWidth = value; }
        }

        public static string FilePath {
            get { return Settings.FilePath; }
            set {
                Settings.FilePath = value;
                Settings.Save();
            }
        }
        public static int StartOnPage {
            get { return Settings.StartOnPage; }
            set {
                Settings.StartOnPage = value;
                Settings.Save();
            }
        }
    }
}
