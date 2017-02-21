/* 
 * FileName:    ViewModelBase.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  2017/2/21 10:51:45
 * Version:     v1.0
 * Description:
 * */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JsonTextViewer
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implemenetation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            var events = PropertyChanged;
            events?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }
}
