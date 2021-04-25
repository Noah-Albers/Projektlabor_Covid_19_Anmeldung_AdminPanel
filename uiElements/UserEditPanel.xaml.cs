using projektlabor.covid19login.adminpanel.datahandling.entities;
using System.Linq;
using System.Windows.Controls;

namespace projektlabor.covid19login.adminpanel.uiElements
{
    /// <summary>
    /// Interaction logic for RegisterForm.xaml
    /// </summary>
    public partial class UserEditPanel : UserControl
    {
        /// <summary>
        /// Data-bindings for all text-values on the form element
        /// </summary>
        public string DataFirstname { get => this.FieldFirstname.Text; set => this.FieldFirstname.Text=value; }
        public string DataLastname { get => this.FieldLastname.Text; set => this.FieldLastname.Text = value; }
        public string DataStreet { get => this.FieldStreet.Text; set => this.FieldStreet.Text = value; }
        public string DataStreetnumber { get => this.FieldStreetNumber.Text; set => this.FieldStreetNumber.Text = value; }
        public string DataLocation { get => this.FieldLocation.Text; set => this.FieldLocation.Text = value; }
        public string DataPlz { get => this.FieldPLZ.Text; set => this.FieldPLZ.Text = value; }
        public string DataTelephone { get => this.FieldTelephone.Text; set => this.FieldTelephone.Text = value; }
        public string DataEmail { get => this.FieldEmail.Text; set => this.FieldEmail.Text = value; }
        public string DataRFID { get => this.FieldRFID.Text; set => this.FieldRFID.Text = value; }
        public bool? DataAutoDeleteAccount { get => this.CheckboxDelAccount.IsChecked.Value; set => this.CheckboxDelAccount.IsChecked = value; }

        /// <summary>
        /// All field value using the extended user entity
        /// </summary>
        public UserEntity UserInput
        {
            set
            {
                this.DataAutoDeleteAccount = value.AutoDeleteAccount;
                this.DataEmail = value.Email;
                this.DataFirstname = value.Firstname;
                this.DataLastname = value.Lastname;
                this.DataTelephone = value.TelephoneNumber;
                this.DataLocation = value.Location;
                this.DataPlz = value.PLZ.ToString();
                this.DataStreetnumber = value.StreetNumber;
                this.DataStreet = value.Street;
                this.DataRFID = value.Rfid;
            }
        }

        /// <summary>
        /// Holds all form field elements that are used at the registration form.
        /// This is used to autodelete all data from these forms.
        /// </summary>
        private readonly CustomInput[] fieldGroup;

        public UserEditPanel()
        {
            InitializeComponent();
            this.DataContext = this;
            this.fieldGroup = new CustomInput[] {
                this.FieldRFID,
                this.FieldTelephone,
                this.FieldEmail,
                this.FieldFirstname,
                this.FieldLastname,
                this.FieldLocation,
                this.FieldStreetNumber,
                this.FieldPLZ,
                this.FieldStreet
            };
        }

        /// <summary>
        /// Merges all values from the form onto the given user-entity.
        /// Be sure to check that everyting is correct first using the UpdateFields method.
        /// </summary>
        public void MergeOn(UserEntity ent)
        {
            ent.AutoDeleteAccount = this.DataAutoDeleteAccount;
            ent.Email = this.DataEmail;
            ent.Firstname = this.DataFirstname;
            ent.Lastname = this.DataLastname;
            ent.TelephoneNumber = this.DataTelephone;
            ent.Location = this.DataLocation;
            ent.PLZ = this.DataPlz.Length > 0 ? (int?)int.Parse(this.DataPlz) : null;
            ent.StreetNumber = this.DataStreetnumber;
            ent.Street = this.DataStreet;
            ent.Rfid = this.DataRFID;
        }

        /// <summary>
        /// Checks if every field has its value set if not optional and if all field match their case.
        /// If a field does not match their case, it displays the error
        /// </summary>
        public bool UpdateFields() => this.fieldGroup.Select(field => field.UpdateResolvable() == 0).Aggregate((a, b) => a && b);

        /// <summary>
        /// Resets the register form back to the default register screen
        /// </summary>
        public void ResetForm()
        {
            // Updates the button and checkboxs
            this.CheckboxDelAccount.IsChecked = false;

            // Clears all fields
            foreach (var field in this.fieldGroup)
                field.Reset();
        }
    }
}
