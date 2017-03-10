﻿using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Gfe
{
    public partial class ConfigurationGfe : Form
    {
        static public bool changement = false;
        static public bool changementFichier = false;

        private string logiciel = null;
        private bool desactivation = false;

        private void ChargerConfig()
        {
            cb_couleur.Text = Principale.couleur;
            cb_dispo.Text = Principale.affichage;
            txt_externe.Text = Principale.logiciel;
            txt_extension.Text = Principale.extension;
            check_rappel.Checked = Principale.rappel;
            check_constanteVerif.Checked = Principale.rechargementConstant;
            check_sousdossier.Checked = Principale.sousDossier;
            check_conversion.Checked = Principale.conversion;

            if(!Program.nonXP)
                check_conversion.Enabled = false;
        }

        private void ChangerDossier()
        {
            logiciel = null;

            OpenFileDialog selectionFichier = new OpenFileDialog()
            {
                InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory),
                Filter = "Exécutables (*.exe)|*.exe|Tous les fichiers (*.*)|*.*",
                Title = "Choisissez le logiciel externe pour la modification des fichiers...",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (selectionFichier.ShowDialog() == DialogResult.OK)
            {
                logiciel = selectionFichier.FileName;
            }

            desactivation = false;
        }

        public ConfigurationGfe()
        {
            this.Icon = Properties.Resources.icone;

            InitializeComponent();
            ChargerConfig();
        }

        private void BoutonAppliquer_Clic(object sender, EventArgs e)
        {
            if (Principale.extension != txt_extension.Text | Principale.sousDossier != check_sousdossier.Checked)
                changementFichier = true;

            changement = true;

            Principale.affichage = cb_dispo.Text;
            Principale.logiciel = txt_externe.Text;
            Principale.couleur = cb_couleur.Text;
            Principale.extension = txt_extension.Text.Replace(" ", string.Empty).Replace(".", string.Empty);
            Principale.rappel = check_rappel.Checked;
            Principale.rechargementConstant = check_constanteVerif.Checked;
            Principale.sousDossier = check_sousdossier.Checked;
            Principale.conversion = check_conversion.Checked;

            Registre.MiseAjourConfig();

            this.DestroyHandle();
        }

        private void BoutonReset_Clic(object sender, EventArgs e)
        {
            DialogResult question = MessageBox.Show("Voulez-vous perdre votre configuration et remettre celle par défaut ?", "Réinitialisation de la configuration", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

            if (question == DialogResult.Yes)
            {
                changement = true;
                changementFichier = true;

                Principale.logiciel = Environment.SystemDirectory + @"\mspaint.exe";
                Principale.affichage = "étirer";
                Principale.couleur = "Noir";
                Principale.extension = "png;jpg;jpeg;bmp;tiff;tif";
                Principale.rappel = true;
                Principale.sousDossier = false;
                Principale.rechargementConstant = false;
                Principale.conversion = false;

                Registre.MiseAjourConfig();
                ChargerConfig();

                MessageBox.Show("Votre configuration a été réinitialisée !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
                    
        }

        private void BoutonExplorer_Clic(object sender, EventArgs e)
        {
            this.Enabled = false;
            desactivation = true;

            Thread proc = new Thread(new ThreadStart(ChangerDossier));
            proc.SetApartmentState(ApartmentState.STA);
            proc.Start();

            while (desactivation == true) { Thread.Sleep(100); }

            if(!string.IsNullOrEmpty(logiciel))
                txt_externe.Text = logiciel;

            this.Enabled = true;
            this.Activate();
            this.Show();
        }
    }
}
