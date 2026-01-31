using System;

namespace Horodateur.Models
{
    public class HeureTravaillee
    {
        public Pompier Pompier { get; set; }
        public Intervention Intervention { get; set; }
        public TimeSpan HeuresNormales { get; set; }
        public TimeSpan HeuresMajorees { get; set; }

        public double CalculerHeuresMajorees()
        {
            double taux = Intervention.Appel switch
            {
                TypeAppel.Urgent => 1.5,
                TypeAppel.Nuit => 2.0,
                _ => 1.0
            };

            return HeuresNormales.TotalHours * taux;
        }
    }
}