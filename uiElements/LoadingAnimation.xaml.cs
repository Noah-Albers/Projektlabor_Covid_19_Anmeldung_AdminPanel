using System.Windows.Controls;

namespace projektlabor.covid19login.adminpanel.uiElements
{
    /// <summary>
    /// Interaction logic for LoadingAnimation.xaml
    /// </summary>
    public partial class LoadingAnimation : UserControl
    {
        public LoadingAnimation()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
