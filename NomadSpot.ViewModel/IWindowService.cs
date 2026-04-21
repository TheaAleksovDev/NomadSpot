namespace NomadSpot.ViewModel
{
    public interface IWindowService
    {
        void ShowLocations();
        void ShowAddLocation();
        void ShowAddReview();
        void ShowReviews();
        void ShowFilter(IFilterViewModel filter);
        void ShowReviewFilter(IFilterViewModel filter);
    }
}
