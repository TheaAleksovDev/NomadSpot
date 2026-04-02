using NomadSpot.Model.Entities;
using NomadSpot.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NomadSpot.WPF.Views
{
    public partial class LocationListWindow : Window
    {
        private readonly LocationViewModel _viewModel;

        public LocationListWindow(LocationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            UpdateColumns();
            LocationsGrid.ItemsSource = _viewModel.LocationList.Items;
        }

        private void Column_Changed(object sender, RoutedEventArgs e)
        {
            UpdateColumns();
        }

        private void UpdateColumns()
        {
            if (LocationsGrid == null) return;

            var columns = new List<string>();
            if (ColName.IsChecked == true)    columns.Add("Name");
            if (ColAddress.IsChecked == true) columns.Add("Address");
            if (ColRating.IsChecked == true)  columns.Add("Rating");
            if (ColNoise.IsChecked == true)   columns.Add("NoiseLevel");
            if (ColWifi.IsChecked == true)    columns.Add("HasWifi");

            _viewModel.LocationList.SetColumns(columns);

            var bindings = new Dictionary<string, (string header, string path)>
            {
                ["Name"]      = ("Name",        "Name"),
                ["Address"]   = ("Address",     "Address"),
                ["Rating"]    = ("Rating",      "Rating"),
                ["NoiseLevel"]= ("Noise Level", "NoiseLevel"),
                ["HasWifi"]   = ("WiFi",        "HasWifi"),
            };

            LocationsGrid.Columns.Clear();
            foreach (var col in _viewModel.LocationList.VisibleColumns ?? columns)
            {
                if (bindings.TryGetValue(col, out var info))
                    LocationsGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header  = info.header,
                        Binding = new System.Windows.Data.Binding(info.path)
                    });
            }
        }

        private void LocationsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocationsGrid.SelectedItem is Location selected)
                _viewModel.LocationList.SelectedItem = selected;
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

            var window = new ReviewListWindow(_viewModel, location.Name);
            window.Show();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LocationList.Clear();
            LocationsGrid.ItemsSource = null;
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
                MessageBox.Show("Location marked as inactive.");
            }
        }
    }
}