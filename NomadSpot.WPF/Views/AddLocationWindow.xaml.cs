using NomadSpot.Model.Entities;
using NomadSpot.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NomadSpot.WPF.Views
{
    public partial class AddLocationWindow : Window
    {
        private readonly LocationViewModel _viewModel;

        public AddLocationWindow(LocationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutdoorPanel == null) return;
            if (TypeBox.SelectedItem is ComboBoxItem item)
                OutdoorPanel.Visibility = item.Content.ToString() == "Outdoor"
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                !double.TryParse(LatBox.Text, out double lat) ||
                !double.TryParse(LonBox.Text, out double lon))
            {
                MessageBox.Show("Please fill in all required fields correctly.");
                return;
            }

            bool isIndoor = ((ComboBoxItem)TypeBox.SelectedItem).Content.ToString() == "Indoor";

            Location location;
            if (isIndoor)
            {
                location = new IndoorLocation
                {
                    Name = NameBox.Text,
                    Address = AddressBox.Text,
                    Latitude = lat,
                    Longitude = lon,
                    NoiseLevel = int.TryParse(NoiseBox.Text, out int noise) ? noise : 0,
                    HasWifi = WifiBox.IsChecked == true,
                    HasPowerOutlets = PowerBox.IsChecked == true,
                    LastVerified = DateTime.Now,
                    IsActive = true,
                    LocationType = "Indoor"
                };
            }
            else
            {
                location = new OutdoorLocation
                {
                    Name = NameBox.Text,
                    Address = AddressBox.Text,
                    Latitude = lat,
                    Longitude = lon,
                    NoiseLevel = int.TryParse(NoiseBox.Text, out int noise) ? noise : 0,
                    HasWifi = WifiBox.IsChecked == true,
                    HasPowerOutlets = PowerBox.IsChecked == true,
                    LastVerified = DateTime.Now,
                    IsActive = true,
                    LocationType = "Outdoor",
                    HasBenches = BenchesBox.IsChecked == true,
                    HasShade = ShadeBox.IsChecked == true,
                    PetFriendly = PetBox.IsChecked == true,
                    HasPublicToilet = ToiletBox.IsChecked == true,
                    NearShops = ShopsBox.IsChecked == true
                };
            }

            _viewModel.AddLocation(location);
            MessageBox.Show("Location added successfully!");
            Close();
        }
    }
}
