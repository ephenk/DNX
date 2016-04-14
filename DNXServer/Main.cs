using System;
using System.Net;
using System.Net.Sockets;
using DNXServer.Net;
using DNXServer.Action;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace DNXServer
{
	class MainClass
	{
		public static bool isSilent;
		public static bool isOnline;

		public static void Main(string[] args)
		{
			isSilent = false;
			isOnline = true;

			Log.InitializeLog ();
			Log.Info("Starting DNX server engine");

			DNXConfig.ReadConfigFile();

			Log.InitiateConnectionToServer ();

			// everything is started from this socket
			new DNXSocket();

			Log.Info ("Server engine waiting, press 'Q' to quit");
			
//			for(;;) // The for (; ; ) { } causes 100% cpu usage
//			{
			while (isOnline) {
				ConsoleKeyInfo key = Console.ReadKey ();
				if (key.Key == ConsoleKey.Q) {
					Log.Info ("Server engine terminated");
					break;	
				} else if (key.Key == ConsoleKey.P) {
					Log.Info ("Server pause writing..");
					isSilent = true;

					Console.WriteLine ("Waiting for broadcast message...");	
					string msg = Console.ReadLine ();

					if (msg != null || msg != "") {
						new BroadcastAction ().SendBroadcastMessage (msg);
					}
					isSilent = false;	
					Log.Info ("Server resume writing..");
				}
//				else if(key.Key == ConsoleKey.M)
//				{
//					Log.Info("Server pause writing..");
//					isSilent = true;
//
//					Console.WriteLine("Do you want to perform server maintenance? Y/N");	
//					string confirm = Console.ReadLine ();
//
//					if(confirm == "N" || confirm == "n")
//					{
//						isSilent = false;	
//						Log.Info("Server resume writing..");
//					}
//					else if(confirm == "Y"|| confirm =="y")
//					{
//						int timer = 0;
//						string message;
//
//						Console.WriteLine ("Please input the time to perform maintenance in seconds.. ( 0 to instantly perform maintenance )");
//						timer = Convert.ToInt32(Console.ReadLine ());
//
//						Console.WriteLine ("Please set the maintenance message");
//						message = Console.ReadLine ();
//
//						new ServerStatus ().PerformMaintenance (Convert.ToInt32 (timer), message);
//
//
//						isSilent = false;	
//						Log.Info("Server resume writing..");
//					}
//
//				}
//			}
			}
		}
	}
}
