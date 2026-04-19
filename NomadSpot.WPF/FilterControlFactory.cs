using NomadSpot.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NomadSpot.WPF
{
    public class FilterControlFactory : IFilterControlFactory
    {
        public FrameworkElement Build(IEnumerable<FilterProperty> properties)
        {
            var panel = new StackPanel();

            foreach (var prop in properties)
            {
                var container = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
                container.Children.Add(new TextBlock { Text = prop.DisplayName, FontWeight = FontWeights.SemiBold });
                container.Children.Add(Create(prop));
                panel.Children.Add(container);
            }

            return panel;
        }

        private FrameworkElement Create(FilterProperty p)
        {
            Type propType = p.PropertyType;

            if (propType == typeof(bool) || propType == typeof(bool?))
            {
                var cb = new CheckBox { IsThreeState = true };
                cb.SetBinding(CheckBox.IsCheckedProperty,
                    new Binding("Value") { Source = p, Mode = BindingMode.TwoWay });
                return cb;
            }

            if (p.IsSlider)
                return BuildSliderPanel(p);

            if (propType == typeof(int)  || propType == typeof(double) ||
                propType == typeof(int?) || propType == typeof(double?))
            {
                var tb = new TextBox { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
                tb.SetBinding(TextBox.TextProperty,
                    new Binding("Value") { Source = p, Mode = BindingMode.TwoWay, TargetNullValue = "" });
                return tb;
            }

            if (propType.IsEnum)
            {
                var combo = new ComboBox { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
                combo.Items.Add("(Any)");
                foreach (var name in Enum.GetNames(propType))
                    combo.Items.Add(name);
                combo.SelectedIndex = 0;
                combo.SelectionChanged += (s, e) =>
                    p.Value = combo.SelectedIndex <= 0
                        ? null
                        : (object)(int)Enum.Parse(propType, combo.SelectedItem.ToString());
                return combo;
            }

            if (propType == typeof(DateTime) || propType == typeof(DateTime?))
            {
                var dp = new DatePicker { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
                dp.SetBinding(DatePicker.SelectedDateProperty,
                    new Binding("Value") { Source = p, Mode = BindingMode.TwoWay });
                return dp;
            }

            var textBox = new TextBox { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
            textBox.SetBinding(TextBox.TextProperty,
                new Binding("Value") { Source = p, Mode = BindingMode.TwoWay, TargetNullValue = "" });
            return textBox;
        }

        private static StackPanel BuildSliderPanel(FilterProperty p)
        {
            var panel = new StackPanel();
            panel.Children.Add(BuildSliderRow("Min:", p, isMin: true));
            panel.Children.Add(BuildSliderRow("Max:", p, isMin: false));

            var resetBtn = new Button
            {
                Content             = "Reset",
                Padding             = new Thickness(6, 2, 6, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin              = new Thickness(0, 4, 0, 0)
            };
            resetBtn.Click += (s, e) => { p.MinValue = p.Min; p.MaxValue = p.Max; };
            panel.Children.Add(resetBtn);
            return panel;
        }

        private static StackPanel BuildSliderRow(string labelText, FilterProperty p, bool isMin)
        {
            var bindingPath = isMin ? "MinValue" : "MaxValue";
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };

            row.Children.Add(new TextBlock { Text = labelText, Width = 30, VerticalAlignment = VerticalAlignment.Center });

            var slider = new Slider
            {
                Minimum = p.Min.Value, Maximum = p.Max.Value,
                TickFrequency = 1, IsSnapToTickEnabled = true, Width = 160
            };
            slider.SetBinding(Slider.ValueProperty,
                new Binding(bindingPath) { Source = p, Mode = BindingMode.TwoWay });
            row.Children.Add(slider);

            var valueLabel = new TextBlock { Width = 25, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            valueLabel.SetBinding(TextBlock.TextProperty,
                new Binding(bindingPath) { Source = p, StringFormat = "0" });
            row.Children.Add(valueLabel);

            return row;
        }
    }
}
