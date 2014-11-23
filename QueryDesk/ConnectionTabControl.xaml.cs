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
using System.Xml;
using System.Reflection;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace QueryDesk
{
    public class QDConnectionFailedException : Exception
    {
    }

    public class QDConnectionTypeNotSupportedException : Exception
    {
    }

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConnectionTabControl : UserControl
    {
        private IAppDBServersAndQueries appDB;
        public IQueryableConnection DBConnection;
        private int connectionId = 0;

        private AppDBServerType type = AppDBServerType.Void;
        private string connectionString = string.Empty;

        private StoredQuery currentQuery = null;

        public ConnectionTabControl()
        {
            InitializeComponent();

            // Apply the SQL syntax highlighting definition
            edSQL.SyntaxHighlighting = HighlightingLoader.Load(XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("QueryDesk.Resources.SQL.xshd")), HighlightingManager.Instance);
        }

        public void SetDatabaseConnection(AppDBServerType type, string connectionString)
        {
            this.connectionString = connectionString;
            this.type = type;

            LoadConnectionSettings();
        }

        /// <summary>
        /// Initialize some Tab related things to align.
        /// </summary>
        public void Initialize(IAppDBServersAndQueries appDB, int serverId)
        {
            this.appDB = appDB;
            this.connectionId = serverId;

            btnEditQuery.IsEnabled = (appDB is IAppDBEditableQueries);
            btnDelQuery.IsEnabled = (appDB is IAppDBEditableQueries);
            btnAddQuery.IsEnabled = (appDB is IAppDBEditableQueries);

            var what = Content as Grid;
            what.Margin = new Thickness(0, 0, 0, 0);
            what.HorizontalAlignment = HorizontalAlignment.Stretch;
            what.VerticalAlignment = VerticalAlignment.Stretch;

            Reload();
        }

        private void Reload()
        {
            cmbQueries.ItemsSource = appDB.GetQueriesListing(connectionId);
            cmbQueries.DisplayMemberPath = "name";
            cmbQueries.SelectedValuePath = "id";
        }

        public void LoadConnectionSettings()
        {
            // todo: doesn't have to be MySQL, use some kind of factory that returns an interface to do queries with
            DBConnection = ConnectionFactory.NewConnection((int)type, connectionString);
            if (DBConnection == null)
            {
                throw new QDConnectionTypeNotSupportedException();
            }

            if (!DBConnection.Connect())
            {
                throw new QDConnectionFailedException();
            }
        }

        private string ProcessParameters(StoredQuery qry)
        {
            string exampleqrystring = qry.ToString();

            // Loop through query parameters
            foreach (var param in qry.Parameters)
            {
                // Replace defined parameter placeholder with given values
                exampleqrystring = exampleqrystring.Replace("?" + param.Key, "'" + param.Value + "'");
                exampleqrystring = exampleqrystring.Replace(":" + param.Key, "'" + param.Value + "'");
                exampleqrystring = exampleqrystring.Replace("@" + param.Key, "'" + param.Value + "'");
            }

            // Return parsed query
            return exampleqrystring;
        }

        private bool AskForParameters(StoredQuery qry)
        {
            // Check for query params
            if (qry.HasParameters())
            {
                // Display query param window
                var frm = new QueryParams();
                frm.SetQuery(qry);

                if (frm.ShowDialog() == true)
                {
                    frm.SaveParamsToQuery();
                    return true;
                }
            }
            else
            {
                // if there are no parameters, that's OK
                return true;
            }

            return false;
        }

        private void cmbQueries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = cmbQueries.SelectedItem;
            if (row != null)
            {
                var link = new AppDBQueryLink(row);
                currentQuery = null;
                edSQL.Text = link.sqltext;
            }
            else
            {
                currentQuery = null;
                edSQL.Text = string.Empty;
            }
        }

        private void btnGoQuery_Click(object sender, RoutedEventArgs e)
        {
            // parse query parameters
            var row = cmbQueries.SelectedItem;
            if (row != null)
            {
                var link = new AppDBQueryLink(row);
                if (currentQuery == null)
                {
                    currentQuery = new StoredQuery(link.sqltext);
                }

                // Aks for query parameters and save them
                if (!AskForParameters(currentQuery))
                {
                    // Return if parameter input was canceled
                    return;
                }

                currentQuery.RewriteParameters(DBConnection.GetParamPrefixChar());

                // Processs query parameters
                edSQL.Text = ProcessParameters(currentQuery);

                barQuery.Items.Clear();

                var hyjackquery = false;

                CExplainableQuery expl = QueryExplanationFactory.NewExplain(DBConnection, currentQuery);
                if (expl != null)
                {
                    if (expl.HasErrors())
                    {
                        MessageBox.Show(expl.GetErrorMsg(), "Query error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (!expl.IsAllIndexed())
                    {
                        if (MessageBox.Show("This query does not fully make use of indexes, are you sure you want to execute this query?", "Query warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            hyjackquery = true;
                        }
                    }
                    else if (expl.IsUsingBadStuff())
                    {
                        if (MessageBox.Show("This query could take a long time to run, are you sure you want to execute this query?", "Query warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            hyjackquery = true;
                        }
                    }
                    else if (expl.GetMaxResults() >= 65535)
                    {
                        if (MessageBox.Show("This query could possibly return a lot of rows, are you sure you want to execute this query?", "Query warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            hyjackquery = true;
                        }
                    }
                }

                if (hyjackquery)
                {
                    // execute the explain instead (only works for mysql queries)
                    var qryExplain = expl._get();

                    DBConnection.Query(qryExplain);
                }
                else
                {
                    // execute query and get result set
                    DBConnection.Query(currentQuery);
                }

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

                    string s = "Results: " + dt.Rows.Count;
                    barQuery.Items.Add(s);
                }
                else
                {
                    gridQueryResults.ItemsSource = null;
                    string s = "No results";
                    barQuery.Items.Add(s);
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
                frm.Initialize(link, DBConnection);

                bool? b = frm.ShowDialog();
                if (b == true)
                {
                    var editable = (IAppDBEditableQueries)appDB;
                    editable.SaveQuery(link);

                    // we don't have to do Reload() here, the object (row) should be edited directly, but we still need to reset CurrentQuery (used by GoQuery button)
                    cmbQueries_SelectionChanged(cmbQueries, null);
                }
            }
        }

        private void btnAddQuery_Click(object sender, RoutedEventArgs e)
        {
            var link = new AppDBQueryLink(new AppDBDummyQuery(0, connectionId, "New Query", string.Empty));
            var frm = new frmQueryEdit();
            frm.Initialize(link, DBConnection);

            bool? b = frm.ShowDialog();
            if (b == true)
            {
                var editable = (IAppDBEditableQueries)appDB;
                editable.SaveQuery(link);

                Reload();
            }
        }

        private void btnDelQuery_Click(object sender, RoutedEventArgs e)
        {
            var row = cmbQueries.SelectedItem;
            if (row != null)
            {
                var link = new AppDBQueryLink(row);

                // ask to be sure user hit the right button
                var r = MessageBox.Show("Are you sure you want to delete this query?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes)
                {
                    // AppDB needs to be editable in order to have saveQuery and delQuery functions,
                    //  but this button will be disabled if it's not editable, so we can just blindly do a typecast it here
                    var editable = (IAppDBEditableQueries)appDB;
                    editable.DelQuery(link);

                    Reload();
                }
            }
        }
    }
}
