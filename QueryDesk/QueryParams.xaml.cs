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
using System.Windows.Shapes;
using System.Xml;
using System.Reflection;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace QueryDesk
{
    /// <summary>
    /// Interaction logic for QueryParams.xaml
    /// </summary>
    public partial class QueryParams : Window
    {
        protected StoredQuery currentQuery;
        protected Dictionary<string, TextBox> paramBoxes = new Dictionary<string, TextBox>();

        public QueryParams()
        {
            InitializeComponent();

            // Apply the SQL syntax highlighting definition
            edQueryDescription.SyntaxHighlighting = HighlightingLoader.Load(XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("QueryDesk.Resources.SQL.xshd")), HighlightingManager.Instance);
        }

        protected void InitDescriptions()
        {
            edQueryDescription.Text = currentQuery.SQL;
        }

        protected void InitParams()
        {
            bool focusSet = false;
            int currentRow = 0;

            foreach (var param in currentQuery.Parameters)
            {
                Label lbl = new Label();
                lbl.Margin = new Thickness(0, -3, 0, 0);
                lbl.Content = param.Key;
                TextBox ed = new TextBox();
                RowDefinition rowDef = new RowDefinition();

                /*
                 note: can't bind, param.Value is read-only, need to set values by doing CurrentQuery.parameters["keyname"] = "new value";
                 ed.DataContext = param;
                 ed.SetBinding(TextBox.TextProperty, "Value");
                */

                if (param.Value == null)
                {
                    ed.Text = string.Empty;
                }
                else
                {
                    ed.Text = (string)param.Value;
                }

                paramBoxes.Add(param.Key, ed);

                // Set textbox height
                ed.Height = 23;

                // Add row definition with textbox height
                rowDef.Height = new GridLength(ed.Height, GridUnitType.Pixel);
                gridParams.RowDefinitions.Add(rowDef);

                // Assign label to grid
                Grid.SetRow(lbl, currentRow);
                Grid.SetRowSpan(lbl, 2);
                Grid.SetColumn(lbl, 0);

                // Assign edit box to grid
                Grid.SetRow(ed, currentRow);
                Grid.SetRowSpan(ed, 1);
                Grid.SetColumn(ed, 1);

                // Add elements to grid
                gridParams.Children.Add(lbl);
                gridParams.Children.Add(ed);

                // Increase row number
                currentRow++;

                if (!focusSet)
                {
                    focusSet = true;
                    ed.Focus();
                }
            }
        }

        public void SetQuery(StoredQuery qry)
        {
            currentQuery = qry;

            InitDescriptions();
            InitParams();
        }

        public void SaveParamsToQuery()
        {
            foreach (var key in paramBoxes.Keys)
            {
                TextBox ed = paramBoxes[key];
                currentQuery.Parameters[key] = ed.Text;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
