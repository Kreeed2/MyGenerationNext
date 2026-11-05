using System.Collections;
using System.Text;

// Runtime surface that template code was written against (Zeus namespace of the legacy MyGeneration engine).
namespace Zeus
{
    public interface ILog
    {
        void Write(string text);
        void Write(string format, params object[] args);
        void Write(Exception ex);
    }

    public sealed class ZeusSimpleLog : ILog
    {
        public List<string> LogItems { get; } = [];
        public List<Exception> Exceptions { get; } = [];
        public event Action<string>? LogEntryAdded;

        public void Write(string text) { LogItems.Add(text); LogEntryAdded?.Invoke(text); }
        public void Write(string format, params object[] args) => Write(string.Format(format, args));
        public void Write(Exception ex) { Exceptions.Add(ex); Write(ex.ToString()); }
    }

    public interface IZeusInput
    {
        object? this[string key] { get; set; }
        bool Contains(string key);
        void Remove(string key);
        ICollection Keys { get; }
    }

    public sealed class ZeusInput : IZeusInput
    {
        private readonly Hashtable _items = new();
        public object? this[string key] { get => _items[key]; set => _items[key] = value; }
        public bool Contains(string key) => _items.ContainsKey(key);
        public void Remove(string key) => _items.Remove(key);
        public ICollection Keys => _items.Keys;
        public ICollection Values => _items.Values;
    }

    public interface IZeusOutput
    {
        void write(string text);
        void writeln(string text);
        void autoTab(string text);
        void autoTabLn(string text);
        void incTab();
        void decTab();
        int tabLevel { get; set; }
        string text { get; set; }
        void clear();
        void rollback(int num);
        void append(string path);
        void save(string path, object action);
        void saveEnc(string path, object action, object encoding);
        void setPreserveSource(string path, string prefix, string suffix);
        void preserve(string key);
        string getPreservedData(string key);
        string getPreserveBlock(string key);
        ICollection SavedFiles { get; }
    }

    /// <summary>The template output buffer (the 'output' object). Faithful port of the legacy ZeusOutput.</summary>
    public sealed class ZeusOutput : IZeusOutput
    {
        private const string PreserveBegin = "PRESERVE_BEGIN";
        private const string PreserveEnd = "PRESERVE_END";
        private const string Nl = "\r\n";

        private string _preservePrefix = "//::";
        private string _preserveSuffix = ":://";
        private Hashtable? _preserveData;
        private readonly ArrayList _savedFiles = [];
        private readonly StringBuilder _output = new();

        /// <summary>Optional redirection of all file writes (used for dry runs). Receives (path, content).</summary>
        public Action<string, string>? FileWriteHook { get; set; }

        public void rollback(int num) => _output.Length -= num;
        public void write(string text) => _output.Append(text);
        public void writeln(string text) => _output.Append(text).Append(Nl);
        public void autoTab(string text) => _output.Append('\t', Math.Max(0, tabLevel)).Append(text);
        public void autoTabLn(string text) { autoTab(text); _output.Append(Nl); }
        public void incTab() => tabLevel++;
        public void decTab() => tabLevel--;
        public int tabLevel { get; set; }
        public string text { get => _output.ToString(); set { clear(); _output.Append(value); } }
        public void clear() => _output.Clear();
        public ICollection SavedFiles => _savedFiles;

        // templates were written for Windows and build paths with backslashes
        private static string Norm(string path) =>
            Path.DirectorySeparatorChar == '\\' ? path : path.Replace('\\', Path.DirectorySeparatorChar);

        public void append(string path)
        {
            path = Norm(path);
            EnsureDirectory(path);
            File.AppendAllText(path, _output.ToString(), new UTF8Encoding(false));
        }

        public void save(string path, object action) => saveEnc(path, action, null!);

        public void saveEnc(string path, object action, object encoding)
        {
            path = Norm(path);
            var enc = encoding as Encoding ?? (encoding?.ToString()?.ToLowerInvariant()) switch
            {
                "ascii" => Encoding.ASCII,
                "unicode" => Encoding.Unicode,
                "bigendianunicode" => Encoding.BigEndianUnicode,
                "utf8" => new UTF8Encoding(true),
                _ => new UTF8Encoding(false),
            };

            var a = "o";
            if (action is bool b) a = b ? "b" : "o";
            else if (action is string s && s.Length > 0) a = s.ToLowerInvariant()[..1];

            var exists = File.Exists(path);
            bool write = true, appendMode = false;
            if (exists)
            {
                switch (a)
                {
                    case "b":
                        var i = 0;
                        string bak;
                        do bak = $"{path}.{++i:000}.bak"; while (File.Exists(bak));
                        File.Move(path, bak);
                        break;
                    case "o": File.Delete(path); break;
                    case "a": appendMode = true; break;
                    default: write = false; break; // "d": never overwrite
                }
            }
            if (!write) return;

            EnsureDirectory(path);
            using (var w = new StreamWriter(path, appendMode, enc)) w.Write(_output.ToString());
            if (!_savedFiles.Contains(path)) _savedFiles.Add(path);
            FileWriteHook?.Invoke(path, _output.ToString());
        }

