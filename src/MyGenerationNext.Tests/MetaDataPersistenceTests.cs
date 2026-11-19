using System.Xml.Linq;
using EHA.ZESA.Util.GeneratorUI;
using MyGenerationNext.Core;

namespace MyGenerationNext.Tests;

[CollectionDefinition("Generator state", DisableParallelization = true)]
public class GeneratorStateCollection { }

[Collection("Generator state")]
public class MetaDataPersistenceTests
{
    private static readonly XNamespace Ns = "http://www.tempuri.org/MetaData.xsd";

    private static ColumnInfo Col(string n, string t, int ord, bool pk = false, bool nullable = false, int len = 0) =>
        new(n, t, SqlServerMetadataReader.ToClrType(t, nullable), nullable, pk, pk, ord) { MaxLength = len, Precision = 10 };

    private static DatabaseInfo FakeDb() => new("Fake", [
        new TableInfo("dbo", "BA_ANWENDUNGSFALL", false,
            [Col("ANWENDUNGSFALL_ID", "int", 1, true), Col("MANDANT", "int", 2), Col("ANWENDUNGSFALL_KENNUNG", "varchar", 3, len: 50)])
        {
            Indexes = [new IndexInfo("UQ_AWF", true, false, ["MANDANT", "ANWENDUNGSFALL_KENNUNG"])],
        },
        new TableInfo("dbo", "GP_PARTNER", false,
            [Col("PARTNER_ID", "int", 1, true), Col("MANDANT", "int", 2), Col("ANWENDUNGSFALL_ID", "int", 3, nullable: true),
             Col("NAME", "varchar", 4, nullable: true, len: 100)])
        {
            ForeignKeys = [new ForeignKeyInfo("FK_P_AWF", "GP_PARTNER", "BA_ANWENDUNGSFALL", ["ANWENDUNGSFALL_ID"], ["ANWENDUNGSFALL_ID"])],
            Indexes = [new IndexInfo("PK", true, true, ["PARTNER_ID"])],
        },
        new TableInfo("dbo", "V_PARTNER", true, [Col("PARTNER_ID", "int", 1), Col("NAME", "varchar", 2, len: 100)])
        {
            SubTables = ["GP_PARTNER"],
        },
    ])
    {
        Procedures = [
            new ProcedureInfo("dbo", "f_sel_partner", "IF", [new ParameterInfo("@MANDANT", "int", false, false, 1)])
            {
                ReturnColumns = [Col("PARTNER_ID", "int", 1), Col("NAME", "varchar", 2, nullable: true, len: 100)],
            },
        ],
    };

    private static string TempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgn-meta-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string WriteMetaFile(string dir, params (string ObjectName, string Type, string Namespace, bool Generate)[] rows)
    {
        var path = Path.Combine(dir, "BLGeneratorUIMeta.xml");
        var root = new XElement(Ns + "MetaData",
            new XElement(Ns + "PathInfo", new XElement(Ns + "TargetPath", @"C:\TFS64Repos\ZESA\Source")));
        foreach (var (objectName, type, ns, generate) in rows)
            root.Add(new XElement(Ns + "MetaData",
                new XElement(Ns + "ObjectName", objectName),
                new XElement(Ns + "Namespace", ns),
                new XElement(Ns + "LOV", "false"),
                new XElement(Ns + "Generate", generate ? "true" : "false"),
                new XElement(Ns + "Type", type),
                new XElement(Ns + "EntityName", objectName)));
        new XDocument(new XDeclaration("1.0", "utf-8", null), root).Save(path);
        return path;
    }

    private static (BLGeneratorUI Ui, string Target) Run(string? metaFile, bool saveMetaData, params string[] selected)
    {
        var db = FakeDb();
        var target = Path.Combine(Path.GetTempPath(), "mgn-out-" + Guid.NewGuid().ToString("N"));
        GeneratorEnvironment.Reset();
        GeneratorEnvironment.Database = db;
        GeneratorEnvironment.TargetPath = target;
        GeneratorEnvironment.AutoSelectStammdatenDependencies = false;
        GeneratorEnvironment.SelectedObjects = new HashSet<string>(selected, StringComparer.OrdinalIgnoreCase);
        GeneratorEnvironment.MetaDataFile = metaFile;
        GeneratorEnvironment.SaveMetaData = saveMetaData;
        var ui = new BLGeneratorUI(new global::MyMeta.dbRoot(db));
        Assert.Equal(DialogResult.OK, ui.ShowDialog());
        return (ui, target);
    }

