using System;

namespace DNXServer
{
	public class SkillCooldown
	{
		public int rounds;
		public int SkillId;

		public SkillCooldown (int round, int skill_id)
		{
			this.rounds = round;
			this.SkillId = skill_id;
		}
	}
}

