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
                else if ((propType == typeof(int) || propType == typeof(double) ||
                          propType == typeof(int?) || propType == typeof(double?)) && p.IsSlider)
                {
                    var sliderPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    var valueLabel = new TextBlock
                    {
                        Text = "(Any)",
                        Width = 40,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(8, 0, 0, 0)
                    };
                    var slider = new Slider
                    {
                        Minimum = p.Min.Value,
                        Maximum = p.Max.Value,
                        TickFrequency = 1,
                        IsSnapToTickEnabled = true,
                        Width = 180,
                        Value = p.Min.Value,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    var clearBtn = new Button
                    {
                        Content = "Any",
                        Margin = new Thickness(8, 0, 0, 0),
                        Padding = new Thickness(6, 2, 6, 2)
                    };
                    slider.ValueChanged += (s, e) =>
                    {
                        p.Value = slider.Value;
                        valueLabel.Text = slider.Value.ToString("0");
                    };
                    clearBtn.Click += (s, e) =>
                    {
                        p.Value = null;
                        valueLabel.Text = "(Any)";
                    };
                    sliderPanel.Children.Add(slider);
                    sliderPanel.Children.Add(valueLabel);
                    sliderPanel.Children.Add(clearBtn);
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
