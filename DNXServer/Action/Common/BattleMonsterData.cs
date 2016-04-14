using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;

namespace DNXServer.Action
{
	public class BattleMonsterData : BaseAction
	{
		//monster attributes from DB
		public Dictionary<int,BattleItemData> itemDict = new Dictionary <int,BattleItemData> ();
		public List<BuffData> activeBuffList = new List<BuffData> ();
		public List<SkillCooldown> skillCooldown = new List<SkillCooldown> ();

		public string MonsterQRID;
		public int MonsterID;
		public int UserID;
		public int MonsterTemplate;
		public string MonsterTemplateName;
		public int Version;
		public int Subversion;
		public int HP;
		public int HPRegen;
		public int HPRegen_PM;
		public int MP;
		public int MPRegen;
		public int MPRegen_PM;
		public int SPD;
		public int SPD_PM;
		public int PATK;
		public int PATK_PM;
		public int MATK;
		public int MATK_PM;
		public int PDEF;
		public int DEF_PM;
		public int MDEF;
		public int MDEF_PM;
		public int ACC;
		public int ACC_PM;
		public int EVA;
		public int EVA_PM;
		public int CRIT;
		public int CRIT_PM;
		public int Hunger;
		public int Happiness;
		public int Clean;
		public int Discipline;
		public int Sick;
		public int EXP;
		public int EXP_Needed;
		public double EXP_Mult;
		public int Win;
		public int Lose;
		public int F_Legs;
		public int R_Legs;
		public int Wings;
		public int Tail;
		public int L_Slot;
		public int R_Slot;
		public int isPrinted;
		public string expiredDate;

		//BATTLE STATUS
		public bool isStunned;
		public bool isParalyzed;
		public bool isPoisoned;
		public int CurrentHP;
		public int CurrentHPRegen;
		public int CurrentMP;
		public int CurrentMPRegen;
		public int CurrentPATK;
		public int CurrentPDEF;
		public int CurrentMATK;
		public int CurrentMDEF;
		public int CurrentSPD;
		public int CurrentACC;
		public int CurrentEVA;
		public int CurrentCRIT;

		public List<string> defaultSkill = new List <string> ();
		public List<string> mammalFLSkill = new List <string> ();
		public List<string> mammalTailSkill = new List <string> ();
		public List<string> insectFLSkill = new List <string> ();
		public List<string> insectTailSkill = new List <string> ();
		public List<string> ultimateSkill = new List <string> ();
	
