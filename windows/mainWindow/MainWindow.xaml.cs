using projektlabor.covid19login.adminpanel.connection.requests;
using projektlabor.covid19login.adminpanel.Properties.langs;
using projektlabor.covid19login.adminpanel.windows.configWindow;
using projektlabor.covid19login.adminpanel.windows.requests;
using System.Windows;
using projektlabor.covid19login.adminpanel.utils;
using projektlabor.covid19login.adminpanel.windows.dialogs;
using System;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.connection;

namespace projektlabor.covid19login.adminpanel.windows.mainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(SimpleAdminEntity profile, Config config, RequestData credentials, long authCode)
        {
            InitializeComponent();
        }
    }
}
