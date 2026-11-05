using System.Collections;
using MyMeta;

namespace EHA.ZESA.Util.GeneratorUI
{
    /// <summary>
    /// Headless port of the legacy WinForms BLGeneratorUI. The dialog is replaced by <see cref="GeneratorEnvironment"/>
    /// (object selection, target path, saved metadata); all selection/Stammdaten/GenInfo logic is the original.
    /// </summary>
    public class BLGeneratorUI
    {
        public const string TYPE_TABLE = "Table";
        public const string TYPE_VIEW = "View";
        public const string TYPE_FUNCTION = "Function";

        public static string PathTfsRepos = string.Empty;
        public static string SqlConnnectionStringZesaEntw = string.Empty;

        private BLGeneratorSettings mBlGeneratorSettings;
        private readonly dbRoot mMetaData;
        private ArrayList mGenInfoData = new();
        private string mTargetPath = string.Empty;
        private List<string> mStammdatentabellen = new();
        private readonly ArrayList mStammdatenWithForeignKeys = new();
        private readonly ArrayList mKeineDatenuebernahmeGenerieren = new();
        private readonly List<string> mExistingTableValuedParameterForTables = new();
        private bool mNeedPruefeTvpGeneration;
        private readonly ArrayList mFunctionGenInfos = new();
        private readonly List<MetaDataRow> mRows = new();

        public BLGeneratorUI(dbRoot pMeta)
        {
            mMetaData = pMeta;
        }

        public ArrayList Data => mGenInfoData;
        public ArrayList FunctionGenInfoData => mFunctionGenInfos;
        public string TargetPath => mTargetPath;
        public ArrayList StammdatenWithForeignKeys => mStammdatenWithForeignKeys;
        public ArrayList ExistingTableValuedParameterForTables => new ArrayList(mExistingTableValuedParameterForTables);
        public dbRoot Meta => mMetaData;
        public bool NeedPruefeTvpGeneration => mNeedPruefeTvpGeneration;
        /// <summary>Rows with the final selection (after ShowDialog).</summary>
        public IReadOnlyList<MetaDataRow> Rows => mRows;

        /// <summary>Runs the selection headlessly using <see cref="GeneratorEnvironment"/>.</summary>
        public DialogResult ShowDialog()
        {
            if (mMetaData.DefaultDatabase == null) return DialogResult.Cancel;

            mBlGeneratorSettings = GeneratorHelper.DeserializeSettings();
            mBlGeneratorSettings.EnitaetenAusnahmeDatenuebernahme ??= new List<string>();
            mBlGeneratorSettings.EnitaetenPflichDatenuebernahme ??= new List<string>();
            mStammdatentabellen = new List<string>(GeneratorEnvironment.StammdatenTables);

            FillDataSet();
            var targetPath = GeneratorEnvironment.TargetPath;
            LoadSavedLocalMetadata(ref targetPath);
            mTargetPath = targetPath;
            PathTfsRepos = mTargetPath.Replace("/", "\\") + "\\";

            var objToGen = GeneratorHelper.GetObjectsToGenerate();
            if (GeneratorEnvironment.SelectedObjects is { } selected)
            {
                foreach (var row in mRows)
                    row.Generate = selected.Contains(row.ObjectName, StringComparer.OrdinalIgnoreCase);
            }
            else if (objToGen.Count > 0)
            {
                foreach (var row in mRows) row.Generate = false;
                foreach (var name in objToGen)
                    foreach (var row in mRows.Where(r => r.ObjectName.ToUpper() == name.ToUpper()))
                        row.Generate = true;
            }

            PruefeStammdatenUebernahme();
            FillGenInfo();
            SaveLocalMetadata();
            return DialogResult.OK;
        }

        private static bool IgnoreStammdaten => GeneratorEnvironment.IgnoreStammdatenuebernahme;
        private static bool AutoSelect => !IgnoreStammdaten && GeneratorEnvironment.AutoSelectStammdatenDependencies;

        private static MetaDataRow NewRow(string name, string ns, string type) => new()
        {
            ObjectName = name,
            Namespace = ns,
            Generate = true,
            Type = type,
            EntityName = GeneratorHelper.BuildEntityName(name, type),
        };

