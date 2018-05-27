using FileFinder.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileFinder.Commands
{
    internal class SearchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
            //return SearchManagerViewModel.Instance.ReadyToSearch;
        }

        public void Execute(object parameter)
        {
            SearchManagerViewModel.Instance.ExecuteSearch();
        }
    }
}
