using System.Windows;
using Client.ViewModels;
using Client.Models;

namespace Client
{
    public partial class MainWindow : Window
    {
        private currencyRate curRate;

        public MainWindow()
        {
            InitializeComponent();

            curRate = new currencyRate();

            DataContext = new CurrecncyRateViewModel(curRate, MainPlot);
        }
    }
}
