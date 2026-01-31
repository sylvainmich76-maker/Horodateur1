using System;
using System.Windows;
using Horodateur.ViewModels;

namespace Horodateur.Views
{
    public partial class ParametresWindow : Window
    {
        public ParametresWindow()
        {
            InitializeComponent();

            var viewModel = new ParametresViewModel();
            DataContext = viewModel;

            // S'abonner à l'événement de fermeture
            viewModel.FermetureDemandee += ViewModel_FermetureDemandee;

            Loaded += ParametresWindow_Loaded;
        }

        private void ParametresWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Synchroniser le PasswordBox avec le ViewModel
            var viewModel = DataContext as ParametresViewModel;
            if (viewModel != null && !string.IsNullOrEmpty(viewModel.MotDePasse))
            {
                PasswordBox.Password = viewModel.MotDePasse;
            }

            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ParametresViewModel;
            if (viewModel != null)
            {
                viewModel.MotDePasse = PasswordBox.Password;
            }
        }

        private void ViewModel_FermetureDemandee()
        {
            // Fermer la fenêtre quand le ViewModel le demande
            Dispatcher.Invoke(() =>
            {
                // Pour une fenêtre ouverte avec ShowDialog(), on peut utiliser DialogResult
                // Mais seulement si on a besoin de retourner un résultat
                // this.DialogResult = true; // Optionnel
                this.Close();
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            // Se désabonner de l'événement
            var viewModel = DataContext as ParametresViewModel;
            if (viewModel != null)
            {
                viewModel.FermetureDemandee -= ViewModel_FermetureDemandee;
            }

            PasswordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            base.OnClosed(e);
        }
    }
}