		public BattleMonsterData (string monsterQRID)
		{
			StringDictionary mons;
			using (Command ("server_get_battle_monster_data")) 
			{
				AddParameter("mons", monsterQRID);
				mons = ExecuteSpRow();
			}
			this.MonsterQRID = monsterQRID;
			this.MonsterID = Convert.ToInt32(mons["_mons_card_id"]);
			this.UserID = Convert.ToInt32(mons["_user_id"]);
			this.MonsterTemplate = Convert.ToInt32(mons["_mons_tmplt_id"]);
			this.MonsterTemplateName = mons["_mon_tmplt_name"];
			this.Version = Convert.ToInt32(mons["_version"]);
			this.Subversion = Convert.ToInt32(mons["_subversion"]);
			this.HP = Convert.ToInt32(mons["_hp"]);
			this.HPRegen = Convert.ToInt32(mons["_hp_regen"]);
			this.HPRegen_PM = Convert.ToInt32(mons["_hp_regen_pm"]);
			this.MP = Convert.ToInt32(mons["_mp"]);
			this.MPRegen = Convert.ToInt32(mons["_mp_regen"]);
			this.MPRegen_PM = Convert.ToInt32(mons["_mp_regen_pm"]);
			this.SPD = Convert.ToInt32(mons["_spd"]);
			this.SPD_PM = Convert.ToInt32(mons["_spd_pm"]);
			this.PATK = Convert.ToInt32(mons["_p_atk"]);
			this.PATK_PM = Convert.ToInt32(mons["_p_atk_pm"]);
			this.MATK = Convert.ToInt32(mons["_m_atk"]);
			this.MATK_PM = Convert.ToInt32(mons["_m_atk_pm"]);
			this.PDEF = Convert.ToInt32(mons["_p_def"]);
			this.DEF_PM = Convert.ToInt32(mons["_p_def_pm"]);
			this.MDEF = Convert.ToInt32(mons["_m_def"]);
			this.MDEF_PM = Convert.ToInt32(mons["_m_def_pm"]);
			this.ACC = Convert.ToInt32(mons["_acc"]);
			this.ACC_PM = Convert.ToInt32(mons["_acc_pm"]);
			this.EVA = Convert.ToInt32(mons["_eva"]);
			this.EVA_PM = Convert.ToInt32(mons["_eva_pm"]);
			this.CRIT = Convert.ToInt32(mons["_cri"]);
			this.CRIT_PM = Convert.ToInt32(mons["_cri_pm"]);
			this.Hunger = Convert.ToInt32(mons["_hunger"]);
			this.Happiness = Convert.ToInt32(mons["_happiness"]);
			this.Clean = Convert.ToInt32(mons["_clean"]);
			this.Discipline = Convert.ToInt32(mons["_discipline"]);
			this.Sick = Convert.ToInt32(mons ["_sick"]);
			this.EXP = Convert.ToInt32(mons["_exp"]);
			this.EXP_Needed = Convert.ToInt32(mons["_exp_needed"]);
			this.EXP_Mult = Convert.ToDouble(mons["_exp_mult"]);
			this.Win = Convert.ToInt32(mons["_wins"]);
			this.Lose = Convert.ToInt32(mons["_loses"]);
			this.F_Legs = Convert.ToInt32(mons["_f_legs"]);
			this.R_Legs = Convert.ToInt32 (mons["_r_legs"]);
			this.L_Slot = Convert.ToInt32 (mons["_l_slot"]);
			this.R_Slot = Convert.ToInt32 (mons["_r_slot"]);
			this.Wings = Convert.ToInt32 (mons["_w_slot"]);
			this.Tail = Convert.ToInt32(mons["_tail"]);
			this.isPrinted = Convert.ToInt32 (mons ["_is_printed"]);
			this.expiredDate = mons ["_expired_date"];
			this.isParalyzed = false;
			this.isPoisoned = false;
			this.isStunned = false;

			this.CurrentHP = this.HP;
			this.CurrentMP = this.MP;
			this.CurrentHPRegen = this.HPRegen;
			this.CurrentMPRegen = this.MPRegen;
			this.CurrentPATK = this.PATK;
			this.CurrentPDEF = this.PDEF;
			this.CurrentMATK = this.MATK;
			this.CurrentMDEF = this.MDEF;
			this.CurrentSPD = this.SPD;
			this.CurrentACC = this.ACC;
			this.CurrentEVA = this.EVA;
			this.CurrentCRIT = this.CRIT;
		}

		public int SetBattleItems(string item1, string item2, string item3)
		{
			itemDict.Clear();
			string[] itemCards = new string[3]{item1,item2,item3};
			int response = 1;
			int result = 0;
			int count = 0;
			foreach (string item in itemCards) 
			{
				if(item != string.Empty)
				{
					result = SetBattleItem (item,count);
					if(result==0)
					{
						response = 0;
						break;
					}
					count++;
				}
			}			
			return response;
		}

		public int SetBattleItem(string itemQRID, int count)
		{

			int result = 0;
			switch (count) 
			{
			case 0:
				itemDict.Add (0, new BattleItemData (itemQRID));
				result = 1;
				break;
			case 1:
				itemDict.Add (1, new BattleItemData (itemQRID));
				result = 1;
				break;
			case 2:
				itemDict.Add (2, new BattleItemData (itemQRID));
				result = 1;
				break;

			default:
				result = 0;
				break;
			}
			return result;
		}

