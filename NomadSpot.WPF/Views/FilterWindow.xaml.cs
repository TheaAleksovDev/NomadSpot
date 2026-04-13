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
                else if ((propType == typeof(int) || propType == typeof(double) ||
                          propType == typeof(int?) || propType == typeof(double?)) && p.IsSlider)
                {
                    var sliderPanel = new StackPanel();

                    // Min row
                    var minRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                    var minLabel = new TextBlock { Text = "Min:", Width = 30, VerticalAlignment = VerticalAlignment.Center };
                    var minValueLabel = new TextBlock { Text = p.MinValue?.ToString("0") ?? p.Min?.ToString("0"), Width = 25, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
                    var minSlider = new Slider { Minimum = p.Min.Value, Maximum = p.Max.Value, TickFrequency = 1, IsSnapToTickEnabled = true, Width = 160, Value = p.MinValue ?? p.Min.Value };
                    minSlider.ValueChanged += (s, e) => { p.MinValue = minSlider.Value; minValueLabel.Text = minSlider.Value.ToString("0"); };
                    minRow.Children.Add(minLabel);
                    minRow.Children.Add(minSlider);
                    minRow.Children.Add(minValueLabel);

                    // Max row
                    var maxRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                    var maxLabel = new TextBlock { Text = "Max:", Width = 30, VerticalAlignment = VerticalAlignment.Center };
                    var maxValueLabel = new TextBlock { Text = p.MaxValue?.ToString("0") ?? p.Max?.ToString("0"), Width = 25, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
                    var maxSlider = new Slider { Minimum = p.Min.Value, Maximum = p.Max.Value, TickFrequency = 1, IsSnapToTickEnabled = true, Width = 160, Value = p.MaxValue ?? p.Max.Value };
                    maxSlider.ValueChanged += (s, e) => { p.MaxValue = maxSlider.Value; maxValueLabel.Text = maxSlider.Value.ToString("0"); };
                    maxRow.Children.Add(maxLabel);
                    maxRow.Children.Add(maxSlider);
                    maxRow.Children.Add(maxValueLabel);

                    var resetBtn = new Button { Content = "Reset", Padding = new Thickness(6, 2, 6, 2), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 4, 0, 0) };
                    resetBtn.Click += (s, e) =>
                    {
                        p.MinValue = p.Min;
                        p.MaxValue = p.Max;
                        minSlider.Value = p.Min.Value;
                        maxSlider.Value = p.Max.Value;
                    };

                    sliderPanel.Children.Add(minRow);
                    sliderPanel.Children.Add(maxRow);
                    sliderPanel.Children.Add(resetBtn);
                    container.Children.Add(sliderPanel);
                    panel.Children.Add(container);
                    continue;
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
                else if (propType.IsEnum)
                {
                    var combo = new ComboBox { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
                    combo.Items.Add("(Any)");
                    foreach (var name in Enum.GetNames(propType))
                        combo.Items.Add(name);
                    combo.SelectedIndex = 0;
                    combo.SelectionChanged += (s, e) =>
                    {
                        p.Value = combo.SelectedIndex <= 0
                            ? null
                            : (object)(int)Enum.Parse(propType, combo.SelectedItem.ToString());
                    };
                    control = combo;
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
