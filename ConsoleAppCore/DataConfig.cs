using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleAppCore
{
	public static class DataConfig
	{
		public static string Connection { get; set; }
		static DataConfig()
		{
			Connection = "Data Source=DESKTOP-O5UA3O7;Initial Catalog=CountCheckDB;uid=ananth;password=ananth";
		}
	}
}
