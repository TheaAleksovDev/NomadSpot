using System;
using System.Collections.Generic;
using System.Reflection;

namespace NomadSpot.ViewModel
{
    public class FilterProperty
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Type PropertyType { get; set; }
        public object Value { get; set; }
    }

    public class FilterViewModel<T> : BaseViewModel
    {
        private List<FilterProperty> _filterProperties;

        public List<FilterProperty> FilterProperties
        {
            get => _filterProperties;
            set => SetProperty(ref _filterProperties, value);
        }

        public FilterViewModel()
        {
            GenerateFilters();
        }

        private void GenerateFilters()
        {
            FilterProperties = new List<FilterProperty>();

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                FilterProperties.Add(new FilterProperty
                {
                    Name = prop.Name,
                    DisplayName = FormatDisplayName(prop.Name),
                    PropertyType = prop.PropertyType,
                    Value = null
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
                if (prop.Value != null)
                    filters[prop.Name] = prop.Value;
            }
            return filters;
        }

        public void ClearFilters()
        {
            foreach (var prop in FilterProperties)
                prop.Value = null;
        }
    }
}