    private static XElement Row(XDocument doc, string objectName) =>
        doc.Root!.Elements(Ns + "MetaData").Single(e => e.Element(Ns + "ObjectName")!.Value == objectName);

    [Fact]
    public void ShowDialog_MergesSavedNamespaces_ForViewsAndFunctions()
    {
        var dir = TempDir();
        try
        {
            var meta = WriteMetaFile(dir,
                ("V_PARTNER", "View", "Kundenabrechnung", true),
                ("f_sel_partner", "Function", "Ticketing", true),
                ("BA_ANWENDUNGSFALL", "Table", "WRONG", false));

            var (ui, _) = Run(meta, saveMetaData: false, "V_PARTNER", "f_sel_partner", "BA_ANWENDUNGSFALL");

            Assert.Equal("Kundenabrechnung", ui.Rows.Single(r => r.ObjectName == "V_PARTNER").Namespace);
            Assert.Equal("Ticketing", ui.Rows.Single(r => r.ObjectName == "f_sel_partner").Namespace);
            Assert.Equal("Basis", ui.Rows.Single(r => r.ObjectName == "BA_ANWENDUNGSFALL").Namespace);

            var genInfos = ui.Data.Cast<GenInfo>().ToList();
            Assert.Equal("Kundenabrechnung", genInfos.Single(g => g.ObjectName == "V_PARTNER").Namespace);
            Assert.Equal("Ticketing", genInfos.Single(g => g.ObjectName == "f_sel_partner").Namespace);
        }
        finally
        {
            GeneratorEnvironment.Reset();
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ShowDialog_WithSaveMetaData_PersistsOverrides_WithoutClobberingGenerate()
    {
        var dir = TempDir();
        try
        {
            var meta = WriteMetaFile(dir,
                ("V_PARTNER", "View", "Kundenabrechnung", true),
                ("BA_ANWENDUNGSFALL", "Table", "Basis", false));

            var (_, target) = Run(meta, saveMetaData: true, "BA_ANWENDUNGSFALL");

            var doc = XDocument.Load(meta);
            var view = Row(doc, "V_PARTNER");
            Assert.Equal("Kundenabrechnung", view.Element(Ns + "Namespace")!.Value);
            Assert.Equal("true", view.Element(Ns + "Generate")!.Value);
            Assert.Equal("false", Row(doc, "BA_ANWENDUNGSFALL").Element(Ns + "Generate")!.Value);
            Assert.Equal(target, doc.Root!.Element(Ns + "PathInfo")!.Element(Ns + "TargetPath")!.Value);
        }
        finally
        {
            GeneratorEnvironment.Reset();
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ShowDialog_WithSaveMetaData_CreatesMetaDataFile_WhenMissing()
    {
        var dir = TempDir();
        try
        {
            var meta = Path.Combine(dir, "sub", "BLGeneratorUIMeta.xml");
            Run(meta, saveMetaData: true, "V_PARTNER");

            Assert.True(File.Exists(meta));
            var doc = XDocument.Load(meta);
            Assert.Contains(doc.Root!.Elements(Ns + "MetaData"),
                e => e.Element(Ns + "ObjectName")!.Value == "V_PARTNER");
        }
        finally
        {
            GeneratorEnvironment.Reset();
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ShowDialog_WithoutMetaDataFile_UsesConventionNamespacesOnly()
    {
        var dir = TempDir();
        try
        {
            var (ui, _) = Run(null, saveMetaData: true, "V_PARTNER", "BA_ANWENDUNGSFALL");

            Assert.Equal("Basis", ui.Rows.Single(r => r.ObjectName == "BA_ANWENDUNGSFALL").Namespace);
            Assert.Equal("", ui.Rows.Single(r => r.ObjectName == "V_PARTNER").Namespace);
            Assert.False(File.Exists(Path.Combine(dir, "BLGeneratorUIMeta.xml")));
        }
        finally
        {
            GeneratorEnvironment.Reset();
            Directory.Delete(dir, true);
        }
    }
}
