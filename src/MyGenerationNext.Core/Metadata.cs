using Microsoft.Data.SqlClient;

namespace MyGenerationNext.Core;

public sealed record ColumnInfo(
    string Name,
    string SqlType,
    string ClrType,
    bool IsNullable,
    bool IsPrimaryKey,
    bool IsIdentity,
    int Ordinal)
{
    public int MaxLength { get; init; }
    public int Precision { get; init; }
    public int Scale { get; init; }
    public bool IsComputed { get; init; }
    public bool HasDefault { get; init; }
}

public sealed record ForeignKeyInfo(
    string Name,
    string ForeignTable,
    string PrimaryTable,
    IReadOnlyList<string> ForeignColumns,
    IReadOnlyList<string> PrimaryColumns);

public sealed record IndexInfo(string Name, bool IsUnique, bool IsPrimaryKey, IReadOnlyList<string> Columns);

public sealed record TableInfo(string Schema, string Name, bool IsView, IReadOnlyList<ColumnInfo> Columns)
{
    public IReadOnlyList<ForeignKeyInfo> ForeignKeys { get; init; } = [];
    public IReadOnlyList<IndexInfo> Indexes { get; init; } = [];
    /// <summary>For views: the tables the view depends on.</summary>
    public IReadOnlyList<string> SubTables { get; init; } = [];
    public IEnumerable<ColumnInfo> PrimaryKey => Columns.Where(c => c.IsPrimaryKey);
}

public sealed record ParameterInfo(string Name, string SqlType, bool IsOutput, bool IsNullable, int Ordinal);

/// <summary>A stored procedure or scalar/table-valued function.</summary>
public sealed record ProcedureInfo(string Schema, string Name, string ObjectType, IReadOnlyList<ParameterInfo> Parameters)
{
    /// <summary>Result columns of inline/multi-statement table-valued functions.</summary>
    public IReadOnlyList<ColumnInfo> ReturnColumns { get; init; } = [];
    public bool IsFunction => ObjectType is "FN" or "IF" or "TF";
}

public sealed record DatabaseInfo(string Name, IReadOnlyList<TableInfo> Tables)
{
    public IReadOnlyList<ProcedureInfo> Procedures { get; init; } = [];
    public IEnumerable<TableInfo> TablesOnly => Tables.Where(t => !t.IsView);
    public IEnumerable<TableInfo> Views => Tables.Where(t => t.IsView);
    public TableInfo? Find(string name) =>
        Tables.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
}

/// <summary>Reads tables, views, keys, indexes, procedures and functions from MS SQL Server.</summary>
public static class SqlServerMetadataReader
{
    private const string Query = """
        SELECT s.name, o.name, o.type, c.name, ty.name, c.is_nullable, c.is_identity, c.column_id,
               CAST(CASE WHEN EXISTS (
                   SELECT 1 FROM sys.index_columns ic
                   JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                   WHERE i.is_primary_key = 1 AND ic.object_id = c.object_id AND ic.column_id = c.column_id)
                    THEN 1 ELSE 0 END AS bit),
               c.max_length, c.precision, c.scale, c.is_computed,
               CAST(CASE WHEN c.default_object_id <> 0 THEN 1 ELSE 0 END AS bit)
        FROM sys.objects o
        JOIN sys.schemas s ON s.schema_id = o.schema_id
        JOIN sys.columns c ON c.object_id = o.object_id
        JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        WHERE o.type IN ('U', 'V', 'IF', 'TF') AND o.is_ms_shipped = 0
        ORDER BY s.name, o.name, c.column_id;

        SELECT fk.name, ft.name, pt.name, fc.name, pc.name
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
        JOIN sys.objects ft ON ft.object_id = fk.parent_object_id
        JOIN sys.objects pt ON pt.object_id = fk.referenced_object_id
        JOIN sys.columns fc ON fc.object_id = fkc.parent_object_id AND fc.column_id = fkc.parent_column_id
        JOIN sys.columns pc ON pc.object_id = fkc.referenced_object_id AND pc.column_id = fkc.referenced_column_id
        ORDER BY ft.name, fk.name, fkc.constraint_column_id;

        SELECT o.name, i.name, i.is_unique, i.is_primary_key, c.name
        FROM sys.indexes i
        JOIN sys.objects o ON o.object_id = i.object_id
        JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE o.type IN ('U', 'V') AND i.type > 0 AND o.is_ms_shipped = 0
        ORDER BY o.name, i.index_id, ic.key_ordinal;

        SELECT DISTINCT v.name, t.name
        FROM sys.sql_expression_dependencies e
        JOIN sys.objects v ON v.object_id = e.referencing_id AND v.type = 'V'
        JOIN sys.objects t ON t.object_id = e.referenced_id AND t.type = 'U'
        ORDER BY v.name, t.name;

        SELECT s.name, o.name, o.type, p.name, ty.name, p.is_output, p.parameter_id
        FROM sys.objects o
        JOIN sys.schemas s ON s.schema_id = o.schema_id
        LEFT JOIN sys.parameters p ON p.object_id = o.object_id AND p.parameter_id > 0
        LEFT JOIN sys.types ty ON ty.user_type_id = p.user_type_id
        WHERE o.type IN ('P', 'FN', 'IF', 'TF') AND o.is_ms_shipped = 0
        ORDER BY s.name, o.name, p.parameter_id;
        """;

