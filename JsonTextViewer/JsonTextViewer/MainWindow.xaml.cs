using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace JsonTextViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var ver = this.GetType().Assembly.GetName().Version;
            string version = $"v{ver.Major}.{ver.Minor}";
            Title = $"Json Text Viewer {version}";
           
            DataContext = new MainWindowViewModel();    
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            UrlHistoriesManager.Instance.SaveToFile();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                if (sender is TextBox tb)
                {
                    const string spaces = "  ";
                    string tmp = tb.Text;
                    if (tb.SelectionLength > 0)
                    {
                        // remove selected text before inserting,
                        // just like replace selected text with spaces
                        tmp = tmp.Remove(tb.SelectionStart, tb.SelectionLength);
                    }
                    tmp = tmp.Insert(tb.CaretIndex, spaces);
                    int newIndex = tb.CaretIndex + spaces.Length;
                    tb.Text = tmp;
                    tb.CaretIndex = newIndex;
                    e.Handled = true;
                }
            }
        }
    }
}
