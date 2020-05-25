using SDLL.Configuration;
using System;
using System.IO;
using System.Windows.Controls;

namespace SDLP.UserControls
{
    /// <summary>
    /// Logique d'interaction pour BDDInformations.xaml
    /// </summary>
    [Serializable]
    public partial class BDDInformations : UserControl
    {
        public BDDInformations()
        {
            InitializeComponent();

            BDDConfiguration configuration = Charger();
            AdresseIpTextBox.Text = configuration.IPAddress;
            NomBddTextBox.Text = configuration.BDDName;
            MotDePasseWatermalPasswordBox.Password = configuration.Password;
            UtilisateurTextBox.Text = configuration.User;
        }

        public static BDDConfiguration Charger()
        {
            BDDConfiguration configuration = new BDDConfiguration();

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "bdd.bin")))
                configuration = BDDConfiguration.Load();

            return configuration;
        }

        public void Sauvegarder() => new BDDConfiguration() { IPAddress = AdresseIpTextBox.Text, BDDName = NomBddTextBox.Text, Password = MotDePasseWatermalPasswordBox.Password, User = UtilisateurTextBox.Text }.Save();
    }
}
