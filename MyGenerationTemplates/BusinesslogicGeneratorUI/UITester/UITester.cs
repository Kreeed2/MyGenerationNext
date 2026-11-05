using System;
using System.Collections;
using System.Diagnostics;

using EHA.ZESA.Util.GeneratorUI;

//using MyMeta;

namespace EHA.ZESA.Util.UITester
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class UITester
	{

		static void TestNUnitHelper()
		{
			// E:\Projekte\EHA\Zesa\Server\ZESAServer\bin\debug\ZESAServer.dll
            //dbRoot root = new dbRoot();
            //bool result = root.Connect(dbDriver.SQL, @"Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=ZESA_ENTW;Data Source=HEW01EHA05");
            //root.ShowSystemData = false;
            //BLGeneratorUI ui = new BLGeneratorUI(root);
//			ViewGeneratorUI ui = new ViewGeneratorUI(root);
			// ui.SelectedDll = @"E:\Projekte\EHA\Zesa\Server\ZESAServer\bin\debug\ZESAServer.dll";
            //ui.ShowDialog();
//			ArrayList selectedTypes = ui.Data;
//			foreach(TypeInfo info in selectedTypes)
//			{
//				Console.WriteLine("========================================");
//				Console.WriteLine(info.TypeName);
//				Console.WriteLine("========================================");
//				ArrayList methods = info.Methods;
//				foreach(string method in methods)
//				{
//					Console.WriteLine("\t{0}", method);
//				}
//			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			TestNUnitHelper();
		}
	}
}
