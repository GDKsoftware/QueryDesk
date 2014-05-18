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
using System.Configuration;
using System.Data;

namespace QueryDesk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IAppDBServersAndQueries AppDB = null;
        private AppDBServerLink CurrentSelectedServerLink = new AppDBServerLink(new AppDBDummyServer());

        public MainWindow()
        {
            InitializeComponent();

            edName.DataContext = CurrentSelectedServerLink;
            edServer.DataContext = CurrentSelectedServerLink;
            edPort.DataContext = CurrentSelectedServerLink;
            edUsername.DataContext = CurrentSelectedServerLink;
            edDatabase.DataContext = CurrentSelectedServerLink;

            // note: passwordbox bindings are done manually
            //edPassword.DataContext = CurrentSelectedServerLink;

            LoadConnectionSettings();

            RefreshConnectionList();

            EnableDisable();
        }

        public void LoadConnectionSettings()
        {
            string connstr = (string)ConfigurationManager.AppSettings["connection"];

            // connect to database through connection string set in the App.config
            AppDB = new AppDBMySQL(connstr);
            //AppDB = new AppDBDummy(connstr);
        }

        private void lstConnections_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstConnections.SelectedIndex >= 0)
            {
                // determine selected connection
                var connection = new AppDBServerLink(lstConnections.SelectedItem);

                // todo: handle connection errors before opening new tab

                // connect to the right server
                Cursor = Cursors.Wait;
                try
                {
                    ConnectToServer(connection.id, connection.name, connection.getConnectionString());
                }
                finally
                {
                    Cursor = Cursors.Arrow;
                }
            }
        }

        private void EnableDisable()
        {
            // disable controls if AppDB implementation is readonly
            pnlEditServerInfo.IsEnabled = (AppDB is IAppDBEditableServers);
            btnNewServer.IsEnabled = pnlEditServerInfo.IsEnabled;
            btnDeleteServer.IsEnabled = pnlEditServerInfo.IsEnabled;
        }

        /// <summary>
        /// Refresh or initialize the list of connections we configures
        /// </summary>
        private void RefreshConnectionList()
        {
            // when refreshing the list, the selected entry will most likely disappear
            long selectedid = CurrentSelectedServerLink.id;

            // make sure the interface won't link to non existing object
            CurrentSelectedServerLink.SetSource(new AppDBDummyServer());

            // set list items to query results
            lstConnections.ItemsSource = AppDB.getServerListing();
            lstConnections.DisplayMemberPath = "name";
            lstConnections.SelectedValuePath = "id";

            int selectedidx = 0;

            if (selectedid > 0)
            {
                var item =
                    from row in lstConnections.ItemsSource.Cast<DataRowView>()
                    where (long)row["id"] == selectedid
                    select row;
            
                selectedidx = lstConnections.Items.IndexOf(item.Take(1));
            }

            if (selectedidx == -1)
            {
                selectedidx = 0;
            }

            lstConnections.SelectedIndex = selectedidx;
        }

        /// <summary>
        /// Open a new tab for the selected server, if we can connect to the server.
        /// </summary>
        /// <param name="connection_id">id</param>
        /// <param name="title">connection name to put in the tab header, should probably be accompanied by the id?</param>
        private void ConnectToServer(long connection_id, string title, string connectionstring)
        {
            try
            {
                var tabcontent = new ConnectionTabControl();

                tabcontent.Height = Double.NaN;
                tabcontent.Width = Double.NaN;

                tabcontent.Margin = new Thickness(0, 0, 0, 0);
                tabcontent.HorizontalAlignment = HorizontalAlignment.Stretch;
                tabcontent.VerticalAlignment = VerticalAlignment.Stretch;

                // setup the datasource to provide querynames
                tabcontent.Initialize(AppDB, connection_id);

                // this also connects to the database and will throw an exception when we can't connect
                tabcontent.setDatabaseConnection(connectionstring);

                // create a new tab with usercontrol instance and stretch align that to the tab
                var tab = new TabItem();
                tab.Header = title;
                tab.Content = tabcontent;

                pgTabs.Items.Add(tab);
                pgTabs.SelectedIndex = pgTabs.Items.IndexOf(tab);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lstConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // todo: check for unsaved settings

            // rebind to newly selected items
            var selection = lstConnections.SelectedItem;
            if (selection != null)
            {
                CurrentSelectedServerLink.SetSource(selection);
                edPassword.Password = CurrentSelectedServerLink.password;
            }
        }

        private void btnSaveServerSettings_Click(object sender, RoutedEventArgs e)
        {
            var editable = (IAppDBEditableServers)AppDB;

            long id = CurrentSelectedServerLink.id;
            CurrentSelectedServerLink.password = edPassword.Password;
            if (editable.saveServer(CurrentSelectedServerLink) != id)
            {
                RefreshConnectionList();
            }
        }

        private void btnNewServer_Click(object sender, RoutedEventArgs e)
        {
            CurrentSelectedServerLink.SetSource(new AppDBDummyServer());
        }

        private void btnDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            var editable = (IAppDBEditableServers)AppDB;

            editable.delServer(CurrentSelectedServerLink);

            RefreshConnectionList();
        }
    }
}


