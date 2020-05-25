using Dev2Be.Toolkit;
using SDLL;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace SDLP
{
    /// <summary>
    /// Logique d'interaction pour SaveBDDWindow.xaml
    /// </summary>
    public partial class SaveBDDWindow : Window
    {
        private Activity activity;
        private List<Class> classes;

        public SaveBDDWindow(Activity activity, Teacher teacher)
        {
            InitializeComponent();

            this.activity = activity;

            classes = new List<Class>();

            BDDAccess bddAccess = new BDDAccess();

            if(bddAccess.Connect())
            {
                classes = bddAccess.GetClasses();

                TeacherNameTextBox.Text = teacher.FirstName + " " + teacher.LastName;

                foreach (var classe in classes)
                    ClassesListBox.Items.Add(new CheckBox() { Content = classe.ClassName });

                ActivityNameTextBox.Text = activity.ActivityName;

                VisibilityCheckBox.IsChecked = activity.ActivityVisibility;

                foreach (var classe in activity.Classes)
                    foreach (CheckBox listBoxItem in ClassesListBox.Items)
                        if (listBoxItem.Content.ToString().Equals(classe.ClassName, StringComparison.CurrentCultureIgnoreCase))
                            listBoxItem.IsChecked = true;
            }
            else
            {
                MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);

                Close();
            }
        }

        internal void ShowDialog(Window owner)
        {
            Owner = owner;

            ShowDialog();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);

            ShowInTaskbar = false;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            activity.TeacherName = TeacherNameTextBox.Text;

            if (string.IsNullOrEmpty(ActivityNameTextBox.Text))
            {
                MessageBox.Show("Entrer un nom pour l'activité avant de la sauvegarder.", new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }
            else
                activity.ActivityName = ActivityNameTextBox.Text;

            activity.Classes.Clear();

            int compteur = 0;

            foreach (CheckBox item in ClassesListBox.Items)
            {
                if ((bool)item.IsChecked)
                    activity.Classes.Add(new Class() { IDClass = classes[compteur].IDClass, ClassName = classes[compteur].ClassName });

                compteur++;
            }

            if (activity.Classes.Count <= 0)
            {
                MessageBox.Show("Selectionner au minimum une classe pour pouvoir sauvegarder l'activité.", new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }

            activity.ActivityVisibility = (bool)VisibilityCheckBox.IsChecked;

            BDDAccess bddAccess = new BDDAccess();

            if(bddAccess.Connect())
            {
                bddAccess.SaveActivities(activity);

                if(!string.IsNullOrEmpty(bddAccess.Information))
                {
                    MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
                    return;
                }
            }
            else
            {
                MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
                return;
            }
        }
    }
}
