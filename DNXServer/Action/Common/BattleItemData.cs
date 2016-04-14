using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DNXServer.Action
{
	public class BattleItemData : BaseAction
	{
		public string ItemQRID;
		public int ItemID;
		public int UserID;
		public int ItemTemplate;
		public int ItemCategory;
		public string ItemName;
		public int ItemQty;
		public int isPrinted;
		public string expiredDate;
		public int ToSelfHP;
		public int ToSelfHPRegen;
		public int ToSelfMP;
		public int ToSelfMPRegen;
		public int ToSelfPATK;
		public int ToSelfMATK;
		public int ToSelfPDEF;
		public int ToSelfMDEF;
		public int ToSelfSPD;
		public int ToSelfACC;
		public int ToSelfEVA;
		public int ToSelfCRIT;
		public int ToSelfContinuousHP;
		public int ToSelfContinuousHPRegen;
		public int ToSelfContinuousMP;
		public int ToSelfContinuousMPRegen;
		public int ToSelfContinuousPATK;
		public int ToSelfContinuousMATK;
		public int ToSelfContinuousPDEF;
		public int ToSelfContinuousMDEF;
		public int ToSelfContinuousSPD;
		public int ToSelfContinuousACC;
		public int ToSelfContinuousEVA;
		public int ToSelfContinuousCRIT;
		public int EffectLifetime;

		public BattleItemData (string ItemQRID)
		{
			StringDictionary item;
			using (Command (@"server_get_battle_item_data")) 
			{
				AddParameter("item", ItemQRID);
				item = ExecuteSpRow();
			}
			this.UserID = Convert.ToInt32 (item["_user_id"]);
			this.ItemQRID = ItemQRID;
			this.ItemName = item["_item_name"];
			this.ItemID = Convert.ToInt32 (item["_item_card_id"]);
			this.ItemTemplate = Convert.ToInt32 (item["_item_tmplt_id"]);
			this.ItemQty = Convert.ToInt32 (item["_qty"]);
			this.isPrinted = Convert.ToInt32 (item ["_is_printed"]);
			this.expiredDate = item ["_expired_date"];
			this.ItemCategory = Convert.ToInt32 (item ["_item_category"]);
			this.ToSelfHP = Convert.ToInt32 (item ["_to_self_hp"]);
			this.ToSelfHPRegen = Convert.ToInt32 (item ["_to_self_hp_regen"]);
			this.ToSelfMP = Convert.ToInt32 (item ["_to_self_mp"]);
			this.ToSelfMPRegen = Convert.ToInt32 (item ["_to_self_mp_regen"]);
			this.ToSelfPATK = Convert.ToInt32 (item ["_to_self_patk"]);
			this.ToSelfMATK = Convert.ToInt32 (item ["_to_self_matk"]);
			this.ToSelfPDEF = Convert.ToInt32 (item ["_to_self_pdef"]);
			this.ToSelfMDEF = Convert.ToInt32 (item ["_to_self_mdef"]);
			this.ToSelfSPD = Convert.ToInt32 (item ["_to_self_move_speed"]);
			this.ToSelfACC = Convert.ToInt32 ( item ["_to_self_acc"]);
			this.ToSelfEVA = Convert.ToInt32 (item ["_to_self_eva"]);
			this.ToSelfCRIT = Convert.ToInt32 (item ["_to_self_crit"]);
			this.ToSelfContinuousHP = Convert.ToInt32 (item ["_to_self_continuous_hp"]);
			this.ToSelfContinuousHPRegen = Convert.ToInt32 (item ["_to_self_continuous_hp_regen"]);
			this.ToSelfContinuousMP = Convert.ToInt32 (item ["_to_self_continuous_mp"]);
			this.ToSelfContinuousMPRegen = Convert.ToInt32 (item ["_to_self_continuous_mp_regen"]);
			this.ToSelfContinuousPATK = Convert.ToInt32 (item ["_to_self_continuous_patk"]);
			this.ToSelfContinuousMATK = Convert.ToInt32 (item ["_to_self_continuous_matk"]);
			this.ToSelfContinuousPDEF = Convert.ToInt32 (item ["_to_self_continuous_pdef"]);
			this.ToSelfContinuousMDEF = Convert.ToInt32 (item ["_to_self_continuous_mdef"]);
			this.ToSelfContinuousSPD = Convert.ToInt32 (item ["_to_self_continuous_move_speed"]);
			this.ToSelfContinuousACC = Convert.ToInt32 (item ["_to_self_continuous_acc"]);
			this.ToSelfContinuousEVA = Convert.ToInt32 (item ["_to_self_continuous_eva"]);
			this.ToSelfContinuousCRIT = Convert.ToInt32 (item ["_to_self_continuous_crit"]);
			this.EffectLifetime = Convert.ToInt32(item["_effect_lifetime"]);
		}

		public string GetItemData()
		{
			List<string> item = new List<string>();
			item.Add (this.ItemQRID);
			item.Add ("1");
			item.Add(this.ItemTemplate.ToString());
			item.Add (this.ItemQty.ToString());
			item.Add (this.isPrinted.ToString ());
			item.Add (this.expiredDate);
		
			string final = String.Join("~", item);
			Log.Debug (final);
			return final;
		}

	}
}

