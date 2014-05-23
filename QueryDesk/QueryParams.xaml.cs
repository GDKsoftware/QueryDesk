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

namespace QueryDesk
{
    /// <summary>
    /// Interaction logic for QueryParams.xaml
    /// </summary>
    public partial class QueryParams : Window
    {
        protected StoredQuery CurrentQuery;
        protected Dictionary<string, TextBox> ParamBoxes = new Dictionary<string, TextBox>();

        public QueryParams()
        {
            InitializeComponent();
        }

        protected void InitDescriptions()
        {
            lblQueryDescription.Text = CurrentQuery.SQL;
        }

        protected void InitParams()
        {
            bool focusSet = false;

            foreach (var param in CurrentQuery.parameters)
            {
                Label lbl = new Label();
                lbl.Margin = new Thickness(0,0,0,0);
                lbl.Content = param.Key;
                TextBox ed = new TextBox();

                // note: can't bind, param.Value is read-only, need to set values by doing CurrentQuery.parameters["keyname"] = "new value";
                //ed.DataContext = param;
                //ed.SetBinding(TextBox.TextProperty, "Value");

                if (param.Value == null)
                {
                    ed.Text = "";
                }
                else
                {
                    ed.Text = (string)param.Value;
                }

                ParamBoxes.Add(param.Key, ed);

                gridParams.RowDefinitions.Add(new RowDefinition());

                Grid.SetRow(lbl, 0);
                Grid.SetRowSpan(lbl, 1);
                Grid.SetColumn(lbl, 0);
                Grid.SetRow(ed, 0);
                Grid.SetRowSpan(ed, 1);
                Grid.SetColumn(ed, 1);

                gridParams.Children.Add(lbl);
                gridParams.Children.Add(ed);

                if (!focusSet)
                {
                    focusSet = true;
                    ed.Focus();
                }
            }
        }

        public void SetQuery(StoredQuery qry)
        {
            CurrentQuery = qry;

            InitDescriptions();
            InitParams();
        }

        public void SaveParamsToQuery()
        {
            foreach (var key in ParamBoxes.Keys)
            {
                TextBox ed = ParamBoxes[key];
                CurrentQuery.parameters[key] = ed.Text;
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
