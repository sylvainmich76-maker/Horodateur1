using System;

namespace Horodateur.Models
{
    public enum TypeIntervention
    {
        Feu,
        Secours,
        Divers
    }

    public enum TypeAppel
    {
        Normal,
        Urgent,
        Nuit
    }

    public class Intervention
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public TypeIntervention Type { get; set; }
        public TypeAppel Appel { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string Commentaires { get; set; }

        public TimeSpan Duree => DateFin - DateDebut;
    }
}