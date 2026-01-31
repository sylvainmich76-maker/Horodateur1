using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Horodateur.Models;
using Horodateur.Services;
using Horodateur.Views;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Horodateur.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private EmailService _emailService;
        private readonly PdfService _pdfService;
        private ArchivageService _archivageService;
        private readonly ParametresService _parametresService;

        private ObservableCollection<Pompier> _pompiers;
        public ObservableCollection<Pompier> Pompiers
        {
            get => _pompiers;
            set
            {
                if (_pompiers != value)
                {
                    _pompiers = value;
                    OnPropertyChanged();
                }
            }
        }

        private Intervention _interventionActuelle;
        public Intervention InterventionActuelle
        {
            get => _interventionActuelle;
            set
            {
                if (_interventionActuelle != value)
                {
                    _interventionActuelle = value;
                    OnPropertyChanged();
                }
            }
        }
        // AJOUTEZ CETTE MÉTHODE :
        private void NotifierChangementValidation()
        {
            OnPropertyChanged(nameof(DebugValidation));
            OnPropertyChanged(nameof(PompiersVisibles)); // AJOUTEZ cette ligne

            // Force le rafraîchissement de CanExecute
            var command = EnregistrerCommand as RelayCommand;
            command?.NotifyCanExecuteChanged();

            Console.WriteLine($"PompiersVisibles = {PompiersVisibles}");
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

        private bool _isSending;
        public bool IsSending
        {
            get => _isSending;
            set
            {
                if (_isSending != value)
                {
                    _isSending = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    OnPropertyChanged();

                    // Activer/désactiver le mode édition pour tous les pompiers
                    foreach (var pompier in Pompiers)
                    {
                        pompier.IsEditable = value;
                    }
                }
            }
        }
        // AJOUTEZ cette propriété :
        public bool PompiersVisibles
        {
            get
            {
                // Les pompiers sont visibles seulement si le nom ET l'adresse sont remplis
                bool visible = !string.IsNullOrWhiteSpace(InterventionActuelle.Nom) &&
                              !string.IsNullOrWhiteSpace(InterventionActuelle.Adresse);

                // Debug
                Console.WriteLine($"PompiersVisibles: Nom='{InterventionActuelle.Nom}', " +
                                $"Adresse='{InterventionActuelle.Adresse}', " +
                                $"Résultat={visible}");

                return visible;
            }
        }
        // NOUVELLES PROPRIÉTES POUR LES COMBOBOX
        private TypeIntervention _selectedTypeIntervention = TypeIntervention.Feu;
        public TypeIntervention SelectedTypeIntervention
        {
            get => _selectedTypeIntervention;
            set
            {
                if (_selectedTypeIntervention != value)
                {
                    _selectedTypeIntervention = value;
                    InterventionActuelle.Type = value; // Met à jour l'intervention
                    OnPropertyChanged();
                    NotifierChangementValidation();
                }
            }
        }

        private TypeAppel _selectedTypeAppel = TypeAppel.Normal;
        public TypeAppel SelectedTypeAppel
        {
            get => _selectedTypeAppel;
            set
            {
                if (_selectedTypeAppel != value)
                {
                    _selectedTypeAppel = value;
                    InterventionActuelle.Appel = value; // Met à jour l'intervention
                    OnPropertyChanged();
                    NotifierChangementValidation();
                }
            }
        }

        public ICommand TogglePompierCommand { get; }
        public ICommand EnregistrerCommand { get; }
        public ICommand OuvrirParametresCommand { get; }
        public ICommand ReinitialiserCommand { get; }
        public ICommand AjouterPompierCommand { get; }
        public ICommand EditerPompiersCommand { get; }
        public ICommand SauvegarderPompiersCommand { get; }
        public ICommand SupprimerPompierCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _parametresService = new ParametresService();
            var parametres = _parametresService.ChargerParametres();

            _emailService = new EmailService(parametres);
            _pdfService = new PdfService();
            _archivageService = new ArchivageService(parametres.CheminArchivage);

            // Initialiser la liste vide d'abord
            Pompiers = new ObservableCollection<Pompier>();

            // Commands existantes
            TogglePompierCommand = new RelayCommand<Pompier>(TogglePompier);
            EnregistrerCommand = new RelayCommand(Enregistrer, CanEnregistrer);
            OuvrirParametresCommand = new RelayCommand(OuvrirParametres);
            ReinitialiserCommand = new RelayCommand(Reinitialiser);

            // Nouvelles commands pour la gestion des pompiers
            AjouterPompierCommand = new RelayCommand(AjouterPompier);
            EditerPompiersCommand = new RelayCommand(EditerPompiers);
            SauvegarderPompiersCommand = new RelayCommand(SauvegarderPompiers);
            SupprimerPompierCommand = new RelayCommand<Pompier>(SupprimerPompier);

            // Charger les pompiers depuis le fichier
            ChargerPompiers();

            // Initialiser l'intervention
            InterventionActuelle = new Intervention
            {
                DateDebut = DateTime.Now,
                DateFin = DateTime.Now.AddHours(2),
                Type = TypeIntervention.Feu,
                Appel = TypeAppel.Normal
            };

            // Initialiser les propriétés de sélection
            SelectedTypeIntervention = TypeIntervention.Feu;
            SelectedTypeAppel = TypeAppel.Normal;

            StatusMessage = "Prêt - " + Pompiers.Count + " pompiers chargés";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ChargerPompiers()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "Horodateur");
                string filePath = Path.Combine(appFolder, "pompiers.json");

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var pompiersData = JsonSerializer.Deserialize<List<PompierData>>(json);

                    if (pompiersData != null && pompiersData.Any())
                    {
                        Pompiers.Clear();

                        foreach (var data in pompiersData)
                        {
                            Pompiers.Add(new Pompier
                            {
                                Id = data.Id,
                                Nom = data.Nom,
                                Prenom = data.Prenom,
                                Grade = data.Grade,
                                Email = data.Email,
                                EstPresent = false,
                                IsEditable = false
                            });
                        }

                        return; // Chargement réussi
                    }
                }

                // Si aucun fichier ou fichier vide, charger les données par défaut
                InitialiserPompiersParDefaut();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des pompiers: {ex.Message}\nChargement des pompiers par défaut.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                InitialiserPompiersParDefaut();
            }
        }

        private void InitialiserPompiersParDefaut()
        {
            Pompiers.Clear();

            var pompiersDefaut = new List<Pompier>
            {
                new Pompier { Id = 1, Nom = "Dupont", Prenom = "Jean", Grade = "Capitaine", Email = "j.dupont@example.com" },
                new Pompier { Id = 2, Nom = "Martin", Prenom = "Pierre", Grade = "Lieutenant", Email = "p.martin@example.com" },
                new Pompier { Id = 3, Nom = "Bernard", Prenom = "Marie", Grade = "Sergent", Email = "m.bernard@example.com" },
                new Pompier { Id = 4, Nom = "Dubois", Prenom = "Sophie", Grade = "Pompier", Email = "s.dubois@example.com" },
                new Pompier { Id = 5, Nom = "Thomas", Prenom = "Luc", Grade = "Pompier", Email = "l.thomas@example.com" }
            };

            foreach (var pompier in pompiersDefaut)
            {
                Pompiers.Add(pompier);
            }
        }

        private void SauvegarderPompiers()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "Horodateur");
                string filePath = Path.Combine(appFolder, "pompiers.json");

                // Créer le dossier s'il n'existe pas
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                // Préparer les données pour la sauvegarde (sans propriétés temporaires)
                var pompiersData = Pompiers.Select(p => new PompierData
                {
                    Id = p.Id,
                    Nom = p.Nom,
                    Prenom = p.Prenom,
                    Grade = p.Grade,
                    Email = p.Email
                }).ToList();

                string json = JsonSerializer.Serialize(pompiersData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, json);
                StatusMessage = $"{Pompiers.Count} pompiers sauvegardés";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur sauvegarde: {ex.Message}";
                MessageBox.Show($"Erreur lors de la sauvegarde des pompiers: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AjouterPompier()
        {
            var dialog = new AjouterPompierWindow
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true && dialog.NouveauPompier != null)
            {
                // Vérifier si le pompier existe déjà
                if (Pompiers.Any(p =>
                    p.Nom.Equals(dialog.NouveauPompier.Nom, StringComparison.OrdinalIgnoreCase) &&
                    p.Prenom.Equals(dialog.NouveauPompier.Prenom, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Ce pompier existe déjà dans la liste.",
                        "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Pompiers.Add(dialog.NouveauPompier);
                StatusMessage = $"Pompier {dialog.NouveauPompier.NomComplet} ajouté";
                SauvegarderPompiers(); // Sauvegarder automatiquement
            }
        }

        private void AjouterPompierSimple(string nom, string prenom, string grade)
        {
            try
            {
                // Générer un ID unique (timestamp + hash)
                int nouvelId = Math.Abs((nom + prenom + DateTime.Now.Ticks).GetHashCode());

                var nouveauPompier = new Pompier
                {
                    Id = nouvelId,
                    Nom = nom.Trim(),
                    Prenom = prenom.Trim(),
                    Grade = grade,
                    Email = "",
                    EstPresent = false,
                    IsEditable = false
                };

                Pompiers.Add(nouveauPompier);
                StatusMessage = $"Pompier {nouveauPompier.NomComplet} ajouté";
                SauvegarderPompiers();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur ajout: {ex.Message}";
            }
        }

        private void EditerPompiers()
        {
            IsEditMode = !IsEditMode; // Basculer le mode édition

            if (IsEditMode)
            {
                StatusMessage = "Mode édition activé - Cliquez sur 🗑️ pour supprimer un pompier";
            }
            else
            {
                StatusMessage = "Mode édition désactivé";
            }
        }

        private void SupprimerPompier(Pompier pompier)
        {
            if (pompier != null && IsEditMode)
            {
                var result = MessageBox.Show(
                    $"Voulez-vous vraiment supprimer le pompier:\n\n" +
                    $"{pompier.NomComplet}\n" +
                    $"{pompier.Grade}",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Pompiers.Remove(pompier);
                    StatusMessage = $"Pompier {pompier.NomComplet} supprimé";
                    SauvegarderPompiers(); // Sauvegarder après suppression
                }
            }
        }

        private void TogglePompier(Pompier pompier)
        {
            if (pompier != null)
            {
                pompier.EstPresent = !pompier.EstPresent;
                OnPropertyChanged(nameof(Pompiers));
                NotifierChangementValidation();

                // Mettre à jour le statut
                int presents = Pompiers.Count(p => p.EstPresent);
                if (!string.IsNullOrEmpty(InterventionActuelle.Nom))
                {
                    StatusMessage = $"{presents} pompier(s) présent(s) pour '{InterventionActuelle.Nom}'";
                }
            }
        }

        private bool CanEnregistrer()
        {
            return !IsSending &&
                   !string.IsNullOrEmpty(InterventionActuelle.Nom) &&
                   !string.IsNullOrEmpty(InterventionActuelle.Adresse) &&
                   InterventionActuelle.DateDebut < InterventionActuelle.DateFin &&
                   Pompiers.Any(p => p.EstPresent);
        }
        public string DebugValidation
        {
            get
            {
                try
                {
                    return $"DEBUG - {DateTime.Now:HH:mm:ss}\n" +
                           $"1. Nom: '{(string.IsNullOrWhiteSpace(InterventionActuelle.Nom) ? "VIDE" : InterventionActuelle.Nom)}'\n" +
                           $"2. Adresse: '{(string.IsNullOrWhiteSpace(InterventionActuelle.Adresse) ? "VIDE" : InterventionActuelle.Adresse)}'\n" +
                           $"3. Dates: {InterventionActuelle.DateDebut:HH:mm} < {InterventionActuelle.DateFin:HH:mm} = " +
                           $"{(InterventionActuelle.DateDebut < InterventionActuelle.DateFin ? "✅" : "❌")}\n" +
                           $"4. Pompiers: {Pompiers.Count(p => p.EstPresent)}/{Pompiers.Count} sélectionnés\n" +
                           $"5. Bouton activé: {(CanEnregistrer() ? "✅ OUI" : "❌ NON")}";
                }
                catch
                {
                    return "Debug en cours d'initialisation...";
                }
            }
        }
        private async void Enregistrer()
        {
            try
            {
                IsSending = true;
                StatusMessage = "Génération du rapport...";

                var pompiersPresents = Pompiers.Where(p => p.EstPresent).ToList();
                string pdfPath = _pdfService.GenererRapport(InterventionActuelle, pompiersPresents);

                StatusMessage = "Envoi du rapport...";

                string sujet = $"Rapport d'intervention - {InterventionActuelle.Nom}";
                string corps = $"<h1>Rapport d'intervention</h1>" +
                               $"<p><strong>Nom:</strong> {InterventionActuelle.Nom}</p>" +
                               $"<p><strong>Adresse:</strong> {InterventionActuelle.Adresse}</p>" +
                               $"<p><strong>Type:</strong> {InterventionActuelle.Type}</p>" +
                               $"<p><strong>Appel:</strong> {InterventionActuelle.Appel}</p>" +
                               $"<p><strong>Date:</strong> {InterventionActuelle.DateDebut:dd/MM/yyyy HH:mm} - {InterventionActuelle.DateFin:dd/MM/yyyy HH:mm}</p>" +
                               $"<p><strong>Durée:</strong> {InterventionActuelle.Duree:hh\\:mm}</p>" +
                               $"<p><strong>Pompiers présents:</strong> {pompiersPresents.Count}</p>" +
                               $"<p><strong>Commentaires:</strong> {InterventionActuelle.Commentaires}</p>";

                bool emailEnvoye = _emailService.EnvoyerRapport(pdfPath, sujet, corps);

                // Archiver le PDF
                string archivePath = _archivageService.ArchiverPdf(pdfPath);

                StatusMessage = emailEnvoye
                    ? "Rapport envoyé et archivé avec succès!"
                    : "Rapport archivé mais erreur d'envoi email";

                await Task.Delay(2000);
                Reinitialiser();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur: {ex.Message}";
                MessageBox.Show($"Erreur lors de l'enregistrement: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSending = false;
            }
        }

        private void OuvrirParametres()
        {
            var parametresWindow = new ParametresWindow
            {
                Owner = Application.Current.MainWindow
            };

            parametresWindow.ShowDialog();

            // Recharger les paramètres après fermeture
            var parametres = _parametresService.ChargerParametres();
            _emailService = new EmailService(parametres);
            _archivageService = new ArchivageService(parametres.CheminArchivage);

            StatusMessage = "Paramètres mis à jour";
        }

        private void Reinitialiser()
        {
            // Réinitialiser la présence des pompiers
            foreach (var pompier in Pompiers)
            {
                pompier.EstPresent = false;
            }

            // Réinitialiser l'intervention
            InterventionActuelle = new Intervention
            {
                DateDebut = DateTime.Now,
                DateFin = DateTime.Now.AddHours(2),
                Type = TypeIntervention.Feu,
                Appel = TypeAppel.Normal,
                Nom = string.Empty,
                Adresse = string.Empty,
                Commentaires = string.Empty
            };

            // Réinitialiser les propriétés de sélection des ComboBox
            SelectedTypeIntervention = TypeIntervention.Feu;
            SelectedTypeAppel = TypeAppel.Normal;

            StatusMessage = "Prêt - " + Pompiers.Count + " pompiers disponibles";

            // Désactiver le mode édition si actif
            if (IsEditMode)
            {
                IsEditMode = false;
            }
            NotifierChangementValidation();
        }

        // Classe interne pour la sérialisation/désérialisation
        private class PompierData
        {
            public int Id { get; set; }
            public string Nom { get; set; } = string.Empty;
            public string Prenom { get; set; } = string.Empty;
            public string Grade { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}