        private static void EnsureDirectory(string path)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        }

        public void setPreserveSource(string path, string prefix, string suffix)
        {
            path = Norm(path);
            _preservePrefix = prefix;
            _preserveSuffix = suffix;
            _preserveData = new Hashtable();
            if (!File.Exists(path)) return;

            using var reader = new StringReader(MyGenerationNext.Core.Zeus.TemplateFileReader.ReadAllText(path));
            string beginTag = prefix + PreserveBegin, endTag = prefix + PreserveEnd;
            var inBlock = false;
            var key = "";
            var data = new StringBuilder();

            var line = reader.ReadLine();
            while (line != null)
            {
                if (inBlock)
                {
                    var i = line.IndexOf(endTag, StringComparison.Ordinal);
                    if (i >= 0)
                    {
                        data.Append(line.AsSpan(0, i));
                        _preserveData[key] = data.ToString();
                        data = new StringBuilder();
                        line = line[i..];
                        inBlock = false;
                        continue;
                    }
                    data.Append(line).Append(Nl);
                }
                else
                {
                    var i = line.IndexOf(beginTag, StringComparison.Ordinal);
                    if (i >= 0)
                    {
                        i += beginTag.Length;
                        var j = line.IndexOf(suffix, i, StringComparison.Ordinal);
                        if (j >= 0)
                        {
                            key = line[i..j].Trim();
                            line = line[(j + suffix.Length)..];
                            inBlock = true;
                            continue;
                        }
                    }
                }
                line = reader.ReadLine();
            }
        }

        public void preserve(string key) => _output.Append(getPreserveBlock(key));
        public string getPreservedData(string key) => _preserveData![key]!.ToString()!;

        public string getPreserveBlock(string key)
        {
            if (_preserveData is null) return "";
            var sb = new StringBuilder();
            sb.Append(_preservePrefix).Append(PreserveBegin).Append(' ').Append(key).Append(_preserveSuffix);
            if (_preserveData.ContainsKey(key)) sb.Append(getPreservedData(key));
            sb.Append(_preservePrefix).Append(PreserveEnd).Append(' ').Append(key).Append(_preserveSuffix);
            return sb.ToString();
        }
    }

    public interface IZeusContext
    {
        IZeusInput Input { get; }
        IZeusOutput Output { get; }
        Hashtable Objects { get; }
        ILog Log { get; }
    }

    public class ZeusTemplateContext : IZeusContext
    {
        public IZeusInput Input { get; protected set; } = new ZeusInput();
        public IZeusOutput Output { get; protected set; } = new ZeusOutput();
        public Hashtable Objects { get; protected set; } = new();
        public ILog Log { get; set; } = new ZeusSimpleLog();
    }

    public class ZeusGuiContext : ZeusTemplateContext
    {
        public UserInterface.GuiController Gui { get; protected set; } = new();
    }

    public class ZeusContext : ZeusGuiContext
    {
        public ZeusContext(IZeusOutput? output = null) { if (output is not null) Output = output; Objects["ui"] = Gui; }
    }
}

namespace Zeus.UserInterface
{
    /// <summary>Headless stand-in for the Zeus GUI controller; hosts supply input programmatically instead.</summary>
    public class GuiController
    {
        public string Title { get; set; } = "";
        public int Width { get; set; } = 400;
        public int Height { get; set; } = 300;
        public bool ShowGui { get; set; } = true;
        public bool ForceDisplay { get; set; }
        public bool IsCanceled { get; set; }
        public Hashtable Defaults { get; } = new();
    }
}

namespace Zeus.DotNetScript
{
    public abstract class _DotNetScriptTemplate
    {
        protected IZeusContext context;
        protected IZeusInput input;
        protected IZeusOutput output;
        protected Hashtable objects;

        protected _DotNetScriptTemplate(IZeusContext context)
        {
            this.context = context;
            input = context.Input;
            objects = context.Objects;
            output = context.Output;
        }

        public abstract void Render();
    }

    public abstract class _DotNetScriptGui
    {
        protected IZeusContext context;
        protected IZeusInput input;
        protected Hashtable objects;

        protected _DotNetScriptGui(IZeusContext context)
        {
            this.context = context;
            input = context.Input;
            objects = context.Objects;
        }

        public abstract void Setup();
    }
}
