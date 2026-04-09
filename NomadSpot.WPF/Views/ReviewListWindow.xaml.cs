using NomadSpot.Model.Entities;
using NomadSpot.ViewModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NomadSpot.WPF.Views
{
    public partial class ReviewListWindow : Window
    {
        private readonly LocationViewModel _viewModel;
        private readonly string[] _defaultColumns = { "Author", "Rating", "Date", "Comment" };
        private readonly string _locationType;

        private static readonly string[] IndoorOnlyColumns = { "ComfortLevel", "PriceLevel" };
        private static readonly string[] OutdoorOnlyColumns = { "Cleanliness", "Crowdedness" };

        public ReviewListWindow(LocationViewModel viewModel, string locationName, string locationType)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _locationType = locationType;
            TitleBlock.Text = $"Reviews for: {locationName}";
            BuildColumnCheckboxes();
            UpdateColumns();
            RefreshGrid();
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

            var excluded = _locationType == "Indoor" ? OutdoorOnlyColumns : IndoorOnlyColumns;
            foreach (var col in _viewModel.ReviewList.GetAllColumnNames().Where(c => !excluded.Contains(c)))
            {
                var cb = new CheckBox
                {
                    Content = col,
                    IsChecked = _defaultColumns.Contains(col),
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                cb.Checked += (s, e) => { UpdateColumns(); RefreshGrid(); };
                cb.Unchecked += (s, e) => { UpdateColumns(); RefreshGrid(); };
                ColumnsPanel.Children.Add(cb);
            }
        }

        private void UpdateColumns()
        {
            if (ReviewsGrid == null) return;

            var columns = ColumnsPanel.Children
                .OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => cb.Content.ToString())
                .ToList();

            _viewModel.ReviewList.SetColumns(columns);

            ReviewsGrid.Columns.Clear();
            foreach (var col in columns)
                ReviewsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = col,
                    Binding = new System.Windows.Data.Binding(col),
                    Width = col == "Comment"
                        ? new DataGridLength(1, DataGridLengthUnitType.Star)
                        : DataGridLength.Auto
                });
        }

        private void RefreshGrid()
        {
            ReviewsGrid.ItemsSource = null;
            ReviewsGrid.ItemsSource = _viewModel.ReviewList.Items;
        }

        private void ReviewsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReviewsGrid.SelectedItem is Review review)
                _viewModel.ReviewList.SelectedItem = review;
        }

        private void FilterReviews_Click(object sender, RoutedEventArgs e)
        {
            var filterWindow = new FilterWindow(_viewModel.ReviewFilter, () =>
            {
                _viewModel.FindReviewsByFilter();
                RefreshGrid();
            });
            filterWindow.Show();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ReviewList.Clear();
            RefreshGrid();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
