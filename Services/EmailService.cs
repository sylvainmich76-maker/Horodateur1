using System;
using System.IO;
using Horodateur.Models;

// MailKit namespaces
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Horodateur.Services
{
    public class EmailService
    {
        private readonly Parametres _parametres;

        public EmailService(Parametres parametres)
        {
            _parametres = parametres;
        }

        public bool EnvoyerRapport(string fichierPdf, string sujet, string corps)
        {
            try
            {
                // Création du message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Horodateur", _parametres.SmtpUsername));
                message.To.Add(new MailboxAddress("Destinataire", _parametres.EmailDestinataire));
                message.Subject = sujet;

                // Construction du corps
                var builder = new BodyBuilder
                {
                    HtmlBody = corps
                };

                // Ajout de la pièce jointe
                if (!string.IsNullOrEmpty(fichierPdf) && File.Exists(fichierPdf))
                {
                    builder.Attachments.Add(fichierPdf);
                }

                message.Body = builder.ToMessageBody();

                // Envoi avec SMTP
                using (var client = new SmtpClient())
                {
                    client.Connect(_parametres.SmtpServer, _parametres.SmtpPort, SecureSocketOptions.StartTls);
                    client.Authenticate(_parametres.SmtpUsername, _parametres.SmtpPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur MailKit: {ex.Message}");
                return false;
            }
        }
    }
}