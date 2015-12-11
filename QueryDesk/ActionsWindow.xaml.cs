using System;
using System.Windows;

namespace QueryDesk
{
    /// <summary>
    /// Interaction logic for ActionsWindow.xaml
    /// </summary>
    public partial class ActionsWindow : Window
    {
        public ActionsWindow()
        {
            InitializeComponent();
        }

        public string SelectedActionType()
        {
            return cbActionType.Text;
        }

        public string GetActionParams()
        {
            return edActionParameters.Text;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void FormActionsWindow_Activated(object sender, EventArgs e)
        {
            cbActionType.SelectedIndex = 0;
        }
    }
}