    public static async Task<DatabaseInfo> ReadAsync(string connectionString, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(Query, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);

        var tables = new List<(string Schema, string Name, string Type, List<ColumnInfo> Cols)>();
        while (await r.ReadAsync(ct))
        {
            var (schema, name, type) = (r.GetString(0), r.GetString(1), r.GetString(2).Trim());
            if (tables.Count == 0 || tables[^1].Schema != schema || tables[^1].Name != name)
                tables.Add((schema, name, type, []));
            var sqlType = r.GetString(4);
            var nullable = r.GetBoolean(5);
            tables[^1].Cols.Add(new ColumnInfo(r.GetString(3), sqlType, ToClrType(sqlType, nullable), nullable,
                r.GetBoolean(8), r.GetBoolean(6), r.GetInt32(7))
            {
                MaxLength = r.GetInt16(9), Precision = r.GetByte(10), Scale = r.GetByte(11),
                IsComputed = r.GetBoolean(12), HasDefault = r.GetBoolean(13),
            });
        }

        await r.NextResultAsync(ct);
        var fks = new Dictionary<string, List<(string Name, string Pt, List<string> Fc, List<string> Pc)>>(StringComparer.OrdinalIgnoreCase);
        while (await r.ReadAsync(ct))
        {
            var (fkName, ft, pt) = (r.GetString(0), r.GetString(1), r.GetString(2));
            if (!fks.TryGetValue(ft, out var list)) fks[ft] = list = [];
            var fk = list.Find(x => x.Name == fkName);
            if (fk.Name is null) { fk = (fkName, pt, [], []); list.Add(fk); }
            fk.Fc.Add(r.GetString(3));
            fk.Pc.Add(r.GetString(4));
        }

        await r.NextResultAsync(ct);
        var idx = new Dictionary<string, List<(string Name, bool Unique, bool Pk, List<string> Cols)>>(StringComparer.OrdinalIgnoreCase);
        while (await r.ReadAsync(ct))
        {
            var (tn, iname) = (r.GetString(0), r.GetString(1));
            if (!idx.TryGetValue(tn, out var list)) idx[tn] = list = [];
            var ix = list.Find(x => x.Name == iname);
            if (ix.Name is null) { ix = (iname, r.GetBoolean(2), r.GetBoolean(3), []); list.Add(ix); }
            ix.Cols.Add(r.GetString(4));
        }

        await r.NextResultAsync(ct);
        var deps = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        while (await r.ReadAsync(ct))
        {
            if (!deps.TryGetValue(r.GetString(0), out var list)) deps[r.GetString(0)] = list = [];
            list.Add(r.GetString(1));
        }

        await r.NextResultAsync(ct);
        var procs = new List<(string Schema, string Name, string Type, List<ParameterInfo> Params)>();
        while (await r.ReadAsync(ct))
        {
            var (schema, name, type) = (r.GetString(0), r.GetString(1), r.GetString(2).Trim());
            if (procs.Count == 0 || procs[^1].Schema != schema || procs[^1].Name != name)
                procs.Add((schema, name, type, []));
            if (!r.IsDBNull(3))
                procs[^1].Params.Add(new ParameterInfo(r.GetString(3), r.GetString(4), r.GetBoolean(5), true, r.GetInt32(6)));
        }

        var tableInfos = tables.Where(t => t.Type is "U" or "V").Select(t => new TableInfo(t.Schema, t.Name, t.Type == "V", t.Cols)
        {
            ForeignKeys = fks.GetValueOrDefault(t.Name)?.Select(f => new ForeignKeyInfo(f.Name, t.Name, f.Pt, f.Fc, f.Pc)).ToArray() ?? [],
            Indexes = idx.GetValueOrDefault(t.Name)?.Select(i => new IndexInfo(i.Name, i.Unique, i.Pk, i.Cols)).ToArray() ?? [],
            SubTables = deps.GetValueOrDefault(t.Name)?.ToArray() ?? [],
        }).ToArray();

        var returnCols = tables.Where(t => t.Type is "IF" or "TF").ToDictionary(t => t.Name, t => (IReadOnlyList<ColumnInfo>)t.Cols);
        var procInfos = procs.Select(p => new ProcedureInfo(p.Schema, p.Name, p.Type, p.Params)
        {
            ReturnColumns = returnCols.GetValueOrDefault(p.Name) ?? [],
        }).ToArray();

        return new DatabaseInfo(conn.Database, tableInfos) { Procedures = procInfos };
    }

    public static string ToClrType(string sqlType, bool nullable)
    {
        var (type, isValueType) = sqlType.ToLowerInvariant() switch
        {
            "bigint" => ("long", true),
            "int" => ("int", true),
            "smallint" => ("short", true),
            "tinyint" => ("byte", true),
            "bit" => ("bool", true),
            "decimal" or "numeric" or "money" or "smallmoney" => ("decimal", true),
            "float" => ("double", true),
            "real" => ("float", true),
            "date" or "datetime" or "datetime2" or "smalldatetime" => ("DateTime", true),
            "datetimeoffset" => ("DateTimeOffset", true),
            "time" => ("TimeSpan", true),
            "uniqueidentifier" => ("Guid", true),
            "binary" or "varbinary" or "image" or "timestamp" or "rowversion" => ("byte[]", false),
            "xml" or "char" or "varchar" or "text" or "nchar" or "nvarchar" or "ntext" or "sysname" => ("string", false),
            _ => ("object", false),
        };
        return nullable && isValueType ? type + "?" : type;
    }
}
