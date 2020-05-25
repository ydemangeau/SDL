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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SDLP.UserControls
{
    /// <summary>
    /// Logique d'interaction pour FileAssociation.xaml
    /// </summary>
    public partial class FileAssociation : UserControl
    {
        public FileAssociation()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;

            if (hyperlink.Tag as string == "sdlp")
            {

            }
            else if (hyperlink.Tag as string == "sdle")
            {

            }
        }
    }
}
