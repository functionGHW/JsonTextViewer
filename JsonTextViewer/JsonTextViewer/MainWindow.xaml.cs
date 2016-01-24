using System;
using System.Collections.Generic;
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
            var vm = new ViewModel();
            DataContext = vm;

            vm.Method = "Get";
            vm.ResponseText = "Press Enter to send request";
            vm.Url = "http://www.example.com/";
            vm.RequestBody = "# Lines start with '#' are comments and will be ignored.\n" +
                             "# Add your message body for Post or Put here.\n" +
                             "# Default type of the body is text,\n" +
                             "# you can change it by add a line \":: {type}\" as the first valid line of the content.\n" +
                             "# The type can be one of { text, form, json }\n" + 
                             "# Form content example:\n" +
                             "#     :: form\n" +
                             "#     name=John\n" +
                             "#     Age=23\n";
        }
    }
}
