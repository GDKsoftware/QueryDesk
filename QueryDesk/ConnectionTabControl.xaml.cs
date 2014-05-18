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
using System.Data;

namespace QueryDesk
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConnectionTabControl : UserControl
    {
        private IAppDBServersAndQueries AppDB;
        
        private string dbconnectionstring = "";
        private MySqlConnection DB = null;

        public ConnectionTabControl()
        {
            InitializeComponent();
        }
        
        public void setDatabaseConnection(string connstr)
        {
            dbconnectionstring = connstr;

            LoadConnectionSettings();
        }

        /// <summary>
        /// Initialize some Tab related things to align.
        /// </summary>
        public void Initialize(IAppDBServersAndQueries AppDB, long server_id)
        {
            this.AppDB = AppDB;

            btnEditQuery.IsEnabled = (AppDB is IAppDBEditableQueries);

            var what = Content as Grid;
            what.Margin = new Thickness(0, 0, 0, 0);
            what.HorizontalAlignment = HorizontalAlignment.Stretch;
            what.VerticalAlignment = VerticalAlignment.Stretch;

            cmbQueries.ItemsSource = AppDB.getQueriesListing(server_id);
            cmbQueries.DisplayMemberPath = "name";
            cmbQueries.SelectedValuePath = "id";
        }

        public void LoadConnectionSettings()
        {
            // todo: doesn't have to be MySQL, use some kind of factory that returns an interface to do queries with
            try
            {
                DB = new MySqlConnection(dbconnectionstring);

                DB.Open(); // throws exception if failed to connect
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                throw;
            }
        }

        private string AskForParameters(StoredQuery qry)
        {
            var exampleqrystring = qry.ToString();

            // offer way to enter parameters

            // note: can't foreach this, because we edit the values inside this loop
            for (int i = 0; i < qry.parameters.Count; i++)
            {
                var key = qry.parameters.Keys.ElementAt<string>(i);
                //var value = qry.parameters[key];

                var answer = "";

                // todo: put all parameters together with a dynamically setup form

                // for now ask for parameter values 1 by 1
                var askparam = new QuickQuestionWindow();
                askparam.Question = key + " ?";

                bool? b = askparam.ShowDialog();    // ok button lets this 'ShowModal' returns true
                if (b == true)
                {
                    answer = askparam.Answer();
                }

                qry.parameters[key] = answer;

                // replace stuff directly in the query as an example sql text
                exampleqrystring = exampleqrystring.Replace("?" + key, "'" + answer + "'");
                exampleqrystring = exampleqrystring.Replace(":" + key, "'" + answer + "'");
            }

            return exampleqrystring;
        }

        private void cmbQueries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = cmbQueries.SelectedItem;
            if (row != null)
            {
                var link = new AppDBQueryLink(row);
                edSQL.Text = link.sqltext;
            }
        }

        private void btnGoQuery_Click(object sender, RoutedEventArgs e)
        {
            // parse query parameters
            var row = cmbQueries.SelectedItem;
            if (row != null)
            {
                var link = new AppDBQueryLink(row);
                var qry = new StoredQuery(link.sqltext);

                edSQL.Text = AskForParameters(qry);

                // display results in datagrid

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                var cmd = new MySqlCommand(qry.SQL, DB);
                foreach (var p in qry.parameters)
                {
                    cmd.Parameters.AddWithValue(p.Key, p.Value);
                }
                adapter.SelectCommand = cmd;

                DataSet ds = new DataSet();

                try
                {
                    adapter.Fill(ds, "query");
                }
                catch (Exception x)
                {
                    // todo: handle query errors in a better way
                    MessageBox.Show(x.Message);
                    return;
                }

                var dt = ds.Tables["query"];

                gridQueryResults.AutoGenerateColumns = true;
                gridQueryResults.ItemsSource = dt.DefaultView;
            }
        }

        private void btnEditQuery_Click(object sender, RoutedEventArgs e)
        {
            var row = cmbQueries.SelectedItem;
            if (row != null)
            {
                var link = new AppDBQueryLink(row);

                var frm = new frmQueryEdit();
                frm.Initialize(link);

                bool? b = frm.ShowDialog();
                if (b == true)
                {
                    var editable = (IAppDBEditableQueries)AppDB;
                    editable.saveQuery(link);

                    edSQL.Text = link.sqltext;
                }
            }
        }
    }
}