        private void FillDataSet()
        {
            mRows.Clear();
            mExistingTableValuedParameterForTables.Clear();
            foreach (var tvp in GeneratorEnvironment.TableValuedTypeNames)
                if (!string.IsNullOrEmpty(GeneratorHelper.FindNamespace(tvp))) mExistingTableValuedParameterForTables.Add(tvp);

            var db = mMetaData.DefaultDatabase!;
            foreach (ITable aTable in db.Tables)
            {
                string aNamespace = GeneratorHelper.FindNamespace(aTable.Name);
                if (aNamespace != string.Empty)
                {
                    var aRow = NewRow(aTable.Name, aNamespace, TYPE_TABLE);
                    aRow.PrimaryKeyIntegerColumnNames = string.Join(",", GetIntegerPrimaryAndForeignKeyColumnNames(aTable, out bool isPkInteger).ToArray());
                    aRow.IsPrimaryKeyInteger = isPkInteger;
                    mRows.Add(aRow);
                }
            }

            foreach (IView aView in db.Views)
            {
                var aRow = NewRow(aView.Name, string.Empty, TYPE_VIEW);
                aRow.PrimaryKeyIntegerColumnNames = string.Join(",", EliminateAliasesForPrimaryKeyColumns(GetIntegerPrimaryAndForeignKeyColumnNames(aView.SubTables), aView));
                mRows.Add(aRow);
            }

            // data access functions: table-valued/scalar functions named f_sel*
            foreach (IProcedure aProcedure in db.Procedures)
            {
                if (aProcedure.Type == (int)ProcedureType.Function && aProcedure.Name.StartsWith("f_sel", StringComparison.OrdinalIgnoreCase))
                    mRows.Add(NewRow(aProcedure.Name, GeneratorHelper.FindNamespace(aProcedure.Name), TYPE_FUNCTION));
            }
        }

        private void LoadSavedLocalMetadata(ref string targetPath)
        {
            var file = GeneratorEnvironment.MetaDataFile;
            if (string.IsNullOrEmpty(file) || !File.Exists(file)) return;

            var (savedTarget, saved) = MetaDataXml.Load(file);
            if (!string.IsNullOrEmpty(savedTarget) && string.IsNullOrEmpty(targetPath)) targetPath = savedTarget;

            // saved manual changes are merged into the freshly read rows; values from the database win
            foreach (var savedRow in saved)
            {
                var row = mRows.FirstOrDefault(r => r.ObjectName == savedRow.ObjectName);
                if (row == null) continue;
                row.Generate = savedRow.Generate;
                row.Type = savedRow.Type;
                if (!row.LOV && string.IsNullOrEmpty(row.LovColumns)) { row.LOV = savedRow.LOV; row.LovColumns = savedRow.LovColumns; }
                if (!row.NORMTIMEFROM && string.IsNullOrEmpty(row.NormTimeFromColumns)) { row.NORMTIMEFROM = savedRow.NORMTIMEFROM; row.NormTimeFromColumns = savedRow.NormTimeFromColumns; }
                if (!row.NORMTIMETO && string.IsNullOrEmpty(row.NormTimeToColumns)) { row.NORMTIMETO = savedRow.NORMTIMETO; row.NormTimeToColumns = savedRow.NormTimeToColumns; }
                if (!row.Stammdatenuebernahme) row.Stammdatenuebernahme = savedRow.Stammdatenuebernahme;
                if (string.IsNullOrEmpty(row.AliasKennungColumn)) row.AliasKennungColumn = savedRow.AliasKennungColumn;
                if (string.IsNullOrEmpty(row.DbReaderDataTypeColumns)) row.DbReaderDataTypeColumns = savedRow.DbReaderDataTypeColumns;
                if (string.IsNullOrEmpty(row.DbReaderOperatorColumns)) row.DbReaderOperatorColumns = savedRow.DbReaderOperatorColumns;
                if (string.IsNullOrEmpty(row.Namespace)) row.Namespace = savedRow.Namespace;
            }
        }

