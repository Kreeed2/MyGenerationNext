using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BlGeneratorProjectFiles
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
            Console.WriteLine("zum Starten der ProjectFile Generation: Hit Return :-)");
            Console.Read();
            
            // prüfen ob Verzeichnis besteht
            DirectoryInfo dirInfo = new DirectoryInfo("Configurations");
            if (!dirInfo.Exists)
                dirInfo.Create();

            // Verzeichnis leeren
            FileInfo[] existingFiles = dirInfo.GetFiles();
            foreach(FileInfo fi in existingFiles)
                fi.Delete();

            // Debug Stopwatch
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Config verarbeiten
            ProcessConfigurationFile();

            sw.Stop();
            Console.WriteLine();
            Console.WriteLine(string.Format("Das alles hat insgesamt {0} sec. gedauert, zum Beenden: Hit Return :-) ",sw.ElapsedMilliseconds / 1000m));

            Console.ReadKey();
        }

        /// <summary>
        /// Verarbeitet die DbReader.config sofern sie vorhanden ist.
        /// </summary>
        private static void ProcessConfigurationFile()
        {
            //XAttribute a = new XAttribute("id", "Grafik_x0020_4");
            //string hu = a.ToString();
            
            
            if (!File.Exists("ZESAConfiguration_pre.csproj"))
                return;

            // Config einlesen
            string[] myConfigLines = File.ReadAllLines("ZESAConfiguration_pre.csproj");
            // String Builder zum bauen der Config
            StringBuilder configBuilder = new StringBuilder();
           
            // das einzufügende Element
            XElement element = new XElement("None");
            XAttribute attribute = new XAttribute("Include", @"Configuration\Allgemein\Database\TA_NNEAT_NNENTGELT_Sicherung.config");
            element.Add(attribute);
            string myInsert = element.ToString();
            bool inserted = false;
            
            // wir gehen durch die Config
            for(int i = 0; i < myConfigLines.Length; i++)
            {
                // Wenn die Zeile mit Non Include beginnt ist das unser Berecih für das Configuration Projekt
                if(myConfigLines[i].Trim().StartsWith("<None Include"))
                {
                    // wir zählen führende Whitepsaces
                    int whtspaces = myConfigLines[i].Length - myConfigLines[i].TrimStart().Length;
                    
                    // und sortieren den neuen Eintrag in die bestehenden ein
                    string myLine = myConfigLines[i].Trim();
                    int vergleich = String.Compare(myLine, myInsert, StringComparison.OrdinalIgnoreCase);
                    if (vergleich > 0 && !inserted)
                    {
                        configBuilder.AppendLine(myInsert.PadLeft(myInsert.Length+whtspaces));
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
            File.WriteAllText("ZESAConfiguration_post.csproj", configBuilder.ToString());



            //xmlConfigZiel.Save("_NEU_BLGeneratorUIMeta.xml");
            Console.Write("Done...");
        }
    }
}