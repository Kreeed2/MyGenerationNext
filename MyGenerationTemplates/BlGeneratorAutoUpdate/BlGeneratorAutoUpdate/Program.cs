using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BlGeneratorAutoUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // DEBUG ONLY!
                //ConsoleMessage("Debugging, Hit Enter when ready...", true);

                if (args.Length <= 0)
                {
                    ConsoleMessage("Ungültige Parameter übergeben!", true);
                    return;

                    //// DEBUG ONLY!
                    //const string arg =
                    //    "D:\\TFS64Repos\\ZESA\\Utilities\\MyGenerationTemplates\\BusinesslogicGeneratorUI\\GeneratorDll\\Version2.2.0" +
                    //    "," + "C:\\Program Files (x86)\\MyGeneration13";
                    //args = new string[1];
                    //args[0] = arg;
                }

                Console.WriteLine("Führe Update von BlGenerator durch.");
                Thread.Sleep(10000);

                // prüfen ob MyGeneration noch läuft, wenn ja, beenden
                Process[] processes = Process.GetProcessesByName("MyGeneration.exe");
                if (processes.Length > 0)
                    foreach (Process p in processes)
                        p.Kill();

                string source = string.Empty;
                string destination = string.Empty;
                if(args.Length < 2)
                {
                    string[] paras;
                    if(args[0].Contains(","))
                    {
                        paras = args[0].Split(',');
                    }
                    else
                    {
                        ConsoleMessage("Ungültige Parameter übergeben!", true);
                        return;
                    }
                    
                    if(paras.Length > 1)
                    {
                        source = paras[0];
                        for(int i = 1; i < paras.Length; i++)
                        {
                            destination += paras[i];
                            if (i < paras.Length-1)
                                destination += " ";
                        }
                    }
                }
                else if(args.Length == 2)
                {
                    // wir bekommen als 1. Argument den Ort der MyGeneration Ausführung mit
                    source = args[0];
                    // wir bekommen als 2. Argument den Speicherort der BlGeneartor DLL mit
                    destination = args[1];
                }
                else if(args.Length > 2)
                {
                    string[] paras = args[0].Split(',');
                    if (paras.Length > 1)
                    {
                        source = paras[0];
                        for (int i = 1; i < paras.Length; i++)
                        {
                            destination += paras[i];
                            destination += " ";
                        }
                    }
                    for(int i = 1;i < args.Length; i++)
                    {
                        destination += args[i];
                        destination += " ";
                    }
                }

                if(string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
                {
                    ConsoleMessage("Ungültige Parameter übergeben!", true);
                    return;
                }

                source = source.TrimEnd();
                destination = destination.TrimEnd();
                
                if (!source.EndsWith("\\")) source += "\\";
                if (!destination.EndsWith("\\")) destination += "\\";

                DirectoryInfo dirInfo = new DirectoryInfo(source);
                FileInfo[] files = dirInfo.GetFiles("*.dll");

                foreach (FileInfo sourceFileInfo in files)
                {
                    FileInfo destFileInfo = new FileInfo(destination + sourceFileInfo.Name);
                    if (destFileInfo.Exists && destFileInfo.IsReadOnly)
                    {
                        destFileInfo.IsReadOnly = false;
                    }
                    sourceFileInfo.CopyTo(destination + sourceFileInfo.Name, true);
                }

                // nach dem kopieren starten wir myGeneration neu
                Process.Start(destination + "\\MyGeneration.exe");

                ConsoleMessage("Der BlGenerator ist nun auf dem neuesen Stand", false);
            }
            catch (Exception e)
            {
                ConsoleMessage("Es ist folgender Fehler aufgetreten: " + e, true);
            }
        }

        // wir geben eine Message auf der konsole aus
        private static void ConsoleMessage(string pText, bool pError)
        {
            ConsoleWriteNewLine(2);
            Console.ForegroundColor = pError ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(pText);
            ConsoleWriteNewLine(2);
            Console.ForegroundColor = ConsoleColor.White;
            
            if(pError)
            {
                Console.WriteLine("Bitte drücken Sie 'Enter' zum Beenden");
                Console.Read();
            }
            else
            {
                Console.WriteLine("Das Fenster schliesst automatisch");
                Thread.Sleep(3000);
            }
        }

        // wir schreiben x-mal eine neue Zeile in die Konsole
        private static void ConsoleWriteNewLine(int pLines)
        {
            for (int i = 0; i <= pLines; i++)
                Console.WriteLine();
        }
    }
}
