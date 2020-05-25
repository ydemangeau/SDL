using SDLP.UserControls;
using System.Windows;
using System.Windows.Controls;

namespace SDLP
{
    /// <summary>
    /// Logique d'interaction pour OptionWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        BDDInformations bddInformations;
        LDAPInformations ldapInformations;

        public OptionsWindow()
        {
            InitializeComponent();

            bddInformations = new BDDInformations();
            ldapInformations = new LDAPInformations();
        }

        public OptionsWindow(string option) : this()
        {
            foreach (TreeViewItem parentItem in OptionsListTreeView.Items)
            {
                if (parentItem.Header as string == option)
                {
                    parentItem.IsSelected = true;
                    break;
                }

                foreach (TreeViewItem childItem in parentItem.Items)
                {
                    if (childItem.Header as string == option)
                    {
                        childItem.IsSelected = true;
                        parentItem.IsExpanded = true;
                        break;
                    }
                }
            }
        }

        public void ShowDialog(Window owner)
        {
            Owner = owner;

            ShowDialog();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem treeViewItem = OptionsListTreeView.SelectedItem as TreeViewItem;

            switch (treeViewItem.Header)
            {
                case "Base de données":
                    OptionsGrid.Children.Clear();
                    OptionsGrid.Children.Add(bddInformations);
                    break;
                case "Serveur de domaine":
                    OptionsGrid.Children.Clear();
                    OptionsGrid.Children.Add(ldapInformations);
                    break;
                case "Association de fichiers":

                default:
                    break;
            }
        }

        private void SauvegarderButton_Click(object sender, RoutedEventArgs e)
        {
            bddInformations.Sauvegarder();
            ldapInformations.Sauvegarder();

            Close();
        }
    }
}
