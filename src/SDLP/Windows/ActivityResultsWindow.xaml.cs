using Dev2Be.Toolkit;
using Microsoft.Win32;
using SDLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace SDLP
{
    /// <summary>
    /// Logique d'interaction pour ActivityResultsWindow.xaml
    /// </summary>
    public partial class ActivityResultsWindow : Window
    {
        private ActivityResult selectedActivity = null;

        ContextMenu cm;

        private List<ActivityResult> activityResults;

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.RemoveIcon();

            ShowInTaskbar = false;
        }

        public ActivityResultsWindow(Activity activity)
        {
            InitializeComponent();

            Title = Title += activity.ActivityName;

            BDDAccess bddAccess = new BDDAccess();

            if(bddAccess.Connect())
            {
                activityResults = bddAccess.GetStudentResults(activity);

                ResultsListView.ItemsSource = activityResults;

                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ResultsListView.ItemsSource);
            }

            if(!string.IsNullOrEmpty(bddAccess.Information))
                MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
        }

        internal void Show(Window owner)
        {
            Owner = owner;

            Show();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListBoxItem)ResultsListView.ContainerFromElement((Button)sender));

            selectedActivity = curItem.Content as ActivityResult;

            cm = FindResource("ActionContextMenu") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            List<ActivityResult> deletedResults = null;

            if (selectedActivity != null)
            {
                ActivityResult activityResult = selectedActivity;

                BDDAccess bddAccess = new BDDAccess();

                if (bddAccess.Connect())
                    bddAccess.DeleteResult(activityResult);

                if (!string.IsNullOrEmpty(bddAccess.Information))
                    MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
            }
            else if (ResultsListView.SelectedIndex != -1)
            {
                deletedResults = new List<ActivityResult>();

                foreach (ActivityResult activityResult in ResultsListView.SelectedItems)
                {
                    deletedResults.Add(activityResult);
                }

                BDDAccess bddAccess = new BDDAccess();

                if (bddAccess.Connect())
                    bddAccess.DeleteResults(deletedResults);

                if (!string.IsNullOrEmpty(bddAccess.Information))
                    MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
            }

            selectedActivity = null;
        }

        private void OpenResultMenuItem_Click(object sender, RoutedEventArgs e)
        {
            cm.IsOpen = false;

            ResultViewer activityResultsWindow = new ResultViewer(selectedActivity);
            activityResultsWindow.Show(this);
            activityResultsWindow.Focus();
        }

        private void ExportResultMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Fichier Studio des Langues Elève|*.sdle", FileName = selectedActivity.ActivityName + " - " + selectedActivity.Student.LastName + " " + selectedActivity.Student.FirstName };

            if ((bool)saveFileDialog.ShowDialog())
                selectedActivity.Save(saveFileDialog.FileName);
        }

        private void ResultsListViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ResultsListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ResultsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }
    }
}
