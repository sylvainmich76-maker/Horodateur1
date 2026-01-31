using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Horodateur.Models
{
    public class Pompier : INotifyPropertyChanged
    {
        private bool _estPresent;
        private bool _isEditable;

        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool EstPresent
        {
            get => _estPresent;
            set
            {
                if (_estPresent != value)
                {
                    _estPresent = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                if (_isEditable != value)
                {
                    _isEditable = value;
                    OnPropertyChanged();
                }
            }
        }

        public string NomComplet => $"{Nom} {Prenom}";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}