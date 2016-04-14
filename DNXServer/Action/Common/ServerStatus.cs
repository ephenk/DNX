using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Timers;
using DNXServer.Net;

namespace DNXServer.Action
{
	public class ServerStatus : BaseAction 
	{
		public ServerStatus ()
		{
		}

		public string StatsCheck()
		{
			//bool isOffline;
			//string status;
			//StringDictionary server;
			List <string> server = new List<string> ();

			using (Command(@"server_get_server_status"))
			{
				foreach (StringDictionary result in ExecuteSpRead()) 
				{
					List<string> stat = new List <string> ();
					stat.Add (result ["_status_desc"]);
					stat.Add (result ["_is_offline"]);

					string serverStat = String.Join("~", stat);
					server.Add (serverStat);
				}
				string final = String.Join("",server);
				return final;
			}

			/*
			using (Command(@"SELECT status_desc,is_offline FROM server_status")) 
			{
				server = ExecuteRow();
				isOffline = Convert.ToBoolean(server["is_offline"]);
				status = server["status_desc"];
			}
			return isOffline + ";" + status;
			*/

		}

//		static Timer timer;
//
//		public void PerformMaintenance( int seconds, string message)
//		{
//			if(seconds != 0)
//			{
//				timer = new Timer (seconds * 1000);
//				timer.Enabled = true;
//			}
//
//			using(Command("UPDATE server_status SET is_offline = False ,status_desc := stat"))
//			{
//				AddParameter ("stat", message);
//				ExecuteRow ();
//			}
//
//		}

	}
}

