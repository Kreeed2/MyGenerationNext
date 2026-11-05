using System.Collections;
using MyGenerationNext.Core;

// Lean, read-only compatibility layer for the legacy MyMeta API that the ZESA templates are written against,
// backed by the SQL Server metadata read into DatabaseInfo.
namespace MyMeta
{
    public enum ParamDirection { Unknown = 0, Input = 1, Output = 2, InputOutput = 3, ReturnValue = 4 }

    public interface IMetaCollection<out T> : IEnumerable<T>
    {
        int Count { get; }
        T this[int index] { get; }
        T this[string name] { get; }
    }

    public class MetaCollection<T> : IMetaCollection<T> where T : class
    {
        private readonly List<T> _items;
        private readonly Func<T, string> _name;

        public MetaCollection(IEnumerable<T> items, Func<T, string> name) { _items = items.ToList(); _name = name; }
        public int Count => _items.Count;
        public T this[int index] => _items[index];
        public T this[string name] => _items.FirstOrDefault(i => string.Equals(_name(i), name, StringComparison.OrdinalIgnoreCase))!;
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public interface IColumns : IMetaCollection<IColumn> { }
    public interface ITables : IMetaCollection<ITable> { }
    public interface IViews : IMetaCollection<IView> { }
    public interface IForeignKeys : IMetaCollection<IForeignKey> { }
    public interface IIndexes : IMetaCollection<IIndex> { }
    public interface IProcedures : IMetaCollection<IProcedure> { }
    public interface IParameters : IMetaCollection<IParameter> { }

    public class Columns(IEnumerable<IColumn> items) : MetaCollection<IColumn>(items, c => c.Name), IColumns { }
    public class Tables(IEnumerable<ITable> items) : MetaCollection<ITable>(items, t => t.Name), ITables { }
    public class Views(IEnumerable<IView> items) : MetaCollection<IView>(items, t => t.Name), IViews { }
    public class ForeignKeys(IEnumerable<IForeignKey> items) : MetaCollection<IForeignKey>(items, t => t.Name), IForeignKeys { }
    public class Indexes(IEnumerable<IIndex> items) : MetaCollection<IIndex>(items, t => t.Name), IIndexes { }
    public class Procedures(IEnumerable<IProcedure> items) : MetaCollection<IProcedure>(items, t => t.Name), IProcedures { }
    public class Parameters(IEnumerable<IParameter> items) : MetaCollection<IParameter>(items, t => t.Name), IParameters { }

    public interface IColumn
    {
        string Name { get; }
        string Alias { get; }
        string DataTypeName { get; }
        string DataTypeNameComplete { get; }
        string LanguageType { get; }
        string DbTargetType { get; }
        int Ordinal { get; }
        bool IsNullable { get; }
        int CharacterMaxLength { get; }
        int NumericPrecision { get; }
        int NumericScale { get; }
        bool IsComputed { get; }
        bool IsInPrimaryKey { get; }
        bool IsAutoKey { get; }
        bool IsInForeignKey { get; }
        bool HasDefault { get; }
        string Description { get; }
        ITable? Table { get; }
        IView? View { get; }
    }

    public class Column(ColumnInfo info, bool inForeignKey, ITabularEntity? owner = null) : IColumn
    {
        public ITable? Table => owner as ITable is { } t && owner is not IView ? t : null;
        public IView? View => owner as IView;
        public string Name => info.Name;
        public string Alias => info.Name;
        public string DataTypeName => info.SqlType;
        public string DataTypeNameComplete => info.SqlType.ToLowerInvariant() switch
        {
            "varchar" or "varbinary" => $"{info.SqlType}({(info.MaxLength < 0 ? "MAX" : info.MaxLength.ToString())})",
            "nvarchar" => $"{info.SqlType}({(info.MaxLength < 0 ? "MAX" : (info.MaxLength / 2).ToString())})",
            "char" or "binary" => $"{info.SqlType}({info.MaxLength})",
            "nchar" => $"{info.SqlType}({info.MaxLength / 2})",
            "decimal" or "numeric" => $"{info.SqlType}({info.Precision},{info.Scale})",
            _ => info.SqlType,
        };
        public string LanguageType => LanguageMapping.ToCSharp(info.SqlType);
        public string DbTargetType => "SqlDbType." + char.ToUpperInvariant(info.SqlType[0]) + info.SqlType[1..];
        public int Ordinal => info.Ordinal;
        public bool IsNullable => info.IsNullable;
        public int CharacterMaxLength => info.SqlType.StartsWith('n') && info.SqlType != "numeric" ? info.MaxLength / 2 : info.MaxLength;
        public int NumericPrecision => info.Precision;
        public int NumericScale => info.Scale;
        public bool IsComputed => info.IsComputed || info.SqlType.Equals("timestamp", StringComparison.OrdinalIgnoreCase);
        public bool IsInPrimaryKey => info.IsPrimaryKey;
        public bool IsAutoKey => info.IsIdentity;
        public bool IsInForeignKey => inForeignKey;
        public bool HasDefault => info.HasDefault;
        public string Description => "";
    }

