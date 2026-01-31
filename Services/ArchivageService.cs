using System;
using System.IO;

namespace Horodateur.Services
{
    public class ArchivageService
    {
        private readonly string _basePath;

        public ArchivageService(string basePath) // Pas de problème ici
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public string ArchiverPdf(string sourcePath)
        {
            if (!File.Exists(sourcePath))
                return null;

            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(_basePath, fileName);

            // Ajouter un timestamp si le fichier existe déjà
            int counter = 1;
            while (File.Exists(destPath))
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                destPath = Path.Combine(_basePath, $"{nameWithoutExt}_{counter}{extension}");
                counter++;
            }

            File.Move(sourcePath, destPath);
            return destPath;
        }
    }
}