﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows;

namespace QueryDesk
{
    public class QueryComposerCompletionData: ICompletionData
    {
        protected string stringval;
        protected string type;
        protected string desc;

        public QueryComposerCompletionData(string type, string strval, string desc)
        {
            this.type = type;
            this.stringval = strval;
            this.desc = desc;
        }

        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        public object Content
        {
            get { return stringval; }
        }

        public object Description
        {
            get { return desc; }
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public double Priority
        {
            get { return 0; }
        }

        public string Text
        {
            get { return stringval; }
        }
    }

    public class QueryComposerHelper
    {
        protected IQueryableConnection DBConnection = null;
        protected Dictionary<string, Dictionary<string, string>> DBLayout = null;

        public QueryComposerHelper(IQueryableConnection connection)
        {
            DBConnection = connection;

            InitializeLayout();
        }

        protected void InitializeLayout()
        {
            DBLayout = new Dictionary<string, Dictionary<string, string>>();

            foreach (var tablename in DBConnection.ListTableNames())
            {
                var fields = DBConnection.ListFieldNames(tablename);
                DBLayout.Add(tablename, fields);
            }
        }

        /// <summary>
        /// Look for a word in s before character index pos
        /// </summary>
        /// <param name="s"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected string ExtractPreviousWord(string s, int pos)
        {
            string w = "";
            int p = pos;
            while (p >= 0)
            {
                var c = s[p];
                // word separators; space, dot, comma, tab, enter
                if ((c == ' ') || (c == '.') || (c == ',') || (c == '<') || (c == '>') || (c == '=') || (c == '(') || (c == ')') || (c == 7) || (c == 10) || (c == 13))
                {
                    if (w.StartsWith("`") || w.StartsWith("["))
                    {
                        if (w.EndsWith("`") || w.EndsWith("]"))
                        {
                            return w.Substring(1, w.Length - 2);
                        }
                    }

                    return w;
                }
                else
                {
                    w = c + w;
                }
                p--;
            }

            return "";
        }

        protected string ExtractNextWord(string s, int pos)
        {
            string w = "";
            int p = pos;
            while (p < s.Length)
            {
                var c = s[p];
                // word separators; space, dot, comma, tab, enter
                if ((c == ' ') || (c == '.') || (c == ',') || (c == '<') || (c == '>') || (c == '=') || (c == '(') || (c == ')') || (c == 7) || (c == 10) || (c == 13))
                {
                    if (w.Trim() != "")
                    {
                        if (w.StartsWith("`") || w.StartsWith("["))
                        {
                            if (w.EndsWith("`") || w.EndsWith("]"))
                            {
                                return w.Substring(1, w.Length - 2);
                            }
                        }

                        return w;
                    }
                }
                else
                {
                    w = w + c;
                }
                p++;
            }

            return "";
        }

        protected string DetectSQLTableInQuery(string s)
        {
            var iFrom   = s.IndexOf("from", StringComparison.OrdinalIgnoreCase);
            var iInto   = s.IndexOf("into", StringComparison.OrdinalIgnoreCase);
            var iUpdate = s.IndexOf("update", StringComparison.OrdinalIgnoreCase);

            if (iFrom != -1)
            {
                return ExtractNextWord(s, iFrom + 4);
            }
            else if (iInto != -1)
            {
                return ExtractNextWord(s, iInto + 4);
            }
            else if (iUpdate != -1)
            {
                return ExtractNextWord(s, iUpdate + 6);
            }

            return "";
        }

        /// <summary>
        /// Add a list of completion options to data, based on current cursor position in textarea
        /// </summary>
        /// <param name="e">Textarea (AvalonEdit only) and cursor information</param>
        /// <param name="data"></param>
        public void Initialize(TextCompositionEventArgs e, IList<ICompletionData> data)
        {
            //e.Source
            var textarea = (TextArea)(((RoutedEventArgs)(e)).Source);
            var caret = textarea.Caret;
            var line = caret.Location.Line;
            var col = caret.Location.Column;

            string word = "";
            bool showtables = false;

            // if the character on/before the current cursor position is a dot, extract the word that's in front of it (likely a tablename)
            if (textarea.Document.Text[textarea.Document.Lines[line - 1].Offset + col - 2] == '.')
            {
                word = ExtractPreviousWord(textarea.Document.Text, textarea.Document.Lines[line - 1].Offset + col - 3);
            }
            else
            {
                word = DetectSQLTableInQuery(textarea.Document.Text);
                showtables = true;
            }

            // loop over tables first to show fields of detected table
            if (word != "")
            {
                foreach (var tablename in DBLayout.Keys)
                {
                    if (word.Equals(tablename, StringComparison.OrdinalIgnoreCase))
                    {
                        // word matches a tablename; list all fields in this table
                        foreach (var fieldname in DBLayout[tablename])
                        {
                            data.Add(new QueryComposerCompletionData("field", fieldname.Key, fieldname.Value));
                        }
                    }
                }
            }

            if (showtables)
            {
                // loop over list again to show tablenames
                foreach (var tablename in DBLayout.Keys)
                {
                    data.Add(new QueryComposerCompletionData("table", tablename, null));
                }
            }
        }
    }
}
