using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace DNXServer
{
	public static class Log
	{
		public static Thread thread;
		public static Socket clientSock;
		public static FileStream logFile;

		public static void InitializeLog()
		{
			logFile = File.Open("Logs/trace_" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00"), FileMode.Append);
		}

		public static void InitiateConnectionToServer()
		{
			thread = new Thread(new System.Threading.ThreadStart(ConnectToConsoleServer));
			thread.Start ();
		}

		public static void ConnectToConsoleServer()
		{
			try
			{
				TcpClient client = new TcpClient(DNXConfig.ConsoleHost, DNXConfig.ConsolePort);
				Log.Info ("Connected to Console Server");
				clientSock = client.Client;
			}
			catch(Exception)
			{
				Log.Info ("Connection cannot be made to Console Server");
			}
			finally {
				thread.Interrupt ();
			}
		}

		public static void SendToConsoleServer(string str)
		{
			if(clientSock != null && clientSock.Connected)
			{
				clientSock.Send (System.Text.Encoding.UTF8.GetBytes(str));
			}
		}

		public static void WriteToConsoleAndFile(string log)
		{
			if(!System.IO.Directory.Exists("Logs"))
			{
				System.IO.Directory.CreateDirectory("Logs");
			}
			string s = DateTime.Now.ToString () + ": " + log.ToString () + "\n";
			logFile.Write(Encoding.Default.GetBytes(s), 0, s.Length);

			//Console.Write (MainClass.pause);
			if(!MainClass.isSilent)
			{
				Console.Write(s);
			}
			logFile.Flush ();

		}

		public static void Info(string log)
		{
			WriteToConsoleAndFile("I: " + log);
			SendToConsoleServer ("I: " + log);
		}
		
		public static void Debug(string log)
		{
			WriteToConsoleAndFile("D: " + log);	
			SendToConsoleServer ("D: " + log);
		}
		
		public static void Error(string log)
		{
			WriteToConsoleAndFile("ERROR! " + log);	
		}

		public static void Warning(string log)
		{
			WriteToConsoleAndFile("WARNING! " + log);
		}
	}
}

