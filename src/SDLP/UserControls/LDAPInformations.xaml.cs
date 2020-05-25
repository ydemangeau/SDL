using SDLL.Configuration;
using System;
using System.IO;
using System.Windows.Controls;

namespace SDLP.UserControls
{
    /// <summary>
    /// Logique d'interaction pour LDAPInformations.xaml
    /// </summary>
    [Serializable]
    public partial class LDAPInformations : UserControl
    {
        DomainServerConfiguration domainServerConfiguration;

        public LDAPInformations()
        {
            InitializeComponent();

            domainServerConfiguration = new DomainServerConfiguration();

            DataContext = domainServerConfiguration;

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "serveurDomaine.bin")))
            {
                DomainServerConfiguration informationsServeurDomaine = DomainServerConfiguration.Charger();
                IPAddressTextBox.Text = informationsServeurDomaine.IPAddress;
                DomainTextBox.Text = informationsServeurDomaine.DomainName;
            }
        }

        public void Sauvegarder()
        {
            domainServerConfiguration.IPAddress = IPAddressTextBox.Text;
            domainServerConfiguration.DomainName = DomainTextBox.Text;

            domainServerConfiguration.Sauvegarder();
        }
    }
}
