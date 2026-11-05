using System.Data;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using MyGenerationNext.Core;

namespace EHA.ZESA.Util.GeneratorUI
{
    /// <summary>
    /// Host-side configuration of the headless generator UI. Replaces the hard-coded Windows paths and
    /// the WinForms dialog of the legacy BLGeneratorUI.
    /// </summary>
    public static class GeneratorEnvironment
    {
        public static string? MemberUseOldDataSetLogicFile { get; set; }
        public static string? ObjectsToGenerateFile { get; set; }
        public static string? SettingsFile { get; set; }
        /// <summary>Saved selection state (BLGeneratorUIMeta.xml). Read and, if SaveMetaData is set, written back.</summary>
        public static string? MetaDataFile { get; set; }
        public static bool SaveMetaData { get; set; }
        public static string TargetPath { get; set; } = "";
        /// <summary>
        /// When set, only these database objects (by name, case-insensitive) are generated; everything else
        /// is deselected. When null, the saved/default selection is used.
        /// </summary>
        public static ISet<string>? SelectedObjects { get; set; }
        public static bool IgnoreStammdatenuebernahme { get; set; }
        public static DatabaseInfo? Database { get; set; }
        /// <summary>Registered master-data tables (from ba_funktion).</summary>
        public static List<string> StammdatenTables { get; set; } = [];
        /// <summary>Names of existing TT_ table-valued parameter types.</summary>
        public static List<string> TableValuedTypeNames { get; set; } = [];

        public static void Reset()
        {
            MemberUseOldDataSetLogicFile = ObjectsToGenerateFile = SettingsFile = MetaDataFile = null;
            SaveMetaData = false;
            TargetPath = "";
            SelectedObjects = null;
            IgnoreStammdatenuebernahme = false;
            Database = null;
            StammdatenTables = [];
            TableValuedTypeNames = [];
        }

        /// <summary>Loads server-side extras that are not part of <see cref="DatabaseInfo"/>. Failures leave the lists empty.</summary>
        public static async Task LoadServerExtrasAsync(string connectionString, CancellationToken ct = default)
        {
            try
            {
                await using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync(ct);
                TableValuedTypeNames = await QueryAsync(conn, "SELECT name FROM sys.table_types WHERE name LIKE 'TT[_]%'", ct);
                TableValuedTypeNames = TableValuedTypeNames.Select(n => n.ToUpperInvariant()[3..]).Distinct().ToList();
                try
                {
                    StammdatenTables = await QueryAsync(conn,
                        "select distinct funktion from ba_funktion where funktion like 'stammdaten%' and mandant = 100", ct);
                }
                catch (SqlException) { StammdatenTables = []; }
            }
            catch (SqlException) { }
        }

