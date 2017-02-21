/* 
 * FileName:    MainWindowViewModel.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  2017/2/21 10:53:53
 * Version:     v1.0
 * Description:
 * */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JsonTextViewer
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel()
        {
            TaskList = new ObservableCollection<PageViewModel>();
            TaskList.Add(CreatePageViewModel());

            ClosePageCommand = new SimpleCommand(ClosePageCommandAction);
            AddPageCommand = new SimpleCommand(AddPageCommandAction);
            
        }

        public ObservableCollection<PageViewModel> TaskList { get; set; }

        public ICommand ClosePageCommand { get; set; }

        private void ClosePageCommandAction(object arg)
        {
            var item = arg as PageViewModel;
            if (item != null)
                TaskList.Remove(item);
        }

        public ICommand AddPageCommand { get; set; }

        public void AddPageCommandAction(object arg)
        {
            TaskList.Add(CreatePageViewModel());
        }


        private PageViewModel CreatePageViewModel()
        {
            var vm = new PageViewModel();
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
            return vm;
        }
    }
}
