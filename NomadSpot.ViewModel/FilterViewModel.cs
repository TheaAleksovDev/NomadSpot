using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;

namespace NomadSpot.ViewModel
{
    public class FilterProperty : BaseViewModel
    {
        public string   Name         { get; set; }
        public string   DisplayName  { get; set; }
        public Type     PropertyType { get; set; }
        public string[] EnumValues   { get; set; }
        public double?  Min          { get; set; }
        public double?  Max          { get; set; }
        public bool     IsSlider     => Min.HasValue && Max.HasValue;

        private object  _value;
        private double? _minValue;
        private double? _maxValue;

        public object  Value    { get => _value;    set => SetProperty(ref _value,    value); }
        public double? MinValue { get => _minValue; set => SetProperty(ref _minValue, value); }
        public double? MaxValue { get => _maxValue; set => SetProperty(ref _maxValue, value); }
    }

    public class FilterViewModel<T> : BaseViewModel, IFilterViewModel
    {
        private static readonly HashSet<string> _skippedProperties = new()
        {
            "Id", "Name", "Address", "Latitude", "Longitude",
            "LocationType", "IsActive", "OpeningHours", "LocationId",
            "Author", "Comment"
        };

        private static readonly Dictionary<string, (double Min, double Max)> _sliderRanges = new()
        {
            { "Rating",       (1, 5) },
            { "NoiseLevel",   (1, 5) },
            { "WifiStrength", (0, 5) },
            { "ComfortLevel", (1, 5) },
            { "PriceLevel",   (1, 5) },
            { "Cleanliness",  (1, 5) },
            { "Crowdedness",  (1, 5) },
        };

        public event EventHandler Searched;

        private ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get => _searchCommand;
            set => _searchCommand = new RelayCommand(
                ()  => { value?.Execute(null); Searched?.Invoke(this, EventArgs.Empty); },
                ()  => value?.CanExecute(null) ?? false);
        }

        public ICommand ClearCommand { get; }

        private List<FilterProperty> _filterProperties;

        public List<FilterProperty> FilterProperties
        {
            get => _filterProperties;
            set => SetProperty(ref _filterProperties, value);
        }

        public FilterViewModel()
        {
            ClearCommand = new RelayCommand(ClearFilters);
            GenerateFilters();
        }

        private void GenerateFilters()
        {
            FilterProperties = new List<FilterProperty>();

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                          .Where(p => !_skippedProperties.Contains(p.Name)))
            {
                _sliderRanges.TryGetValue(prop.Name, out var range);
                FilterProperties.Add(new FilterProperty
                {
                    Name = prop.Name,
                    DisplayName = FormatDisplayName(prop.Name),
                    PropertyType = prop.PropertyType,
                    Value = null,
                    EnumValues = prop.PropertyType.IsEnum ? Enum.GetNames(prop.PropertyType) : null,
                    Min = range == default ? null : range.Min,
                    Max = range == default ? null : range.Max,
                    MinValue = range == default ? null : range.Min,
                    MaxValue = range == default ? null : range.Max,
                });
            }
        }

        private string FormatDisplayName(string propertyName)
        {
            var result = "";
            foreach (char c in propertyName)
            {
                if (char.IsUpper(c) && result.Length > 0)
                    result += " ";
                result += c;
            }
            return result;
        }

        public Dictionary<string, object> GetActiveFilters()
        {
            var filters = new Dictionary<string, object>();
            foreach (var prop in FilterProperties)
            {
                if (prop.IsSlider)
                {
                    if (prop.MinValue.HasValue && prop.MinValue != prop.Min)
                        filters[prop.Name + "_min"] = prop.MinValue.Value;
                    if (prop.MaxValue.HasValue && prop.MaxValue != prop.Max)
                        filters[prop.Name + "_max"] = prop.MaxValue.Value;
                }
                else if (prop.Value != null)
                {
                    filters[prop.Name] = prop.Value;
                }
            }
            return filters;
        }

        public void ClearFilters()
        {
            foreach (var prop in FilterProperties)
            {
                prop.Value = null;
                if (prop.IsSlider)
                {
                    prop.MinValue = prop.Min;
                    prop.MaxValue = prop.Max;
                }
            }
        }
    }
}
