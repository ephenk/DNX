using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace DNXServer.Action
{
	public class CardAction : BaseAction
	{
		public CardAction ()
		{

		}

		public string RegisterCard(SessionData so, string qrid)
		{
			/*
			-1 General Error - error yang client ga perlu tau / error di aplikasi server (try-catch) ato error database (koneksi/query)
			-2 Upgrade Card - upgrade card di-register lewat monster detail page
			-3 Owned by other User
			-4 Owned by this User
			-5 Invalid QRID - qrid kartu ga ada ato owner kartu nya userID -1 (Davy Jones)
			-6 User Locked - too many random scan
			*/
			List<string> cardType = new List<string>();
			StringDictionary checkCardType;
			string getUserID = "" ;
			string getCardTypeID = "" ;
			string response = "";
			string getRemainingAttemp = "";

			string userID = Convert.ToString(so.UserId);

			using (Command(@"server_check_card_type")) 
			{
				AddParameter ("qrid", qrid);
				AddParameter ("uid", so.UserId);
				checkCardType = ExecuteSpRow();
				if (checkCardType != null && checkCardType.Count > 0) 
				{
					getCardTypeID = checkCardType["_card_type_id"];
					getUserID = checkCardType["_user_id"];
					getRemainingAttemp = checkCardType["_remaining_attempts"];
				} 
				else
				{
					response = "-5`0" ;
				}
			}

			if (string.IsNullOrEmpty (response)) {

				if(getCardTypeID == "-1")
				{
					response = "-6"; // lock user from scanning qrid
				}
				else if (getCardTypeID == "0" && string.IsNullOrEmpty (getUserID)) 
				{
					response = "1`0";
					//update monster cards
					using (Command(@"server_register_monster_card"))					
					{
						AddParameter ("qrid", qrid);
						AddParameter ("userID", so.UserId);
						ExecuteSpWrite ();
					}			
				} 
				else if (getCardTypeID == "1" && string.IsNullOrEmpty(getUserID)) 
				{
					response = "1`1";
					//update item cards
					using (Command(@"server_register_item_card")) 
					{
						AddParameter ("qrid", qrid);
						AddParameter ("userID", so.UserId);
						ExecuteSpWrite ();
					}
				} 
				else if (getCardTypeID == "2") 
				{
					response = "-2`" + getRemainingAttemp; // trying to register an upgrade cards
				} 
				else if (getUserID == userID) 
				{
					response = "-4`" + getRemainingAttemp; // already register by this user
				} 
				else if (getUserID != userID) 
				{
					response = "-3`" + getRemainingAttemp; // trying to register another person cards
				} 

			}

			if (response == "1`0") 
			{
				string newCard = GetMonsterStat(qrid);
				return (int)CommandResponseEnum.RegisterCardResult + "`" + response + "`" + newCard + "`";
			} 
			else if (response == "1`1")
			{
				string newCard = GetItemStat(qrid);
				return (int)CommandResponseEnum.RegisterCardResult + "`" + response + "`" + newCard + "`";
			}
			else
			{
				return (int)CommandResponseEnum.RegisterCardResult + "`" + response + "`";
			}
		}

		public string UnregisterCard(SessionData so, string qrid)
		{
			/*
			-1 General Error - error yang client ga perlu tau / error di aplikasi server (try-catch) ato error database (koneksi/query)
			-2 Upgrade Card - upgrade card di-unregister lewat monster detail page
			-3 Owned by other User
			-4 Invalid QRID - qrid kartu ga ada ato owner kartu nya userID -1 (Davy Jones)
			-5 User Locked - too many random scan
			*/

			List<string> cardType = new List<string>();
			StringDictionary checkCardType;
			string getNewQRID = "";
			string getUserID = "" ;
			string getCardTypeID = "" ;
			string response = "";
			string getRemainingAttemp = "";
			string userID = Convert.ToString(so.UserId);

			using (Command(@"server_check_card_type")) 
			{
				AddParameter ("qrid", qrid);
				AddParameter ("uid", so.UserId);
				checkCardType = ExecuteSpRow();
				if (checkCardType != null && checkCardType.Count > 0) 
				{
					getCardTypeID = checkCardType["_card_type_id"];
					getUserID = checkCardType["_user_id"];
					getRemainingAttemp = checkCardType["_remaining_attempts"];
					//Console.WriteLine(getCardTypeID + "~" + getUserID);
				} 
				else
				{
					response = "-4`0";
				}
			}

			if (string.IsNullOrEmpty (response)) {

				if(getCardTypeID == "-1")
				{
					response = "-5"; // lock user from scanning qrid
				}
				else if (getCardTypeID == "0" && getUserID == userID) 
				{
					response = "1`0";
					//update monster cards
					using (Command(@"server_unregister_monster_card"))					
					{
						AddParameter ("qrid", qrid);
						AddParameter ("userID", so.UserId);
						ExecuteSpWrite ();
					}							
				} 
				else if (getCardTypeID == "1" || getCardTypeID == "2") 
				{
					response = "-2`" + getRemainingAttemp; // unregister selain monster cards
				} 
				else if (getUserID != userID) 
				{
					response = "-3`" + getRemainingAttemp; // not owned by user
				}

			}

			if (response == "1`0") 
			{
				// tambahin balikin url buat di kasih ke user baru
				// flow: user A unregister-> di unregister oleh server -> send request ke web dnx buat generate qrid 
				// -> website kirim url qrid -> server terima response -> server kirim ke user A
				using (Command (@"server_update_monster_qrid")) 
				{
					AddParameter ("qrid", qrid);
					getNewQRID = ExecuteSpRow()["_qrid"];
				}

				string url = new WebsiteRequest().WebRequestQRID(getNewQRID);
				return (int)CommandResponseEnum.UnregisterCardResult + "`" + response + "`" + qrid + "`" + url + "`";
			} 
			else 
			{
				return (int)CommandResponseEnum.UnregisterCardResult + "`" + response + "`";	
			}

			//return (int)CommandResponseEnum.UnregisterCardResult;
		}

		public string RegisterUpgradeCard(SessionData so, string upqrid, string qrid)
		{
			/*
			UPGRADE ERROR CODES
			-1 = GENERAL ERROR
			-2 = CARD IS NOT UPGRADE CARD
			-3 = ALREADY REGISTERED TO OTHER MONSTER
			-4 = ALREADY REGISTERED TO THIS MONSTER
			-5 = INCOMPATIBLE
			-6 = INVALID QRID
			-7 = FEATURE LOCKED
			*/
			StringDictionary checkUpgradeCard;
			StringDictionary ownerUpgradeCard;
			StringDictionary checkCompatible;
			string getUserID = "" ;
			string getOwner = "";
			string getCardTypeID = "" ;
			string response = "";
			string getRemainingAttemp = "";
			string userID = Convert.ToString(so.UserId);
			bool isAvailable = false;

			using (Command(@"server_check_card_type")) 
			{
				AddParameter ("qrid", upqrid);
				AddParameter ("uid", so.UserId);
				checkUpgradeCard = ExecuteSpRow();
				if (checkUpgradeCard != null && checkUpgradeCard.Count > 0) 
				{
					getCardTypeID = checkUpgradeCard["_card_type_id"];
					getUserID = checkUpgradeCard["_user_id"];
					getRemainingAttemp = checkUpgradeCard["_remaining_attempts"];
					Console.WriteLine(getCardTypeID + "~" + getUserID);
				} 
				else
				{
					response = "-6`0"; // invalid qrid
				}
			}

			if (string.IsNullOrEmpty(response)) {

				if (getCardTypeID == "0" || getCardTypeID == "1") {
					response = "-2`" + getRemainingAttemp; // not upgrade card
				} else if (getCardTypeID == "-1") {
					response = "-7"; // lock user from scanning qrid
				} else {
					using (Command (@"server_check_upgrade_card_owner")) {

						AddParameter ("qrid", upqrid);
						ownerUpgradeCard = ExecuteSpRow ();
						if (ownerUpgradeCard != null && ownerUpgradeCard.Count > 0) {
							getOwner = ownerUpgradeCard["_mons_card_qrid"];
						} 
						else 
						{
							isAvailable = true;
						}
					}

					if (getOwner == qrid) {
						// registered to this user
						response = "-4`" + getRemainingAttemp;
					}
					else 
					{
						//registered to other user
						response = "-3`" + getRemainingAttemp;
					}

					if (isAvailable == true) 
					{
						// check compatible
						using (Command (@"server_check_compatible_upgrade_card")) {

							AddParameter ("qrid", qrid);
							AddParameter ("up_qrid", upqrid);
							checkCompatible = ExecuteSpRow ();
							if (checkCompatible != null && checkCompatible.Count > 0) {							

								response = "1";
							} 
							else 
							{
								// null response: incompatible
								response = "-5`" + getRemainingAttemp;
							}
						}
						//if null incompatible else registered
					}
				}
			}

			//do query to check upgrade card registered or not
			if (response == "1") 
			{
				//kalo success register
				using (Command (@"server_register_upgrade_card")) {
					AddParameter ("qrid", qrid);
					AddParameter ("up_qrid", upqrid);
					AddParameter ("userid", so.UserId);
					ExecuteSpWrite();
				}

				string success = new InventoryAction ().GetInventory (so, qrid);
				return success;
			} 
			else 
			{
				return (int)CommandResponseEnum.UpgradeMonsterResult + "`" + response + "`";
			}
		}

		public string GetMonsterStat(string MonsterQRID)
		{
			Command (@"server_get_monster_card");
			{
				AddParameter ("monster_qrid", MonsterQRID);
				List<string> monsters = new List<string> ();

				StringDictionary result = ExecuteSpRow ();
					List<string> mons = new List<string> ();

					mons.Add (result ["_mons_card_qrid"]);
					mons.Add ("0"); // monster card type
					mons.Add (result ["_mons_tmplt_id"]);
					mons.Add (result ["_mons_tmplt_name"]);
					mons.Add (result ["_version"]);
					mons.Add (result ["_subversion"]);
					mons.Add (result ["_hp"]);
					mons.Add (result ["_hp_regen"]);
					mons.Add (result ["_hp_regen_pm"]);
					mons.Add (result ["_mp"]);
					mons.Add (result ["_mp_regen"]);
					mons.Add (result ["_mp_regen_pm"]);
					mons.Add (result ["_spd"]);
					mons.Add (result ["_spd_pm"]);
					mons.Add (result ["_p_atk"]);
					mons.Add (result ["_p_atk_pm"]);
					mons.Add (result ["_m_atk"]);
					mons.Add (result ["_m_atk_pm"]);
					mons.Add (result ["_p_def"]);
					mons.Add (result ["_p_def_pm"]);
					mons.Add (result ["_m_def"]);
					mons.Add (result ["_m_def_pm"]);
					mons.Add (result ["_acc"]);
					mons.Add (result ["_acc_pm"]);

					mons.Add (result ["_eva"]);
					mons.Add (result ["_eva_pm"]);
					mons.Add (result ["_cri"]);
					mons.Add (result ["_cri_pm"]);
					mons.Add (result ["_f_legs"]);
					mons.Add (result ["_r_legs"]);
					mons.Add (result ["_tail"]);
					mons.Add (result ["_w_slot"]);
					mons.Add (result ["_l_slot"]);
					mons.Add (result ["_r_slot"]);
					mons.Add (result ["_wins"]);
					mons.Add (result ["_loses"]);

					mons.Add (result ["_hunger"]);
					mons.Add (result ["_happiness"]);
					mons.Add (result ["_clean"]);
					mons.Add (result ["_discipline"]);
					mons.Add (result ["_sick"]);

					mons.Add (result ["_exp"]);
					mons.Add (result ["_exp_needed"]);

					mons.Add (result["_expired_date"]);
					mons.Add (result["_is_printed"]);

					string monsString = String.Join ("~", mons);
					monsters.Add (monsString);

				string monsterData = String.Join ("`", monsters);
				return monsterData;
			}
		}

		public string GetItemStat(string itemQRID)
		{
			Command (@"server_get_item_card");
			{
				AddParameter ("item_qrid", itemQRID);
				List<string> itemList = new List<string> ();

				foreach (StringDictionary result in ExecuteSpRead()) {
					List<string> item = new List<string> ();

					item.Add (result ["_item_card_qrid"]);
					item.Add ("1"); // monster card type
					item.Add (result ["_item_tmplt_id"]);
					item.Add (result ["_qty"]);

					string itemString = String.Join ("~", item);
					itemList.Add (itemString);
				}
				string itemData = String.Join ("`", itemList);
				return itemData;
			}
		}

		public string GetUserMonsterCards(SessionData so)
		{
			string userID = Convert.ToString(so.UserId);

			StringBuilder builder = new StringBuilder ();

			string monsterString = new MonsterAction ().GetMonsterData (so);
			string itemString = new ItemAction().GetItems(so);

			if (string.IsNullOrEmpty(monsterString) && string.IsNullOrEmpty(itemString)) 
			{
				return ((int)CommandResponseEnum.LoadCard).ToString() + "``" ;
			}

			builder.Append ((int)CommandResponseEnum.LoadCard);

			if (monsterString.Length != 0) {
				builder.Append ("`");
				builder.Append (monsterString);
			}

			if (itemString.Length != 0) {
				builder.Append ("`");
				builder.Append (itemString);
			}
			builder.Append ("`");

			return builder.ToString();
		}
	}
}

