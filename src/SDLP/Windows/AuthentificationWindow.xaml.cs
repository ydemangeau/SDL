using SDLL;
using SDLL.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SDLP
{
    /// <summary>
    /// Logique d'interaction pour AuthentificationWindow.xaml
    /// </summary>
    public partial class AuthentificationWindow : Window
    {
        private Teacher teacher;

        public AuthentificationWindow(ref Teacher teacher)
        {
            this.teacher = teacher;

            InitializeComponent();
        }

        public void ShowDialog(Window owner)
        {
            Owner = owner;

            ShowDialog();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);

            ShowInTaskbar = false;
        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid())
            {
                try
                {
                    DomainServerConfiguration informationsServeurDomaine = DomainServerConfiguration.Charger();

                    string ipDomaine = informationsServeurDomaine.IPAddress + "/";
                    string[] domaines = informationsServeurDomaine.DomainName.Split('.');

                    foreach (string domaine in domaines)
                        ipDomaine += "DC=" + domaine + ",";

                    ipDomaine = ipDomaine.TrimEnd(',');

                    Ldap ldap = new Ldap(ipDomaine, UtilisateurWatermarkTextBox.Text, MotDePasseWatermarkTextBox.Password);

                    if (ldap.Authentification())
                    {
                        List<string> groups = ldap.GetGroup();

                        string group = groups[0];

                        teacher.FirstName = ldap.GetFirstName();
                        teacher.LastName = ldap.GetLastName();
                        teacher.Subject = Teacher.GetSubject(ldap.GetGroup()[0]);

                        if (teacher.Subject == "Anglais")
                        {
                            Ldap.IsAuthificated = true;
                            Close();
                        }
                        else
                        {
                            InformationsTextBlock.Text = "Vous ne disposez pas des autorisations nécessaires pour continuer. Veuillez vérifier vos identifants avant de réessayer.";

                            teacher.Save(Path.Combine(Directory.GetCurrentDirectory(), teacher.FirstName + ".credential"));
                        }
                    }
                    else
                    {
                        InformationsTextBlock.Text = "Nom d'utilisateur ou mot de passe invalide. Veuillez vérifier vos identifants avant de réessayer.";
                        
                        teacher.Save(Path.Combine(Directory.GetCurrentDirectory(), teacher.FirstName + ".credential"));
                    }
                }
                catch (Exception ex)
                {
                    InformationsTextBlock.Text = "L'erreur suivante s'est produite : " + ex.Message;

                    using (FileStream fileStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), "AuthentificationErreur" + DateTime.Now+".txt")))
                    {
                        byte[] info = new UTF8Encoding(true).GetBytes("Message : " + ex.Message + Environment.NewLine + ex.StackTrace);
                        fileStream.Write(info, 0, info.Length);
                    }

                    teacher.Save(Path.Combine(Directory.GetCurrentDirectory(), teacher.FirstName + ".credential"));
                }
            }
        }

        private void WatermarkTextBox_LostFocus(object sender, RoutedEventArgs e) => IsValid();

        private bool IsValid()
        {
            if (string.IsNullOrEmpty(UtilisateurWatermarkTextBox.Text))
            {
                MotDePasseWatermarkTextBox.BorderBrush = Brushes.Red;
                UtilisateurWatermarkTextBox.BorderBrush = Brushes.Red;

                InformationsTextBlock.Text = "Veuillez rentrer un nom d'utilisateur.";

                return false;
            }

            return true;
        }
    }
}
