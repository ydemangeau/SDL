using Dev2Be.Toolkit;
using SDLL;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SDLP
{
    /// <summary>
    /// Logique d'interaction pour OuvrirBDD.xaml
    /// </summary>
    public partial class OpenBDDWindow : Window
    {
        List<Activity> activities;

        public string ActivityInformations { get; set; }

        public OpenBDDWindow(Teacher teacher)
        {
            InitializeComponent();

            BDDAccess bddAccess = new BDDAccess();
            
            if(bddAccess.Connect())
            {
                activities = bddAccess.GetActivities(teacher);

                if (!string.IsNullOrEmpty(bddAccess.Information))
                {
                    MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
                    return;
                }

                foreach (Activity activite in activities)
                    ActivitesListBox.Items.Add(activite.ActivityName);
            }
            else
                MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
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
