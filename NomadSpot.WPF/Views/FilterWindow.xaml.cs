using NomadSpot.ViewModel;
using System.Windows;

namespace NomadSpot.WPF.Views
{
    public partial class FilterWindow : Window
    {
        public FilterWindow(IFilterViewModel filterViewModel, IFilterControlFactory factory)
        {
            InitializeComponent();
            DataContext = filterViewModel;
            FiltersScrollViewer.Content = factory.Build(filterViewModel.FilterProperties);
        }
    }
}