        private void SaveLocalMetadata()
        {
            var file = GeneratorEnvironment.MetaDataFile;
            if (!GeneratorEnvironment.SaveMetaData || string.IsNullOrEmpty(file)) return;
            MetaDataXml.Save(file, mTargetPath, mRows);
        }

        private void FillGenInfo()
        {
            // first check the Stammdatenuebernahme and tick the dependent tables
            foreach (var aRow in mRows)
            {
                if (aRow.Generate && !IgnoreStammdaten)
                {
                    if (string.IsNullOrEmpty(aRow.AliasKennungColumn)) aRow.AliasKennungColumn = "KENNUNG";
                    if (aRow.Stammdatenuebernahme && AutoSelect)
                    {
                        foreach (StammdatenuebernahmeInfo sInfo in GetForeignKeys(aRow))
                        {
                            var myRow = mRows.First(r => r.ObjectName.Equals(sInfo.ObjectName));
                            myRow.Generate = true;
                            myRow.Stammdatenuebernahme = true;
                        }
                    }
                }
            }

            mGenInfoData = new ArrayList(mRows.Count);
            foreach (var aRow in mRows)
            {
                if (!aRow.Generate) continue;
                string objectname = aRow.ObjectName;

                if (!mNeedPruefeTvpGeneration && mExistingTableValuedParameterForTables.Contains(objectname))
                    mNeedPruefeTvpGeneration = true;

                var info = new GenInfo
                {
                    ObjectName = objectname,
                    Namespace = aRow.Namespace,
                    Lov = aRow.LOV,
                    NormTimeFrom = aRow.NORMTIMEFROM,
                    NormTimeTo = aRow.NORMTIMETO,
                    Generate = aRow.Generate,
                    ObjectType = aRow.Type,
                    EntityName = aRow.EntityName,
                    Stammdatenuebernahme = aRow.Stammdatenuebernahme,
                    AliasKennungColumn = aRow.AliasKennungColumn.ToLowerInvariant(),
                    DbReaderDataTypeColumns = aRow.DbReaderDataTypeColumns,
                    DbReaderOperatorColumns = aRow.DbReaderOperatorColumns,
                    IsPrimaryKeyInteger = aRow.IsPrimaryKeyInteger,
                    IntegerPrimaryKeyColumns = aRow.PrimaryKeyIntegerColumnNames,
                };
                if (aRow.LOV) info.LovColumns = aRow.LovColumns;
                if (aRow.NORMTIMEFROM) info.NormTimeFromColumns = aRow.NormTimeFromColumns;
                if (aRow.NORMTIMETO) info.NormTimeToColumns = aRow.NormTimeToColumns;
                if (aRow.Type.Equals(TYPE_FUNCTION))
                {
                    var funcGeninfo = new FunctionGenInfo(objectname)
                    {
                        FunctionReturnValueColumns = SqlFunctionAnalyzer.GetTvfReturnColumns(objectname),
                        FunctionInputParameterColumns = GetInputParameterForFunction(objectname),
                    };
                    mFunctionGenInfos.Add(funcGeninfo);
                }
                mGenInfoData.Add(info);

                if (aRow.Stammdatenuebernahme && !IgnoreStammdaten)
                {
                    var sInfo = new StammdatenuebernahmeInfo
                    {
                        ObjectName = aRow.ObjectName,
                        Namespace = aRow.Namespace,
                        ForeignKeys = GetForeignKeys(aRow),
                        ColumnsForUniqueIndices = GetColumnsForUniqueIndices(aRow),
                        AliasKennungColumn = aRow.AliasKennungColumn,
                    };

                    // sort it in before the first entry that depends on it
                    int insertAt = mStammdatenWithForeignKeys.Count;
                    for (int i = 0; i < mStammdatenWithForeignKeys.Count; i++)
                    {
                        if (mStammdatenWithForeignKeys[i] is StammdatenuebernahmeInfo sdInfo &&
                            sdInfo.ForeignKeys.Cast<StammdatenuebernahmeInfo>().Any(s => s.ObjectName.Equals(sInfo.ObjectName)))
                        {
                            insertAt = i;
                            break;
                        }
                    }
                    mStammdatenWithForeignKeys.Insert(insertAt, sInfo);
                }
            }
        }

