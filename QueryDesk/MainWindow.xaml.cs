using MySql.Data.MySqlClient;
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
        private MySqlConnection DB = null;

        public MainWindow()
        {
            InitializeComponent();

            LoadConnectionSettings();

            RefreshConnectionList();
        }

        public void LoadConnectionSettings()
        {
            // connect to database through connection string set in the App.config
            try
            {
                string connstr = (string)ConfigurationManager.AppSettings["connection"];
                DB = new MySqlConnection(connstr);

                DB.Open(); // throws exception if failed to connect
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lstConnections_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstConnections.SelectedIndex >= 0)
            {
                // determine selected connection
                int id = (int)lstConnections.SelectedValue;
                var row = (DataRowView)(lstConnections.SelectedItem);
                string title = (string)row.Row["name"];

                // connect to the right server
                ConnectToServer(id, title);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Refresh or initialize the list of connections we configures
        /// </summary>
        private void RefreshConnectionList()
        {
            //  todo: make interface to provide data, doesn't have to come from a database.

            MySqlDataAdapter adapter = new MySqlDataAdapter();
            var cmd = new MySqlCommand("select id, name from connection order by name asc", DB);

            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "connection");

            var dt = ds.Tables["connection"];

            // set list items to query results
            lstConnections.ItemsSource = dt.DefaultView;
            lstConnections.DisplayMemberPath = "name";
            lstConnections.SelectedValuePath = "id";
        }

        /// <summary>
        /// Open a new tab for the selected server.
        /// </summary>
        /// <param name="connection_id">id</param>
        /// <param name="title">connection name to put in the tab header, should probably be accompanied by the id?</param>
        private void ConnectToServer(int connection_id, string title)
        {
            // create a new tab with usercontrol instance and stretch align that to the tab
            var tab = new TabItem();
            tab.Header = title;

            var tabcontent = new ConnectionTabControl();
            tab.Content = tabcontent;
            pgTabs.Items.Add(tab);

            tabcontent.Height = Double.NaN;
            tabcontent.Width = Double.NaN;

            tabcontent.Margin = new Thickness(0, 0, 0, 0);
            tabcontent.HorizontalAlignment = HorizontalAlignment.Stretch;
            tabcontent.VerticalAlignment = VerticalAlignment.Stretch;


            // setup the datasource to provide querynames

            //  todo: make interface to provide data, doesn't have to come from a database

            MySqlDataAdapter adapter = new MySqlDataAdapter();
            var cmd = new MySqlCommand("select id, name, sqltext from query where connection_id=?connection_id order by name asc", DB);
            cmd.Parameters.AddWithValue("?connection_id", connection_id);
            
            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "query");

            var dt = ds.Tables["query"];
            tabcontent.Initialize();
            tabcontent.setQuerySource(dt, "name");

            pgTabs.SelectedIndex = pgTabs.Items.IndexOf(tab);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // test, this should happen when you double click a server entry
            //ConnectToServer("Test DB");
        }
    }
}