    public static class LanguageMapping
    {
        public static string ToCSharp(string sqlType) => sqlType.ToLowerInvariant() switch
        {
            "bigint" => "long",
            "binary" or "sql_variant" => "object",
            "bit" => "bool",
            "char" or "nchar" or "nvarchar" or "varchar" or "text" or "ntext" or "xml" or "sysname" => "string",
            "datetime" or "smalldatetime" or "date" or "datetime2" => "DateTime",
            "datetimeoffset" => "DateTimeOffset",
            "time" => "TimeSpan",
            "decimal" or "money" or "numeric" or "smallmoney" => "decimal",
            "float" => "double",
            "real" => "float",
            "image" or "timestamp" or "varbinary" or "rowversion" => "byte[]",
            "int" => "int",
            "smallint" => "short",
            "tinyint" => "byte",
            "uniqueidentifier" => "Guid",
            _ => "object",
        };
    }

    public interface ITabularEntity
    {
        string Name { get; }
        string Alias { get; }
        string Schema { get; }
        string Description { get; }
        IColumns Columns { get; }
        IForeignKeys ForeignKeys { get; }
        IIndexes Indexes { get; }
        IColumns PrimaryKeys { get; }
    }

    public interface ITable : ITabularEntity { }

    public interface IView : ITabularEntity
    {
        ITables SubTables { get; }
    }

    public interface IForeignKey
    {
        string Name { get; }
        string Alias { get; }
        ITable ForeignTable { get; }
        ITable PrimaryTable { get; }
        IColumns ForeignColumns { get; }
        IColumns PrimaryColumns { get; }
    }

    public interface IIndex
    {
        string Name { get; }
        bool Unique { get; }
        bool PrimaryKey { get; }
        IColumns Columns { get; }
    }

    /// <summary>Concrete types are exposed because templates and helpers cast to them (e.g. foreach (Index idx ...)).</summary>
    public class Index(IndexInfo info, IColumns columns) : IIndex
    {
        public string Name => info.Name;
        public bool Unique => info.IsUnique;
        public bool PrimaryKey => info.IsPrimaryKey;
        public IColumns Columns => columns;
    }

    public class ForeignKey(Func<string, ITable> lookup, ForeignKeyInfo info) : IForeignKey
    {
        public string Name => info.Name;
        public string Alias => info.Name;
        public ITable ForeignTable => lookup(info.ForeignTable);
        public ITable PrimaryTable => lookup(info.PrimaryTable);
        public IColumns ForeignColumns => new Columns(info.ForeignColumns.Select(c => ForeignTable.Columns[c]));
        public IColumns PrimaryColumns => new Columns(info.PrimaryColumns.Select(c => PrimaryTable.Columns[c]));
    }

    public class Table : ITable
    {
        private readonly TableInfo _info;
        private readonly Lazy<IColumns> _columns;
        protected readonly Func<string, ITable> Lookup;

        public Table(TableInfo info, Func<string, ITable> lookup)
        {
            _info = info;
            Lookup = lookup;
            var fkCols = info.ForeignKeys.SelectMany(f => f.ForeignColumns).ToHashSet(StringComparer.OrdinalIgnoreCase);
            _columns = new(() => new Columns(info.Columns.Select(c => (IColumn)new Column(c, fkCols.Contains(c.Name), this))));
        }

