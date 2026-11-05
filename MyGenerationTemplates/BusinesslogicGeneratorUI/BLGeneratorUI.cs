using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using MyMeta;

namespace EHA.ZESA.Util.GeneratorUI
{
    /// <summary>
	/// Gui für die Auswahl der Tabellen / Views für welche Businesslogik generiert werden soll.
	/// </summary>
	/// <remarks>
	/// <p>Die Gui kann nur in das Programm MyGeneration eingebunden werden.</p>
	/// <p>Die Metadaten werden direkt aus der Datenbank und alternativ aus einer Powerdesigner-Datei
	/// gelesen. Die Metadaten aus der Datenbank haben Vorrang gegenüber den Metadaten des Powerdesigners.
	/// Jedoch wurden im Powerdesigner einige zusätzliche Metadaten definiert (z.B. ob eine Tabelle
	/// als ListOfValue dient und welche Spalten in diesem Fall gelesen werden sollen). Diese Daten können
	/// jedoch alternativ manuell erfasst werden.</p>
	/// <p>Die Einstellungen in diesem Dialog werden in einer XML-Datei gespeichert und werden bei
	/// wiederholtem Aufruf als Voreinstellung präsentiert.</p>
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
	///			<td width="20%">25.11.2004</td>
	///			<td width="60%">Erstellt</td>
	///		</tr>
	///		<tr>
	///			<td width="20%">Roland Jesse</td>
	///			<td width="20%">27.04.2005</td>
	///			<td width="60%">Parsen des Powerdesigner-PDM erweitert um NORMTIME-Tags zur Unterstützung der Normalisierung von Datumszeitwerten.</td>
	///		</tr>
	///		<tr>
	///			<td width="20%">Andreas Seibt</td>
	///			<td width="20%">09.06.2005</td>
	///			<td width="60%">Ausschließen KOMA-Tabellen und -Views</td>
	///		</tr>
	///		<tr>
	///			<td width="20%">Andreas Seibt</td>
	///			<td width="20%">30.06.2005</td>
	///			<td width="60%">Einschließen KOMA-Tabellen und -Views</td>
	///		</tr>
	///		<tr>
	///			<td width="20%">Andreas Seibt</td>
	///			<td width="20%">11.10.2005</td>
	///			<td width="60%">Optimizer-Hints aus Powerdesigner auslesen</td>
	///		</tr>
    /// ///		<tr>
    ///			<td width="20%">Thomas Berchelt</td>
    ///			<td width="20%">15.3.2012</td>
    ///			<td width="60%">Grundlegende Überarbeitung SQL-Server als Metadaten Repository.</td>
    ///		</tr>
	/// </table>
	/// </div>
	/// </remarks>
	public class BLGeneratorUI : Form
    {
        #region Members

        /// <summary>Der Listeneintrag bezieht sich auf eine Tabelle.</summary>
		public const string TYPE_TABLE = "Table";
        /// <summary>Der Listeneintrag bezieht sich auf eine View.</summary>
        public const string TYPE_VIEW = "View";
        /// <summary>Der Listeneintrag bezieht sich auf eine Function.</summary>
        public const string TYPE_FUNCTION = "Function";
        // Pfad zu TfsRepos
        public static string PathTfsRepos = string.Empty;

        // Der Name der Konfiguationsdatei
        //private const string FILENAME_METADATA = "BLGeneratorUIMeta.xml";
        private const string FILENAME_METADATA = @"C:\TFS64Repos\ZESA\Utilities\MyGenerationTemplates\BLGeneratorMetaData\BLGeneratorUIMeta.xml";

        private BLGeneratorSettings mBlGeneratorSettings;
        private bool mForceClose;

        // Die von MyGeneration eingelesenen Metadaten
        private dbRoot mMetaData;
        // Eine Liste mit den Metadaten für die ausgewählten Tabellen / Views
        private ArrayList mGenInfoData;
        // Das Ausgabeverzeichnis für die generierten Dateien
        private string mTargetPath;
        // die beim letzten Aufruf gesicherten Eingaben
        private MetaData mSavedLocalMetaData;
        // eine Connection für den SQL-Server
        private SqlConnection mSqlConnnectionZesaEntw;
        public static string SqlConnnectionStringZesaEntw;
        // alle Stammdatentabellen
        private List<string> mStammdatentabellen;
        // alle relevanten Tabellen und deren Abhängige Tabellen zur Stammdatenübernahme
        private readonly ArrayList mStammdatenWithForeignKeys = new ArrayList();
        // beinhaltet alle tabellen für die K E I N E Datenübernahme generiert werden darf
        private readonly ArrayList mKeineDatenuebernahmeGenerieren = new ArrayList();
        // alle vorhandenen TableVAluedParameter für schreibenden DB-Zugriff
        private readonly List<string> mExistingTableValuedParameterForTables = new List<string>();
        private bool mNeedPruefeTvpGeneration;
        // alle Informationen die für die Genereirung von Funktionskonfigurationen benötigt werden
        // Liste von FunctionGenInfos
        private readonly ArrayList mFunctionGenInfos = new ArrayList();

        private Button BtnOK;
        private Panel PnlButtons;
        private Panel PnlInput;
        private Label label1;
        private TextBox TxtPath;
        private Button BtnSetPath;
        private FolderBrowserDialog DlgPath;
        private DataGrid grdData;
        private DataGridTableStyle DefaultTableStyle;
        private MetaData mMetaDataFromDatabase;
        private DataGridTextBoxColumn colTableName;
        private DataGridTextBoxColumn colNamespace;
        private DataGridBoolColumn colGenerate;
        private DataGridBoolColumn colLOV;
        private Button BtnSelectAll;
        private Button BtnDeselectAll;
        private Button BtnCancel;
        private DataGridTextBoxColumn colLovColumns;
        private Label label2;
        private OpenFileDialog dlgOpen;
        private DataGridTextBoxColumn colNormTimeFromColumns;
        private DataGridTextBoxColumn colNormTimeToColumns;
        private DataGridBoolColumn colStammdatenuebernahme;
        private StatusStrip sbStatus;
        private ToolStripStatusLabel sbStatusLabel;
        private DataGridTextBoxColumn colAliasKennungColumn;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private DataGridTextBoxColumn colDbReaderDatatypeColumns;
        private DataGridTextBoxColumn colDbReaderOperatorColumns;
        private Label lblTfsWorkspace;
        private TextBox txtTfsWorkspace;
        private Button btnLockTfsField;

        #endregion Members
        private ImageList imlLockButton;
        private CheckBox chkStammdatenuebernahmeIgnorieren;
        private System.ComponentModel.IContainer components;


        #region Constructor / Destructor

