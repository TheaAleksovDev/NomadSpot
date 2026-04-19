using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace NomadSpot.ViewModel
{
    public class LocationViewModel : BaseViewModel
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IReviewRepository _reviewRepository;

        // ── Search form ──────────────────────────────────────────────────────────
        private double _userLatitude  = 42.6977;
        private double _userLongitude = 23.3219;
        private int _resultCount = 3;
        private int _searchTypeIndex = 0; // 0 = Indoor, 1 = Outdoor

        public double UserLatitude   { get => _userLatitude;   set => SetProperty(ref _userLatitude, value); }
        public double UserLongitude  { get => _userLongitude;  set => SetProperty(ref _userLongitude, value); }
        public int    ResultCount    { get => _resultCount;    set => SetProperty(ref _resultCount, value); }

        public int SearchTypeIndex
        {
            get => _searchTypeIndex;
            set
            {
                SetProperty(ref _searchTypeIndex, value);
                OnPropertyChanged(nameof(IsIndoorSearch));
                OnPropertyChanged(nameof(CurrentFilter));
            }
        }
        public bool IsIndoorSearch => _searchTypeIndex == 0;
        public IFilterViewModel CurrentFilter =>
            IsIndoorSearch ? IndoorFilter : (IFilterViewModel)OutdoorFilter;

        // ── Lists ────────────────────────────────────────────────────────────────
        public ListViewModel<Location> LocationList { get; } = new ListViewModel<Location>();
        public ListViewModel<Review>   ReviewList   { get; } = new ListViewModel<Review>();
        public FilterViewModel<IndoorLocation>  IndoorFilter  { get; } = new FilterViewModel<IndoorLocation>();
        public FilterViewModel<OutdoorLocation> OutdoorFilter { get; } = new FilterViewModel<OutdoorLocation>();
        public FilterViewModel<Review>          ReviewFilter  { get; } = new FilterViewModel<Review>();

        // ── Computed from selected location ──────────────────────────────────────
        public bool   HasSelectedLocation      => LocationList.SelectedItem != null;
        public bool   IsSelectedLocationIndoor => LocationList.SelectedItem?.LocationType == "Indoor";
        public bool   IsSelectedLocationOutdoor=> LocationList.SelectedItem?.LocationType == "Outdoor";
        public string ReviewListTitle          => $"Reviews for: {LocationList.SelectedItem?.Name ?? ""}";

        // ── Selected location details ─────────────────────────────────────────────
        private List<KeyValuePair<string, object>> _selectedLocationDetails;
        public List<KeyValuePair<string, object>> SelectedLocationDetails
        {
            get => _selectedLocationDetails;
            private set => SetProperty(ref _selectedLocationDetails, value);
        }

        private void LoadLocationDetails(Location loc)
        {
            SelectedLocationDetails = loc == null ? null
                : loc.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(loc) ?? "-"))
                    .ToList();
        }

        // ── Selected review ───────────────────────────────────────────────────────
        private Review _selectedReview;
        public Review SelectedReview
        {
            get => _selectedReview;
            set
            {
                _selectedReview = value;
                ReviewList.SelectedItem = value;
                LoadReviewDetails(value);
                OnPropertyChanged(nameof(SelectedReview));
                OnPropertyChanged(nameof(HasSelectedReview));
            }
        }
        public bool HasSelectedReview => _selectedReview != null;

        private List<KeyValuePair<string, object>> _selectedReviewDetails;
        public List<KeyValuePair<string, object>> SelectedReviewDetails
        {
            get => _selectedReviewDetails;
            private set => SetProperty(ref _selectedReviewDetails, value);
        }

        private void LoadReviewDetails(Review review)
        {
            SelectedReviewDetails = review == null ? null
                : review.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(review) ?? "-"))
                    .ToList();
        }

        public List<string> FilteredReviewColumns => ReviewList.GetAllColumnNames()
            .Where(c => LocationList.SelectedItem?.LocationType == "Indoor"
                ? !ReviewOutdoorOnlyColumns.Contains(c)
                : !ReviewIndoorOnlyColumns.Contains(c))
            .ToList();

        // ── Add Location form properties ─────────────────────────────────────────
        private string _newName = "";
        private string _newAddress = "";
        private double _newLatitude;
        private double _newLongitude;
        private int    _newLocationTypeIndex = 0; // 0 = Indoor, 1 = Outdoor
        private int    _newNoiseLevel = 1;
        private int    _newWifiStrength = 0;
        private bool   _newHasPowerOutlets;
        private int    _newComfortLevel = 1;
        private int    _newPriceLevel = 1;
        private string _newOpeningHours = "08:00-22:00";
        private int    _newIndoorTypeIndex = 0;
        private bool   _newHasBenches;
        private bool   _newHasShade;
        private bool   _newPetFriendly;
        private bool   _newHasPublicToilet;
        private bool   _newNearShops;
        private int    _newOutdoorTypeIndex = 0;

        public static readonly string[] IndoorTypes  = { "Cafe", "Library", "CoworkingSpace", "University", "Other" };
        public static readonly string[] OutdoorTypes = { "Park", "Bench", "SportsCourt", "Beach", "Garden", "Plaza", "Other" };

        public static readonly string[] LocationIndoorOnlyColumns  = { "ComfortLevel", "PriceLevel", "OpeningHours", "IndoorType" };
        public static readonly string[] LocationOutdoorOnlyColumns = { "HasBenches", "HasShade", "PetFriendly", "HasPublicToilet", "NearShops", "OutdoorType" };
        public static readonly string[] ReviewIndoorOnlyColumns    = { "ComfortLevel", "PriceLevel" };
        public static readonly string[] ReviewOutdoorOnlyColumns   = { "Cleanliness", "Crowdedness" };

        public string NewName
        {
            get => _newName;
            set { if (SetProperty(ref _newName, value)) ((RelayCommand)SaveNewLocationCommand).RaiseCanExecuteChanged(); }
        }
        public string NewAddress         { get => _newAddress;         set => SetProperty(ref _newAddress, value); }
        public double NewLatitude
        {
            get => _newLatitude;
            set { if (SetProperty(ref _newLatitude, value)) ((RelayCommand)SaveNewLocationCommand).RaiseCanExecuteChanged(); }
        }
        public double NewLongitude
        {
            get => _newLongitude;
            set { if (SetProperty(ref _newLongitude, value)) ((RelayCommand)SaveNewLocationCommand).RaiseCanExecuteChanged(); }
        }
        public int NewLocationTypeIndex
        {
            get => _newLocationTypeIndex;
            set
            {
                SetProperty(ref _newLocationTypeIndex, value);
                OnPropertyChanged(nameof(NewIsIndoor));
                OnPropertyChanged(nameof(NewIsOutdoor));
            }
        }
        public bool NewIsIndoor  => _newLocationTypeIndex == 0;
        public bool NewIsOutdoor => _newLocationTypeIndex == 1;
        public int  NewNoiseLevel       { get => _newNoiseLevel;       set => SetProperty(ref _newNoiseLevel, value); }
        public int  NewWifiStrength     { get => _newWifiStrength;     set => SetProperty(ref _newWifiStrength, value); }
        public bool NewHasPowerOutlets  { get => _newHasPowerOutlets;  set => SetProperty(ref _newHasPowerOutlets, value); }
        public int  NewComfortLevel     { get => _newComfortLevel;     set => SetProperty(ref _newComfortLevel, value); }
        public int  NewPriceLevel       { get => _newPriceLevel;       set => SetProperty(ref _newPriceLevel, value); }
        public string NewOpeningHours   { get => _newOpeningHours;     set => SetProperty(ref _newOpeningHours, value); }
        public int  NewIndoorTypeIndex  { get => _newIndoorTypeIndex;  set => SetProperty(ref _newIndoorTypeIndex, value); }
        public IndoorType NewIndoorType
        {
            get => Enum.Parse<IndoorType>(IndoorTypes[_newIndoorTypeIndex]);
            set => NewIndoorTypeIndex = Array.IndexOf(IndoorTypes, value.ToString());
        }
        public bool NewHasBenches       { get => _newHasBenches;       set => SetProperty(ref _newHasBenches, value); }
        public bool NewHasShade         { get => _newHasShade;         set => SetProperty(ref _newHasShade, value); }
        public bool NewPetFriendly      { get => _newPetFriendly;      set => SetProperty(ref _newPetFriendly, value); }
        public bool NewHasPublicToilet  { get => _newHasPublicToilet;  set => SetProperty(ref _newHasPublicToilet, value); }
        public bool NewNearShops        { get => _newNearShops;        set => SetProperty(ref _newNearShops, value); }
        public int  NewOutdoorTypeIndex { get => _newOutdoorTypeIndex; set => SetProperty(ref _newOutdoorTypeIndex, value); }
        public OutdoorType NewOutdoorType
        {
            get => Enum.Parse<OutdoorType>(OutdoorTypes[_newOutdoorTypeIndex]);
            set => NewOutdoorTypeIndex = Array.IndexOf(OutdoorTypes, value.ToString());
        }

        // ── Add Review form properties ────────────────────────────────────────────
        private string _newReviewAuthor = "";
        private string _newReviewComment = "";
        private int    _newReviewRating = 1;
        private int    _newReviewNoiseLevel = 1;
        private int    _newReviewWifiStrength = 0;
        private int    _newReviewComfortLevel = 1;
        private int    _newReviewPriceLevel = 1;
        private int    _newReviewCleanliness = 1;
        private int    _newReviewCrowdedness = 1;

        public string NewReviewAuthor
        {
            get => _newReviewAuthor;
            set { if (SetProperty(ref _newReviewAuthor, value)) ((RelayCommand)SaveNewReviewCommand).RaiseCanExecuteChanged(); }
        }
        public string NewReviewComment      { get => _newReviewComment;      set => SetProperty(ref _newReviewComment, value); }
        public int    NewReviewRating       { get => _newReviewRating;       set => SetProperty(ref _newReviewRating, value); }
        public int    NewReviewNoiseLevel   { get => _newReviewNoiseLevel;   set => SetProperty(ref _newReviewNoiseLevel, value); }
        public int    NewReviewWifiStrength { get => _newReviewWifiStrength; set => SetProperty(ref _newReviewWifiStrength, value); }
        public int    NewReviewComfortLevel { get => _newReviewComfortLevel; set => SetProperty(ref _newReviewComfortLevel, value); }
        public int    NewReviewPriceLevel   { get => _newReviewPriceLevel;   set => SetProperty(ref _newReviewPriceLevel, value); }
        public int    NewReviewCleanliness  { get => _newReviewCleanliness;  set => SetProperty(ref _newReviewCleanliness, value); }
        public int    NewReviewCrowdedness  { get => _newReviewCrowdedness;  set => SetProperty(ref _newReviewCrowdedness, value); }

        // ── Status message ───────────────────────────────────────────────────────
        private string _statusMessage;
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        // ── Column selection ──────────────────────────────────────────────────────
        private static readonly string[] DefaultLocationColumns = { "Name", "Address", "LocationType", "Rating" };
        private static readonly string[] DefaultReviewColumns   = { "Author", "Rating", "NoiseLevel", "WifiStrength", "Comment", "Date" };

        public ObservableCollection<ColumnOption> LocationColumns { get; private set; } = new();
        public ObservableCollection<ColumnOption> ReviewColumns   { get; private set; } = new();

        // For Blazor compatibility
        public List<string> SelectedLocationColumns { get; private set; } = new(DefaultLocationColumns);
        public List<string> SelectedReviewColumns   { get; private set; } = new(DefaultReviewColumns);

        public void ToggleLocationColumn(string col, bool add)
        {
            if (add && !SelectedLocationColumns.Contains(col)) SelectedLocationColumns.Add(col);
            else if (!add) SelectedLocationColumns.Remove(col);
            LocationList.SetColumns(SelectedLocationColumns);
        }

        public void ToggleReviewColumn(string col, bool add)
        {
            if (add && !SelectedReviewColumns.Contains(col)) SelectedReviewColumns.Add(col);
            else if (!add) SelectedReviewColumns.Remove(col);
            ReviewList.SetColumns(SelectedReviewColumns);
        }

        public void SelectLocation(Location loc) => LocationList.SelectedItem = loc;

        // ── Navigation events ────────────────────────────────────────────────────
        public event EventHandler LocationsFound;
        public event EventHandler OpenAddLocationRequested;
        public event EventHandler OpenAddReviewRequested;
        public event EventHandler OpenReviewListRequested;
        public event EventHandler LocationSaved;
        public event EventHandler ReviewSaved;
        public event Action<IFilterViewModel> OpenFilterRequested;
        public event Action<IFilterViewModel> OpenReviewFilterRequested;

        // ── Commands ─────────────────────────────────────────────────────────────
        public ICommand FindLocationsCommand    { get; }
        public ICommand FindWithFiltersCommand  { get; }
        public ICommand OpenAddLocationCommand  { get; }
        public ICommand OpenAddReviewCommand    { get; }
        public ICommand ViewReviewsCommand      { get; }
        public ICommand SaveNewLocationCommand  { get; }
        public ICommand SaveNewReviewCommand    { get; }
        public ICommand MarkInactiveCommand     { get; }
        public ICommand ClearLocationsCommand   { get; }
        public ICommand ClearReviewsCommand     { get; }
        public ICommand FilterReviewsCommand    { get; }
        public ICommand OpenFilterCommand       { get; }
        public ICommand ClearFiltersCommand         { get; }
        public ICommand OpenReviewFilterCommand     { get; }
        public ICommand ClearResultsCommand         { get; }
        public ICommand UseMyLocationCommand        { get; set; }
        public ICommand CloseReviewDetailCommand    { get; }
        public ICommand CloseLocationDetailCommand  { get; }

        public LocationViewModel(ILocationRepository locationRepository, IReviewRepository reviewRepository)
        {
            _locationRepository = locationRepository;
            _reviewRepository   = reviewRepository;

            FindLocationsCommand   = new RelayCommand(ExecuteFindLocations);
            FindWithFiltersCommand = new RelayCommand(ExecuteFindWithFilters);
            OpenAddLocationCommand = new RelayCommand(() => OpenAddLocationRequested?.Invoke(this, EventArgs.Empty));
            OpenAddReviewCommand   = new RelayCommand(param =>
            {
                if (param is Location loc) SelectLocation(loc);
                ResetNewReviewForm();
                OpenAddReviewRequested?.Invoke(this, EventArgs.Empty);
            });
            ViewReviewsCommand     = new RelayCommand(param => ExecuteViewReviews(param));
            SaveNewLocationCommand = new RelayCommand(ExecuteSaveNewLocation,
                                                      () => !string.IsNullOrWhiteSpace(NewName)
                                                            && NewLatitude != 0
                                                            && NewLongitude != 0);
            SaveNewReviewCommand   = new RelayCommand(ExecuteSaveNewReview,
                                                      () => !string.IsNullOrWhiteSpace(NewReviewAuthor));
            MarkInactiveCommand    = new RelayCommand(param => ExecuteMarkInactive(param), () => HasSelectedLocation);
            ClearLocationsCommand  = new RelayCommand(() => { LocationList.Clear(); LocationList.SelectedItem = null; });
            ClearReviewsCommand    = new RelayCommand(() => ReviewList.Clear());
            FilterReviewsCommand   = new RelayCommand(ExecuteFilterReviews);
            OpenFilterCommand      = new RelayCommand(ExecuteOpenFilter);
            ClearFiltersCommand      = new RelayCommand(() => { IndoorFilter.ClearFilters(); OutdoorFilter.ClearFilters(); });
            OpenReviewFilterCommand  = new RelayCommand(() => OpenReviewFilterRequested?.Invoke(ReviewFilter));
            CloseReviewDetailCommand    = new RelayCommand(() => SelectedReview = null);
            CloseLocationDetailCommand  = new RelayCommand(() => SelectLocation(null));
            ClearResultsCommand        = new RelayCommand(() =>
            {
                LocationList.Clear();
                ReviewList.Clear();
                LocationList.SelectedItem = null;
                SelectedLocationColumns = new List<string>(DefaultLocationColumns);
                SelectedReviewColumns   = new List<string>(DefaultReviewColumns);
                LocationList.SetColumns(SelectedLocationColumns);
                ReviewFilter.ClearFilters();
                StatusMessage = null;
            });

            IndoorFilter.SearchCommand  = FindWithFiltersCommand;
            OutdoorFilter.SearchCommand = FindWithFiltersCommand;
            ReviewFilter.SearchCommand  = FilterReviewsCommand;

            LocationList.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LocationList.SelectedItem))
                {
                    LoadLocationDetails(LocationList.SelectedItem);
                    OnPropertyChanged(nameof(HasSelectedLocation));
                    OnPropertyChanged(nameof(IsSelectedLocationIndoor));
                    OnPropertyChanged(nameof(IsSelectedLocationOutdoor));
                    OnPropertyChanged(nameof(ReviewListTitle));
                    ((RelayCommand)OpenAddReviewCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ViewReviewsCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)MarkInactiveCommand).RaiseCanExecuteChanged();
                }
            };
        }

        // ── Command implementations ───────────────────────────────────────────────
        private void ExecuteFindLocations()
        {
            IndoorFilter.ClearFilters();
            OutdoorFilter.ClearFilters();
            FindClosestLocations(IsIndoorSearch);
            RebuildLocationColumns(IsIndoorSearch);
            SelectedLocationColumns = new List<string>(DefaultLocationColumns);
            LocationList.SetColumns(SelectedLocationColumns);
            LocationsFound?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteFindWithFilters()
        {
            FindClosestLocations(IsIndoorSearch);
            RebuildLocationColumns(IsIndoorSearch);
            LocationsFound?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteOpenFilter()
        {
            OpenFilterRequested?.Invoke(CurrentFilter);
        }

        private void ExecuteViewReviews(object param)
        {
            if (param is Location loc) SelectLocation(loc);
            var target = LocationList.SelectedItem;
            if (target == null) return;
            LoadReviews(target.Id);
            var isIndoor = target.LocationType == "Indoor";
            RebuildReviewColumns(isIndoor);
            var excluded = isIndoor ? ReviewOutdoorOnlyColumns : ReviewIndoorOnlyColumns;
            SelectedReviewColumns = new List<string>(DefaultReviewColumns)
                .Where(c => !excluded.Contains(c)).ToList();
            ReviewList.SetColumns(SelectedReviewColumns);
            OpenReviewListRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteSaveNewLocation()
        {
            Location location;
            if (NewIsIndoor)
            {
                location = new IndoorLocation
                {
                    Name             = NewName,
                    Address          = NewAddress,
                    Latitude         = NewLatitude,
                    Longitude        = NewLongitude,
                    NoiseLevel       = NewNoiseLevel,
                    WifiStrength     = NewWifiStrength,
                    HasPowerOutlets  = NewHasPowerOutlets,
                    IsActive         = true,
                    LocationType     = "Indoor",
                    ComfortLevel     = NewComfortLevel,
                    PriceLevel       = NewPriceLevel,
                    OpeningHours     = NewOpeningHours,
                    IndoorType       = Enum.Parse<IndoorType>(IndoorTypes[NewIndoorTypeIndex])
                };
            }
            else
            {
                location = new OutdoorLocation
                {
                    Name             = NewName,
                    Address          = NewAddress,
                    Latitude         = NewLatitude,
                    Longitude        = NewLongitude,
                    NoiseLevel       = NewNoiseLevel,
                    WifiStrength     = NewWifiStrength,
                    HasPowerOutlets  = NewHasPowerOutlets,
                    IsActive         = true,
                    LocationType     = "Outdoor",
                    HasBenches       = NewHasBenches,
                    HasShade         = NewHasShade,
                    PetFriendly      = NewPetFriendly,
                    HasPublicToilet  = NewHasPublicToilet,
                    NearShops        = NewNearShops,
                    OutdoorType      = Enum.Parse<OutdoorType>(OutdoorTypes[NewOutdoorTypeIndex])
                };
            }

            AddLocation(location);
            StatusMessage = "Location added successfully!";
            LocationSaved?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteSaveNewReview()
        {
            var review = new Review
            {
                LocationId    = LocationList.SelectedItem.Id,
                Author        = NewReviewAuthor,
                Comment       = NewReviewComment,
                Rating        = NewReviewRating,
                NoiseLevel    = NewReviewNoiseLevel,
                WifiStrength  = NewReviewWifiStrength,
                ComfortLevel  = IsSelectedLocationIndoor ? NewReviewComfortLevel : 0,
                PriceLevel    = IsSelectedLocationIndoor ? NewReviewPriceLevel   : 0,
                Cleanliness   = IsSelectedLocationIndoor ? 0 : NewReviewCleanliness,
                Crowdedness   = IsSelectedLocationIndoor ? 0 : NewReviewCrowdedness,
                Date          = DateTime.Now
            };

            var locationId = review.LocationId;
            AddReview(review);
            FindClosestLocations(LastSearchWasIndoor);
            LocationList.SelectedItem = LocationList.Items.FirstOrDefault(l => l.Id == locationId);
            StatusMessage = "Review added successfully!";
            ReviewSaved?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteMarkInactive(object param)
        {
            if (param is Location loc) SelectLocation(loc);
            var target = LocationList.SelectedItem;
            if (target == null) return;
            SetInactive(target.Id);
            FindClosestLocations(LastSearchWasIndoor);
            LocationList.SelectedItem = null;
            StatusMessage = $"'{target.Name}' marked as inactive.";
        }

        private void ExecuteFilterReviews()
        {
            var filters = ReviewFilter.GetActiveFilters();
            ReviewList.SetItems(LoadReviewsWithFilter(filters));
        }

        // ── Form helpers ──────────────────────────────────────────────────────────
        public void ResetNewReviewForm()
        {
            NewReviewAuthor       = "";
            NewReviewRating       = 1;
            NewReviewNoiseLevel   = 1;
            NewReviewWifiStrength = 0;
            NewReviewComfortLevel = 1;
            NewReviewPriceLevel   = 1;
            NewReviewCleanliness  = 1;
            NewReviewCrowdedness  = 1;
            NewReviewComment      = "";
        }

        // ── Column builders ───────────────────────────────────────────────────────
        private void RebuildLocationColumns(bool indoorOnly)
        {
            var excluded = indoorOnly ? LocationOutdoorOnlyColumns : LocationIndoorOnlyColumns;
            var type     = indoorOnly ? typeof(IndoorLocation) : typeof(OutdoorLocation);
            LocationColumns = new ObservableCollection<ColumnOption>(
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => p.Name)
                    .Where(n => !excluded.Contains(n))
                    .Select(n => new ColumnOption { Name = n, IsVisible = DefaultLocationColumns.Contains(n) }));
            OnPropertyChanged(nameof(LocationColumns));
        }

        private void RebuildReviewColumns(bool indoorLocation)
        {
            var excluded = indoorLocation ? ReviewOutdoorOnlyColumns : ReviewIndoorOnlyColumns;
            ReviewColumns = new ObservableCollection<ColumnOption>(
                typeof(Review).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => p.Name)
                    .Where(n => !excluded.Contains(n))
                    .Select(n => new ColumnOption { Name = n, IsVisible = DefaultReviewColumns.Contains(n) }));
            OnPropertyChanged(nameof(ReviewColumns));
        }

        // ── Domain methods ────────────────────────────────────────────────────────
        public void LoadReviews(int locationId)
        {
            var reviews = _reviewRepository.GetByLocationId(locationId).ToList();
            ReviewList.SetItems(reviews);
        }

        public IEnumerable<Review> LoadReviewsWithFilter(Dictionary<string, object> filters)
        {
            return _reviewRepository.GetByFilter(filters);
        }

        public void AddReview(Review review)
        {
            _reviewRepository.Add(review);

            var reviews = _reviewRepository.GetByLocationId(review.LocationId).ToList();

            var location = _locationRepository.GetById(review.LocationId);
            if (location != null)
            {
                location.Rating = reviews.Average(r => r.Rating);

                var noisyReviews = reviews.Where(r => r.NoiseLevel > 0).ToList();
                if (noisyReviews.Any())
                    location.NoiseLevel = (int)Math.Round(noisyReviews.Average(r => r.NoiseLevel));

                var wifiReviews = reviews.Where(r => r.WifiStrength > 0).ToList();
                if (wifiReviews.Any())
                    location.WifiStrength = (int)Math.Round(wifiReviews.Average(r => r.WifiStrength));

                if (location is IndoorLocation indoor)
                {
                    var comfortReviews = reviews.Where(r => r.ComfortLevel > 0).ToList();
                    if (comfortReviews.Any())
                        indoor.ComfortLevel = (int)Math.Round(comfortReviews.Average(r => r.ComfortLevel));

                    var priceReviews = reviews.Where(r => r.PriceLevel > 0).ToList();
                    if (priceReviews.Any())
                        indoor.PriceLevel = (int)Math.Round(priceReviews.Average(r => r.PriceLevel));
                }
                else if (location is OutdoorLocation outdoor)
                {
                    var cleanlinessReviews = reviews.Where(r => r.Cleanliness > 0).ToList();
                    if (cleanlinessReviews.Any())
                        outdoor.Cleanliness = (int)Math.Round(cleanlinessReviews.Average(r => r.Cleanliness));

                    var crowdednessReviews = reviews.Where(r => r.Crowdedness > 0).ToList();
                    if (crowdednessReviews.Any())
                        outdoor.Crowdedness = (int)Math.Round(crowdednessReviews.Average(r => r.Crowdedness));
                }

                _locationRepository.Update(location);
            }
        }

        public void SetInactive(int id) => _locationRepository.SetInactive(id);

        public void AddLocation(Location location) => _locationRepository.Add(location);

        public bool LastSearchWasIndoor { get; private set; }

        public void FindClosestLocations(bool indoorOnly)
        {
            LastSearchWasIndoor = indoorOnly;
            var filters = indoorOnly
                ? IndoorFilter.GetActiveFilters()
                : OutdoorFilter.GetActiveFilters();

            filters["LocationType"] = indoorOnly ? "Indoor" : "Outdoor";

            if (!filters.ContainsKey("IsActive"))
                filters["IsActive"] = true;

            var locations = _locationRepository.GetByFilter(filters)
                .OrderBy(l => CalculateDistance(l.Latitude, l.Longitude))
                .Take(ResultCount)
                .ToList();

            LocationList.SetItems(locations);
        }

        private double CalculateDistance(double lat, double lon)
        {
            const double R = 6371;
            var dLat = ToRad(lat - UserLatitude);
            var dLon = ToRad(lon - UserLongitude);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(UserLatitude)) * Math.Cos(ToRad(lat)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private double ToRad(double deg) => deg * Math.PI / 180;

        public IEnumerable<Review> GetFilteredReviews()
        {
            IEnumerable<Review> reviews = ReviewList.Items;
            var filters = ReviewFilter.GetActiveFilters();

            foreach (var f in filters)
            {
                var colName = f.Key.Replace("_min", "").Replace("_max", "");
                var prop    = typeof(Review).GetProperty(colName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null) continue;

                if (f.Key.EndsWith("_min"))
                    reviews = reviews.Where(r => Convert.ToDouble(prop.GetValue(r)) >= Convert.ToDouble(f.Value));
                else if (f.Key.EndsWith("_max"))
                    reviews = reviews.Where(r => Convert.ToDouble(prop.GetValue(r)) <= Convert.ToDouble(f.Value));
                else
                    reviews = reviews.Where(r => prop.GetValue(r)?.ToString() == f.Value?.ToString());
            }

            return reviews;
        }
    }
}