		public string GetMonsterData()
		{
			List<string> mons = new List<string>();

			mons.Add(this.MonsterQRID);
			mons.Add("0"); // monster card type
			mons.Add(Convert.ToString(this.MonsterTemplate));
			mons.Add(this.MonsterTemplateName);
			mons.Add(this.Version.ToString());
			mons.Add(this.Subversion.ToString());
			mons.Add(this.HP.ToString());
			mons.Add(Convert.ToString(this.HPRegen));
			mons.Add(Convert.ToString(this.HPRegen_PM));
			mons.Add(Convert.ToString(this.MP));
			mons.Add(Convert.ToString(this.MPRegen));
			mons.Add(Convert.ToString(this.MPRegen_PM));
			mons.Add(Convert.ToString(this.SPD));
			mons.Add(Convert.ToString(this.SPD_PM));
			mons.Add(Convert.ToString(this.PATK));
			mons.Add(Convert.ToString(this.PATK_PM));
			mons.Add(Convert.ToString(this.MATK));
			mons.Add(Convert.ToString(this.MATK_PM));
			mons.Add(Convert.ToString(this.PDEF));
			mons.Add(Convert.ToString(this.DEF_PM));
			mons.Add(Convert.ToString(this.MDEF));
			mons.Add(Convert.ToString(this.MDEF_PM));
			mons.Add(Convert.ToString(this.ACC));
			mons.Add(Convert.ToString(this.ACC_PM));

			mons.Add(Convert.ToString(this.EVA));
			mons.Add(Convert.ToString(this.EVA_PM));
			mons.Add(Convert.ToString(this.CRIT));
			mons.Add(Convert.ToString(this.CRIT_PM));
			mons.Add(Convert.ToString(this.F_Legs));
			mons.Add(Convert.ToString(this.R_Legs));
			mons.Add(Convert.ToString(this.Tail));
			mons.Add(Convert.ToString(this.Wings));
			mons.Add(Convert.ToString(this.L_Slot));
			mons.Add(Convert.ToString(this.R_Slot));
			mons.Add(Convert.ToString(this.Win));
			mons.Add(Convert.ToString(this.Lose));

			mons.Add(Convert.ToString(this.Hunger));
			mons.Add(Convert.ToString(this.Happiness));
			mons.Add(Convert.ToString(this.Clean));
			mons.Add(Convert.ToString(this.Discipline));
			mons.Add(Convert.ToString(this.Sick));

			mons.Add(Convert.ToString(this.EXP));
			mons.Add(Convert.ToString(this.EXP_Needed));

			mons.Add (Convert.ToString(this.isPrinted));
			mons.Add (this.expiredDate);

			string final = String.Join("~", mons);
			return final;
		}

		public string GetMonsterCondition()
		{
			return Convert.ToInt32(isPoisoned) + "~" + Convert.ToInt32(isParalyzed) + "~" + Convert.ToInt32(isStunned);
		}

		public string GetItemData()
		{
			string final;
			List <string> itemCollection = new List<string> ();
			foreach(var dict in itemDict)
			{
				itemCollection.Add (Convert.ToString((dict.Value as BattleItemData).ItemTemplate));
			}

			if(itemCollection != null)
			{

				final = String.Join ("~", itemCollection);
			}
			else
			{
				final = "";
			}		

			return final;
		}

