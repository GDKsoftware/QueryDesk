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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // parse query parameters

            // offer way to enter parameters

            // open query

            // display results in datagrid
        }
    }
}
