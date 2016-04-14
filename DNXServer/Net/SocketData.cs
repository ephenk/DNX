using System;
using System.Net.Sockets;

namespace DNXServer.Net
{
	public class SocketData
	{
		public static int n = 0;
		private int no;
		private Socket sock;
		
		public const int BYTE_SIZE = 1024;
		public byte[] data = new byte[SocketData.BYTE_SIZE];
		
		public SocketData(Socket sock)
		{
			this.no = SocketData.n;
			SocketData.n++;
			this.sock = sock;
		}

		public void WriteLine(string msg)
		{
			Log.Info ("Socket #" + this.no + ": " + msg);
		}
		
		public Socket Sock
		{
			get
			{ 
				return this.sock;
			}
		}
	}
}

