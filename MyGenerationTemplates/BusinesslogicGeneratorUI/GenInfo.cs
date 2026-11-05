using System.Collections;
using System.Collections.Generic;

namespace EHA.ZESA.Util.GeneratorUI
{
	/// <summary>
	/// Summary description for MyUIData.
	/// </summary>
	public class GenInfo
	{

		private string mObjectName = string.Empty;
		private string mNamespace  = string.Empty;
		private string mObjectType = string.Empty;
		private string mEntityName = string.Empty;
		private string mLovColumns = string.Empty;
		private string mNormTimeFromColumns = string.Empty;
		private string mNormTimeToColumns   = string.Empty;
        private string mAliasKennungColumn = string.Empty;
        private string mDbReaderDataTypeColumns = string.Empty;
        private string mDbReaderOperatorColumns = string.Empty;
		private bool   mLOV          = false;
		private bool   mNormTimeFrom = false;
		private bool   mNormTimeTo   = false;
		private bool   mGenerate     = false;
        private bool   mStammdatenuebernahme = false;
	    private bool   mIsPrimaryKeyInteger = false;
	    private string mIntegerPrimaryKeyColumns = string.Empty;
        
	    private readonly SortedList mMemberWhichUseOldDataSetLogic;

		public GenInfo()
		{
		    mMemberWhichUseOldDataSetLogic = GeneratorHelper.GetMemberWhichUseOldDataSetLogic();
		}

		/// <summary>
		/// Der Name einer Tabelle oder View.
		/// </summary>
		public string ObjectName
		{
			get { return mObjectName; }
			set { mObjectName = value; }
		}

		/// <summary>
		/// Der Namespace für die zugeordnete Businesslogik.
		/// </summary>
		public string Namespace
		{
			get { return mNamespace; }
			set { mNamespace = value; }
		}

		/// <summary>
		/// Der Typ des Objektes (Tabelle oder View).
		/// </summary>
		public string ObjectType
		{
			get { return mObjectType; }
			set { mObjectType = value; }
		}

		/// <summary>
		/// Der Name der Tabelle ohne Präfix in Groß-/Kleinschreibung.
		/// </summary>
		public string EntityName
		{
			get { return mEntityName; }
			set { mEntityName = value; }
		}

		/// <summary>
		/// Eine durch Kommas getrennte Auflistung der Spalten, welche für eine
		/// Werteliste gelesen werden.
		/// </summary>
		public string LovColumns
		{
			get { return mLovColumns; }
			set { mLovColumns = value; }
		}

		/// <summary>
		/// Zeigt an, ob die Tabelle als ListOfValue dient.
		/// </summary>
		public bool Lov
		{
			get { return mLOV; }
			set { mLOV = value; }
		}


		/// <summary>
		/// Eine durch Kommas getrennte Auflistung der Spalten, dessen Datums/Zeitwert auf 00:00
		/// zu normalisieren ist.
		/// </summary>
		public string NormTimeFromColumns
		{
			get { return mNormTimeFromColumns; }
			set { mNormTimeFromColumns = value; }
		}


		/// <summary>
		/// Zeit an, ob zu normalisierende von-Zeiten in der Tabelle vorhanden sein könnten.
		/// </summary>
		public bool NormTimeFrom
		{
			get { return mNormTimeFrom; }
			set { mNormTimeFrom = value; }
		}


		/// <summary>
		/// Eine durch Kommas getrennte Auflistung der Spalten, dessen Datums/Zeitwert auf 23:59
		/// zu normalisieren ist.
		/// </summary>
		public string NormTimeToColumns
		{
			get { return mNormTimeToColumns; }
			set { mNormTimeToColumns = value; }
		}


		/// <summary>
		/// Zeit an, ob zu normalisierende bis-Zeiten in der Tabelle vorhanden sein könnten.
		/// </summary>
		public bool NormTimeTo
		{
			get { return mNormTimeTo; }
			set { mNormTimeTo = value; }
		}


		/// <summary>
		/// Zeigt an, ob für die Tabelle / View Businesslogik generiert werden soll.
		/// </summary>
		public bool Generate
		{
			get { return mGenerate; }
			set { mGenerate = value; }
		}

        /// <summary>
        /// Zeigt an, ob für die Tabelle Stammdaten im Releasewechsel übernommen werden sollen
        /// </summary>
        public bool Stammdatenuebernahme
        {
            get { return mStammdatenuebernahme; }
            set { mStammdatenuebernahme = value; }
        }

        /// <summary>
        /// Zeigt an, ob für die Tabelle einen Integer PrimaryKey hat
        /// </summary>
        public bool IsPrimaryKeyInteger
        {
            get { return mIsPrimaryKeyInteger; }
            set { mIsPrimaryKeyInteger = value; }
        }

        /// <summary>
		/// Eine durch Kommas getrennte Auflistung der Spalten, die als PrimaryKey vom Typ Integer definiert sind
		/// </summary>
        public string IntegerPrimaryKeyColumns
        {
            get { return mIntegerPrimaryKeyColumns; }
            set { mIntegerPrimaryKeyColumns = value; }
        }

        /// <summary>
        /// gibt an, ob alte DataSet Logik benutzt werden soll.
        /// </summary>
        public bool UsesOldDataSetLogic
	    {
            get { return mMemberWhichUseOldDataSetLogic.Contains(mObjectName); }
        }

        /// <summary>
        /// Eine Spalte die als Kennung fungiert
        /// </summary>
        public string AliasKennungColumn
        {
            get { return mAliasKennungColumn; }
            set { mAliasKennungColumn = value; }
        }

        /// <summary>
        /// enthält die Datentypen für Columns die im DbReader generiert werden sollen,
        /// damit lassen sich die Standardeinträge übersteuern.
        /// </summary>
        public string DbReaderDataTypeColumns
	    {
            get { return mDbReaderDataTypeColumns; }
            set { mDbReaderDataTypeColumns = value; }
	    }

        /// <summary>
        /// enthält die Operatoren für Columns die im DbReader generiert werden sollen,
        /// damit lassen sich die Standardeinträge übersteuern.
        /// </summary>
        public string DbReaderOperatorColumns
        {
            get { return mDbReaderOperatorColumns; }
            set { mDbReaderOperatorColumns = value; }
        }

        #region private Methoden

