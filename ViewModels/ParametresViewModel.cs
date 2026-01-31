using System;
using System.Windows;
using System.Windows.Input;
using Horodateur.Models;
using Horodateur.Services;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Horodateur.ViewModels
{
    public class ParametresViewModel : INotifyPropertyChanged
    {
        private readonly ParametresService _parametresService;
        private Parametres _parametresOriginaux;

        private Parametres _parametresActuels;
        public Parametres ParametresActuels
        {
            get => _parametresActuels;
            set
            {
                if (_parametresActuels != value)
                {
                    _parametresActuels = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        // Propriété pour le mot de passe
        private string _motDePasse;
        public string MotDePasse
        {
            get => _motDePasse;
            set
            {
                if (_motDePasse != value)
                {
                    _motDePasse = value;
                    ParametresActuels.SmtpPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand EnregistrerCommand { get; }
        public ICommand AnnulerCommand { get; }
        public ICommand ParcourirCommand { get; }

        // Événement pour notifier que la fenêtre peut se fermer
        public event Action FermetureDemandee;

        public event PropertyChangedEventHandler PropertyChanged;

        public ParametresViewModel()
        {
            _parametresService = new ParametresService();

            // Charger les paramètres actuels
            ParametresActuels = _parametresService.ChargerParametres();

            // Initialiser le mot de passe
            MotDePasse = ParametresActuels.SmtpPassword;

            // Sauvegarder une copie pour annuler
            _parametresOriginaux = CloneParametres(ParametresActuels);

            EnregistrerCommand = new RelayCommand(Enregistrer, CanEnregistrer);
            AnnulerCommand = new RelayCommand(Annuler);
            ParcourirCommand = new RelayCommand(Parcourir);

            StatusMessage = "";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Parametres CloneParametres(Parametres source)
        {
            return new Parametres
            {
                SmtpServer = source.SmtpServer,
                SmtpPort = source.SmtpPort,
                SmtpUsername = source.SmtpUsername,
                SmtpPassword = source.SmtpPassword,
                EmailDestinataire = source.EmailDestinataire,
                CheminArchivage = source.CheminArchivage
            };
        }

        private bool CanEnregistrer()
        {
            // Validation de base
            return !string.IsNullOrWhiteSpace(ParametresActuels.SmtpServer) &&
                   ParametresActuels.SmtpPort > 0 &&
                   ParametresActuels.SmtpPort <= 65535 &&
                   !string.IsNullOrWhiteSpace(ParametresActuels.SmtpUsername) &&
                   !string.IsNullOrWhiteSpace(ParametresActuels.EmailDestinataire) &&
                   !string.IsNullOrWhiteSpace(ParametresActuels.CheminArchivage);
        }

        private void Enregistrer()
        {
            try
            {
                // Validation supplémentaire
                if (!IsValidEmail(ParametresActuels.EmailDestinataire))
                {
                    StatusMessage = "Adresse email destinataire invalide";
                    MessageBox.Show("L'adresse email destinataire n'est pas valide.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!IsValidEmail(ParametresActuels.SmtpUsername))
                {
                    StatusMessage = "Adresse email expéditeur invalide";
                    MessageBox.Show("L'adresse email expéditeur n'est pas valide.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Sauvegarder les paramètres
                _parametresService.SauvegarderParametres(ParametresActuels);

                // Mettre à jour la copie originale
                _parametresOriginaux = CloneParametres(ParametresActuels);

                StatusMessage = "✅ Paramètres enregistrés avec succès";

                // Notifier que la fenêtre peut se fermer
                FermetureDemandee?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur lors de l'enregistrement";
                MessageBox.Show($"Erreur: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Annuler()
        {
            // Restaurer les paramètres originaux
            ParametresActuels = CloneParametres(_parametresOriginaux);
            MotDePasse = ParametresActuels.SmtpPassword;

            // Notifier que la fenêtre peut se fermer
            FermetureDemandee?.Invoke();
        }

        private void Parcourir()
        {
            // Utiliser System.Windows.Forms pour le FolderBrowserDialog
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Sélectionnez le dossier d'archivage";
                dialog.SelectedPath = ParametresActuels.CheminArchivage;
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ParametresActuels.CheminArchivage = dialog.SelectedPath;
                    OnPropertyChanged(nameof(ParametresActuels));
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}