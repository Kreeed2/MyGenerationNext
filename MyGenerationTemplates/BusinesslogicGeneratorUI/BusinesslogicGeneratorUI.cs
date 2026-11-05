using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using EHA.ZESA.Util.ViewGeneratorUI;

using MyMeta;

namespace EHA.ZESA.Util.BusinesslogicGenerator
{
	/// <summary>
	/// Summary description for MyUI.
	/// </summary>
	public class BusinesslogicGeneratorUI : System.Windows.Forms.Form
	{
		private dbRoot   mMetaData;
		private SelectedData mData;

		internal System.Windows.Forms.Button BtnOK;
		private System.Windows.Forms.Panel PnlButtons;
		private System.Windows.Forms.Panel PnlInput;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox TxtPath;
		private System.Windows.Forms.Button BtnSetPath;
		private System.Windows.Forms.FolderBrowserDialog DlgPath;
		private System.Windows.Forms.DataGrid grdData;
		private System.Windows.Forms.DataGridTableStyle DefaultTableStyle;
		private EHA.ZESA.Util.ViewGeneratorUI.MetaData dsMetaData;
		private System.Windows.Forms.DataGridTextBoxColumn colTableName;
		private System.Windows.Forms.DataGridTextBoxColumn colNamespace;
		private System.Windows.Forms.DataGridBoolColumn colGenerate;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public BusinesslogicGeneratorUI(dbRoot pMetaData)
		{
			InitializeComponent();

			mMetaData = pMetaData;
			mData = new SelectedData();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BtnOK = new System.Windows.Forms.Button();
			this.PnlButtons = new System.Windows.Forms.Panel();
			this.PnlInput = new System.Windows.Forms.Panel();
			this.BtnSetPath = new System.Windows.Forms.Button();
			this.TxtPath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.DlgPath = new System.Windows.Forms.FolderBrowserDialog();
			this.grdData = new System.Windows.Forms.DataGrid();
			this.DefaultTableStyle = new System.Windows.Forms.DataGridTableStyle();
			this.dsMetaData = new EHA.ZESA.Util.ViewGeneratorUI.MetaData();
			this.colTableName = new System.Windows.Forms.DataGridTextBoxColumn();
			this.colNamespace = new System.Windows.Forms.DataGridTextBoxColumn();
			this.colGenerate = new System.Windows.Forms.DataGridBoolColumn();
			this.PnlButtons.SuspendLayout();
			this.PnlInput.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grdData)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dsMetaData)).BeginInit();
			this.SuspendLayout();
			// 
			// BtnOK
			// 
			this.BtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BtnOK.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.BtnOK.Location = new System.Drawing.Point(760, 5);
			this.BtnOK.Name = "BtnOK";
			this.BtnOK.TabIndex = 6;
			this.BtnOK.Text = "&OK";
			this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// PnlButtons
			// 
			this.PnlButtons.Controls.Add(this.BtnOK);
			this.PnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PnlButtons.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.PnlButtons.Location = new System.Drawing.Point(0, 688);
			this.PnlButtons.Name = "PnlButtons";
			this.PnlButtons.Size = new System.Drawing.Size(840, 30);
			this.PnlButtons.TabIndex = 11;
			// 
			// PnlInput
			// 
			this.PnlInput.Controls.Add(this.BtnSetPath);
			this.PnlInput.Controls.Add(this.TxtPath);
			this.PnlInput.Controls.Add(this.label1);
			this.PnlInput.Dock = System.Windows.Forms.DockStyle.Top;
			this.PnlInput.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.PnlInput.Location = new System.Drawing.Point(0, 0);
			this.PnlInput.Name = "PnlInput";
			this.PnlInput.Size = new System.Drawing.Size(840, 55);
			this.PnlInput.TabIndex = 12;
			// 
			// BtnSetPath
			// 
			this.BtnSetPath.Location = new System.Drawing.Point(790, 15);
			this.BtnSetPath.Name = "BtnSetPath";
			this.BtnSetPath.Size = new System.Drawing.Size(23, 23);
			this.BtnSetPath.TabIndex = 4;
			this.BtnSetPath.Text = "...";
			this.BtnSetPath.Click += new System.EventHandler(this.BtnSetPath_Click);
			// 
			// TxtPath
			// 
			this.TxtPath.AutoSize = false;
			this.TxtPath.Location = new System.Drawing.Point(105, 15);
			this.TxtPath.Name = "TxtPath";
			this.TxtPath.Size = new System.Drawing.Size(685, 23);
			this.TxtPath.TabIndex = 3;
			this.TxtPath.Text = "";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(5, 15);
			this.label1.Name = "label1";
			this.label1.TabIndex = 1;
			this.label1.Text = "Zielverzeichnis:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// grdData
			// 
			this.grdData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grdData.DataMember = "TableData";
			this.grdData.DataSource = this.dsMetaData;
			this.grdData.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grdData.Location = new System.Drawing.Point(5, 65);
			this.grdData.Name = "grdData";
			this.grdData.Size = new System.Drawing.Size(830, 615);
			this.grdData.TabIndex = 13;
			this.grdData.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																																												this.DefaultTableStyle});
			// 
			// DefaultTableStyle
			// 
			this.DefaultTableStyle.DataGrid = this.grdData;
			this.DefaultTableStyle.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
																																																				this.colTableName,
																																																				this.colNamespace,
																																																				this.colGenerate});
			this.DefaultTableStyle.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.DefaultTableStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultTableStyle.MappingName = "TableData";
			// 
			// dsMetaData
			// 
			this.dsMetaData.DataSetName = "MetaData";
			this.dsMetaData.Locale = new System.Globalization.CultureInfo("de-DE");
			// 
			// colTableName
			// 
			this.colTableName.Format = "";
			this.colTableName.FormatInfo = null;
			this.colTableName.HeaderText = "Tabelle / View";
			this.colTableName.MappingName = "TableName";
			this.colTableName.ReadOnly = true;
			this.colTableName.Width = 300;
			// 
			// colNamespace
			// 
			this.colNamespace.Format = "";
			this.colNamespace.FormatInfo = null;
			this.colNamespace.HeaderText = "Namespace";
			this.colNamespace.MappingName = "Namespace";
			this.colNamespace.Width = 400;
			// 
			// colGenerate
			// 
			this.colGenerate.FalseValue = false;
			this.colGenerate.HeaderText = "Generieren?";
			this.colGenerate.MappingName = "Generate";
			this.colGenerate.NullValue = "False";
			this.colGenerate.TrueValue = true;
			this.colGenerate.Width = 75;
			// 
			// BusinesslogicGeneratorUI
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(840, 718);
			this.Controls.Add(this.grdData);
			this.Controls.Add(this.PnlInput);
			this.Controls.Add(this.PnlButtons);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BusinesslogicGeneratorUI";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "ZESA View-Generator";
			this.Load += new System.EventHandler(this.ViewGeneratorUI_Load);
			this.PnlButtons.ResumeLayout(false);
			this.PnlInput.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.grdData)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dsMetaData)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		public SelectedData Data
		{
			get { return mData; }
			set { mData = value; }
		}

		private void ViewGeneratorUI_Load(object sender, System.EventArgs e)
		{
			if(mMetaData.DefaultDatabase != null)
			{
				FillDataSet();
//				LstViews.DataSource = mMetaData.DefaultDatabase.Views;
//				LstViews.DisplayMember = "Name";
//
//				PropGrid.SelectedObject = mMetaData;
			}	
//			CboNamespaces.SelectedIndex = 0;
		}

		private void BtnOK_Click(object sender, System.EventArgs e)
		{
//			foreach(object item in this.LstViews.SelectedItems)
//			{
//				mData.AddView(item);
//			}
//			mData.Namespace = CboNamespaces.Text;
//			mData.TargetPath = TxtPath.Text;
			this.DialogResult = DialogResult.OK;
		}

		private void BtnSetPath_Click(object sender, System.EventArgs e)
		{
			DlgPath.Description = "Bitte wählen SIe das Zielverzeichnis aus";
			DlgPath.ShowNewFolderButton = true;
			DlgPath.SelectedPath = Environment.CurrentDirectory;
			if(DlgPath.ShowDialog(this) == DialogResult.OK)
			{
				TxtPath.Text = DlgPath.SelectedPath;	
			}
		}


		private void FillDataSet()
		{
			foreach(ITable aTable in mMetaData.DefaultDatabase.Tables)
			{
				string aNamespace = FindNamespace(aTable);
				if(aNamespace != string.Empty)
				{
					MetaData.TableDataRow aRow = dsMetaData.TableData.NewTableDataRow();
					aRow.TableName = aTable.Name;
					aRow.Namespace = aNamespace;
					aRow.Generate  = true;
					dsMetaData.TableData.Rows.Add(aRow);
				}
			}
			foreach(IView aView in mMetaData.DefaultDatabase.Views)
			{
				string aNamespace = FindNamespace(aView);
				MetaData.TableDataRow aRow = dsMetaData.TableData.NewTableDataRow();
				aRow.TableName = aView.Name;
				if(aNamespace != string.Empty)
				{
					aRow.Namespace = aNamespace;
					aRow.Generate  = true;
					dsMetaData.TableData.Rows.Add(aRow);
				}
				else
				{
					aRow.Namespace = string.Empty;
					aRow.Generate  = false;
					dsMetaData.TableData.Rows.Add(aRow);
				}
			}
		}

		private string FindNamespace(ITable pTable)
		{
			string[] parts = pTable.Name.Split('_');
			switch(parts[0])
			{
				case "BA":
					return "Basis";
				case "GP":
					return "Geschaeftspartner";
				case "SY":
					return "Allgemein";
				case "TA":
					return "PreismodelleKonditionen";
				case "TS":
					return "TechnischeStruktur";
                case "TT":
					return "Ticketing";
				case "VT":
					return "Vertrag";
                case "KA":
                    return "Kundenabrechnung";
				default:
					return string.Empty;
			}
		}

		private string FindNamespace(IView pView)
		{
			if(pView.Properties.ContainsKey("Namespace"))
			{
				return (string)pView.Properties["Namespace"].Value;
			}
			return string.Empty;
		}

	}
}
