using System.Windows;

namespace projektlabor.covid19login.adminpanel.windows.utils
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {

        /// <summary>
        /// Reference for the text that gets displayed
        /// </summary>
        public string DisplayText
        {
            get => this.textdisplay.Text;
            set => this.Dispatcher.Invoke(()=>this.textdisplay.Text = value);
        }

        public LoadingWindow(string title)
        {
            InitializeComponent();

            this.Title = title;
        }
    }
}
