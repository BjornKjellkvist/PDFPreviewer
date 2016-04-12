using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PDFPreview.Core {
    class Navigator {
        bool FirstTime = true;
        public void MoveToPage(object sender, System.Windows.Input.KeyEventArgs e) {
            //FIRSTTIME FIXES RECURSION, DONT ASK ME WHY...
            var WinIndex = System.Windows.Application.Current.Windows.OfType<Window>()
                .Select((str, index) => new { str, index })
                .Where(x => x.str.Equals(sender))
                .FirstOrDefault();

            if (e.Key == Key.Escape && FirstTime) {
                WinIndex.str.Close();
                FirstTime = false;
                return;
            }
            if (e.Key == Key.Left && WinIndex.index < System.Windows.Application.Current.Windows.Count && WinIndex.index > 1) {
                Window moveTo = System.Windows.Application.Current.Windows.OfType<Window>().ElementAt(WinIndex.index - 1);
                if (moveTo.WindowState == WindowState.Minimized) {
                    moveTo.WindowState = WindowState.Normal;
                }
                moveTo.Activate();
            }
            if (FirstTime && e.Key == Key.Right && WinIndex.index < System.Windows.Application.Current.Windows.Count - 1) {
                Window moveTo = System.Windows.Application.Current.Windows.OfType<Window>().ElementAt(WinIndex.index + 1);
                if (moveTo.WindowState == WindowState.Minimized) {
                    moveTo.WindowState = WindowState.Normal;
                }
                moveTo.Activate();
                FirstTime = false;
                return;
            }
            FirstTime = true;
        }
    }
}