		public string[] GetListSkill()
		{
			List <string> skills = new List<string> ();
			using(Command(@"SELECT skill_id, slot_id FROM monster_skill WHERE monster_id = :id ORDER BY relation_id "))
			{
				AddParameter ("id", this.MonsterTemplate);

				foreach(StringDictionary temp in ExecuteRead())
				{
					switch( (SlotSkillEnum) Convert.ToInt32(temp["slot_id"]))
					{
						case SlotSkillEnum.Default:
							defaultSkill.Add (temp ["skill_id"]);
							break;

						case SlotSkillEnum.InsectFrontLeg:
							insectFLSkill.Add (temp ["skill_id"]);
							break;

						case SlotSkillEnum.InsectTail:
							insectTailSkill.Add (temp ["skill_id"]);
							break;

						case SlotSkillEnum.MammalFrontLeg:
							mammalFLSkill.Add (temp ["skill_id"]);
							break;

						case SlotSkillEnum.MammalTail:
							mammalTailSkill.Add (temp ["skill_id"]);
							break;

						case SlotSkillEnum.Ultimate:
							ultimateSkill.Add (temp ["skill_id"]);
							break;
					}
				}
			}

			// 1. default skill
			foreach(string s in defaultSkill)
			{
				skills.Add (s);
			}

			// 2. front leg skill
			if(F_Legs == 0)
			{
				foreach(string s in mammalFLSkill)
				{
					skills.Add (s);
				}
			}
			else if (F_Legs == 2)
			{
				foreach(string s in insectFLSkill)
				{
					skills.Add (s);
				}
			}

			// 3. harusnya ada rear legs tapi blm di pakai

			// 4. tail skill
			if(Tail == 0)
			{
				foreach(string s in mammalTailSkill)
				{
					skills.Add (s);
				}
			}
			else if(Tail == 2)
			{
				foreach(string s in insectTailSkill)
				{
					skills.Add (s);
				}
			}

			//TODO 5. wings skill

			//TODO 6. weapon skill

			//ultimate skill

			if(Version == 3)
			{
				foreach(string s in ultimateSkill)
				{
					skills.Add (s);
				}
			}

			return skills.ToArray() ;
		}


//		public string[] GetListSkill()
//		{
//			List <string> skill = new List<string>();
//
//			//version 3 ultimate unlocked
//
//			if(Version == 3)
//			{
//				switch(MonsterTemplate)
//				{
//				case 0: //Aphelion
//					skill.Add ("49");
//					break;
//				case 1: //Breccia
//					skill.Add ("42");
//					break;
//				case 2: //Gibbous
//					skill.Add ("10");
//					break;
//				case 3: //Quadra
//					skill.Add ("63");
//					break;
//				case 4: //Rille
//					skill.Add ("79");
//					break;
//				case 5: //Lacus
//					skill.Add ("72");
//					break;
//				case 6: //Anorthos
//					skill.Add ("88");
//					break;
//				case 7: //Nox
//					skill.Add ("95");
//					break;
//				}
//			}
//
//			//get skill from monster template for base skill, f_legs, and tail
//			// nanti akses nya pakai index list, cek di project dnx kiki->skill slot control.cs buat urutan nya
//			// rear legs blom ada, nanti bakal diisi skill pasif
//			//weapon & wings juga blom ada
//			switch(MonsterTemplate)
//			{
//			case 0: //Aphelion
//				skill.Add ("0");
//				skill.Add ("1");
//
//				if(F_Legs == 0)
//				{
//					skill.Add("51");
//				}
//				else if(F_Legs == 2)
//				{
//					skill.Add("50");
//				}
//
//				if(Tail == 0)
//				{
//					skill.Add("53");
//				}
//				else if (Tail == 2)
//				{
//					skill.Add ("52");
//				}
//				break;
//			case 1: //Breccia
//				skill.Add ("40");
//				skill.Add ("41");
//
//				if(F_Legs == 0)
//				{
//					skill.Add("44");
//				}
//				else if(F_Legs == 2)
//				{
//					skill.Add("43");
//				}
//
//				if(Tail == 0)
//				{
//					skill.Add("46");
//				}
//				else if (Tail == 2)
//				{
//					skill.Add ("45");
//				}
//
//				break;
//			case 2: //Gibbous
//				skill.Add ("56");
//				skill.Add ("9");
//
//				if(F_Legs == 0)
//				{
//					skill.Add("58");
//				}
//				else if(F_Legs == 2)
//				{
//					skill.Add("57");
//				}
//
//				if(Tail == 0)
//				{
//					skill.Add("60");
//				}
//				else if (Tail == 2)
//				{
//					skill.Add ("59");
//				}
//
//				break;
//			case 3: //Quadra
//				skill.Add ("13");
//				skill.Add ("14");
//
//				if(F_Legs == 0)
//				{
//					skill.Add("64");
//				}
//				else if(F_Legs == 2)
//				{
//					skill.Add("65");
//				}
//
//				if(Tail == 0)
//				{
//					skill.Add("66");
//				}
//				else if (Tail == 2)
//				{
//					skill.Add ("67");
//				}
//
//				break;
//			case 4: //Rille
//				skill.Add ("17");
//				skill.Add ("18");
//
//				if(F_Legs == 0)
//				{
//					skill.Add("80");
//				}
//				else if(F_Legs == 2)
//				{
//					skill.Add("81");
//				}
//
//				if(Tail == 0)
//				{
//					skill.Add("82");
//				}
//				else if (Tail == 2)
//				{
//					skill.Add ("83");
//				}
//
//				break;
//			case 5: //Lacus
//				skill.Add ("70");
//				skill.Add ("71");
//
//				if(F_Legs == 0)
//				{
//					skill.Add("73");
//				}
//				else if(F_Legs == 2)
//				{
//					skill.Add("74");
//				}
//
//				if(Tail == 0)
//				{
//					skill.Add("75");
//				}
//				else if (Tail == 2)
//				{
//					skill.Add ("76");
//				}
//
//				break;
//			case 6: //Anorthos
//				skill.Add ("86");
//				skill.Add ("87");
//
//				if(F_Legs == 0)
//				{
//					skill.Add("90");
//				}
//				else if(F_Legs == 2)
//				{
//					skill.Add("89");
//				}
//
//				if(Tail == 0)
//				{
//					skill.Add("92");
//				}
//				else if (Tail == 2)
//				{
//					skill.Add ("91");
//				}
//
//				break;
//			case 7: //Nox
//				skill.Add ("29");
//				skill.Add ("30");
//
//				if(F_Legs == 0)
//				{
//					skill.Add("97");
//				}
//				else if(F_Legs == 2)
//				{
//					skill.Add("96");
//				}
//
//				if(Tail == 0)
//				{
//					skill.Add("98");
//				}
//				else if (Tail == 2)
//				{
//					skill.Add ("99");
//				}
//
//				break;						
//			}
//
//			return skill.ToArray();
//		}

