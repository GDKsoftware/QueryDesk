using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Reflection;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Document;

namespace QueryDesk
{
    public static class QueryComposerResources
    {
        public static BitmapSource Table = null;
        public static BitmapSource Field = null;
        public static XshdSyntaxDefinition SQLHighlighter = null;
        public static IHighlightingDefinition SQLSyntaxHiglighting = null;
        internal static Dictionary<IQueryableConnection, QueryComposerHelper> Composers = new Dictionary<IQueryableConnection,QueryComposerHelper>();

        public static void Init()
        {
            if (Table == null)
            {
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QueryDesk.Resources.Table_748.png");
                PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                Table = decoder.Frames[0];
            }
            
            if (Field == null)
            {
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QueryDesk.Resources.Template_514.png");
                PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                Field = decoder.Frames[0];
            }

            if (SQLHighlighter == null)
            {
                var reader = XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("QueryDesk.Resources.SQL.xshd"));
                SQLHighlighter = HighlightingLoader.LoadXshd(reader);

                SQLSyntaxHiglighting = HighlightingLoader.Load(SQLHighlighter, HighlightingManager.Instance);
            }
        }

        public static QueryComposerHelper ComposerHelper(IQueryableConnection connection)
        {
            QueryComposerHelper composer;

            if (!Composers.TryGetValue(connection, out composer))
            {
                composer = new QueryComposerHelper(connection);
                Composers.Add(connection, composer);
            }

            return composer;
        }

