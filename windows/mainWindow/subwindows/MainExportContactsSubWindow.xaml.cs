using OfficeOpenXml;
using projektlabor.covid19login.adminpanel.connection;
using projektlabor.covid19login.adminpanel.connection.requests;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.Properties.langs;
using projektlabor.covid19login.adminpanel.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using OfficeOpenXml.Style;
using projektlabor.covid19login.adminpanel.windows.utils;

namespace projektlabor.covid19login.adminpanel.windows.mainWindow.subwindows
{
    /// <summary>
    /// Interaction logic for MainExportContactsSubWindow.xaml
    /// </summary>
    public partial class MainExportContactsSubWindow : MainSubWindow
    {

        /// <summary>
        /// Task that performs a request to the server to ask for users that can be displayed at the user-select
        /// </summary>
        private Task userSelectTask;

        public MainExportContactsSubWindow(Action<TechnicalError> technicalErrorHandler, Action<CommonError> commonErrorHandler, RequestData credentials, long authCode) : base(technicalErrorHandler,commonErrorHandler,credentials,authCode)
        {
            InitializeComponent();

            // Set the current date minus 2 weeks
            this.dateSelect.SelectedDate = DateTime.Now.AddDays(-14);
        }

        /// <summary>
        /// Event handler for the user-select event to request users
        /// </summary>
        private void OnUserSelectRequestUsers()
        {
            // Checks if there is already a request running
            if (this.userSelectTask != null)
                return;

            // Creates the request
            var req = new GrabUsersRequest()
            {
                OnCommonError = this.CommonRequestErrorHandler,
                OnTechnicalError = this.TechnicalRequestErrorHandler,
                OnReceive = OnReceiveUsers
            };

            // Starts the task
            this.userSelectTask = Task.Run(() =>
            {
                // Performs the request
                req.DoRequest(this.credentials, this.authCode);

                // Resets the task
                this.userSelectTask = null;
            });

            // Executes if the request is successfull and users are returned
            void OnReceiveUsers(SimpleUserEntity[] users) => this.Dispatcher.Invoke(() =>
                // Forwords the received users
                this.userSelect.SupplyUsers(users)
            );
        }

        /// <summary>
        /// Event handler for when the export button gets clicked
        /// </summary>
        private void OnExportButtonClicked(object sender, RoutedEventArgs e)
        {
            // Tries to get the aerosole time
            if(!int.TryParse(this.timeSelect.Text,out int marginTime) || marginTime < 0)
            {
                this.DisplayMessage(string.Empty,Lang.main_sub_contacts_time);
                return;
            }

            // Tries to get the time
            if (!this.dateSelect.SelectedDate.HasValue)
            {
                this.DisplayMessage(string.Empty, Lang.main_sub_contacts_time);
                return;
            }

            // Checks if a date has been specified
            if(this.dateSelect.SelectedDate == null)
            {
                this.DisplayMessage(string.Empty, Lang.main_sub_contacts_date);
                return;
            }

            // Checks if a user has been selected
            if(this.userSelect.User == null)
            {
                this.DisplayMessage(string.Empty, Lang.main_sub_contacts_user);
                return;
            }

            // Creates the request for the contacts
            var req = new AdminInfectedContactsRequest()
            {
                OnCommonError = this.CommonRequestErrorHandler,
                OnTechnicalError = this.TechnicalRequestErrorHandler,
                OnNotFoundError = OnUserNotFound,
                OnSuccess = OnSuccessfullRequest
            };

            // Gets the missing values
            DateTime afterDate = this.dateSelect.SelectedDate.Value;
            int userId = this.userSelect.User.Id.Value;

            // Opens the loading-overlay
            this.OverlayLoading.Visibility = Visibility.Visible;

            // Starts the task
            Task.Run(() =>
            {
                // Performs the request
                req.DoRequest(this.credentials, this.authCode, afterDate, userId, marginTime);

                // Hides the overlay
                this.Dispatcher.Invoke(() => this.OverlayLoading.Visibility = Visibility.Collapsed);
            });

            // Executes if the selected user could not be found
            void OnUserNotFound() => this.Dispatcher.Invoke(() =>
            {
                // Clears the selected user
                this.userSelect.User = null;

                // Displays the error
                this.DisplayMessage(string.Empty, Lang.main_sub_contacts_req_user);
            });

            // Executes if the request was successfull and the contacts have been received
            void OnSuccessfullRequest(UserEntity profile,KeyValuePair<UserEntity, ContactInfoEntity[]>[] contacts) => this.Dispatcher.Invoke(() =>
            {
                // Creates an excel-work-sheet
                using(var ep = new ExcelPackage())
                {
                    // Prints the sheets
                    this.ExportToExcel(ep, profile, contacts);

                    // Saves or cancel the excel-sheet's
                    Save(ep);
                }

                // Waits for the user to eighter save the excel-sheet or abort the saving 
                void Save(ExcelPackage package)
                {
                    // Creates a save-file-dialog to select the save location for the document
                    SaveFileDialog sfd = new SaveFileDialog()
                    {
                        ValidateNames = true,
                        CheckPathExists = true,
                        SupportMultiDottedExtensions = true,
                        FileName = "Export.xlsx",
                    };

                    // Opens the propt and waits for the users respone and checks if the user selected no export path
                    if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        return;

                    try
                    {
                        // Tries to save
                        package.SaveAs(new FileInfo(Path.GetFullPath(sfd.FileName)));
                    }
                    catch
                    {
                        // Creates the window to display the error and ask the user to retry
                        var win = new YesNoWindow(string.Empty, () => Save(package), null, Lang.global_button_retry, Lang.global_button_cancel, string.Empty, Lang.main_sub_contacts_save_fail);

                        // Displays the window
                        win.ShowDialog();
                    }
                }
            });
        }

