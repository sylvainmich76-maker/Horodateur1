using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Horodateur.ViewModels;

namespace Horodateur.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); // Cette ligne est CRITIQUE
            DataContext = new MainViewModel();
        }
    }
}