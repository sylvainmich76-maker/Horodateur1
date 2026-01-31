using System;
using System.Windows;
using System.Windows.Controls;
using Horodateur.Models;

namespace Horodateur.Views
{
    public partial class AjouterPompierWindow : Window
    {
        public Pompier? NouveauPompier { get; private set; }

        public AjouterPompierWindow()
        {
            InitializeComponent();
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxNom.Text) ||
                string.IsNullOrWhiteSpace(TextBoxPrenom.Text))
            {
                MessageBox.Show("Le nom et le prénom sont obligatoires.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Générer un ID unique
            int nouvelId = DateTime.Now.GetHashCode() & 0x7FFFFFFF; // ID positif

            // Créer le nouveau pompier
            NouveauPompier = new Pompier
            {
                Id = nouvelId,
                Nom = TextBoxNom.Text.Trim(),
                Prenom = TextBoxPrenom.Text.Trim(),
                Grade = (ComboGrade.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pompier",
                Email = TextBoxEmail.Text.Trim(),
                EstPresent = false,
                IsEditable = false
            };

            DialogResult = true;
            Close();
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}