        /// <summary>
        /// Creates a excel-sheet from the given infected-user and his contacts
        /// </summary>
        /// <param name="package">The excel-sheet</param>
        /// <param name="profile">The infected-user profile</param>
        /// <param name="contacts">All contacts of the user</param>
        private void ExportToExcel(ExcelPackage package, UserEntity profile, KeyValuePair<UserEntity, ContactInfoEntity[]>[] contacts)
        {
            // Creates the overview
            var overview = package.Workbook.Worksheets.Add(Lang.excel_overview);
            overview.DefaultColWidth = 12;
            overview.Column(1).Width = 4;

            // Prints the user profile
            this.PrintUserInfos(overview, profile, Lang.excel_title_infected, 1, 1);

            // Holds all subpages and the longest exposure time of contacts
            var contactInfos = new Tuple<ExcelWorksheet, UserEntity, TimeSpan, bool>[contacts.Length];

            // Creates the sub-pages for all contact's
            for (int i = 0; i < contacts.Length; i++)
            {
                // Gets the contact
                var c = contacts[i];

                // Creates the subpage and gets the longest exposure time
                var info = this.CreateSubsheetForContact(package, i + 1, c.Key, c.Value);

                // Appends the user and inserts into the contact-infos
                contactInfos[i] = new Tuple<ExcelWorksheet, UserEntity, TimeSpan, bool>(info.Item1, c.Key, info.Item2, info.Item3);
            }

            // Prints all contacts
            this.PrintContactTable(overview, Lang.excel_title_contacts, 1, 13, contactInfos);
        }

        /// <summary>
        /// Creates a subsheet for the person where the person's information and all exposure times are printed
        /// </summary>
        /// <param name="package">The excel-package</param>
        /// <param name="userid">The id of the user (Used just to seperate the sheets)</param>
        /// <param name="profile">User's profile</param>
        /// <param name="contacts">All contacts the user had with the infected person</param>
        /// <returns>
        /// Firstly (ExcelWorksheet) the sub-sheet with the user's infos
        /// 
        /// Secondly (TimeSpan, bool) also the worst exposure time for further processing
        /// </returns>
        private Tuple<ExcelWorksheet, TimeSpan, bool> CreateSubsheetForContact(ExcelPackage package, int userid, UserEntity profile, ContactInfoEntity[] contacts)
        {
            // Creates the sheet
            var sheet = package.Workbook.Worksheets.Add($"{Lang.excel_contacts_contact} {userid}");
            sheet.DefaultColWidth = 12;
            sheet.Column(1).Width = 4;

            // Prints the user profile
            this.PrintUserInfos(sheet, profile, Lang.excel_title_contacperson, 1, 1);

            // Prints the back button
            this.PrintBackButton(sheet, 11, 1, package.Workbook.Worksheets[0]);

            // Gets the contacts with their times
            var contactWTimes = contacts.Select(contact =>
            {
                // Gets the overlapping time
                var overlap = GetOverlappingTimespan(contact);

                return new Tuple<ContactInfoEntity,TimeSpan,bool>(contact,overlap.Key,overlap.Value);
            }).ToArray();

            // Prints the contact-times table with all overlapping times
            this.PrintContactTimesTable(sheet, Lang.excel_title_contacttimes, 1, 13, contactWTimes);

            // Gets the worst exposure time to get a fast overview
            var longestTime = GetWorstExposureTime(contactWTimes.Select(x => new KeyValuePair<TimeSpan,bool>(x.Item2, x.Item3)).ToArray());

            // Returns a tuple with the worksheet, the longest exposure time and a bool that indecates of that time is only with aerosole's
            return new Tuple<ExcelWorksheet, TimeSpan, bool>(sheet, longestTime.Key, longestTime.Value);
        }

