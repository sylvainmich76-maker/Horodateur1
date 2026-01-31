using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Horodateur.Models;

namespace Horodateur.Services
{
    public class PdfService
    {
        public string GenererRapport(Intervention intervention, List<Pompier> pompiers)
        {
            string fileName = $"Rapport_{intervention.Nom}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            // Créer un nouveau document PDF
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Rapport d'intervention";

            // Ajouter une page
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // CORRECTION : Utiliser XFontStyleEx au lieu de XFontStyle pour PdfSharp 6.x
            XFont titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
            XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
            XFont normalFont = new XFont("Arial", 10, XFontStyleEx.Regular);
            XFont italicFont = new XFont("Arial", 10, XFontStyleEx.Italic);
            XFont boldItalicFont = new XFont("Arial", 10, XFontStyleEx.BoldItalic);

            // Position de départ
            double yPos = 50;
            double leftMargin = 50;
            double rightMargin = page.Width - 50;

            // Titre
            gfx.DrawString("RAPPORT D'INTERVENTION", titleFont, XBrushes.Black,
                new XRect(leftMargin, yPos, rightMargin - leftMargin, 30),
                XStringFormats.Center);
            yPos += 40;

            // Ligne de séparation
            gfx.DrawLine(XPens.Gray, leftMargin, yPos, rightMargin, yPos);
            yPos += 20;

            // Section 1 : Informations de l'intervention
            gfx.DrawString("1. INFORMATIONS DE L'INTERVENTION", headerFont, XBrushes.DarkBlue,
                new XRect(leftMargin, yPos, rightMargin - leftMargin, 20),
                XStringFormats.TopLeft);
            yPos += 25;

            // Table des informations
            AddInfoRow(gfx, "Nom de l'intervention:", intervention.Nom, leftMargin, ref yPos,
                      rightMargin, normalFont);
            AddInfoRow(gfx, "Adresse:", intervention.Adresse, leftMargin, ref yPos,
                      rightMargin, normalFont);
            AddInfoRow(gfx, "Type d'intervention:", intervention.Type.ToString(), leftMargin, ref yPos,
                      rightMargin, normalFont);
            AddInfoRow(gfx, "Type d'appel:", intervention.Appel.ToString(), leftMargin, ref yPos,
                      rightMargin, normalFont);
            AddInfoRow(gfx, "Date et heure de début:", intervention.DateDebut.ToString("dd/MM/yyyy HH:mm"),
                      leftMargin, ref yPos, rightMargin, normalFont);
            AddInfoRow(gfx, "Date et heure de fin:", intervention.DateFin.ToString("dd/MM/yyyy HH:mm"),
                      leftMargin, ref yPos, rightMargin, normalFont);
            AddInfoRow(gfx, "Durée totale:", intervention.Duree.ToString(@"hh\:mm") + " heures",
                      leftMargin, ref yPos, rightMargin, normalFont);

            yPos += 10;

            // Section 2 : Commentaires
            if (!string.IsNullOrEmpty(intervention.Commentaires))
            {
                gfx.DrawString("2. COMMENTAIRES", headerFont, XBrushes.DarkBlue,
                    new XRect(leftMargin, yPos, rightMargin - leftMargin, 20),
                    XStringFormats.TopLeft);
                yPos += 25;

                // Gérer les commentaires multilignes
                string[] commentLines = SplitText(intervention.Commentaires, 80);
                foreach (var line in commentLines)
                {
                    gfx.DrawString(line, normalFont, XBrushes.Black,
                        new XRect(leftMargin, yPos, rightMargin - leftMargin, 15),
                        XStringFormats.TopLeft);
                    yPos += 15;
                }
                yPos += 10;
            }

            // Section 3 : Pompiers présents
            var pompiersPresents = pompiers.Where(p => p.EstPresent).ToList();
            if (pompiersPresents.Any())
            {
                gfx.DrawString("3. POMPIERS PRÉSENTS (" + pompiersPresents.Count + ")",
                    headerFont, XBrushes.DarkBlue,
                    new XRect(leftMargin, yPos, rightMargin - leftMargin, 20),
                    XStringFormats.TopLeft);
                yPos += 25;

                foreach (var pompier in pompiersPresents)
                {
                    gfx.DrawString($"• {pompier.NomComplet}", boldItalicFont, XBrushes.Black,
                        new XRect(leftMargin, yPos, rightMargin - leftMargin, 15),
                        XStringFormats.TopLeft);

                    gfx.DrawString($"  Grade: {pompier.Grade}", normalFont, XBrushes.DarkGray,
                        new XRect(leftMargin + 20, yPos + 15, rightMargin - leftMargin, 12),
                        XStringFormats.TopLeft);

                    yPos += 30;
                }
            }

            // Section 4 : Informations de génération
            yPos += 20;
            gfx.DrawLine(XPens.LightGray, leftMargin, yPos, rightMargin, yPos);
            yPos += 20;

            gfx.DrawString("INFORMATIONS DE GÉNÉRATION",
                new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.Gray,
                new XRect(leftMargin, yPos, rightMargin - leftMargin, 15),
                XStringFormats.TopLeft);
            yPos += 15;

            gfx.DrawString($"• Document généré le: {DateTime.Now:dd/MM/yyyy à HH:mm:ss}",
                italicFont, XBrushes.Gray,
                new XRect(leftMargin, yPos, rightMargin - leftMargin, 12),
                XStringFormats.TopLeft);
            yPos += 12;

            gfx.DrawString($"• Application: Horodateur v1.0",
                italicFont, XBrushes.Gray,
                new XRect(leftMargin, yPos, rightMargin - leftMargin, 12),
                XStringFormats.TopLeft);
            yPos += 12;

            gfx.DrawString($"• Service: Incendie et Secours",
                italicFont, XBrushes.Gray,
                new XRect(leftMargin, yPos, rightMargin - leftMargin, 12),
                XStringFormats.TopLeft);

            // Cadre autour du document
            gfx.DrawRectangle(XPens.LightGray, leftMargin - 5, 40,
                rightMargin - leftMargin + 10, yPos + 30);

            // Enregistrer le document
            document.Save(filePath);
            document.Close();

            return filePath;
        }

        private void AddInfoRow(XGraphics gfx, string label, string value,
                               double left, ref double yPos, double right, XFont font)
        {
            // Label en gras
            gfx.DrawString(label,
                new XFont("Arial", 10, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(left, yPos, 150, 15),
                XStringFormats.TopLeft);

            // Valeur
            gfx.DrawString(value, font, XBrushes.Black,
                new XRect(left + 160, yPos, right - left - 160, 15),
                XStringFormats.TopLeft);

            yPos += 18;
        }

        private string[] SplitText(string text, int maxLength)
        {
            List<string> lines = new List<string>();

            while (text.Length > maxLength)
            {
                int breakPoint = text.LastIndexOf(' ', maxLength);
                if (breakPoint == -1) breakPoint = maxLength;

                lines.Add(text.Substring(0, breakPoint).Trim());
                text = text.Substring(breakPoint).Trim();
            }

            if (!string.IsNullOrEmpty(text))
                lines.Add(text);

            return lines.ToArray();
        }
    }
}