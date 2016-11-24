using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Xml;
using System.Reflection;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

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
        private long connectionId = 0;

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
        public void Initialize(IAppDBServersAndQueries appDB, long serverId)
        {
            this.appDB = appDB;
            this.connectionId = serverId;

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
            btnRefreshQuery.IsEnabled = false;

            var row = cmbQueries.SelectedItem;
            if (row != null)
            {
                var link = new AppDBQueryLink(row);
                currentQuery = null;
                edSQL.Text = link.sqltext;

                // Disable query action buttons
                enableQueryActionControls(true);
            }
            else
            {
                currentQuery = null;
                edSQL.Text = string.Empty;

                // Disable query action buttons
                enableQueryActionControls(false);
            }
        }

        /// <summary>
        /// Enable or disable query action controls
        /// </summary>
        /// <param name="enable"></param>
        private void enableQueryActionControls(Boolean enable)
        {
            // Enable/disable query action buttons
            btnEditQuery.IsEnabled = enable;
            btnDelQuery.IsEnabled = enable;
            btnGoQuery.IsEnabled = enable;
        }

        private void btnGoQuery_Click(object sender, RoutedEventArgs e)
        {
            btnRefreshQuery.IsEnabled = false;

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

                    // Enable Refresh query button
                    btnRefreshQuery.IsEnabled = true;

                    // Enable Feed to action
                    btnFeedToAction.IsEnabled = true;
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

        private void btnRefreshQuery_Click(object sender, RoutedEventArgs e)
        {
            barQuery.Items.Clear();

            DBConnection.Query(currentQuery);

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
                    btnRefreshQuery.IsEnabled = false;

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
                btnRefreshQuery.IsEnabled = false;

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
                    btnRefreshQuery.IsEnabled = false;

                    // AppDB needs to be editable in order to have saveQuery and delQuery functions,
                    //  but this button will be disabled if it's not editable, so we can just blindly do a typecast it here
                    var editable = (IAppDBEditableQueries)appDB;
                    editable.DelQuery(link);

                    Reload();
                }
            }
        }
        
        private void btnFeedToAction_Click(object sender, RoutedEventArgs e)
        {
            ActionsWindow frmActionsWindow = new ActionsWindow();

            bool? DialogResult = frmActionsWindow.ShowDialog();

            if (DialogResult == true)
            {
                // initialize status/progression indicator
                while (barQuery.Items.Count < 2) {
                    barQuery.Items.Add("");
                }

                barQuery.Items[1] = "Actions performed: 0";

                // todo: make classes to handle these things elegantly

                if (frmActionsWindow.SelectedActionType() == "Call URL")
                {
                    string urltemplate = frmActionsWindow.GetActionParams();

                    // get parameters through regular expression (format url like "http://test.com/test.php?id={{id}}&name={{name}}" )
                    var urlparams = Regex.Matches(urltemplate, "(?<={{).*?(?=}})");

                    if (gridQueryResults.ItemsSource != null)
                    {
                        var recno = 0;

                        // logfile
                        var logfile = new StreamWriter("actionresults.txt");
                        logfile.WriteLine("Starting actions.");

                        DataView dt = (DataView)gridQueryResults.ItemsSource;
                        foreach (DataRowView row in dt)
                        {
                            string urlrow = urltemplate;

                            // replace all bracketted parameters in url with the fieldvalues in the current row 
                            foreach (Match param in urlparams)
                            {
                                string p = param.Value;

                                urlrow = urlrow.Replace("{{" + p + "}}", WebUtility.UrlEncode(row.Row[p].ToString()));
                            }

                            logfile.WriteLine("Calling URL: " + urlrow + "");

                            var request = WebRequest.Create(urlrow);
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                            // log status (200 OK, 404 File not found, etc)
                            logfile.WriteLine("Response Status: " + response.StatusDescription);

                            // read data in response and log that in our logfile
                            var dataStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(dataStream);
                            string responseFromServer = reader.ReadToEnd ();
                            logfile.WriteLine(responseFromServer);

                            response.Close();
                            
                            // continue

                            recno++;
                            barQuery.Items[1] = "Actions performed: " + recno;
                        }

                        logfile.WriteLine("Actions done.");

                        logfile.Close();
                    }
                }
            }
        }
    }
}
