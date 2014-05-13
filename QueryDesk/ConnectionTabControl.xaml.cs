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
        public ConnectionTabControl()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Fill combobox with items from datatable.
        /// </summary>
        /// <param name="dt">DataTable with query names</param>
        /// <param name="field">Display text/short description fieldname</param>
        public void setQuerySource(DataTable dt, string field)
        {
            cmbQueries.ItemsSource = dt.DefaultView;
            cmbQueries.DisplayMemberPath = field;
            cmbQueries.SelectedValuePath = "id";
        }

        /// <summary>
        /// Initialize some Tab related things to align.
        /// </summary>
        public void Initialize()
        {
            var what = Content as Grid;
            what.Margin = new Thickness(0, 0, 0, 0);
            what.HorizontalAlignment = HorizontalAlignment.Stretch;
            what.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private string AskForParameters(StoredQuery qry)
        {
            var exampleqrystring = qry.ToString();

            // offer way to enter parameters

            // todo: qry.parameters should be a dictonary with keys (parameter name) and values (parameter value)
            //  so that the StoredQuery will contain the parameter values to bind to the real queries
            foreach (var p in qry.parameters)
            {
                var answer = "";

                // todo: put all parameters together with a dynamically setup form

                // for now ask for parameter values 1 by 1
                var askparam = new QuickQuestionWindow();
                askparam.Question = p + " ?";

                bool? b = askparam.ShowDialog();    // ok button lets this 'ShowModal' returns true
                if (b == true)
                {
                    answer = askparam.Answer();
                }

                // replace stuff directly in the query as an example sql text
                exampleqrystring = exampleqrystring.Replace("?" + p, "'" + answer + "'");
                exampleqrystring = exampleqrystring.Replace(":" + p, "'" + answer + "'");
            }

            return exampleqrystring;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // parse query parameters
            var row = (DataRowView)(cmbQueries.SelectedItem);
            var qry = new StoredQuery((string)row.Row["sqltext"]);

            edSQL.Text = AskForParameters(qry);

            // display results in datagrid
        }
    }
}