		public string GetAll()
		{
			List<string> mons = new List<string>();
			mons.Add (this.MonsterQRID);
			mons.Add (this.MonsterID.ToString());
			mons.Add (this.UserID.ToString());
			mons.Add(this.MonsterTemplate.ToString());
			mons.Add(this.MonsterTemplateName);				
			mons.Add(this.Version.ToString());
			mons.Add(this.Subversion.ToString());
			mons.Add(this.HP.ToString());
			mons.Add (this.HPRegen.ToString ());
			mons.Add (this.HPRegen_PM.ToString ());
			mons.Add(this.MP.ToString());
			mons.Add (this.MPRegen.ToString ());
			mons.Add (this.MPRegen_PM.ToString ());
			mons.Add(this.SPD.ToString());
			mons.Add (this.SPD_PM.ToString ());
			mons.Add(this.ACC.ToString());
			mons.Add(this.ACC_PM.ToString());
			mons.Add(this.PATK.ToString());
			mons.Add (this.PATK_PM.ToString ());
			mons.Add(this.MATK.ToString());
			mons.Add (this.MATK_PM.ToString ());
			mons.Add(this.PDEF.ToString());
			mons.Add (this.DEF_PM.ToString ());
			mons.Add(this.MDEF.ToString());
			mons.Add(this.MDEF_PM.ToString());
			mons.Add(this.EVA.ToString());
			mons.Add(this.EVA_PM.ToString());
			mons.Add(this.CRIT.ToString());
			mons.Add(this.CRIT_PM.ToString());
			mons.Add (this.Hunger.ToString ());
			mons.Add (this.Happiness.ToString ());
			mons.Add (this.Clean.ToString ());
			mons.Add (this.Discipline.ToString ());
			mons.Add (this.Sick.ToString ());
			mons.Add (this.EXP.ToString ());
			mons.Add (this.EXP_Needed.ToString ());
			mons.Add (this.EXP_Mult.ToString ());
			mons.Add (this.Win.ToString ());
			mons.Add (this.Lose.ToString ());
			mons.Add(this.F_Legs.ToString());
			mons.Add(this.R_Legs.ToString());
			mons.Add(this.L_Slot.ToString());
			mons.Add(this.R_Slot.ToString());
			mons.Add(this.Wings.ToString());
			mons.Add(this.Tail.ToString());				

			string final = String.Join("~", mons);
			return final;
		}

		public void ApplyBuff(BuffData buff)
		{
			Log.Info ("apply buff to " + this.MonsterTemplateName);

			if(!buff.isContinuous)
			{
				this.HP += buff.HP;
				this.HPRegen += buff.HPRegen;
				this.MP += buff.MP;
				this.MPRegen += buff.MPRegen;
				this.PATK += buff.PATK;
				this.MATK += buff.MATK;
				this.PDEF += buff.PDEF;
				this.MDEF += buff.MDEF;
				this.SPD += buff.SPD;
				this.ACC += buff.ACC;
				this.EVA += buff.EVA;
				this.CRIT += buff.CRIT;
			}

			if(buff.isParalyze)
			{
				this.isParalyzed = true;
			}
			if(buff.isStun)
			{
				this.isStunned = true;
			}
			if(buff.isPoison)
			{
				this.isPoisoned = true;
			}				
			activeBuffList.Add (buff);
		}

		public void RemoveBuff(BuffData buff)
		{
			Log.Info ("remove buff " + buff.BuffName + " from " + this.MonsterTemplateName);
			this.HP -= buff.HP;
			this.HPRegen -= buff.HPRegen;
			this.MP -= buff.MP;
			this.MPRegen -= buff.MPRegen;
			this.PATK -= buff.PATK;
			this.MATK -= buff.MATK;
			this.PDEF -= buff.PDEF;
			this.MDEF -= buff.MDEF;
			this.SPD -= buff.SPD;
			this.ACC -= buff.ACC;
			this.EVA -= buff.EVA;
			this.CRIT -= buff.CRIT;

			if(buff.isParalyze)
			{
				this.isParalyzed = false;
			}

			if(buff.isStun)
			{
				this.isStunned = false;
			}

			if(buff.isPoison)
			{
				this.isPoisoned = false;
			}

			activeBuffList.Remove (buff);
		}
	}
}

