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
            LocationsGrid.Columns.Clear();

            if (ColName.IsChecked == true)
                LocationsGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
            if (ColAddress.IsChecked == true)
                LocationsGrid.Columns.Add(new DataGridTextColumn { Header = "Address", Binding = new System.Windows.Data.Binding("Address") });
            if (ColRating.IsChecked == true)
                LocationsGrid.Columns.Add(new DataGridTextColumn { Header = "Rating", Binding = new System.Windows.Data.Binding("Rating") });
            if (ColNoise.IsChecked == true)
                LocationsGrid.Columns.Add(new DataGridTextColumn { Header = "Noise Level", Binding = new System.Windows.Data.Binding("NoiseLevel") });
            if (ColWifi.IsChecked == true)
                LocationsGrid.Columns.Add(new DataGridTextColumn { Header = "WiFi", Binding = new System.Windows.Data.Binding("HasWifi") });
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