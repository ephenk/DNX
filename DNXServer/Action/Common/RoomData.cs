using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace DNXServer.Action
{

	public class RoomData : BaseAction
	{
		public SessionData RoomMaster { get; set; }
		public SessionData RoomGuest { get; set; }
		public BattleMonsterData monsterRM { get; set;}
		public BattleMonsterData monsterRG { get; set;}

		public bool IsBattle { get; set; }
		public bool RoomMasterIsReady { get; set;}
		public bool RoomGuestIsReady { get; set; }
		public bool isProcessing = false;
		public int RoomId { get; set; }
		public int skillRM = -3; // klo ga pake skill -2, klo paralyzed -1, default -3
		public int skillRG = -3;
		public int skillRMIndex = -1;
		public int skillRGIndex = -1;
		public int itemRM = -2;
		public int itemRG = -2;
		public int maxRMHP = 0;
		public int maxRGHP = 0;
		public int maxRMMP = 0;
		public int maxRGMP = 0;
		public int suddenDeathRM = -1;
		public int suddenDeathRG = -1;
		public int round;
		public int poisonedRMTurn;
		public int poisonedRGTurn;
		public int paralyzedRMTurn;
		public int paralyzedRGTurn;
		public int stunnedRMTurn;
		public int stunnedRGTurn;
		public string usedItemRM = string.Empty;
		public string usedItemRG = string.Empty;
		public string[] listSkillRM;
		public string[] listSkillRG;
		public string resultBattleRM = string.Empty;
		public string resultBattleRG = string.Empty;
	
		public RoomData(SessionData so, int roomNumber)
		{
			this.IsBattle = false;
			this.RoomGuestIsReady = false;
			so.IsRoomMaster = true;
			this.RoomMaster = so;
			this.RoomId = roomNumber;
		}

		public void JoinRoom(SessionData so, string monsterQRID)
		{
			if (so.IsRoomMaster)
			{
				this.RoomMaster = so;
				so.RoomId = this.RoomId;
				LoadMonsterData (so,monsterQRID);
			}
			else if (this.RoomGuest == null)
			{
				//message to room master
				so.IsRoomGuest = true;
				this.RoomGuest = so;
				so.RoomId = this.RoomId;
				LoadMonsterData (so,monsterQRID);

				// send guest monster stat to room master
				string monsRG = (int)CommandResponseEnum.JoinRoomResult + "`" + "2" + "~" + "Guest join the room" + "`" + RoomGuest.FacebookPicture + "`"+ monsterRG.GetMonsterData() + "~" + RoomGuest.Flee + "`";
				SendToRoomMaster (monsRG);

				// send room master monster to guest
				string monsRM = (int)CommandResponseEnum.JoinRoomResult + "`" + "1" +"~"+ "Join Room successfull" + "`" + RoomMaster.FacebookPicture + "`" + monsterRM.GetMonsterData() + "~" + RoomMaster.Flee + "`";
				SendToRoomGuest (monsRM);
			}
		}

		public void LoadMonsterData(SessionData so,string monsterQRID)
		{
			string monsterStat = string.Empty;
			if(so.IsRoomMaster)
			{
				//assign monster to room master battle monster
				this.monsterRM = new BattleMonsterData (monsterQRID);
				this.maxRMHP = monsterRM.HP;
				this.maxRMMP = monsterRM.MP;
				listSkillRM = monsterRM.GetListSkill ();
			}
			else if (so.IsRoomGuest)
			{
				//assign monster to room guest battle monster
				this.monsterRG = new BattleMonsterData (monsterQRID);
				this.maxRGHP = monsterRG.HP;
				this.maxRGMP = monsterRG.MP;
				listSkillRG = monsterRG.GetListSkill ();
			}
		}

		public void SetBattleItem(SessionData so ,string item1 ,string item2 ,string item3)
		{
			if(so.IsRoomMaster)
			{
				int result = this.monsterRM.SetBattleItems (item1 ,item2 ,item3);
				if(result == 1 && this.RoomGuest != null)
				{
					string items = this.monsterRM.GetItemData ();
					string sendData = (int)CommandResponseEnum.SetBattleItemResult + "`" + items + "`";
					SendToRoomGuest (sendData);
				}
			}
			else if (so.IsRoomGuest)
			{
				int result = this.monsterRG.SetBattleItems (item1 ,item2 ,item3);
				if(result == 1 && this.RoomMaster != null)
				{
					string items = this.monsterRG.GetItemData ();
					string sendData = (int)CommandResponseEnum.SetBattleItemResult + "`" + items + "`";
					SendToRoomMaster (sendData);
				}
			}
		}

		public void KickGuest(SessionData so)
		{
			if(so.IsRoomMaster)
			{
				if(!so.InBattle)
				{
					string sendData = (int)CommandResponseEnum.KickOpponentResult + "`";
					SendToRoomGuest (sendData);
					RoomGuest.IsRoomGuest = false;
					RoomGuest.RoomId = 0;
					RoomGuest.InBattle = false;
					this.RoomGuest = null;
				}
			}
		}

		public void QuitRoom(SessionData so)
		{
			if(so.InBattle)
			{
				using(Command("UPDATE user_list SET flee = flee + 1 WHERE user_id = :id"))
				{
					AddParameter ("id",so.UserId);
					ExecuteRow ();
				}
			}

			if (so.IsRoomMaster)
			{
				if(this.RoomGuest != null)
				{
					string sendData = (int)CommandResponseEnum.QuitRoomResult + "`" + "1" + "~" + "Room master has left the room" + "`";
					SendToRoomGuest (sendData);

					RoomGuest.IsRoomGuest = false;
					RoomGuest.RoomId = 0;
					RoomGuest.InBattle = false;
					this.RoomGuest = null;
				}

				so.IsRoomMaster = false;
				so.RoomId = 0;
				so.InBattle = false;
				this.RoomMaster = null;

				var itemToRemove = BattleAction.ListRoom.Single(r => r.RoomId == this.RoomId);
				BattleAction.ListRoom.Remove(itemToRemove);

			}
			else if (so.IsRoomGuest && so.RoomId == this.RoomId)
			{
				string sendData = (int)CommandResponseEnum.QuitRoomResult + "`" + "2" + "~" + "Room guest has left the room" + "`";
				SendToRoomMaster (sendData);

				// klo flee from battle
				if(so.InBattle)
				{
					RoomMaster.IsRoomMaster = false;
					RoomMaster.InBattle = false;
					RoomMaster.RoomId = 0;
					this.RoomMaster = null;
				}

				so.IsRoomGuest = false;
				so.RoomId = 0;
				so.InBattle = false;
				this.RoomGuest = null;

				if(this.RoomMaster == null)
				{
					var itemToRemove = BattleAction.ListRoom.Single(r => r.RoomId == this.RoomId);
					BattleAction.ListRoom.Remove(itemToRemove);
				}
			}
		}

		public void SetPlayerReady(SessionData so)
		{
			string sendData="";
//			if (so.RoomMaster)
//			{
//				this.RoomMasterIsReady = true;
//				sendData = (int)CommandResponseEnum.BattleReadyResult + "`" + "1" +"~"+ "Room Master is ready" + "`";
//				SendToRoomGuest (sendData);
//			}

			if (so.IsRoomGuest)
			{
				this.RoomGuestIsReady = true;
				sendData = (int)CommandResponseEnum.BattleReadyResult + "`" + "2" +"~"+ "Room Guest is ready" + "`";
				SendToRoomMaster (sendData);
			}
		}

		public void UnsetPlayerReady(SessionData so)
		{
			string sendData="";
//			if (so.RoomMaster)
//			{
//				this.RoomMasterIsReady = false;
//				sendData = (int)CommandResponseEnum.BattleReadyResult + "`" + "-1" +"~"+ "Room Master is unready" + "`";
//				SendToRoomGuest (sendData);
//			}

			if (so.IsRoomGuest)
			{
				this.RoomGuestIsReady = false;
				sendData = (int)CommandResponseEnum.BattleReadyResult + "`" + "-2" +"~"+ "Room Guest is unready" + "`";
				SendToRoomMaster (sendData);
			}
		}

		public void StartBattle()
		{
			if(this.RoomGuestIsReady == true)
			{
				this.IsBattle = true;
				RoomMaster.InBattle = true;
				RoomGuest.InBattle = true;
				round = 1;
				string sendData = (int)CommandResponseEnum.StartOnlineBattleResult + "`" + "1" +"~"+ "Battle Begin" + "`";
				SendToRoomGuest (sendData);
				SendToRoomMaster (sendData);
				Debug ("Start of Battle");

//				Thread thread = new Thread (CheckTimeout);
//				thread.Start ();
			}
			else
			{
				string sendData = (int)CommandResponseEnum.StartOnlineBattleResult + "`" + "-1" +"~"+ "Guest is not ready" + "`";
				SendToRoomMaster (sendData);
			}
		}

		public void SetBattleAction(SessionData so, int action, string item)
		{
			bool isValidItem = false;

			if(IsBattle == false)
			{
				string response = (int)CommandEnum.BattleAction + "`" + "-1" + "`" + "Invalid Battle Action" + "`";
				if(so.IsRoomMaster)
				{
					SendToRoomMaster (response);
				}
				else if(so.IsRoomGuest)
				{
					SendToRoomGuest (response);
				}
			}
			else
			{
				if(this.skillRM == -3 || this.skillRG == -3)
				{
					if (so.IsRoomMaster) 
					{
						if (action == -1) {
							this.skillRMIndex = -1;
							this.skillRM = -2;
							this.itemRM = -1;
						} else {
							this.skillRMIndex = action;
							int skillID = Convert.ToInt32 (listSkillRM [action]);
							//						Console.WriteLine ("SkillID: " + skillID);
							int mp_cost = (int)Util.RandomDouble (SkillManager.SkillDictionary [skillID].ToSelfMPMin, SkillManager.SkillDictionary [skillID].ToSelfMPMax);
							//						Console.WriteLine ("MP Cost: " + mp_cost);
							//						Console.WriteLine ("Current MP: " + monsterRG.MP);
							//set skill chosen, klo lagi cooldown pake yg basic
							if (IsCooldown (monsterRM, skillID) || monsterRM.MP + mp_cost < 0) {
								this.skillRMIndex = 0;
								this.skillRM = Convert.ToInt32 (listSkillRM [0]);
								//							Console.WriteLine ("insufficient MP using basic skill: " + skillRM);
							} else {
								this.skillRM = skillID;
								monsterRM.MP += mp_cost;
								//							Console.WriteLine ("OK to use skill: " + this.skillRM );
							}

							if (item != string.Empty) {

								for (int i = 0; i < monsterRM.itemDict.Count; i++) {
									if (monsterRM.itemDict [i].ItemQRID == item) {
										this.itemRM = i;
										this.usedItemRM += item + "!";
										isValidItem = true;
									}
								}
															
								if (!isValidItem && skillRM == -3) {
									this.skillRMIndex = 0;
									this.skillRM = Convert.ToInt32 (listSkillRM [0]);
									this.itemRM = -2;
								}
							}

							//add to list cooldown
							if (SkillManager.SkillDictionary [skillRM].SkillCooldown != 0) {
								Console.WriteLine ("test RM: " + SkillManager.SkillDictionary [skillRM].SkillName + " cooldown: " + SkillManager.SkillDictionary [skillRM].SkillCooldown);
								int round = SkillManager.SkillDictionary [skillRM].SkillCooldown;
								monsterRM.skillCooldown.Add (new SkillCooldown (round, skillRM));
							}
						}

						//calculate damage if other player already choose action
						if (this.skillRG != -3 || this.skillRG == -2) {
							ProcessBattleAction ();
						}
					} 
					else if (so.IsRoomGuest) 
					{
						if(action == -1)
						{
							this.skillRGIndex = -1;
							this.skillRG = -2;
							this.itemRG = -1;
						}
						else
						{
							this.skillRGIndex = action;
							int skillID = Convert.ToInt32 (listSkillRG [action]);
							//						Console.WriteLine ("SkillID: " + skillID);
							int mp_cost = (int)Util.RandomDouble (SkillManager.SkillDictionary [skillID].ToSelfMPMin, SkillManager.SkillDictionary [skillID].ToSelfMPMax);
							//						Console.WriteLine ("MP Cost: " + mp_cost);
							//						Console.WriteLine ("Current MP: " + monsterRG.MP);
							if(IsCooldown (monsterRG, skillID) || monsterRG.MP + mp_cost < 0)
							{
								this.skillRMIndex = 0;
								this.skillRG = Convert.ToInt32(listSkillRG[0]);
								//							Console.WriteLine ("insufficient MP using basic skill: " + skillRG);
							}
							else
							{
								this.skillRG = skillID;
								monsterRG.MP += mp_cost;
								//							Console.WriteLine ("OK to use skill: " + skillRG );
							}

							//						itemRG = item;

							if (item != string.Empty) {

								for(int i=0; i<monsterRG.itemDict.Count;i++)
								{
									if(monsterRG.itemDict[i].ItemQRID == item)
									{
										this.itemRG = i;
										this.usedItemRG += item + "!";
										isValidItem = true;
									}
								}

								if(!isValidItem && skillRG == -3)
								{
									this.skillRGIndex = 0;
									this.skillRG = Convert.ToInt32(listSkillRG[0]);
									this.itemRG = -2;
								}
							}

							if (SkillManager.SkillDictionary [skillRG].SkillCooldown != 0) {
								Console.WriteLine ("test RG: " + SkillManager.SkillDictionary [skillRG].SkillName + " cooldown: " + SkillManager.SkillDictionary [skillRG].SkillCooldown);
								int round = SkillManager.SkillDictionary [skillRG].SkillCooldown;
								monsterRG.skillCooldown.Add (new SkillCooldown (round, skillRG));
							}
						}										

						if (this.skillRM != -3 || this.skillRM == -2) {
							ProcessBattleAction ();
						}
					}
				}

			}
		}

		public void SendSuddenDeathResult(SessionData so)
		{
			if(so.IsRoomMaster)
			{
				string result = (int)CommandResponseEnum.SuddenDeathResult + "`" + suddenDeathRM + "`";
				Log.Info (result);
				SendToRoomMaster (result);
			}
			else if(so.IsRoomGuest)
			{
				string result = (int)CommandResponseEnum.SuddenDeathResult + "`" + suddenDeathRG + "`";
				Log.Info (result);
				SendToRoomGuest (result);
			}
		}

		public void GenerateSuddenDeath()
		{
			Log.Info("Generating Sudden death for round: " + this.round);
//			this.suddenDeathRM = Util.RandomInt (0, 5);
			//this.suddenDeathRG = Util.RandomInt (0, 5);

			if(round == 4)
			{
				this.suddenDeathRM = 2;
				this.suddenDeathRG = 2;
			}
			else
			{
				this.suddenDeathRM = 5;
				this.suddenDeathRG = 5;
			}

			Log.Info ("RM SD: " + suddenDeathRM);
			Log.Info ("RG SD: " + suddenDeathRG);
		}

		private void ProcessBattleAction()
		{
			isProcessing = true;

			string parameters = string.Empty;
			string battleResultRM = string.Empty;
			string battleResultRG = string.Empty;

			parameters += suddenDeathRM + "~" + suddenDeathRG + "~";

			parameters += skillRMIndex + "~" + skillRGIndex + "~";
			//decide who move first (0 for Room Master, 1 for Room Guest)
			int firstTakingAction = WhosFirst();

			Log.Debug ("RM Skill Used: " + skillRM);
			Log.Debug ("RM Item Used: " + itemRM);
			Log.Debug ("RG Skill Used: " + skillRG);
			Log.Debug ("RG Item Used: " + itemRG);

			parameters += firstTakingAction + "~";

			if(firstTakingAction == 0)
			{
				battleResultRM = new SkillManager ().CalculateBattleDamage (skillRM,itemRM,monsterRM,monsterRG);
				if(this.suddenDeathRM != -1 && suddenDeathRM < 3)
				{
					Log.Info ("Calculating sudden death RM");
					new SuddenDeathManager ().CalculateSuddenDeath (this.suddenDeathRM, this.monsterRM, this.monsterRG);
				}

				battleResultRG = new SkillManager ().CalculateBattleDamage (skillRG, itemRG, monsterRG, monsterRM);
				if(this.suddenDeathRG != -1 && suddenDeathRG < 3)
				{
					Log.Info ("Calculating sudden death RG");
					new SuddenDeathManager ().CalculateSuddenDeath (this.suddenDeathRG, this.monsterRG, this.monsterRM);
				}
			}
			else
			{
				battleResultRG = new SkillManager ().CalculateBattleDamage (skillRG, itemRG, monsterRG, monsterRM);
				if(this.suddenDeathRG != -1 && suddenDeathRG < 3)
				{
					Log.Info ("Calculating sudden death RG");
					new SuddenDeathManager ().CalculateSuddenDeath (this.suddenDeathRG, this.monsterRG, this.monsterRM);
				}

				battleResultRM = new SkillManager ().CalculateBattleDamage (skillRM,itemRM,monsterRM,monsterRG);
				if(this.suddenDeathRM != -1 && suddenDeathRM < 3)
				{
					Log.Info ("Calculating sudden death RM");
					new SuddenDeathManager ().CalculateSuddenDeath (this.suddenDeathRM, this.monsterRM, this.monsterRG);
				}
			}

			parameters += monsterRM.HPRegen + "~" + monsterRM.MPRegen + "~";
			parameters += battleResultRM;

			parameters += monsterRG.HPRegen + "~" + monsterRG.MPRegen + "~";
			parameters += battleResultRG;
			parameters += "!" + itemRM + "~" + itemRG;

//			parameters += monsterRM.HPRegen + "~" + monsterRM.MPRegen + "~";
//			if(!monsterRM.isParalyzed && !monsterRM.isStunned)
//			{
//				battleResultRM = new SkillManager ().CalculateBattleDamage (skillRM,itemRM,monsterRM,monsterRG,round);
//				parameters += battleResultRM;
//			}
//			else
//			{
//				battleResultRM = "0~0~0~0~0~0~0~0~";
//				parameters += battleResultRM;
//			}
//
//			parameters += monsterRG.HPRegen + "~" + monsterRG.MPRegen + "~";
//			if (!monsterRG.isParalyzed && !monsterRG.isStunned) 
//			{			
//				battleResultRG = new SkillManager ().CalculateBattleDamage (skillRG, itemRG, monsterRG, monsterRM, round);
//				parameters += battleResultRG;
//			}
//			else
//			{
//				battleResultRG = "0~0~0~0~0~0~0~0~";
//				parameters += battleResultRG;
//			}

//			if(this.suddenDeathRM != -1 && this.suddenDeathRG != -1)
//			{			
//				if(suddenDeathRM < 3)
//				{
//					Log.Info ("Calculating sudden death RM");
//					new SuddenDeathManager ().CalculateSuddenDeath (this.suddenDeathRM, this.monsterRM, this.monsterRG);
//				}
//				if(suddenDeathRG < 3)
//				{
//					Log.Info ("Calculating sudden death RG");
//					new SuddenDeathManager ().CalculateSuddenDeath (this.suddenDeathRG, this.monsterRG, this.monsterRM);
//				}
//			}

			string result = (int)CommandResponseEnum.SendBattleActionResult + "`" + parameters + "`";
			Log.Debug ("Room battle result: " + result);
			SendToRoomGuest (result);
			SendToRoomMaster (result);

			// cek buff masing" player, remove yg udh abis effect nya
			CheckBuffToRemove ();
			CheckCooldownToRemove ();

			CheckContinuousBuff ();
			CheckEndGame ();

			Debug ("End of Turn");

			// reset last action after calculation
			this.skillRG = -3;
			this.skillRM = -3;

			//reset sudden death
			this.suddenDeathRM = -1;
			this.suddenDeathRG = -1;

			//reset item
			this.itemRM = -2;
			this.itemRG = -2;

			monsterRM.HP = Util.MaxCap (monsterRM.HPRegen, monsterRM.HP, maxRMHP);
			monsterRM.MP = Util.MaxCap (monsterRM.MPRegen, monsterRM.MP, maxRMMP);
			monsterRG.HP = Util.MaxCap (monsterRG.HPRegen, monsterRG.HP, maxRGHP);
			monsterRG.MP = Util.MaxCap (monsterRG.MPRegen, monsterRG.MP, maxRGMP);
			//increase round counter
			round++;

			if(round > 3)
			{
				GenerateSuddenDeath ();
			}
			isProcessing = false;

			Debug ("Start of Turn");
		}

		private int WhosFirst ()
		{
			Log.Info ("Whos First");
			// assume user pake item berdasarkan index dari item yg di set
			float RM_SPD = itemRM == -2 ?
			monsterRM.SPD * (float) Util.RandomDouble (SkillManager.SkillDictionary [skillRM].ToSelfSPDMin, SkillManager.SkillDictionary [skillRM].ToSelfSPDMax):
			monsterRM.SPD;
			float RG_SPD = itemRG == -2 ?
			monsterRG.SPD * (float) Util.RandomDouble (SkillManager.SkillDictionary [skillRG].ToSelfSPDMin, SkillManager.SkillDictionary [skillRG].ToSelfSPDMax):
			monsterRG.SPD;

			if (RM_SPD > RG_SPD) {
				return 0;
			}
			else if (RM_SPD < RG_SPD) {
				return 1;
			}
			return Util.RandomInt (0, 1);
		}

		private bool IsCooldown(BattleMonsterData mon, int skill)
		{
			foreach(var cd in mon.skillCooldown )
			{
				if(cd.rounds > 0)
				{
					Console.WriteLine ("T");
					return true;
				}
			}
			Console.WriteLine ("F");
			return false;
		}

		private bool IsNoMana(int skillID, BattleMonsterData mon)
		{
			int mp_cost = (int)Util.RandomDouble (SkillManager.SkillDictionary [skillID].ToSelfMPMin, SkillManager.SkillDictionary [skillID].ToSelfMPMax);
			return mon.MP >= mp_cost ? false : true;
		}

		public void CheckCooldownToRemove()
		{
			Console.WriteLine ("check cooldown to remove");
			for(int i = 0; i<monsterRM.skillCooldown.Count ; i++)
			{
				monsterRM.skillCooldown [i].rounds += -1;
				if(monsterRM.skillCooldown [i].rounds == 0)
				{
					monsterRM.skillCooldown.Remove(monsterRM.skillCooldown [i]);
				}
			}

			for(int i = 0; i<monsterRG.skillCooldown.Count ; i++)
			{
				monsterRG.skillCooldown [i].rounds += -1;
				if(monsterRG.skillCooldown [i].rounds == 0)
				{
					monsterRG.skillCooldown.Remove(monsterRG.skillCooldown [i]);
				}
			}
		}

		public void CheckContinuousBuff()
		{
			Log.Info ("check continuous buff");
			foreach(var buff in monsterRM.activeBuffList)
			{
				if(buff.isContinuous)
				{
					Log.Info ("Buff name: " + buff.BuffName);
					Log.Info ("Applying continuous buff to RM");
					Log.Info ("Current HP: " + this.monsterRM.HP);
					this.monsterRM.HP += buff.HP;
					Log.Info ("damage HP: " + buff.HP);
					this.monsterRM.HPRegen += buff.HPRegen;
					this.monsterRM.MP += buff.MP;
					this.monsterRM.MPRegen += buff.MPRegen;
					this.monsterRM.PATK += buff.PATK;
					this.monsterRM.PDEF += buff.PDEF;
					this.monsterRM.MATK += buff.MATK;
					this.monsterRM.MDEF += buff.MDEF;
					this.monsterRM.ACC += buff.ACC;
					this.monsterRM.EVA += buff.EVA;
					this.monsterRM.SPD += buff.SPD;
					this.monsterRM.CRIT += buff.CRIT;
				}
			}

			foreach(var buff in monsterRG.activeBuffList)
			{
				if(buff.isContinuous)
				{
					Log.Info ("Buff name: " + buff.BuffName);
					Log.Info ("Applying continuous buff to RG");
					Log.Info ("Current HP: " + this.monsterRG.HP);
					this.monsterRG.HP += buff.HP;
					Log.Info ("damage HP: " + buff.HP);
					this.monsterRG.HPRegen += buff.HPRegen;
					this.monsterRG.MP += buff.MP;
					this.monsterRG.MPRegen += buff.MPRegen;
					this.monsterRG.PATK += buff.PATK;
					this.monsterRG.PDEF += buff.PDEF;
					this.monsterRG.MATK += buff.MATK;
					this.monsterRG.MDEF += buff.MDEF;
					this.monsterRG.ACC += buff.ACC;
					this.monsterRG.EVA += buff.EVA;
					this.monsterRG.SPD += buff.SPD;
					this.monsterRG.CRIT += buff.CRIT;
				}
			}
		}

		public void CheckBuffToRemove()
		{
			Log.Info ("check buff to remove");
			Log.Info ("RM buff count: " + monsterRM.activeBuffList.Count);
			for(int i = 0; i<monsterRM.activeBuffList.Count ; i++)
			{
				Log.Info (monsterRM.activeBuffList [i].BuffName + "buff to be decrease by 1 duration from: " + monsterRM.activeBuffList [i].Lifetime);
				monsterRM.activeBuffList[i].Lifetime += -1;
				if(monsterRM.activeBuffList[i].Lifetime == 0)
				{
					monsterRM.RemoveBuff (monsterRM.activeBuffList[i]);
				}
			}

			Log.Info ("RG buff count: " + monsterRG.activeBuffList.Count);
			for(int i = 0; i<monsterRG.activeBuffList.Count ; i++)
			{
				Log.Info (monsterRG.activeBuffList [i].BuffName + "buff to be decrease by 1 duration from: " + monsterRG.activeBuffList [i].Lifetime);
				monsterRG.activeBuffList[i].Lifetime += -1;
				if(monsterRG.activeBuffList[i].Lifetime == 0)
				{
					monsterRG.RemoveBuff (monsterRG.activeBuffList[i]);
				}
			}
		}

		public void CheckEndGame()
		{
			Log.Info("check end game");
			if(monsterRM.HP <= 0 || monsterRG.HP <= 0)
			{
				this.IsBattle = false;
				RoomMaster.InBattle = false;
				RoomGuest.InBattle = false;

				Log.Info("End Game !!");

				if(monsterRM.HP <= 0)
				{
					string winsItem = string.Empty;
					foreach(var item in monsterRG.itemDict)
					{
						winsItem += (item.Value as BattleItemData).ItemQRID + "!";
					}

					string loseItem = string.Empty;
					foreach(var item in monsterRM.itemDict)
					{
						loseItem += (item.Value as BattleItemData).ItemQRID + "!";
					}

					ProcessBattleResult (this.RoomGuest,this.RoomMaster, monsterRG.MonsterQRID, monsterRM.MonsterQRID, winsItem, loseItem);
				}
				else if(monsterRG.HP <= 0)
				{
					string winsItem = string.Empty;
					foreach(var item in monsterRM.itemDict)
					{
						winsItem += (item.Value as BattleItemData).ItemQRID + "!";
					}

					string loseItem = string.Empty;
					foreach(var item in monsterRG.itemDict)
					{
						loseItem += (item.Value as BattleItemData).ItemQRID + "!";
					}

					ProcessBattleResult (this.RoomMaster,this.RoomGuest, monsterRM.MonsterQRID, monsterRG.MonsterQRID, winsItem, loseItem);
				}
			}
		}

		public void CheckTimeout()
		{
			while(IsBattle)
			{
				bool isTimeout = false;
				bool isPickRM = false;
				bool isPickRG = false;

				for(int i=1; i<=10000; i++)
				{
					Thread.Sleep (1);
					if(i%1000 == 0)
					{
						Console.WriteLine ("Waiting after " + (i / 1000) + " seconds");
					}
					if(isProcessing)
					{
						isPickRM = true;
						isPickRG = true;
						break;
					}
//					Thread.Sleep (100);
//					Console.WriteLine ("Waiting after " + (i/10) + " seconds");
//					if((skillRM != -3 && itemRM == -1) || (skillRM == -3 && itemRM != -1))
//					{
//						Console.WriteLine ("RM Pick");
//						isPickRM = true;
//					}
//					if((skillRG != -3 && itemRG == -1) || (skillRG == -3 && itemRG != -1))
//					{
//						Console.WriteLine ("RG Pick");
//						isPickRG = true;
//					}
//					if(isPickRG && isPickRM)
//					{
//						Console.WriteLine ("Stopping thread.sleep");
//						break;
//					}
				}

				if(!isPickRM)
				{
					skillRM = Convert.ToInt32(listSkillRM[0]);
					itemRM = -2;
					isTimeout = true;
					if(round>3 && suddenDeathRM == -1)
					{
						suddenDeathRM = Util.RandomInt (0, 5);
					}
				}

				if(!isPickRG)
				{
					skillRG = Convert.ToInt32(listSkillRG[0]);
					itemRG = -2;
					isTimeout = true;
					if(round>3 && suddenDeathRG == -1)
					{
						suddenDeathRG = Util.RandomInt (0, 5);
					}
				}						

				if(isTimeout)
				{
					ProcessBattleAction ();
				}
			}				
		}

//		public void CalculateBattleAction(string param)
//		{
//			var split = param.Split ('!');
//			var battleAction = split [0].Split ('`');
//			var item = split [1].Split('~');
//			var skill = battleAction [1].Split('~');
//
//			int firstAct = Convert.ToInt32(skill [4]);
//
//			if(firstAct == 0)
//			{
//				//RM first
//
//
//			}
//			else
//			{
//				//RG first
//
//			}
//
//
//		}

		public void ProcessBattleResult(SessionData sdWinner, SessionData sdLoser, string winMons, string loseMons, string winItems, string loseItems)
		{
			// Send to winner: commandEnum`winString`winStats`
			// Send to loser: commandEnum`loseString`loseStats`

			string[] winItem = winItems.Split (new char[] { '!' });
			string[] loseItem = loseItems.Split (new char[] { '!' });
			string loser;
			string winner;
			string winnerID;
			string loserID;
			int winCoin = 100;
			int loseCoin = 25;
			StringDictionary winCollection;
			StringDictionary loseCollection;

			using (Command (@"
				SELECT user_id, hunger, happiness, clean, discipline, sick, exp_needed, exp, exp_mult, version, subversion,
					hp, mp, p_atk, m_atk, p_def, m_def, acc, eva, mons_card_id
				 FROM monster_cards WHERE mons_card_qrid = :win
			")) {
				AddParameter ("win", winMons);
				winCollection = ExecuteRow ();
				winner = winCollection ["user_id"];
				winnerID = winCollection ["mons_card_id"];
			}

			using (Command (@"
				SELECT user_id, hunger, happiness, clean, discipline, sick, exp_needed, exp, exp_mult, version, subversion,
					hp, mp, p_atk, m_atk, p_def, m_def, acc, eva, mons_card_id
				FROM monster_cards WHERE mons_card_qrid = :lose
			")) {
				AddParameter ("lose", loseMons);
				loseCollection = ExecuteRow ();
				loser = loseCollection ["user_id"];
				loserID = loseCollection ["mons_card_id"];
			}
			string battleId;
			using (Command (@"
				INSERT INTO battle_log(win_user_id, win_mons_id, lose_user_id, lose_mons_id, time)
				VALUES (:win, :winmons, :lose, :losemons, CURRENT_TIMESTAMP) RETURNING battle_id as bid;
			")) {
				AddParameter ("win", winner);
				AddParameter ("winmons", winnerID);
				AddParameter ("lose", loser);
				AddParameter ("losemons", loserID);				
				battleId = ExecuteRow () ["bid"];
			}

			// give coins to player
			using (Command (@"
				UPDATE user_list SET mobile_coins = mobile_coins + :win_coin 
				WHERE user_id = :winner
			")) {
				AddParameter ("win_coin", winCoin);
				AddParameter ("winner", winner);

				ExecuteRow ();
			}

			using (Command (@"
				UPDATE user_list SET mobile_coins = mobile_coins + :lose_coin 
				WHERE user_id = :loser
			")) {
				AddParameter ("lose_coin", loseCoin);
				AddParameter ("loser", loser);
				ExecuteRow ();
			}

			// exp point calculation
			const int baseWinXP = 1000;
			const int baseLoseXP = 250;
			double winMult = (double)(100 + Convert.ToInt32 (winCollection ["hunger"]) +
			                 Convert.ToInt32 (winCollection ["happiness"]) + Convert.ToInt32 (winCollection ["clean"]) +
			                 Convert.ToInt32 (winCollection ["discipline"]) + Convert.ToInt32 (winCollection ["sick"])) / 300.0F;

			double loseMult = (double)(100 + Convert.ToInt32 (winCollection ["hunger"]) +
			                  Convert.ToInt32 (winCollection ["happiness"]) + Convert.ToInt32 (winCollection ["clean"]) +
			                  Convert.ToInt32 (winCollection ["discipline"]) + Convert.ToInt32 (winCollection ["sick"])) / 300.0F;

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

			int currWinXP = winXP + Convert.ToInt32 (winCollection ["exp"]);
			if (currWinXP >= Convert.ToInt32 (winCollection ["exp_needed"])) {
				currWinXP = currWinXP - Convert.ToInt32 (winCollection ["exp_needed"]);
				winLvlUp = true;

				int nextLvlWin = Convert.ToInt32 (winCollection ["version"]) * 10 + Convert.ToInt32 (winCollection ["subversion"]) + 1;
				winVersion = Convert.ToInt32 (nextLvlWin.ToString ().Substring (0, 1));
				winSubversion = Convert.ToInt32 (nextLvlWin.ToString ().Substring (1, 1));

				if (nextLvlWin < 30) {
					using (Command (@"
						SELECT level, exp
						FROM base_exp_req
						WHERE level = :lvl
					")) {
						AddParameter ("lvl", nextLvlWin);
						nextWinNeededXP = Convert.ToInt32 (ExecuteRow () ["exp"]);
					}
				}
			}

			int currLoseXP = loseXP + Convert.ToInt32 (loseCollection ["exp"]);

			if (currLoseXP >= Convert.ToInt32 (loseCollection ["exp_needed"])) {
				currLoseXP = currLoseXP - Convert.ToInt32 (loseCollection ["exp_needed"]);
				loseLvlUp = true;

				int nextLvlLose = Convert.ToInt32 (loseCollection ["version"]) * 10 + Convert.ToInt32 (loseCollection ["subversion"]) + 1;
				loseVersion = Convert.ToInt32 (nextLvlLose.ToString ().Substring (0, 1));
				loseSubversion = Convert.ToInt32 (nextLvlLose.ToString ().Substring (1, 1));

				if (nextLvlLose < 30) {
					using (Command (@"
						SELECT level, exp
						FROM base_exp_req
						WHERE level = :lvl
					")) {
						AddParameter ("lvl", nextLvlLose);
						nextLoseNeededXP = Convert.ToInt32 (ExecuteRow () ["exp"]);
					}
				}
			}

			byte[] evaqriWin = new byte[2];
			new Random ().NextBytes (evaqriWin);

			int evaWin = evaqriWin [0] < 15 ? 1 : 0;
			int criWin = evaqriWin [1] < 15 ? 1 : 0;

			byte[] evaqriLose = new byte[2];
			new Random ().NextBytes (evaqriLose);

			int evaLose = evaqriLose [0] < 15 ? 1 : 0;
			int criLose = evaqriLose [1] < 15 ? 1 : 0;

			// update winner stats
			if (winLvlUp) {
				using (Command (@"
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
				")) {
					AddParameter ("winmons", winMons);
					AddParameter ("eva", evaWin);
					AddParameter ("cri", criWin);
					AddParameter ("version", winVersion);
					AddParameter ("subversion", winSubversion);
					AddParameter ("newexpneeded", nextWinNeededXP);
					AddParameter ("newexp", currWinXP);
					ExecuteWrite ();
				}
			} else {
				using (Command (@"
					UPDATE monster_cards
					SET wins = wins + 1,
						exp = :newexp,
						hunger = greatest(hunger - round(random() * 25), 0),
						happiness = greatest(happiness - round(random() * 25), 0),
						clean = greatest(clean - round(random() * 25), 0),
						discipline = greatest(discipline - round(random() * 25), 0),
						sick = greatest(sick - round(random() * 25), 0)
					WHERE mons_card_qrid = :winmons
				")) {
					AddParameter ("winmons", winMons);
					AddParameter ("newexp", currWinXP);
					ExecuteWrite ();
				}
			}

			// update loser stats
			if (loseLvlUp) {
				using (Command (@"
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
				")) {
					AddParameter ("losemons", loseMons);
					AddParameter ("eva", evaLose);
					AddParameter ("cri", criLose);
					AddParameter ("version", loseVersion);
					AddParameter ("subversion", loseSubversion);
					AddParameter ("newexpneeded", nextLoseNeededXP);
					AddParameter ("newexp", currLoseXP);
					ExecuteWrite ();
				}
			} else {
				using (Command (@"
					UPDATE monster_cards
					SET loses = loses + 1,
						exp = :newexp,
						hunger = greatest(hunger - round(random() * 25), 0),
						happiness = greatest(happiness - round(random() * 25), 0),
						clean = greatest(clean - round(random() * 25), 0),
						discipline = greatest(discipline - round(random() * 25), 0),
						sick = greatest(sick - round(random() * 25), 0)
					WHERE mons_card_qrid = :losemons
				")) {
					AddParameter ("losemons", loseMons);
					AddParameter ("newexp", currLoseXP);
					ExecuteWrite ();
				}
			}


			// update item quantity
			foreach (string row in winItem) {
				using (Command (@"
					UPDATE item_cards
					SET qty = qty - 1
					WHERE item_card_qrid = :item
				")) {
					AddParameter ("item", row);

					ExecuteWrite ();
				}

				using (Command (@"
					INSERT INTO battle_log_detail(battle_id, user_id, mons_id, item_id)
				    SELECT :battle, :winner, :winmons, item_card_id
					FROM item_cards
					WHERE item_card_qrid = :item
				")) {
					AddParameter ("battle", battleId);
					AddParameter ("winner", winner);
					AddParameter ("winmons", winnerID);
					AddParameter ("item", row);
					ExecuteWrite ();
				}
			}

			foreach (string row in loseItem) {
				using (Command (@"
					UPDATE item_cards
					SET qty = qty - 1
					WHERE item_card_qrid = :item
				")) {
					AddParameter ("item", row);

					ExecuteWrite ();
				}

				using (Command (@"
					INSERT INTO battle_log_detail(battle_id, user_id, mons_id, item_id)
				    SELECT :battle, :loser, :losemons, item_card_id
					FROM item_cards
					WHERE item_card_qrid = :item
				")) {
					AddParameter ("battle", battleId);
					AddParameter ("loser", loser);
					AddParameter ("losemons", loserID);
					AddParameter ("item", row);
					ExecuteWrite ();
				}
			}

			//MonsterQRID~EXP`MonsterQRID~EXP`
			StringDictionary newWin = new StringDictionary ();
			StringDictionary newLose = new StringDictionary ();

			using (Command (@"
				SELECT hp, mp, version, subversion, p_atk, m_atk, p_def, m_def, acc, eva
				FROM monster_cards
				WHERE mons_card_qrid = :mons
			")) {
				AddParameter ("mons", winMons);
				newWin = ExecuteRow ();
			}

			using (Command (@"
				SELECT hp, mp, version, subversion, p_atk, m_atk, p_def, m_def, acc, eva
				FROM monster_cards
				WHERE mons_card_qrid = :mons
			")) {
				AddParameter ("mons", loseMons);
				newLose = ExecuteRow ();
			}

			string winString = winMons + "~" + winXP;
			if (winLvlUp) {
				int versionNew = Convert.ToInt32 (newWin ["version"]);
				int subversionNew = Convert.ToInt32 (newWin ["subversion"]);
				int atkNew = Convert.ToInt32 (newWin ["p_atk"]) + Convert.ToInt32 (newWin ["m_atk"]);
				int defNew = Convert.ToInt32 (newWin ["p_def"]) + Convert.ToInt32 (newWin ["m_def"]);
				int accNew = Convert.ToInt32 (newWin ["acc"]);
				int evaNew = Convert.ToInt32 (newWin ["eva"]);

				winString += "~" + versionNew + "~" + subversionNew + "~" + atkNew + "~" + defNew + "~" + accNew + "~" + evaNew;
			}

			string loseString = loseMons + "~" + loseXP;
			if (loseLvlUp) {
				int versionNew = Convert.ToInt32 (newLose ["version"]);
				int subversionNew = Convert.ToInt32 (newLose ["subversion"]);
				int atkNew = Convert.ToInt32 (newLose ["p_atk"]) + Convert.ToInt32 (newLose ["m_atk"]);
				int defNew = Convert.ToInt32 (newLose ["p_def"]) + Convert.ToInt32 (newLose ["m_def"]);
				int accNew = Convert.ToInt32 (newLose ["acc"]);
				int evaNew = Convert.ToInt32 (newLose ["eva"]);

				loseString += "~" + versionNew + "~" + subversionNew + "~" + atkNew + "~" + defNew + "~" + accNew + "~" + evaNew;
			}

			string winStats = new CardAction ().GetMonsterStat (winMons);
			string loseStats = new CardAction ().GetMonsterStat (loseMons);

			string battleResultWinner = (int)CommandResponseEnum.BattleResult + "`" + winString + "~" + winCoin + "`" + winStats + "`";
			string battleResultLoser = (int)CommandResponseEnum.BattleResult + "`" + loseString + "~" + loseCoin + "`" + loseStats + "`";

			if (sdWinner == this.RoomMaster) {
				resultBattleRM = battleResultWinner;
				resultBattleRG = battleResultLoser;
			} else {
				resultBattleRM = battleResultLoser;
				resultBattleRG = battleResultWinner;
			}

			Log.Info ("RM battle result: " + resultBattleRM);
			Log.Info ("RG battle result: " + resultBattleRG);
		}

		public void BattleResult (SessionData so)
		{
			if(so == this.RoomMaster) 
			{
				SendToRoomMaster (resultBattleRM);
				so.IsRoomMaster = false;
				so.RoomId = 0;
				so.InBattle = false;
				this.RoomMaster = null;
			}
			else if(so == this.RoomGuest)
			{
				SendToRoomGuest (resultBattleRG);
				so.IsRoomGuest = false;
				so.RoomId = 0;
				so.InBattle = false;
				this.RoomGuest = null;
			}

			if(this.RoomMaster == null && this.RoomGuest == null)
			{
				var itemToRemove = BattleAction.ListRoom.Single(r => r.RoomId == this.RoomId);
				BattleAction.ListRoom.Remove(itemToRemove);
			}
		}

		public void SendToRoomMaster (string sendData)
		{
			List<byte> temp = new List<byte> ();
			temp.AddRange (Encoding.ASCII.GetBytes (sendData));
			temp.Add (0x03);
			byte[] data = temp.ToArray();
			RoomMaster.WriteLine ("to RM: " + sendData);
			try 
			{
				this.RoomMaster.Sock.Send(data);
			}
			catch(Exception e) 
			{
				StringBuilder str = new StringBuilder();
				str.AppendLine("\n----------------------------------\n\nUnable to send message to room master because error occured at " + DateTime.Now);
				str.AppendLine("Message: " + e.Message);
				str.AppendLine("Source: " + e.Source);
				str.AppendLine("Stack Trace:");
				str.AppendLine(e.StackTrace);

				Log.Error(str.ToString());
			}

		}

		public void SendToRoomGuest(string sendData)
		{
			List<byte> temp = new List<byte> ();
			temp.AddRange (Encoding.ASCII.GetBytes (sendData));
			temp.Add (0x03);
			byte[] data = temp.ToArray();
			RoomGuest.WriteLine ("to RG: " + sendData);
			try 
			{
				this.RoomGuest.Sock.Send(data);
			}
			catch(Exception e) 
			{
				StringBuilder str = new StringBuilder();
				str.AppendLine("\n----------------------------------\n\nUnable to send message to room quest because error occured at " + DateTime.Now);
				str.AppendLine("Message: " + e.Message);
				str.AppendLine("Source: " + e.Source);
				str.AppendLine("Stack Trace:");
				str.AppendLine(e.StackTrace);

				Log.Error(str.ToString());
			}
		}

		public void Debug(string pos)
		{
			Log.Debug ("DEBUG FOR ROUND " + this.round + " " + pos);
			Log.Debug (" ");
			Log.Debug ("==========");
			Log.Debug ("Monster RM");
			Log.Debug ("==========");
			Log.Debug ("Monster: " + monsterRM.MonsterTemplateName);
			Log.Debug ("HP: " + monsterRM.HP);
			Log.Debug ("MP: " + monsterRM.MP);
			Log.Debug ("PATK: " + monsterRM.PATK);
			Log.Debug ("MATK: " + monsterRM.MATK);
			Log.Debug ("PDEF: " + monsterRM.PDEF);
			Log.Debug ("MDEF: " + monsterRM.MDEF);
			Log.Debug ("SPD: " + monsterRM.SPD);
			Log.Debug ("ACC: " + monsterRM.ACC);
			Log.Debug ("EVA: " + monsterRM.EVA);
			Log.Debug ("CRIT: " + monsterRM.CRIT);
			Log.Debug (" ");
			Log.Debug ("=============");
			Log.Debug ("List of Skill");
			Log.Debug ("=============");
			foreach(var skill in listSkillRM)
			{
				Log.Debug (skill);
			}
			Log.Debug (" ");
			Log.Debug ("=================");
			Log.Debug ("Skill on Cooldown");
			Log.Debug ("=================");
			if(monsterRM.skillCooldown.Count > 0)
			{
				foreach(var cd in monsterRM.skillCooldown)
				{
					Log.Debug ("Skill ID: " + cd.SkillId + ", Rounds left: " + cd.rounds );
				}
			}
			else
			{
				Log.Debug ("None");
			}				
			Log.Debug (" ");
			Log.Debug ("===========");
			Log.Debug ("Active Buff");
			Log.Debug ("===========");		
			if(monsterRM.activeBuffList.Count > 0)
			{
				foreach(var buff in monsterRM.activeBuffList)
				{
					Log.Debug ("Buff name: " + buff.BuffName + ", Rounds left: " + buff.Lifetime);
				}
			}
			else
			{
				Log.Debug ("None");
			}
			Log.Debug (" ");
			Log.Debug ("=============");
			Log.Debug ("Active Status");
			Log.Debug ("=============");
			Log.Debug ("Poison: " + monsterRM.isPoisoned);
			Log.Debug ("Paralyze: " + monsterRM.isParalyzed);
			Log.Debug ("Stun: " + monsterRM.isStunned);
			Log.Debug (" ");
			Log.Debug ("==========");
			Log.Debug ("Monster RG");
			Log.Debug ("==========");
			Log.Debug ("Monster: " + monsterRG.MonsterTemplateName);
			Log.Debug ("HP: " + monsterRG.HP);
			Log.Debug ("MP: " + monsterRG.MP);
			Log.Debug ("PATK: " + monsterRG.PATK);
			Log.Debug ("MATK: " + monsterRG.MATK);
			Log.Debug ("PDEF: " + monsterRG.PDEF);
			Log.Debug ("MDEF: " + monsterRG.MDEF);
			Log.Debug ("SPD: " + monsterRG.SPD);
			Log.Debug ("ACC: " + monsterRG.ACC);
			Log.Debug ("EVA: " + monsterRG.EVA);
			Log.Debug ("CRIT: " + monsterRG.CRIT);
			Log.Debug (" ");
			Log.Debug ("=============");
			Log.Debug ("List of Skill");
			Log.Debug ("=============");
			foreach(var skill in listSkillRG)
			{
				Log.Debug (skill);
			}
			Log.Debug (" ");
			Log.Debug ("=================");
			Log.Debug ("Skill on Cooldown");
			Log.Debug ("=================");
			if(monsterRG.skillCooldown.Count>0)
			{
				foreach(var cd in monsterRG.skillCooldown)
				{
					Log.Debug ("Skill ID: " + cd.SkillId + " Rounds left: " + cd.rounds );
				}
			}
			else
			{
				Log.Debug ("None");
			}		
			Log.Debug (" ");
			Log.Debug ("===========");
			Log.Debug ("Active Buff");
			Log.Debug ("===========");
			if(monsterRG.activeBuffList.Count > 0)
			{
				foreach(var buff in monsterRG.activeBuffList)
				{
					Log.Debug ("Buff name: " + buff.BuffName + ", Rounds left: " + buff.Lifetime);
				}
			}
			else
			{
				Log.Debug ("None");
			}
			Log.Debug (" ");
			Log.Debug ("=============");
			Log.Debug ("Active Status");
			Log.Debug ("=============");
			Log.Debug ("Poison: " + monsterRG.isPoisoned);
			Log.Debug ("Paralyze: " + monsterRG.isParalyzed);
			Log.Debug ("Stun: " + monsterRG.isStunned);
			Log.Debug (" ");
		}
	}
}

