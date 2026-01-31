namespace Horodateur.Models
{
    public class Parametres
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string EmailDestinataire { get; set; }
        public string CheminArchivage { get; set; }
    }
}