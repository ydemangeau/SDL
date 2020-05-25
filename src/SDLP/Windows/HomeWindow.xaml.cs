using Microsoft.Win32;
using SDLL;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;

namespace SDLP
{
    /// <summary>
    /// Logique d'interaction pour HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        private bool isClosed = false;

        private Teacher teacher;

        public HomeWindow()
        {
            InitializeComponent();

            teacher = new Teacher();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => isClosed = true;

        private void TutorialHyperLink_Click(object sender, RoutedEventArgs e)
        {
            Process process = null;

            try
            {
                process = new Process();
                process.StartInfo.FileName = "Aide [SDLP].pdf";
                process.StartInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "fr-FR");
                process.Start();
            }
            catch (Exception) { }
            finally
            {
                if (process != null)
                    process.Close();
            }
        }

        private void ActivitiesBDDHyperLink_Click(object sender, RoutedEventArgs e)
        {
            if (!Ldap.IsAuthificated)
                new AuthentificationWindow(ref teacher).ShowDialog(this);

            OpenBDDWindow openBDD = null;

            if (Ldap.IsAuthificated)
            {
                openBDD = new OpenBDDWindow(teacher);
                openBDD.ShowDialog(this);

                if (!string.IsNullOrEmpty(openBDD.ActivityInformations))
                {
                    Hide();

                    new MainWindow(openBDD.ActivityInformations, ref teacher).ShowDialog(this);
                }
            }

            if (!isClosed && !string.IsNullOrEmpty(openBDD.ActivityInformations))
                ShowDialog();
        }

        private void OpenFileHyperLink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;

            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Fichier Studio des Langues Professeur|*.sdlp|Fichier Studio des Langues Elèves|*.sdle" };

            if (hyperlink.Tag as string == "Result")
                openFileDialog.FilterIndex = 2;

            if ((bool)openFileDialog.ShowDialog())
            {
                Hide();

                if (Path.GetExtension(openFileDialog.FileName) == ".sdlp")
                    new MainWindow(ActivityPathParser.ParserInvert(openFileDialog.FileName), ref teacher).ShowDialog();
                else if (Path.GetExtension(openFileDialog.FileName) == ".sdle")
                {
                    ActivityResult activityResult = ActivityResult.Open(openFileDialog.FileName);
                    new ResultViewer(activityResult).ShowDialog();
                }

                if (!isClosed)
                    ShowDialog();
            }
        }

        private void NewActivityHyperLink_Click(object sender, RoutedEventArgs e)
        {
            Hide();

            new MainWindow(ref teacher).ShowDialog(this);

            if (!isClosed) ShowDialog();
        }

        private void ActivitiesHyperLink_Click(object sender, RoutedEventArgs e)
        {
            if (!Ldap.IsAuthificated)
                new AuthentificationWindow(ref teacher).ShowDialog(this);

            if(Ldap.IsAuthificated)
            {

                new ActivitiesWindow(teacher).Show(this);
            }
        }

        private void BDDConfigurationHyperLink_Click(object sender, RoutedEventArgs e)
        {
            new OptionsWindow("Base de données").ShowDialog(this);
        }

        private void DomainServerConfigurationHyperLink_Click(object sender, RoutedEventArgs e)
        {
            new OptionsWindow("Serveur de domaine").ShowDialog(this);
        }
    }
}
