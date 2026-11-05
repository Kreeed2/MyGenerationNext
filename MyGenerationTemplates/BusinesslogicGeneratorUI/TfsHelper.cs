using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace EHA.ZESA.Util.GeneratorUI
{
    /// <summary>
    /// TFS Zugriff
    /// </summary>
    public class TfsHelper
    {
        // Die einzige Instanz unserer Helper-Klasse.
        // Die Variable ist "volatile" deklariert, um sicherzustellen
        // das die Zuweisung zu der Instanzvariablen abgeschlossen ist,
        // bevor auf sie zugegriffen werden kann.
        private static volatile TfsHelper mInstance;

        // Dieses Objekt wird zum Sperren des Threads verwendet.
        // Dadurch, dass dieses Objekt anstelle der RemoteObjectFactory-Instanz
        // an "lock()" übergeben wird, werden Deadlocks vermieden.
        private static readonly object mSyncRoot = new Object();

        // Workspace Name
        private string mCurrentTfsWorkspace;

        // Der Workspace
        private Workspace mTfsWorkspace;
       
        // Der TFS Server
        private VersionControlServer mTfsServer;
       
        /// <summary>
		/// Standard-Konstruktor.
		/// </summary>
		/// <remarks>
		/// Da die Klasse nur statische Methoden enthält, ist ein öffentlicher Konstruktor überflüssig.
		/// </remarks>
        private TfsHelper()
		{
			Init();
		}

        /// <summary>
        /// Gibt die einzige Instanz dieser Klasse zurück.
        /// </summary>
        /// <value>Ein Objekt der Klasse DbReaderHelper.</value>
        /// <remarks>
        /// Diese Property-Methode ist thread-sicher ausgelegt.
        /// </remarks>
        public static TfsHelper Instance
        {
            get
            {
                if (null == mInstance)
                {
                    lock (mSyncRoot)
                    {
                        if (null == mInstance)
                        {
                            mInstance = new TfsHelper();
                        }
                    }
                }
                return mInstance;
            }
        }

        /// <summary>
        /// setzt den TfsWorkspace den der User eingestellt hat
        /// </summary>
        public string CurrentTfSWorkspace
        {
            set { mCurrentTfsWorkspace = value; }
        }

        /// <summary>
        /// Initialsierung
        /// </summary>
        private void Init()
        {
            // Settings holen
            BLGeneratorSettings settings = GeneratorHelper.DeserializeSettings();
            
            // Connect to the team project collection and the server that hosts the version-control repository. 
            TfsTeamProjectCollection tfsCollection = new TfsTeamProjectCollection(new Uri(settings.TfsServerUrl));
            mTfsServer = tfsCollection.GetService<VersionControlServer>();
            
            // Get the workspace Name, it could be overwritten
            string workspace = GetTfsWorkspaceName(settings);
            
            // Get the workspace from TFS Server
            try
            {
                mTfsWorkspace = mTfsServer.GetWorkspace(workspace, Environment.UserDomainName + "\\" + Environment.UserName);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("TFS Arbeitsbereich {0} für Benutzer {1} nicht gefunden.", Environment.MachineName, Environment.UserDomainName + "\\" + Environment.UserName), e);
            }
        }

        /// <summary>
        /// Checkt die Datei BLGeneratorMetaData automatisch ein
        /// </summary>
        public void CheckInMetaDataFile()
        {
            try
            {
                // wir rufen alle Änderungen im Verzeichnis 
                // $/ZESA/Utilities/MyGenerationTemplates/BLGeneratorMetaData ab
                List<PendingChange> pendingChanges = new List<PendingChange>();
                PendingSet[] pendingSets = mTfsWorkspace.QueryPendingSets(new ItemSpec[] { new ItemSpec("$/ZESA/Utilities/MyGenerationTemplates/BLGeneratorMetaData", RecursionType.Full) }, null, null, true);
                foreach (PendingSet myPendingSet in pendingSets)
                {
                    pendingChanges.AddRange(myPendingSet.PendingChanges);
                }

                // prüfen auf Konflikte
                Conflict[] conflicts = mTfsWorkspace.QueryConflicts(new[] { "$/ZESA/Utilities/MyGenerationTemplates/BLGeneratorMetaData" }, true);
                if (null == conflicts || 0 == conflicts.Length)
                {
                    // Keine Konflikte? wir können einchecken
                    WorkspaceCheckInParameters myCheckInParameter = new WorkspaceCheckInParameters(pendingChanges, "Automatischer Eincheckvorgang MyGeneration")
                    {
                        OverrideGatedCheckIn = ((CheckInOptions2)mTfsServer.SupportedFeatures & CheckInOptions2.OverrideGatedCheckIn) == CheckInOptions2.OverrideGatedCheckIn,
                        PolicyOverride = new PolicyOverrideInfo("Check-in from the build.", null)
                    };

                    // Eincheckvorgang
                    mTfsWorkspace.CheckIn(myCheckInParameter);
                }
                else
                {   // Konflikte aufgetreten? Die Konflikte müssen von Hand behoben werden
                    throw new Exception(string.Format("Konflikte in '$/ZESA/Utilities/MyGenerationTemplates/BLGeneratorMetaData.xml' müssen von Hand behoben werden."));
                }
            }
            catch (Exception e)
            {
                string msg = "Beim Einchecken der Datei 'BLGeneratorMetaData.xml' ist folgender Fehler aufgetreten: " + e;
                throw new Exception(msg, e);
            }
        }

        /// <summary>
        /// Checkt die Datei BLGeneratorMetaData automatisch ein
        /// </summary>
        public void CheckOutMetaDataFile(string pMetadataFile)
        {
            // wir bauen uns den GetRequest
            GetRequest getRequest = new GetRequest(new ItemSpec("$/ZESA/Utilities/MyGenerationTemplates/BLGeneratorMetaData", RecursionType.Full), VersionSpec.Latest);
            // und aktualisieren
            mTfsWorkspace.Get(getRequest, GetOptions.GetAll);
            // und checken dann aus
            CheckOut(pMetadataFile);
        }

        /// <summary>
        /// Checkt eine Datei aus
        /// </summary>
        /// <param name="pFullFilenameAndPath"></param>
        public void CheckOut(string pFullFilenameAndPath)
        {
            // prüfe auf geänderten Workspace
            CheckForChangedWorkspace();
            
            // wenn die Datei existiert checken wir sie aus...
            if(new FileInfo(pFullFilenameAndPath).Exists)
                mTfsWorkspace.PendEdit(pFullFilenameAndPath);
        }

        /// <summary>
        /// Fügt eine Datei hinzu
        /// </summary>
        /// <param name="pFullFilenameAndPath"></param>
        public void Add(string pFullFilenameAndPath)
        {
            // prüfe auf geänderten Workspace
            CheckForChangedWorkspace();
            
            // wenn die Datei existiert fügen wir sie hinzu...
            if (new FileInfo(pFullFilenameAndPath).Exists)
                mTfsWorkspace.PendAdd(pFullFilenameAndPath);
        }

        /// <summary>
        /// Führt einen Get auf $/ZESA/Utilities aus
        /// </summary>
        public void GetUtilities()
        {
            // prüfe auf geänderten Workspace
            CheckForChangedWorkspace();
            
            // wir bauen uns den GetRequest
            GetRequest getRequest = new GetRequest(new ItemSpec("$/ZESA/Utilities", RecursionType.Full), VersionSpec.Latest);
            // und aktualisieren
            mTfsWorkspace.Get(getRequest, GetOptions.GetAll);
        }



        #region private Helper

        /// <summary>
        /// Prüft auf einen vom User geänderten Workspace
        /// </summary>
        private void CheckForChangedWorkspace()
        {
            if(!mTfsWorkspace.Name.Equals(mCurrentTfsWorkspace))
                Init();
        }

        /// <summary>
        /// Bestimmt den Workspace name, er kann sowohl über die Oberfläche als auch 
        /// im Settings XML geändert werden
        /// </summary>
        /// <param name="pSettings"></param>
        /// <returns></returns>
        private string GetTfsWorkspaceName(BLGeneratorSettings pSettings)
        {
            string retValue;
            // Prüfe auf abweichenden Workspace aus den Settings
            if (string.IsNullOrEmpty(pSettings.AbweichenderTfsWorkspace))
            {
                // ist dieser leer, prüfe auf den von aussen (aus dem Textfeld) zugewiesenen
                if (string.IsNullOrEmpty(mCurrentTfsWorkspace))
                {
                    // auch leer? Dann ist Standard der MachineName
                    retValue = Environment.MachineName;
                }
                else
                {
                    // nicht ler, nimm den aus dem Textfeld
                    retValue = mCurrentTfsWorkspace;
                }
            }
            else
            {
                // ist der von aussen zugewiesene gefüllt UND der abweichende aus den Settings gefüllt
                if (!string.IsNullOrEmpty(mCurrentTfsWorkspace))
                {
                    if (!pSettings.AbweichenderTfsWorkspace.Equals(mCurrentTfsWorkspace))
                    {
                        // sind sie ungleich, gewinnt der von aussen zugewiesene da er über das Textfeld gepflegt werden kann
                        retValue = mCurrentTfsWorkspace;
                    }
                    else
                    {
                        // andernfalls egal, wir nehmen den aus den Settings
                        retValue = pSettings.AbweichenderTfsWorkspace;
                    }
                }
                else
                {
                    // der von aussen zugewiesene ist leer, es gibt aber einen in den Settings, nehmen wir den aus den Settings
                    retValue = pSettings.AbweichenderTfsWorkspace;
                }
            }
            // Rückgabewert zurückgeben
            return retValue;
        }

        #endregion private Helper

    }
}
