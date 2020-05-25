using Dev2Be.Toolkit;
using Dev2Be.Toolkit.Extensions;
using Microsoft.Win32;
using SDLL;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SDLP
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Activity activity;

        private Teacher teacher;

        public MainWindow()
        {
            InitializeComponent();

            NewCommandBinding_Executed(null, null);

            AssemblyInformations assemblyInformation = new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name);

            if (assemblyInformation.InformationalVersion.Contains("alpha") || assemblyInformation.InformationalVersion.Contains("beta") || assemblyInformation.InformationalVersion.Contains("preview") || assemblyInformation.InformationalVersion.Contains("rc"))
                Title = Title + " | Build " + assemblyInformation.InformationalVersion;

            teacher = new Teacher();
        }

        public MainWindow(ref Teacher teacher) : this()
        {
            this.teacher = teacher;
        }

        public MainWindow(string activityInformations):this()
        {
            activity = ActivityPathParser.Parser(activityInformations);

            activity.ProvidedWords = activity.ProvidedWords.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            TexteTextBox.DataContext = activity;

            ProvidedWordsListBox.ItemsSource = activity.ProvidedWords;
        }

        public MainWindow(string activityInformations, ref Teacher teacher) : this()
        {
            activity = ActivityPathParser.Parser(activityInformations);

            this.teacher = teacher;

            if (activity.IDActivity != -2)
            {
                BDDAccess bddAccess = new BDDAccess();
                
                if(bddAccess.Connect())
                {
                    activity = bddAccess.GetActivity(activity.IDActivity);

                    activity.ProvidedWords = activity.ProvidedWords.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                    TexteTextBox.DataContext = activity;

                    ProvidedWordsListBox.ItemsSource = activity.ProvidedWords;

                    if (!string.IsNullOrEmpty(bddAccess.Information))
                        MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
                }
            }
        }

        public void ShowDialog(Window owner)
        {
            Owner = owner;
            ShowDialog();
        }

        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            activity = new Activity() { Text = "" };
            TexteTextBox.DataContext = activity;

            ProvidedWordsListBox.ItemsSource = activity.ProvidedWords;
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Ldap.IsAuthificated)
                new AuthentificationWindow(ref teacher).ShowDialog(this);

            if (Ldap.IsAuthificated)
            {
                OpenBDDWindow openBDD = new OpenBDDWindow(teacher);
                openBDD.ShowDialog(this);

                if (openBDD.ActivityInformations != "" && openBDD.ActivityInformations != null)
                {
                    activity = ActivityPathParser.Parser(openBDD.ActivityInformations);

                    if (activity.IDActivity != -2)
                    {
                        BDDAccess bddAccess = new BDDAccess();

                        if (bddAccess.Connect())
                        {
                            activity = bddAccess.GetActivity(activity.IDActivity);

                            activity.ProvidedWords = activity.ProvidedWords.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                            TexteTextBox.DataContext = activity;

                            ProvidedWordsListBox.ItemsSource = activity.ProvidedWords;

                            if (!string.IsNullOrEmpty(bddAccess.Information))
                                MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
                        }
                    }
                }
            }
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(activity.Text))
            {
                if (!Ldap.IsAuthificated)
                    new AuthentificationWindow(ref teacher).ShowDialog(this);

                if (Ldap.IsAuthificated)
                {
                    SaveBDDWindow saveBDDWindow = new SaveBDDWindow(activity, teacher);
                    saveBDDWindow.ShowDialog(this);
                }
            }
            else
                MessageBox.Show("Veuillez saisir du texte avant d'enregistrer l'activité.", new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        private void ImportActivityCommandBingind_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Fichier Studio des Langues Professeur|*.sdlp" };

            if ((bool)openFileDialog.ShowDialog())
            {
                if (Path.GetExtension(openFileDialog.FileName) == ".sdlp")
                {
                    activity = Activity.Open(openFileDialog.FileName);

                    TexteTextBox.DataContext = activity;

                    activity.ProvidedWords = activity.ProvidedWords.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                    ProvidedWordsListBox.ItemsSource = activity.ProvidedWords;
                }                
            }
        }

        private void ExportActivityCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Fichier Studio des Langues Professeur|*.sdlp" };

            if ((bool)saveFileDialog.ShowDialog())
                activity.Save(saveFileDialog.FileName);
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) => Close();

        private void ExitCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) => System.Windows.Application.Current.Shutdown(0);

        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (TexteTextBox.SelectedText != "")
                TexteTextBox.SelectedText = TexteTextBox.SelectedText.Remove(0);
        }

        private void ToUpperCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (TexteTextBox.SelectedText != "")
                TexteTextBox.SelectedText = TexteTextBox.SelectedText.ToUpper();
        }

        private void ToLowerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (TexteTextBox.SelectedText != "")
                TexteTextBox.SelectedText = TexteTextBox.SelectedText.ToLower();
        }

        private void WordWrapCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO ==> Gérer état checkbox lors de l'utilisation du raccourci.

            if(WordWrapMenuItem.IsChecked)
            {
                TexteTextBox.TextWrapping = TextWrapping.Wrap;
            }
            else
            {
                TexteTextBox.TextWrapping = TextWrapping.NoWrap;
            }
        }

        private void ActivitiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Ldap.IsAuthificated)
                new AuthentificationWindow(ref teacher).ShowDialog(this);

            if (Ldap.IsAuthificated)
                new ActivitiesWindow(teacher).Show(this);
        }

        private void BDDConfigurationMenuItem_Click(object sender, RoutedEventArgs e) => new OptionsWindow("Base de données").ShowDialog(this);

        private void LDAPConfigurationMenuItem_Click(object sender, RoutedEventArgs e) => new OptionsWindow("Serveur de domaine").ShowDialog(this);

        private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProvidedWordsWatermarkTextBox.Text != "")
                AddProvidedWords(ProvidedWordsWatermarkTextBox.Text);

            ProvidedWordsWatermarkTextBox.Text = "";
            ProvidedWordsListBox.Items.Refresh();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ProvidedWordsListBox.SelectedItems)
                activity.ProvidedWords.Remove(item.ToString());

            ProvidedWordsListBox.Items.Refresh();
        }

        private void AddProvidedWordsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (TexteTextBox.SelectedText != "")
                AddProvidedWords(TexteTextBox.SelectedText);

            ProvidedWordsListBox.Items.Refresh();
        }

        public void AddProvidedWords(string word)
        {
            CultureInfo cultureInfo = new CultureInfo("fr-Fr");

            if (cultureInfo.CompareInfo.IndexOf(TexteTextBox.Text, word, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)
            {
                string[] words = word.Split(Activity.SpecialCharacters, StringSplitOptions.RemoveEmptyEntries);

                foreach (string item in words)
                {
                    if (item.IsFullWord(TexteTextBox.Text, Dev2Be.Toolkit.Enumerations.StringComparison.IgnoreCaseAndDiacritics))
                    {
                        if (!activity.ProvidedWords.Contains(word, StringComparer.OrdinalIgnoreCase))
                            activity.ProvidedWords.Add(item);
                    }
                }
            }
        }
    }
}
