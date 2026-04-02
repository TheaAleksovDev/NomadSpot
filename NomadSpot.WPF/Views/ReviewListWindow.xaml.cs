using NomadSpot.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NomadSpot.WPF.Views
{
    public partial class ReviewListWindow : Window
    {
        private readonly LocationViewModel _viewModel;

        public ReviewListWindow(LocationViewModel viewModel, string locationName)
        {
            InitializeComponent();
            _viewModel = viewModel;
            TitleBlock.Text = $"Reviews for: {locationName}";
            UpdateColumns();
            RefreshGrid();
        }

        private void UpdateColumns()
        {
            var columns = new List<string>();
            if (ColAuthor.IsChecked == true)  columns.Add("Author");
            if (ColRating.IsChecked == true)  columns.Add("Rating");
            if (ColDate.IsChecked == true)    columns.Add("Date");
            if (ColComment.IsChecked == true) columns.Add("Comment");

            _viewModel.ReviewList.SetColumns(columns);

            ReviewsGrid.Columns.Clear();
            foreach (var col in columns)
            {
                ReviewsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = col,
                    Binding = new System.Windows.Data.Binding(col),
                    Width = col == "Comment" ? new DataGridLength(1, DataGridLengthUnitType.Star) : DataGridLength.Auto
                });
            }
        }

        private void RefreshGrid()
        {
            ReviewsGrid.ItemsSource = null;
            ReviewsGrid.ItemsSource = _viewModel.ReviewList.Items;
        }

        private void Columns_Changed(object sender, RoutedEventArgs e)
        {
            UpdateColumns();
            RefreshGrid();
        }

        private void ReviewsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReviewsGrid.SelectedItem is NomadSpot.Model.Entities.Review review)
                _viewModel.ReviewList.SelectedItem = review;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ReviewList.Clear();
            RefreshGrid();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