        private static SortedList InitializeMembersWithOldLogic()
        {
            SortedList resultList = new SortedList(813);
            resultList.Add("BA_ANWENDUNGSFALL", "BA_ANWENDUNGSFALL");
            resultList.Add("BA_AUSWERTUNG", "BA_AUSWERTUNG");
            resultList.Add("BA_AUSWERTUNG_PARAMETER", "BA_AUSWERTUNG_PARAMETER");
            resultList.Add("BA_BENUTZER", "BA_BENUTZER");
            resultList.Add("BA_BUCHUNGSKREIS", "BA_BUCHUNGSKREIS");
            resultList.Add("BA_DATENGRUPPE", "BA_DATENGRUPPE");
            resultList.Add("BA_DATENGRUPPE_BENUTZER", "BA_DATENGRUPPE_BENUTZER");
            resultList.Add("BA_DOMAENE", "BA_DOMAENE");
            resultList.Add("BA_EINHEIT", "BA_EINHEIT");
            resultList.Add("BA_EINHEIT_EINHEITENKATEGORIE", "BA_EINHEIT_EINHEITENKATEGORIE");
            resultList.Add("BA_EINHEITENKATEGORIE", "BA_EINHEITENKATEGORIE");
            resultList.Add("BA_EK_KUNDENSEGMENT", "BA_EK_KUNDENSEGMENT");
            resultList.Add("BA_FILE_IMPORT", "BA_FILE_IMPORT");
            resultList.Add("BA_FILE_IMPORT_DEFINITION", "BA_FILE_IMPORT_DEFINITION");
            resultList.Add("BA_FILE_IMPORT_VERARBEITUNG", "BA_FILE_IMPORT_VERARBEITUNG");
            resultList.Add("BA_FUNKTION", "BA_FUNKTION");
            resultList.Add("BA_FUNKTION_ANWENDUNGSFALL_DOMAENE", "BA_FUNKTION_ANWENDUNGSFALL_DOMAENE");
            resultList.Add("BA_FUNKTIONSGRUPPE", "BA_FUNKTIONSGRUPPE");
            resultList.Add("BA_FUNKTIONSGRUPPE_BENUTZER", "BA_FUNKTIONSGRUPPE_BENUTZER");
            resultList.Add("BA_FUNKTIONSGRUPPE_FUNKTION", "BA_FUNKTIONSGRUPPE_FUNKTION");
            resultList.Add("BA_HISTORIENBELEG", "BA_HISTORIENBELEG");
            resultList.Add("BA_KALENDER", "BA_KALENDER");
            resultList.Add("BA_MANDANT", "BA_MANDANT");
            resultList.Add("BA_NUTZZEITRAUM", "BA_NUTZZEITRAUM");
            resultList.Add("BA_ONLINEHILFE", "BA_ONLINEHILFE");
            resultList.Add("BA_ONLINEHILFE_SPARTE", "BA_ONLINEHILFE_SPARTE");
            resultList.Add("BA_ORGANISATIONSEINHEIT", "BA_ORGANISATIONSEINHEIT");
            resultList.Add("BA_RAHMENKASSENZEICHEN", "BA_RAHMENKASSENZEICHEN");
            resultList.Add("BA_REGIONALSTRUKTUR", "BA_REGIONALSTRUKTUR");
            resultList.Add("BA_SOMMERZEIT", "BA_SOMMERZEIT");
            resultList.Add("BA_TRANSAKTIONSPROTOKOLL_INOUT", "BA_TRANSAKTIONSPROTOKOLL_INOUT");
            resultList.Add("BA_TRANSAKTIONSPROTOKOLL_INOUT_RUECK", "BA_TRANSAKTIONSPROTOKOLL_INOUT_RUECK");
            resultList.Add("BA_WAEHRUNGSUMRECHNUNG", "BA_WAEHRUNGSUMRECHNUNG");
            resultList.Add("BACKUP_STAFFEL", "BACKUP_STAFFEL");
            resultList.Add("CACHE_TPHL", "CACHE_TPHL");
            resultList.Add("GP_ABTEILUNG", "GP_ABTEILUNG");
            resultList.Add("GP_ADRESSE", "GP_ADRESSE");
            resultList.Add("GP_ADRESSREGEL", "GP_ADRESSREGEL");
            resultList.Add("GP_ADRESSREGEL_ADRESSTYP", "GP_ADRESSREGEL_ADRESSTYP");
            resultList.Add("GP_ADRESSROLLE", "GP_ADRESSROLLE");
            resultList.Add("GP_ADRESSTYP", "GP_ADRESSTYP");
            resultList.Add("GP_ANREDE", "GP_ANREDE");
            resultList.Add("GP_BANK", "GP_BANK");
            resultList.Add("GP_BANKVERBINDUNG", "GP_BANKVERBINDUNG");
            resultList.Add("GP_EXPORT_DEBITOR", "GP_EXPORT_DEBITOR");
            resultList.Add("GP_EXPORT_DEBITOR_SAP_MAPPING", "GP_EXPORT_DEBITOR_SAP_MAPPING");
            resultList.Add("GP_EXTERNE_NUMMER", "GP_EXTERNE_NUMMER");
            resultList.Add("GP_EXTERNE_NUMMER_TYP", "GP_EXTERNE_NUMMER_TYP");
            resultList.Add("GP_GESCHAEFTSPARTEI", "GP_GESCHAEFTSPARTEI");
            resultList.Add("GP_GESCHAEFTSPARTEI_ABTEILUNG_PERSON", "GP_GESCHAEFTSPARTEI_ABTEILUNG_PERSON");
            resultList.Add("GP_GESCHAEFTSPARTEI_ABTEILUNG_PERSON_ADRESSE", "GP_GESCHAEFTSPARTEI_ABTEILUNG_PERSON_ADRESSE");
            resultList.Add("GP_GESCHAEFTSPARTEI_ABTEILUNG_PERSON_KOMMUNIKATIONSKANAL", "GP_GESCHAEFTSPARTEI_ABTEILUNG_PERSON_KOMMUNIKATIONSKANAL");
            resultList.Add("GP_GESCHAEFTSPARTEI_GESCHAEFTSPARTEIGRUPPE", "GP_GESCHAEFTSPARTEI_GESCHAEFTSPARTEIGRUPPE");
            resultList.Add("GP_GESCHAEFTSPARTEI_ROLLE", "GP_GESCHAEFTSPARTEI_ROLLE");
            resultList.Add("GP_GESCHAEFTSPARTEI_TARIFZEITENKALENDER", "GP_GESCHAEFTSPARTEI_TARIFZEITENKALENDER");
            resultList.Add("GP_GESCHAEFTSPARTEIGRUPPE", "GP_GESCHAEFTSPARTEIGRUPPE");
            resultList.Add("GP_GESCHAEFTSPARTEIGRUPPE_PREISSEGMENT", "GP_GESCHAEFTSPARTEIGRUPPE_PREISSEGMENT");
            resultList.Add("GP_GESCHAEFTSPARTEISTATUS", "GP_GESCHAEFTSPARTEISTATUS");
            resultList.Add("GP_GESCHAEFTSPARTEITYP", "GP_GESCHAEFTSPARTEITYP");
            resultList.Add("GP_HI_GESCHAEFTSPARTEI", "GP_HI_GESCHAEFTSPARTEI");
            resultList.Add("GP_KOMMUNIKATIONSKANAL", "GP_KOMMUNIKATIONSKANAL");
            resultList.Add("GP_KONTIERUNGSOBJEKT", "GP_KONTIERUNGSOBJEKT");
            resultList.Add("GP_LAENDERSCHLUESSEL", "GP_LAENDERSCHLUESSEL");
            resultList.Add("GP_LASTPROFILVERFAHREN", "GP_LASTPROFILVERFAHREN");
            resultList.Add("GP_PERSON", "GP_PERSON");
            resultList.Add("GP_REGION", "GP_REGION");
            resultList.Add("GP_TAGESINTERVALL", "GP_TAGESINTERVALL");
            resultList.Add("GP_TARIFZEITEN", "GP_TARIFZEITEN");
            resultList.Add("GP_TARIFZEITENKALENDER", "GP_TARIFZEITENKALENDER");
            resultList.Add("GP_TEMPERATURGRUPPE", "GP_TEMPERATURGRUPPE");
            resultList.Add("GP_VNB_BILANZKREIS", "GP_VNB_BILANZKREIS");
            resultList.Add("GP_VNB_ENET", "GP_VNB_ENET");
            resultList.Add("GP_VNB_PLZ", "GP_VNB_PLZ");
            resultList.Add("GP_ZAEHLPUNKT_VNB_ENET", "GP_ZAEHLPUNKT_VNB_ENET");
            resultList.Add("GP_ZAEHLPUNKT_VNB_ENET_AKTUELL", "GP_ZAEHLPUNKT_VNB_ENET_AKTUELL");
            resultList.Add("GP_ZAEHLPUNKT_VNB_ENET_AKTUELL_GP", "GP_ZAEHLPUNKT_VNB_ENET_AKTUELL_GP");
            resultList.Add("KAI_TEMP", "KAI_TEMP");
            resultList.Add("KO_ABFRAGE", "KO_ABFRAGE");
            resultList.Add("KO_ABFRAGE_TMP", "KO_ABFRAGE_TMP");
            resultList.Add("KO_BERECHTIGUNGSGRUPPE", "KO_BERECHTIGUNGSGRUPPE");
            resultList.Add("KO_BERECHTIGUNGSGRUPPE_BENUTZER", "KO_BERECHTIGUNGSGRUPPE_BENUTZER");
            resultList.Add("KO_DRUCKVERLAUF", "KO_DRUCKVERLAUF");
            resultList.Add("KO_EXEMPLARE", "KO_EXEMPLARE");
            resultList.Add("KO_EXEMPLARE_TMP", "KO_EXEMPLARE_TMP");
            resultList.Add("KO_HINWEIS", "KO_HINWEIS");
            resultList.Add("KO_HINWEIS_BENUTZER", "KO_HINWEIS_BENUTZER");
            resultList.Add("KO_PROZESS", "KO_PROZESS");
            resultList.Add("KO_SCHREIBEN_ANHANG", "KO_SCHREIBEN_ANHANG");
            resultList.Add("KO_VERSION", "KO_VERSION");
            resultList.Add("KO_VERTRAG_VORLAGE", "KO_VERTRAG_VORLAGE");
            resultList.Add("KO_VERZEICHNIS", "KO_VERZEICHNIS");
            resultList.Add("KO_VORLAGE", "KO_VORLAGE");
            resultList.Add("KO_VORLAGE_VERSION", "KO_VORLAGE_VERSION");
            resultList.Add("LOG_EINRICHTEN_MANDANT_BUCHUNGSKREIS", "LOG_EINRICHTEN_MANDANT_BUCHUNGSKREIS");
            resultList.Add("LOG_PRUEFUNG_GULTIGKEIT_HI_ANSCHLUSSOBJEKT", "LOG_PRUEFUNG_GULTIGKEIT_HI_ANSCHLUSSOBJEKT");
            resultList.Add("LOG_UPDATE_VERTRAG_PREISSEGMENT_NUTZENERGIE_GLEICH", "LOG_UPDATE_VERTRAG_PREISSEGMENT_NUTZENERGIE_GLEICH");
            resultList.Add("PRF_SONDERNETZNUTZUNGVERTRAG", "PRF_SONDERNETZNUTZUNGVERTRAG");
            resultList.Add("PROTOKOLL_UPDATE_ANLAGE_BILANZKREIS", "PROTOKOLL_UPDATE_ANLAGE_BILANZKREIS");
            resultList.Add("SY_DOMAENEN_NUMMER", "SY_DOMAENEN_NUMMER");
            resultList.Add("SY_KONFIGURATION", "SY_KONFIGURATION");
            resultList.Add("SY_PROZESS_MONITORING", "SY_PROZESS_MONITORING");
            resultList.Add("sysdiagrams", "sysdiagrams");
            resultList.Add("TA_ABSCHLAGSFREIGABE_STICHTAG", "TA_ABSCHLAGSFREIGABE_STICHTAG");
            resultList.Add("TA_HEIZOELPREIS", "TA_HEIZOELPREIS");
            resultList.Add("TA_IMPORT_AKT_ENERGIEPREIS", "TA_IMPORT_AKT_ENERGIEPREIS");
            resultList.Add("TA_KALKULATIONSVARIANTE", "TA_KALKULATIONSVARIANTE");
            resultList.Add("TA_KONDITION", "TA_KONDITION");
            resultList.Add("TA_KONDITIONSART", "TA_KONDITIONSART");
            resultList.Add("TA_KONDITIONSPOSITION", "TA_KONDITIONSPOSITION");
            resultList.Add("TA_KONDITIONSPREIS", "TA_KONDITIONSPREIS");
            resultList.Add("TA_KONDITIONSSTAFFEL", "TA_KONDITIONSSTAFFEL");
            resultList.Add("TA_KONZESSIONSABGABE", "TA_KONZESSIONSABGABE");
            resultList.Add("TA_LEISTUNGSSPITZE", "TA_LEISTUNGSSPITZE");
            resultList.Add("TA_MARKTSEGMENT", "TA_MARKTSEGMENT");
            resultList.Add("TA_NN_ABSCHLAG_MEHRERLOESE_PARAGRAPH_32", "TA_NN_ABSCHLAG_MEHRERLOESE_PARAGRAPH_32");
            resultList.Add("TA_NN_ENTGELT", "TA_NN_ENTGELT");
            resultList.Add("TA_NN_ENTGELT_KA_MANUELL", "TA_NN_ENTGELT_KA_MANUELL");
            resultList.Add("TA_NN_ENTGELT_PLZ_VNB", "TA_NN_ENTGELT_PLZ_VNB");
            resultList.Add("TA_NN_ENTGELT_STATUS", "TA_NN_ENTGELT_STATUS");
            resultList.Add("TA_NN_ENTGELT_VNB", "TA_NN_ENTGELT_VNB");
            resultList.Add("TA_NN_MEHR_MINDERMENGEN_MONATSPREISE", "TA_NN_MEHR_MINDERMENGEN_MONATSPREISE");
            resultList.Add("TA_NNEAT_ELEKTRABGABE", "TA_NNEAT_ELEKTRABGABE");
            resultList.Add("TA_NNEAT_NETZBEREICHE", "TA_NNEAT_NETZBEREICHE");
            resultList.Add("TA_NNEAT_NNENTGELT", "TA_NNEAT_NNENTGELT");
            resultList.Add("TA_NNEAT_PLZ_VNB", "TA_NNEAT_PLZ_VNB");
            resultList.Add("TA_NNEAT_STRANDEDCOSTS", "TA_NNEAT_STRANDEDCOSTS");
            resultList.Add("TA_NNEAT_VNB", "TA_NNEAT_VNB");
            resultList.Add("TA_NNEAT_ZAEHLPUNKTPAUSCHALE", "TA_NNEAT_ZAEHLPUNKTPAUSCHALE");
            resultList.Add("TA_NNEAT_ZUSCHLAEGESNT", "TA_NNEAT_ZUSCHLAEGESNT");
            resultList.Add("TA_OPERAND", "TA_OPERAND");
            resultList.Add("TA_POSITIONPREIS", "TA_POSITIONPREIS");
            resultList.Add("TA_PREISART", "TA_PREISART");
            resultList.Add("TA_PREISSEGMENT", "TA_PREISSEGMENT");
            resultList.Add("TA_RABATT", "TA_RABATT");
            resultList.Add("TA_RABATTSTAFFEL", "TA_RABATTSTAFFEL");
            resultList.Add("TA_RABATTTYP", "TA_RABATTTYP");
            resultList.Add("TA_RABATTWERT", "TA_RABATTWERT");
            resultList.Add("TA_REFERENZORTE_HEIZOELPREIS", "TA_REFERENZORTE_HEIZOELPREIS");
            resultList.Add("TA_STAFFEL", "TA_STAFFEL");
            resultList.Add("TA_STAFFELART", "TA_STAFFELART");
            resultList.Add("TA_TAGESAKTUELLER_ENERGIEPREIS", "TA_TAGESAKTUELLER_ENERGIEPREIS");
            resultList.Add("TA_TARIFART", "TA_TARIFART");
            resultList.Add("TA_TARIFSCHEMA_STATUS", "TA_TARIFSCHEMA_STATUS");
            resultList.Add("TA_TARIFSCHEMAKOPF", "TA_TARIFSCHEMAKOPF");
            resultList.Add("TA_TARIFSCHEMAPOSITION", "TA_TARIFSCHEMAPOSITION");
            resultList.Add("TA_TARIFTYP", "TA_TARIFTYP");
            resultList.Add("TA_TARIFTYP_MESSART", "TA_TARIFTYP_MESSART");
            resultList.Add("TA_VERGUETUNGSART", "TA_VERGUETUNGSART");
            resultList.Add("TA_WAEHRUNGSSCHLUESSEL", "TA_WAEHRUNGSSCHLUESSEL");
            resultList.Add("TEMP_TS_HI_ANLAGE", "TEMP_TS_HI_ANLAGE");
            resultList.Add("TS_AKTIVGRUPPE", "TS_AKTIVGRUPPE");
            resultList.Add("TS_ANLAGE", "TS_ANLAGE");
            resultList.Add("TS_ANLAGE_MESSART", "TS_ANLAGE_MESSART");
            resultList.Add("TS_ANLAGE_NUTZENERGIE", "TS_ANLAGE_NUTZENERGIE");
            resultList.Add("TS_ANLAGE_PROFILTYP", "TS_ANLAGE_PROFILTYP");
            resultList.Add("TS_ANLAGE_STATUS", "TS_ANLAGE_STATUS");
            resultList.Add("TS_ANLAGE_VERSORGUNGSPRODUKT_EIGENSCHAFT", "TS_ANLAGE_VERSORGUNGSPRODUKT_EIGENSCHAFT");
            resultList.Add("TS_ANSCHLUSSOBJEKT", "TS_ANSCHLUSSOBJEKT");
            resultList.Add("TS_ANSCHLUSSOBJEKT_STATUS", "TS_ANSCHLUSSOBJEKT_STATUS");
            resultList.Add("TS_BILANZKREIS", "TS_BILANZKREIS");
            resultList.Add("TS_DATENLOGGERDETAILS", "TS_DATENLOGGERDETAILS");
            resultList.Add("TS_DATENLOGGEREINGANGSGROESSE", "TS_DATENLOGGEREINGANGSGROESSE");
            resultList.Add("TS_DATENLOGGERMESSKREIS", "TS_DATENLOGGERMESSKREIS");
            resultList.Add("TS_DATENLOGGERMODUL", "TS_DATENLOGGERMODUL");
            resultList.Add("TS_DATENLOGGERMODULTYP", "TS_DATENLOGGERMODULTYP");
            resultList.Add("TS_FLAECHENART", "TS_FLAECHENART");
            resultList.Add("TS_GASDRUCKMESSUNG", "TS_GASDRUCKMESSUNG");
            resultList.Add("TS_GERAET", "TS_GERAET");
            resultList.Add("TS_GERAET_DEFEKT", "TS_GERAET_DEFEKT");
            resultList.Add("TS_GERAET_RUECKMELDUNG", "TS_GERAET_RUECKMELDUNG");
            resultList.Add("TS_GERAET_RUECKMELDUNG_STATUS", "TS_GERAET_RUECKMELDUNG_STATUS");
            resultList.Add("TS_GERAET_STATUS", "TS_GERAET_STATUS");
            resultList.Add("TS_GERAETTYP", "TS_GERAETTYP");
            resultList.Add("TS_HERSTELLERTYP", "TS_HERSTELLERTYP");
            resultList.Add("TS_HI_ANLAGE", "TS_HI_ANLAGE");
            resultList.Add("TS_HI_ANSCHLUSSOBJEKT", "TS_HI_ANSCHLUSSOBJEKT");
            resultList.Add("TS_HI_GERAET", "TS_HI_GERAET");
            resultList.Add("TS_HI_VERBRAUCHSSTELLE", "TS_HI_VERBRAUCHSSTELLE");
            resultList.Add("TS_HI_ZAEHLPUNKT", "TS_HI_ZAEHLPUNKT");
            resultList.Add("TS_KENNGROESSEN", "TS_KENNGROESSEN");
            resultList.Add("TS_KLASSIFIKATION", "TS_KLASSIFIKATION");
            resultList.Add("TS_MESSART", "TS_MESSART");
            resultList.Add("TS_MESSART_INDEX", "TS_MESSART_INDEX");
            resultList.Add("TS_NETZEBENE", "TS_NETZEBENE");
            resultList.Add("TS_NUTZART", "TS_NUTZART");
            resultList.Add("TS_PRODUKT_VERBRAUCHSSTELLE", "TS_PRODUKT_VERBRAUCHSSTELLE");
            resultList.Add("TS_PROFILTYP", "TS_PROFILTYP");
            resultList.Add("TS_REFERENZBETRIEB", "TS_REFERENZBETRIEB");
            resultList.Add("TS_REFERENZBETRIEB_ANLAGE", "TS_REFERENZBETRIEB_ANLAGE");
            resultList.Add("TS_REFERENZBETRIEB_FAKTOR", "TS_REFERENZBETRIEB_FAKTOR");
            resultList.Add("TS_REFERENZBETRIEB_KENNGROESSEN", "TS_REFERENZBETRIEB_KENNGROESSEN");
            resultList.Add("TS_REFERENZBETRIEB_PREISSEGMENT", "TS_REFERENZBETRIEB_PREISSEGMENT");
            resultList.Add("TS_REFERENZBETRIEB_PROZENT", "TS_REFERENZBETRIEB_PROZENT");
            resultList.Add("TS_REFERENZBETRIEB_PROZENT_STATUS", "TS_REFERENZBETRIEB_PROZENT_STATUS");
            resultList.Add("TS_SPARTE", "TS_SPARTE");
            resultList.Add("TS_STUECKLISTE", "TS_STUECKLISTE");
            resultList.Add("TS_STUECKLISTENPOSITION", "TS_STUECKLISTENPOSITION");
            resultList.Add("TS_VERBRAUCHSSTELLE", "TS_VERBRAUCHSSTELLE");
            resultList.Add("TS_VERBRAUCHSSTELLE_DETAIL", "TS_VERBRAUCHSSTELLE_DETAIL");
            resultList.Add("TS_VERBRAUCHSSTELLE_STATUS", "TS_VERBRAUCHSSTELLE_STATUS");
            resultList.Add("TS_VERSORGUNGSPRODUKT_EIGENSCHAFT", "TS_VERSORGUNGSPRODUKT_EIGENSCHAFT");
            resultList.Add("TS_WANDLERFAKTOR", "TS_WANDLERFAKTOR");
            resultList.Add("TS_WETTERSTATION", "TS_WETTERSTATION");
            resultList.Add("TS_WETTERSTATION_PLZ", "TS_WETTERSTATION_PLZ");
            resultList.Add("TS_ZAEHLPUNKT", "TS_ZAEHLPUNKT");
            resultList.Add("TS_ZFUE_UEBERWACHUNGSGRUPPE", "TS_ZFUE_UEBERWACHUNGSGRUPPE");
            resultList.Add("V_ABSCHALGSKALKULATION_ABSCHLAGSLISTE", "V_ABSCHALGSKALKULATION_ABSCHLAGSLISTE");
            resultList.Add("V_ABSCHLAGSKALKULATION_ALLE_ANLAGEN", "V_ABSCHLAGSKALKULATION_ALLE_ANLAGEN");
            resultList.Add("V_ABSCHLAGSKALKULATION_BUENDELVERTRAG", "V_ABSCHLAGSKALKULATION_BUENDELVERTRAG");
            resultList.Add("V_ABSCHLAGSKALKULATION_ERGEBNIS_DETAILS_EK", "V_ABSCHLAGSKALKULATION_ERGEBNIS_DETAILS_EK");
            resultList.Add("V_ABSCHLAGSKALKULATION_ERGEBNIS_DETAILS_VK", "V_ABSCHLAGSKALKULATION_ERGEBNIS_DETAILS_VK");
            resultList.Add("V_ABSCHLAGSKALKULATION_ERGEBNIS_PERIODEN_VK", "V_ABSCHLAGSKALKULATION_ERGEBNIS_PERIODEN_VK");
            resultList.Add("V_ABSCHLAGSKALKULATION_KALKULIERTE_ANLAGEN", "V_ABSCHLAGSKALKULATION_KALKULIERTE_ANLAGEN");
            resultList.Add("V_ABSCHLAGSKALKULATION_LAUF_FORTSETZEN", "V_ABSCHLAGSKALKULATION_LAUF_FORTSETZEN");
            resultList.Add("V_ABSCHLAGSKALKULATION_LAUF_UEBERSICHT", "V_ABSCHLAGSKALKULATION_LAUF_UEBERSICHT");
            resultList.Add("V_ABSCHLAGSKALKULATION_RAHMENVERTRAG", "V_ABSCHLAGSKALKULATION_RAHMENVERTRAG");
            resultList.Add("V_ABSCHLAGSKALKULATIONSLAUF_EXPORT", "V_ABSCHLAGSKALKULATIONSLAUF_EXPORT");
            resultList.Add("V_ADRESSE_ADRESSROLLE", "V_ADRESSE_ADRESSROLLE");
            resultList.Add("V_ADRESSE_ADRESSTYP", "V_ADRESSE_ADRESSTYP");
            resultList.Add("V_ADRESSE_GESCHAEFTSPARTEI", "V_ADRESSE_GESCHAEFTSPARTEI");
            resultList.Add("V_ADRESSE_GESCHAEFTSPARTEI_PERSON", "V_ADRESSE_GESCHAEFTSPARTEI_PERSON");
            resultList.Add("V_ADRESSE_GESCHAEFTSPARTEI_VERSAND", "V_ADRESSE_GESCHAEFTSPARTEI_VERSAND");
            resultList.Add("V_ADRESSE_LAND", "V_ADRESSE_LAND");
            resultList.Add("V_ADRESSE_ROLLE_TYP_PERSON", "V_ADRESSE_ROLLE_TYP_PERSON");
            resultList.Add("V_AKTIVGRUPPE", "V_AKTIVGRUPPE");
            resultList.Add("V_AKTIVGRUPPE_KLASSIFIKATION", "V_AKTIVGRUPPE_KLASSIFIKATION");
            resultList.Add("V_AKTUELLE_ADRESSEN", "V_AKTUELLE_ADRESSEN");
            resultList.Add("V_ANFRAGE_ANGEBOT_ANLAGE_MESSART_PIVOT", "V_ANFRAGE_ANGEBOT_ANLAGE_MESSART_PIVOT");
            resultList.Add("V_ANFRAGE_ANLAGE_JAHRESMENGE", "V_ANFRAGE_ANLAGE_JAHRESMENGE");
            resultList.Add("V_ANFRAGE_ANLAGEN", "V_ANFRAGE_ANLAGEN");
            resultList.Add("V_ANFRAGE_ANLAGEN_DISTINCT", "V_ANFRAGE_ANLAGEN_DISTINCT");
            resultList.Add("V_ANFRAGE_ANLAGEN_UEBERFUEHRT", "V_ANFRAGE_ANLAGEN_UEBERFUEHRT");
            resultList.Add("V_ANFRAGE_BERECHNUNGSZEITPUNKT_MAX", "V_ANFRAGE_BERECHNUNGSZEITPUNKT_MAX");
            resultList.Add("V_ANFRAGE_DOMAINKEY", "V_ANFRAGE_DOMAINKEY");
            resultList.Add("V_ANFRAGE_EINZELVERTRAG_LISTE_EK", "V_ANFRAGE_EINZELVERTRAG_LISTE_EK");
            resultList.Add("V_ANFRAGE_ENERGIE_BEZUG_STROM_ANLAGEN_ZUORDNEN_ANLAGEN_AUSWAHL", "V_ANFRAGE_ENERGIE_BEZUG_STROM_ANLAGEN_ZUORDNEN_ANLAGEN_AUSWAHL");
            resultList.Add("V_ANFRAGE_EXCEL_EXPORT", "V_ANFRAGE_EXCEL_EXPORT");
            resultList.Add("V_ANFRAGE_EXCEL_EXPORT_AUFTRAGNEHMER", "V_ANFRAGE_EXCEL_EXPORT_AUFTRAGNEHMER");
            resultList.Add("V_ANFRAGE_EXCEL_EXPORT_VERTRAGDATEN", "V_ANFRAGE_EXCEL_EXPORT_VERTRAGDATEN");
            resultList.Add("V_ANFRAGE_EXCEL_EXPORT_ZAEHLPUNKT_VNB", "V_ANFRAGE_EXCEL_EXPORT_ZAEHLPUNKT_VNB");
            resultList.Add("V_ANFRAGE_LISTE", "V_ANFRAGE_LISTE");
            resultList.Add("V_ANFRAGE_VERTRAGSPARTNER", "V_ANFRAGE_VERTRAGSPARTNER");
            resultList.Add("V_ANGEBOT_ANLAGE_JAHRESMENGE", "V_ANGEBOT_ANLAGE_JAHRESMENGE");
            resultList.Add("V_ANGEBOT_ANLAGEN", "V_ANGEBOT_ANLAGEN");
            resultList.Add("V_ANGEBOT_ANLAGEN_UEBERFUEHRT", "V_ANGEBOT_ANLAGEN_UEBERFUEHRT");
            resultList.Add("V_ANGEBOT_DOMAINKEY", "V_ANGEBOT_DOMAINKEY");
            resultList.Add("V_ANGEBOT_ENERGIELIEFERUNG_STROM_DOMAINKEY", "V_ANGEBOT_ENERGIELIEFERUNG_STROM_DOMAINKEY");
            resultList.Add("V_ANGEBOT_IMPORT", "V_ANGEBOT_IMPORT");
            resultList.Add("V_ANGEBOT_LISTE", "V_ANGEBOT_LISTE");
            resultList.Add("V_ANGEBOT_VERTRAGSPARTNER", "V_ANGEBOT_VERTRAGSPARTNER");
            resultList.Add("V_ANL_MESSART_PIVOT", "V_ANL_MESSART_PIVOT");
            resultList.Add("V_ANL_MESSART_PIVOT1", "V_ANL_MESSART_PIVOT1");
            resultList.Add("V_ANL_MESSART_PIVOT2", "V_ANL_MESSART_PIVOT2");
            resultList.Add("V_ANLAGE_BILANZKREIS_AKTUELL", "V_ANLAGE_BILANZKREIS_AKTUELL");
            resultList.Add("V_ANLAGE_CLUSTER_REFERENZ", "V_ANLAGE_CLUSTER_REFERENZ");
            resultList.Add("V_ANLAGE_CLUSTERZUORDNUNG", "V_ANLAGE_CLUSTERZUORDNUNG");
            resultList.Add("V_ANLAGE_DATENUMFELD_GP_VERTRAG", "V_ANLAGE_DATENUMFELD_GP_VERTRAG");
            resultList.Add("V_ANLAGE_DATENUMFELD_VERTRAEGE", "V_ANLAGE_DATENUMFELD_VERTRAEGE");
            resultList.Add("V_ANLAGE_DATENUMFELD_VERTRAGSPARTNER_VERTRAG", "V_ANLAGE_DATENUMFELD_VERTRAGSPARTNER_VERTRAG");
            resultList.Add("V_ANLAGE_DATENUMFELD_ZUGEORDNETE_VERTRAEGE", "V_ANLAGE_DATENUMFELD_ZUGEORDNETE_VERTRAEGE");
            resultList.Add("V_ANLAGE_DETAILS", "V_ANLAGE_DETAILS");
            resultList.Add("V_ANLAGE_DOMAINKEY", "V_ANLAGE_DOMAINKEY");
            resultList.Add("V_ANLAGE_EIGENTUEMER", "V_ANLAGE_EIGENTUEMER");
            resultList.Add("V_ANLAGE_ENET_VERGLEICHSZEITPUNKT", "V_ANLAGE_ENET_VERGLEICHSZEITPUNKT");
            resultList.Add("V_ANLAGE_GERAET", "V_ANLAGE_GERAET");
            resultList.Add("V_ANLAGE_GERAET_RUECKMELDUNG_STATUS", "V_ANLAGE_GERAET_RUECKMELDUNG_STATUS");
            resultList.Add("V_ANLAGE_GESCHAEFTSBEZIEHUNG_GESCHAFTSPARTEI", "V_ANLAGE_GESCHAEFTSBEZIEHUNG_GESCHAFTSPARTEI");
            resultList.Add("V_ANLAGE_HAENDLEREINZELVERTRAG_GENERIEREN", "V_ANLAGE_HAENDLEREINZELVERTRAG_GENERIEREN");
            resultList.Add("V_ANLAGE_MESSART", "V_ANLAGE_MESSART");
            resultList.Add("V_ANLAGE_MESSART_NETZEBENE", "V_ANLAGE_MESSART_NETZEBENE");
            resultList.Add("V_ANLAGE_MIT_HISTORIE", "V_ANLAGE_MIT_HISTORIE");
            resultList.Add("V_ANLAGE_NETZEBENE_NETZVERLUSTEBENE", "V_ANLAGE_NETZEBENE_NETZVERLUSTEBENE");
            resultList.Add("V_ANLAGE_NETZNUTZUNGSENTGELTE", "V_ANLAGE_NETZNUTZUNGSENTGELTE");
            resultList.Add("V_ANLAGE_NUTZENERGIE_REFERENZBETRIEB", "V_ANLAGE_NUTZENERGIE_REFERENZBETRIEB");
            resultList.Add("V_ANLAGE_NUTZER", "V_ANLAGE_NUTZER");
            resultList.Add("V_ANLAGE_PLZ_ORT", "V_ANLAGE_PLZ_ORT");
            resultList.Add("V_ANLAGE_PROFILTYP", "V_ANLAGE_PROFILTYP");
            resultList.Add("V_ANLAGE_PROFILTYP_AKTUELL", "V_ANLAGE_PROFILTYP_AKTUELL");
            resultList.Add("V_ANLAGE_STATUS", "V_ANLAGE_STATUS");
            resultList.Add("V_ANLAGE_STATUS_ZAEHLPUNKT_SPARTE", "V_ANLAGE_STATUS_ZAEHLPUNKT_SPARTE");
            resultList.Add("V_ANLAGE_UEBERSICHT", "V_ANLAGE_UEBERSICHT");
            resultList.Add("V_ANLAGE_UEBERSICHT_KONTO", "V_ANLAGE_UEBERSICHT_KONTO");
            resultList.Add("V_ANLAGE_UEBERSICHT_OHNE_DATENLOGGERZUORDNUNG", "V_ANLAGE_UEBERSICHT_OHNE_DATENLOGGERZUORDNUNG");
            resultList.Add("V_ANLAGE_UEBERSICHT_Test", "V_ANLAGE_UEBERSICHT_Test");
            resultList.Add("V_ANLAGE_UEBERSICHT_ZAEHLERNUMMER", "V_ANLAGE_UEBERSICHT_ZAEHLERNUMMER");
            resultList.Add("V_ANLAGE_VERBRAUCHSSTELLE_ONN", "V_ANLAGE_VERBRAUCHSSTELLE_ONN");
            resultList.Add("V_ANLAGE_VERBRAUCHSSTELLE_UEBERSICHT", "V_ANLAGE_VERBRAUCHSSTELLE_UEBERSICHT");
            resultList.Add("V_ANLAGE_VERTRAG_GENERIEREN_EK", "V_ANLAGE_VERTRAG_GENERIEREN_EK");
            resultList.Add("V_ANLAGE_VERTRAG_GENERIEREN_EK_ENB", "V_ANLAGE_VERTRAG_GENERIEREN_EK_ENB");
            resultList.Add("V_ANLAGE_VERTRAG_GENERIEREN_VK", "V_ANLAGE_VERTRAG_GENERIEREN_VK");
            resultList.Add("V_ANLAGE_VERTRAG_GENERIEREN_VK_GUELTIGKEIT", "V_ANLAGE_VERTRAG_GENERIEREN_VK_GUELTIGKEIT");
            resultList.Add("V_ANLAGE_VERTRAG_ZUORDNEN", "V_ANLAGE_VERTRAG_ZUORDNEN");
            resultList.Add("V_ANLAGE_VERTRAGLICH_VERSORGT", "V_ANLAGE_VERTRAGLICH_VERSORGT");
            resultList.Add("V_ANLAGE_VERTRAGSKONTO_VERTRAG", "V_ANLAGE_VERTRAGSKONTO_VERTRAG");
            resultList.Add("V_ANLAGE_VERTRAGSKONTO_VERTRAG_NUTZER", "V_ANLAGE_VERTRAGSKONTO_VERTRAG_NUTZER");
            resultList.Add("V_ANLAGE_VERTRAGSKONTO_VERTRAG_STORNO", "V_ANLAGE_VERTRAGSKONTO_VERTRAG_STORNO");
            resultList.Add("V_ANLAGE_VKBEGINN", "V_ANLAGE_VKBEGINN");
            resultList.Add("V_ANLAGE_ZAEHLPUNKT", "V_ANLAGE_ZAEHLPUNKT");
            resultList.Add("V_ANLAGE_ZAEHLPUNKT_GESCHAEFTSBEZIEHUNG", "V_ANLAGE_ZAEHLPUNKT_GESCHAEFTSBEZIEHUNG");
            resultList.Add("V_ANLAGE_ZAEHLPUNKT_VNB", "V_ANLAGE_ZAEHLPUNKT_VNB");
            resultList.Add("V_ANLAGE_ZAEHLPUNKT_VNB_NETZEBENE", "V_ANLAGE_ZAEHLPUNKT_VNB_NETZEBENE");
            resultList.Add("V_ANLAGE_ZAEHLPUNKT_VNB_NETZEBENE_NUTZER", "V_ANLAGE_ZAEHLPUNKT_VNB_NETZEBENE_NUTZER");
            resultList.Add("V_ANLAGEN_ANFRAGE_EXCEL_EXPORT", "V_ANLAGEN_ANFRAGE_EXCEL_EXPORT");
            resultList.Add("V_ANLAGEN_BILANZKREIS_ZUORDNEN", "V_ANLAGEN_BILANZKREIS_ZUORDNEN");
            resultList.Add("V_ANLAGEN_HAENDLERRAHMENVERTRAG_MIT_NUTZER", "V_ANLAGEN_HAENDLERRAHMENVERTRAG_MIT_NUTZER");
            resultList.Add("V_ANLAGEN_OFFENE_POSITIONEN", "V_ANLAGEN_OFFENE_POSITIONEN");
            resultList.Add("V_ANSCHLUSSOBJEKT_DOMAINKEY", "V_ANSCHLUSSOBJEKT_DOMAINKEY");
            resultList.Add("V_ANSCHLUSSOBJEKT_STATUS", "V_ANSCHLUSSOBJEKT_STATUS");
            resultList.Add("V_ANSCHLUSSOBJEKTDATEN_AKTUELL", "V_ANSCHLUSSOBJEKTDATEN_AKTUELL");
            resultList.Add("V_ANSCHLUSSOBJEKTDATEN_HIST", "V_ANSCHLUSSOBJEKTDATEN_HIST");
            resultList.Add("V_ANSPRECHPARTNER_ERFASSEN_ABTEILUNG", "V_ANSPRECHPARTNER_ERFASSEN_ABTEILUNG");
            resultList.Add("V_ANSPRECHPARTNER_ERFASSEN_GESCHAEFTSPARTEI", "V_ANSPRECHPARTNER_ERFASSEN_GESCHAEFTSPARTEI");
            resultList.Add("V_ANWENDUNGSFALL_FUNKTION_ONLINEHILFE", "V_ANWENDUNGSFALL_FUNKTION_ONLINEHILFE");
            resultList.Add("V_ANWENDUNGSFALL_STAMMDATEN", "V_ANWENDUNGSFALL_STAMMDATEN");
            resultList.Add("V_AUSWERTUNG_DATENGRUPPE", "V_AUSWERTUNG_DATENGRUPPE");
            resultList.Add("V_AUSWERTUNG_PARAMETER", "V_AUSWERTUNG_PARAMETER");
            resultList.Add("V_AUTOMATIK_ANSCHLUSSOBJEKT_STATUS_WECHSEL", "V_AUTOMATIK_ANSCHLUSSOBJEKT_STATUS_WECHSEL");
            resultList.Add("V_AUTOMATIK_VERBRAUCHSSTELLE_ANLAGE", "V_AUTOMATIK_VERBRAUCHSSTELLE_ANLAGE");
            resultList.Add("V_AUTOMATIK_VERBRAUCHSSTELLE_STATUS_WECHSEL", "V_AUTOMATIK_VERBRAUCHSSTELLE_STATUS_WECHSEL");
            resultList.Add("V_AUTOMATIK_VERTRAG_VERTRAGSKONTO", "V_AUTOMATIK_VERTRAG_VERTRAGSKONTO");
            resultList.Add("V_AUTOMATIK_VERTRAGSKONTO_ANLAGE", "V_AUTOMATIK_VERTRAGSKONTO_ANLAGE");
            resultList.Add("V_BANK_LAENDERSCHLUESSEL", "V_BANK_LAENDERSCHLUESSEL");
            resultList.Add("V_BANKVERBINDUNG_BANK", "V_BANKVERBINDUNG_BANK");
            resultList.Add("V_BEITRITT_GENERIEREN", "V_BEITRITT_GENERIEREN");
            resultList.Add("V_BEITRITTSERKLAERUNG_DOMAINKEY", "V_BEITRITTSERKLAERUNG_DOMAINKEY");
            resultList.Add("V_BENCHMARK_DOMAINKEY", "V_BENCHMARK_DOMAINKEY");
            resultList.Add("V_BENCHMARK_LISTE", "V_BENCHMARK_LISTE");
            resultList.Add("V_BENCHMARK_VERTRAGZUORDNUNG", "V_BENCHMARK_VERTRAGZUORDNUNG");
            resultList.Add("V_BENUTZER_DATENGRUPPE", "V_BENUTZER_DATENGRUPPE");
            resultList.Add("V_BENUTZER_FUNKTIONSRECHTE", "V_BENUTZER_FUNKTIONSRECHTE");
            resultList.Add("V_BENUTZER_MANAGER_ANSPRECHP", "V_BENUTZER_MANAGER_ANSPRECHP");
            resultList.Add("V_BILANZKREIS", "V_BILANZKREIS");
            resultList.Add("V_BILANZKREIS_SPARTE", "V_BILANZKREIS_SPARTE");
            resultList.Add("V_BUENDELVERTRAG_DOMAINKEY", "V_BUENDELVERTRAG_DOMAINKEY");
            resultList.Add("V_CLUSTER_ENERGIEVERTEILUNG", "V_CLUSTER_ENERGIEVERTEILUNG");
            resultList.Add("V_CLUSTER_REFERENZWERTE", "V_CLUSTER_REFERENZWERTE");
            resultList.Add("V_CLUSTER_UMRECHNUNGSFAKTOREN", "V_CLUSTER_UMRECHNUNGSFAKTOREN");
            resultList.Add("V_DATAFACTORY_VERTRAG", "V_DATAFACTORY_VERTRAG");
            resultList.Add("V_DATAFACTORY_VERTRAG_IDS", "V_DATAFACTORY_VERTRAG_IDS");
            resultList.Add("V_DATENGRUPPE_BENUTZER", "V_DATENGRUPPE_BENUTZER");
            resultList.Add("V_DATENGRUPPE_BENUTZER_DOMAIN", "V_DATENGRUPPE_BENUTZER_DOMAIN");
            resultList.Add("V_DATENGRUPPE_BENUTZER_ORGANISATIONSEINHEIT", "V_DATENGRUPPE_BENUTZER_ORGANISATIONSEINHEIT");
            resultList.Add("V_DATENGRUPPE_BENUTZER_ORGANISATIONSEINHEIT_VTMANAGER", "V_DATENGRUPPE_BENUTZER_ORGANISATIONSEINHEIT_VTMANAGER");
            resultList.Add("V_DATENGRUPPE_ORGANISATIONSEINHEIT", "V_DATENGRUPPE_ORGANISATIONSEINHEIT");
            resultList.Add("V_DATENLOGGERDETAILS_EINGANGSGROESSE", "V_DATENLOGGERDETAILS_EINGANGSGROESSE");
            resultList.Add("V_DATENLOGGERDETAILS_EINGANGSGROESSE_NUTZART", "V_DATENLOGGERDETAILS_EINGANGSGROESSE_NUTZART");
            resultList.Add("V_DATENLOGGEREINGANGSGROESSE_DATENLOGGERMESSKREIS", "V_DATENLOGGEREINGANGSGROESSE_DATENLOGGERMESSKREIS");
            resultList.Add("V_DATENLOGGERMODUL_DETAILS_EINGANGSGROESSE", "V_DATENLOGGERMODUL_DETAILS_EINGANGSGROESSE");
            resultList.Add("V_DATENLOGGERMODUL_DETAILS_MESSKREIS", "V_DATENLOGGERMODUL_DETAILS_MESSKREIS");
            resultList.Add("V_DATENLOGGERMODUL_MODULTYP", "V_DATENLOGGERMODUL_MODULTYP");
            resultList.Add("V_DATENUMFELD_TECHNISCHE_STRUKTUR", "V_DATENUMFELD_TECHNISCHE_STRUKTUR");
            resultList.Add("V_DATENUMFELD_TECHNISCHE_STRUKTUR_NNE", "V_DATENUMFELD_TECHNISCHE_STRUKTUR_NNE");
            resultList.Add("V_EINHEIT_EINHEITENKATEGORIE", "V_EINHEIT_EINHEITENKATEGORIE");
            resultList.Add("V_EINZELEINKAUFSVERTRAG_EXCEL_EXPORT", "V_EINZELEINKAUFSVERTRAG_EXCEL_EXPORT");
            resultList.Add("V_EINZELEINKAUFSVERTRAG_SUCHE", "V_EINZELEINKAUFSVERTRAG_SUCHE");
            resultList.Add("V_EINZELVERTRAG_LISTE_EK", "V_EINZELVERTRAG_LISTE_EK");
            resultList.Add("V_EINZELVERTRAG_LISTE_EK_GEN_UNION", "V_EINZELVERTRAG_LISTE_EK_GEN_UNION");
            resultList.Add("V_EINZELVERTRAG_NUTZER", "V_EINZELVERTRAG_NUTZER");
            resultList.Add("V_EINZELVERTRAGSKONTO_SUCHPOPUP", "V_EINZELVERTRAGSKONTO_SUCHPOPUP");
            resultList.Add("V_EINZELVERTRAGSKONTO_SUCHPOPUP_EINKAUF", "V_EINZELVERTRAGSKONTO_SUCHPOPUP_EINKAUF");
            resultList.Add("V_ENERGIEBEZUGSVERTRAG_DOMAINKEY", "V_ENERGIEBEZUGSVERTRAG_DOMAINKEY");
            resultList.Add("V_ENERGIELIEFERVERTRAG_DOMAINKEY", "V_ENERGIELIEFERVERTRAG_DOMAINKEY");
            resultList.Add("V_ERGEBNIS_GENERIERTE_BEITRITTSERKLAERUNGEN", "V_ERGEBNIS_GENERIERTE_BEITRITTSERKLAERUNGEN");
            resultList.Add("V_ERGEBNIS_GENERIERTE_ENERGIELIEFERVERTRAEGE", "V_ERGEBNIS_GENERIERTE_ENERGIELIEFERVERTRAEGE");
            resultList.Add("V_ERGEBNIS_GENERIERTE_HAENDLEREINZELVERTRAEGE", "V_ERGEBNIS_GENERIERTE_HAENDLEREINZELVERTRAEGE");
            resultList.Add("V_EX_ExportPrognosysAnlageClusterZuordnung_DE", "V_EX_ExportPrognosysAnlageClusterZuordnung_DE");
            resultList.Add("V_EX_ExportPrognosysClusterReferenzAnlage_DE", "V_EX_ExportPrognosysClusterReferenzAnlage_DE");
            resultList.Add("V_FILE_IMPORT_STATUS", "V_FILE_IMPORT_STATUS");
            resultList.Add("V_FUNKTION_ANWENDUNGSFALL", "V_FUNKTION_ANWENDUNGSFALL");
            resultList.Add("V_FUNKTION_FUNKTIONSGRUPPE", "V_FUNKTION_FUNKTIONSGRUPPE");
            resultList.Add("V_FUNKTIONSGRUPPE_BENUTZER", "V_FUNKTIONSGRUPPE_BENUTZER");
            resultList.Add("V_GERAET_DATENLOGGER", "V_GERAET_DATENLOGGER");
            resultList.Add("V_GERAET_DEFEKT_GESCHAEFTSBEZIEHUNG", "V_GERAET_DEFEKT_GESCHAEFTSBEZIEHUNG");
            resultList.Add("V_GERAET_DOMAENE", "V_GERAET_DOMAENE");
            resultList.Add("V_GERAET_DOMAINKEY", "V_GERAET_DOMAINKEY");
            resultList.Add("V_GERAET_EXCEL_EXPORT", "V_GERAET_EXCEL_EXPORT");
            resultList.Add("V_GERAET_EXCEL_EXPORT_GP", "V_GERAET_EXCEL_EXPORT_GP");
            resultList.Add("V_GERAET_GESCHAEFTSBEZIEHUNGEN", "V_GERAET_GESCHAEFTSBEZIEHUNGEN");
            resultList.Add("V_GESCHAEFTSBEZIEHUNG_NUTZER", "V_GESCHAEFTSBEZIEHUNG_NUTZER");
            resultList.Add("V_GESCHAEFTSBEZIEHUNG_TEMPERATURGRUPPE", "V_GESCHAEFTSBEZIEHUNG_TEMPERATURGRUPPE");
            resultList.Add("V_GESCHAEFTSPARTEI_ABTEILUNG", "V_GESCHAEFTSPARTEI_ABTEILUNG");
            resultList.Add("V_GESCHAEFTSPARTEI_ABTEILUNG_ADRESSROLLE", "V_GESCHAEFTSPARTEI_ABTEILUNG_ADRESSROLLE");
            resultList.Add("V_GESCHAEFTSPARTEI_ABTEILUNG_GESCHAEFTSBEZIEHUNG", "V_GESCHAEFTSPARTEI_ABTEILUNG_GESCHAEFTSBEZIEHUNG");
            resultList.Add("V_GESCHAEFTSPARTEI_ABTEILUNG_PERSON_ADRESSE", "V_GESCHAEFTSPARTEI_ABTEILUNG_PERSON_ADRESSE");
            resultList.Add("V_GESCHAEFTSPARTEI_ADRESSE", "V_GESCHAEFTSPARTEI_ADRESSE");
            resultList.Add("V_GESCHAEFTSPARTEI_ANSPRECHPARTNER", "V_GESCHAEFTSPARTEI_ANSPRECHPARTNER");
            resultList.Add("V_GESCHAEFTSPARTEI_ANSPRECHPARTNER_PERSON", "V_GESCHAEFTSPARTEI_ANSPRECHPARTNER_PERSON");
            resultList.Add("V_GESCHAEFTSPARTEI_BEZIEHUNG", "V_GESCHAEFTSPARTEI_BEZIEHUNG");
            resultList.Add("V_GESCHAEFTSPARTEI_DOMAIN_ABTEILUNG", "V_GESCHAEFTSPARTEI_DOMAIN_ABTEILUNG");
            resultList.Add("V_GESCHAEFTSPARTEI_DOMAIN_ADRESSE", "V_GESCHAEFTSPARTEI_DOMAIN_ADRESSE");
            resultList.Add("V_GESCHAEFTSPARTEI_DOMAIN_ANSPRECHPARTNER", "V_GESCHAEFTSPARTEI_DOMAIN_ANSPRECHPARTNER");
            resultList.Add("V_GESCHAEFTSPARTEI_DOMAIN_KOMMUNIKATIONSKANAL", "V_GESCHAEFTSPARTEI_DOMAIN_KOMMUNIKATIONSKANAL");
            resultList.Add("V_GESCHAEFTSPARTEI_EXTERNE_NUMMER", "V_GESCHAEFTSPARTEI_EXTERNE_NUMMER");
            resultList.Add("V_GESCHAEFTSPARTEI_FIRMA_DOMAINKEY", "V_GESCHAEFTSPARTEI_FIRMA_DOMAINKEY");
            resultList.Add("V_GESCHAEFTSPARTEI_GESCHAEFTSBEZIEHUNG", "V_GESCHAEFTSPARTEI_GESCHAEFTSBEZIEHUNG");
            resultList.Add("V_GESCHAEFTSPARTEI_GESCHAEFTSBEZIEHUNG_VERTRAG", "V_GESCHAEFTSPARTEI_GESCHAEFTSBEZIEHUNG_VERTRAG");
            resultList.Add("V_GESCHAEFTSPARTEI_GESCHAEFTSBEZIEHUNG_VERTRAG_EXPORT", "V_GESCHAEFTSPARTEI_GESCHAEFTSBEZIEHUNG_VERTRAG_EXPORT");
            resultList.Add("V_GESCHAEFTSPARTEI_GESCHAEFTSBEZIEHUNG_ZAEHLPUNKT_EXPORT", "V_GESCHAEFTSPARTEI_GESCHAEFTSBEZIEHUNG_ZAEHLPUNKT_EXPORT");
            resultList.Add("V_GESCHAEFTSPARTEI_GESCHAEFTSPARTEIGRUPPEN", "V_GESCHAEFTSPARTEI_GESCHAEFTSPARTEIGRUPPEN");
            resultList.Add("V_GESCHAEFTSPARTEI_GESCHAEFTSPARTEIGRUPPEN_ZUORDNENBAR", "V_GESCHAEFTSPARTEI_GESCHAEFTSPARTEIGRUPPEN_ZUORDNENBAR");
            resultList.Add("V_GESCHAEFTSPARTEI_HI_GESCHAEFTSPARTEI", "V_GESCHAEFTSPARTEI_HI_GESCHAEFTSPARTEI");
            resultList.Add("V_GESCHAEFTSPARTEI_KOMMUNIKATION", "V_GESCHAEFTSPARTEI_KOMMUNIKATION");
            resultList.Add("V_GESCHAEFTSPARTEI_KOMMUNIKATION_EMAIL", "V_GESCHAEFTSPARTEI_KOMMUNIKATION_EMAIL");
            resultList.Add("V_GESCHAEFTSPARTEI_KOMMUNIKATION_FAX", "V_GESCHAEFTSPARTEI_KOMMUNIKATION_FAX");
            resultList.Add("V_GESCHAEFTSPARTEI_KOMMUNIKATION_PERSON", "V_GESCHAEFTSPARTEI_KOMMUNIKATION_PERSON");
            resultList.Add("V_GESCHAEFTSPARTEI_KOMMUNIKATION_TELEFON", "V_GESCHAEFTSPARTEI_KOMMUNIKATION_TELEFON");
            resultList.Add("V_GESCHAEFTSPARTEI_LISTE", "V_GESCHAEFTSPARTEI_LISTE");
            resultList.Add("V_GESCHAEFTSPARTEI_LISTE_EXTNRTYP", "V_GESCHAEFTSPARTEI_LISTE_EXTNRTYP");
            resultList.Add("V_GESCHAEFTSPARTEI_LISTE_INKL_SPEZ_ROLLEN", "V_GESCHAEFTSPARTEI_LISTE_INKL_SPEZ_ROLLEN");
            resultList.Add("V_GESCHAEFTSPARTEI_LISTE_OHNE_ROLLE", "V_GESCHAEFTSPARTEI_LISTE_OHNE_ROLLE");
            resultList.Add("V_GESCHAEFTSPARTEI_TARIFZEITENKALENDER", "V_GESCHAEFTSPARTEI_TARIFZEITENKALENDER");
            resultList.Add("V_GESCHAEFTSPARTEI_VNB_BILANZKREIS", "V_GESCHAEFTSPARTEI_VNB_BILANZKREIS");
            resultList.Add("V_GESCHAEFTSPARTEIGRUPPE", "V_GESCHAEFTSPARTEIGRUPPE");
            resultList.Add("V_GESCHAEFTSPARTEIGRUPPE_PREISSEGMENT", "V_GESCHAEFTSPARTEIGRUPPE_PREISSEGMENT");
            resultList.Add("V_GESCHAEFTSPARTEIGRUPPE_PREISSEGMENT_STAMMDATEN", "V_GESCHAEFTSPARTEIGRUPPE_PREISSEGMENT_STAMMDATEN");
            resultList.Add("V_GETKOSTEN_ANLAGE_MIT_HISTORIE", "V_GETKOSTEN_ANLAGE_MIT_HISTORIE");
            resultList.Add("V_GETKOSTEN_BUENDELVERTRAG", "V_GETKOSTEN_BUENDELVERTRAG");
            resultList.Add("V_GETKOSTEN_BUENDELVERTRAG_ANGEBOT", "V_GETKOSTEN_BUENDELVERTRAG_ANGEBOT");
            resultList.Add("V_GETKOSTEN_EINZELVERTRAG", "V_GETKOSTEN_EINZELVERTRAG");
            resultList.Add("V_GETKOSTEN_GESCHAEFTSPARTEIGRUPPE", "V_GETKOSTEN_GESCHAEFTSPARTEIGRUPPE");
            resultList.Add("V_GETKOSTEN_KALKULATIONSVARIANTE", "V_GETKOSTEN_KALKULATIONSVARIANTE");
            resultList.Add("V_GETKOSTEN_NNE_ANLAGE_VNBS", "V_GETKOSTEN_NNE_ANLAGE_VNBS");
            resultList.Add("V_GETKOSTEN_PREISSEGMENT", "V_GETKOSTEN_PREISSEGMENT");
            resultList.Add("V_GETKOSTEN_RABATT", "V_GETKOSTEN_RABATT");
            resultList.Add("V_GETKOSTEN_STAFFEL", "V_GETKOSTEN_STAFFEL");
            resultList.Add("V_GETKOSTEN_TARIFTYPEN", "V_GETKOSTEN_TARIFTYPEN");
            resultList.Add("V_GETKOSTEN_VERTRAG_PREISSEGMENT", "V_GETKOSTEN_VERTRAG_PREISSEGMENT");
            resultList.Add("V_GP_EXTERNE_NUMMER_AKTUELL", "V_GP_EXTERNE_NUMMER_AKTUELL");
            resultList.Add("V_GP_GESCHAEFTSBEZIEHUNG", "V_GP_GESCHAEFTSBEZIEHUNG");
            resultList.Add("V_GP_LISTE_NETZBETREIBER", "V_GP_LISTE_NETZBETREIBER");
            resultList.Add("V_GP_NETZBETREIBER_BEZIEHUNG", "V_GP_NETZBETREIBER_BEZIEHUNG");
            resultList.Add("V_GP_NETZBETREIBER_PLZ", "V_GP_NETZBETREIBER_PLZ");
            resultList.Add("V_GPREGULIERER_DOMAINKEY", "V_GPREGULIERER_DOMAINKEY");
            resultList.Add("V_HAENDLEREINZELVERTRAEGE_KUENDIGUNG_EINZELVERTRAEGE_AUSWAHL", "V_HAENDLEREINZELVERTRAEGE_KUENDIGUNG_EINZELVERTRAEGE_AUSWAHL");
            resultList.Add("V_HAENDLEREINZELVERTRAG_DOM_SUCHE", "V_HAENDLEREINZELVERTRAG_DOM_SUCHE");
            resultList.Add("V_HAENDLEREINZELVERTRAG_DOMAINKEY", "V_HAENDLEREINZELVERTRAG_DOMAINKEY");
            resultList.Add("V_HAENDLERRAHMENVERTRAG_ANLAGE_ZUORDNUNG", "V_HAENDLERRAHMENVERTRAG_ANLAGE_ZUORDNUNG");
            resultList.Add("V_HEIZOELPREIS", "V_HEIZOELPREIS");
            resultList.Add("V_HEV_FREIGEBEN", "V_HEV_FREIGEBEN");
            resultList.Add("V_HI_ANLAGE_AKTUELL", "V_HI_ANLAGE_AKTUELL");
            resultList.Add("V_HI_ANLAGE_STATUS", "V_HI_ANLAGE_STATUS");
            resultList.Add("V_KO_ANLAGE_JAHRESWERTE", "V_KO_ANLAGE_JAHRESWERTE");
            resultList.Add("V_KO_ANSPRECHPARTNER_TELEFON", "V_KO_ANSPRECHPARTNER_TELEFON");
            resultList.Add("V_KO_DATENLOGGER_GESCHAEFTSBEZIEHUNG", "V_KO_DATENLOGGER_GESCHAEFTSBEZIEHUNG");
            resultList.Add("V_KO_DATENLOGGER_KANAL_1_16", "V_KO_DATENLOGGER_KANAL_1_16");
            resultList.Add("V_KO_EINZELVERTRAG", "V_KO_EINZELVERTRAG");
            resultList.Add("V_KO_GESCHAEFTSPARTEI_ADRESSE", "V_KO_GESCHAEFTSPARTEI_ADRESSE");
            resultList.Add("V_KO_GP_ANSCHRECHPARTNER_FAX", "V_KO_GP_ANSCHRECHPARTNER_FAX");
            resultList.Add("V_KO_GP_ANSPRECHPARTNER_TELEFON", "V_KO_GP_ANSPRECHPARTNER_TELEFON");
            resultList.Add("V_KO_GP_NUTZER_ADRESSE", "V_KO_GP_NUTZER_ADRESSE");
            resultList.Add("V_KO_GP_NUTZER_ADRESSE_EINKAUF", "V_KO_GP_NUTZER_ADRESSE_EINKAUF");
            resultList.Add("V_KO_Inbetriebnahmeprotokoll", "V_KO_Inbetriebnahmeprotokoll");
            resultList.Add("V_KO_MAX_KUENDIGUNGZUM_ANLAGE", "V_KO_MAX_KUENDIGUNGZUM_ANLAGE");
            resultList.Add("V_KO_NUTZER", "V_KO_NUTZER");
            resultList.Add("V_KO_VERBRAUCHSSTELLE_VERTRAG", "V_KO_VERBRAUCHSSTELLE_VERTRAG");
            resultList.Add("V_KO_VERTRAGSKONTO_ZFUe", "V_KO_VERTRAGSKONTO_ZFUe");
            resultList.Add("V_KO_Vertragspartner_NETZGEBIET", "V_KO_Vertragspartner_NETZGEBIET");
            resultList.Add("V_KOMAGRUPPE_BENUTZER", "V_KOMAGRUPPE_BENUTZER");
            resultList.Add("V_KOMMUNIKATION_KANAL_DATUM", "V_KOMMUNIKATION_KANAL_DATUM");
            resultList.Add("V_KOMMUNIKATIONSKANAL_AENDERN", "V_KOMMUNIKATIONSKANAL_AENDERN");
            resultList.Add("V_KOMMUNIKATIONSKANAL_ZU_ANSPRECHPARTNER_ERFASSEN_ANSPRECHPARTNER", "V_KOMMUNIKATIONSKANAL_ZU_ANSPRECHPARTNER_ERFASSEN_ANSPRECHPARTNER");
            resultList.Add("V_KOMMUNIKATIONSKANAL_ZU_ANSPRECHPARTNER_ERFASSEN_GESCHAEFTSPARTEI", "V_KOMMUNIKATIONSKANAL_ZU_ANSPRECHPARTNER_ERFASSEN_GESCHAEFTSPARTEI");
            resultList.Add("V_KONDITION", "V_KONDITION");
            resultList.Add("V_KONDITIONSART", "V_KONDITIONSART");
            resultList.Add("V_KONTIERUNGSOBJEKT_VERTRAGSART", "V_KONTIERUNGSOBJEKT_VERTRAGSART");
            resultList.Add("V_KOOPERATIONSVERTRAG_DOMAINKEY", "V_KOOPERATIONSVERTRAG_DOMAINKEY");
            resultList.Add("V_KREDITOR_ZUORDNEN_KREDITOR_AUSWAHL", "V_KREDITOR_ZUORDNEN_KREDITOR_AUSWAHL");
            resultList.Add("V_KUNDENORGANISATION_FLAECHENART", "V_KUNDENORGANISATION_FLAECHENART");
            resultList.Add("V_KUNDENORGANISATION_VERTRAG", "V_KUNDENORGANISATION_VERTRAG");
            resultList.Add("V_KUNDENSTAMM_REGION", "V_KUNDENSTAMM_REGION");
            resultList.Add("V_MARKTSEGMENT_PROFILTYP", "V_MARKTSEGMENT_PROFILTYP");
            resultList.Add("V_MESSART", "V_MESSART");
            resultList.Add("V_MONITOR_EK_VERTRAGSBEGINN", "V_MONITOR_EK_VERTRAGSBEGINN");
            resultList.Add("V_MONITOR_KUENDIGUNG_EK", "V_MONITOR_KUENDIGUNG_EK");
            resultList.Add("V_MONITOR_KUENDIGUNG_EK_STATUS_INK", "V_MONITOR_KUENDIGUNG_EK_STATUS_INK");
            resultList.Add("V_MONITOR_KUENDIGUNG_EK_VERTRAGSKONFORM", "V_MONITOR_KUENDIGUNG_EK_VERTRAGSKONFORM");
            resultList.Add("V_MONITOR_KUENDIGUNG_VRK", "V_MONITOR_KUENDIGUNG_VRK");
            resultList.Add("V_MONITOR_NETZNUTZUNG", "V_MONITOR_NETZNUTZUNG");
            resultList.Add("V_MONITORING_SCHNITTSTELLEN", "V_MONITORING_SCHNITTSTELLEN");
            resultList.Add("V_MONITORING_SCHNITTSTELLEN_RUECKMELDUNGEN", "V_MONITORING_SCHNITTSTELLEN_RUECKMELDUNGEN");
            resultList.Add("V_NETZBETREIBER_AKTUELL", "V_NETZBETREIBER_AKTUELL");
            resultList.Add("V_NETZBETREIBER_PLZ", "V_NETZBETREIBER_PLZ");
            resultList.Add("V_NETZEBENE", "V_NETZEBENE");
            resultList.Add("V_NETZNUTZUNGSENTGELTE", "V_NETZNUTZUNGSENTGELTE");
            resultList.Add("V_NETZNUTZUNGSENTGELTE_AT", "V_NETZNUTZUNGSENTGELTE_AT");
            resultList.Add("V_NETZNUTZUNGSENTGELTE_AT_FALLBACK", "V_NETZNUTZUNGSENTGELTE_AT_FALLBACK");
            resultList.Add("V_NETZNUTZUNGSENTGELTE_FALLBACK", "V_NETZNUTZUNGSENTGELTE_FALLBACK");
            resultList.Add("V_NN_ARBEITSLISTE_EXPORT", "V_NN_ARBEITSLISTE_EXPORT");
            resultList.Add("V_NN_ENTGELT_KA_MANUELL", "V_NN_ENTGELT_KA_MANUELL");
            resultList.Add("V_NN_ENTGELT_UEBERSICHT_AT", "V_NN_ENTGELT_UEBERSICHT_AT");
            resultList.Add("V_NN_ENTGELT_UEBERSICHT_DE", "V_NN_ENTGELT_UEBERSICHT_DE");
            resultList.Add("V_NNE_ABGLEICH_VNB_ZAEHLPUNKT", "V_NNE_ABGLEICH_VNB_ZAEHLPUNKT");
            resultList.Add("V_NNE_REFDATENKORREKTUR_AKTUELLE_VNB", "V_NNE_REFDATENKORREKTUR_AKTUELLE_VNB");
            resultList.Add("V_NNE_REFDATENKORREKTUR_POTENZIELLE_VNB", "V_NNE_REFDATENKORREKTUR_POTENZIELLE_VNB");
            resultList.Add("V_NNE_REFDATENKORREKTUR_POTENZIELLE_VNB_LISTE", "V_NNE_REFDATENKORREKTUR_POTENZIELLE_VNB_LISTE");
            resultList.Add("V_NNE_ZAEHLPUNKT_PLZ_ORT", "V_NNE_ZAEHLPUNKT_PLZ_ORT");
            resultList.Add("V_NNEAT_REFDATENKORREKTUR_POTENZIELLE_VNB", "V_NNEAT_REFDATENKORREKTUR_POTENZIELLE_VNB");
            resultList.Add("V_NNEAT_REFDATENKORREKTUR_POTENZIELLE_VNB_LISTE", "V_NNEAT_REFDATENKORREKTUR_POTENZIELLE_VNB_LISTE");
            resultList.Add("V_NUTZART_SPARTE", "V_NUTZART_SPARTE");
            resultList.Add("V_NUTZENERGIE_FAKTOREN", "V_NUTZENERGIE_FAKTOREN");
            resultList.Add("V_NUTZENERGIE_PROZENTE", "V_NUTZENERGIE_PROZENTE");
            resultList.Add("V_NUTZENERGIE_TAGESARTEN", "V_NUTZENERGIE_TAGESARTEN");
            resultList.Add("V_NUTZER_LISTE", "V_NUTZER_LISTE");
            resultList.Add("V_OFFENE_POSITIONEN_AF1706", "V_OFFENE_POSITIONEN_AF1706");
            resultList.Add("V_OFFENEPOSITION_AN_AB_MELDUNG_NETZNUTZUNG", "V_OFFENEPOSITION_AN_AB_MELDUNG_NETZNUTZUNG");
            resultList.Add("V_OFFENEPOSITION_DOMAINKEY", "V_OFFENEPOSITION_DOMAINKEY");
            resultList.Add("V_OFFENEPOSITION_EXCELEXPORT", "V_OFFENEPOSITION_EXCELEXPORT");
            resultList.Add("V_OFFENEPOSITION_MANAGER", "V_OFFENEPOSITION_MANAGER");
            resultList.Add("V_ONLINEHILFE_AF_SPARTE", "V_ONLINEHILFE_AF_SPARTE");
            resultList.Add("V_PERSON_VT_GP_ANREDE_ABTEILUNG", "V_PERSON_VT_GP_ANREDE_ABTEILUNG");
            resultList.Add("V_POT_VERTRAGSPARTNER", "V_POT_VERTRAGSPARTNER");
            resultList.Add("V_POT_VERTRAGSPARTNER_VERSAND", "V_POT_VERTRAGSPARTNER_VERSAND");
            resultList.Add("V_PREISART", "V_PREISART");
            resultList.Add("V_PRF_ABSCHLAGSSTOP", "V_PRF_ABSCHLAGSSTOP");
            resultList.Add("V_PRF_ABSCHLAGSSTOP_1", "V_PRF_ABSCHLAGSSTOP_1");
            resultList.Add("V_PRF_ABSCHLAGSSTOP_2", "V_PRF_ABSCHLAGSSTOP_2");
            resultList.Add("V_PRF_ANSICHT_ALLER_ORDNUNGSBEGRIFFEN", "V_PRF_ANSICHT_ALLER_ORDNUNGSBEGRIFFEN");
            resultList.Add("V_PRF_ENERGIELIEFERVERTRAG_BEITRITTSERKLAERUNG", "V_PRF_ENERGIELIEFERVERTRAG_BEITRITTSERKLAERUNG");
            resultList.Add("V_PRF_NICHTEINGEZOGENE_ABSCHLAEGE", "V_PRF_NICHTEINGEZOGENE_ABSCHLAEGE");
            resultList.Add("V_PRF_VERTRAGSKONTO_SD_TRANSFERIERT", "V_PRF_VERTRAGSKONTO_SD_TRANSFERIERT");
            resultList.Add("V_PRF_VERTRAGSKONTO_SD_VALIDIERT", "V_PRF_VERTRAGSKONTO_SD_VALIDIERT");
            resultList.Add("V_PRF_VK_VERTRAG_KUENDIGUNG_2005_ABSCHLAG_IN_ZUKUNFT", "V_PRF_VK_VERTRAG_KUENDIGUNG_2005_ABSCHLAG_IN_ZUKUNFT");
            resultList.Add("V_PRF_VK_VERTRAG_OHNE_ABSCHLAG", "V_PRF_VK_VERTRAG_OHNE_ABSCHLAG");
            resultList.Add("V_PROFILTYP", "V_PROFILTYP");
            resultList.Add("V_PROFILTYPSTUNDENWECHSEL", "V_PROFILTYPSTUNDENWECHSEL");
            resultList.Add("V_RABATTYP", "V_RABATTYP");
            resultList.Add("V_RAHMENBEITRITTSERKLAERUNG_DOMAINKEY", "V_RAHMENBEITRITTSERKLAERUNG_DOMAINKEY");
            resultList.Add("V_RAHMENBEITRITTSERKLAERUNG_PLAUSIBILITAET", "V_RAHMENBEITRITTSERKLAERUNG_PLAUSIBILITAET");
            resultList.Add("V_RAHMENENERGIEBERATUNGSVERTRAG_DOMAINKEY", "V_RAHMENENERGIEBERATUNGSVERTRAG_DOMAINKEY");
            resultList.Add("V_RAHMENENERGIELIEFERVERTRAG_DOMAINKEY", "V_RAHMENENERGIELIEFERVERTRAG_DOMAINKEY");
            resultList.Add("V_RAHMENVERTRAG_ANLAGE", "V_RAHMENVERTRAG_ANLAGE");
            resultList.Add("V_RAHMENVERTRAGSKONTO_RECHNUNGSSTELLER", "V_RAHMENVERTRAGSKONTO_RECHNUNGSSTELLER");
            resultList.Add("V_RBE_EXCELEXPORT", "V_RBE_EXCELEXPORT");
            resultList.Add("V_RECHNUNGSSTELLER_ZUORDNEN_RECHNUNGSSTELLER_AUSWAHL", "V_RECHNUNGSSTELLER_ZUORDNEN_RECHNUNGSSTELLER_AUSWAHL");
            resultList.Add("V_REFERENZBETRIEB_DOMAINKEY", "V_REFERENZBETRIEB_DOMAINKEY");
            resultList.Add("V_REFERENZBETRIEB_KENNGROESSEN", "V_REFERENZBETRIEB_KENNGROESSEN");
            resultList.Add("V_REFERENZBETRIEB_PREISSEGMENT", "V_REFERENZBETRIEB_PREISSEGMENT");
            resultList.Add("V_REFERENZBETRIEBSSTAETTE_ANLAGEN_UEBERSICHT", "V_REFERENZBETRIEBSSTAETTE_ANLAGEN_UEBERSICHT");
            resultList.Add("V_REFERENZBETRIEBSSTAETTE_JAHRESZEITWERTE", "V_REFERENZBETRIEBSSTAETTE_JAHRESZEITWERTE");
            resultList.Add("V_REGION", "V_REGION");
            resultList.Add("V_ROLLE_GESCHAEFTSBEZIEHUNG_GESCHAEFTSPARTEI", "V_ROLLE_GESCHAEFTSBEZIEHUNG_GESCHAEFTSPARTEI");
            resultList.Add("V_ROLLE_ZUORDNEN_GP_LISTE", "V_ROLLE_ZUORDNEN_GP_LISTE");
            resultList.Add("V_ROLLE_ZUORDNEN_GP_ZUGEORDNETE_ROLLEN", "V_ROLLE_ZUORDNEN_GP_ZUGEORDNETE_ROLLEN");
            resultList.Add("V_ROLLEN_GESCHAEFTSPARTEI", "V_ROLLEN_GESCHAEFTSPARTEI");
            resultList.Add("V_SACHBEARBEITER_OFFENEPOSITION", "V_SACHBEARBEITER_OFFENEPOSITION");
            resultList.Add("V_SDEXPORT_GESCHAEFTSBEZIEHUNG_ADRESSE", "V_SDEXPORT_GESCHAEFTSBEZIEHUNG_ADRESSE");
            resultList.Add("V_SDEXPORT_GESCHAEFTSBEZIEHUNG_EXTERNE_NR", "V_SDEXPORT_GESCHAEFTSBEZIEHUNG_EXTERNE_NR");
            resultList.Add("V_SDEXPORT_VERTRAGSKONTO_BUCHUNGSKREISSACHBEARBEITER", "V_SDEXPORT_VERTRAGSKONTO_BUCHUNGSKREISSACHBEARBEITER");
            resultList.Add("V_SDEXPORT_VERTRAGSKONTO_GESCHAEFTSPARTEI", "V_SDEXPORT_VERTRAGSKONTO_GESCHAEFTSPARTEI");
            resultList.Add("V_SDEXPORT_VERTRAGSKONTO_NUTZER", "V_SDEXPORT_VERTRAGSKONTO_NUTZER");
            resultList.Add("V_SDEXPORT_VERTRAGSKONTO_VERBRAUCHSSTELLE_ADRESSE", "V_SDEXPORT_VERTRAGSKONTO_VERBRAUCHSSTELLE_ADRESSE");
            resultList.Add("V_SDEXPORT_VERTRAGSKONTO_ZAHLUNGSBEDINGUNG", "V_SDEXPORT_VERTRAGSKONTO_ZAHLUNGSBEDINGUNG");
            resultList.Add("V_STAFFEL_STAFFELART", "V_STAFFEL_STAFFELART");
            resultList.Add("V_STAMMDATENEXPORT_ANLAGE_BASIS", "V_STAMMDATENEXPORT_ANLAGE_BASIS");
            resultList.Add("V_STAMMDATENEXPORT_ANLAGE_MESSARTEN", "V_STAMMDATENEXPORT_ANLAGE_MESSARTEN");
            resultList.Add("V_STAMMDATENEXPORT_ANLAGE_PROFILTYP", "V_STAMMDATENEXPORT_ANLAGE_PROFILTYP");
            resultList.Add("V_STAMMDATENEXPORT_ANLAGE_VERTRAG", "V_STAMMDATENEXPORT_ANLAGE_VERTRAG");
            resultList.Add("V_STAMMDATENEXPORT_ANLAGE_VERTRAG_ALL", "V_STAMMDATENEXPORT_ANLAGE_VERTRAG_ALL");
            resultList.Add("V_STAMMDATENEXPORT_ANLAGE_VERTRAG_GESCHAEFTSPARTEIEN", "V_STAMMDATENEXPORT_ANLAGE_VERTRAG_GESCHAEFTSPARTEIEN");
            resultList.Add("V_STAMMDATENEXPORT_ANLAGE_VERTRAG_PREISSEGMENT", "V_STAMMDATENEXPORT_ANLAGE_VERTRAG_PREISSEGMENT");
            resultList.Add("V_STAMMDATENEXPORT_ANSCHLUSSOBJEKT_ADRESSE", "V_STAMMDATENEXPORT_ANSCHLUSSOBJEKT_ADRESSE");
            resultList.Add("V_STAMMDATENEXPORT_KREDITOR_ADRESSE", "V_STAMMDATENEXPORT_KREDITOR_ADRESSE");
            resultList.Add("V_STAMMDATENEXPORT_KREDITOR_BANKVERBINDUNGEN", "V_STAMMDATENEXPORT_KREDITOR_BANKVERBINDUNGEN");
            resultList.Add("V_STAMMDATENEXPORT_KREDITOR_BASIS", "V_STAMMDATENEXPORT_KREDITOR_BASIS");
            resultList.Add("V_STAMMDATENEXPORT_KREDITOR_EXTERNE_NUMMER", "V_STAMMDATENEXPORT_KREDITOR_EXTERNE_NUMMER");
            resultList.Add("V_STAMMDATENEXPORT_VERBRAUCHSSTELLE_ADRESSE", "V_STAMMDATENEXPORT_VERBRAUCHSSTELLE_ADRESSE");
            resultList.Add("V_STAMMDATENEXPORT_VERTRAG_NUTZER", "V_STAMMDATENEXPORT_VERTRAG_NUTZER");
            resultList.Add("V_STAMMDATENEXPORT_ZAEHLPUNKT_VNB", "V_STAMMDATENEXPORT_ZAEHLPUNKT_VNB");
            resultList.Add("V_STEUERERLAUBNISSCHEIN_SPARTE", "V_STEUERERLAUBNISSCHEIN_SPARTE");
            resultList.Add("V_STROMSTEUERERLAUBNISSCHEIN", "V_STROMSTEUERERLAUBNISSCHEIN");
            resultList.Add("V_STUECKLISTE_AKTIVGRUPPE", "V_STUECKLISTE_AKTIVGRUPPE");
            resultList.Add("V_STUECKLISTE_VERTRAG", "V_STUECKLISTE_VERTRAG");
            resultList.Add("V_STUECKLISTENPOSITION_HERSTELLERTYP", "V_STUECKLISTENPOSITION_HERSTELLERTYP");
            resultList.Add("V_STUECKLISTENPOSITION_LST", "V_STUECKLISTENPOSITION_LST");
            resultList.Add("V_TAGESAKTUELLER_ENERGIEPREIS", "V_TAGESAKTUELLER_ENERGIEPREIS");
            resultList.Add("V_TARIFART", "V_TARIFART");
            resultList.Add("V_TARIFSCHEMA_ANLAGE_VERTRAEGE", "V_TARIFSCHEMA_ANLAGE_VERTRAEGE");
            resultList.Add("V_TARIFSCHEMA_ANLAGE_ZUGEORDNETE_VERTRAEGE", "V_TARIFSCHEMA_ANLAGE_ZUGEORDNETE_VERTRAEGE");
            resultList.Add("V_TARIFSCHEMA_ANLAGENUEBERSICHT", "V_TARIFSCHEMA_ANLAGENUEBERSICHT");
            resultList.Add("V_TARIFSCHEMA_DOMAINKEY", "V_TARIFSCHEMA_DOMAINKEY");
            resultList.Add("V_TARIFSCHEMA_KONDITIONEN_PREISE", "V_TARIFSCHEMA_KONDITIONEN_PREISE");
            resultList.Add("V_TARIFSCHEMA_KONDITIONEN_STAFFELN", "V_TARIFSCHEMA_KONDITIONEN_STAFFELN");
            resultList.Add("V_TARIFSCHEMA_KOOPERATION", "V_TARIFSCHEMA_KOOPERATION");
            resultList.Add("V_TARIFSCHEMA_RABATT", "V_TARIFSCHEMA_RABATT");
            resultList.Add("V_TARIFSCHEMA_RABATTSTAFFEL", "V_TARIFSCHEMA_RABATTSTAFFEL");
            resultList.Add("V_TARIFSCHEMA_RAHMENENERGIEBEZUGSVERTRAG", "V_TARIFSCHEMA_RAHMENENERGIEBEZUGSVERTRAG");
            resultList.Add("V_TARIFSCHEMA_STANDARD", "V_TARIFSCHEMA_STANDARD");
            resultList.Add("V_TARIFSCHEMA_STATUS", "V_TARIFSCHEMA_STATUS");
            resultList.Add("V_TARIFSCHEMA_WITHOUT_STORNIERT", "V_TARIFSCHEMA_WITHOUT_STORNIERT");
            resultList.Add("V_TARIFSCHEMAKOPF", "V_TARIFSCHEMAKOPF");
            resultList.Add("V_TARIFSCHEMAPOSITION_KONDITION", "V_TARIFSCHEMAPOSITION_KONDITION");
            resultList.Add("V_TARIFSCHEMAPOSITION_PREISE", "V_TARIFSCHEMAPOSITION_PREISE");
            resultList.Add("V_TARIFSCHEMAPOSITION_PREISE_KOOPERATION", "V_TARIFSCHEMAPOSITION_PREISE_KOOPERATION");
            resultList.Add("V_TARIFSCHEMAPOSITION_STAFFELN", "V_TARIFSCHEMAPOSITION_STAFFELN");
            resultList.Add("V_TARIFSCHEMAVORLAGE", "V_TARIFSCHEMAVORLAGE");
            resultList.Add("V_TARIFSCHEMAVORLAGE_ALLE", "V_TARIFSCHEMAVORLAGE_ALLE");
            resultList.Add("V_TARIFSCHEMAVORLAGE_KOOPERATION", "V_TARIFSCHEMAVORLAGE_KOOPERATION");
            resultList.Add("V_TARIFTYP", "V_TARIFTYP");
            resultList.Add("V_TEMPERATURGRUPPE_PROFILTYP", "V_TEMPERATURGRUPPE_PROFILTYP");
            resultList.Add("V_VERBRAUCHSSTELLE_DATEN", "V_VERBRAUCHSSTELLE_DATEN");
            resultList.Add("V_VERBRAUCHSSTELLE_DATEN_AKTUELL", "V_VERBRAUCHSSTELLE_DATEN_AKTUELL");
            resultList.Add("V_VERBRAUCHSSTELLE_DATEN_AKTUELL_ALLE", "V_VERBRAUCHSSTELLE_DATEN_AKTUELL_ALLE");
            resultList.Add("V_VERBRAUCHSSTELLE_DATEN_ANLAGE", "V_VERBRAUCHSSTELLE_DATEN_ANLAGE");
            resultList.Add("V_VERBRAUCHSSTELLE_DATEN_ANLAGE_ALLE", "V_VERBRAUCHSSTELLE_DATEN_ANLAGE_ALLE");
            resultList.Add("V_VERBRAUCHSSTELLE_DATEN_HIST_ANZ", "V_VERBRAUCHSSTELLE_DATEN_HIST_ANZ");
            resultList.Add("V_VERBRAUCHSSTELLE_DATEN_PRODUKT", "V_VERBRAUCHSSTELLE_DATEN_PRODUKT");
            resultList.Add("V_VERBRAUCHSSTELLE_DATEN_PRODUKT_ALLE", "V_VERBRAUCHSSTELLE_DATEN_PRODUKT_ALLE");
            resultList.Add("V_VERBRAUCHSSTELLE_DOMAINKEY", "V_VERBRAUCHSSTELLE_DOMAINKEY");
            resultList.Add("V_VERBRAUCHSSTELLE_EIGENTUEMER_ALLE", "V_VERBRAUCHSSTELLE_EIGENTUEMER_ALLE");
            resultList.Add("V_VERBRAUCHSSTELLE_EIGENTUEMER_EXCEL_EXPORT", "V_VERBRAUCHSSTELLE_EIGENTUEMER_EXCEL_EXPORT");
            resultList.Add("V_VERBRAUCHSSTELLE_GESCHAEFTSPARTEI_FIRMA", "V_VERBRAUCHSSTELLE_GESCHAEFTSPARTEI_FIRMA");
            resultList.Add("V_VERBRAUCHSSTELLE_GESCHAEFTSPARTEI_PERSON", "V_VERBRAUCHSSTELLE_GESCHAEFTSPARTEI_PERSON");
            resultList.Add("V_VERBRAUCHSSTELLE_NUTZER", "V_VERBRAUCHSSTELLE_NUTZER");
            resultList.Add("V_VERBRAUCHSSTELLE_NUTZER_EXCEL_EXPORT", "V_VERBRAUCHSSTELLE_NUTZER_EXCEL_EXPORT");
            resultList.Add("V_VERBRAUCHSSTELLE_STATUS", "V_VERBRAUCHSSTELLE_STATUS");
            resultList.Add("V_VERBRAUCHSSTELLE_VALIDIERUNG", "V_VERBRAUCHSSTELLE_VALIDIERUNG");
            resultList.Add("V_VERBRAUCHSSTELLE_VERTRAEGE", "V_VERBRAUCHSSTELLE_VERTRAEGE");
            resultList.Add("V_VERTRAG_ALLGEMEIN", "V_VERTRAG_ALLGEMEIN");
            resultList.Add("V_VERTRAG_ANSPRECHPARTNER", "V_VERTRAG_ANSPRECHPARTNER");
            resultList.Add("V_VERTRAG_BENUTZER_DATENGRUPPE", "V_VERTRAG_BENUTZER_DATENGRUPPE");
            resultList.Add("V_VERTRAG_BENUTZER_EXT_DATENGRUPPE", "V_VERTRAG_BENUTZER_EXT_DATENGRUPPE");
            resultList.Add("V_VERTRAG_BENUTZER_SICHTBARKEITSGRUPPE", "V_VERTRAG_BENUTZER_SICHTBARKEITSGRUPPE");
            resultList.Add("V_VERTRAG_BERICHT", "V_VERTRAG_BERICHT");
            resultList.Add("V_VERTRAG_DOMAINKEY_ALLGEMEIN", "V_VERTRAG_DOMAINKEY_ALLGEMEIN");
            resultList.Add("V_VERTRAG_DOMAINKEY_ALLGEMEIN_GELOESCHT", "V_VERTRAG_DOMAINKEY_ALLGEMEIN_GELOESCHT");
            resultList.Add("V_VERTRAG_EINKAUF_ZU_VERKAUF_EINZEL", "V_VERTRAG_EINKAUF_ZU_VERKAUF_EINZEL");
            resultList.Add("V_VERTRAG_EK_KUNDENSEGMENT", "V_VERTRAG_EK_KUNDENSEGMENT");
            resultList.Add("V_VERTRAG_GESCHAEFTSPARTEIGRUPPE_PREISSEGMENT", "V_VERTRAG_GESCHAEFTSPARTEIGRUPPE_PREISSEGMENT");
            resultList.Add("V_VERTRAG_GESCHAEFTSPARTEIGRUPPEN", "V_VERTRAG_GESCHAEFTSPARTEIGRUPPEN");
            resultList.Add("V_VERTRAG_ID_KOOP_RAHMEN_BEITRITT", "V_VERTRAG_ID_KOOP_RAHMEN_BEITRITT");
            resultList.Add("V_VERTRAG_KALKULATIONSVARIANTE", "V_VERTRAG_KALKULATIONSVARIANTE");
            resultList.Add("V_VERTRAG_KONDITION_VERTRAG", "V_VERTRAG_KONDITION_VERTRAG");
            resultList.Add("V_VERTRAG_KONTOGRUNDDATEN", "V_VERTRAG_KONTOGRUNDDATEN");
            resultList.Add("V_VERTRAG_KUENDIGUNG", "V_VERTRAG_KUENDIGUNG");
            resultList.Add("V_VERTRAG_KUENDIGUNG_AUTOTICKET_ANLAGE", "V_VERTRAG_KUENDIGUNG_AUTOTICKET_ANLAGE");
            resultList.Add("V_VERTRAG_KUENDIGUNG_AUTOTICKET_EIGENTUEMER", "V_VERTRAG_KUENDIGUNG_AUTOTICKET_EIGENTUEMER");
            resultList.Add("V_VERTRAG_KUENDIGUNGSBESTAETIGUNG_ERFASSEN", "V_VERTRAG_KUENDIGUNGSBESTAETIGUNG_ERFASSEN");
            resultList.Add("V_VERTRAG_KUENDIGUNGSWUNSCH_ERFASSEN", "V_VERTRAG_KUENDIGUNGSWUNSCH_ERFASSEN");
            resultList.Add("V_VERTRAG_LAUFZEITEN", "V_VERTRAG_LAUFZEITEN");
            resultList.Add("V_VERTRAG_LISTE", "V_VERTRAG_LISTE");
            resultList.Add("V_VERTRAG_LISTE_ANSPRECHPARTNER_ZUORDNEN", "V_VERTRAG_LISTE_ANSPRECHPARTNER_ZUORDNEN");
            resultList.Add("V_VERTRAG_LISTE_NEU", "V_VERTRAG_LISTE_NEU");
            resultList.Add("V_VERTRAG_LISTE_SUCHPOPUP_TECHNISCHES_GERAET", "V_VERTRAG_LISTE_SUCHPOPUP_TECHNISCHES_GERAET");
            resultList.Add("V_VERTRAG_LISTE_VERTRAGDOM", "V_VERTRAG_LISTE_VERTRAGDOM");
            resultList.Add("V_VERTRAG_NETZBETREIBER", "V_VERTRAG_NETZBETREIBER");
            resultList.Add("V_VERTRAG_OPOS", "V_VERTRAG_OPOS");
            resultList.Add("V_VERTRAG_PREISSEGMENT", "V_VERTRAG_PREISSEGMENT");
            resultList.Add("V_VERTRAG_STATUS", "V_VERTRAG_STATUS");
            resultList.Add("V_VERTRAG_STROMSTEUER", "V_VERTRAG_STROMSTEUER");
            resultList.Add("V_VERTRAG_SUCHE", "V_VERTRAG_SUCHE");
            resultList.Add("V_VERTRAG_SUCHE_WIZARD", "V_VERTRAG_SUCHE_WIZARD");
            resultList.Add("V_VERTRAG_VERBRAUCHSSTELLE_ADRESSE", "V_VERTRAG_VERBRAUCHSSTELLE_ADRESSE");
            resultList.Add("V_VERTRAG_VERTEILUNGSNETZBETREIBER_ADRESSE", "V_VERTRAG_VERTEILUNGSNETZBETREIBER_ADRESSE");
            resultList.Add("V_VERTRAG_VERTRAGSAUSWAHL", "V_VERTRAG_VERTRAGSAUSWAHL");
            resultList.Add("V_VERTRAG_VERTRAGSBEGINN_VERSCHIEBEN", "V_VERTRAG_VERTRAGSBEGINN_VERSCHIEBEN");
            resultList.Add("V_VERTRAG_VERTRAGSKONTO_NEU", "V_VERTRAG_VERTRAGSKONTO_NEU");
            resultList.Add("V_VERTRAG_VERTRAGSPARTNER", "V_VERTRAG_VERTRAGSPARTNER");
            resultList.Add("V_VERTRAG_ZEITSTRAHL", "V_VERTRAG_ZEITSTRAHL");
            resultList.Add("V_VERTRAG_ZU_NETZBETREIBER", "V_VERTRAG_ZU_NETZBETREIBER");
            resultList.Add("V_VERTRAG_ZU_NETZBETREIBER_KREDITOR", "V_VERTRAG_ZU_NETZBETREIBER_KREDITOR");
            resultList.Add("V_VERTRAG_ZU_NETZBETREIBER_RECHNUNGSSTELLER", "V_VERTRAG_ZU_NETZBETREIBER_RECHNUNGSSTELLER");
            resultList.Add("V_VERTRAG_ZU_RECHNUNGSSTELLER", "V_VERTRAG_ZU_RECHNUNGSSTELLER");
            resultList.Add("V_VERTRAG_ZU_RECHNUNGSSTELLER_KREDITOR", "V_VERTRAG_ZU_RECHNUNGSSTELLER_KREDITOR");
            resultList.Add("V_VERTRAGSKONTO_ANLAGE_VERBRAUCHSTELLE", "V_VERTRAGSKONTO_ANLAGE_VERBRAUCHSTELLE");
            resultList.Add("V_VERTRAGSKONTO_DOMAINKEY", "V_VERTRAGSKONTO_DOMAINKEY");
            resultList.Add("V_VERTRAGSKONTO_GP_BANK", "V_VERTRAGSKONTO_GP_BANK");
            resultList.Add("V_VERTRAGSKONTO_KONTIERUNGSOBJEKT", "V_VERTRAGSKONTO_KONTIERUNGSOBJEKT");
            resultList.Add("V_VERTRAGSKONTO_LISTE", "V_VERTRAGSKONTO_LISTE");
            resultList.Add("V_VERTRAGSKONTO_VERTRAG_LISTE", "V_VERTRAGSKONTO_VERTRAG_LISTE");
            resultList.Add("V_VERTRAGSKONTOROLLEN_DATENUMFELD", "V_VERTRAGSKONTOROLLEN_DATENUMFELD");
            resultList.Add("V_VERTRAGSLISTE_FREIGEGEBEN", "V_VERTRAGSLISTE_FREIGEGEBEN");
            resultList.Add("V_VERTRAGSPARTNER", "V_VERTRAGSPARTNER");
            resultList.Add("V_VERTRAGSPARTNER_ADRESSE", "V_VERTRAGSPARTNER_ADRESSE");
            resultList.Add("V_VERTRAGSPARTNER_ANSPRECHPARTNER", "V_VERTRAGSPARTNER_ANSPRECHPARTNER");
            resultList.Add("V_VNB_ANMELDEFORMULARIEN_AKTUELL", "V_VNB_ANMELDEFORMULARIEN_AKTUELL");
            resultList.Add("V_VNB_BILANZKREIS_SPARTE", "V_VNB_BILANZKREIS_SPARTE");
            resultList.Add("V_VNB_PLZ", "V_VNB_PLZ");
            resultList.Add("V_VNB_ZAEHLPUNKT_GESCHAEFTSBEZIEHUNGEN", "V_VNB_ZAEHLPUNKT_GESCHAEFTSBEZIEHUNGEN");
            resultList.Add("V_VNB_ZAEHLPUNKTZUORDNUNG", "V_VNB_ZAEHLPUNKTZUORDNUNG");
            resultList.Add("V_VNB_ZAEHLPUNKTZUORDNUNG_BILANZKREIS", "V_VNB_ZAEHLPUNKTZUORDNUNG_BILANZKREIS");
            resultList.Add("V_WEBDIENST_ABSCHLAEGE_ZU_PERIODE", "V_WEBDIENST_ABSCHLAEGE_ZU_PERIODE");
            resultList.Add("V_WETTERSTATION_PLZ", "V_WETTERSTATION_PLZ");
            resultList.Add("V_ZAEHLPUNKT_AKTUELL", "V_ZAEHLPUNKT_AKTUELL");
            resultList.Add("V_ZAEHLPUNKT_ANLAGE_VERTRAGSKONTO", "V_ZAEHLPUNKT_ANLAGE_VERTRAGSKONTO");
            resultList.Add("V_ZAEHLPUNKT_BEZEICHNUNG_HISTORIE", "V_ZAEHLPUNKT_BEZEICHNUNG_HISTORIE");
            resultList.Add("V_ZAEHLPUNKT_DOMAENE_LISTE", "V_ZAEHLPUNKT_DOMAENE_LISTE");
            resultList.Add("V_ZAEHLPUNKT_DOMAINKEY", "V_ZAEHLPUNKT_DOMAINKEY");
            resultList.Add("V_ZAEHLPUNKT_GESCHAEFTSBEZIEHUNG_GESCHAEFTSPARTEI", "V_ZAEHLPUNKT_GESCHAEFTSBEZIEHUNG_GESCHAEFTSPARTEI");
            resultList.Add("V_ZAEHLPUNKT_GUELTIGKEIT", "V_ZAEHLPUNKT_GUELTIGKEIT");
            resultList.Add("V_ZAEHLPUNKT_LISTE", "V_ZAEHLPUNKT_LISTE");
            resultList.Add("V_ZAEHLPUNKT_MANUELL_ZUORDNEN", "V_ZAEHLPUNKT_MANUELL_ZUORDNEN");
            resultList.Add("V_ZAEHLPUNKT_MIT_ANSCHLUSSOBJEKTADRESSE", "V_ZAEHLPUNKT_MIT_ANSCHLUSSOBJEKTADRESSE");
            resultList.Add("V_ZAEHLPUNKT_MIT_HISTORIE", "V_ZAEHLPUNKT_MIT_HISTORIE");
            resultList.Add("V_ZAEHLPUNKT_NUR_HISTORIE", "V_ZAEHLPUNKT_NUR_HISTORIE");
            resultList.Add("V_ZAEHLPUNKT_VNB_ENET", "V_ZAEHLPUNKT_VNB_ENET");
            resultList.Add("V_ZAEHLPUNKT_VNB_ENET_AKTUAL", "V_ZAEHLPUNKT_VNB_ENET_AKTUAL");
            resultList.Add("V_ZAEHLPUNKT_VNB_ENET_AKTUAL_GP", "V_ZAEHLPUNKT_VNB_ENET_AKTUAL_GP");
            resultList.Add("V_ZAEHLPUNKT_VNB_ERMITTELN", "V_ZAEHLPUNKT_VNB_ERMITTELN");
            resultList.Add("V_ZAEHLPUNKT_VNB_MIT_ADRESSE", "V_ZAEHLPUNKT_VNB_MIT_ADRESSE");
            resultList.Add("V_ZAEHLPUNKTE_NETZBETREIBER", "V_ZAEHLPUNKTE_NETZBETREIBER");
            resultList.Add("V_ZAEHLPUNKTE_OHNE_GESCHAEFTSBEZIEHUNG", "V_ZAEHLPUNKTE_OHNE_GESCHAEFTSBEZIEHUNG");
            resultList.Add("V_ZAEHLPUNKTE_STROM", "V_ZAEHLPUNKTE_STROM");
            resultList.Add("VT_ABSCHLAG_LAUF", "VT_ABSCHLAG_LAUF");
            resultList.Add("VT_ABSCHLAG_LAUF_DETAILS", "VT_ABSCHLAG_LAUF_DETAILS");
            resultList.Add("VT_ADS_ZEITSCHEIBEN", "VT_ADS_ZEITSCHEIBEN");
            resultList.Add("VT_ANFRAGE_ANLAGE", "VT_ANFRAGE_ANLAGE");
            resultList.Add("VT_ANFRAGE_BERECHNUNGSZEITPUNKT", "VT_ANFRAGE_BERECHNUNGSZEITPUNKT");
            resultList.Add("VT_ANFRAGE_JAHRESMENGE", "VT_ANFRAGE_JAHRESMENGE");
            resultList.Add("VT_ANGEBOT_ANLAGE", "VT_ANGEBOT_ANLAGE");
            resultList.Add("VT_ANGEBOT_IMPORT", "VT_ANGEBOT_IMPORT");
            resultList.Add("VT_ANGEBOT_JAHRESMENGE", "VT_ANGEBOT_JAHRESMENGE");
            resultList.Add("VT_ANGEBOT_ZUSATZ", "VT_ANGEBOT_ZUSATZ");
            resultList.Add("VT_BENCHMARK", "VT_BENCHMARK");
            resultList.Add("VT_BENCHMARK_VERTRAG", "VT_BENCHMARK_VERTRAG");
            resultList.Add("VT_BERICHT", "VT_BERICHT");
            resultList.Add("VT_GESCHAEFTSBEZIEHUNG", "VT_GESCHAEFTSBEZIEHUNG");
            resultList.Add("VT_GESCHAEFTSMODELL", "VT_GESCHAEFTSMODELL");
            resultList.Add("VT_JAHRESMENGE", "VT_JAHRESMENGE");
            resultList.Add("VT_KALKULATION_KOSTEN_EK", "VT_KALKULATION_KOSTEN_EK");
            resultList.Add("VT_KALKULATION_KOSTEN_VK", "VT_KALKULATION_KOSTEN_VK");
            resultList.Add("VT_KALKULATION_MONATSMENGE", "VT_KALKULATION_MONATSMENGE");
            resultList.Add("VT_KALKULATION_NUTZENERGIE", "VT_KALKULATION_NUTZENERGIE");
            resultList.Add("VT_KALKULATION_NUTZENERGIE_DETAILS", "VT_KALKULATION_NUTZENERGIE_DETAILS");
            resultList.Add("VT_KUENDIGUNG_BEZUGSZEITPUNKT", "VT_KUENDIGUNG_BEZUGSZEITPUNKT");
            resultList.Add("VT_KUNDENORGANISATION", "VT_KUNDENORGANISATION");
            resultList.Add("VT_KUNDENORGANISATION_FLAECHE", "VT_KUNDENORGANISATION_FLAECHE");
            resultList.Add("VT_KUNDENORGANISATION_VERTRAG", "VT_KUNDENORGANISATION_VERTRAG");
            resultList.Add("VT_KUNDENSTAMM", "VT_KUNDENSTAMM");
            resultList.Add("VT_MAHNVERFAHREN", "VT_MAHNVERFAHREN");
            resultList.Add("VT_MONATSMENGE", "VT_MONATSMENGE");
            resultList.Add("VT_NETZNUTZUNGSANTEIL", "VT_NETZNUTZUNGSANTEIL");
            resultList.Add("VT_NN_HEV_VERARBEITUNG", "VT_NN_HEV_VERARBEITUNG");
            resultList.Add("VT_NN_HEV_VERARBEITUNGSSTATUS", "VT_NN_HEV_VERARBEITUNGSSTATUS");
            resultList.Add("VT_OFFENE_POS_STATUS", "VT_OFFENE_POS_STATUS");
            resultList.Add("VT_OFFENE_POSITION", "VT_OFFENE_POSITION");
            resultList.Add("VT_PRODUKT", "VT_PRODUKT");
            resultList.Add("VT_PRODUKT_STATUS", "VT_PRODUKT_STATUS");
            resultList.Add("VT_PRODUKTART", "VT_PRODUKTART");
            resultList.Add("VT_ROLLE", "VT_ROLLE");
            resultList.Add("VT_SSE_VERFOLGUNG", "VT_SSE_VERFOLGUNG");
            resultList.Add("VT_STROMSTEUERERLAUBNISSCHEIN", "VT_STROMSTEUERERLAUBNISSCHEIN");
            resultList.Add("VT_VERTRAG", "VT_VERTRAG");
            resultList.Add("VT_VERTRAG_BERICHT", "VT_VERTRAG_BERICHT");
            resultList.Add("VT_VERTRAG_BUCHUNGSKREISSACHBEARBEITER", "VT_VERTRAG_BUCHUNGSKREISSACHBEARBEITER");
            resultList.Add("VT_VERTRAG_EK_KUNDENSEGMENT", "VT_VERTRAG_EK_KUNDENSEGMENT");
            resultList.Add("VT_VERTRAG_KALKULATIONSVARIANTE", "VT_VERTRAG_KALKULATIONSVARIANTE");
            resultList.Add("VT_VERTRAG_KALKULATIONSVARIANTE_DETAILS", "VT_VERTRAG_KALKULATIONSVARIANTE_DETAILS");
            resultList.Add("VT_VERTRAG_KONDITION", "VT_VERTRAG_KONDITION");
            resultList.Add("VT_VERTRAG_PREISSEGMENT", "VT_VERTRAG_PREISSEGMENT");
            resultList.Add("VT_VERTRAG_PRODUKT", "VT_VERTRAG_PRODUKT");
            resultList.Add("VT_VERTRAGSART", "VT_VERTRAGSART");
            resultList.Add("VT_VERTRAGSBEREICH", "VT_VERTRAGSBEREICH");
            resultList.Add("VT_VERTRAGSKATEGORIE", "VT_VERTRAGSKATEGORIE");
            resultList.Add("VT_VERTRAGSKONTO", "VT_VERTRAGSKONTO");
            resultList.Add("VT_VERTRAGSKONTOSTATUS", "VT_VERTRAGSKONTOSTATUS");
            resultList.Add("VT_VERTRAGSKONTOTYP", "VT_VERTRAGSKONTOTYP");
            resultList.Add("VT_VERTRAGSTYP", "VT_VERTRAGSTYP");
            resultList.Add("VT_VERTRIEBSLINIE", "VT_VERTRIEBSLINIE");
            resultList.Add("VT_VOLLMACHT", "VT_VOLLMACHT");
            resultList.Add("VT_ZAHLUNGSBEDINGUNG", "VT_ZAHLUNGSBEDINGUNG");
            resultList.Add("VT_ZAHLWEG", "VT_ZAHLWEG");
            return resultList;
        }

        #endregion
    }
}
