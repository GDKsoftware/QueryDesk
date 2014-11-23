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
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace QueryDesk
{
    /// <summary>
    /// Interaction logic for QueryEdit.xaml
    /// </summary>
    public partial class frmQueryEdit : Window
    {
        protected AppDBQueryLink currentQuery;
        protected CompletionWindow completionWindow;
        protected QueryComposerHelper completionHelper;

        public frmQueryEdit()
        {
            InitializeComponent();

            // Apply the SQL syntax highlighting definition
            edSQL.SyntaxHighlighting = QueryComposerResources.SQLSyntaxHiglighting;

            edSQL.TextArea.TextEntering += edSQL_TextArea_TextEntering;
            edSQL.TextArea.TextEntered += edSQL_TextArea_TextEntered;
        }

        private void edSQL_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                edSQL_TextArea_Sense(sender, e);
            }
        }

        private void edSQL_TextArea_Sense(object sender, TextCompositionEventArgs e)
        {
            completionWindow = new CompletionWindow(edSQL.TextArea);
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

            // fill data (completion options) based on e (position, context etc)
            completionHelper.Initialize(e, data);
            completionWindow.Show();
            completionWindow.CompletionList.IsFiltering = false;
            completionWindow.CompletionList.SelectItem(completionHelper.Currentword);
            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };
        }

        private void edSQL_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            var m = ((System.Windows.Input.KeyboardDevice)(((System.Windows.Input.InputEventArgs)(e)).Device)).Modifiers;
            if (e.Text == " " && (m.HasFlag(ModifierKeys.Control)))
            {
                // ctrl+space opens completion window without typing the space in the editor
                edSQL_TextArea_Sense(sender, e);
                e.Handled = true;
            }
            else if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }

            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        public void Initialize(AppDBQueryLink linkQueryRow, IQueryableConnection connection)
        {
            currentQuery = linkQueryRow;
            completionHelper = QueryComposerResources.ComposerHelper(connection);

            Reset();
        }

        private void Reset()
        {
            edShortDescription.Text = currentQuery.name;
            edSQL.Text = currentQuery.sqltext;

            // Set form title
            SetTitle();

            // Focus first input field
            FocusFirstInput();
        }

        private void FocusFirstInput()
        {
            // Focus first input field
            if (currentQuery.id > 0)
            {
                edSQL.Focus();
                edSQL.CaretOffset = edSQL.Text.Length;
            }
            else
            {
                edShortDescription.Focus();
                edShortDescription.SelectAll();
            }
        }

        private void SetTitle()
        {
            // Set form title depending on edit state
            Title = currentQuery.id > 0 ? "Edit query" : "Add query";
        }

        private void Save()
        {
            currentQuery.name = edShortDescription.Text;
            currentQuery.sqltext = edSQL.Text;
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
