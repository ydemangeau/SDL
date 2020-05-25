using SDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SDLP
{
    /// <summary>
    /// Logique d'interaction pour OuvrirBDD.xaml
    /// </summary>
    public partial class OpenBDD : Window
    {
        List<Activity> activities;

        public string ActivityInformations { get; set; }

        public OpenBDD()
        {
            InitializeComponent();

            BDDAccess bddAccess = new BDDAccess();
            
            if(bddAccess.Connect())
            {
                activities = bddAccess.GetActivities(new Teacher() { FirstName = "Vincent", LastName = "RAPIN" });

                if (!string.IsNullOrEmpty(bddAccess.Information))
                {
                    MessageBox.Show(bddAccess.Information, "Studio des Langues", MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
                    return;
                }

                foreach (Activity activite in activities)
                    ActivitesListBox.Items.Add(activite.ActivityName);
            }
            else
                MessageBox.Show(bddAccess.Information, "Studio des Langues", MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
        }

        public void ShowDialog(Window owner)
        {
            Owner = owner;

            ShowDialog();
        }

        private void ActivitesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ActivitesListBox.SelectedIndex != -1)
            {
                OpenActivity();

                Close();
            }
        }

        private void OuvrirButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActivitesListBox.SelectedIndex != -1)
            {
                OpenActivity();

                Close();
            }
        }

        private void OpenActivity() => ActivityInformations = ActivityPathParser.ParserInvert(activities[ActivitesListBox.SelectedIndex]);
    }
}