        public string Name => _info.Name;
        public string Alias => _info.Name;
        public string Schema => _info.Schema;
        public string Description => "";
        public IColumns Columns => _columns.Value;
        public IColumns PrimaryKeys => new Columns(Columns.Where(c => c.IsInPrimaryKey));
        public IForeignKeys ForeignKeys => new MyMeta.ForeignKeys(_info.ForeignKeys.Select(f => (IForeignKey)new ForeignKey(Lookup, f)));
        public IIndexes Indexes => new MyMeta.Indexes(_info.Indexes.Select(i =>
            (IIndex)new Index(i, new MyMeta.Columns(i.Columns.Select(c => Columns[c])))));
    }

    public class View(TableInfo info, Func<string, ITable> lookup) : Table(info, lookup), IView
    {
        public ITables SubTables => new Tables(info.SubTables.Select(t => Lookup(t)).Where(t => t is not null));
    }

    public enum ProcedureType { Procedure = 1, Function = 2 }

    public interface IParameter
    {
        string Name { get; }
        string TypeName { get; }
        string LanguageType { get; }
        ParamDirection Direction { get; }
        bool IsNullable { get; }
        int Ordinal { get; }
    }

    public class Parameter(ParameterInfo info) : IParameter
    {
        public string Name => info.Name;
        public string TypeName => info.SqlType;
        public string LanguageType => LanguageMapping.ToCSharp(info.SqlType);
        public ParamDirection Direction => info.IsOutput ? ParamDirection.InputOutput : ParamDirection.Input;
        public bool IsNullable => info.IsNullable;
        public int Ordinal => info.Ordinal;
    }

    public interface IProcedure
    {
        string Name { get; }
        string Alias { get; }
        string Schema { get; }
        int Type { get; }
        IParameters Parameters { get; }
    }

    public class Procedure(ProcedureInfo info) : IProcedure
    {
        public string Name => info.Name;
        public string Alias => info.Name;
        public string Schema => info.Schema;
        public int Type => info.IsFunction ? (int)ProcedureType.Function : (int)ProcedureType.Procedure;
        public IParameters Parameters => new MyMeta.Parameters(info.Parameters.Select(p => (IParameter)new Parameter(p)));
    }

    public interface IDatabase
    {
        string Name { get; }
        string Alias { get; }
        ITables Tables { get; }
        IViews Views { get; }
        IProcedures Procedures { get; }
    }

    public class Database : IDatabase
    {
        private readonly DatabaseInfo _info;
        private readonly Dictionary<string, ITable> _byName;

        public Database(DatabaseInfo info)
        {
            _info = info;
            _byName = new(StringComparer.OrdinalIgnoreCase);
            foreach (var t in info.Tables)
                _byName[t.Name] = t.IsView ? new View(t, Lookup) : new Table(t, Lookup);
            Tables = new MyMeta.Tables(info.Tables.Where(t => !t.IsView).Select(t => _byName[t.Name]));
            Views = new MyMeta.Views(info.Tables.Where(t => t.IsView).Select(t => (IView)_byName[t.Name]));
            Procedures = new MyMeta.Procedures(info.Procedures.Select(p => (IProcedure)new Procedure(p)));
        }

        private ITable Lookup(string name) => _byName.GetValueOrDefault(name)!;

        public string Name => _info.Name;
        public string Alias => _info.Name;
        public ITables Tables { get; }
        public IViews Views { get; }
        public IProcedures Procedures { get; }
    }

    /// <summary>Root metadata object, exposed to templates as the 'MyMeta' intrinsic variable.</summary>
    public class dbRoot
    {
        public dbRoot() { }
        public dbRoot(DatabaseInfo info) => Connect(info);

        public IDatabase? DefaultDatabase { get; private set; }
        public DatabaseInfo? Info { get; private set; }
        public bool IsConnected => DefaultDatabase is not null;

        public void Connect(DatabaseInfo info) { Info = info; DefaultDatabase = new Database(info); }
        public void SaveUserMetaData() { } // user metadata is not persisted in this implementation
    }
}
