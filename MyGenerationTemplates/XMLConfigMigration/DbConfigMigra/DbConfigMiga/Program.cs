using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DbConfigMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            ErstelleEinzelConfigs();
        }

        /// <summary>
        /// Erstellt die Einzelconfigs
        /// </summary>
        private static void ErstelleEinzelConfigs()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("zum Starten der DbConfig Migration: Hit Return :-)");
            Console.Read();
            
            // prüfen ob Verzeichnis besteht
            DirectoryInfo dirInfo = new DirectoryInfo("DbReader_Configurations");
            if (!dirInfo.Exists)
                dirInfo.Create();

            // Verzeichnis leeren
            FileInfo[] existingFiles = dirInfo.GetFiles();
            foreach(FileInfo fi in existingFiles)
                fi.Delete();

            // Debug Stopwatch
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // DbReader verarbeiten
            ProcessDbReaderConfigurationFile();

            // DbWriter verarbeiten
            ProcessDbWriterConfigurationFile();

            // Stammelemente und Knoten einfügen
            ProcessEinzelConfigsAddStammelementUndKnoten(dirInfo);

            sw.Stop();
            Console.WriteLine();
            Console.WriteLine(string.Format("Das alles hat insgesamt {0} sec. gedauert, zum Beenden: Hit Return :-) ",sw.ElapsedMilliseconds / 1000m));

            Console.ReadKey();
        }

        /// <summary>
        /// Verarbeitet die DbReader.config sofern sie vorhanden ist.
        /// </summary>
        private static void ProcessDbReaderConfigurationFile()
        {
            if(!File.Exists("DbReader.config"))
                return;
            
            // Config laden
            XDocument xmlConfig = XDocument.Load("DbReader.config");
            // alle Querys raussuchen
            var queries = from query in xmlConfig.Descendants("Query")
                          select query;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine(string.Format("Verarbeite {0} DbReaderConfig Elemente (Querys)", queries.Count()));
           
            foreach (XElement element in queries)
            {
                XNode prevNode = element.PreviousNode;
                string dateiname = string.Empty;
                if (prevNode.NodeType.ToString() == "Comment")
                {
                    string val = prevNode.ToString();
                    if (!val.Contains("Beginn der Definition"))
                        val = element.FirstAttribute.Value;

                    val = val.Replace("<!--", string.Empty);
                    int startIndex = val.IndexOf('-', 4);
                    if (startIndex >= 0)
                    {
                        string ersetze = val.Substring(val.IndexOf('-', 4));
                        val = val.Replace(ersetze, string.Empty);
                    }
                    val = val.Replace("_List", string.Empty);
                    dateiname = val.Trim().ToUpper() + ".config";
                }
                else
                {
                    // alternative Findung des Dateinamens
                    XAttribute attrib = element.Attribute("id") ?? element.Attribute("ID");
                    if (null != attrib)
                        dateiname = attrib.Value.Trim().ToUpper() + ".config";
                }

                if (!string.IsNullOrEmpty(dateiname))
                    File.WriteAllText(Environment.CurrentDirectory + "\\DbReader_Configurations\\" + dateiname, element + Environment.NewLine + Environment.NewLine);

                Console.Write(".");
            }
        }

        /// <summary>
        /// Verarbeitet die DbWriter.config sofern sie vorhanden ist.
        /// </summary>
        private static void ProcessDbWriterConfigurationFile()
        {
            if (!File.Exists("DbWriter.config"))
                return;
            
            // DBWriter DataAdapter
            XDocument xmlConfig = XDocument.Load("DbWriter.config");
            // alle DataAdapter raussuchen
            var queries = from query in xmlConfig.Descendants("DataAdapter")
                      select query;

            Console.WriteLine();
            Console.WriteLine(string.Format("Verarbeite {0} DbWriterConfig Elemente (DataAdapter)", queries.Count()));

            foreach (XElement element in queries)
            {
                XNode prevNode = element.PreviousNode;
                string dateiname = string.Empty;
                if (prevNode.NodeType.ToString() == "Comment")
                {
                    string val = prevNode.ToString();
                    if (!val.Contains("Beginn der Definition"))
                        val = element.FirstAttribute.Value;

                    val = val.Replace("<!--", string.Empty);
                    int startIndex = val.IndexOf('-', 4);
                    if (startIndex >= 0)
                    {
                        string ersetze = val.Substring(val.IndexOf('-', 4));
                        val = val.Replace(ersetze, string.Empty);
                    }
                    dateiname = val.Trim().ToUpper() + ".config";
                }
                else
                {
                    // alternative Findung des Dateinamens
                    XAttribute attrib = element.Attribute("id") ?? element.Attribute("ID");
                    if (null != attrib)
                        dateiname = attrib.Value.Trim().ToUpper() + ".config";
                }

                if (!string.IsNullOrEmpty(dateiname))
                    File.AppendAllText(Environment.CurrentDirectory + "\\DbReader_Configurations\\" + dateiname, element + Environment.NewLine + Environment.NewLine);


                Console.Write(".");
            }

            // DBWriter SqlCommand
            xmlConfig = XDocument.Load("DbWriter.config");
            // alle SqlCommands raussuchen
            queries = from query in xmlConfig.Descendants("SqlCommand")
                      select query;

            Console.WriteLine();
            Console.WriteLine(string.Format("Verarbeite {0} DbWriterConfig Elemente (SqlCommands)", queries.Count()));

            foreach (XElement element in queries)
            {
                XNode prevNode = element.PreviousNode;
                string dateiname = string.Empty;
                if (prevNode.NodeType.ToString() == "Comment")
                {
                    string val = prevNode.ToString();
                    if (!val.Contains("Beginn der Definition"))
                        val = element.FirstAttribute.Value;

                    val = val.Replace("<!--", string.Empty);
                    int startIndex = val.IndexOf('-', 4);
                    if (startIndex >= 0)
                    {
                        string ersetze = val.Substring(val.IndexOf('-', 4));
                        val = val.Replace(ersetze, string.Empty);
                    }
                    dateiname = val.Trim().ToUpper() + ".config";
                }
                else
                {
                    // alternative Findung des Dateinamens
                    XAttribute attrib = element.Attribute("id") ?? element.Attribute("ID");
                    if (null != attrib)
                        dateiname = attrib.Value.Trim().ToUpper() + ".config";
                }

                if (!string.IsNullOrEmpty(dateiname))
                    File.AppendAllText(Environment.CurrentDirectory + "\\DbReader_Configurations\\" + dateiname, element.ToString());

                Console.Write(".");
            }
        }

        /// <summary>
        /// Verarbeitet die Einzelcondfigs und fügt das fehlende Stammelemenet und fehlende Knoten ein
        /// </summary>
        private static void ProcessEinzelConfigsAddStammelementUndKnoten(DirectoryInfo pDirInfo)
        {
            // gehe durch alle Configs und Füge Stammelement hinzu
            FileInfo[] configFiles = pDirInfo.GetFiles("*.config");

            Console.WriteLine();
            Console.WriteLine(string.Format("Verarbeite {0} Config Elemente (Einfügen von Stammelementen)", configFiles.Count()));
            foreach (FileInfo fi in configFiles)
            {
                string fileInhalt = File.ReadAllText(fi.FullName);
                fileInhalt = "<configuration>" + fileInhalt + "</configuration>";
                File.WriteAllText(fi.FullName, fileInhalt);
                Console.Write(".");
            }

            // gehe Durch alle Dateien und füge DbReader / DbWriter Knoten ein
            Console.WriteLine();
            Console.WriteLine(string.Format("Verarbeite {0} Config Elemente (Einfügen von Knoten)", configFiles.Count()));
            XDocument xmlConfig;
            foreach (FileInfo fi in configFiles)
            {
                xmlConfig = XDocument.Load(fi.FullName);

                // Knoten Suchen
                // alle SqlCommands raussuchen
                var queriesDbReader = from query in xmlConfig.Descendants("Query")
                                      select query;

                var queriesDbWriterdataAdapter = from query in xmlConfig.Descendants("DataAdapter")
                                                 select query;

                var queriesDbWriterSqlCommand = from query in xmlConfig.Descendants("SqlCommand")
                                                select query;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("<configuration>");
                if (queriesDbReader.Count() > 0)
                {
                    sb.AppendLine("<DbReader>");
                    sb.AppendLine("<DataAccess>");
                    sb.AppendLine("<Queries>");
                }
                foreach (XElement element in queriesDbReader)
                {
                    sb.AppendLine(element.ToString());
                }
                if (queriesDbReader.Count() > 0)
                {
                    sb.AppendLine("</Queries>");
                    sb.AppendLine("</DataAccess>");
                    sb.AppendLine("</DbReader>");
                }
                sb.AppendLine();

                if (queriesDbWriterdataAdapter.Count() > 0 || queriesDbWriterSqlCommand.Count() > 0)
                    sb.AppendLine("<DbWriter>");

                if (queriesDbWriterdataAdapter.Count() > 0)
                    sb.AppendLine("<DataAdapters>");
                foreach (XElement element in queriesDbWriterdataAdapter)
                {
                    sb.AppendLine(element.ToString());
                }
                if (queriesDbWriterdataAdapter.Count() > 0)
                    sb.AppendLine("</DataAdapters>");
                sb.AppendLine();

                if (queriesDbWriterSqlCommand.Count() > 0)
                    sb.AppendLine("<SqlCommands>");
                foreach (XElement element in queriesDbWriterSqlCommand)
                {
                    sb.AppendLine(element.ToString());
                }
                if (queriesDbWriterSqlCommand.Count() > 0)
                    sb.AppendLine("</SqlCommands>");

                if (queriesDbWriterdataAdapter.Count() > 0 || queriesDbWriterSqlCommand.Count() > 0)
                    sb.AppendLine("</DbWriter>");

                sb.AppendLine("</configuration>");

                XmlTextWriter xmlWriter = new XmlTextWriter(fi.FullName, Encoding.UTF8);
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteRaw(sb.ToString());
                xmlWriter.Flush();
                xmlWriter.Close();

                Console.Write(".");
            }
        }
    }
}