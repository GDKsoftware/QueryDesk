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
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Reflection;

namespace QueryDesk
{
    /// <summary>
    /// Interaction logic for QueryEdit.xaml
    /// </summary>
    public partial class frmQueryEdit : Window
    {
        protected AppDBQueryLink CurrentQuery;

        public frmQueryEdit()
        {
            InitializeComponent();

            // Apply the SQL syntax highlighting definition
            edSQL.SyntaxHighlighting = HighlightingLoader.Load(XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("QueryDesk.Resources.SQL.xshd")), HighlightingManager.Instance);
        }

        public void Initialize(AppDBQueryLink linkQueryRow)
        {
            CurrentQuery = linkQueryRow;

            Reset();
        }

        private void Reset()
        {
            edShortDescription.Text = CurrentQuery.name;
            edSQL.Text = CurrentQuery.sqltext;
        }

        private void Save()
        {
            CurrentQuery.name = edShortDescription.Text;
            CurrentQuery.sqltext = edSQL.Text;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();

            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
