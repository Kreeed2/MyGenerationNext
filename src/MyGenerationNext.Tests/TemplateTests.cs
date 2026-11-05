using EHA.ZESA.Util.GeneratorUI;
using MyGenerationNext.Core;
using MyGenerationNext.Core.Zeus;

namespace MyGenerationNext.Tests;

public class TemplateTests
{
    private static string TemplatesDir()
    {
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent)
        {
            var p = Path.Combine(d.FullName, "MyGenerationTemplates");
            if (File.Exists(Path.Combine(p, "BL.csgen"))) return p;
        }
        throw new DirectoryNotFoundException("MyGenerationTemplates not found");
    }

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
             Col("NAME", "varchar", 4, nullable: true, len: 100), Col("GUELTIG_VON", "datetime", 5, nullable: true)])
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

    [Fact]
    public void AllTemplatesParse()
    {
        var files = Directory.GetFiles(TemplatesDir(), "*.csgen");
        Assert.NotEmpty(files);
        foreach (var f in files)
        {
            var t = ZeusTemplate.Load(f);
            Assert.False(string.IsNullOrEmpty(t.UniqueId), f);
        }
    }

    [Fact]
    public void BlCsgenIsParsedAsGroup()
    {
        var t = ZeusTemplate.Load(Path.Combine(TemplatesDir(), "BL.csgen"));
        Assert.True(t.IsGroup);
        Assert.Equal(11, t.IncludedTemplatePaths.Count);
        Assert.False(t.Gui.IsEmpty);
        foreach (var inc in t.IncludedTemplatePaths) Assert.True(File.Exists(t.ResolveInclude(inc)), inc);
    }

    [Fact]
    public void AllTemplatesCompile()
    {
        foreach (var f in Directory.GetFiles(TemplatesDir(), "ZESA*.csgen").Append(Path.Combine(TemplatesDir(), "BL.csgen")))
        {
            var t = ZeusTemplate.Load(f);
            if (!t.Gui.IsEmpty)
                DotNetScriptCompiler.Compile(t, DotNetScriptCodeParser.Parse(t, t.Gui, true), "gui" + Guid.NewGuid().ToString("N"));
            if (!t.Body.IsEmpty)
                DotNetScriptCompiler.Compile(t, DotNetScriptCodeParser.Parse(t, t.Body, false), "body" + Guid.NewGuid().ToString("N"));
        }
    }

    [Fact]
    public void BlGroupRunsAndProducesFiles()
    {
        var target = Path.Combine(Path.GetTempPath(), "mgn-" + Guid.NewGuid().ToString("N"));
        GeneratorEnvironment.Reset();
        var db = FakeDb();
        GeneratorEnvironment.Database = db;
        GeneratorEnvironment.TargetPath = target;
        GeneratorEnvironment.SelectedObjects = new HashSet<string>(["GP_PARTNER", "BA_ANWENDUNGSFALL", "V_PARTNER", "f_sel_partner"], StringComparer.OrdinalIgnoreCase);
        try
        {
            var result = ZeusExecutor.Run(Path.Combine(TemplatesDir(), "BL.csgen"), new ZeusRunOptions { MyMeta = new global::MyMeta.dbRoot(db) });
            Assert.Equal(12, result.RenderedTemplates.Count);
            Assert.NotEmpty(result.SavedFiles);
            Assert.All(result.SavedFiles, f => Assert.True(File.Exists(f), f));
            Assert.Contains(result.SavedFiles, f => f.Contains("GP_PARTNER", StringComparison.OrdinalIgnoreCase) || f.Contains("Partner", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            GeneratorEnvironment.Reset();
            if (Directory.Exists(target) && Environment.GetEnvironmentVariable("MGN_KEEP") is null) Directory.Delete(target, true);
            else Console.WriteLine("KEPT " + target);
        }
    }
}
