using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PDFPreview.Core {
    class Navigator {
        public void MoveToPage(object sender, System.Windows.Input.KeyEventArgs e) {
            var Window = System.Windows.Application.Current.Windows.OfType<Window>()
                .Select((str, index) => new { str, index })
                .Where(x => x.str.Equals(sender))
                .FirstOrDefault();

            if (e.Key == Key.Escape) {
                Window.str.Close();
                if (Application.Current.Windows.Count < 2) {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).WindowState = WindowState.Normal;
                    ((MainWindow)System.Windows.Application.Current.MainWindow).Activate();
                }
            }
            if (e.Key == Key.Left && Window.index < System.Windows.Application.Current.Windows.Count && Window.index > 1) {
                Window moveTo = System.Windows.Application.Current.Windows.OfType<Window>().ElementAt(Window.index - 1);
                if (moveTo.WindowState == WindowState.Minimized) {
                    moveTo.WindowState = WindowState.Normal;
                }
                moveTo.Activate();
            }
            if (e.Key == Key.Right && Window.index < System.Windows.Application.Current.Windows.Count - 1) {
                Window moveTo = System.Windows.Application.Current.Windows.OfType<Window>().ElementAt(Window.index + 1);
                if (moveTo.WindowState == WindowState.Minimized) {
                    moveTo.WindowState = WindowState.Normal;
                }
                moveTo.Activate();

            }
            e.Handled = true;
        }

    }
}
