using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace DNXServer.Action
{
	public class BroadcastAction:BaseAction
	{
		public BroadcastAction ()
		{
		}

		public void SendBroadcastMessage (string message)
		{
			string final  = (int)CommandResponseEnum.Announcement + "`" + message + "`";
			List<byte> temp = new List<byte> ();
			temp.AddRange (Encoding.ASCII.GetBytes (final));
			temp.Add (0x03);

			byte[] data = temp.ToArray();

			for (int j = 0; j < LoginAction.ListUser.Count; j++) 
			{
				LoginAction.ListUser[j].Sock.Send (data);
			}

			Console.WriteLine("Broadcast message has been sent");
		}

		/*
		public string SetMaintenance(SessionData so, string message)
		{			
			if(so.UserId == -1)
			{
				using (Command(@"UPDATE server_status SET is_offline = true, status_desc = :message
					")) {
					AddParameter ("message", message);
					ExecuteWrite ();
				}
				return (int)CommandResponseEnum.SetMaintenance + "`" + "Maintenance has been set" + "`";
			}
			else
			{
				return ((int)CommandResponseEnum.None).ToString ();
			}

		}
		*/
	}
}

