using NomadSpot.Model.Entities;
using NomadSpot.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NomadSpot.WPF.Views
{
    public partial class AddReviewWindow : Window
    {
        private readonly LocationViewModel _viewModel;
        private readonly bool _isIndoor;

        public AddReviewWindow(LocationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _isIndoor = _viewModel.LocationList.SelectedItem?.LocationType == "Indoor";
            IndoorPanel.Visibility = _isIndoor ? Visibility.Visible : Visibility.Collapsed;
            OutdoorPanel.Visibility = _isIndoor ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RatingBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RatingLabel != null) RatingLabel.Text = $"Rating: {(int)RatingBox.Value}";
        }

        private void NoiseLevelBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (NoiseLevelLabel != null) NoiseLevelLabel.Text = $"Noise Level: {(int)NoiseLevelBox.Value}";
        }

        private void WifiStrengthBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WifiStrengthLabel != null) WifiStrengthLabel.Text = $"WiFi Strength: {(int)WifiStrengthBox.Value}  (0 = No WiFi)";
        }

        private void ComfortLevelBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ComfortLevelLabel != null) ComfortLevelLabel.Text = $"Comfort Level: {(int)ComfortLevelBox.Value}";
        }

        private void PriceLevelBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PriceLevelLabel != null) PriceLevelLabel.Text = $"Price Level: {(int)PriceLevelBox.Value}";
        }

        private void CleanlinessBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CleanlinessLabel != null) CleanlinessLabel.Text = $"Cleanliness: {(int)CleanlinessBox.Value}";
        }

        private void CrowdednessBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CrowdednessLabel != null) CrowdednessLabel.Text = $"Crowdedness: {(int)CrowdednessBox.Value}";
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AuthorBox.Text))
            {
                MessageBox.Show("Please enter an author name.");
                return;
            }

            var review = new Review
            {
                LocationId = _viewModel.LocationList.SelectedItem.Id,
                Author = AuthorBox.Text,
                Comment = CommentBox.Text,
                Rating = (int)RatingBox.Value,
                NoiseLevel = (int)NoiseLevelBox.Value,
                WifiStrength = (int)WifiStrengthBox.Value,
                ComfortLevel = _isIndoor ? (int)ComfortLevelBox.Value : 0,
                PriceLevel = _isIndoor ? (int)PriceLevelBox.Value : 0,
                Cleanliness = _isIndoor ? 0 : (int)CleanlinessBox.Value,
                Crowdedness = _isIndoor ? 0 : (int)CrowdednessBox.Value,
                Date = DateTime.Now
            };

            _viewModel.AddReview(review);
            MessageBox.Show("Review added successfully!");
            Close();
        }
    }
}
