using System.Windows;

namespace NomadSpot.WPF.Views
{
    public partial class ReviewListWindow : Window
    {
        public ReviewListWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
