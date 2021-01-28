using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppCore
{
	public class DataManager
	{
		

		public List<MsgDetails> FetchNewData()
		{
			DataTable dt = new DataTable();
			List<MsgDetails> msgList = new List<MsgDetails>();
			using(SqlConnection conData = new SqlConnection(DataConfig.Connection))
			{
				if(conData.State==ConnectionState.Closed)
				{
				  	conData.Open();
				}
				SqlDataAdapter da = new SqlDataAdapter("Get_ContentsMsgDetails", conData);
				da.SelectCommand.CommandType = CommandType.StoredProcedure;
				da.Fill(dt);
				msgList =  dt.AsEnumerable().Select(row => new MsgDetails
				{

					MsgID = row.Field<long?>(0).GetValueOrDefault(),
					MsgFirst= String.IsNullOrEmpty(row.Field<string>(1))
			? "not found"
			: row.Field<string>(1),
					MsgSecond = String.IsNullOrEmpty(row.Field<string>(2))
			? "not found"
			: row.Field<string>(2),
					MsgThird = String.IsNullOrEmpty(row.Field<string>(3))
			? "not found"
			: row.Field<string>(3),
					MsgFour = String.IsNullOrEmpty(row.Field<string>(4))
			? "not found"
			: row.Field<string>(4),
					MsgEnumId=(long)DataStatus.Pending,
					IssueDate=row.Field<DateTime>(7)
				}).ToList();

			}
			 return msgList;
		}
		public bool InsertToMaster(List<MsgDetails> list)
		{
			bool success = false;
				using (SqlConnection conData = new SqlConnection(DataConfig.Connection))
				{
					if (conData.State == ConnectionState.Closed)
					{
						conData.Open();
					}
				foreach (MsgDetails model in list)
				{
					DataTable dt = new DataTable();
					SqlDataAdapter daMaster = new SqlDataAdapter("Ins_MasterMsgTbl", conData);
					daMaster.SelectCommand.CommandType = CommandType.StoredProcedure;
					daMaster.SelectCommand.Parameters.AddWithValue("@MapId", model.MsgID);
					daMaster.Fill(dt);
					int retID = Convert.ToInt32(dt.Rows[0]["ReturnID"].ToString());
					if (retID != 0)
					{

					  success=InsertToChild(list, retID);
					}
				}
			}
			return success;

		    
		}
		public bool InsertToChild(List<MsgDetails> listChild,int id)
		{
			using (SqlConnection conData = new SqlConnection(DataConfig.Connection))
			{
				try
				{
					if (conData.State == ConnectionState.Closed)
					{
						conData.Open();
					}
					foreach (MsgDetails model in listChild)
					{
						for (int i = 1; i <= 4; i++)
						{
							SqlCommand daChild = new SqlCommand("Ins_ChildMsgTbl", conData);
							daChild.CommandType = CommandType.StoredProcedure;
							daChild.Parameters.AddWithValue("@MapId", id);
							daChild.Parameters.AddWithValue("@Content", returnModelMsg(model, i));
						    daChild.ExecuteNonQuery();
							
						}

					}
					return true;
				}
				catch (Exception ex)
				{
					DeleteFromMains();
					return false;
				}
				
			}
	    }
		string returnModelMsg(MsgDetails modelMsg,int count)
		{
			switch(count)
			{
				case 1:return modelMsg.MsgFirst;
					
				case 2:
					return modelMsg.MsgSecond;
				
				case 3:
					return modelMsg.MsgThird;
					
				case 4:
					return modelMsg.MsgFour;
					
			}
			return "";
		}
		public void DeleteFromMains()
		{

		}
	}
}
