using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using MyMeta;

namespace EHA.ZESA.Util.GeneratorUI
{
    /// <summary>
    /// Enum um die 
    /// </summary>
    public enum Project
    {
        Configuration,
        RemotingServiceInterface,
        Server
    }

    [Serializable]
    [Browsable(false)]
    public struct BLGeneratorSettings
    {
        public string AbweichenderTfsWorkspace;
        public string TfsServerUrl;
        public List<string> EnitaetenAusnahmeDatenuebernahme;
        public List<string> EnitaetenPflichDatenuebernahme;
    }

    /// <summary>
    /// Hilfmethoden für die Templates.
    /// </summary>
    /// <remarks>
    /// <p>Hier sind einige Hilfsmethoden versammelt, die in den Templates für die
    /// Generierung der Businesslogik verwendet werden.</p>
    /// <para><b>Historie</b></para>
    /// <div class="tablediv">
    /// <table class="dtTABLE" cellspacing="0">
    ///		<tr>
    ///			<th width="20%">Autor</th>
    ///			<th width="20%">Datum</th>
    ///			<th width="60%">Bemerkungen</th>
    ///		</tr>
    ///		<tr>
    ///			<td width="20%">Andreas Seibt</td>
    ///			<td width="20%">26.11.2004</td>
    ///			<td width="60%">Erstellt</td>
    ///		</tr>
    /// </table>
    /// </div>
    /// </remarks>
    public class GeneratorHelper
    {
        // SettingsFilename
        private static string FILENAME_LIST_TO_GENERATE => GeneratorEnvironment.ObjectsToGenerateFile;
        private static string FILENAME_SETTINGS => GeneratorEnvironment.SettingsFile;
        private static string FILENAME_MEMBER_USE_OLD_LOGIC => GeneratorEnvironment.MemberUseOldDataSetLogicFile;
        /// <summary>
        /// Tabellen- oder Spaltennamen in Groß-/Kleinschreibung umsetzen.
        /// </summary>
        /// <param name="pName">Der umzuwandelnde Name</param>
        /// <returns>Den umgewandelten Namen</returns>
        public static string ConvertName(string pName)
        {
            try
            {
                if (!string.IsNullOrEmpty(pName) && pName.ToLower().Contains(@"@p"))
                    pName = pName.ToLower().Replace(@"@p", "");

                string[] parts = pName.Split('_');
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < parts.Length; i++)
                {
                    string temp = parts[i].ToLower();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        result.Append(parts[i].Substring(0, 1).ToUpper());
                        result.Append(temp.Substring(1));
                        if (i != (parts.Length - 1))
                        {
                            result.Append("_");
                        }
                    }
                }
                return result.ToString();
            }
            catch
            {
                return pName;
            }
        }

        /// <summary>
        /// Schneidet den Präfix der Tabellennamen ab..
        /// </summary>
        /// <param name="pName">Der umzuwandelnde Name</param>
        /// <returns>Den umgewandelten Namen</returns>
        public static string BuildEntityName(string pName, string pType)
        {
            int startValue = 1;

            // Für funktionen nehmen wir den ganzen string
            if (pType.Equals(BLGeneratorUI.TYPE_FUNCTION))
                startValue = 0;

            string[] parts = pName.Split('_');
            StringBuilder result = new StringBuilder();
            for (int i = startValue; i < parts.Length; i++)
            {
                result.Append(parts[i]);
                if (i != (parts.Length - 1))
                {
                    result.Append("_");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Prüft den übergebenen Datentyp auf numerisch.
        /// </summary>
        /// <param name="pColumn">Die Spalte</param>
        /// <param name="pDbReaderDatatypeCols"></param>
        /// <returns><c>true</c> für numerisch, <c>false</c> für nicht-numerisch.</returns>
        /// <remarks>
        /// Datumstypen werden hier auch als numerisch angesehen, da für sie in der 
        /// DbReader.config und in den Parameterklassen ebenfalls zusätzliche Parameter für
        /// Von-/Bis-Abfragen generiert werden müssen.
        /// <p>Eigentlich müsste ich mir einen sinnvolleren Namen überlegen ;-(</p>
        /// </remarks>
        public static bool IsNumeric(IColumn pColumn, Dictionary<string, string> pDbReaderDatatypeCols)
        {
            // wir prüfen auf den übersteuernden Datentyp
            string temp = pColumn.LanguageType.ToLower();
            if (pDbReaderDatatypeCols.ContainsKey(pColumn.Name))
                temp = pDbReaderDatatypeCols[pColumn.Name].ToLower();

            if (temp == "decimal") return true;
            else if (temp == "long") return true;
            else if (temp == "double") return true;
            else if (temp == "int") return true;
            else if (temp == "float") return true;
            else if (temp == "short") return true;
            else if (temp == "byte") return true;
            // auch für Datum muss Von-/Bis-Parameter generiert werden
            else if (temp == "datetime") return true;
            else return false;
        }

        /// <summary>
		/// Prüft den übergebenen Datentyp auf numerisch.
		/// </summary>
        /// <param name="pColumn">Die Spalte</param>
		/// <param name="pDbReaderDatatypeCols"></param>
		/// <returns><c>true</c> für numerisch, <c>false</c> für nicht-numerisch.</returns>
		/// <remarks>
		/// Datumstypen werden hier auch als numerisch angesehen, da für sie in der 
		/// DbReader.config und in den Parameterklassen ebenfalls zusätzliche Parameter für
		/// Von-/Bis-Abfragen generiert werden müssen.
		/// <p>Eigentlich müsste ich mir einen sinnvolleren Namen überlegen ;-(</p>
		/// </remarks>
        public static bool IsNumeric(TvfColumnInfo pColumn, Dictionary<string, string> pDbReaderDatatypeCols)
        {
            // wir prüfen auf den übersteuernden Datentyp
            string temp = pColumn.LanguageType.ToLower();
            if (pDbReaderDatatypeCols.ContainsKey(pColumn.Name))
                temp = pDbReaderDatatypeCols[pColumn.Name].ToLower();

            if (temp == "decimal") return true;
            else if (temp == "long") return true;
            else if (temp == "double") return true;
            else if (temp == "bit") return true;
            else if (temp == "int") return true;
            else if (temp == "smallint") return true;
            else if (temp == "tinyint") return true;
            else if (temp == "float") return true;
            else if (temp == "short") return true;
            else if (temp == "byte") return true;
            // auch für Datum muss Von-/Bis-Parameter generiert werden
            else if (temp == "datetime") return true;
            else return false;
        }

        /// <summary>
        /// Hilfroutine zum Ausrichten der Geleicheitszeichen bei Konstantendefinitionen.
        /// </summary>
        /// <param name="pName">Die auszurichtende Konstante</param>
        /// <param name="pMaxLength">Die maximale Länge</param>
        /// <returns></returns>
        public static string FillUp(string pName, int pMaxLength)
        {
            int diff = pMaxLength - pName.Length;
            StringBuilder result = new StringBuilder("");
            for (int i = 0; i < diff; i++)
            {
                result.Append(" ");
            }
            return result.ToString();
        }

        /// <summary>
        /// Ermittelt den längsten Spaltennamen in einer Tabelle
        /// </summary>
        /// <param name="pColumns">Auflistung der Spalten aus MyMeta</param>
        /// <returns>Die Länge der längsten Spalte</returns>
        public static int MaxColLength(IColumns pColumns)
        {
            int maxLength = 0;
            foreach (Column col in pColumns)
            {
                if (col.Name.Length > maxLength)
                {
                    maxLength = col.Name.Length;
                }
            }
            return maxLength;
        }

        /// <summary>
		/// Ermittelt den längsten Spaltennamen in einer Function
		/// </summary>
		/// <param name="pColumns">Auflistung der Spalten aus MyMeta</param>
		/// <returns>Die Länge der längsten Spalte</returns>
		public static int MaxColLength(FunctionGenInfo pFunctionGenInfo)
        {
            int maxLength = 0;
            foreach (TvfColumnInfo col in pFunctionGenInfo.FunctionInputParameterColumns)
            {
                if (col.Name.Length > maxLength)
                {
                    maxLength = col.Name.Length;
                }
            }

            foreach (TvfColumnInfo col in pFunctionGenInfo.FunctionReturnValueColumns)
            {
                if (col.Name.Length > maxLength)
                {
                    maxLength = col.Name.Length;
                }
            }
            return maxLength;
        }

        /// <summary>
        /// Deserialisiert ZesaOnlineHilfeStruct und gibt diesen zurück
        /// </summary>
        /// <returns></returns>
        public static BLGeneratorSettings DeserializeSettings()
        {
            try
            {
                // die neuen Settings holen oder Zusammenbauen
                FileInfo fi = new FileInfo(string.IsNullOrEmpty(FILENAME_SETTINGS) ? "BLGeneratorUISettings.xml" : FILENAME_SETTINGS);
                BLGeneratorSettings ret;
                if (!fi.Exists)
                {
                    ret = new BLGeneratorSettings();
                    // TFS
                    ret.AbweichenderTfsWorkspace = string.Empty;
                    ret.TfsServerUrl = string.Empty;
                    // Ausnahmen für Stammdatenübernahme
                    ret.EnitaetenAusnahmeDatenuebernahme = new List<string>(1);
                    ret.EnitaetenAusnahmeDatenuebernahme.Add("Ausnahmen (Tabellenname) hier eintragen");
                    // Stammdatenübernahme Pflich
                    ret.EnitaetenPflichDatenuebernahme = new List<string>(5);
                    ret.EnitaetenPflichDatenuebernahme.Add("BA_ANWENDUNGSFALL");
                    ret.EnitaetenPflichDatenuebernahme.Add("BA_DOMAENE");
                    ret.EnitaetenPflichDatenuebernahme.Add("BA_FUNKTION");
                    ret.EnitaetenPflichDatenuebernahme.Add("BA_FUNKTION_ANWENDUNGSFALL_DOMAENE");
                    ret.EnitaetenPflichDatenuebernahme.Add("BA_ONLINEHILFE");
                }
                else
                {

                    XmlSerializer serializer = new XmlSerializer(typeof(BLGeneratorSettings));
                    TextReader tr = new StreamReader(fi.FullName);
                    ret = (BLGeneratorSettings)serializer.Deserialize(tr);
                    tr.Close();
                }
                // Wert zurückgeben
                return ret;
            }
            catch
            {
                return new BLGeneratorSettings();
            }
        }


        /// <summary>
        /// Serialisiert einen ZesaOnlineHilfeStruct,
        /// </summary>
        /// <param name="pInParam">ZesaOnlineHilfe</param>
        public static void SerializeSettings(BLGeneratorSettings pInParam)
        {
            if (string.IsNullOrEmpty(FILENAME_SETTINGS))
                return;
            try
            {
                FileInfo fi = new FileInfo(FILENAME_SETTINGS);
                if (fi.Exists && fi.IsReadOnly)
                {
                    fi.IsReadOnly = false;
                }

                // serialisieren
                XmlSerializer serializer = new XmlSerializer(typeof(BLGeneratorSettings));
                TextWriter tw = new StreamWriter(FILENAME_SETTINGS);
                serializer.Serialize(tw, pInParam);
                tw.Close();
            }
            catch
            {
                // nix zu tun...
            }
        }

        /// <summary>
        /// fügt einen Eintrag für neue Dateien einer Projektdatei hinzu
        /// </summary>
        public static void AddFileToProject(Project pProject, string pInsertString)
        {
            string configFile = BLGeneratorUI.PathTfsRepos;

            switch (pProject)
            {
                case Project.Configuration:
                    configFile += "ZESAConfiguration\\ZESAConfiguration\\ZESAConfiguration.csproj";
                    break;
                case Project.RemotingServiceInterface:
                    configFile += "Communication\\RemotingServiceInterface\\RemotingServiceInterface.csproj";
                    break;
                case Project.Server:
                    configFile += "Server\\ZESAServer\\ZESAServer.csproj";
                    break;
                default:
                    return;
            }

            configFile = configFile.Replace('\\', Path.DirectorySeparatorChar);

            // Prüfen ob der Pfad korrekt ist
            if (!File.Exists(configFile))
                return;

            // String Builder zum bauen der Config
            StringBuilder configBuilder = new StringBuilder();

            // Config einlesen
            string[] myConfigLines = File.ReadAllLines(configFile, Encoding.UTF8);

            // das einzufügende Element ist für die Configs Unterschiedlich
            string myInsert;
            if (pProject == Project.Configuration)
            {
                XElement element = new XElement("None");
                XAttribute attribute = new XAttribute("Include", pInsertString);
                element.Add(attribute);
                myInsert = element.ToString();
            }
            else
            {
                XElement element = new XElement("Compile");
                XAttribute attribute = new XAttribute("Include", pInsertString);
                element.Add(attribute);
                myInsert = element.ToString();
            }

            // wir gehen durch die Config
            string searchStartString = pProject == Project.Configuration ? "<None Include" : "<Compile Include";
            bool inserted = false;
            for (int i = 0; i < myConfigLines.Length; i++)
            {
                // Wenn die Zeile mit unserem SearchString beginnt ist das unser Bereich
                if (myConfigLines[i].Trim().StartsWith(searchStartString))
                {
                    // wir zählen führende Whitepsaces
                    int whtspaces = myConfigLines[i].Length - myConfigLines[i].TrimStart().Length;

                    // und sortieren den neuen Eintrag in die bestehenden ein
                    string myLine = myConfigLines[i].Trim();
                    int vergleich = String.Compare(myLine, myInsert, StringComparison.OrdinalIgnoreCase);
                    if (vergleich > 0 && !inserted)
                    {
                        configBuilder.AppendLine(myInsert.PadLeft(myInsert.Length + whtspaces));
                        inserted = true;
                    }
                }
                if (i + 1 < myConfigLines.Length)
                {
                    configBuilder.AppendLine(myConfigLines[i]);
                }
                else
                {
                    configBuilder.Append(myConfigLines[i]);
                }
            }
            // Am Ende speichern wird ie datei ab
            File.WriteAllText(configFile, configBuilder.ToString(), Encoding.UTF8);
        }

        // holt die übersteuernden Datentypen für die DBReader Generierung
        public static Dictionary<string, string> GetDbReaderDatatypeColumns(GenInfo info)
        {
            Dictionary<string, string> retValueDatatype = new Dictionary<string, string>();
            string[] dbreaderDatatypeColumns = info.DbReaderDataTypeColumns.Split(',');
            foreach (string datatypeColumn in dbreaderDatatypeColumns)
            {
                if (string.IsNullOrEmpty(datatypeColumn))
                    continue;

                string colName = datatypeColumn.Substring(0, datatypeColumn.IndexOf('<'));
                colName = colName.ToUpper().Trim();
                string datatype = datatypeColumn.Substring(datatypeColumn.IndexOf('<'));
                datatype = datatype.Replace("<", "");
                datatype = datatype.Replace(">", "");
                datatype = datatype.ToUpper().Trim();
                if (!retValueDatatype.ContainsKey(colName))
                    retValueDatatype.Add(colName, datatype);
            }

            // wir setzen noch die Datentypen für Integer PK-Columns manuell auf long
            string[] integerPrimaryKeyColumns = info.IntegerPrimaryKeyColumns.Split(',');
            foreach (string pkColumn in integerPrimaryKeyColumns)
            {
                if (string.IsNullOrEmpty(pkColumn))
                    continue;

                string colName = pkColumn.ToUpper().Trim();

                if (!retValueDatatype.ContainsKey(colName))
                    retValueDatatype.Add(colName, "LONG");
            }

            return retValueDatatype;
        }

        // holt die übersteuernden Operatoren für die DBReader Generierung
        public static Dictionary<string, string> GetDbReaderOperatorColumns(GenInfo info)
        {
            Dictionary<string, string> retValueOperator = new Dictionary<string, string>();
            string[] dbreaderOperatorColumns = info.DbReaderOperatorColumns.Split(',');
            foreach (string operatorColumn in dbreaderOperatorColumns)
            {
                if (string.IsNullOrEmpty(operatorColumn))
                    continue;

                string colName = operatorColumn.Substring(0, operatorColumn.IndexOf('<'));
                colName = colName.ToUpper().Trim();
                string operatorCol = operatorColumn.Substring(operatorColumn.IndexOf('<'));
                operatorCol = operatorCol.Replace("<", "");
                operatorCol = operatorCol.Replace(">", "");
                operatorCol = operatorCol.ToUpper().Trim();
                if (!retValueOperator.ContainsKey(colName))
                    retValueOperator.Add(colName, operatorCol);
            }
            return retValueOperator;
        }

        /// <summary>
        /// Ermittelt den Namespace für die Businesslogik, die zu einer
        /// Tabelle generiert wird.
        /// </summary>
        /// <param name="pDataObjectName">Der Name des Datenbankobjektes (Tabelle, View, TVP, etc)</param>
        public static string FindNamespace(string pDataObjectName)
        {
            if (pDataObjectName.Contains("HI_ABRPARAM_"))
                return "Kundenabrechnung";

            if (pDataObjectName.StartsWith("f_sel_"))
                pDataObjectName = pDataObjectName.Substring(6);

            string[] parts = pDataObjectName.Split('_');
            switch (parts[0].ToUpper())
            {
                case "BA":
                    return "Basis";
                case "GP":
                    return "Geschaeftspartner";
                case "KO":
                    return "Koma";
                case "KA":
                    return "Kundenabrechnung";
                case "SY":
                    return "Basis";
                case "TA":
                    return "PreismodelleKonditionen";
                case "TS":
                    return "TechnischeStruktur";
                case "TT":
                    return "Ticketing";
                case "VT":
                    return "Vertrag";
                case "SST":
                    return "Schnittstellen";
                case "LV":
                    return "Lagerverwaltung";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Deserialisert die Liste mit den Membern die die alte DataSet Logik benutzen
        /// </summary>
        /// <returns></returns>
        public static SortedList GetMemberWhichUseOldDataSetLogic()
        {
            if (string.IsNullOrEmpty(FILENAME_MEMBER_USE_OLD_LOGIC) || !File.Exists(FILENAME_MEMBER_USE_OLD_LOGIC))
                return new SortedList();

            XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
            TextReader tr = new StreamReader(FILENAME_MEMBER_USE_OLD_LOGIC);


            List<string> tempMembers = (List<string>)serializer.Deserialize(tr);
            tr.Close();

            SortedList retValue = new SortedList(tempMembers.Count);
            foreach (string member in tempMembers)
                retValue.Add(member, member);

            return retValue;
        }

        public static List<string> GetObjectsToGenerate()
        {
            // Rückgabe
            List<string> retValue = new List<string>();
            // Datei einlesen
            if (!string.IsNullOrEmpty(FILENAME_LIST_TO_GENERATE) && File.Exists(FILENAME_LIST_TO_GENERATE))
            {
                StreamReader file = new StreamReader(FILENAME_LIST_TO_GENERATE);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    retValue.Add(line);
                }
                // Datei schliessen
                file.Close();

                // Liste sortieren
                retValue.Sort();
            }
            // und zurückgeben
            return retValue;
        }

        /// <summary>
        /// gibt zu einer Funktion die Informationen über den Rückgabewert zurück
        /// </summary>
        /// <param name="pProcedureName"></param>
        /// <returns></returns>
        public static List<TvfColumnInfo> GetReturnValueColumnsForFunction(string pProcedureName)
        {
            return SqlFunctionAnalyzer.GetTvfReturnColumns(pProcedureName);
        }

    }
}
