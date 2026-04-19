namespace NomadSpot.ViewModel
{
    public class ColumnOption : BaseViewModel
    {
        public string Name { get; set; }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
    }
}
