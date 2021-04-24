using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.utils;
using System.Windows;
using System.Windows.Controls;

namespace projektlabor.covid19login.adminpanel.windows.mainWindow.uielements
{
    class ActionButton : Button
    {
        /// <summary>
        /// Property for the permissions that the user needs to have to beeing able to access this button
        /// </summary>
        public static readonly DependencyProperty PermissionProperty = DependencyProperty.Register("Permission", typeof(Permissions), typeof(ActionButton), new FrameworkPropertyMetadata(default));

        /// <summary>
        /// The permissions that are required to access this element
        /// </summary>
        public Permissions Permission
        {
            get { return (Permissions)GetValue(PermissionProperty); }
            set { SetValue(PermissionProperty, value); }
        }


        /// <summary>
        /// Enables or disables this button based on if the profile is allowed to access it
        /// </summary>
        /// <param name="profile">The profile to check</param>
        public void UpdateButtonByProfile(SimpleAdminEntity profile)
        {
            // Gets the current required permission
            int perm = this.Permission.GetEnumProperty("level").GetValue<int>();

            // Checks if the profile has those permissions and is not frozen
            this.IsEnabled = !profile.IsFrozen.Value && (perm & profile.Permissions) != 0;
        }

    }

    public enum Permissions
    {
        [EnumProperty("level",0b10)]
        ADMIN,
        [EnumProperty("level", 0b100)]
        RESET_ADMIN
    }
}
