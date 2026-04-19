using NomadSpot.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace NomadSpot.WPF
{
    public interface IFilterControlFactory
    {
        FrameworkElement Build(IEnumerable<FilterProperty> properties);
    }
}
