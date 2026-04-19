using System.Collections.Generic;
using System.Windows.Input;

namespace NomadSpot.ViewModel
{
    public interface IFilterViewModel
    {
        ICommand SearchCommand { get; set; }
        ICommand ClearCommand  { get; }
        event EventHandler Searched;
        List<FilterProperty> FilterProperties { get; }
        Dictionary<string, object> GetActiveFilters();
        void ClearFilters();
    }
}
