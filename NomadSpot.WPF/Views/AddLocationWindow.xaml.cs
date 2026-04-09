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

        private async void UseMyLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var geolocator = new Windows.Devices.Geolocation.Geolocator();
                var position = await geolocator.GetGeopositionAsync();
                LatBox.Text = position.Coordinate.Point.Position.Latitude.ToString("F6");
                LonBox.Text = position.Coordinate.Point.Position.Longitude.ToString("F6");
            }
            catch
            {
                MessageBox.Show("Could not get location. Make sure location access is enabled in Windows Settings.");
            }
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutdoorPanel == null || IndoorPanel == null) return;
            bool isOutdoor = TypeBox.SelectedItem is ComboBoxItem item && item.Content.ToString() == "Outdoor";
            OutdoorPanel.Visibility = isOutdoor ? Visibility.Visible : Visibility.Collapsed;
            IndoorPanel.Visibility = isOutdoor ? Visibility.Collapsed : Visibility.Visible;
        }

        private void NoiseBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (NoiseLevelLabel != null)
                NoiseLevelLabel.Text = $"Noise Level: {(int)NoiseBox.Value}";
        }

        private void WifiBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WifiStrengthLabel != null)
                WifiStrengthLabel.Text = $"WiFi Strength: {(int)WifiBox.Value}  (0 = No WiFi)";
        }

        private void ComfortBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ComfortLevelLabel != null)
                ComfortLevelLabel.Text = $"Comfort Level: {(int)ComfortBox.Value}";
        }

        private void PriceBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PriceLevelLabel != null)
                PriceLevelLabel.Text = $"Price Level: {(int)PriceBox.Value}";
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
                var selectedIndoorType = ((ComboBoxItem)IndoorTypeBox.SelectedItem).Content.ToString();
                location = new IndoorLocation
                {
                    Name = NameBox.Text,
                    Address = AddressBox.Text,
                    Latitude = lat,
                    Longitude = lon,
                    NoiseLevel = (int)NoiseBox.Value,
                    WifiStrength = (int)WifiBox.Value,
                    HasPowerOutlets = PowerBox.IsChecked == true,
                    IsActive = true,
                    LocationType = "Indoor",
                    ComfortLevel = (int)ComfortBox.Value,
                    PriceLevel = (int)PriceBox.Value,
                    OpeningHours = HoursBox.Text,
                    IndoorType = Enum.Parse<IndoorType>(selectedIndoorType)
                };
            }
            else
            {
                var selectedOutdoorType = ((ComboBoxItem)OutdoorTypeBox.SelectedItem).Content.ToString();
                location = new OutdoorLocation
                {
                    Name = NameBox.Text,
                    Address = AddressBox.Text,
                    Latitude = lat,
                    Longitude = lon,
                    NoiseLevel = (int)NoiseBox.Value,
                    WifiStrength = (int)WifiBox.Value,
                    HasPowerOutlets = PowerBox.IsChecked == true,
                    IsActive = true,
                    LocationType = "Outdoor",
                    HasBenches = BenchesBox.IsChecked == true,
                    HasShade = ShadeBox.IsChecked == true,
                    PetFriendly = PetBox.IsChecked == true,
                    HasPublicToilet = ToiletBox.IsChecked == true,
                    NearShops = ShopsBox.IsChecked == true,
                    OutdoorType = Enum.Parse<OutdoorType>(selectedOutdoorType)
                };
            }

            _viewModel.AddLocation(location);
            MessageBox.Show("Location added successfully!");
            Close();
        }
    }
}