        private string[] GetIntegerPrimaryAndForeignKeyColumnNames(ITables pTables)
        {
            var myList = new List<string>();
            foreach (ITable table in pTables)
                myList.AddRange(GetIntegerPrimaryAndForeignKeyColumnNames(table, out _));
            myList.Sort();
            return myList.ToArray();
        }

        private List<string> GetIntegerPrimaryAndForeignKeyColumnNames(ITable table, out bool oIsPkInteger)
        {
            oIsPkInteger = false;
            var retValue = new List<string>();
            foreach (IColumn pkColumn in table.PrimaryKeys)
            {
                if (!pkColumn.DataTypeName.ToLower().Equals("uniqueidentifier") && !retValue.Contains(pkColumn.Name))
                {
                    retValue.Add(pkColumn.Name);
                    oIsPkInteger = true;
                }
            }
            foreach (IForeignKey foreignKey in table.ForeignKeys)
                foreach (IColumn fkCol in foreignKey.ForeignColumns)
                    if (!fkCol.DataTypeName.ToLower().Equals("uniqueidentifier") && !retValue.Contains(fkCol.Name))
                        retValue.Add(fkCol.Name);
            retValue.Sort();
            return retValue;
        }

        private ArrayList GetForeignKeys(MetaDataRow pMetaDataRow)
        {
            var foreignKeys = new ArrayList();
            ITable table = mMetaData.DefaultDatabase!.Tables.First(t => t.Name.Equals(pMetaDataRow.ObjectName));
            for (int i = 0; i < table.ForeignKeys.Count; i++)
            {
                IForeignKey fk = table.ForeignKeys[i];
                if (fk.PrimaryTable.Name.Equals(table.Name)) continue;

                // these FKs create chicken-and-egg problems and are not taken over
                if (pMetaDataRow.ObjectName.Equals("BA_AENDERUNGSBEMERKUNG") &&
                    (fk.PrimaryTable.Name.Equals("BA_ANWENDUNGSFALL") || fk.PrimaryTable.Name.Equals("BA_FUNKTION_ANWENDUNGSFALL_DOMAENE")))
                    continue;

                if ((pMetaDataRow.ObjectName.Equals("BA_ANWENDUNGSFALL") || pMetaDataRow.ObjectName.Equals("BA_ONLINEHILFE") ||
                     pMetaDataRow.ObjectName.Equals("BA_DOMAENE") || pMetaDataRow.ObjectName.Equals("BA_FUNKTION") ||
                     pMetaDataRow.ObjectName.Equals("BA_FUNKTION_ANWENDUNGSFALL_DOMAENE")) &&
                    (fk.PrimaryTable.Name.Equals("BA_AENDERUNGSBEMERKUNG") || fk.PrimaryTable.Name.Equals("BA_FUNKTION_ANWENDUNGSFALL_DOMAENE")))
                    continue;

                var fkMetaRow = mRows.FirstOrDefault(r => r.ObjectName.Equals(fk.PrimaryTable.Name));
                if (fkMetaRow == null) continue;
                if (!GeneratorEnvironment.AutoSelectStammdatenDependencies && !fkMetaRow.Generate) continue;

                if (foreignKeys.Cast<StammdatenuebernahmeInfo>().Any(s => s.ObjectName.Equals(fk.PrimaryTable.Name))) continue;
                if (mKeineDatenuebernahmeGenerieren.Contains(fk.PrimaryTable.Name)) continue;

                foreignKeys.Add(new StammdatenuebernahmeInfo
                {
                    ObjectName = fk.PrimaryTable.Name,
                    Namespace = fkMetaRow.Namespace,
                    AliasKennungColumn = string.IsNullOrEmpty(fkMetaRow.AliasKennungColumn) ? "KENNUNG" : fkMetaRow.AliasKennungColumn,
                });
            }
            return foreignKeys;
        }

