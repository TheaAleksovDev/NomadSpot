using NomadSpot.Model.Entities;
using NomadSpot.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace NomadSpot.WPF.Views
{
    public partial class LocationListWindow : Window
    {
        private readonly LocationViewModel _viewModel;
        private readonly string[] _defaultColumns = { "Name", "Address", "Rating", "LocationType" };
        private static readonly string[] IndoorOnlyColumns = { "ComfortLevel", "PriceLevel", "OpeningHours", "IndoorType" };
        private static readonly string[] OutdoorOnlyColumns = { "HasBenches", "HasShade", "PetFriendly", "HasPublicToilet", "NearShops", "OutdoorType" };

        public LocationListWindow(LocationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            BuildColumnCheckboxes();
            UpdateColumns();
            LocationsGrid.ItemsSource = _viewModel.LocationList.Items;
        }

        private void BuildColumnCheckboxes()
        {
            ColumnsPanel.Children.Clear();
            ColumnsPanel.Children.Add(new TextBlock
            {
                Text = "Columns:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            });

            var excluded = _viewModel.LastSearchWasIndoor ? OutdoorOnlyColumns : IndoorOnlyColumns;
            var allColumns = _viewModel.LocationList.GetAllColumnNames()
                .Where(c => !excluded.Contains(c));

            foreach (var col in allColumns)
            {
                var cb = new CheckBox
                {
                    Content = col,
                    IsChecked = _defaultColumns.Contains(col),
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                cb.Checked += (s, e) => UpdateColumns();
                cb.Unchecked += (s, e) => UpdateColumns();
                ColumnsPanel.Children.Add(cb);
            }
        }

        private void UpdateColumns()
        {
            if (LocationsGrid == null) return;

            var columns = ColumnsPanel.Children
                .OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => cb.Content.ToString())
                .ToList();

            _viewModel.LocationList.SetColumns(columns);

            LocationsGrid.Columns.Clear();
            foreach (var col in columns)
                LocationsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = col,
                    Binding = new System.Windows.Data.Binding(col)
                });
        }

        private void LocationsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocationsGrid.SelectedItem is Location selected)
            {
                _viewModel.LocationList.SelectedItem = selected;
                ShowDetail(selected);
            }
        }

        private void ShowDetail(Location loc)
        {
            DetailTitle.Text = loc.Name;
            var props = loc.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(loc) ?? "-"))
                .ToList();
            DetailItems.ItemsSource = props;
            DetailPanel.Visibility = Visibility.Visible;
        }

        private void CloseDetail_Click(object sender, RoutedEventArgs e)
        {
            DetailPanel.Visibility = Visibility.Collapsed;
        }

        private void AddReview_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.LocationList.SelectedItem == null)
            {
                MessageBox.Show("Please select a location first.");
                return;
            }
            var window = new AddReviewWindow(_viewModel);
            window.Closed += (s, args) =>
            {
                _viewModel.FindClosestLocations(_viewModel.LastSearchWasIndoor);
                LocationsGrid.ItemsSource = null;
                LocationsGrid.ItemsSource = _viewModel.LocationList.Items;
            };
            window.Show();
        }

        private void ViewReviews_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.LocationList.SelectedItem == null)
            {
                MessageBox.Show("Please select a location first.");
                return;
            }

            var location = _viewModel.LocationList.SelectedItem;
            _viewModel.LoadReviews(location.Id);

            var window = new ReviewListWindow(_viewModel, location.Name, location.LocationType);
            window.Show();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LocationList.Clear();
            LocationsGrid.ItemsSource = null;
            DetailPanel.Visibility = Visibility.Collapsed;
        }

        private void MarkInactive_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.LocationList.SelectedItem == null)
            {
                MessageBox.Show("Please select a location first.");
                return;
            }

            var result = MessageBox.Show(
                $"Mark '{_viewModel.LocationList.SelectedItem.Name}' as inactive?",
                "Confirm", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.SetInactive(_viewModel.LocationList.SelectedItem.Id);
                _viewModel.FindClosestLocations(_viewModel.LastSearchWasIndoor);
                LocationsGrid.ItemsSource = null;
                LocationsGrid.ItemsSource = _viewModel.LocationList.Items;
                DetailPanel.Visibility = Visibility.Collapsed;
                MessageBox.Show("Location marked as inactive.");
            }
        }
    }
}
