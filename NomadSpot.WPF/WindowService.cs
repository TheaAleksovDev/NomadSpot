using NomadSpot.ViewModel;

namespace NomadSpot.WPF
{
    public class WindowService : IWindowService
    {
        private readonly object _dataContext;
        private readonly IFilterControlFactory _filterControlFactory;

        public WindowService(object dataContext, IFilterControlFactory filterControlFactory)
        {
            _dataContext = dataContext;
            _filterControlFactory = filterControlFactory;
        }

        public void ShowLocations()
        {
            var w = new Views.LocationListWindow { DataContext = _dataContext };
            w.Show();
        }

        public void ShowAddLocation()
        {
            var w = new Views.AddLocationWindow { DataContext = _dataContext };
            w.Show();
        }

        public void ShowAddReview()
        {
            var w = new Views.AddReviewWindow { DataContext = _dataContext };
            w.Show();
        }

        public void ShowReviews()
        {
            var w = new Views.ReviewListWindow { DataContext = _dataContext };
            w.Show();
        }

        public void ShowFilter(IFilterViewModel filter)
        {
            new Views.FilterWindow(filter, _filterControlFactory).Show();
        }

        public void ShowReviewFilter(IFilterViewModel filter)
        {
            new Views.FilterWindow(filter, _filterControlFactory).Show();
        }
    }
}
