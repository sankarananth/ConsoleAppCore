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
			? ""
			: row.Field<string>(1),
					MsgSecond = String.IsNullOrEmpty(row.Field<string>(2))
			? ""
			: row.Field<string>(2),
					MsgThird = String.IsNullOrEmpty(row.Field<string>(3))
			? ""
			: row.Field<string>(3),
					MsgFour = String.IsNullOrEmpty(row.Field<string>(4))
			? ""
			: row.Field<string>(4),
					RetryCount=row.Field<long>(6),
					MsgEnumId=row.Field<long>(5),
					IssueDate=row.Field<DateTime>(7)
				}).ToList();
				long id=CheckAndUpdteRetry(msgList);
				if(id!=0)
				{
					MsgDetails mdl = msgList.Where(x => x.MsgID == id).FirstOrDefault();
					msgList.Remove(mdl);
				}
			}

			 return msgList;
		}
		private long CheckAndUpdteRetry(List<MsgDetails> msgList)
		{
			bool success = false;
			long iD = 0;
			foreach (MsgDetails mdl in msgList)
			{
				long retryCount = 0;
				if(mdl.MsgFirst==""||mdl.MsgSecond==""||mdl.MsgThird==""||mdl.MsgFour=="")
				{
		         	retryCount = mdl.RetryCount;
					if(retryCount<=2)
					{
						iD=RefetchData(mdl.MsgID, retryCount);
						
					}
					else
					{
						success=updateRetryModel(mdl, retryCount);
					}
				}
			 
			}
			
			return iD;
		}
		private long RefetchData(long modelID,long retryCount)
		{
			bool success = false;
			long modelId = 0;
			using (SqlConnection conData = new SqlConnection(DataConfig.Connection))
			{
				MsgDetails model = new MsgDetails();
				if (conData.State == ConnectionState.Closed)
				{
					conData.Open();
				}
				DataTable dt = new DataTable();
				do
				{
					SqlDataAdapter da = new SqlDataAdapter("Get_ModelIDMsgMaster", conData);
					da.SelectCommand.CommandType = CommandType.StoredProcedure;
					da.SelectCommand.Parameters.AddWithValue("@ID", modelID);
					da.Fill(dt);
					if (dt.Rows.Count > 0)
					{ 
						model.MsgFirst = dt.Rows[0]["MsgFirst"].ToString();
						model.MsgSecond = dt.Rows[0]["MsgSecond"].ToString();
						model.MsgThird = dt.Rows[0]["MsgThird"].ToString();
						model.MsgFour = dt.Rows[0]["MsgFour"].ToString();
						model.MsgID = Convert.ToInt64(dt.Rows[0]["MsgID"]);
					}
					
					if (model.MsgFirst == "" || model.MsgSecond == "" || model.MsgThird == "" || model.MsgFour == "")
					{
						retryCount++;
						success=updateRetryModel(model, retryCount);
						if(retryCount>2)
						{
							modelId = model.MsgID;
						}
					}
				
				} while (retryCount <= 2);
				
			}
			
			return modelId;
	    }
		private bool updateRetryModel(MsgDetails model,long retryCount)
		{
			bool success = false;
			using (SqlConnection conData = new SqlConnection(DataConfig.Connection))
			{
				if (conData.State == ConnectionState.Closed)
				{
					conData.Open();
				}
				SqlCommand cm = new SqlCommand("Upd_MsgMasterRetry", conData);
				cm.CommandType = CommandType.StoredProcedure;
				cm.Parameters.AddWithValue("@modelId", model.MsgID);
				cm.Parameters.AddWithValue("@retrycount", retryCount);
				int enumId = retryCount <= 2 ? (int)DataStatus.Pending:(int)DataStatus.Failed;
				cm.Parameters.AddWithValue("@enumid", enumId);
				int ret = cm.ExecuteNonQuery();
				if (ret != 0)
					success = true;
				
			}
			return success;
	    }
		public bool InsertToMaster(List<MsgDetails> list)
		{
			bool success = false;
			long failId = 0;
				using (SqlConnection conData = new SqlConnection(DataConfig.Connection))
				{
					if (conData.State == ConnectionState.Closed)
					{
						conData.Open();
					}
				foreach (MsgDetails model in list)
				{
					DataTable dt = new DataTable();

					if(model.MsgEnumId!=2)
					{
						SqlDataAdapter daMaster = new SqlDataAdapter("Ins_MasterMsgTbl", conData);
						daMaster.SelectCommand.CommandType = CommandType.StoredProcedure;
						daMaster.SelectCommand.Parameters.AddWithValue("@MapId", model.MsgID);
						daMaster.Fill(dt);
						failId = model.MsgID;
						int retID = Convert.ToInt32(dt.Rows[0]["ReturnID"].ToString());
						if (retID != 0)
						{

							success = InsertToChild(model, retID);
						}
					}
					
				}
			}
			return success;

		    
		}
		public bool InsertToChild(MsgDetails model,int Mastertblid)
		{
			using (SqlConnection conData = new SqlConnection(DataConfig.Connection))
			{
			
					if (conData.State == ConnectionState.Closed)
					{
						conData.Open();
					}
					
						for (int i = 1; i <= 4; i++)
						{
							SqlCommand daChild = new SqlCommand("Ins_ChildMsgTbl", conData);
							daChild.CommandType = CommandType.StoredProcedure;
							daChild.Parameters.AddWithValue("@MapId", Mastertblid);
							daChild.Parameters.AddWithValue("@Content", returnModelMsg(model, i));
						    daChild.ExecuteNonQuery();
							
						}
					return true;
			}
	    }
		string returnModelMsg(MsgDetails modelMsg,int count)
		{
			switch(count)
			{
				case 1:
					return modelMsg.MsgFirst;
					
				case 2:
					return modelMsg.MsgSecond;
				
				case 3:
					return modelMsg.MsgThird;
					
				case 4:
					return modelMsg.MsgFour;
					
			}
			return "";
		}
		public int DeleteorUpdateFromMains(List<MsgDetails> modelList)
		{
			int ret = 0;
			using (SqlConnection conData = new SqlConnection(DataConfig.Connection))
			{
				if (conData.State == ConnectionState.Closed)
				{
					conData.Open();
				}
				foreach (MsgDetails model in modelList)
				{
					SqlCommand daChild = new SqlCommand("Upd_MsgMaster", conData);
					daChild.CommandType = CommandType.StoredProcedure;
					daChild.Parameters.AddWithValue("@ID", model.MsgID);
					ret = daChild.ExecuteNonQuery();
				}
				
			}
			return ret;
		}
	}
}
