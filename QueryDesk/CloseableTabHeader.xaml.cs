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

namespace QueryDesk
{
    /// <summary>
    /// Interaction logic for CloseableTabHeader.xaml
    /// </summary>
    public partial class CloseableTabHeader : UserControl
    {
        public Action OnClose;

        public CloseableTabHeader(string title)
        {
            InitializeComponent();

            lblTabTitle.Content = title;
            this.Width = lblTabTitle.RenderSize.Width + 50;
        }

        public void CloseTab()
        {
            if (OnClose != null)
            {
                OnClose();
            }
        }

        private void btnTabClose_Click(object sender, RoutedEventArgs e)
        {
            CloseTab();
        }
    }
}
