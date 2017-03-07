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

            UrlHistories = UrlHistoriesManager.Instance.UrlHistories;

            UrlHistoriesManager.Instance.UrlHistoriesUpdated += (o, e) =>
            {
                UrlHistories = UrlHistoriesManager.Instance.UrlHistories.ToList();
                OnPropertyChanged(nameof(UrlHistories));
            };
        }

        public IList<string> UrlHistories { get; set; }

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
            vm.Url = UrlHistoriesManager.Instance.UrlHistories.FirstOrDefault() ?? "http://www.example.com/";
            vm.RequestBody = "# Lines start with '#' are comments and will be ignored.\n" +
                             "# Using JSON object to add headers and create request body.\n\n" +
                             "{\n" +
                             "    headers: {\n" +
                             "        # additional headers\n" +
                             "    },\n" +
                             "    # type can be one of { text, form, json }\n" +
                             "    type: \"form\",\n" +
                             "    # for text content, using string replace object \n" +
                             "    body: {\n" +
                             "    },\n" +
                             "    # for file uploading when type is form\n" + 
                             "    file: {\n" +
                             "        # name is required\n" + 
                             "        #name: \"picture\",\n" +
                             "        # path is required\n" +
                             "        #path: \"c:\\\\dir_to_file\\\\file_name.jpg\",\n" +
                             "        # filename is optional\n" +
                             "        #filename: \"example.jpg\"\n" +
                             "    }\n" +
                             "}";
            return vm;
        }
    }
}
