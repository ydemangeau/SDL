using Dev2Be.Toolkit;
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
    /// Logique d'interaction pour ActivitiesWindow.xaml
    /// </summary>
    public partial class ActivitiesWindow : Window
    {
        private Activity selectedActivity = null;

        private bool isClosed = false;

        private List<Activity> activities;

        private ListSortDirection _sortDirection;
        private GridViewColumnHeader _sortColumn;

        private SortAdorner listViewSortAdorner = null;

        private Teacher teacher;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.RemoveIcon();

            ShowInTaskbar = false;
        }

        public ActivitiesWindow(Teacher teacher)
        {
            InitializeComponent();
            
            this.teacher = teacher;

            BDDAccess bddAccess = new BDDAccess();

            if (bddAccess.Connect())
            {
                activities = bddAccess.GetActivities(teacher);

                ActivitiesListView.ItemsSource = activities;

                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ActivitiesListView.ItemsSource);

                view.Filter = ActivityFilter;
            }
            else
            {
                MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);

                isClosed = true;

                Close();
            }
        }

        public void Show(Window owner)
        {
            Owner = owner;

            if(!isClosed)
                Show();
        }

        private bool ActivityFilter(object item)
        {
            if (String.IsNullOrEmpty(ActivitiesFilterWatermark.Text))
                return true;
            else
                return ((item as Activity).ActivityName.IndexOf(ActivitiesFilterWatermark.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }


        private void ActivitesListView_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is GridViewColumnHeader column))
                return;

            if (column.Tag != null)
            {
                if (_sortColumn == column)
                    _sortDirection = _sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                else
                {
                    if (_sortColumn != null)
                    {
                        _sortColumn.Column.HeaderTemplate = null;
                        _sortColumn.Column.Width = _sortColumn.ActualWidth - 20;
                    }

                    _sortColumn = column;
                    _sortDirection = ListSortDirection.Ascending;
                    column.Column.Width = column.ActualWidth + 20;
                }

                string header = string.Empty;

                if (_sortColumn.Column.DisplayMemberBinding is Binding b)
                    header = b.Path.Path;

                ICollectionView resultDataView = CollectionViewSource.GetDefaultView(ActivitiesListView.ItemsSource);
                resultDataView.SortDescriptions.Clear();
                resultDataView.SortDescriptions.Add(new SortDescription(header, _sortDirection));
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (_sortColumn != null)
            {
                AdornerLayer.GetAdornerLayer(_sortColumn).Remove(listViewSortAdorner);
                ActivitiesListView.Items.SortDescriptions.Clear();
            }

            _sortColumn = column;
            listViewSortAdorner = new SortAdorner(_sortColumn, _sortDirection);
            AdornerLayer.GetAdornerLayer(_sortColumn).Add(listViewSortAdorner);
            ActivitiesListView.Items.SortDescriptions.Add(new SortDescription(sortBy, _sortDirection));
        }

        private void ActivitiesFilter_TextChanged(object sender, TextChangedEventArgs e) => CollectionViewSource.GetDefaultView(ActivitiesListView.ItemsSource).Refresh();

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            BDDAccess bddAccess = new BDDAccess();

            if (bddAccess.Connect())
            {
                activities = bddAccess.GetActivities(teacher);

                ActivitiesListView.ItemsSource = activities;

                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ActivitiesListView.ItemsSource);

                view.Filter = ActivityFilter;
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListBoxItem)ActivitiesListView.ContainerFromElement((Button)sender));

            selectedActivity = curItem.Content as Activity;
            
            ContextMenu cm = FindResource("ActionContextMenu") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            List<Activity> deletedActivities = null;

            if (selectedActivity != null)
            {
                deletedActivities = new List<Activity> { selectedActivity };

                BDDAccess bddAccess = new BDDAccess();

                if (bddAccess.Connect())
                    bddAccess.DeleteActivities(deletedActivities);

                if (!string.IsNullOrEmpty(bddAccess.Information))
                    MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
            }            
            else if (ActivitiesListView.SelectedIndex != -1)
            {
                deletedActivities = new List<Activity>();

                foreach (Activity activity in ActivitiesListView.SelectedItems)
                {
                    deletedActivities.Add(activity);
                }

                BDDAccess bddAccess = new BDDAccess();

                if(bddAccess.Connect())
                    bddAccess.DeleteActivities(deletedActivities);

                if (!string.IsNullOrEmpty(bddAccess.Information))
                    MessageBox.Show(bddAccess.Information, new AssemblyInformations(Assembly.GetExecutingAssembly().GetName().Name).Product, MessageBoxButton.OK, bddAccess.MessageBoxImage, MessageBoxResult.OK);
            }

            selectedActivity = null;
        }

        private void ActivityResultsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ActivityResultsWindow activityResultsWindow = new ActivityResultsWindow(selectedActivity);
            activityResultsWindow.Show(this);
            activityResultsWindow.Focus();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                RefreshButton_Click(null, null);
        }
    }
}
