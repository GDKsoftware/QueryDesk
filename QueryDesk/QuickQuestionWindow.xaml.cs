using System;
using System.Windows;
using System.Windows.Input;

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
