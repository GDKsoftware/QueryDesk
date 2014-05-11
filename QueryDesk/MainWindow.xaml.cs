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
        }

        public void LoadConnectionSettings()
        {
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
            //
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ConnectToServer(string servername)
        {
            var tab = new TabItem();
            tab.Header = servername;

            var tabcontent = new UserControl1();
            tab.Content = tabcontent;
            pgTabs.Items.Add(tab);

            tabcontent.Height = Double.NaN;
            tabcontent.Width = Double.NaN;

            tabcontent.Margin = new Thickness(0, 0, 0, 0);
            tabcontent.HorizontalAlignment = HorizontalAlignment.Stretch;
            tabcontent.VerticalAlignment = VerticalAlignment.Stretch;

            //adapter.SelectCommand = new MySqlCommand("select id, name from connection order by name asc", DB);

            MySqlDataAdapter adapter = new MySqlDataAdapter();
            var cmd = new MySqlCommand("select id, name from query where connection_id=?connection_id order by name asc", DB);
            cmd.Parameters.AddWithValue("?connection_id", 1);
            
            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "query");

            var dt = ds.Tables["query"];
            tabcontent.Initialize();
            tabcontent.setQuerySource(dt, "name");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ConnectToServer("Test DB");
        }
    }
}


