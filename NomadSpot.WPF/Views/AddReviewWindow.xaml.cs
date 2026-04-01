using NomadSpot.Model.Entities;
using NomadSpot.ViewModel;
using System;
using System.Windows;

namespace NomadSpot.WPF.Views
{
    public partial class AddReviewWindow : Window
    {
        private readonly LocationViewModel _viewModel;

        public AddReviewWindow(LocationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AuthorBox.Text) ||
                !int.TryParse(RatingBox.Text, out int rating) ||
                rating < 1 || rating > 5)
            {
                MessageBox.Show("Please fill in all fields correctly. Rating must be 1-5.");
                return;
            }

            var review = new Review
            {
                LocationId = _viewModel.LocationList.SelectedItem.Id,
                Author = AuthorBox.Text,
                Comment = CommentBox.Text,
                Rating = rating,
                Date = DateTime.Now
            };

            _viewModel.AddReview(review);
            MessageBox.Show("Review added successfully!");
            Close();
        }
    }
}
