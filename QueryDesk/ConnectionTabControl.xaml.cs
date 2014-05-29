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
    class QDConnectionFailedException : Exception { };
    class QDConnectionTypeNotSupportedException : Exception { };

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConnectionTabControl : UserControl
    {
        private IAppDBServersAndQueries AppDB;
        private IQueryableConnection DBConnection;
        private int connection_id = 0;

        private AppDBServerType dbtype = AppDBServerType.Void;
        private string dbconnectionstring = "";

        private StoredQuery CurrentQuery = null;

        public ConnectionTabControl()
        {
            InitializeComponent();
        }
        
        public void setDatabaseConnection(AppDBServerType type, string connstr)
        {
            dbconnectionstring = connstr;
            dbtype = type;

            LoadConnectionSettings();
        }

        /// <summary>
        /// Initialize some Tab related things to align.
        /// </summary>
        public void Initialize(IAppDBServersAndQueries AppDB, int server_id)
        {
            this.AppDB = AppDB;
            this.connection_id = server_id;

            btnEditQuery.IsEnabled = (AppDB is IAppDBEditableQueries);

            var what = Content as Grid;
            what.Margin = new Thickness(0, 0, 0, 0);
            what.HorizontalAlignment = HorizontalAlignment.Stretch;
            what.VerticalAlignment = VerticalAlignment.Stretch;

            Reload();
        }

        private void Reload()
        {
            cmbQueries.ItemsSource = AppDB.getQueriesListing(connection_id);
            cmbQueries.DisplayMemberPath = "name";
            cmbQueries.SelectedValuePath = "id";
        }

        public void LoadConnectionSettings()
        {
            // todo: doesn't have to be MySQL, use some kind of factory that returns an interface to do queries with
            DBConnection = ConnectionFactory.NewConnection((int)dbtype, dbconnectionstring);
            if (DBConnection == null)
            {
                throw new QDConnectionTypeNotSupportedException();
            }

            if (!DBConnection.Connect())
            {
                throw new QDConnectionFailedException();
            }
        }

        private string AskForParameters(StoredQuery qry)
        {
            string exampleqrystring = null;

            var frm = new QueryParams();
            frm.SetQuery(qry);
            bool? b = frm.ShowDialog();
            if (b == true)
            {
                frm.SaveParamsToQuery();

                exampleqrystring = qry.ToString();

                foreach (var param in qry.parameters)
                {
                    exampleqrystring = exampleqrystring.Replace("?" + param.Key, "'" + param.Value + "'");
                    exampleqrystring = exampleqrystring.Replace(":" + param.Key, "'" + param.Value + "'");
                }
            }


/*
            // old way to enter parameters 1 by 1

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
*/

            return exampleqrystring;
        }

        private void cmbQueries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = cmbQueries.SelectedItem;
            if (row != null)
            {
                var link = new AppDBQueryLink(row);
                CurrentQuery = null;
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
                if (CurrentQuery == null)
                {
                    CurrentQuery = new StoredQuery(link.sqltext);
                }

                edSQL.Text = AskForParameters(CurrentQuery);

                // execute query and get result set

                DBConnection.Query(CurrentQuery);

                DataTable dt;
                try
                {
                    // todo: datatable contains all results, no cursor/rowtravel/stream
                    dt = DBConnection.ResultsAsDataTable();
                }
                catch (Exception x)
                {
                    // todo: handle query errors in a better way
                    MessageBox.Show(x.Message);
                    return;
                }

                // display results in datagrid
                gridQueryResults.AutoGenerateColumns = true;

                if (dt != null)
                {
                    gridQueryResults.ItemsSource = dt.DefaultView;
                }
                else
                {
                    gridQueryResults.ItemsSource = null;
                }
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

        private void btnAddQuery_Click(object sender, RoutedEventArgs e)
        {
            var link = new AppDBQueryLink(new AppDBDummyQuery(0, connection_id, "New Query", ""));
            var frm = new frmQueryEdit();
            frm.Initialize(link);

            bool? b = frm.ShowDialog();
            if (b == true)
            {
                var editable = (IAppDBEditableQueries)AppDB;
                editable.saveQuery(link);

                Reload();
            }
        }
    }
}
