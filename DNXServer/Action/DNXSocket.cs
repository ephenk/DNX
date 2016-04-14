using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using DNXServer.Action;
using System.Data;
using System.Reflection;
using System.Collections.Specialized;

namespace DNXServer.Net
{
	public class DNXSocket
	{
		protected Socket mainSock;
		public DNXSocket ()
		{	
			Log.Info ("Loading Skill Data");
			new SkillManager ().Initialize();

			Log.Info ("Loading Sudden Death Data");
			new SuddenDeathManager ().Initialize();

			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 3697);
			Log.Info("Create new socket");
			mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			                         
			Log.Info("Bind the socket");
			mainSock.Bind(endPoint);
			
			Log.Info("Listening to socket");
			mainSock.Listen(10);
			
			Log.Info("Waiting for connection");
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

		protected void ProcessData(int x, string processedString, SessionData so)
		{
			if(x < SessionData.BYTE_SIZE)
			{	
				processedString = processedString.Trim();
				if(processedString == "")
					return;

				string bufferData = "";
				DNXMessage message = new DNXMessage(processedString);
				CommandEnum command = (CommandEnum) Convert.ToInt32(message.GetValue(0));

				// check server condition every time user send a request
				string server = new ServerStatus().StatsCheck();
				var split = server.Split ('~');
				string status_desc = split[0];
				bool isOffline = Convert.ToBoolean (split[1]);

				if(isOffline == false)
				{
					switch(command)
					{

					case CommandEnum.SendBattleData:
						bufferData = new BattleAction().LocalBattleResult(so, message[1].GetValue(0), message[1].GetValue(1), message[1].GetValue(2), message[1].GetValue(3));
						break;

					case CommandEnum.SendUserData:
						bufferData = new LoginAction().Login(so, message[1].GetValue(0), message[1].GetValue(1), message[1].GetValue(2), message[1].GetValue(3));
						break;

					case CommandEnum.PettingAction:
						bufferData = new PettingAction().PettingResult(so, message[1].GetValue(0), message[1].GetValue(1));
						break;

					case CommandEnum.DropPettingStat:
						bufferData = new DropPettingAction().DropMonsterStat(so, message[1].GetValue(0));
						break;

					case CommandEnum.UpdateMonsterEquipment:
						bufferData = new UpdateMonsterEquipment ().UpdateMonster (so, message [1].GetValue (0), message [1].GetValue (1), message [1].GetValue (2), message [1].GetValue (3), message [1].GetValue (4), message [1].GetValue (5), message [1].GetValue (6));
						break;

					case CommandEnum.RetrieveMonsterInventory:
						bufferData = new InventoryAction().GetInventory(so, message [1].GetValue(0));
						break;

					case CommandEnum.RegisterCard:
						bufferData = new CardAction().RegisterCard(so, message [1].GetValue(0));
						break;

					case CommandEnum.UnregisterCard:
						bufferData = new CardAction().UnregisterCard(so, message [1].GetValue(0));
						break;

					case CommandEnum.UpgradeMonster:
						bufferData = new CardAction().RegisterUpgradeCard(so, message [1].GetValue(0), message [1].GetValue(1));
						break;

					case CommandEnum.RetrieveUserCards:
						bufferData = new CardAction().GetUserMonsterCards(so);
						break;

					case CommandEnum.Logout:
						new LoginAction ().Logout (so);
						DeleteThisSessionFromList (so);
						break;

					case CommandEnum.CreateRoom:
						bufferData = new BattleAction ().CreateRoom (so, message [1].GetValue(0));
						break;

					case CommandEnum.RoomList:
						bufferData = new BattleAction ().GetRoomList ();
						break;

					case CommandEnum.JoinRoom:
						bufferData = new BattleAction ().JoinRoom (so, message [1].GetValue(0), message [1].GetValue(1));
						break;
					
					case CommandEnum.SetBattleItem:
						bufferData = new BattleAction ().SetPlayerItems (so, message [1].GetValue(0),message [1].GetValue(1),message [1].GetValue(2));
						break;

					case CommandEnum.BattleReady:
						if(message [1].GetValue(0)=="1")
						{
							bufferData = new BattleAction ().SetPlayerReady (so);
						}
						else if(message [1].GetValue(0)=="0")
						{
							bufferData = new BattleAction ().UnsetPlayerReady (so);
						}
						break;

					case CommandEnum.StartOnlineBattle:
						bufferData = new BattleAction ().StartOnlineBattle (so);
						break;

					case CommandEnum.BattleAction:
						bufferData = new BattleAction ().OnlineBattleAction(so, message [1].GetValue(0),message [1].GetValue(1));
						break;

					case CommandEnum.SuddenDeath:
						bufferData = new BattleAction ().SuddenDeath (so);
						break;

					case CommandEnum.VerifyPurchase:
						bufferData = new PurchaseAction ().GooglePurchase (so, message [1].GetValue (0), message [1].GetValue (1));
						break;

					case CommandEnum.RetrieveShopItems:
						bufferData = new ShopAction ().RetrieveShopItems (so);
						break;

					case CommandEnum.BuyShopItem:
						bufferData = new ShopAction ().BuyCardPacks (so, message [1].GetValue (0));
						break;

					case CommandEnum.KickOpponent:
						bufferData = new BattleAction ().KickGuest (so);
						break;

					case CommandEnum.FinishBattle:
						bufferData = new BattleAction ().OnlineBattleResult (so);
						break;

					case CommandEnum.BuyBattleSlot:
						bufferData = new ShopAction ().BuyBattleItemSlot (so);
						break;

					case CommandEnum.QuitRoom:
						bufferData = new BattleAction ().QuitRoom (so);
						break;
					
					case CommandEnum.None:
						break;

					}
				}
				else
				{
					bufferData = (int)CommandResponseEnum.Maintenance + "`" + status_desc + "`";
				}

				if(bufferData != "" || bufferData != string.Empty)
				{
					SendData(so, bufferData);
				}
			}
		}

