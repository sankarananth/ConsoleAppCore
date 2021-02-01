using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ConsoleAppCore
{
	class Program
	{
		static void Main(string[] args)
		{

			onStart();
			
		}
	     static void onStart()
		{
			
			Console.WriteLine("\t\t\t****************************************************** \n" +
				"\t\t\t\t\tMELCOW TO DATA STORAGE \n" +
				"\t\t\t******************************************************" +
				"\n");
			Console.WriteLine("What do you want to do ? \n" +
				"1.Start to look for new data \n" +
				"2.Exit the Program");
			int whatTodo = Convert.ToInt32(Console.ReadLine());
			int enumID = Convert.ToInt32(DataStatus.Success);
			switch(whatTodo)
			{
				case 1:
					DataManager dm = new DataManager();
					List<MsgDetails> mainList = dm.FetchNewData();
					Console.WriteLine("Fetching Data Please Wait.. \n");
					if (mainList.Count != 0)
					{
						Console.WriteLine(string.Format("{0} no's of rows of data found.. Its yer lucky day mate!! \n \n" +
					"Carrying out the rest of the process please wait ..",mainList.Count));
						bool success = dm.InsertToMaster(mainList);
						if(success)
						{
							Console.WriteLine("Process Completed Successfully");
							dm.DeleteorUpdateFromMains(mainList);
						}
						else
						{
							Console.WriteLine("Process Failed");
						}
					}
					else
					{
						Console.WriteLine("No new data exists! Do you Want to Exit ?");
					}
					
					break;
				case 2:
					Environment.Exit(0);
					break;

				default:
					Console.WriteLine("The Key you've entered is incorrect you naughty! Please Enter the correct Option \n \n");
					onStart();
					break;
			}
			
		}
	}
}
