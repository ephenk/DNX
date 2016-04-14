using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DNXServer.Action
{
	public class UpdateMonsterEquipment : BaseAction
	{
		public UpdateMonsterEquipment ()
		{

		}

		public string UpdateMonster(SessionData so, string qrid, string flegs, string rlegs, string tail, string wing, string lslot, string rslot)
		{
			//UpdateMonsterEquipment`QRID~FLegs~RLegs~Tail~Wings~LSlot~RSlot`

			using (Command(@"
				UPDATE monster_cards
				SET f_legs = :f_legs,
					r_legs = :r_legs,
					tail = :tail,
					w_slot = :wing,
					l_slot = :l_slot,
					r_slot = :r_slot						
				WHERE mons_card_qrid = :qrid
				")) 
			{
				AddParameter ("qrid", qrid);
				AddParameter ("f_legs", flegs);
				AddParameter ("r_legs", rlegs);
				AddParameter ("tail", tail);
				AddParameter ("wing", wing);
				AddParameter ("l_slot", lslot);
				AddParameter ("r_slot", rslot);				
				ExecuteWrite ();
			}

			string final = new CardAction().GetMonsterStat(qrid);

			return (int)CommandResponseEnum.EditCard + "`" + final + "`";
			//return (int)CommandResponseEnum.EditCard`QRID~FLegs~RLegs~Tail~Wings~LSlot~RSlot`
			//return qrid;
		}
	}
}

