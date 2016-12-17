using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Configuration;
using System.Data;

namespace QueryDesk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private IAppDBServersAndQueries appDB = null;
        private AppDBServerLink currentSelectedServerLink = new AppDBServerLink(new AppDBDummyServer());

        public MainWindow()
        {
            InitializeComponent();

            QueryComposerResources.Init();

            // datacontext of window is inherited by all controls
            this.DataContext = currentSelectedServerLink;

            // listsource for combobox
            cbType.ItemsSource = AppDBTypes.List();
            cbType.DisplayMemberPath = "Value";
            cbType.SelectedValuePath = "Key";

            if (LoadConnectionSettings())
            {
                RefreshConnectionList();
                EnableDisable();
            }
            else
            {
                // note: other options here http://stackoverflow.com/questions/2820357/how-to-exit-a-wpf-app-programmatically
                Application.Current.Shutdown();
            }
        }

        public bool LoadConnectionSettings()
        {
            string connstr = (string)ConfigurationManager.AppSettings["connection"];
            if (connstr != null)
            {
                // connect to database through connection string set in the App.config
                try
                {
                    appDB = new AppDBMySQL(connstr);

                    // AppDB = new AppDBDummy(connstr);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }
            else
            {
                connstr = (string)ConfigurationManager.AppSettings["connectionsqlite"];
                try
                {
                    appDB = new AppDBSQLite(connstr);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }

            return true;
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
                    ConnectToServer(connection);
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
            pnlEditServerInfo.IsEnabled = (appDB is IAppDBEditableServers);
            btnNewServer.IsEnabled = pnlEditServerInfo.IsEnabled;
            btnDeleteServer.IsEnabled = pnlEditServerInfo.IsEnabled;
        }

        /// <summary>
        /// Refresh or initialize the list of connections we configures
        /// </summary>
        private void RefreshConnectionList()
        {
            // when refreshing the list, the selected entry will most likely disappear
            long selectedid = currentSelectedServerLink.id;

            // make sure the interface won't link to non existing object
            currentSelectedServerLink.SetSource(new AppDBDummyServer());

            // set list items to query results
            lstConnections.ItemsSource = appDB.GetServerListing();
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
        private void ConnectToServer(AppDBServerLink connection)
        {
            try
            {
                var tabcontent = new ConnectionTabControl();

                tabcontent.Height = double.NaN;
                tabcontent.Width = double.NaN;

                tabcontent.Margin = new Thickness(0, 0, 0, 0);
                tabcontent.HorizontalAlignment = HorizontalAlignment.Stretch;
                tabcontent.VerticalAlignment = VerticalAlignment.Stretch;

                // setup the datasource to provide querynames
                tabcontent.Initialize(appDB, connection.id);

                // this also connects to the database and will throw an exception when we can't connect
                tabcontent.SetDatabaseConnection((AppDBServerType)connection.type, connection.GetConnectionString());

                // create a new tab with usercontrol instance and stretch align that to the tab
                var tab = new TabItem();

                var header = new CloseableTabHeader(connection.name);
                header.OnClose = () =>
                {
                    // implementation when x button is used on tab
                    pgTabs.Items.Remove(tab);
                    QueryComposerResources.UnsetComposerHelper(tabcontent.DBConnection);
                };
                tab.Header = header;
                tab.Content = tabcontent;

                pgTabs.Items.Add(tab);
                pgTabs.SelectedIndex = pgTabs.Items.IndexOf(tab);

                // if updatelayout() isn't used, recalculatesize() won't work
                pgTabs.UpdateLayout();
                header.RecalculateSize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connecting", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void lstConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // todo: check for unsaved settings

            // rebind to newly selected items
            var selection = lstConnections.SelectedItem;
            if (selection != null)
            {
                currentSelectedServerLink.SetSource(selection);
                edPassword.Password = currentSelectedServerLink.password;
            }
        }

        private void btnSaveServerSettings_Click(object sender, RoutedEventArgs e)
        {
            var editable = (IAppDBEditableServers)appDB;

            long id = currentSelectedServerLink.id;
            currentSelectedServerLink.password = edPassword.Password;
            if (editable.SaveServer(currentSelectedServerLink) != id)
            {
                RefreshConnectionList();
            }
        }

        private void btnNewServer_Click(object sender, RoutedEventArgs e)
        {
            currentSelectedServerLink.SetSource(new AppDBDummyServer());

            // Focus first input field
            cbType.Focus();
        }

        private void btnDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirmationResult;
            var editable = (IAppDBEditableServers)appDB;

            // Ask for confirmation before removing connection
            confirmationResult = MessageBox.Show("Are you sure you want to remove \"" + currentSelectedServerLink.name + "\"?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmationResult == MessageBoxResult.Yes)
            {
                // Remove selected connection
                editable.DelServer(currentSelectedServerLink);

                // Refresh connection list
                RefreshConnectionList();
            }
        }

        public void Dispose()
        {
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Close form
            Close();
        }

        private void TrayIconQueryDesk_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = true;
            WindowState = WindowState.Normal;
            this.Topmost = true;
        }
        
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }
    }
}