        /// <summary>
        /// Sets a table-cell's value to display the contact-time from <see cref="GetOverlappingTimespan(ContactInfoEntity)"/>
        /// If the contact had indirect exposure viva aerosoles, it will display a little watermark,
        /// indicating that it was no direct exposure and that it's only the time between arrivale and leave of the persons
        /// </summary>
        /// <param name="cell">The cell to apply the value to</param>
        /// <param name="contactTime">The exposure time</param>
        /// <param name="isAerosoles">If the contact was only exposed to the aerosoles</param>
        private void SetTableCellToContactTime(ExcelRange cell,TimeSpan contactTime,bool isAerosoles)
        {
            // Sets the value
            cell.Value = contactTime.ToString();

            // Checks if the overlapping time is only the aerosoles
            if (isAerosoles)
            {
                // Unmerges the cell to make space for the aerosoles-tag
                cell.Merge = false;

                // Gets the last cell
                var backCell = cell.Last();

                // Appends the style
                backCell.Style.Font.Color.SetColor(Color.BlueViolet);
                backCell.Style.Font.Bold = true;
                backCell.Value = $"({Lang.excel_aerosoles})";
            }
        }

        /// <summary>
        /// Prints a back-button into the sheet that links back to the main sheet
        /// </summary>
        /// <param name="sheet">The sheet to print on</param>
        /// <param name="startX">X-starting position on the sheet</param>
        /// <param name="startY">Y-starting position on the sheet</param>
        /// <param name="mainSheet">The main sheet</param>
        private void PrintBackButton(ExcelWorksheet sheet,int startX,int startY,ExcelWorksheet mainSheet)
        {
            startX++;
            startY++;

            // Gets the field
            var field = sheet.Cells[startY, startX, startY + 3, startX + 1];

            // Applyes the borders and color
            ExcelUtils.ApplyTableStyle(field);

            // Gets the field with the back button
            var backbuttonField = sheet.Cells[startY + 1, startX, startY + 2, startX + 1];
            // Styles the field
            backbuttonField.Merge = true;
            backbuttonField.Value = Lang.excel_button_back;
            backbuttonField.Style.Font.Size = 20;
            backbuttonField.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            backbuttonField.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Applies the link
            ExcelUtils.LinkTo(backbuttonField, mainSheet);
        }

