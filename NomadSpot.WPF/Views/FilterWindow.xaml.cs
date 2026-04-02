using NomadSpot.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NomadSpot.WPF.Views
{
    public partial class FilterWindow : Window
    {
        private readonly IFilterViewModel _filterViewModel;
        private readonly Action _onSearch;

        public FilterWindow(IFilterViewModel filterViewModel, Action onSearch)
        {
            InitializeComponent();
            _filterViewModel = filterViewModel;
            _onSearch = onSearch;
            DataContext = filterViewModel;
            BuildFilterControls();
        }

        private void BuildFilterControls()
        {
            FiltersPanel.ItemsSource = null;
            FiltersPanel.ItemsSource = _filterViewModel.FilterProperties;

            var panel = new StackPanel();

            foreach (var prop in _filterViewModel.FilterProperties)
            {
                var container = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
                var label = new TextBlock
                {
                    Text = prop.DisplayName,
                    FontWeight = FontWeights.SemiBold
                };
                container.Children.Add(label);

                Control control;
                Type propType = prop.PropertyType;
                var p = prop;

                if (propType == typeof(bool) || propType == typeof(bool?))
                {
                    var cb = new CheckBox { IsThreeState = true };
                    cb.Checked += (s, e) => p.Value = true;
                    cb.Unchecked += (s, e) => p.Value = false;
                    cb.Indeterminate += (s, e) => p.Value = null;
                    control = cb;
                }
                else if (propType == typeof(int) || propType == typeof(double) ||
                         propType == typeof(int?) || propType == typeof(double?))
                {
                    var tb = new TextBox { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
                    tb.TextChanged += (s, e) =>
                    {
                        if (double.TryParse(tb.Text, out double val))
                            p.Value = val;
                        else
                            p.Value = null;
                    };
                    control = tb;
                }
                else if (propType == typeof(DateTime) || propType == typeof(DateTime?))
                {
                    var dp = new DatePicker { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
                    dp.SelectedDateChanged += (s, e) => p.Value = dp.SelectedDate;
                    control = dp;
                }
                else
                {
                    var tb = new TextBox { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
                    tb.TextChanged += (s, e) => p.Value = tb.Text;
                    control = tb;
                }

                container.Children.Add(control);
                panel.Children.Add(container);
            }

            var scroll = (ScrollViewer)((Grid)Content).Children[1];
            scroll.Content = panel;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            _onSearch();
            Close();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _filterViewModel.ClearFilters();
            BuildFilterControls();
        }
    }
}