        public static void UnsetComposerHelper(IQueryableConnection connection)
        {
            Composers.Remove(connection);
        }
    }

    public class CustomSegment: ISegment
    {
        private int os;
        private int le;
        private int eo;

        public CustomSegment(int start, int length)
        {
            os = start;
            le = length;
            eo = os + le;
        }

        public int EndOffset
        {
            get { return eo; }
        }

        public int Length
        {
            get { return le; }
        }

        public int Offset
        {
            get { return os; }
        }
    }

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

        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var word = QueryComposerHelper.ExtractPreviousWord(textArea.Document.Text, completionSegment.EndOffset - 1);

            ISegment replaceSegment = completionSegment;

            if (stringval.Substring(0, word.Length) == word)
            {
                replaceSegment = new CustomSegment(completionSegment.EndOffset - word.Length, word.Length);
            }

            textArea.Document.Replace(replaceSegment, stringval);
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
            get {
                if (this.type == "field")
                {
                    return QueryComposerResources.Field;
                }
                else if (this.type == "table")
                {
                    return QueryComposerResources.Table;
                }
                else
                {
                    return null;
                }
            }
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
        public string Currentword = "";

        public QueryComposerHelper(IQueryableConnection connection)
        {
            DBConnection = connection;

            InitializeLayout();
        }

        protected void InitializeLayout()
        {
            DBLayout = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

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
        public static string ExtractPreviousWord(string s, int pos)
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

            return w;
        }

        public static Tuple<string, int> ChainExtractPreviousWord(string s, int pos)
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
                            return new Tuple<string, int>(w.Substring(1, w.Length - 2), p);
                        }
                    }

                    return new Tuple<string, int>(w, p);
                }
                else
                {
                    w = c + w;
                }
                p--;
            }

            return new Tuple<string, int>(w, p);
        }

        public static string ExtractNextWord(string s, int pos)
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

            return w;
        }

        public static Tuple<string, int> ChainExtractNextWord(string s, int pos)
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
                                return new Tuple<string, int>(w.Substring(1, w.Length - 2), p);
                            }
                        }

                        return new Tuple<string,int>(w, p);
                    }
                }
                else
                {
                    w = w + c;
                }
                p++;
            }

            return new Tuple<string,int>(w, s.Length);
        }

        protected bool IsExistingTable(string s)
        {
            return DBLayout.Keys.Contains(s, StringComparer.OrdinalIgnoreCase);
        }

        public static string DetectSQLTableInQuery(string s)
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

        public bool IsSQLKeyWord(string word)
        {
            foreach (var elem1 in QueryComposerResources.SQLHighlighter.Elements.AsEnumerable<XshdElement>())
            {
                if (elem1 is XshdRuleSet)
                {
                    foreach (var elem2 in (elem1 as XshdRuleSet).Elements.AsEnumerable<XshdElement>())
                    {
                        if (elem2 is XshdKeywords)
                        {
                            return (elem2 as XshdKeywords).Words.Contains(word, StringComparer.OrdinalIgnoreCase);
                        }
                    }
                }
            }

            return false;
        }

        protected Dictionary<string, string> DetectAllSQLTablesInQuery(string sql)
        {
            var detected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var p = ChainExtractNextWord(sql, 0);
            while (p.Item2 < sql.Length)
            {
                var word = p.Item1;

                // possible occurences of alias
                //  "from tablename as alias"
                //  "from tablename alias"
                //  "join tablename as alias on"
                //  "join tablename alias on"

                if (word.Equals("from", StringComparison.OrdinalIgnoreCase) ||
                    word.Equals("into", StringComparison.OrdinalIgnoreCase) ||
                    word.Equals("update", StringComparison.OrdinalIgnoreCase) ||
                    word.Equals("join", StringComparison.OrdinalIgnoreCase))
                {
                    // first word behind from/into/etc is the tablename
                    p = ChainExtractNextWord(sql, p.Item2);
                    var tablename = p.Item1;

                    try
                    {
                        // after that we might have 'as' or an alias, but ... the next word might also be an sql keyword
                        p = ChainExtractNextWord(sql, p.Item2);
                        if (p.Item1.Equals("as", StringComparison.OrdinalIgnoreCase))
                        {
                            // we know 100% sure next word is an alias for this table
                            p = ChainExtractNextWord(sql, p.Item2);

                            detected.Add(p.Item1, tablename);
                        }
                        else
                        {
                            // ... now what
                            if (!IsSQLKeyWord(p.Item1))
                            {
                                detected.Add(p.Item1, tablename);
                            }
                            else
                            {
                                // add without alias
                                detected.Add(tablename, tablename);
                            }
                        }
                    }
                    catch (ArgumentException e)
                    {
                        // gets thrown if key already exists in dictionary, we shouldve probably checked that before adding, but we can also just ignore the exception
                    }

                }

                p = ChainExtractNextWord(sql, p.Item2);
            }

            return detected;
        }

        protected string GetTableNameForAlias(string sql, string alias)
        {
            var detected = DetectAllSQLTablesInQuery(sql);

            string tablename;
            if (detected.TryGetValue(alias, out tablename))
            {
                return tablename;
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
            Currentword = "";
            bool showtables = false;

            // if the character on/before the current cursor position is a dot, extract the word that's in front of it (likely a tablename)
            if (textarea.Document.Text[textarea.Document.Lines[line - 1].Offset + col - 2] == '.')
            {
                word = ExtractPreviousWord(textarea.Document.Text, textarea.Document.Lines[line - 1].Offset + col - 3);

                if (!IsExistingTable(word))
                {
                    // if word is not a tablename, it might be an alias for a table, if so, we need to find out the actual tablename
                    word = GetTableNameForAlias(textarea.Document.Text, word);
                }
            }
            else
            {
                Currentword = ExtractPreviousWord(textarea.Document.Text, textarea.Document.Lines[line - 1].Offset + col - 2);

                word = DetectSQLTableInQuery(textarea.Document.Text);
                showtables = true;
            }

            // loop over tables first to show fields of detected table
            if (word != "")
            {
                Dictionary<string, string> fields;
                if (DBLayout.TryGetValue(word, out fields))
                {
                    // word matches a tablename; list all fields in this table
                    foreach (var fieldname in fields)
                    {
                        data.Add(new QueryComposerCompletionData("field", fieldname.Key, fieldname.Value));
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