        /// <summary>
        /// Prints a vertical-table with all contact times and the exposed time onto the sheet
        /// The exposed time will be calculated by <see cref="GetOverlappingTimespan(ContactInfoEntity)"/>
        /// and applied using <see cref="SetTableCellToContactTime(ExcelRange, TimeSpan, bool)"/>
        /// </summary>
        /// <param name="sheet">The sheet to print on</param>
        /// <param name="title">The title of the table</param>
        /// <param name="startX">X-starting position on the sheet</param>
        /// <param name="startY">Y-starting position on the sheet</param>
        /// <param name="contactInfos">The contactentity and the already calculated exposure time</param>
        private void PrintContactTimesTable(ExcelWorksheet sheet,string title,int startX,int startY, Tuple<ContactInfoEntity, TimeSpan, bool>[] contactInfos)
        {
            // Header of the table
            string[] header = {
                Lang.excel_contacts_start_contact,
                Lang.excel_contacts_stop_contact,
                Lang.excel_contacts_start_infected,
                Lang.excel_contacts_stop_infected,
                Lang.excel_contacts_contactime
            };

            // Format for the timestamps
            string timeformat = "dd.MM.yyyy HH:mm:ss";

            // Creates the table
            ExcelUtils.PrintVerticalTable(sheet, title, startX, startY, header, contactInfos.Length,SupplyCell);

            // Supply function for the cells
            void SupplyCell(int x, int y, ExcelRange cell)
            {
                // Checks for the header
                if (y == 0)
                    return;

                // Gets the raw infos
                var infos = contactInfos[y - 1];

                // Checks for the contact-time row
                if (x == 4)
                {
                    // Sets the contact-time
                    this.SetTableCellToContactTime(cell, infos.Item2, infos.Item3);
                    return;
                }

                // Sets the format
                cell.Style.Numberformat.Format = timeformat;

                // Checks the row
                switch (x)
                {
                    case 0:
                        cell.Value = infos.Item1.contactStartTime.ToString(timeformat);
                        return;
                    case 1:
                        cell.Value = infos.Item1.contactStopTime.ToString(timeformat);
                        return;
                    case 2:
                        cell.Value = infos.Item1.infectedStartTime.ToString(timeformat);
                        return;
                    case 3:
                        cell.Value = infos.Item1.infectedStopTime.ToString(timeformat);
                        return;
                }
            }
        }

        /// <summary>
        /// Supplies a given cell with the given value.
        /// If the value is null, the default not given message in the current language will be displayed
        /// and the cell will be formatted accordingly.
        /// Can optionally ignore excel-number-stored-as-text errors.
        /// </summary>
        /// <param name="cell">The cell to supply the value to</param>
        /// <param name="value">Nullable value to supply to the cell</param>
        /// <param name="ignoreNumberErrors">If excel-number-stored-as-text errors should be ignored on the excel-sheet</param>
        private void SetTableValue(ExcelRange cell,string value,bool ignoreNumberErrors=false)
        {
            // Sets the value
            cell.Value = value ?? Lang.excel_user_not_given;

            // Checks if number-stored-as-text-errors should be ignored
            if(ignoreNumberErrors)
                // Ignores the errors
                cell.Worksheet.IgnoredErrors.Add(cell).NumberStoredAsText = true;

            // Checks if the value is not given
            if (value == null)
            {
                cell.Style.Font.Italic = true;
                cell.Style.Font.Color.SetColor(Color.Gray);
            }
        }

        /// <summary>
        /// Prints a table that displays an overview over all contacts of a user with their data and a link to their sub-sheet.
        /// </summary>
        /// <param name="sheet">The sheet to print this table on</param>
        /// <param name="title">The title of the table</param>
        /// <param name="startX">X-starting position on the sheet</param>
        /// <param name="startY">Y-starting position on the sheet</param>
        /// <param name="contacts">
        /// A tupel that contains
        /// - the worksheet that is dedicated to the user (user-subsheet)
        /// - the userentity with its information
        /// - the longest time a user has been exposed to the infected user
        /// - if the exposure time is with the user itself or only with the aerosoles
        /// </param>
        private void PrintContactTable(ExcelWorksheet sheet, string title, int startX, int startY, Tuple<ExcelWorksheet,UserEntity, TimeSpan, bool>[] contacts)
        {
            // Header for the table
            string[] header = {
                Lang.excel_user_name,
                Lang.excel_user_location,
                Lang.excel_user_street,
                Lang.excel_user_email,
                Lang.excel_user_telephone,
                Lang.excel_contacts_longesttime,
                Lang.excel_contacts_showmore
            };

            // Creates the table
            ExcelUtils.PrintVerticalTable(sheet, title, startX, startY, header, contacts.Length, SupplyCell);

            // Supply function for the cells
            void SupplyCell(int x, int y, ExcelRange cell)
            {
                // Checks for the header
                if (y == 0)
                    return;

                // Gets the tuple of the current contact
                var tuple = contacts[y - 1];

                // Gets the user
                var user = tuple.Item2;

                // Supplies the cells
                switch (x)
                {
                    case 0:
                        this.SetTableValue(cell, $"{user.Firstname} {user.Lastname}");
                        return;
                    case 1:
                        cell.Value = $"{user.PLZ}/{user.Location}";
                        return;
                    case 2:
                        this.SetTableValue(cell, $"{user.Street}. {user.StreetNumber}");
                        return;
                    case 3:
                        this.SetTableValue(cell, user.Email);
                        return;
                    case 4:
                        this.SetTableValue(cell, user.TelephoneNumber,true);
                        return;
                    case 5:
                        this.SetTableCellToContactTime(cell, tuple.Item3, tuple.Item4);
                        return;
                    case 6:
                        this.SetTableValue(cell, Lang.excel_contacts_showmore);
                        // Styles the cell as a link
                        ExcelUtils.LinkTo(cell, tuple.Item1);
                        return;
                }
            }
        }

