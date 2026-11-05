using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DbConfigMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            ErstelleEinzelConfigs();
        }

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
                
            // Config laden
            XDocument xmlConfig = XDocument.Load("DbReader.config");
            // alle Querys raussuchen
            var queries = from query in xmlConfig.Descendants("Query")
                          select query;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine(string.Format("Verarbeite {0} DbReaderConfig Elemente (Querys)", queries.Count()));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (XElement element in queries)
            {
                XNode prevNode = element.PreviousNode;
                string dateiname = string.Empty;
                if(prevNode.NodeType.ToString() == "Comment")
                {
                    string val = prevNode.ToString();
                    if (!val.Contains("Beginn der Definition"))
                        val = element.FirstAttribute.Value;
                      
                    val = val.Replace("<!--", string.Empty);
                    int startIndex = val.IndexOf('-', 4);
                    if(startIndex >= 0)
                    {
                        string ersetze = val.Substring(val.IndexOf('-', 4));
                        val = val.Replace(ersetze, string.Empty);
                    }
                    val = val.Replace("_List", string.Empty);
                    dateiname = val.Trim().ToUpper() + ".config";
                }
                
                if(!string.IsNullOrEmpty(dateiname))
                    File.WriteAllText(Environment.CurrentDirectory + "\\DbReader_Configurations\\" + dateiname, element + Environment.NewLine + Environment.NewLine);

                Console.Write(".");
            }

            // DBWriter DataAdapter
            xmlConfig = XDocument.Load("DbWriter.config");
            // alle DataAdapter raussuchen
            queries = from query in xmlConfig.Descendants("DataAdapter")
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

                if (!string.IsNullOrEmpty(dateiname))
                    File.AppendAllText(Environment.CurrentDirectory + "\\DbReader_Configurations\\" + dateiname, element.ToString());

                Console.Write(".");
            }


            sw.Stop();
            Console.WriteLine();
            Console.WriteLine(string.Format("Das alles hat insgesamt {0} sec. gedauert, zum Beenden: Hit Return :-) ",sw.ElapsedMilliseconds / 1000m));

            Console.ReadKey();
        }
    }
}