        private ArrayList GetColumnsForUniqueIndices(MetaDataRow pMetaDataRow)
        {
            var uniqueColumns = new ArrayList();
            ITable table = mMetaData.DefaultDatabase!.Tables.First(t => t.Name.Equals(pMetaDataRow.ObjectName));
            foreach (MyMeta.Index idx in table.Indexes)
            {
                if (!idx.Unique) continue;
                foreach (IColumn col in idx.Columns)
                {
                    if (table.PrimaryKeys.Count > 0 && !col.Name.Equals(table.PrimaryKeys[0].Name) && !uniqueColumns.Contains(col.Name) &&
                        !col.Name.ToUpper().Equals("MANDANT") && !col.Name.ToUpper().Equals("BUCHUNGSKREIS"))
                        uniqueColumns.Add(col.Name);
                }
            }
            return uniqueColumns;
        }

        private void PruefeStammdatenUebernahme()
        {
            foreach (var aRow in mRows)
            {
                if (aRow.Stammdatenuebernahme)
                {
                    if (aRow.Type.Equals(TYPE_VIEW) || aRow.Type.Equals(TYPE_FUNCTION))
                    {
                        aRow.Stammdatenuebernahme = false;
                        continue;
                    }
                    if (AutoSelect && !aRow.Generate) aRow.Generate = true;

                    if (mBlGeneratorSettings.EnitaetenAusnahmeDatenuebernahme.Contains(aRow.ObjectName)) continue;

                    bool found1 = mStammdatentabellen.Any(tab => tab.Replace(" ", "_").ToLower().EndsWith(aRow.EntityName.Replace(" ", "_").ToLower()));
                    bool found2 = mStammdatentabellen.Any(tab => tab.Replace("_", "").ToLower().EndsWith(aRow.EntityName.Replace("_", "").ToLower()));
                    if (found1 || found2) continue;

                    // not a registered master-data table
                    aRow.Stammdatenuebernahme = false;
                    mKeineDatenuebernahmeGenerieren.Add(aRow.ObjectName);
                }

                if (mBlGeneratorSettings.EnitaetenPflichDatenuebernahme.Contains(aRow.ObjectName))
                {
                    if (!aRow.Stammdatenuebernahme) aRow.Stammdatenuebernahme = true;
                    if (AutoSelect && !aRow.Generate) aRow.Generate = true;
                }
            }
        }

        public List<TvfColumnInfo> GetInputParameterForFunction(string pProcedureName)
        {
            var inputParam = new List<TvfColumnInfo>();
            IProcedure procedure = mMetaData.DefaultDatabase!.Procedures.First(p => p.Name.Equals(pProcedureName));
            foreach (IParameter param in procedure.Parameters)
            {
                if (param.Direction == ParamDirection.Input)
                    inputParam.Add(new TvfColumnInfo { Name = param.Name, IsNullable = param.IsNullable, LanguageType = param.TypeName });
            }
            return inputParam;
        }

        private string[] EliminateAliasesForPrimaryKeyColumns(string[] pPrimaryAndForeignKeyColumnNamesOfTables, IView pView)
        {
            if (pPrimaryAndForeignKeyColumnNamesOfTables == null || !pPrimaryAndForeignKeyColumnNamesOfTables.Any())
                return pPrimaryAndForeignKeyColumnNamesOfTables ?? Array.Empty<string>();

            var primaryKeyColumnsOfTables = pPrimaryAndForeignKeyColumnNamesOfTables.ToList();
            foreach (Column col in pView.Columns)
            {
                if (!GeneratorHelper.IsNumeric(col, new Dictionary<string, string>())) continue;

                // views often alias id columns (e.g. X_Y_ID for Y_ID), which MyMeta doesn't know about
                string[] sepValues = col.Name.ToUpper().Split('_');
                if (sepValues.Length > 2 && sepValues[^1].Equals("ID") && !primaryKeyColumnsOfTables.Contains(col.Name.ToUpper()))
                {
                    var testColName = new System.Text.StringBuilder("ID");
                    for (int i = sepValues.Length - 1; i > 0; i--)
                    {
                        testColName.Insert(0, $"{sepValues[i - 1]}_");
                        if (primaryKeyColumnsOfTables.Contains(testColName.ToString()) && !primaryKeyColumnsOfTables.Contains(col.Name.ToUpper()))
                            primaryKeyColumnsOfTables.Add(col.Name.ToUpper());
                    }
                }
            }
            return primaryKeyColumnsOfTables.ToArray();
        }
    }
}