        private static async Task<List<string>> QueryAsync(SqlConnection conn, string sql, CancellationToken ct)
        {
            var list = new List<string>();
            await using var cmd = new SqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) list.Add(r.GetString(0));
            return list;
        }
    }

    public class SqlFunctionAnalyzer
    {
        public static List<TvfColumnInfo> GetTvfReturnColumns(string functionName)
        {
            var proc = GeneratorEnvironment.Database?.Procedures
                .FirstOrDefault(p => string.Equals(p.Name, functionName, StringComparison.OrdinalIgnoreCase));
            if (proc is null) return [];
            return proc.ReturnColumns.Select(c => new TvfColumnInfo
            {
                Name = c.Name,
                LanguageType = MyMeta.LanguageMapping.ToCSharp(c.SqlType),
                IsNullable = c.IsNullable,
            }).ToList();
        }
    }

    public class FunctionGenInfo
    {
        public FunctionGenInfo(string pFunctionName) { FunctionName = pFunctionName; }
        public string FunctionName { get; private set; }
        public List<TvfColumnInfo> FunctionReturnValueColumns { get; set; } = [];
        public List<TvfColumnInfo> FunctionInputParameterColumns { get; set; } = [];
    }

    [Serializable]
    public class TvfColumnInfo
    {
        public string Name { get; set; } = "";
        public string LanguageType { get; set; } = "";
        public bool IsNullable { get; set; }
    }

    /// <summary>Source control integration is not available; all operations are no-ops.</summary>
    public class TfsHelper
    {
        private static readonly TfsHelper _instance = new();
        public static TfsHelper Instance => _instance;
        public string CurrentTfSWorkspace => Environment.MachineName;
        public void CheckInMetaDataFile() { }
        public void CheckOutMetaDataFile(string pMetadataFile) { }
        public void CheckOut(string pFullFilenameAndPath) { }
        public void Add(string pFullFilenameAndPath) { }
        public void GetUtilities() { }
    }

    public enum DialogResult { None = 0, OK = 1, Cancel = 2 }

    /// <summary>One row of the generator selection (the legacy MetaData.MetaDataRow).</summary>
    public class MetaDataRow
    {
        public string ObjectName = "";
        public string Namespace = "";
        public bool LOV;
        public bool Generate;
        public string Type = "";
        public string EntityName = "";
        public string LovColumns = "";
        public bool NORMTIMEFROM;
        public string NormTimeFromColumns = "";
        public bool NORMTIMETO;
        public string NormTimeToColumns = "";
        public bool Stammdatenuebernahme;
        public string AliasKennungColumn = "";
        public string DbReaderDataTypeColumns = "";
        public string DbReaderOperatorColumns = "";
        public string PrimaryKeyIntegerColumnNames = "";
        public bool IsPrimaryKeyInteger;
    }

    internal static class MetaDataXml
    {
        private static readonly XNamespace Ns = "http://www.tempuri.org/MetaData.xsd";

        public static (string? TargetPath, List<MetaDataRow> Rows) Load(string path)
        {
            var doc = XDocument.Load(path);
            var target = doc.Root?.Element(Ns + "PathInfo")?.Element(Ns + "TargetPath")?.Value;
            var rows = new List<MetaDataRow>();
            foreach (var e in doc.Root?.Elements(Ns + "MetaData") ?? [])
            {
                string S(string n) => e.Element(Ns + n)?.Value ?? "";
                bool B(string n) => bool.TryParse(S(n), out var b) && b;
                rows.Add(new MetaDataRow
                {
                    ObjectName = S("ObjectName"), Namespace = S("Namespace"), LOV = B("LOV"), Generate = B("Generate"),
                    Type = S("Type"), EntityName = S("EntityName"), LovColumns = S("LovColumns"),
                    NORMTIMEFROM = B("NORMTIMEFROM"), NormTimeFromColumns = S("NormTimeFromColumns"),
                    NORMTIMETO = B("NORMTIMETO"), NormTimeToColumns = S("NormTimeToColumns"),
                    Stammdatenuebernahme = B("Stammdatenuebernahme"), AliasKennungColumn = S("AliasKennungColumn"),
                    DbReaderDataTypeColumns = S("DbReaderDataTypeColumns"), DbReaderOperatorColumns = S("DbReaderOperatorColumns"),
                    PrimaryKeyIntegerColumnNames = S("PrimaryKeyIntegerColumnNames"), IsPrimaryKeyInteger = B("IsPrimaryKeyInteger"),
                });
            }
            return (target, rows);
        }

        public static void Save(string path, string targetPath, IEnumerable<MetaDataRow> rows)
        {
            var root = new XElement(Ns + "MetaData", new XElement(Ns + "PathInfo", new XElement(Ns + "TargetPath", targetPath)));
            foreach (var r in rows)
            {
                XElement E(string n, object v) => new(Ns + n, v is bool b ? (b ? "true" : "false") : v);
                root.Add(new XElement(Ns + "MetaData",
                    E("ObjectName", r.ObjectName), E("Namespace", r.Namespace), E("LOV", r.LOV), E("Generate", r.Generate),
                    E("Type", r.Type), E("EntityName", r.EntityName), E("LovColumns", r.LovColumns),
                    E("NORMTIMEFROM", r.NORMTIMEFROM), E("NormTimeFromColumns", r.NormTimeFromColumns),
                    E("NORMTIMETO", r.NORMTIMETO), E("NormTimeToColumns", r.NormTimeToColumns),
                    E("Stammdatenuebernahme", r.Stammdatenuebernahme), E("AliasKennungColumn", r.AliasKennungColumn),
                    E("DbReaderDataTypeColumns", r.DbReaderDataTypeColumns), E("DbReaderOperatorColumns", r.DbReaderOperatorColumns),
                    E("PrimaryKeyIntegerColumnNames", r.PrimaryKeyIntegerColumnNames), E("IsPrimaryKeyInteger", r.IsPrimaryKeyInteger)));
            }
            new XDocument(new XDeclaration("1.0", "utf-8", null), root).Save(path);
        }
    }
}
