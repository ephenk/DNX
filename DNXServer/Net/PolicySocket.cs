using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DNXServer.Net
{
	public class PolicySocket
	{
		private const int PORT = 843;
		protected Socket mainSock;
		
		public PolicySocket ()
		{
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);
			Log.Info("Create new policy socket on port " + PORT);
			mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			                         
			Log.Info("Bind the policy socket");
			mainSock.Bind(endPoint);
			
			Log.Info("Listening to policy request");
			mainSock.Listen(10);
			
			Log.Info("Waiting for policy request connection");
			mainSock.BeginAccept(new AsyncCallback(AcceptConnection), null);
		}
		
		public Socket MainSock
		{
			set
			{
				mainSock = value;	
			}
			get
			{
				return mainSock;
			}
		}

		protected void ReceiveData(IAsyncResult ar)
		{
			SocketData localSo = (SocketData)ar.AsyncState;
			try
			{
				int x = localSo.Sock.EndReceive(ar);
				
				if(x > 0)
				{
					localSo.WriteLine(String.Format("Data size: {0}", x));
					
					if(DNXConfig.ShowInput)
					{
						localSo.WriteLine(String.Format("Raw policy data:\n{0}", Encoding.ASCII.GetString(localSo.data).Trim ('\0')));
					}
					
					string chunk = Encoding.ASCII.GetString(localSo.data).Trim ('\0');
					
					// check if socket policy
					if(chunk == "<policy-file-request/>")
					{
						localSo.WriteLine("Send policy");
						localSo.Sock.Send(Encoding.Default.GetBytes(@"<?xml version=""1.0""?><!DOCTYPE cross-domain-policy SYSTEM ""http://www.adobe.com/xml/dtds/cross-domain-policy.dtd""><cross-domain-policy><site-control permitted-cross-domain-policies=""master-only""/><allow-access-from domain=""*"" to-ports=""*""/></cross-domain-policy>" + '\0'));
						
						localSo.Sock.BeginReceive(localSo.data, 0, SocketData.BYTE_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), localSo);
						return;
					}
					
					localSo.Sock.BeginReceive(localSo.data, 0, SocketData.BYTE_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), localSo);
				}
				else
				{
					localSo.WriteLine ("Policy request connection closed");
				}
			}
			catch(Exception e)
			{
				localSo.WriteLine("Disconnected: " + e.Message);
				localSo.WriteLine("Stack Trace: ");
				localSo.WriteLine(e.StackTrace);
			}
		}

		protected int i = 0;
		protected void AcceptConnection(IAsyncResult ar)
		{
			try
			{
				Socket acc = mainSock.EndAccept(ar);
				i++;
				
				SocketData so = new SocketData(acc);
				so.WriteLine("Connection accepted, waiting for data");
					
				Log.Info("Waiting for new policy request connection");
				//Console.WriteLine("Waiting for new policy request connection");

				mainSock.BeginAccept(new AsyncCallback(AcceptConnection), null);

				try
				{		
					so.Sock.BeginReceive(so.data, 0, SocketData.BYTE_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), so);
				}
				catch(Exception e)
				{
					so.Sock.Send (System.Text.ASCIIEncoding.Default.GetBytes(e.Message));
					so.WriteLine(e.Message);
				}
			}
			catch(Exception e)
			{
				Log.Info (e.Message);
				Log.Info (e.StackTrace);
				//Console.WriteLine(e.Message);
				//Console.WriteLine(e.StackTrace);
			}
		}

	}
}