        /// <summary>
        /// Standard-Konstruktor
        /// </summary>
        public BLGeneratorUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Dieser Konstruktor wird von MyGeneration aufgerufen.
        /// </summary>
        /// <param name="pMeta">Das Objekt mit den Metadaten des Datenbankservers</param>
        public BLGeneratorUI(dbRoot pMeta)
        {
            InitializeComponent();
            mMetaData = pMeta;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion Constructor / Destructor


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BLGeneratorUI));
            this.BtnOK = new System.Windows.Forms.Button();
            this.PnlButtons = new System.Windows.Forms.Panel();
            this.sbStatus = new System.Windows.Forms.StatusStrip();
            this.sbStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnDeselectAll = new System.Windows.Forms.Button();
            this.BtnSelectAll = new System.Windows.Forms.Button();
            this.PnlInput = new System.Windows.Forms.Panel();
            this.btnLockTfsField = new System.Windows.Forms.Button();
            this.imlLockButton = new System.Windows.Forms.ImageList(this.components);
            this.lblTfsWorkspace = new System.Windows.Forms.Label();
            this.txtTfsWorkspace = new System.Windows.Forms.TextBox();
            this.chkStammdatenuebernahmeIgnorieren = new System.Windows.Forms.CheckBox();
            this.BtnSetPath = new System.Windows.Forms.Button();
            this.TxtPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.DlgPath = new System.Windows.Forms.FolderBrowserDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
            this.grdData = new System.Windows.Forms.DataGrid();
            this.mMetaDataFromDatabase = new EHA.ZESA.Util.GeneratorUI.MetaData();
            this.DefaultTableStyle = new System.Windows.Forms.DataGridTableStyle();
            this.colGenerate = new System.Windows.Forms.DataGridBoolColumn();
            this.colLOV = new System.Windows.Forms.DataGridBoolColumn();
            this.colTableName = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colNamespace = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colLovColumns = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colNormTimeFromColumns = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colNormTimeToColumns = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colStammdatenuebernahme = new System.Windows.Forms.DataGridBoolColumn();
            this.colAliasKennungColumn = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colDbReaderDatatypeColumns = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colDbReaderOperatorColumns = new System.Windows.Forms.DataGridTextBoxColumn();
            this.PnlButtons.SuspendLayout();
            this.sbStatus.SuspendLayout();
            this.PnlInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mMetaDataFromDatabase)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnOK
            // 
            this.BtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOK.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnOK.Location = new System.Drawing.Point(1166, 5);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(125, 23);
            this.BtnOK.TabIndex = 2;
            this.BtnOK.Text = "&Generieren";
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // PnlButtons
            // 
            this.PnlButtons.Controls.Add(this.sbStatus);
            this.PnlButtons.Controls.Add(this.BtnCancel);
            this.PnlButtons.Controls.Add(this.BtnDeselectAll);
            this.PnlButtons.Controls.Add(this.BtnSelectAll);
            this.PnlButtons.Controls.Add(this.BtnOK);
            this.PnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PnlButtons.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PnlButtons.Location = new System.Drawing.Point(0, 1002);
            this.PnlButtons.Name = "PnlButtons";
            this.PnlButtons.Size = new System.Drawing.Size(1430, 52);
            this.PnlButtons.TabIndex = 2;
            // 
            // sbStatus
            // 
            this.sbStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sbStatusLabel,
            this.toolStripStatusLabel1});
            this.sbStatus.Location = new System.Drawing.Point(0, 30);
            this.sbStatus.Name = "sbStatus";
            this.sbStatus.Size = new System.Drawing.Size(1430, 22);
            this.sbStatus.TabIndex = 4;
            this.sbStatus.Text = "statusStrip1";
            // 
            // sbStatusLabel
            // 
            this.sbStatusLabel.Name = "sbStatusLabel";
            this.sbStatusLabel.Size = new System.Drawing.Size(0, 17);
            this.sbStatusLabel.DoubleClick += new System.EventHandler(this.sbStatusLabel_DoubleClick);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnCancel.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnCancel.Location = new System.Drawing.Point(1301, 5);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(125, 23);
            this.BtnCancel.TabIndex = 3;
            this.BtnCancel.Text = "&Abbrechen";
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnDeselectAll
            // 
            this.BtnDeselectAll.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnDeselectAll.Location = new System.Drawing.Point(140, 5);
            this.BtnDeselectAll.Name = "BtnDeselectAll";
            this.BtnDeselectAll.Size = new System.Drawing.Size(125, 23);
            this.BtnDeselectAll.TabIndex = 1;
            this.BtnDeselectAll.Text = "Auswahl auf&heben";
            this.BtnDeselectAll.Click += new System.EventHandler(this.BtnDeselectAll_Click);
            // 
            // BtnSelectAll
            // 
            this.BtnSelectAll.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnSelectAll.Location = new System.Drawing.Point(5, 4);
            this.BtnSelectAll.Name = "BtnSelectAll";
            this.BtnSelectAll.Size = new System.Drawing.Size(125, 23);
            this.BtnSelectAll.TabIndex = 0;
            this.BtnSelectAll.Text = "&Alle auswählen";
            this.BtnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
            // 
            // PnlInput
            // 
            this.PnlInput.Controls.Add(this.btnLockTfsField);
            this.PnlInput.Controls.Add(this.lblTfsWorkspace);
            this.PnlInput.Controls.Add(this.txtTfsWorkspace);
            this.PnlInput.Controls.Add(this.chkStammdatenuebernahmeIgnorieren);
            this.PnlInput.Controls.Add(this.BtnSetPath);
            this.PnlInput.Controls.Add(this.TxtPath);
            this.PnlInput.Controls.Add(this.label1);
            this.PnlInput.Dock = System.Windows.Forms.DockStyle.Top;
            this.PnlInput.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PnlInput.Location = new System.Drawing.Point(0, 0);
            this.PnlInput.Name = "PnlInput";
            this.PnlInput.Size = new System.Drawing.Size(1430, 60);
            this.PnlInput.TabIndex = 0;
            // 
            // btnLockTfsField
            // 
            this.btnLockTfsField.ImageKey = "Lock.ico";
            this.btnLockTfsField.ImageList = this.imlLockButton;
            this.btnLockTfsField.Location = new System.Drawing.Point(1100, 20);
            this.btnLockTfsField.Name = "btnLockTfsField";
            this.btnLockTfsField.Size = new System.Drawing.Size(21, 21);
            this.btnLockTfsField.TabIndex = 7;
            this.btnLockTfsField.UseVisualStyleBackColor = true;
            this.btnLockTfsField.Click += new System.EventHandler(this.btnLockTfsField_Click);
            // 
            // imlLockButton
            // 
            this.imlLockButton.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlLockButton.ImageStream")));
            this.imlLockButton.TransparentColor = System.Drawing.Color.Transparent;
            this.imlLockButton.Images.SetKeyName(0, "Lock.ico");
            this.imlLockButton.Images.SetKeyName(1, "Unlock.ico");
            // 
            // lblTfsWorkspace
            // 
            this.lblTfsWorkspace.AutoSize = true;
            this.lblTfsWorkspace.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTfsWorkspace.Location = new System.Drawing.Point(830, 25);
            this.lblTfsWorkspace.Name = "lblTfsWorkspace";
            this.lblTfsWorkspace.Size = new System.Drawing.Size(98, 15);
            this.lblTfsWorkspace.TabIndex = 3;
            this.lblTfsWorkspace.Text = "Tfs-Workspace:";
            this.lblTfsWorkspace.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtTfsWorkspace
            // 
            this.txtTfsWorkspace.Location = new System.Drawing.Point(930, 20);
            this.txtTfsWorkspace.Name = "txtTfsWorkspace";
            this.txtTfsWorkspace.ReadOnly = true;
            this.txtTfsWorkspace.Size = new System.Drawing.Size(170, 21);
            this.txtTfsWorkspace.TabIndex = 4;
            // 
            // chkStammdatenuebernahmeIgnorieren
            // 
            this.chkStammdatenuebernahmeIgnorieren.AutoSize = true;
            this.chkStammdatenuebernahmeIgnorieren.Checked = true;
            this.chkStammdatenuebernahmeIgnorieren.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStammdatenuebernahmeIgnorieren.ForeColor = System.Drawing.Color.Red;
            this.chkStammdatenuebernahmeIgnorieren.Location = new System.Drawing.Point(1155, 22);
            this.chkStammdatenuebernahmeIgnorieren.Name = "chkStammdatenuebernahmeIgnorieren";
            this.chkStammdatenuebernahmeIgnorieren.Size = new System.Drawing.Size(263, 19);
            this.chkStammdatenuebernahmeIgnorieren.TabIndex = 6;
            this.chkStammdatenuebernahmeIgnorieren.Text = "KEINE Stammdatenübernahme generieren";
            this.chkStammdatenuebernahmeIgnorieren.UseVisualStyleBackColor = true;
            // 
            // BtnSetPath
            // 
            this.BtnSetPath.Location = new System.Drawing.Point(790, 20);
            this.BtnSetPath.Name = "BtnSetPath";
            this.BtnSetPath.Size = new System.Drawing.Size(21, 21);
            this.BtnSetPath.TabIndex = 2;
            this.BtnSetPath.Text = "...";
            this.BtnSetPath.Click += new System.EventHandler(this.BtnSetPath_Click);
            // 
            // TxtPath
            // 
            this.TxtPath.Location = new System.Drawing.Point(105, 20);
            this.TxtPath.Name = "TxtPath";
            this.TxtPath.Size = new System.Drawing.Size(685, 21);
            this.TxtPath.TabIndex = 1;
            this.TxtPath.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TxtPath_KeyUp);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Zielverzeichnis:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(5, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1420, 56);
            this.label2.TabIndex = 3;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // dlgOpen
            // 
            this.dlgOpen.Filter = "Power-Designer Files|*.pdm|All Files|*.*";
            this.dlgOpen.RestoreDirectory = true;
            this.dlgOpen.Title = "PDM-Datei auswählen";
            // 
            // grdData
            // 
            this.grdData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdData.CaptionVisible = false;
            this.grdData.DataMember = "MetaData";
            this.grdData.DataSource = this.mMetaDataFromDatabase;
            this.grdData.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.grdData.Location = new System.Drawing.Point(5, 125);
            this.grdData.Name = "grdData";
            this.grdData.Size = new System.Drawing.Size(1421, 877);
            this.grdData.TabIndex = 1;
            this.grdData.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
            this.DefaultTableStyle});
            this.grdData.CurrentCellChanged += new System.EventHandler(this.grdData_CurrentCellChanged);
            // 
            // mMetaDataFromDatabase
            // 
            this.mMetaDataFromDatabase.DataSetName = "MetaData";
            this.mMetaDataFromDatabase.Locale = new System.Globalization.CultureInfo("de-DE");
            this.mMetaDataFromDatabase.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // DefaultTableStyle
            // 
            this.DefaultTableStyle.DataGrid = this.grdData;
            this.DefaultTableStyle.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
            this.colGenerate,
            this.colLOV,
            this.colTableName,
            this.colNamespace,
            this.colLovColumns,
            this.colNormTimeFromColumns,
            this.colNormTimeToColumns,
            this.colStammdatenuebernahme,
            this.colAliasKennungColumn,
            this.colDbReaderDatatypeColumns,
            this.colDbReaderOperatorColumns});
            this.DefaultTableStyle.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefaultTableStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.DefaultTableStyle.MappingName = "MetaData";
            // 
            // colGenerate
            // 
            this.colGenerate.AllowNull = false;
            this.colGenerate.HeaderText = "Generieren?";
            this.colGenerate.MappingName = "Generate";
            this.colGenerate.NullValue = "False";
            this.colGenerate.Width = 75;
            // 
            // colLOV
            // 
            this.colLOV.AllowNull = false;
            this.colLOV.HeaderText = "ListOfValue?";
            this.colLOV.MappingName = "LOV";
            this.colLOV.NullValue = "False";
            this.colLOV.Width = 75;
            // 
            // colTableName
            // 
            this.colTableName.Format = "";
            this.colTableName.FormatInfo = null;
            this.colTableName.HeaderText = "Tabelle / View";
            this.colTableName.MappingName = "ObjectName";
            this.colTableName.ReadOnly = true;
            this.colTableName.Width = 180;
            // 
            // colNamespace
            // 
            this.colNamespace.Format = "";
            this.colNamespace.FormatInfo = null;
            this.colNamespace.HeaderText = "Namespace";
            this.colNamespace.MappingName = "Namespace";
            this.colNamespace.Width = 180;
            // 
            // colLovColumns
            // 
            this.colLovColumns.Format = "";
            this.colLovColumns.FormatInfo = null;
            this.colLovColumns.HeaderText = "Lov-Spalten";
            this.colLovColumns.MappingName = "LovColumns";
            this.colLovColumns.Width = 350;
            // 
            // colNormTimeFromColumns
            // 
            this.colNormTimeFromColumns.Format = "";
            this.colNormTimeFromColumns.FormatInfo = null;
            this.colNormTimeFromColumns.HeaderText = "Normal. Von-Spalten";
            this.colNormTimeFromColumns.MappingName = "NormTimeFromColumns";
            this.colNormTimeFromColumns.Width = 75;
            // 
            // colNormTimeToColumns
            // 
            this.colNormTimeToColumns.Format = "";
            this.colNormTimeToColumns.FormatInfo = null;
            this.colNormTimeToColumns.HeaderText = "Normal. Bis-Spalten";
            this.colNormTimeToColumns.MappingName = "NormTimeToColumns";
            this.colNormTimeToColumns.Width = 75;
            // 
            // colStammdatenuebernahme
            // 
            this.colStammdatenuebernahme.AllowNull = false;
            this.colStammdatenuebernahme.HeaderText = "Stammdatenübernahme?";
            this.colStammdatenuebernahme.MappingName = "Stammdatenuebernahme";
            this.colStammdatenuebernahme.NullValue = "False";
            this.colStammdatenuebernahme.Width = 75;
            // 
            // colAliasKennungColumn
            // 
            this.colAliasKennungColumn.Format = "";
            this.colAliasKennungColumn.FormatInfo = null;
            this.colAliasKennungColumn.HeaderText = "Aliasspalte Kennung (für Stammdatenübernahme)";
            this.colAliasKennungColumn.MappingName = "AliasKennungColumn";
            this.colAliasKennungColumn.Width = 270;
            // 
            // colDbReaderDatatypeColumns
            // 
            this.colDbReaderDatatypeColumns.Format = "";
            this.colDbReaderDatatypeColumns.FormatInfo = null;
            this.colDbReaderDatatypeColumns.HeaderText = "DbReader Spalten übersteuernder Datentyp";
            this.colDbReaderDatatypeColumns.MappingName = "DbReaderDataTypeColumns";
            this.colDbReaderDatatypeColumns.Width = 350;
            // 
            // colDbReaderOperatorColumns
            // 
            this.colDbReaderOperatorColumns.Format = "";
            this.colDbReaderOperatorColumns.FormatInfo = null;
            this.colDbReaderOperatorColumns.HeaderText = "DbReader Spalten übersteuernder Operator";
            this.colDbReaderOperatorColumns.MappingName = "DbReaderOperatorColumns";
            this.colDbReaderOperatorColumns.Width = 350;
            // 
            // BLGeneratorUI
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1430, 1054);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.grdData);
            this.Controls.Add(this.PnlInput);
            this.Controls.Add(this.PnlButtons);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BLGeneratorUI";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ZESA Businesslogik-Generator Version 2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BLGeneratorUI_FormClosing);
            this.Load += new System.EventHandler(this.ViewGeneratorUI_Load);
            this.PnlButtons.ResumeLayout(false);
            this.PnlButtons.PerformLayout();
            this.sbStatus.ResumeLayout(false);
            this.sbStatus.PerformLayout();
            this.PnlInput.ResumeLayout(false);
            this.PnlInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mMetaDataFromDatabase)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion


        #region Properties

        /// <summary>
        /// Eine Liste mit den Metadaten aller ausgewählten Tabellen / Views.
        /// </summary>
        public ArrayList Data
        {
            get { return mGenInfoData; }
            set { mGenInfoData = value; }
        }

        /// <summary>
        /// die Infos zum Generieren von Funktionsaufrufen
        /// </summary>
        public ArrayList FunctionGenInfoData
        {
            get { return mFunctionGenInfos; }
        }

        /// <summary>
        /// Das Verzeichnis, in dem der generierte Code abgelegt wird.
        /// </summary>
        public string TargetPath
        {
            get { return mTargetPath; }
            set { mTargetPath = value; }
        }

        /// <summary>
        /// gibt eine Liste mit allen relevanten Informationen
        /// zur Stammdatenübernahme zurück. Dort entahlen sind KeyValuePairs
        /// Key: Tabellenname als string
        /// Value: Foreign Keys der Tabelle als eine Liste von strings 
        /// </summary>
        public ArrayList StammdatenWithForeignKeys
        {
            get { return mStammdatenWithForeignKeys; }
        }

        /// <summary>
        /// gibt eine Liste mit auf der Datenbank vorhandenen existierenden TableValuedParametern (TVP)
        /// (erkennbar am Präfix TT_) zurück (Tabellennamen), damit das Generierungsskript diese Liste mit den aktuell zu 
        /// generierenden abgleicht. Dies schützt davor, dass plötzlich vorhandene TVP bei der Prüfung ausgelassen werden,
        /// da der Code für die Prüfung der TVP immer komplett und nihct inkrementell genereirt wird.
        /// </summary>
        public ArrayList ExistingTableValuedParameterForTables
        {
            get { return mNeedPruefeTvpGeneration ? new ArrayList(mExistingTableValuedParameterForTables) : null; }
        }

        /// <summary>
        /// Die von MyGeneration eingelesenen Metadaten
        /// </summary>
        public dbRoot Meta
        {
            set { mMetaData = value; }
        }

        #endregion Properties


        #region Initialization

        private void InitBLGenerator()
        {
            // wir holen uns die Settings
            mBlGeneratorSettings = GeneratorHelper.DeserializeSettings();
            mKeineDatenuebernahmeGenerieren.Clear();
            // setzen den Workspace
            txtTfsWorkspace.Text = string.IsNullOrEmpty(mBlGeneratorSettings.AbweichenderTfsWorkspace)
                                       ? Environment.MachineName
                                       : mBlGeneratorSettings.AbweichenderTfsWorkspace;

            // wir bauen die SQL Connections auf
            string[] conStringArray = mMetaData.ConnectionString.Split(';');
            string conString = string.Empty;
            foreach (string s in conStringArray)
            {
                // den Provider benötigen wir nicht
                if (!s.ToLower().Contains("provider"))
                {
                    if (!string.IsNullOrEmpty(conString))
                    {
                        conString += ";";
                    }
                    conString = conString + s;
                }
            }
            // Verbinden uns gegen die ENTW-Datenbank
            mSqlConnnectionZesaEntw = new SqlConnection(conString);
            SqlConnnectionStringZesaEntw = conString;

            // Gui an Bildschirm anpassen
            Height = Screen.PrimaryScreen.Bounds.Height > 50
                ? Screen.PrimaryScreen.Bounds.Height - 50
                : Screen.PrimaryScreen.Bounds.Height;
        }

        /// <summary>
        /// Das Event wird beim Laden des Formulars ausgelöst.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Die Metadaten der Datenbank werden in  den Anzeigebereich geladen. Danach
        /// werden die in der letzten Sitzung gespeicherten Daten in die aktuell
        /// gelesenen Daten gemerged.
        /// </remarks>
        private void ViewGeneratorUI_Load(object sender, EventArgs e)
        {
            // zuerst Lokale Metadaten laden
            if (mMetaData.DefaultDatabase != null)
            {
                Cursor = Cursors.WaitCursor;
                InitBLGenerator();
                FillDataSet();
                LoadSavedLocalMetadata();
                MergeData();
                GetStammdatenTabellen();

                List<string> objToGen = GeneratorHelper.GetObjectsToGenerate();
                if (objToGen.Any())
                {
                    BtnDeselectAll_Click(this, new EventArgs());

                    foreach (string dataObject in objToGen)
                    {
                        foreach (MetaData.MetaDataRow row in mMetaDataFromDatabase.MetaDataRows)
                        {
                            if (row.ObjectName.ToUpper().Equals(dataObject.ToUpper()))
                            {
                                row.BeginEdit();
                                row.Generate = true;
                                row.EndEdit();
                            }
                        }
                    }
                    // Änderungen persistieren
                    mMetaDataFromDatabase.AcceptChanges();
                }

                // erst danach: Version prüfen!
                CheckVersion();
                Cursor = Cursors.Default;
            }
        }

        #endregion Initialization


        #region Load Metadata

        /// <summary>
        /// Füllt die Metadaten in ein internes DataSet.
        /// </summary>
        /// <remarks>
        /// Für die interne Repräsentation der Daten wurde ein DataSet
        /// aus folgenden Gründen gewählt: Ein DataSet kann einfach als
        /// XML-Datei abgespeichert werden und die Unterschiede zwischen
        /// Views und Tabellen können ausgeglichen werden.
        /// </remarks>
        private void FillDataSet()
        {
            try
            {
                // wir öffnen die connection (IMMER gegen ENTW)
                if (mSqlConnnectionZesaEntw.State != ConnectionState.Open)
                    mSqlConnnectionZesaEntw.Open();

                // hole alle TybleValuedParameters
                DataTable structuredTypeMembersNachTypeNameOrdinalPosition = mSqlConnnectionZesaEntw.GetSchema("StructuredTypeMembers");

                List<string> alleTvps = (from row in structuredTypeMembersNachTypeNameOrdinalPosition.AsEnumerable()
                                         where row["TYPE_NAME"].ToString().ToUpper().StartsWith("TT_")
                                         select row["TYPE_NAME"].ToString().ToUpper().Substring(3)).Distinct().ToList();

                // Ergebnis der ArrayList hinzufügen
                if (alleTvps.Any())
                {
                    // existierende Liste sicherheitshalber löschen
                    mExistingTableValuedParameterForTables.Clear();

                    // wenn der Namespace aufgelöst werden kann den TVP zur Liste hinzufügen
                    foreach (string tvp in alleTvps)
                    {
                        if (!string.IsNullOrEmpty(GeneratorHelper.FindNamespace(tvp)))
                            mExistingTableValuedParameterForTables.Add(tvp);
                    }
                }


                // Alle Datenzugriffsfunktionen aus ZESA auslesen
                SqlCommand cmd = new SqlCommand("SELECT name FROM sys.objects WHERE type_desc like '%function%' and name like 'f_sel%'", mSqlConnnectionZesaEntw);
                List<string> existingFunctionsRedesignedDataAccess = new List<string>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string functionName = reader["name"].ToString();
                        if (!existingFunctionsRedesignedDataAccess.Contains(functionName))
                            existingFunctionsRedesignedDataAccess.Add(functionName);
                    }
                }

                // behandeln alle Tabellen
                foreach (ITable aTable in mMetaData.DefaultDatabase.Tables)
                {
                    string aNamespace = GeneratorHelper.FindNamespace(aTable.Name);
                    if (aNamespace != string.Empty)
                    {
                        bool oIsPkInteger;

                        MetaData.MetaDataRow aRow = mMetaDataFromDatabase.MetaDataRows.NewMetaDataRow();
                        aRow.ObjectName = aTable.Name;
                        aRow.Namespace = aNamespace;
                        aRow.LOV = false;
                        aRow.NORMTIMEFROM = false;
                        aRow.NORMTIMETO = false;
                        aRow.Generate = true;
                        aRow.Type = TYPE_TABLE;
                        aRow.EntityName = GeneratorHelper.BuildEntityName(aTable.Name, aRow.Type);
                        aRow.LovColumns = string.Empty;
                        aRow.NormTimeFromColumns = string.Empty;
                        aRow.NormTimeToColumns = string.Empty;
                        aRow.AliasKennungColumn = string.Empty;
                        aRow.Stammdatenuebernahme = false;
                        aRow.DbReaderDataTypeColumns = string.Empty;
                        aRow.DbReaderOperatorColumns = string.Empty;
                        aRow.PrimaryKeyIntegerColumnNames = String.Join(",", GetIntegerPrimaryAndForeignKeyColumnNames(aTable, out oIsPkInteger).ToArray());
                        aRow.IsPrimaryKeyInteger = oIsPkInteger;
                        mMetaDataFromDatabase.MetaDataRows.Rows.Add(aRow);
                    }
                }

                // behandeln alle Views
                foreach (IView aView in mMetaData.DefaultDatabase.Views)
                {
                    MetaData.MetaDataRow aRow = mMetaDataFromDatabase.MetaDataRows.NewMetaDataRow();
                    aRow.ObjectName = aView.Name;
                    aRow.Namespace = string.Empty;
                    aRow.LOV = false;
                    aRow.NORMTIMEFROM = false;
                    aRow.NORMTIMETO = false;
                    aRow.Generate = true;
                    aRow.Type = TYPE_VIEW;
                    aRow.EntityName = GeneratorHelper.BuildEntityName(aView.Name, aRow.Type);
                    aRow.LovColumns = string.Empty;
                    aRow.NormTimeFromColumns = string.Empty;
                    aRow.NormTimeToColumns = string.Empty;
                    aRow.AliasKennungColumn = string.Empty;
                    aRow.Stammdatenuebernahme = false;
                    aRow.DbReaderDataTypeColumns = string.Empty;
                    aRow.DbReaderOperatorColumns = string.Empty;
                    aRow.PrimaryKeyIntegerColumnNames = String.Join(",", EliminateAliasesForPrimaryKeyColumns(GetIntegerPrimaryAndForeignKeyColumnNames(aView.SubTables), aView));
                    aRow.IsPrimaryKeyInteger = false;
                    mMetaDataFromDatabase.MetaDataRows.Rows.Add(aRow);
                }

                // behandeln alle Funktionen
                foreach (IProcedure aProcedure in mMetaData.DefaultDatabase.Procedures)
                {
                    if (existingFunctionsRedesignedDataAccess.Contains(aProcedure.Name))
                    {
                        MetaData.MetaDataRow aRow = mMetaDataFromDatabase.MetaDataRows.NewMetaDataRow();
                        aRow.ObjectName = aProcedure.Name;
                        aRow.Namespace = GeneratorHelper.FindNamespace(aProcedure.Name);
                        aRow.LOV = false;
                        aRow.NORMTIMEFROM = false;
                        aRow.NORMTIMETO = false;
                        aRow.Generate = true;
                        aRow.Type = TYPE_FUNCTION;
                        aRow.EntityName = GeneratorHelper.BuildEntityName(aProcedure.Name, aRow.Type);
                        aRow.LovColumns = string.Empty;
                        aRow.NormTimeFromColumns = string.Empty;
                        aRow.NormTimeToColumns = string.Empty;
                        aRow.AliasKennungColumn = string.Empty;
                        aRow.Stammdatenuebernahme = false;
                        aRow.DbReaderDataTypeColumns = string.Empty;
                        aRow.DbReaderOperatorColumns = string.Empty;
                        aRow.PrimaryKeyIntegerColumnNames = string.Empty;
                        aRow.IsPrimaryKeyInteger = false;
                        mMetaDataFromDatabase.MetaDataRows.Rows.Add(aRow);
                    }
                }
            }
            finally
            {
                // zu guter letzt die Connection wieder zumachen
                if (mSqlConnnectionZesaEntw.State != ConnectionState.Closed)
                    mSqlConnnectionZesaEntw.Close();
            }
        }

        /// <summary>
        /// Führt die lokal zwischengespeicherten Daten und die Metadaten aus dem SQL Server zusammen
        /// </summary>
        private void MergeData()
        {
            // Zuerst manuelle Änderungen mergen, die haben Prio1.
            if (null != mSavedLocalMetaData)
            {
                foreach (MetaData.MetaDataRow savedRow in mSavedLocalMetaData.MetaDataRows)
                {
                    foreach (MetaData.MetaDataRow metaDataRowFromDataBase in mMetaDataFromDatabase.MetaDataRows)
                    {
                        if (savedRow.ObjectName == metaDataRowFromDataBase.ObjectName)
                        {
                            metaDataRowFromDataBase.BeginEdit();
                            metaDataRowFromDataBase.Generate = savedRow.Generate;
                            metaDataRowFromDataBase.Type = savedRow.Type;

                            // SQL Server hat Vorrang
                            // wir füllen die LOV Info nur, wenn sie noch nicht vom SQL-Server gekommen ist
                            if (!Convert.ToBoolean(metaDataRowFromDataBase.LOV) && string.IsNullOrEmpty(metaDataRowFromDataBase.LovColumns))
                            {
                                metaDataRowFromDataBase.LOV = savedRow.LOV;
                                metaDataRowFromDataBase.LovColumns = savedRow.LovColumns;
                            }
                            // wir füllen die NORMTIMEFROM Info nur, wenn sie noch nicht vom SQL-Server gekommen ist
                            if (!Convert.ToBoolean(metaDataRowFromDataBase.NORMTIMEFROM) && string.IsNullOrEmpty(metaDataRowFromDataBase.NormTimeFromColumns))
                            {
                                metaDataRowFromDataBase.NORMTIMEFROM = savedRow.NORMTIMEFROM;
                                metaDataRowFromDataBase.NormTimeFromColumns = savedRow.NormTimeFromColumns;
                            }
                            // wir füllen die NORMTIMETO Info nur, wenn sie noch nicht vom SQL-Server gekommen ist
                            if (!Convert.ToBoolean(metaDataRowFromDataBase.NORMTIMETO) && string.IsNullOrEmpty(metaDataRowFromDataBase.NormTimeToColumns))
                            {
                                metaDataRowFromDataBase.NORMTIMETO = savedRow.NORMTIMETO;
                                metaDataRowFromDataBase.NormTimeToColumns = savedRow.NormTimeToColumns;
                            }
                            // wir füllen die STAMMDATENUEBERNAHME Info nur, wenn sie noch nicht vom SQL-Server gekommen ist
                            if (!Convert.ToBoolean(metaDataRowFromDataBase.Stammdatenuebernahme))
                            {
                                metaDataRowFromDataBase.Stammdatenuebernahme = savedRow.Stammdatenuebernahme;
                            }
                            // wir füllen die ALIASKENNUNG Info nur, wenn sie noch nicht vom SQL-Server gekommen ist
                            if (string.IsNullOrEmpty(metaDataRowFromDataBase.AliasKennungColumn))
                            {
                                metaDataRowFromDataBase.AliasKennungColumn = savedRow.AliasKennungColumn;
                            }
                            // wir füllen den DbReaderDataType info nur, wenn sie noch nicht vom SQL-Server gekommen ist
                            if (string.IsNullOrEmpty(metaDataRowFromDataBase.DbReaderDataTypeColumns))
                            {
                                metaDataRowFromDataBase.DbReaderDataTypeColumns = savedRow.DbReaderDataTypeColumns;
                            }
                            // wir füllen den DbReaderOperator info nur, wenn sie noch nicht vom SQL-Server gekommen ist
                            if (string.IsNullOrEmpty(metaDataRowFromDataBase.DbReaderOperatorColumns))
                            {
                                metaDataRowFromDataBase.DbReaderOperatorColumns = savedRow.DbReaderOperatorColumns;
                            }
                            // wir füllen den Namespace nur, wenn er noch nicht vom SQL-Server gekommen ist
                            if (string.IsNullOrEmpty(metaDataRowFromDataBase.Namespace))
                            {
                                metaDataRowFromDataBase.Namespace = savedRow.Namespace;
                            }

                            // EndEdit
                            metaDataRowFromDataBase.EndEdit();
                            break;
                        }
                    }
                }
                mMetaDataFromDatabase.AcceptChanges();
            }
            // Änderungen am DataSet druchführen
            mMetaDataFromDatabase.AcceptChanges();
        }

        /// <summary>
        /// Liest die gespeicherten Daten und merged sie mit den aktuell gelesenen.
        /// </summary>
        private void LoadSavedLocalMetadata()
        {
            try
            {
                FileInfo fi = new FileInfo(FILENAME_METADATA);
                if (fi.Exists)
                {
                    TfsHelper.Instance.CheckOutMetaDataFile(FILENAME_METADATA);
                }

                //string localMetaData = File.ReadAllText(FILENAME_METADATA);
                // FileStream aStream = new FileStream(FILENAME_METADATA, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);
                TextReader textReader = new StreamReader(FILENAME_METADATA);
                XmlTextReader aReader = new XmlTextReader(textReader);
                mSavedLocalMetaData = new MetaData();
                mSavedLocalMetaData.ReadXml(aReader, XmlReadMode.IgnoreSchema);
                aReader.Close();
                if (mSavedLocalMetaData.PathInfoRows.Count > 0)
                {
                    TxtPath.Text = mSavedLocalMetaData.PathInfoRows[0].TargetPath.Replace("/", "\\");
                    PathTfsRepos = mSavedLocalMetaData.PathInfoRows[0].TargetPath.Replace("/", "\\") + "\\";
                }
            }
            catch (FileNotFoundException)
            {
                // Ist ok, wenn Datei nicht vorhanden, wird sie neu angelegt
            }
        }

        #endregion Load Metadata


        #region SaveMetadata

        /// <summary>
        /// Speichert die Eingabe des Anwenders.
        /// </summary>
        /// <remarks>
        /// Die XML-Datei befindet sich im selben verzeichnis wie MyGeneration.exe.
        /// </remarks>
        private void SaveLocalMetadata()
        {
            // Settings
            if (!string.IsNullOrEmpty(txtTfsWorkspace.Text))
                mBlGeneratorSettings.AbweichenderTfsWorkspace = txtTfsWorkspace.Text;
            GeneratorHelper.SerializeSettings(mBlGeneratorSettings);

            if (!string.IsNullOrEmpty(TxtPath.Text))
            {
                // Save Path Info Row, if Path Textbox is filled
                if (mMetaDataFromDatabase.PathInfoRows.Count <= 0)
                {
                    MetaData.PathInfoRow aRow = mMetaDataFromDatabase.PathInfoRows.NewPathInfoRow();
                    aRow.TargetPath = TxtPath.Text.Replace("/", "\\");
                    mMetaDataFromDatabase.PathInfoRows.Rows.Add(aRow);
                }
                else
                {
                    mMetaDataFromDatabase.PathInfoRows[0].TargetPath = TxtPath.Text.Replace("/", "\\");
                }
                // Don't forget to Persist the Changes
                mMetaDataFromDatabase.AcceptChanges();
            }

            FileInfo fi = new FileInfo(FILENAME_METADATA);
            if (fi.Exists && fi.IsReadOnly)
            {
                TfsHelper.Instance.CheckOutMetaDataFile(FILENAME_METADATA);
                fi = new FileInfo(FILENAME_METADATA);
                if (fi.IsReadOnly)
                {
                    sbStatusLabel.ForeColor = Color.Red;
                    sbStatusLabel.Text = string.Format("Die Datei '{0}' kann nicht aus dem TFS ausgecheckt werden.", FILENAME_METADATA);
                }
            }

            //string localMetaData = File.ReadAllText(FILENAME_METADATA);
            //FileStream aStream = new FileStream(FILENAME_METADATA, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 65536);
            TextWriter textwriter = new StreamWriter(FILENAME_METADATA);
            XmlTextWriter aWriter = new XmlTextWriter(textwriter);
            aWriter.IndentChar = ' ';
            aWriter.Indentation = 2;
            aWriter.Formatting = Formatting.Indented;
            mMetaDataFromDatabase.WriteXml(aWriter, XmlWriteMode.IgnoreSchema);
            aWriter.Close();

            try
            {
                // File einchecken
                TfsHelper.Instance.CheckInMetaDataFile();
            }
            catch (Exception e)
            {
                sbStatusLabel.ForeColor = Color.Red;
                sbStatusLabel.Text = e.Message;
            }
        }

        #endregion SaveMetadata


        #region FillGenInfo

        /// <summary>
        /// Die Metadaten vom Anwender ausgewählten Tabellen / Views sammeln.
        /// </summary>
        private void FillGenInfo()
        {
            // erst einmal Stammdatnübernahme prüfen und abhängige Tabellen anhaken
            foreach (MetaData.MetaDataRow aRow in mMetaDataFromDatabase.MetaDataRows)
            {
                if (aRow.Generate && !chkStammdatenuebernahmeIgnorieren.Checked)
                {
                    // Aliaskennung setzen
                    if (string.IsNullOrEmpty(aRow.AliasKennungColumn))
                        aRow.AliasKennungColumn = "KENNUNG";
                    // Prüfe auf Stammdatenübernahme, wenn gesetzt, dann hole relevante Infos dafür
                    if (aRow.Stammdatenuebernahme)
                    {
                        ArrayList foreignKeys = GetForeignKeys(aRow);
                        foreach (StammdatenuebernahmeInfo sInfo in foreignKeys)
                        {
                            StammdatenuebernahmeInfo sInfoTemp = sInfo;
                            MetaData.MetaDataRow myRow = (from MetaData.MetaDataRow metaRow in mMetaDataFromDatabase.MetaDataRows
                                                          where metaRow.ObjectName.Equals(sInfoTemp.ObjectName)
                                                          select metaRow).First();
                            myRow.Generate = true;
                            myRow.Stammdatenuebernahme = true;
                        }
                    }
                }
            }

            mGenInfoData = new ArrayList(mMetaDataFromDatabase.MetaDataRows.Rows.Count);
            foreach (MetaData.MetaDataRow aRow in mMetaDataFromDatabase.MetaDataRows)
            {
                if (aRow.Generate)
                {
                    // wir merken uns den Objektnamen, wir benötigen ihn häufiger,
                    // können aber aus unerfindlichen Gründen nur einmal darauf zugreifen :-(
                    string objectname = aRow.ObjectName;

                    // Prüfe auf Generierung der TableValuedParameter
                    if (!mNeedPruefeTvpGeneration && mExistingTableValuedParameterForTables.Contains(objectname))
                        mNeedPruefeTvpGeneration = true;

                    // GenInfo aufbauen
                    GenInfo info = new GenInfo();
                    info.ObjectName = objectname;
                    info.Namespace = aRow.Namespace;
                    info.Lov = aRow.LOV;
                    info.NormTimeFrom = aRow.NORMTIMEFROM;
                    info.NormTimeTo = aRow.NORMTIMETO;
                    info.Generate = aRow.Generate;
                    info.ObjectType = aRow.Type;
                    info.EntityName = aRow.EntityName;
                    info.Stammdatenuebernahme = aRow.Stammdatenuebernahme;
                    info.AliasKennungColumn = aRow.AliasKennungColumn.ToLowerInvariant();
                    info.DbReaderDataTypeColumns = aRow.DbReaderDataTypeColumns;
                    info.DbReaderOperatorColumns = aRow.DbReaderOperatorColumns;
                    info.IsPrimaryKeyInteger = aRow.IsPrimaryKeyInteger;
                    info.IntegerPrimaryKeyColumns = aRow.PrimaryKeyIntegerColumnNames;
                    if (aRow.LOV)
                    {
                        info.LovColumns = aRow.LovColumns;
                    }
                    if (aRow.NORMTIMEFROM)
                    {
                        info.NormTimeFromColumns = aRow.NormTimeFromColumns;
                    }
                    if (aRow.NORMTIMETO)
                    {
                        info.NormTimeToColumns = aRow.NormTimeToColumns;
                    }
                    if (aRow.Type.Equals(TYPE_FUNCTION))
                    {
                        FunctionGenInfo funcGeninfo = new FunctionGenInfo(objectname);
                        if (!mFunctionGenInfos.Contains(funcGeninfo))
                        {
                            funcGeninfo.FunctionReturnValueColumns = new SqlFunctionAnalyzer(mSqlConnnectionZesaEntw.ConnectionString).GetTvfReturnColumns(objectname);
                            funcGeninfo.FunctionInputParameterColumns = GetInputParameterForFunction(objectname);
                            mFunctionGenInfos.Add(funcGeninfo);
                        }
                    }
                    mGenInfoData.Add(info);

                    // Prüfe auf Stammdatenübernahme, wenn gesetzt, dann hole relevante Infos dafür
                    if (aRow.Stammdatenuebernahme && !chkStammdatenuebernahmeIgnorieren.Checked)
                    {
                        // StamdatenübernahmeInfo Objekt aufbauen
                        StammdatenuebernahmeInfo sInfo = new StammdatenuebernahmeInfo();
                        sInfo.ObjectName = aRow.ObjectName;
                        sInfo.Namespace = aRow.Namespace;
                        sInfo.ForeignKeys = GetForeignKeys(aRow);
                        sInfo.ColumnsForUniqueIndices = GetColumnsForUniqueIndices(aRow);
                        sInfo.AliasKennungColumn = aRow.AliasKennungColumn;

                        // wir sortieren das Element an der richtigen Stelle ein
                        int insertAt = mStammdatenWithForeignKeys.Count;
                        for (int i = 0; i < mStammdatenWithForeignKeys.Count; i++)
                        {
                            StammdatenuebernahmeInfo sdInfo = mStammdatenWithForeignKeys[i] as StammdatenuebernahmeInfo;
                            if (sdInfo != null)
                            {
                                var test = from StammdatenuebernahmeInfo s in sdInfo.ForeignKeys
                                           where s.ObjectName.Equals(sInfo.ObjectName)
                                           select s;
                                if (test.Any())
                                {
                                    insertAt = i;
                                    break;
                                }
                            }
                        }
                        // das Element an die ermitelte Stelle schieben
                        mStammdatenWithForeignKeys.Insert(insertAt, sInfo);
                    }
                }
            }
        }


        #endregion FillGenInfo

        #region IntegerPrimaryKeys

        /// <summary>
        /// gibt eine Liste aller PrimaryKeys vom TypInteger für Tabellen oder Views aus
        /// </summary>
        /// <param name="pTables"></param>
        /// <returns></returns>
        private string[] GetIntegerPrimaryAndForeignKeyColumnNames(ITables pTables)
        {
            // gehe durch jede Tabelle die übergeben wurden
            // Rückgabewert
            List<string> myList = new List<string>();
            foreach (ITable table in pTables)
            {
                bool oIsPkInteger;
                // und prüfe die PK-Columns auf uniqueidentifier
                myList.AddRange(GetIntegerPrimaryAndForeignKeyColumnNames(table, out oIsPkInteger));
            }

            myList.Sort();

            // Liste zurückgeben
            return myList.ToArray();
        }

        /// <summary>
        /// gibt eine Liste von Integer-Pks zurück
        /// </summary>
        /// <param name="table"></param>
        /// <param name="oIsPkInteger"></param>
        /// <returns></returns>
        private List<string> GetIntegerPrimaryAndForeignKeyColumnNames(ITable table, out bool oIsPkInteger)
        {
            oIsPkInteger = false;

            List<string> retValue = new List<string>();
            // prüfe die PK-Columns auf uniqueidentifier
            foreach (IColumn pkColumn in table.PrimaryKeys)
            {
                if (!pkColumn.DataTypeName.ToLower().Equals("uniqueidentifier") && !retValue.Contains(pkColumn.Name))
                {
                    retValue.Add(pkColumn.Name);
                    oIsPkInteger = true;
                }

            }

            // sowie alle FK-Columns auf uniqueidentifier
            foreach (IForeignKey foreignKey in table.ForeignKeys)
            {
                foreach (IColumn foreignKeyColumn in foreignKey.ForeignColumns)
                {
                    if (!foreignKeyColumn.DataTypeName.ToLower().Equals("uniqueidentifier") && !retValue.Contains(foreignKeyColumn.Name))
                        retValue.Add(foreignKeyColumn.Name);
                }
            }

            retValue.Sort();

            // wir geben unser Ergebnis zurück
            return retValue;
        }

        #endregion IntegerPrimaryKeys


        #region Stammdatenübernahme

        /// <summary>
        /// ermittelt ALLE Foreign Keys (auch abhängige) zu einer Tabelle
        /// </summary>
        /// <param name="pMetaDataRow"></param>
        /// <returns></returns>
        private ArrayList GetForeignKeys(MetaData.MetaDataRow pMetaDataRow)
        {
            ArrayList foreignKeys = new ArrayList();
            // entsprechende Tabelle aus den Metadaten holen
            ITable table = (from ITable tab in mMetaData.DefaultDatabase.Tables where tab.Name.Equals(pMetaDataRow.ObjectName) select tab).First();
            for (int i = 0; i < table.ForeignKeys.Count; i++)
            {
                IForeignKey fk = table.ForeignKeys[i];
                if (!fk.PrimaryTable.Name.Equals(table.Name))
                {
                    // ForeignKey für AenderunsgbemerkungId, Funktion_Anwendungsfall_DomaeneId werden für folgende Tabellen nicht übernommen!
                    // da diese Abhängigkeiten ein Henne-Ei Problem erzeugen...

                    // BA_AENDERUNGSBEMERKUNG
                    // BA_FUNKTION
                    // BA_ANWENDUNGSFALL
                    // BA_DOMAENE
                    // BA_FUNKTION_ANWENUNGSFALL_DOMAENE
                    if (pMetaDataRow.ObjectName.Equals("BA_AENDERUNGSBEMERKUNG"))
                    {
                        if (fk.PrimaryTable.Name.Equals("BA_ANWENDUNGSFALL") ||
                            fk.PrimaryTable.Name.Equals("BA_FUNKTION_ANWENDUNGSFALL_DOMAENE"))
                        {
                            // FK nicht übernehmen
                            continue;
                        }
                    }

                    if (pMetaDataRow.ObjectName.Equals("BA_ANWENDUNGSFALL") ||
                        pMetaDataRow.ObjectName.Equals("BA_ONLINEHILFE") ||
                        pMetaDataRow.ObjectName.Equals("BA_DOMAENE") ||
                        pMetaDataRow.ObjectName.Equals("BA_FUNKTION") ||
                        pMetaDataRow.ObjectName.Equals("BA_FUNKTION_ANWENDUNGSFALL_DOMAENE"))
                    {
                        if (fk.PrimaryTable.Name.Equals("BA_AENDERUNGSBEMERKUNG") ||
                            fk.PrimaryTable.Name.Equals("BA_FUNKTION_ANWENDUNGSFALL_DOMAENE"))
                        {
                            // FK nicht übernehmen
                            continue;
                        }
                    }

                    // die Metadaten zum Foreign Key
                    MetaData.MetaDataRow fkMetadaDataRow = (from MetaData.MetaDataRow metaRow in mMetaDataFromDatabase.MetaDataRows
                                                            where metaRow.ObjectName.Equals(fk.PrimaryTable.Name)
                                                            select metaRow).First();

                    // prüfen ob der FK schon in der Liste enthalten ist
                    IEnumerable<StammdatenuebernahmeInfo> bestehendeFks = from StammdatenuebernahmeInfo sdInfo in foreignKeys
                                                                          where sdInfo.ObjectName.Equals(fk.PrimaryTable.Name)
                                                                          select sdInfo;

                    // wenn dem so ist, wird er nicht noch einmal hinzugefügt
                    if (bestehendeFks.Any())
                        continue;


                    // wenn keine Stammdaten für die entsprechende Tabelle generiert werden darf, so
                    // dürfen sie auch nicht in den FK's enthaltens sein...
                    if (mKeineDatenuebernahmeGenerieren.Contains(fk.PrimaryTable.Name))
                        continue;

                    StammdatenuebernahmeInfo sInfo = new StammdatenuebernahmeInfo();
                    sInfo.ObjectName = fk.PrimaryTable.Name;
                    sInfo.Namespace = fkMetadaDataRow.Namespace;
                    sInfo.AliasKennungColumn = string.IsNullOrEmpty(fkMetadaDataRow.AliasKennungColumn) ? "KENNUNG" : fkMetadaDataRow.AliasKennungColumn;
                    // der Rückgabeliste hinzufügen
                    foreignKeys.Add(sInfo);
                }
            }
            return foreignKeys;
        }

        /// <summary>
        /// ermittelt ALLE Unique Indexes mit den zugehörigen Einschränkungen zu einer Tabelle
        /// </summary>
        /// <param name="pMetaDataRow"></param>
        /// <returns></returns>
        private ArrayList GetColumnsForUniqueIndices(MetaData.MetaDataRow pMetaDataRow)
        {
            ArrayList uniqueColumns = new ArrayList();
            // entsprechende Tabelle aus den Metadaten holen
            ITable table = (from ITable tab in mMetaData.DefaultDatabase.Tables where tab.Name.Equals(pMetaDataRow.ObjectName) select tab).First();
            // Index ermitteln
            IIndexes indexes = table.Indexes;
            foreach (Index idx in indexes)
            {
                // Ist der Index eindeutig, Columns merken
                if (idx.Unique)
                {
                    IColumns cols = idx.Columns;
                    foreach (Column col in cols)
                    {
                        // Wenn der Spaltenname nicht der PK ist und der Spaltenname noch nicht in der Liste ist und nicht der Mandant oder der Buchungskreis ist
                        // füge ihn hinzu...
                        if (!col.Name.Equals(table.PrimaryKeys[0].Name) && !uniqueColumns.Contains(col.Name) && !col.Name.ToUpper().Equals("MANDANT") && !col.Name.ToUpper().Equals("BUCHUNGSKREIS"))
                        {
                            uniqueColumns.Add(col.Name);
                        }
                    }
                }
            }
            // Liste zurückgeben
            return uniqueColumns;
        }

        /// <summary>
        /// Prüft ob Stammdatenübernahme angehakt wurde, wenn ja, wird weiter geprüft, ob 
        /// gewähltes DB Objekt eine Stammdatentabelle ist, wenn nicht so kann das Objekt nicht gewählt werden
        /// </summary>
        private void PruefeStammdatenUebernahme()
        {
            sbStatusLabel.ForeColor = Color.Black;
            sbStatusLabel.Text = string.Empty;

            foreach (MetaData.MetaDataRow aRow in mMetaDataFromDatabase.MetaDataRows)
            {
                if (aRow.Stammdatenuebernahme)
                {
                    // Prüfen, ob es sich um eine View handelt, wenn ja Fehler
                    if (aRow.Type.Equals(TYPE_VIEW) || aRow.Type.Equals(TYPE_FUNCTION))
                    {
                        aRow.Stammdatenuebernahme = false;
                        sbStatusLabel.ForeColor = Color.Red;
                        sbStatusLabel.Text = string.Format("Entität {0} ist eine View / Function und kann daher nicht ausgewählt werden!", aRow.ObjectName);
                        continue;
                    }

                    // zur Generierung anhaken, wenn Datenübernahme generiert werden soll
                    if (!chkStammdatenuebernahmeIgnorieren.Checked && !aRow.Generate)
                        aRow.Generate = true;

                    // Prüfen ob die Tabelle in den Settings vermerkt ist, 
                    // wenn ja Prüfung gegen Stammdatentabellen überspringen...
                    if (mBlGeneratorSettings.EnitaetenAusnahmeDatenuebernahme.Contains(aRow.ObjectName))
                        continue;

                    // Prüfung gegen die Stammdatentabellen
                    MetaData.MetaDataRow myRow = aRow;
                    bool stammdatensatzVorhandenNormierungEmptyStringToUnderscore = (from tab in mStammdatentabellen
                                                                                     let tabNormiert = tab.Replace(" ", "_").ToLower()
                                                                                     let entityNameNormiert = myRow.EntityName.Replace(" ", "_").ToLower()
                                                                                     where tabNormiert.ToLower().EndsWith(entityNameNormiert)
                                                                                     select tab).Any();
                    bool stammdatensatzVorhandenNormierungTrimUnderscore = (from tab in mStammdatentabellen
                                                                            let tabNormiert = tab.Replace("_", "").ToLower()
                                                                            let entityNameNormiert = myRow.EntityName.Replace("_", "").ToLower()
                                                                            where tabNormiert.ToLower().EndsWith(entityNameNormiert)
                                                                            select tab).Any();


                    if (stammdatensatzVorhandenNormierungEmptyStringToUnderscore ||
                        stammdatensatzVorhandenNormierungTrimUnderscore)
                    {
                        // Stammdatensatz vorhanden? weiter gehts...
                        continue;
                    }
                    else
                    {
                        // keine Stammdatensätze? Fehler werfen!
                        aRow.Stammdatenuebernahme = false;
                        mKeineDatenuebernahmeGenerieren.Add(aRow.ObjectName);
                        sbStatusLabel.ForeColor = Color.Red;
                        sbStatusLabel.Text = string.Format("Entität {0} ist keine registrierte Stammdatentabelle und kann daher nicht ausgewählt werden!", aRow.ObjectName);
                    }
                }

                // Prüft ob die Datenübernahme Pflicht ist, wenn dem so ist, wird sie angehakt
                if (mBlGeneratorSettings.EnitaetenPflichDatenuebernahme.Contains(aRow.ObjectName))
                {
                    if (!aRow.Stammdatenuebernahme)
                    {
                        aRow.Stammdatenuebernahme = true;
                        sbStatusLabel.ForeColor = Color.Blue;
                        sbStatusLabel.Text = string.Format("Entität {0} ist verpflichtend für die Stammdatenübernahme und kann daher nicht abgewählt werden!", aRow.ObjectName);
                    }

                    // zur Generierung anhaken, wenn Datenübernahme generiert werden soll
                    if (!chkStammdatenuebernahmeIgnorieren.Checked && !aRow.Generate)
                        aRow.Generate = true;
                }
            }
        }

        /// <summary>
        /// gibt zu einer Funktion die Input Parameter zurück
        /// </summary>
        /// <param name="pProcedureName"></param>
        /// <returns></returns>
        public List<TvfColumnInfo> GetInputParameterForFunction(string pProcedureName)
        {
            List<TvfColumnInfo> inputParam = new List<TvfColumnInfo>();
            IProcedure procedure = (from IProcedure proc in mMetaData.DefaultDatabase.Procedures where proc.Name.Equals(pProcedureName) select proc).First();
            foreach (IParameter param in procedure.Parameters)
            {
                // uns interessieren nur INputParameter
                if (param.Direction == ParamDirection.Input)
                {
                    TvfColumnInfo col = new TvfColumnInfo();
                    col.Name = param.Name;
                    col.IsNullable = param.IsNullable;
                    col.LanguageType = param.TypeName;
                    inputParam.Add(col);
                }
            }
            return inputParam;
        }

        /// <summary>
        /// Ermittelt alle in ZEAS hinterlegten Stammdatentabellen über die tabelle BA_FUNKTION
        /// </summary>
        /// <returns></returns>
        private void GetStammdatenTabellen()
        {
            mStammdatentabellen = new List<string>();
            // wir ermitteln alle Stammdatentabellenaus BA_FUNKTION
            SqlDataAdapter sqlAdapter = new SqlDataAdapter("select distinct funktion from ba_funktion where funktion like 'stammdaten%' and mandant = 100", mSqlConnnectionZesaEntw);
            DataSet stammdatenTabellen = new DataSet();

            try
            {
                if (mSqlConnnectionZesaEntw.State != ConnectionState.Open)
                    mSqlConnnectionZesaEntw.Open();
                // DataSet füllen
                sqlAdapter.Fill(stammdatenTabellen);
            }
            finally
            {
                if (mSqlConnnectionZesaEntw.State != ConnectionState.Closed)
                    mSqlConnnectionZesaEntw.Close();
            }

            // nur wenn etwas zurückgekommen ist, dem Rückgabewert hinzufügen
            if (stammdatenTabellen.Tables.Count > 0 && stammdatenTabellen.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in stammdatenTabellen.Tables[0].Rows)
                {
                    mStammdatentabellen.Add(row[0].ToString());
                }
            }
        }

        #endregion Stammdatenübernahme

        #region Events von Steuerelementen

        /// <summary>
        /// Der Anwender möchte das Ausgabeverzeichnis auswählen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetPath_Click(object sender, EventArgs e)
        {
            DlgPath.Description = "Bitte wählen Sie das Zielverzeichnis aus";
            DlgPath.ShowNewFolderButton = true;
            DlgPath.SelectedPath = Environment.CurrentDirectory;
            if (DlgPath.ShowDialog(this) == DialogResult.OK)
            {
                TxtPath.Text = DlgPath.SelectedPath;
            }
        }

        /// <summary>
        /// Die Generierung wird gestartet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            // erst einmal validieren
            if (!ValidateBeforeGenerate())
                return;

            mTargetPath = TxtPath.Text;
            // Pfad prüfen
            string testPath = TxtPath.Text.Replace("/", "\\") + "\\";
            // und korrigieren wenn er nicht stimmt
            if (!testPath.Equals(PathTfsRepos))
                PathTfsRepos = testPath;

            Cursor = Cursors.WaitCursor;
            // Stammdatenübernahme prüfen,
            PruefeStammdatenUebernahme();
            // GenInfo füllen, und ab gehts...
            FillGenInfo();
            SaveLocalMetadata();

            Cursor = Cursors.Default;
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Die Generierung wird abgebrochen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            SaveLocalMetadata();
            mGenInfoData = null;
            mTargetPath = string.Empty;
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// das Formular wird geschlossen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BLGeneratorUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mForceClose)
                return;

            // Daten speichern, ggf im TFS  einchecken
            SaveLocalMetadata();
        }

        /// <summary>
        /// Alle Tabellen / Views selektieren.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (MetaData.MetaDataRow row in mMetaDataFromDatabase.MetaDataRows)
            {
                row.BeginEdit();
                row.Generate = true;
                row.EndEdit();
            }
            mMetaDataFromDatabase.AcceptChanges();
        }

        /// <summary>
        /// Alle Tabellen / Views deselektieren.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (MetaData.MetaDataRow row in mMetaDataFromDatabase.MetaDataRows)
            {
                row.BeginEdit();
                row.Generate = false;
                row.EndEdit();
            }
            mMetaDataFromDatabase.AcceptChanges();
        }

        // Fängt das CurrentCellChanged Event ab um Validierungen durchzuführen...
        private void grdData_CurrentCellChanged(object sender, EventArgs e)
        {
            PruefeStammdatenUebernahme();
        }

        // Stellt den Inhalt der Statusbar in einer Messagebox dar
        private void sbStatusLabel_DoubleClick(object sender, EventArgs e)
        {
            if (sbStatusLabel.Text.Length > 0)
                MessageBox.Show(sbStatusLabel.Text, "Statuszeile", MessageBoxButtons.OK);
        }

        #endregion Events von Steuerelementen


        #region Helper

        /// <summary>
        /// Validiert grundlegende Einstellungen vor der Generierung
        /// </summary>
        private bool ValidateBeforeGenerate()
        {
            // Zielverzeichnis gewählt?
            if (string.IsNullOrEmpty(TxtPath.Text))
            {
                sbStatusLabel.ForeColor = Color.Red;
                sbStatusLabel.Text = "Kein Zielverzeichnis gewählt!";
                return false;
            }

            // Prüfe auf ausgefüllten Namespace
            foreach (MetaData.MetaDataRow aRow in mMetaDataFromDatabase.MetaDataRows)
            {
                if (aRow.Generate)
                {
                    if (string.IsNullOrEmpty(aRow.Namespace))
                    {
                        sbStatusLabel.ForeColor = Color.Red;
                        sbStatusLabel.Text = string.Format("Für die Entität {0} wurde kein Namespace gesetzt!", aRow.ObjectName);
                        return false;
                    }
                }
            }
            // alles in Ordnung
            return true;
        }

        /// <summary>
        /// Prüft die Version der Generator DLL
        /// </summary>
        private void CheckVersion()
        {

#if DEBUG
            Text = Text + " - DEBUG MODUS - ";
            return;
#endif
            Text = Text + " - PRODUKTIV MODUS - ";

            // Cursor setzen
            Cursor = Cursors.WaitCursor;

            // Erst einmal einen Get auf die Utilities machen
            TfsHelper.Instance.GetUtilities();

            string path = GetAutoUpdatePath();
            // Existiert nicht? keine Versionsprüfung
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            // Version einlesen
            string curVersion = File.ReadAllText(path);
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (curVersion != assemblyVersion)
            {
                MessageBox.Show(
                   "Es liegt eine geänderte Version der Generator DLL vor, die Version wird jetzt automatisch aktualisiert.",
                   "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Information);

                string destination = Application.StartupPath;
                string source = string.Empty;
                string[] pathParts = PathTfsRepos.Split('\\');
                int i = 0;
                while (pathParts[i].ToLower() != "zesa")
                {
                    source += pathParts[i] + "\\";
                    i++;
                }
                source += "ZESA\\Utilities\\MyGenerationTemplates\\BusinesslogicGeneratorUI\\GeneratorDll\\Version" + curVersion.Substring(0, curVersion.LastIndexOf("."));

                string autoUpdaterPath = path.Substring(0, path.LastIndexOf("\\"));
                if (!autoUpdaterPath.EndsWith("\\")) autoUpdaterPath += "\\";

                // Cursor setzen
                Cursor = Cursors.Default;

                mForceClose = true;
                Process.Start(autoUpdaterPath + "BlGeneratorAutoUpdate.exe", source + "," + destination);
                Application.Exit();
            }
        }

        // HiddenFeature: Aktualisiert das Versionsfile wenn F12 in der Textbox gedrückt wird :-)
        private void TxtPath_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.F12)
            {
                // Pfad auslesen
                string path = GetAutoUpdatePath();

                // Leerer Pfad? zurück
                if (string.IsNullOrEmpty(path))
                    return;

                // Existiert? dann auschecken
                if (File.Exists(path))
                    TfsHelper.Instance.CheckOut(path);

                string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                File.WriteAllText(path, assemblyVersion);
            }
        }

        /// <summary>
        /// Holt den Pfad zur Autoupdatefunktionialität
        /// </summary>
        /// <returns></returns>
        private string GetAutoUpdatePath()
        {
            if (string.IsNullOrEmpty(PathTfsRepos))
                return string.Empty;

            string[] pathParts = PathTfsRepos.Split('\\');
            string path = string.Empty;
            int i = 0;
            while (pathParts[i].ToLower() != "zesa")
            {
                path += pathParts[i] + "\\";
                i++;
            }
            path += "ZESA\\Utilities\\MyGenerationTemplates\\BusinesslogicGeneratorUI\\GeneratorDll\\AutoUpdate\\CurrentVersion.txt";
            return path;
        }

        // Lock and Unlock TFS Field
        private void btnLockTfsField_Click(object sender, EventArgs e)
        {
            if (txtTfsWorkspace.ReadOnly)
            {
                // UNLOCK
                txtTfsWorkspace.ReadOnly = false;
                btnLockTfsField.ImageIndex = 1;
            }
            else
            {
                // LOCK
                txtTfsWorkspace.ReadOnly = true;
                btnLockTfsField.ImageIndex = 0;

                // dem TfsHelper den ggf. geänderten Workspace mitteilen
                if (!string.IsNullOrEmpty(txtTfsWorkspace.Text))
                    TfsHelper.Instance.CurrentTfSWorkspace = txtTfsWorkspace.Text;
            }
        }

        /// <summary>
        /// Eliminiert Aliasnamen die eigentlich IntPks sind - es wird zumindest versucht
        /// </summary>
        /// <param name="pPrimaryAndForeignKeyColumnNamesOfTables"></param>
        /// <param name="pView"></param>
        /// <returns></returns>
        private string[] EliminateAliasesForPrimaryKeyColumns(string[] pPrimaryAndForeignKeyColumnNamesOfTables, IView pView)
        {
            if (null == pPrimaryAndForeignKeyColumnNamesOfTables || !pPrimaryAndForeignKeyColumnNamesOfTables.Any())
                return pPrimaryAndForeignKeyColumnNamesOfTables;

            List<string> primaryKeyColumnsOfTables = pPrimaryAndForeignKeyColumnNamesOfTables.ToList();
            foreach (Column col in pView.Columns)
            {
                if (GeneratorHelper.IsNumeric(col, new Dictionary<string, string>()))
                {
                    // wir zählen underscores, indem wir den string anach splitten lassen
                    // wenn es mehr als einen gibt, bilde einen Substring vom Ende bis zum Vorletzten underscore ud prüfe ob es sich um ein PK Feld handelt
                    // diese Krücke ist leider motwendig da für viele Ids in Views Aliases verwendet werden, die in MyMeta leider unbekannt sind.
                    string[] sepValues = col.Name.ToUpper().Split(new[] { '_' });

                    if (sepValues.Length > 2 && sepValues[sepValues.Length - 1].ToUpper().Equals("ID") && !primaryKeyColumnsOfTables.Contains(col.Name.ToUpper()))
                    {
                        StringBuilder testColName = new StringBuilder("ID");
                        // wir prüfen alle möglichen SepValues
                        for (int i = sepValues.Length - 1; i > 0; i--)
                        {
                            testColName.Insert(0, string.Format("{0}_", sepValues[i - 1]));
                            if (primaryKeyColumnsOfTables.Contains(testColName.ToString()) && !primaryKeyColumnsOfTables.Contains(col.Name.ToUpper()))
                                primaryKeyColumnsOfTables.Add(col.Name.ToUpper());
                        }
                    }
                }
            }
            // Liste zurückgeben
            return primaryKeyColumnsOfTables.ToArray();
        }

        #endregion Helper
    }
}
