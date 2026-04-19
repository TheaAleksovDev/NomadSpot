using NomadSpot.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NomadSpot.WPF
{
    public static class DataGridColumnsBehavior
    {
        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.RegisterAttached(
                "ColumnsSource",
                typeof(ObservableCollection<ColumnOption>),
                typeof(DataGridColumnsBehavior),
                new PropertyMetadata(null, OnColumnsSourceChanged));

        public static ObservableCollection<ColumnOption> GetColumnsSource(DependencyObject obj) =>
            (ObservableCollection<ColumnOption>)obj.GetValue(ColumnsSourceProperty);

        public static void SetColumnsSource(DependencyObject obj, ObservableCollection<ColumnOption> value) =>
            obj.SetValue(ColumnsSourceProperty, value);

        private static void OnColumnsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid) return;
            if (e.NewValue is not ObservableCollection<ColumnOption> columns) return;

            foreach (var col in columns)
                col.PropertyChanged += (s, ev) =>
                {
                    if (ev.PropertyName == nameof(ColumnOption.IsVisible))
                        RebuildColumns(grid, (ObservableCollection<ColumnOption>)grid.GetValue(ColumnsSourceProperty));
                };

            RebuildColumns(grid, columns);
        }

        private static void RebuildColumns(DataGrid grid, ObservableCollection<ColumnOption> columns)
        {
            if (columns == null) return;
            grid.Columns.Clear();
            foreach (var col in columns)
            {
                if (col.IsVisible)
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header  = col.Name,
                        Binding = new Binding(col.Name)
                    });
            }
        }
    }
}
