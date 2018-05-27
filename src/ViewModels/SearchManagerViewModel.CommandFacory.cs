using FileFinder.Commands;
using System.Windows.Input;

namespace FileFinder.ViewModel
{
    public partial class SearchManagerViewModel : BaseViewModel
    {
        SearchCommand _searchCommand;
        public ICommand GetSearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new SearchCommand());
            }
        }
    }
}