		protected void SendData(SessionData so, string sendData)
		{
			try 
			{
				List<byte> temp = new List<byte> ();
				temp.AddRange (Encoding.ASCII.GetBytes (sendData));
				temp.Add (0x03);

				byte[] data = temp.ToArray();

				so.WriteLine(sendData);
				//Log.Info(sendData);
				so.Sock.Send(data);
			}
			catch(Exception e) 
			{
				StringBuilder str = new StringBuilder();
				str.AppendLine("\n----------------------------------\n\nUnable to send message to client because error occured at " + DateTime.Now);
				str.AppendLine("Message: " + e.Message);
				str.AppendLine("Source: " + e.Source);
				str.AppendLine("Stack Trace:");
				str.AppendLine(e.StackTrace);

				Log.Error(str.ToString());
			}
		}

		protected int i = 0;
		protected void AcceptConnection(IAsyncResult ar)
		{
			Socket acc = mainSock.EndAccept(ar);
			SessionData so = new SessionData(acc);
			try
			{
				i++;
				so.WriteLine("Connection accepted, waiting for data");
				Log.Info("Waiting for new connection");	

				mainSock.BeginAccept(new AsyncCallback(AcceptConnection), null);
				
				try
				{
					so.Sock.BeginReceive(so.data, 0, SessionData.BYTE_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), so);
				}
				catch(Exception e)
				{
					so.Sock.Send (ASCIIEncoding.Default.GetBytes(e.Message));
					so.WriteLine(e.Message);
				}
			}
			catch(Exception e)
			{
				Log.Info (e.Message);
				Log.Info (e.StackTrace);
			}
		}

