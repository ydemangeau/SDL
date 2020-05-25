using SDLL;
using System;
using System.Windows;

namespace SDLP
{
    /// <summary>
    /// Logique d'interaction pour ResultViewer.xaml
    /// </summary>
    public partial class ResultViewer : Window
    {
        public ResultViewer(ActivityResult activityResult)
        {
            InitializeComponent();

            Title += activityResult.ActivityName + " - " + activityResult.Student.FirstName + " " + activityResult.Student.LastName;

            ActivityNameTextBlock.Text = activityResult.ActivityName;
            ActivityTextTextBlock.Text = activityResult.GetTextToDisplay();
            NumberOfFoundWordsTextBlock.Text = activityResult.FoundWords.Count + "/" + activityResult.NumberOfWordsTotal;
        }

        internal void Show(Window owner)
        {
            Owner = owner;

            Show();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);

            ShowInTaskbar = false;
        }
    }
}
