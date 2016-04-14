using System;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace DNXServer.Action
{
	public class MonsterAction: BaseAction
	{
		public MonsterAction()
		{
		}

		public string GetMonsterData(SessionData so)
		{
			List<string> monsters = new List<string>();
			/*
			 * SELECT l.mons_card_id, l.mons_card_qrid, l.user_id, l.mons_tmplt_id, l.version, 
			       l.subversion, l.hp, l.hp_regen, l.hp_regen_pm, l.mp, l.mp_regen, l.mp_regen_pm, 
			       l.spd, l.spd_pm, l.p_atk, l.p_atk_pm, l.m_atk, l.m_atk_pm, l.p_def, l.p_def_pm, 
			       l.m_def, l.m_def_pm, l.acc, l.acc_pm, l.eva, l.eva_pm, l.cri, l.cri_pm, l.f_legs, 
			       l.r_legs, l.tail, l.wings, l.l_slot, l.r_slot, l.hunger, l.happiness, l.clean, 
			       l.discipline, l.sick, l.exp, l.exp_needed, l.exp_mult, l.wins, l.loses, l.is_printed,
					t.mons_tmplt_name
				FROM monster_cards l
				LEFT JOIN monster_template t
					ON t.mons_tmplt_id = l.mons_tmplt_id 
				WHERE l.user_id = :userid
				ORDER BY t.mons_tmplt_name ASC, l.version DESC, l.subversion DESC
				*/

			using(Command(@"server_get_monster_cards"))
			{
				AddParameter("owner", so.UserId);

				foreach(StringDictionary result in ExecuteSpRead())
				{
					List<string> item = new List<string>();
					item.Add(result["_mons_card_qrid"]);
					item.Add("0"); // monster card type
					item.Add(result["_mons_tmplt_id"]);
					item.Add(result["_mons_tmplt_name"]);
					item.Add(result["_version"]);
					item.Add(result["_subversion"]);
					item.Add(result["_hp"]);
					item.Add(result["_hp_regen"]);
					item.Add(result["_hp_regen_pm"]);
					item.Add(result["_mp"]);
					item.Add(result["_mp_regen"]);
					item.Add(result["_mp_regen_pm"]);
					item.Add(result["_spd"]);
					item.Add(result["_spd_pm"]);
					item.Add(result["_p_atk"]);
					item.Add(result["_p_atk_pm"]);
					item.Add(result["_m_atk"]);
					item.Add(result["_m_atk_pm"]);
					item.Add(result["_p_def"]);
					item.Add(result["_p_def_pm"]);
					item.Add(result["_m_def"]);
					item.Add(result["_m_def_pm"]);
					item.Add(result["_acc"]);
					item.Add(result["_acc_pm"]);
					
					item.Add(result["_eva"]);
					item.Add(result["_eva_pm"]);
					item.Add(result["_cri"]);
					item.Add(result["_cri_pm"]);
					item.Add(result["_f_legs"]);
					item.Add(result["_r_legs"]);
					item.Add(result["_tail"]);
					item.Add(result["_w_slot"]);
					//item.Add(result["_wings"]);
					item.Add(result["_l_slot"]);
					item.Add(result["_r_slot"]);
					item.Add(result["_wins"]);
					item.Add(result["_loses"]);
						
					item.Add(result["_hunger"]);
					item.Add(result["_happiness"]);
					item.Add(result["_clean"]);
					item.Add(result["_discipline"]);
					item.Add(result["_sick"]);
					
					item.Add(result["_exp"]);
					item.Add(result["_exp_needed"]);

					item.Add (result["_expired_date"]);
					item.Add (result["_is_printed"]);

					string itemString = String.Join("~", item);
					monsters.Add(itemString);
				}
				string final = String.Join("`", monsters);
				return final;
			}
				
		}
	}
}
