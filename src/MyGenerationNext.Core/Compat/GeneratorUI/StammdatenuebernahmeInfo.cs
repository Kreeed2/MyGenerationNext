using System.Collections;

namespace EHA.ZESA.Util.GeneratorUI
{
	/// <summary>
	/// ein Infoobjekt für die Stammdatenübernahme
	/// </summary>
	public class StammdatenuebernahmeInfo
	{

		private string mObjectName = string.Empty;
		private string mNamespace  = string.Empty;
	    private string mAliasKennungColumn = string.Empty;
		private ArrayList mForeignKeys = new ArrayList();
        private ArrayList mColumnsForUniqueIndices = new ArrayList();

        /// <summary>
		/// Der Name der Tabelle.
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
        /// Eine Liste von StammdatenuebernahemInfos für die ForeignKeys
        /// </summary>
	    public ArrayList ForeignKeys
	    {
            get { return mForeignKeys; }
            set { mForeignKeys = value; }
	    }

        /// <summary>
        /// Eine Liste von Spalten die in Unique Indizes vorkommen
        /// </summary>
        public ArrayList ColumnsForUniqueIndices
        {
            get { return mColumnsForUniqueIndices; }
            set { mColumnsForUniqueIndices = value; }
        }

        /// <summary>
        /// gibt die Splate an, die als Kennung fungiert (sofern angegeben)
        /// </summary>
	    public string AliasKennungColumn
	    {
            get { return mAliasKennungColumn; }
            set { mAliasKennungColumn = value; }
	    }
    }
}
