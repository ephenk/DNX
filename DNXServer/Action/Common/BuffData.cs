using System;
using DNXServer.Action;
using System.ComponentModel;
using System.Diagnostics;
using System.Configuration;

namespace DNXServer
{
	public class BuffData
	{
		public int HP;
		public int HPRegen;
		public int MP;
		public int MPRegen;
		public int PATK;
		public int MATK;
		public int PDEF;
		public int MDEF;
		public int SPD;
		public int ACC;
		public int EVA;
		public int CRIT;
		public int Lifetime;
		public string BuffName;
		public int Category;
		public bool isStun = false;
		public bool isParalyze = false;
		public bool isPoison = false;
		public bool isContinuous;

		public BuffData (int hp, int hp_regen, int mp, int mp_regen, int patk, int matk, int pdef, int mdef, 
			int spd, int acc, int eva, int crit, int round, string buff_name, int category, int status, bool continuous)
		{
			Log.Info ("masuk buff HP: " + hp);
			this.HP = hp;
			this.HPRegen = hp_regen;
			this.MP = mp;
			this.MPRegen = mp_regen;
			this.PATK = patk;
			this.MATK = matk;
			this.PDEF = pdef;
			this.MDEF = mdef;
			this.SPD = spd;
			this.ACC = acc;
			this.EVA = eva;
			this.CRIT = crit;
			this.Lifetime = round + 1;
			this.BuffName = buff_name;
			this.Category = category;
			this.isContinuous = continuous;

			Log.Info ("Status buff: " + status);
			switch(status)
			{
			case 1:
				this.isStun = true;
				break;
			case 2:
				this.isParalyze = true;
				break;
			case 3: 
				this.isPoison = true;
				break;
			default:
				break;
			}

		}

	}
}

