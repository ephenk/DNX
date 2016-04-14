using System;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace DNXServer.Action
{
	public class DropPettingAction: BaseAction
	{
		public DropPettingAction()
		{
		}

		public string DropMonsterStat(SessionData so, string qrid)
		{
			using (Command(@"
					UPDATE monster_cards
					SET hunger = greatest(hunger - round(random() * 25), 0),
						happiness = greatest(happiness - round(random() * 25), 0),
						clean = greatest(clean - round(random() * 25), 0),
						discipline = greatest(discipline - round(random() * 25), 0),
						sick = greatest(sick - round(random() * 25), 0)
					WHERE mons_card_qrid = :qrid
				")) 
			{
				AddParameter ("qrid",qrid);
				ExecuteWrite();
			}

			string final = new CardAction().GetMonsterStat(qrid);

			return (int)CommandResponseEnum.EditCard + "`" + final + "`";
		}
	}

}