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
    /// Interaction logic for QuickQuestionWindow.xaml
    /// </summary>
    public partial class QuickQuestionWindow : Window
    {
        public string Question
        {
            get { return (string)lblQuestion.Content; }
            set { lblQuestion.Content = value; }
        }

        public Func<string> Answer = () => string.Empty;

        public QuickQuestionWindow()
        {
            InitializeComponent();

            Answer = () => edAnswer.Text;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void edAnswer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnOk_Click(btnOk, null);
            }
        }
    }
}
