using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using Mono.Security.X509;
using DNXServer.Net;
using System.Management.Instrumentation;
using System.Diagnostics;
using System.Threading;

namespace DNXServer.Action
{
	public class BattleAction: BaseAction
	{
		public BattleAction()
		{

		}
		public static List <RoomData> ListRoom = new List<RoomData>();
		public static int BattleRoomId = 1;

		public string FindRoom(SessionData so, string roomID)
		{
			if(string.IsNullOrEmpty(roomID))
			{
				roomID = Convert.ToString (so.RoomId);
			}

			bool isFindRoom = false;
			int roomIndex = 0;
			for (roomIndex = 0; roomIndex < ListRoom.Count; roomIndex++)
			{
				if (ListRoom[roomIndex].RoomId == Convert.ToInt32(roomID))
				{
					isFindRoom = true;
					break;
				}
			}
			string response = Convert.ToString (isFindRoom) + "~" + Convert.ToString (roomIndex);
			return response;
		}

		public bool CheckMonster(SessionData so, string monsterQRID)
		{
			StringDictionary check;
			using (Command (@"
				SELECT user_id,version FROM monster_cards
				WHERE mons_card_qrid = :mons
			")) 
			{
				AddParameter("mons", monsterQRID);
				check = ExecuteRow();
			}

			if (check==null || Convert.ToInt32 (check ["user_id"]) != so.UserId || Convert.ToInt32(check ["version"]) <= 1) 
			{
				return false;
			}
			return true;
		}			

		public bool CheckItem(SessionData so, string item1 , string item2, string item3)
		{
			string[] item = new string[3]{item1,item2,item3};
			StringDictionary battleItem;
			bool isValid = true;
			foreach (string row in item) {
				if(row != string.Empty)
				{
					using (Command (@"
						SELECT user_id FROM item_cards
						WHERE item_card_qrid = :item
					")) 
					{
						AddParameter ("item", row);
						battleItem = ExecuteRow();
					}

					if(battleItem == null || Convert.ToInt32(battleItem["user_id"])!=so.UserId)
					{
						isValid = false;
						break;
					}
				}
			}
			return isValid;
		}

		public string CreateRoom(SessionData so , string monsterQRID)
		{
			// user who create room is treated as Room Master, they always join the room if the process is successful
			bool isCreated = false;
				for (int roomIndex = 0; roomIndex < ListRoom.Count; roomIndex++) {
					if (ListRoom [roomIndex].RoomMaster.UserId == so.UserId) {
						isCreated = true;
						//return error
						return (int)CommandResponseEnum.CreateRoomResult + "`" + "-1" + "~" + "Room already been created by this user" + "`";						
					}
				}

			//check monster validity
			bool isValidMonster = CheckMonster(so,monsterQRID);

			if (isValidMonster == false) {
				return (int)CommandResponseEnum.CreateRoomResult + "`" + "-2" + "~" + "Monster is not valid" + "`";		
			}

			//create the room
			if (isCreated == false) {
				if(so.IsRoomGuest == false && so.RoomId == 0)
				{
					RoomData room = new RoomData(so,BattleRoomId);
					BattleRoomId++;
//					if (roomPassword != string.Empty) {
//						room.RoomPassword = roomPassword;
//					} else {
//						room.RoomPassword = string.Empty;
//					}
					ListRoom.Add (room);
					// make the player join the room that just created
					for (int roomIndex = 0; roomIndex < ListRoom.Count; roomIndex++) {
						if (ListRoom [roomIndex].RoomMaster.UserId == so.UserId) {
							ListRoom [roomIndex].JoinRoom(so,monsterQRID);
							return (int)CommandResponseEnum.CreateRoomResult + "`" +"1"+"~"+ "Create room successfull" + "`";
						}
					}
				}
				else
				{
					return (int)CommandResponseEnum.CreateRoomResult + "`" +"-3"+"~"+ "Create room failed, you already join a room" + "`";
				}

			}
			return (int)CommandResponseEnum.CreateRoomResult + "`" +"-4"+"~"+ "Create room failed" + "`";
		}

		public string GetRoomList()
		{
			//if (ListRoom.Count != 0) {
				StringBuilder sb = new StringBuilder ();
				for (int j = 0; j < ListRoom.Count; j++) {
					// dont show
					if(ListRoom[j].IsBattle == false && ListRoom[j].RoomGuest == null)
					{
						sb.Append (ListRoom [j].RoomId);
						sb.Append ("~");
						sb.Append (ListRoom [j].monsterRM.MonsterTemplate);
						sb.Append ("~");
						sb.Append (ListRoom [j].monsterRM.MonsterTemplateName);
						sb.Append ("~");
						sb.Append (ListRoom [j].monsterRM.Version);
						sb.Append ("~");
						sb.Append (ListRoom [j].monsterRM.Subversion);
						sb.Append ("~");
						sb.Append (ListRoom [j].monsterRM.Win);
						sb.Append ("~");
						sb.Append (ListRoom [j].monsterRM.Lose);
						sb.Append ("~");
						sb.Append (ListRoom [j].RoomMaster.Flee);
						sb.Append ("`");
					}
				}
				if(sb.Length >0)
				{
					sb.Length--;
				}
				return (int)CommandResponseEnum.RoomListResult + "`" + sb.ToString () + "`";
//			} else {
//				return (int)CommandResponseEnum.RoomListResult + "``";
//			}
		}

		public string JoinRoom(SessionData so, string roomId,string monsterQRID)
		{
//			int roomNumber = Convert.ToInt32(roomId);
//			bool isFindRoom = false;
//			int roomIndex = 0;
//
//			for (roomIndex = 0; roomIndex < ListRoom.Count; roomIndex++)
//			{
//				if (ListRoom[roomIndex].RoomId == roomNumber)
//				{
//					isFindRoom = true;
//					break;
//				}
//			}
			// ambil user picture FB
			string checkRoom = FindRoom (so, roomId);
			var split = checkRoom.Split ('~');
			bool isFindRoom = Convert.ToBoolean(split[0]);
			int roomIndex = Convert.ToInt32 (split [1]);

			// check monster validity
			bool isValidMonster = CheckMonster (so, monsterQRID);

			if (!isValidMonster) {
				return (int)CommandResponseEnum.JoinRoomResult + "`" + "-1" + "~" + "Monster is not valid" + "`";		
			}

			if (isFindRoom)
			{
				if (ListRoom[roomIndex].RoomGuest == null || ListRoom[roomIndex].RoomMaster == null)
				{
//					if (ListRoom[roomIndex].IsPassworded)
//					{
//						if (ListRoom[roomIndex].RoomPassword == roomPassword)
//						{
//							if (so.RoomId == 0) {
//								so.RoomId = ListRoom [roomIndex].RoomId;
//								if(so.RoomMaster == true)
//								{
//									ListRoom[roomIndex].JoinRoom(so,monsterQRID);
//									return (int)CommandResponseEnum.JoinRoomResult + "`" + "1" +"~"+ "Join Room successfull" + "`";
//								}
//								else
//								{
//									ListRoom[roomIndex].JoinRoom(so,monsterQRID);
//									//string monsRM = ListRoom[roomIndex].JoinRoom(so,monsterQRID);
//									//return (int)CommandResponseEnum.JoinRoomResult + "`" + "1" +"~"+ "Join Room successfull" + "~" + monsRM + "`";
//								}
//							} 
//						}
//						else
//						{
//							return (int)CommandResponseEnum.JoinRoomResult + "`" + "-3"+ "~" + "Wrong room password" + "`";
//						}
//					}
//					else
//					{
						if (so.RoomId == 0) {
							so.RoomId = ListRoom [roomIndex].RoomId;
							if(so.IsRoomMaster == true && so.UserId == ListRoom[roomIndex].RoomMaster.UserId)
							{
								ListRoom[roomIndex].JoinRoom(so,monsterQRID);
								//return (int)CommandResponseEnum.JoinRoomResult + "`" + "1" +"~"+ "Join Room successfull" + "`";
							}
							else if(so.IsRoomMaster == true && so.UserId != ListRoom[roomIndex].RoomMaster.UserId)
							{
							return (int)CommandResponseEnum.JoinRoomResult + "`" + "-4" + "You cannot join another room because you already create a room" + "`";
							}
							else
							{
								ListRoom[roomIndex].JoinRoom(so,monsterQRID);								
							}
						}
					//}
				}
				else
				{
					return (int)CommandResponseEnum.JoinRoomResult + "`" + "-2" +"~"+ "Room is full" + "`" + "`" + "`";
				}
			}
			else if(isFindRoom == false)
			{
				return (int)CommandResponseEnum.JoinRoomResult + "`" + "-3" +"~"+ "Room not found or room master has left the room" + "`" + "`" + "`";
			}
			return string.Empty;
		}

		public string SetPlayerItems(SessionData so, string item1, string item2, string item3)
		{
			string checkRoom = FindRoom (so, null);
			var split = checkRoom.Split ('~');
			bool isFindRoom = Convert.ToBoolean(split[0]);
			int roomIndex = Convert.ToInt32 (split [1]);
			int roomId = so.RoomId;
			bool isValidItem = CheckItem (so, item1, item2, item3);

			if(isValidItem == false)
			{
				return (int)CommandResponseEnum.SetBattleItemResult + "`" + "-1" + "~" + "Item is not valid" + "`";		
			}

			if (isFindRoom) {
				ListRoom [roomIndex].SetBattleItem (so, item1, item2, item3);
				return string.Empty;
				//return (int)CommandResponseEnum.SetBattleItemResult + "`" + "1" +"~"+ "Item is set" + "`";
			} 
			else{
				//return error
				return (int)CommandResponseEnum.SetBattleItemResult + "`" + "-3" +"~"+ "Room not found" + "`";	
			}					
		}

		public string SetPlayerReady(SessionData so)
		{
			string checkRoom = FindRoom (so, null);
			var split = checkRoom.Split ('~');
			bool isFindRoom = Convert.ToBoolean(split[0]);
			int roomIndex = Convert.ToInt32 (split [1]);
			int roomId = so.RoomId;

			if (isFindRoom) {
				if (so.UserId == ListRoom [roomIndex].RoomMaster.UserId) {
					//ListRoom [roomIndex].SetPlayerReady (so);
					return (int)CommandResponseEnum.BattleReadyResult + "`" + "-2" +"~"+ "You are the room master" + "`";
				} else if (so.UserId == ListRoom [roomIndex].RoomGuest.UserId) {
					ListRoom [roomIndex].SetPlayerReady (so);
					return string.Empty;
					//return (int)CommandResponseEnum.BattleReadyResult + "`" + "1" +"~"+ "Room Guest is ready" + "`";
				} else {
					//return error
					return (int)CommandResponseEnum.BattleReadyResult + "`" + "-3" + "~" + "You are not joining a room" + "`";
				}
			} else {
				//return error
				return (int)CommandResponseEnum.BattleReadyResult + "`" + "-4" +"~"+ "Room not found" + "`";
			}
		}

		public string UnsetPlayerReady(SessionData so)
		{
			string checkRoom = FindRoom (so, null);
			var split = checkRoom.Split ('~');
			bool isFindRoom = Convert.ToBoolean(split[0]);
			int roomIndex = Convert.ToInt32 (split [1]);
			int roomId = so.RoomId;

			if (isFindRoom) {
				if (so.UserId == ListRoom [roomIndex].RoomMaster.UserId) {
					//ListRoom [roomIndex].UnsetPlayerReady (so);
					return (int)CommandResponseEnum.BattleReadyResult + "`" + "-2" +"~"+ "You are the room master" + "`";
				} else if (so.UserId == ListRoom [roomIndex].RoomGuest.UserId) {
					ListRoom [roomIndex].UnsetPlayerReady (so);
					return string.Empty;
					//return (int)CommandResponseEnum.BattleReadyResult + "`" + "-1" +"~"+ "Room Guest is unready" + "`";
				} else {
					//return error
					return (int)CommandResponseEnum.BattleReadyResult + "`" + "-3" + "~" + "You are not joining a room" + "`";
				}
			} else {
				//return error
				return (int)CommandResponseEnum.BattleReadyResult + "`" + "-4" +"~"+ "Room not found" + "`";
			}
		}

		public string KickGuest(SessionData so)
		{
			string checkRoom = FindRoom (so, null);
			var split = checkRoom.Split ('~');
			bool isFindRoom = Convert.ToBoolean(split[0]);
			int roomIndex = Convert.ToInt32 (split [1]);
			int roomId = so.RoomId;

			if (isFindRoom) 
			{
				if (so.UserId == ListRoom [roomIndex].RoomMaster.UserId) {
					ListRoom [roomIndex].KickGuest (so);
				}
			}
			return string.Empty;
		}

		public string StartOnlineBattle(SessionData so)
		{
			string checkRoom = FindRoom (so, null);
			var split = checkRoom.Split ('~');
			bool isFindRoom = Convert.ToBoolean(split[0]);
			int roomIndex = Convert.ToInt32 (split [1]);
			int roomId = so.RoomId;

			if (isFindRoom) 
			{
				if (so.UserId == ListRoom [roomIndex].RoomMaster.UserId) 
				{
					ListRoom [roomIndex].StartBattle ();
					return string.Empty;
				} 
				else if (so.UserId == ListRoom [roomIndex].RoomGuest.UserId) 
				{
					return (int)CommandResponseEnum.StartOnlineBattleResult + "`" + "-2" +"~"+ "You are the Room Guest" + "`";
				} 
				else 
				{
					return (int)CommandResponseEnum.StartOnlineBattleResult + "`" + "-3" + "~" + "You are not joining a room" + "`";
				}
			}
			else
			{
				return (int)CommandResponseEnum.StartOnlineBattleResult + "`" + "-4" + "Room not found";
			}
		}

		public string OnlineBattleAction(SessionData so, string action , string item)
		{
			string checkRoom = FindRoom (so, null);
			var split = checkRoom.Split ('~');
			bool isFindRoom = Convert.ToBoolean(split[0]);
			int roomIndex = Convert.ToInt32 (split [1]);
			int roomId = so.RoomId;

			if (isFindRoom) 
			{
				ListRoom [roomIndex].SetBattleAction (so,Convert.ToInt32(action),item);			
			}
			else 
			{
				return (int)CommandResponseEnum.SendBattleActionResult + "`" + "-1" +"~"+ "Room not found" + "`";
			}
			return string.Empty;
		}

		public string SuddenDeath(SessionData so)
		{
			string checkRoom = FindRoom (so, null);
			var split = checkRoom.Split ('~');
			bool isFindRoom = Convert.ToBoolean(split[0]);
			int roomIndex = Convert.ToInt32 (split [1]);
			int roomId = so.RoomId;

			if (isFindRoom) 
			{
				if (so.UserId == ListRoom [roomIndex].RoomMaster.UserId || so.UserId == ListRoom[roomIndex].RoomGuest.UserId) {
					ListRoom [roomIndex].SendSuddenDeathResult (so);
				} 
				else 
				{
					return (int)CommandResponseEnum.SendBattleActionResult + "`" + "-2" + "~" + "You are not joining a room" + "`";
				}
			}
			else 
			{
				return (int)CommandResponseEnum.SendBattleActionResult + "`" + "-1" +"~"+ "Room not found" + "`";
			}

			return string.Empty;
		}

		public string QuitRoom(SessionData so)
		{
			int roomId = so.RoomId;
			bool isFindRoom = false;
			int roomIndex = 0;
			for (roomIndex = 0; roomIndex < ListRoom.Count; roomIndex++)
			{
				if (ListRoom[roomIndex].RoomId == roomId)
				{
					isFindRoom = true;
					break;
				}
			}

			if (isFindRoom)
			{
				if (so.UserId == ListRoom [roomIndex].RoomMaster.UserId || so.UserId == ListRoom [roomIndex].RoomGuest.UserId) 
				{
					ListRoom [roomIndex].QuitRoom (so);
				}
				else
				{
					return (int)CommandResponseEnum.QuitRoomResult + "`" + "-1" + "~" + "You are not joining any room" + "`";
				}
			}
			else
			{
				return (int)CommandResponseEnum.QuitRoomResult + "`" + "-2" + "~" + "Room not found" + "`";
			}

			return string.Empty;
		}

		public string OnlineBattleResult(SessionData so)
		{
			string response = string.Empty;
			int roomId = so.RoomId;
			bool isFindRoom = false;
			int roomIndex = 0;
			for (roomIndex = 0; roomIndex < ListRoom.Count; roomIndex++)
			{
				if (ListRoom[roomIndex].RoomId == roomId)
				{
					isFindRoom = true;
					break;
				}
			}

			if (isFindRoom)
			{
				if (so.UserId == ListRoom[roomIndex].RoomMaster.UserId || so.UserId == ListRoom[roomIndex].RoomGuest.UserId)
				{
					ListRoom[roomIndex].BattleResult(so);
				}
				else
				{
					return (int)CommandResponseEnum.QuitRoomResult + "`" + "-1" + "~" + "You are not joining any room" + "`";
				}
			}
			else
			{
				return (int)CommandResponseEnum.QuitRoomResult + "`" + "-2" + "~" + "Room not found" + "`";
			}

			return string.Empty;
		}

		public string LocalBattleResult(SessionData so, string winMons, string loseMons, string winItems, string loseItems)
		{
			// WinMonsterQR~LoseMonsterQR~WinItemQR!WinItemQR!WinItemQR!~LoseItemQR!LoseItemQR!LoseItemQR!

			string[] winItem = winItems.Split(new char[] { '!' });
			string[] loseItem = loseItems.Split(new char[] { '!' });
			string loser;
			string winner;
			string winnerID;
			string loserID;
			int winCoin = 100;
			int loseCoin = 25;
			StringDictionary winCollection;
			StringDictionary loseCollection;

			using(Command(@"
				SELECT user_id, hunger, happiness, clean, discipline, sick, exp_needed, exp, exp_mult, version, subversion,
					hp, mp, p_atk, m_atk, p_def, m_def, acc, eva, mons_card_id
				 FROM monster_cards WHERE mons_card_qrid = :win
			"))
			{
				AddParameter("win", winMons);
				winCollection = ExecuteRow();
				winner = winCollection["user_id"];
				winnerID = winCollection["mons_card_id"];
			}

			using(Command(@"
				SELECT user_id, hunger, happiness, clean, discipline, sick, exp_needed, exp, exp_mult, version, subversion,
					hp, mp, p_atk, m_atk, p_def, m_def, acc, eva, mons_card_id
				FROM monster_cards WHERE mons_card_qrid = :lose
			"))
			{
				AddParameter("lose", loseMons);
				loseCollection = ExecuteRow();
				loser = loseCollection["user_id"];
				loserID = loseCollection["mons_card_id"];
			}
			string battleId;
			using(Command(@"
				INSERT INTO battle_log(win_user_id, win_mons_id, lose_user_id, lose_mons_id, time)
				VALUES (:win, :winmons, :lose, :losemons, CURRENT_TIMESTAMP) RETURNING battle_id as bid
			"))
			{
				AddParameter("win", winner);
				AddParameter("winmons", winnerID);
				AddParameter("lose", loser);
				AddParameter("losemons", loserID);				
				battleId = ExecuteRow()["bid"];
			}

			// give coins to player
			using(Command(@"
				UPDATE user_list SET mobile_coins = mobile_coins + :win_coin 
				WHERE user_id = :winner
			"))
			{
				AddParameter ("win_coin", winCoin);
				AddParameter ("winner", winner);

				ExecuteRow() ;
			}

			using(Command(@"
				UPDATE user_list SET mobile_coins = mobile_coins + :lose_coin 
				WHERE user_id = :loser
			"))
			{
				AddParameter ("lose_coin", loseCoin);
				AddParameter ("loser", loser);
				ExecuteRow ();
			}

			// exp point calculation
			const int baseWinXP = 1000;
			const int baseLoseXP = 250;
			double winMult = (double)(100 + Convert.ToInt32(winCollection["hunger"]) +
				Convert.ToInt32(winCollection["happiness"]) + Convert.ToInt32(winCollection["clean"]) +
				Convert.ToInt32(winCollection["discipline"]) + Convert.ToInt32(winCollection["sick"])) / 300.0F;
			
			double loseMult = (double)(100 + Convert.ToInt32(winCollection["hunger"]) +
				Convert.ToInt32(winCollection["happiness"]) + Convert.ToInt32(winCollection["clean"]) +
				Convert.ToInt32(winCollection["discipline"]) + Convert.ToInt32(winCollection["sick"])) / 300.0F;
			
			int winXP = (int)(baseWinXP * winMult);
			int loseXP = (int)(baseLoseXP * loseMult);

			bool winLvlUp = false;
			bool loseLvlUp = false;

			int nextWinNeededXP = 0;
			int nextLoseNeededXP = 0;

			int winVersion = 0;
			int winSubversion = 0;
			int loseVersion = 0;
			int loseSubversion = 0;

			int currWinXP = winXP + Convert.ToInt32(winCollection["exp"]);
			if(currWinXP >= Convert.ToInt32(winCollection["exp_needed"]))
			{
				currWinXP = currWinXP - Convert.ToInt32(winCollection["exp_needed"]);
				winLvlUp = true;
				
				int nextLvlWin = Convert.ToInt32(winCollection["version"]) * 10 + Convert.ToInt32(winCollection["subversion"]) + 1;
				winVersion = Convert.ToInt32(nextLvlWin.ToString().Substring(0, 1));
				winSubversion = Convert.ToInt32(nextLvlWin.ToString().Substring(1, 1));

				if(nextLvlWin < 30)
				{
					using(Command(@"
						SELECT level, exp
						FROM base_exp_req
						WHERE level = :lvl
					"))
					{
						AddParameter("lvl", nextLvlWin);
						nextWinNeededXP = Convert.ToInt32(ExecuteRow()["exp"]);
					}
				}
			}

			int currLoseXP = loseXP + Convert.ToInt32(loseCollection["exp"]);

			if(currLoseXP >= Convert.ToInt32(loseCollection["exp_needed"]))
			{
				currLoseXP = currLoseXP - Convert.ToInt32(loseCollection["exp_needed"]);
				loseLvlUp = true;
				
				int nextLvlLose = Convert.ToInt32(loseCollection["version"]) * 10 + Convert.ToInt32(loseCollection["subversion"]) + 1;
				loseVersion = Convert.ToInt32(nextLvlLose.ToString().Substring(0, 1));
				loseSubversion = Convert.ToInt32(nextLvlLose.ToString().Substring(1,1));

				if(nextLvlLose < 30)
				{
					using(Command(@"
						SELECT level, exp
						FROM base_exp_req
						WHERE level = :lvl
					"))
					{
						AddParameter("lvl", nextLvlLose);
						nextLoseNeededXP = Convert.ToInt32(ExecuteRow()["exp"]);
					}
				}
			}
						
			byte[] evaqriWin = new byte[2];
			new Random().NextBytes(evaqriWin);
			
			int evaWin = evaqriWin[0] < 15 ? 1 : 0;
			int criWin = evaqriWin[1] < 15 ? 1 : 0;
			
			byte[] evaqriLose = new byte[2];
			new Random().NextBytes(evaqriLose);
			
			int evaLose = evaqriLose[0] < 15 ? 1 : 0;
			int criLose = evaqriLose[1] < 15 ? 1 : 0;

			// update winner stats
			if(winLvlUp)
			{
				using(Command(@"
					UPDATE monster_cards
					SET wins = wins + 1,
						hp = hp + 20 + round(random() * 5), -- hp = hp + random(20, 25)
						hp_regen = hp_regen + 1,
						p_atk = p_atk + 1 + round(random() * 2), -- p_atk = p_atk + random(1, 3)
						m_atk = m_atk + 1 + round(random() * 2), -- m_atk = m_atk + random(1, 3)
						p_def = p_def + 1 + round(random() * 2), -- p_def = p_def + random(1, 3)
						m_def = m_def + 1 + round(random() * 2), -- m_def = m_def + random(1, 3)
						acc = acc + 1 + round(random()), -- acc = acc + random(1, 2)
						eva = eva + :eva,
						cri = cri + :cri,
						spd = spd + 1 + round(random()), -- spd = spd + random(1, 2)
						version =:version,
						subversion =:subversion,
						exp = :newexp,
						exp_needed = :newexpneeded,
						hunger = greatest(hunger - round(random() * 25), 0),
						happiness = greatest(happiness - round(random() * 25), 0),
						clean = greatest(clean - round(random() * 25), 0),
						discipline = greatest(discipline - round(random() * 25), 0),
						sick = greatest(sick - round(random() * 25), 0)
					WHERE mons_card_qrid = :winmons
				"))
				{
					AddParameter("winmons", winMons);
					AddParameter("eva", evaWin);
					AddParameter("cri", criWin);
					AddParameter ("version", winVersion);
					AddParameter ("subversion", winSubversion);
					AddParameter("newexpneeded", nextWinNeededXP);
					AddParameter("newexp", currWinXP);
					ExecuteWrite();
				}
			}
			else
			{
				using(Command(@"
					UPDATE monster_cards
					SET wins = wins + 1,
						exp = :newexp,
						hunger = greatest(hunger - round(random() * 25), 0),
						happiness = greatest(happiness - round(random() * 25), 0),
						clean = greatest(clean - round(random() * 25), 0),
						discipline = greatest(discipline - round(random() * 25), 0),
						sick = greatest(sick - round(random() * 25), 0)
					WHERE mons_card_qrid = :winmons
				"))
				{
					AddParameter("winmons", winMons);
					AddParameter("newexp", currWinXP);
					ExecuteWrite();
				}
			}

			// update loser stats
			if(loseLvlUp)
			{
				using(Command(@"
					UPDATE monster_cards
					SET loses = loses + 1,
						hp = hp + 20 + round(random() * 5), -- hp = hp + random(20, 25)
						hp_regen = hp_regen + 1,
						p_atk = p_atk + 1 + round(random() * 2), -- p_atk = p_atk + random(1, 3)
						m_atk = m_atk + 1 + round(random() * 2), -- m_atk = m_atk + random(1, 3)
						p_def = p_def + 1 + round(random() * 2), -- p_def = p_def + random(1, 3)
						m_def = m_def + 1 + round(random() * 2), -- m_def = m_def + random(1, 3)
						acc = acc + 1 + round(random()), -- acc = acc + random(1, 2)
						eva = eva + :eva,
						cri = cri + :cri,
						spd = spd + 1 + round(random()), -- spd = spd + random(1, 2)
						version =:version,
						subversion =:subversion,
						exp = :newexp,
						exp_needed = :newexpneeded,
						hunger = greatest(hunger - round(random() * 25), 0),
						happiness = greatest(happiness - round(random() * 25), 0),
						clean = greatest(clean - round(random() * 25), 0),
						discipline = greatest(discipline - round(random() * 25), 0),
						sick = greatest(sick - round(random() * 25), 0)
					WHERE mons_card_qrid = :losemons
				"))
				{
					AddParameter("losemons", loseMons);
					AddParameter("eva", evaLose);
					AddParameter("cri", criLose);
					AddParameter ("version", loseVersion);
					AddParameter ("subversion", loseSubversion);
					AddParameter("newexpneeded", nextLoseNeededXP);
					AddParameter("newexp", currLoseXP);
					ExecuteWrite();
				}
			}
			else
			{
				using(Command(@"
					UPDATE monster_cards
					SET loses = loses + 1,
						exp = :newexp,
						hunger = greatest(hunger - round(random() * 25), 0),
						happiness = greatest(happiness - round(random() * 25), 0),
						clean = greatest(clean - round(random() * 25), 0),
						discipline = greatest(discipline - round(random() * 25), 0),
						sick = greatest(sick - round(random() * 25), 0)
					WHERE mons_card_qrid = :losemons
				"))
				{
					AddParameter("losemons", loseMons);
					AddParameter("newexp", currLoseXP);
					ExecuteWrite();
				}
			}


			// update item quantity
			foreach(string row in winItem)
			{
				using(Command(@"
					UPDATE item_cards
					SET qty = qty - 1
					WHERE item_card_qrid = :item
				"))
				{
					AddParameter("item", row);
					
					ExecuteWrite();
				}

				using(Command(@"
					INSERT INTO battle_log_detail(battle_id, user_id, mons_id, item_id)
				    SELECT :battle, :winner, :winmons, item_card_id
					FROM item_cards
					WHERE item_card_qrid = :item
				"))
				{
					AddParameter("battle", battleId);
					AddParameter("winner", winner);
					AddParameter("winmons", winnerID);
					AddParameter("item", row);
					ExecuteWrite();
				}
			}

			foreach(string row in loseItem)
			{
				using(Command(@"
					UPDATE item_cards
					SET qty = qty - 1
					WHERE item_card_qrid = :item
				"))
				{
					AddParameter("item", row);
					
					ExecuteWrite();
				}
				
				using(Command(@"
					INSERT INTO battle_log_detail(battle_id, user_id, mons_id, item_id)
				    SELECT :battle, :loser, :losemons, item_card_id
					FROM item_cards
					WHERE item_card_qrid = :item
				"))
				{
					AddParameter("battle", battleId);
					AddParameter("loser", loser);
					AddParameter("losemons", loserID);
					AddParameter("item", row);
					ExecuteWrite();
				}
			}	

			//MonsterQRID~EXP`MonsterQRID~EXP`
			StringDictionary newWin = new StringDictionary();
			StringDictionary newLose = new StringDictionary();

			using(Command(@"
				SELECT hp, mp, version, subversion, p_atk, m_atk, p_def, m_def, acc, eva
				FROM monster_cards
				WHERE mons_card_qrid = :mons
			"))
			{
				AddParameter("mons", winMons);
				newWin = ExecuteRow();
			}

			using(Command(@"
				SELECT hp, mp, version, subversion, p_atk, m_atk, p_def, m_def, acc, eva
				FROM monster_cards
				WHERE mons_card_qrid = :mons
			"))
			{
				AddParameter("mons", loseMons);
				newLose = ExecuteRow();
			}

			string winString = winMons + "~" + winXP;
			if(winLvlUp)
			{
				int versionNew = Convert.ToInt32 (newWin ["version"]);
				int subversionNew = Convert.ToInt32 (newWin ["subversion"]);

//				int atkOld = Convert.ToInt32(winCollection["p_atk"]) + Convert.ToInt32(winCollection["m_atk"]);
				int atkNew = Convert.ToInt32(newWin["p_atk"]) + Convert.ToInt32(newWin["m_atk"]);
				//int atkDelta = (atkNew - atkOld) / 2;
				
//				int defOld = Convert.ToInt32(winCollection["p_def"]) + Convert.ToInt32(winCollection["m_def"]);
				int defNew = Convert.ToInt32(newWin["p_def"]) + Convert.ToInt32(newWin["m_def"]);
//				int defDelta = (defNew - defOld) / 2;

//				int accDelta = Convert.ToInt32(newWin["acc"]) - Convert.ToInt32(winCollection["acc"]);
//				int evaDelta = Convert.ToInt32(newWin["eva"]) - Convert.ToInt32(winCollection["eva"]); 

				int accNew = Convert.ToInt32 (newWin ["acc"]);
				int evaNew = Convert.ToInt32 (newWin ["eva"]);

				winString += "~" + versionNew + "~" + subversionNew + "~" + atkNew + "~" + defNew + "~" + accNew + "~" + evaNew;
			}

			string loseString = loseMons + "~" + loseXP;
			if(loseLvlUp)
			{
				int versionNew = Convert.ToInt32 (newLose ["version"]);
				int subversionNew = Convert.ToInt32 (newLose ["subversion"]);

//				int atkOld = Convert.ToInt32(loseCollection["p_atk"]) + Convert.ToInt32(loseCollection["m_atk"]);
				int atkNew = Convert.ToInt32(newLose["p_atk"]) + Convert.ToInt32(newLose["m_atk"]);
//				int atkDelta = (atkNew - atkOld) / 2;
				
//				int defOld = Convert.ToInt32(loseCollection["p_def"]) + Convert.ToInt32(loseCollection["m_def"]);
				int defNew = Convert.ToInt32(newLose["p_def"]) + Convert.ToInt32(newLose["m_def"]);
//				int defDelta = (defNew - defOld) / 2;
				
//				int accDelta = Convert.ToInt32(newLose["acc"]) - Convert.ToInt32(loseCollection["acc"]);
//				int evaDelta = Convert.ToInt32(newLose["eva"]) - Convert.ToInt32(loseCollection["eva"]); 

				int accNew = Convert.ToInt32 (newLose ["acc"]);
				int evaNew = Convert.ToInt32 (newLose ["eva"]);

				loseString += "~" + versionNew + "~" + subversionNew + "~" + atkNew + "~" + defNew + "~" + accNew + "~" + evaNew;
			}

			string winStats = new CardAction ().GetMonsterStat (winMons);
			string loseStats = new CardAction ().GetMonsterStat (loseMons);

			//MonsterQRID~EXP~ATK~DEF~ACC~EVA~`MonsterQRID~EXP~ATK~DEF~ACC~EVA~`
			return (int)CommandResponseEnum.BattleResult + "`" + winString + "~" + winCoin + "`" + loseString + "~" + loseCoin + "`" + winStats + "`" + loseStats + "`";
		}

	}
}

