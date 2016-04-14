using System;
using System.Net.Sockets;

namespace DNXServer.Action
{
	public class SessionData
	{
		public static int n = 0;
		private int no;
		private Socket socketData;

		public const int BYTE_SIZE = 1024;
		public byte[] data = new byte[SessionData.BYTE_SIZE];
		public System.Text.StringBuilder str = new System.Text.StringBuilder();
		
		public SessionData(Socket sock)
		{
			this.no = SessionData.n;
			SessionData.n++;
			this.socketData = sock;
		}

		public void WriteLine(string msg)
		{
			Log.Info("Socket #" + this.no + ": " + msg);
		}
		
		public Socket Sock
		{
			get
			{ 
				return this.socketData;
			}
		}

		public int Flee { get; set;}
		public int UserId { get; set; }
		public int UserCoin { get; set;}
		public int ItemSlot {get;set;}
		public string Username { get; set;}
		public string FacebookId { get; set; }
		public string FacebookPicture { get; set;}
		public int BattleId { get; set; }
		public bool InBattle { get; set; }
		public string Version { get; set;}
		public bool IsRoomMaster { get; set;}
		public bool IsRoomGuest { get; set;}
		public int RoomId { get; set;}
	}
}

