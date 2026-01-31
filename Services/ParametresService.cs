using System.IO;
using System.Text.Json;
using Horodateur.Models;

namespace Horodateur.Services
{
    public class ParametresService
    {
        private readonly string _configPath;

        public ParametresService()
        {
            _configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Horodateur",
                "config.json");
        }

        public Parametres ChargerParametres()
        {
            if (!File.Exists(_configPath))
            {
                return new Parametres
                {
                    SmtpServer = "smtp.gmail.com",
                    SmtpPort = 587,
                    CheminArchivage = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "ArchivesHorodateur")
                };
            }

            string json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<Parametres>(json);
        }

        public void SauvegarderParametres(Parametres parametres)
        {
            string directory = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(parametres, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_configPath, json);
        }
    }
}