        /// <summary>
        /// Prints a display of the user into the stylesheet using a horizontal-table
        /// </summary>
        /// <param name="sheet">The sheet to print on</param>
        /// <param name="user">The user's profile</param>
        /// <param name="title">The title of the table</param>
        /// <param name="startX">X-starting position of the table</param>
        /// <param name="startY">Y-starting position of the table</param>
        private void PrintUserInfos(ExcelWorksheet sheet, UserEntity user, string title, int startX, int startY)
        {

            // Header for the profile
            string[] header = {
                Lang.excel_user_vorname,
                Lang.excel_user_lastname,
                Lang.excel_user_location,
                Lang.excel_user_street,
                Lang.excel_user_email,
                Lang.excel_user_telephone
            };

            // Gets an array with the values to supply the table with
            string[] values = new string[] {
                user.Firstname,
                user.Lastname,
                $"{user.PLZ}/{user.Location}",
                $"{user.Street}. {user.StreetNumber}",
                user.Email,
                user.TelephoneNumber
            };

            // Creates the table
            ExcelUtils.PrintHorizontalTable(sheet, title, startX, startY, header, 1, SupplyCell);

            // Supply function for the cells
            void SupplyCell(int x, int y, ExcelRange cell)
            {
                // Checks for header
                if (x == 0)
                    return;

                // Sets the value
                this.SetTableValue(cell, values[y],y==5);
            }
        }

        /// <summary>
        /// Takes in an array with exposure times and returns the worst one.
        /// Either the smallest amount of time between arrival and leave if there was no direct contact
        /// or the most amount of time the persons had exposure to each other
        /// </summary>
        /// <param name="times">The times</param>
        private KeyValuePair<TimeSpan,bool> GetWorstExposureTime(KeyValuePair<TimeSpan, bool>[] times)
        {
            // Checks if the user had direct exposure at least once
            var hadDirectExposure = times.Any(x => !x.Value);

            // Gets the longest exposure time or smallest time a user has first contact with the aerosoles
            return times.Where(x=>hadDirectExposure!=x.Value).Aggregate((a, b) => hadDirectExposure ? (a.Key > b.Key ? a : b) : (a.Key < b.Key ? a : b));
        }

        /// <summary>
        /// Returns the amount of time the contact was exposed to the infected.
        /// If the contact had no direct exposure to the infected it returns the amount of tim between the arrival and leave of both persons.
        /// 
        /// The bool indecates if its only the aerosole's time (true) or direct exposure time (false)
        /// 
        /// </summary>
        /// <param name="contactInfo">The contact information</param>
        private KeyValuePair<TimeSpan, bool> GetOverlappingTimespan(ContactInfoEntity contactInfo)
        {
            // Gets the date-times
            DateTime
                startA = contactInfo.contactStartTime,
                endA = contactInfo.contactStopTime,
                startB = contactInfo.infectedStartTime,
                endB = contactInfo.infectedStopTime;

            // Ensures that a and b are in a correct order or calculation
            if (startA > startB)
            {
                // Swaps the variables
                (startB, startA) = (startA, startB);
                (endA, endB) = (endB, endA);
            }

            // Checks for no overlapping
            if (endA < startB)
                return new KeyValuePair<TimeSpan, bool>(startB - endA, true);

            // Checks for full overlapping
            if (endA > endB)
                return new KeyValuePair<TimeSpan, bool>(endB - startB, false);

            // The timespans are partly overlapping
            return new KeyValuePair<TimeSpan, bool>(endA - startB, false);

        }

    }
}
