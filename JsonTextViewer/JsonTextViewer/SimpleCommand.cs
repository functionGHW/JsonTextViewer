/* 
 * FileName:    SimpleCommand.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  1/24/2016 1:38:13 PM
 * Version:     v1.0
 * Description:
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JsonTextViewer
{
    public class SimpleCommand : ICommand
    {
        private readonly Action<object> cmd;

        public SimpleCommand(Action<object> cmd)
        {
            this.cmd = cmd;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            cmd?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
