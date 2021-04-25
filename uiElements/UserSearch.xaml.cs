using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace projektlabor.covid19login.adminpanel.uiElements
{
    /// <summary>
    /// Interaction logic for UserSearch.xaml
    /// </summary>
    public partial class UserSearch : UserControl
    {
        /// <summary>
        /// Event handler that executes when the a user got selected
        /// </summary>
        public Action<SimpleUserEntity> OnSelect { get; set; }

        /// <summary>
        /// Event handler that executes when the user opens the form
        /// Expects that once the user have been received the method SupplyUsers gets called.
        /// </summary>
        public Action OnRequestUsers { get; set; }

        public UserSearch() => InitializeComponent();

        /// <summary>
        /// Supplies the form with users from which the admin can then select one
        /// </summary>
        /// <param name="users">All users that got received</param>
        public void SupplyUsers(SimpleUserEntity[] users) => this.Dispatcher.Invoke(() =>
        {
            try
            {
                // Removes all previous users
                this.List.Items.Clear();
            }catch {}

            // Appends the new users
            foreach (var u in users)
            {
                // Creates the list item
                ListViewItem lvi = new ListViewItem
                {
                    // Creates the display
                    Content = u,
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Colors.White)
                };

                // Appends the user
                this.List.Items.Add(lvi);
            }

            // Shows the user list
            this.ViewList.Visibility = Visibility.Visible;
            // Hides the loading animation
            this.ViewLoading.Visibility = Visibility.Collapsed;

            this.FieldSearch.Focus();
        });

        /// <summary>
        /// Closes the user-menu
        /// </summary>
        public void CloseUserMenu()
        {
            // Checks that an item got selected
            if (this.List.SelectedItem == null)
                return;

            // Closes the popup
            this.Popup.IsOpen = false;
        }

        /// <summary>
        /// Executes when the button to search users gets clicked
        /// </summary>
        private void OnOpenButtonClick(object sender, RoutedEventArgs e)
        {
            // Resets the search bar
            this.FieldSearch.Text = string.Empty;

            // Shows the loading animation
            this.ViewLoading.Visibility = Visibility.Visible;

            // Hides the list view
            this.ViewList.Visibility = Visibility.Collapsed;

            // Calls the callback to ask for users
            this.OnRequestUsers?.Invoke();
        }
    
        /// <summary>
        /// Executes when the user selects another user
        /// </summary>
        private void OnSelectUser(object server, SelectionChangedEventArgs e)
        {
            // Closes the menu
            this.CloseUserMenu();

            // Gets the selected user
            // Updates the selected user
            SimpleUserEntity user = (SimpleUserEntity)((ListViewItem)this.List.SelectedItem).Content;

            // Executes the handler
            this.OnSelect?.Invoke(user);
        }

        /// <summary>
        /// Executes when the user types to search
        /// </summary>
        private void OnSearch(object sender, TextChangedEventArgs e)
        {
            // Iterates over all listed users
            foreach (var userItem in this.List.Items)
            {
                // Gets the listitem
                var itm = userItem as ListViewItem;

                // Checks if the user matches
                // Shows or hides the user
                itm.Visibility = (itm.Content as SimpleUserEntity).IsMatching(this.FieldSearch.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
