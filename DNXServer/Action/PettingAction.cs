using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DNXServer.Action
{
	public class PettingAction : BaseAction
	{
		public PettingAction ()
		{

		}

		public string PettingResult(SessionData so, string qrid, string petEnum)
		{
			//PettingAction`QRID~enumPettingAction`

			int exp;
			int exp_needed;
			int hunger;
			int happiness;
			int clean;
			int discipline;
			int sick;
			float multiplier;
			StringDictionary monsterData;

			using(Command(@"
				SELECT user_id, hunger, happiness, clean, discipline, sick, exp_needed, exp, exp_mult, version, subversion,
					hp, mp, p_atk, m_atk, p_def, m_def, acc, eva, mons_card_id
				 FROM monster_cards WHERE mons_card_qrid = :qrid
			"))
			{
				AddParameter("qrid", qrid);
				monsterData = ExecuteRow();
				exp = Convert.ToInt32(monsterData["exp"]);
				exp_needed = Convert.ToInt32(monsterData["exp_needed"]);
				hunger = Convert.ToInt32(monsterData["hunger"]);
				happiness = Convert.ToInt32(monsterData["happiness"]);
				clean = Convert.ToInt32(monsterData["clean"]);
				discipline = Convert.ToInt32(monsterData["discipline"]);
				sick = Convert.ToInt32(monsterData["sick"]);
				multiplier = Convert.ToSingle(monsterData["exp_mult"]);

			}

			DNXMessage message = new DNXMessage(petEnum);
			PettingEnum command = (PettingEnum) Convert.ToInt32(message.GetValue(0));

			switch(command)
			{
				case PettingEnum.Feed:					
				hunger += 50;
				happiness += 15;
				clean -= 25;
				discipline -= 10;
				sick -= 15;
				exp += 35;
				break;

				case PettingEnum.Play:					
				hunger -= 25;
				happiness += 50;
				clean -= 25;
				discipline -= 15;
				sick -= 5;
				exp += 35;
				break;

				case PettingEnum.Bath:					
				hunger -= 5;
				happiness += 25;
				clean += 50;
				discipline += 25;
				sick += 5;
				exp += 30;
				break;

				case PettingEnum.Scold:	
				hunger -= 5;				
				happiness -= 35;
				discipline += 50;
				exp += 5;
				break;

				case PettingEnum.Heal:					
				hunger += 25;			
				happiness += 5;
				clean -= 5;
				discipline -= 5;
				sick += 50;
				exp += 95;
				break;									
			}

			hunger = Util.Clamp (hunger, 0, 100);
			happiness = Util.Clamp (happiness, 0, 100);
			clean = Util.Clamp (clean, 0, 100);
			discipline = Util.Clamp (discipline, 0, 100);
			sick = Util.Clamp (sick, 0, 100);

			Console.WriteLine ("EXP: " + exp);
			if (exp >= exp_needed) 
			{
				exp = exp - exp_needed;
				int nextLvl = Convert.ToInt32(monsterData["version"]) * 10 + Convert.ToInt32(monsterData["subversion"]) + 1;
				int nextExpNeeded = 0;
				int version = Convert.ToInt32(nextLvl.ToString().Substring(0, 1));
				int subversion = Convert.ToInt32(nextLvl.ToString().Substring(1,1));
								
				if(nextLvl < 30)
				{
					using(Command(@"
						SELECT level, exp
						FROM base_exp_req
						WHERE level = :lvl
					"))
					{
						AddParameter("lvl", nextLvl);
						nextExpNeeded = Convert.ToInt32(ExecuteRow()["exp"]);
					}
				}

				nextExpNeeded = Convert.ToInt32 (nextExpNeeded * multiplier);
				
				byte[] evaqri = new byte[2];
				new Random().NextBytes(evaqri);

				int eva = evaqri[0] < 15 ? 1 : 0;
				int cri = evaqri[1] < 15 ? 1 : 0;
				
				using (Command(@"
				UPDATE monster_cards
				SET hunger = :hunger,
					happiness = :happiness,
					clean = :clean,
					discipline = :discipline,
					sick = :sick,
					hp = hp + 20 + round(random() * 5),
					hp_regen = hp_regen + 1,
					p_atk = p_atk + 1 + round(random() * 2),
					m_atk = m_atk + 1 + round(random() * 2),
					p_def = p_def + 1 + round(random() * 2),
					m_def = m_def + 1 + round(random() * 2),
					acc = acc + 1 + round(random()),
					eva = eva + :eva,
					cri = cri + :cri,
					spd = spd + 1 + round(random()),
					version =:version,
					subversion =:subversion,
					exp = :exp,
					exp_needed = :exp_needed						
				WHERE mons_card_qrid = :qrid
				")) 
				{
					AddParameter ("qrid", qrid);
					AddParameter ("hunger", hunger);
					AddParameter ("happiness", happiness);
					AddParameter ("clean", clean);
					AddParameter ("discipline", discipline);
					AddParameter ("sick", sick);
					AddParameter("eva", eva);
					AddParameter("cri", cri);
					AddParameter ("version", version);
					AddParameter ("subversion", subversion);
					AddParameter ("exp", exp);
					AddParameter ("exp_needed", nextExpNeeded);					
					ExecuteWrite ();
				}
			} 
			else 
			{
				using (Command(@"
				UPDATE monster_cards
				SET hunger = :hunger,
					happiness = :happiness,
					clean = :clean,
					discipline = :discipline,
					sick = :sick,
					exp = :exp						
				WHERE mons_card_qrid = :qrid
				")) 
				{
					AddParameter ("qrid", qrid);
					AddParameter ("hunger", hunger);
					AddParameter ("happiness", happiness);
					AddParameter ("clean", clean);
					AddParameter ("discipline", discipline);
					AddParameter ("sick", sick);
					AddParameter ("exp", exp);											
					ExecuteWrite ();
				}
			}
			string final = new CardAction().GetMonsterStat(qrid);

			return (int)CommandResponseEnum.EditCard + "`" + final + "`";
		}
	}
}



