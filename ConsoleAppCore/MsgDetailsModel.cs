using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleAppCore
{
	public class MsgDetails
	{
		public long MsgID { get; set; }
		public string MsgFirst { get; set; }
		public string MsgSecond { get; set; }
		public string MsgThird { get; set; }
		public string MsgFour { get; set; }
		public long MsgEnumId { get; set; }
		public long RetryCount { get; set; }
		public DateTime IssueDate { get; set; }
		public string UserID { get; set; }
		public bool isDeleted { get; set; }

	}
	public enum DataStatus
	{
		Pending=0,
		Success=1,
		Failed=2		
	}
}
