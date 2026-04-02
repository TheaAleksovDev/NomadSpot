using System.Collections.Generic;

namespace NomadSpot.ViewModel
{
    public interface IFilterViewModel
    {
        List<FilterProperty> FilterProperties { get; }
        Dictionary<string, object> GetActiveFilters();
        void ClearFilters();
    }
}