		protected void ReceiveData(IAsyncResult ar)
		{
			SessionData localSo = (SessionData)ar.AsyncState;
			StringBuilder strbuild = new StringBuilder();
			try
			{
				int x = localSo.Sock.EndReceive(ar);				
				localSo.WriteLine("Receive data");
				
				if(x > 0)
				{
					localSo.WriteLine(String.Format("Data size: {0}", x));
					/*
					StringBuilder sb = new StringBuilder();
					foreach(byte b in localSo.data)
					{
						sb.Append(b.ToString("X2") + " ");
					}

					Console.WriteLine("PRE DECRYPTED:" + sb.ToString());

					MemoryStream ms = new MemoryStream(localSo.data);

					RijndaelManaged RMCrypto = new RijndaelManaged();
					RMCrypto.Padding = PaddingMode.Zeros;

					byte[] Key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
					byte[] IV = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };

					CryptoStream CryptStream = new CryptoStream(ms,RMCrypto.CreateDecryptor(Key, IV),CryptoStreamMode.Read);

					List<byte> decryptedBytes = new List<byte>();
					int decryptedByte = 0;
					while(decryptedByte != -1)
					{
						decryptedByte = CryptStream.ReadByte();
						//if ((byte)decryptedByte
						decryptedBytes.Add((byte)decryptedByte);
					}
					//Console.WriteLine("DECRYPTED MESSAGE:" + Encoding.ASCII.GetString(decryptedBytes.ToArray()));

					//Read the stream.
					//StreamReader SReader = new StreamReader(CryptStream);
					//Console.WriteLine("Decrypted:" + SReader.ReadToEnd());
					//Close the streams.
					//SReader.Close();
					ms.Close();
					localSo.data = decryptedBytes.ToArray();

					*/
					if(DNXConfig.ShowInput)
					{
						localSo.WriteLine(String.Format("Raw data: {0}", Encoding.ASCII.GetString(localSo.data).Trim ('\0')));
					}
					
					string chunk = Encoding.ASCII.GetString(localSo.data).Trim ('\0');

					localSo.WriteLine("Data received: " + chunk);
					strbuild.Append(chunk);					
					localSo.str.Append(chunk);

					for(int ii=0; ii<SessionData.BYTE_SIZE; ii++)
					{
						localSo.data[ii] = 0;
					}

					ProcessData(x, strbuild.ToString(), localSo);	

					localSo.Sock.BeginReceive(localSo.data, 0, SessionData.BYTE_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), localSo);
				}
				else
				{
					Log.Info("Connection Closed");

					DeleteThisSessionFromList(localSo);

				}
			}
			catch(Exception e)
			{
				int i = 0;
				if (File.Exists ("ERROR_ID")) {
					try {
						i = Convert.ToInt32 (File.ReadAllText ("ERROR_ID")) + 1;
					} catch {
					}
				}

				StringBuilder str = new StringBuilder();
				str.AppendLine("ERROR_ID=" + i);
				str.AppendLine("\n----------------------------------\n\nDisconnecting client because error occured at " + DateTime.Now);
				str.AppendLine("Message: " + e.Message);
				str.AppendLine("Source: " + e.Source);
				str.AppendLine("Stack Trace:");
				str.AppendLine(e.StackTrace);
				str.AppendLine("Input data: " + strbuild.ToString());
				
				Log.Error(str.ToString());

				File.WriteAllText ("ERROR_ID", i.ToString());

				if(!System.IO.Directory.Exists("Logs"))
				{
					System.IO.Directory.CreateDirectory("Logs");
				}

				DateTime now = DateTime.Now;
				File.AppendAllText("Logs/error_" + now.Year + now.Month.ToString("00") + now.Day.ToString("00"), str.ToString());

				// send error response to client 
				string error = Convert.ToString((int)CommandResponseEnum.Error) + "`" + i + "~Sorry we encounter a problem processing your data. If the problem persists, do not hesitate to contact us.`";

				List<byte> temp = new List<byte> ();
				temp.AddRange (Encoding.ASCII.GetBytes (error));
				temp.Add (0x03);

				byte[] data = temp.ToArray();

				try
				{
					localSo.Sock.Send (data);
				}
				catch(Exception)
				{
					Log.Info("Could not send Error ID, Connection to client already closed");
				}
				// remove user from user list if error occur
				DeleteThisSessionFromList (localSo);

				//localSo.Sock.BeginReceive(localSo.data, 0, SessionData.BYTE_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), localSo);
			}
			
		}

		protected void DeleteThisSessionFromList(SessionData so)
		{
			foreach (SessionData ses in LoginAction.ListUser)
			{
				if (ses == so) 
				{
					foreach(RoomData room in BattleAction.ListRoom)
					{
						if(ses == room.RoomMaster)
						{
							if(room.RoomGuest != null)
							{
								string report = (int)CommandResponseEnum.QuitRoomResult + "`" + "1" + "~" + "Room master has left the room" + "`";
								room.SendToRoomGuest(report);
								room.RoomGuest.IsRoomGuest = false;
								room.RoomGuest.RoomId = 0;
								room.RoomGuest.InBattle = false;
								room.RoomGuest = null;
							}
							BattleAction.ListRoom.Remove(room);
							break;
						}
						else if ( ses == room.RoomGuest )
						{
							if(room.RoomMaster != null)
							{
								string report = (int)CommandResponseEnum.QuitRoomResult + "`" + "2" + "~" + "Room guest has left the room" + "`";
								room.SendToRoomMaster(report);
							}
							room.RoomGuest = null;
							if(room.RoomMaster == null)
							{
								BattleAction.ListRoom.Remove(room);
							}
							break;
						}
					}
					LoginAction.ListUser.Remove(so);
					break;
				}
			}
		}

//		protected void PushMessage(object so)
//		{
//			SessionData newSo = (SessionData)so;
//			
//			int i = 0;
//			try
//			{
//				for(;;)
//				{
//					i++;
//					
//					if(newSo.Sock.Connected)
//					{
//						newSo.WriteLine("Push #" + i.ToString());
//						newSo.Sock.Send(System.Text.Encoding.ASCII.GetBytes("[\"MSG~\", \"Push " + i.ToString() + "\"]\n\0"));
//						System.Threading.Thread.Sleep(1000);
//					}
//					else
//					{
//						break;	
//					}
//				}
//			}
//			catch(SocketException)
//			{
//				newSo.WriteLine("Exception... Closing.");
//			}
//			finally
//			{
//				newSo.WriteLine("Disconnected. Push aborted");
//				System.Threading.Thread.CurrentThread.Abort();
//			}
//		}
	}
}

