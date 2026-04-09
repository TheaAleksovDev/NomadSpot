using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NomadSpot.ViewModel
{
    public class ListViewModel<T> : BaseViewModel
    {
        private IEnumerable<T> _items;
        private T _selectedItem;
        private IEnumerable<string> _visibleColumns;

        public IEnumerable<T> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public T SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public IEnumerable<string> VisibleColumns
        {
            get => _visibleColumns;
            set => SetProperty(ref _visibleColumns, value);
        }

        public void SetItems(IEnumerable<T> items)
        {
            Items = items;
            SelectedItem = default;
        }

        public void SetColumns(IEnumerable<string> columns)
        {
            VisibleColumns = columns;
        }

        public void Clear()
        {
            Items = null;
            SelectedItem = default;
            VisibleColumns = null;
        }

        public IEnumerable<string> GetAllColumnNames()
        {
            var type = Items?.FirstOrDefault()?.GetType() ?? typeof(T);
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Select(p => p.Name);
        }

        public Dictionary<string, object> GetSelectedItemProperties()
        {
            if (SelectedItem == null) return null;

            var result = new Dictionary<string, object>();
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                result[prop.Name] = prop.GetValue(SelectedItem);
            }
            return result;
        }
    }
}
