using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BlGeneratorMetaMigra
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
            Console.WriteLine("zum Starten der BlGeneratorMeta Migration: Hit Return :-)");
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
            if (!File.Exists("BLGeneratorUIMetaQuelle.xml") || !File.Exists("BLGeneratorUIMetaZiel.xml"))
                return;
            
            // Configs laden
            XDocument xmlConfigQuelle = XDocument.Load("BLGeneratorUIMetaQuelle.xml");

            XDocument xmlConfigZiel = XDocument.Load("BLGeneratorUIMetaZiel.xml");

            // alle Querys raussuchen
            var metaQuelle = from metadata in xmlConfigQuelle.Descendants("MetaData")
                         select metadata;

            var metaZiel = from metadata in xmlConfigZiel.Descendants("MetaData")
                             select metadata;

            // each Meta
            foreach (XElement elementQuelle in metaQuelle)
            {
                
                // pruefe auf Objectname und Namespace
                var viewElementsQuelle = from elements in elementQuelle.Elements()
                          where (elements.Name.ToString().Equals("ObjectName") && elements.Value.StartsWith("V_")) 
                          select elements.Parent;

                if(viewElementsQuelle.Count() <= 0)
                    continue;

                foreach(XElement xElementQuelle in viewElementsQuelle)
                {
                    string objectNameQuelle = string.Empty;
                    string namespaceNameQuelle = string.Empty;
                    foreach(XElement xeQuelle in xElementQuelle.Elements())
                    {
                        if(xeQuelle.Name.ToString().Equals("ObjectName"))
                        {
                            objectNameQuelle = xeQuelle.Value;
                        }
                        
                        if(xeQuelle.Name.ToString().Equals("Namespace"))
                        {
                            if(!string.IsNullOrEmpty(xeQuelle.Value))
                            {
                                namespaceNameQuelle = xeQuelle.Value;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(objectNameQuelle) && !string.IsNullOrEmpty(namespaceNameQuelle))
                    {
                        // in Ziel einsetzen
                        foreach (XElement element in metaZiel)
                        {

                            // pruefe auf Objectname und Namespace
                            var viewElements = from elements in element.Elements()
                                               where (elements.Name.ToString().Equals("ObjectName") && elements.Value.StartsWith("V_"))
                                               select elements.Parent;

                            if (viewElements.Count() <= 0)
                                continue;

                            foreach (XElement no in viewElements)
                            {
                                string objectName = string.Empty;
                                foreach (XElement xe in no.Elements())
                                {
                                    if (xe.Name.ToString().Equals("ObjectName"))
                                    {
                                        objectName = xe.Value;
                                    }

                                    if (xe.Name.ToString().Equals("Namespace"))
                                    {
                                        if (string.IsNullOrEmpty(xe.Value) && objectNameQuelle.Equals(objectName))
                                        {
                                            xe.Value = namespaceNameQuelle;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            xmlConfigZiel.Save("_NEU_BLGeneratorUIMeta.xml");
            Console.Write("Done...");
        }
    }
}