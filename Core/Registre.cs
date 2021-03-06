﻿using System;
using Microsoft.Win32;
using System.Windows.Forms;
using Gfe.Langues;
using Gfe.Fenetres;

namespace Gfe.Core
{
    public class Registre
    {
        public const string emplacement = @"SOFTWARE\Gestionnaire de Fonds d'Écran";
        private const string nomContextuel = "Gestionnaire de Fonds d'Écran";

        static public RegistryKey registre = Registry.CurrentUser.OpenSubKey(emplacement, RegistryKeyPermissionCheck.ReadWriteSubTree);
        public static string ancienChemin = null;
        public static int ancienId = 0;
        public static bool miseAJour = false;

        static public void Initialisation()
        {
            if (registre == null)
            {
                registre = Registry.CurrentUser.CreateSubKey(emplacement);

                ResetRegistre();
            }
            else
            {
                try
                {
                    // Clef pour la disposition du fond d'écran
                    if (registre.GetValue("Disposition").ToString() != null)
                        Principale.affichage = registre.GetValue("Disposition").ToString();
                    else
                        registre.SetValue("Disposition", "etirer");

                    // Clef pour le logiciel externe
                    if (registre.GetValue("LogicielExterne").ToString() != null)
                        Principale.logiciel = registre.GetValue("LogicielExterne").ToString();
                    else
                        registre.SetValue("LogicielExterne", Principale.logiciel);

                    // Clef pour l'ancien chemin utilisé la dernière fois dans le logiciel
                    if (registre.GetValue("AncienChemin").ToString() != null)
                        ancienChemin = registre.GetValue("AncienChemin").ToString();

                    if (registre.GetValue("CouleurFond").ToString() != null)
                        Principale.couleur = registre.GetValue("CouleurFond").ToString();

                    // Clef pour les extensions utilisées par le logiciel
                    if (registre.GetValue("Extensions").ToString() != null)
                        Principale.extension = registre.GetValue("Extensions").ToString();
                    else
                        registre.SetValue("Extensions", Principale.extension);

                    // Clef pour l'ancien ID de fond utilisé dans le logiciel
                    if (registre.GetValue("AncienID") is int)
                        ancienId = Convert.ToInt32(registre.GetValue("AncienID"));
                    else
                        registre.SetValue("AncienID", 0);

                    if (registre.GetValue("Historique").ToString() != null)
                        Principale.historique = registre.GetValue("Historique").ToString();
                    else
                        registre.SetValue("Historique", "");

                    // Quelques clefs booléennes
                    Principale.rappel = Convert.ToBoolean(registre.GetValue("Rappel"));
                    Principale.sousDossier = Convert.ToBoolean(registre.GetValue("SousDossier"));
                    Principale.rechargementConstant = Convert.ToBoolean(registre.GetValue("VerifConstante"));
                    Principale.conversion = Convert.ToBoolean(registre.GetValue("ConversionBitmap"));
                    miseAJour = Convert.ToBoolean(registre.GetValue("MiseAJour"));
                }
                catch
                {
                    MessageBox.Show(Texte.ErreurRegistre, Texte.ErreurTitre, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    ResetRegistre();
                }

                Principale.chemin = ancienChemin;
            }
        }

        static public void ViderRegistre()
        {
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Directory\shell\" + nomContextuel, false);
            Registry.CurrentUser.DeleteSubKeyTree(emplacement, false);
        }

        static public void ResetRegistre()
        {
            registre.SetValue("Disposition", Principale.affichage);
            registre.SetValue("LogicielExterne", Principale.logiciel);
            registre.SetValue("nbErreur", 0);
            registre.SetValue("nbFond", 0);
            registre.SetValue("Rappel", Principale.rappel);
            registre.SetValue("AncienChemin", "");
            registre.SetValue("AncienID", 0);
            registre.SetValue("MiseAJour", false);
            registre.SetValue("Extensions", Principale.extension);
            registre.SetValue("SousDossier", Principale.sousDossier);
            registre.SetValue("VerifConstante", Principale.rechargementConstant);
            registre.SetValue("Historique", "");
            registre.SetValue("ConversionBitmap", false);
            registre.SetValue("CouleurFond", Principale.couleur);
        }

        static public void MiseAjourConfig()
        {
            registre.SetValue("LogicielExterne", Principale.logiciel);
            registre.SetValue("Disposition", Principale.affichage);
            registre.SetValue("Rappel", Principale.rappel);
            registre.SetValue("AncienID", Principale.id);
            registre.SetValue("Extensions", Principale.extension);
            registre.SetValue("SousDossier", Principale.sousDossier);
            registre.SetValue("ConversionBitmap", Principale.conversion);
            registre.SetValue("VerifConstante", Principale.rechargementConstant);
            registre.SetValue("CouleurFond", Principale.couleur);

            // Pour l'historique
            Principale.historique = Func.GenererHistorique(Principale.chemin, Principale.historique);

            if(!string.IsNullOrEmpty(Principale.historique))
                registre.SetValue("Historique", Principale.historique);

            if(!string.IsNullOrEmpty(Principale.chemin))
                registre.SetValue("AncienChemin", Principale.chemin);
        }

        static public void CompterErreur()
        {
            int var = 0;

            try { var = Convert.ToInt32(registre.GetValue("nbErreur")) + 2; }
            catch { var = 1; }

            registre.SetValue("nbErreur", var);
        }

        static public void CompterFond()
        {
            int var = 0;

            try { var = Convert.ToInt32(registre.GetValue("nbFond")) + 1; }
            catch { var = 1; }

            registre.SetValue("nbFond", var);
        }

        static public void AjouterContextuel()
        {
            RegistryKey clef = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\" + nomContextuel, RegistryKeyPermissionCheck.ReadWriteSubTree);
            RegistryKey sousClef = clef.CreateSubKey("command", RegistryKeyPermissionCheck.ReadWriteSubTree);

            clef.SetValue("", Texte.MenuContextuelTexte);
            clef.SetValue("Icon", Program.dossierAppdata + "icone.ico");
            sousClef.SetValue("", "\"" + System.Reflection.Assembly.GetEntryAssembly().Location + "\" /O \"%1\"");
            sousClef.Close();
            clef.Close();
        }

        static public void RetirerContextuel()
        {
            RegistryKey clef = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Directory\shell", true);

            clef.DeleteSubKeyTree(nomContextuel);
            clef.Close();
        }

        static public bool VerifierContextuel()
        {

            RegistryKey clef = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Directory\shell\" + nomContextuel + @"\command", false);
            bool resultat = false;

            try
            {
                if (clef.GetValue("") != null)
                    resultat = true;
            }
            catch { resultat = false; }

            return resultat;
        }

        static public string RecupererLangue()
        {
            try
            {
                return registre.GetValue("Langue").ToString().ToLower().Substring(0, 2);
            }
            catch { return string.Empty; }
        }
    }
}
