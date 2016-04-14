using System;
using DNXServer.Action;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;

namespace DNXServer
{
	public class SuddenDeathManager : BaseAction
	{
		public static List <SuddenDeath> ListSD = new List<SuddenDeath>();
		public static Dictionary<int,SuddenDeath> SD_Dictionary = new Dictionary<int,SuddenDeath>();

		public SuddenDeathManager ()
		{
		}

		public void Initialize()
		{
			using (Command ("SELECT skill_id FROM sudden_death")) {
				foreach (StringDictionary query in SilentExecuteRead()) {
					int index = Convert.ToInt32(query ["skill_id"]) -1;
					SuddenDeath sd = new SuddenDeath (query ["skill_id"]);
					SD_Dictionary.Add(index,sd) ;
				}
			}
		}

		public void CalculateSuddenDeath(int index, BattleMonsterData monsterSource, BattleMonsterData monsterTarget)
		{
			Log.Info ("calculating sudden death: "+ SD_Dictionary[index].SkillName +" for: " + monsterSource.MonsterTemplateName);

			int round;
			int toSelfContinuousHP = 0;
			int toSelfContinuousMP = 0;
			int toSelfContinuousHPRegen = 0;
			int toSelfContinuousMPRegen = 0;
			int toSelfContinuousPATK = 0;
			int toSelfContinuousPDEF = 0;
			int toSelfContinuousMATK = 0;
			int toSelfContinuousMDEF = 0;
			int toSelfContinuousSPD = 0;
			int toSelfContinuousACC = 0;
			int toSelfContinuousEVA = 0;
			int toSelfContinuousCRIT = 0;
			int toOpponentContinuousHP = 0;
			int toOpponentContinuousMP = 0;
			int toOpponentContinuousMPRegen = 0;
			int toOpponentContinuousHPRegen = 0;
			int toOpponentContinuousPATK = 0;
			int toOpponentContinuousMATK = 0;
			int toOpponentContinuousPDEF = 0;
			int toOpponentContinuousMDEF = 0;
			int toOpponentContinuousSPD = 0;
			int toOpponentContinuousACC = 0;
			int toOpponentContinuousEVA = 0;
			int toOpponentContinuousCRIT = 0;

			switch(SD_Dictionary[index].Category)
			{
			case 1: 
				monsterTarget.HP += (int)Util.RandomDouble(SD_Dictionary[index].ToOpponentHPMin, SD_Dictionary[index].ToOpponentHPMax);
				monsterTarget.MP +=(int)Util.RandomDouble(SD_Dictionary[index].ToOpponentMPMin, SD_Dictionary[index].ToOpponentMPMax);
				break;

			case 4: 
				Log.Info ("category 4");
				goto case 1;

			case 8:
				Log.Info ("category 8");
				bool isExistOnMonster = false;
				foreach (var activeBuff in monsterTarget.activeBuffList)
				{
					if(SD_Dictionary[index].ToOpponentStatus == 1 && activeBuff.isStun)
					{
						isExistOnMonster = true;
						break;
					}

					if(SD_Dictionary[index].ToOpponentStatus == 2 && activeBuff.isParalyze)
					{
						isExistOnMonster = true;
						break;
					}

					if(SD_Dictionary[index].ToOpponentStatus == 3 && activeBuff.isPoison)
					{
						isExistOnMonster = true;
						break;
					}
				}

				if(!isExistOnMonster)
				{
					round = SD_Dictionary [index].EffectLifetime;
					monsterTarget.ApplyBuff (new BuffData (toOpponentContinuousHP, toOpponentContinuousHPRegen, toOpponentContinuousMP,
						toOpponentContinuousMPRegen, toOpponentContinuousPATK, toOpponentContinuousMATK, toOpponentContinuousPDEF, toOpponentContinuousMDEF,
						toOpponentContinuousSPD, toOpponentContinuousACC, toOpponentContinuousEVA, toOpponentContinuousCRIT, round,
						SD_Dictionary [index].SkillName,SD_Dictionary [index].Category, SD_Dictionary [index].ToOpponentStatus
						,false));
				}
				goto case 1;

			case 9:				 
				Log.Info ("category 9 goto 1");
				round = Convert.ToInt32 (SD_Dictionary [index].EffectLifetime);

				toSelfContinuousHP = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousHPMin, SD_Dictionary [index].ToSelfContinuousHPMax));
				toSelfContinuousHPRegen = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousHPRegenMin, SD_Dictionary [index].ToSelfContinuousHPRegenMax));
				toSelfContinuousMP = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousMPMin, SD_Dictionary [index].ToSelfContinuousMPMax));
				toSelfContinuousMPRegen = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousMPRegenMin, SD_Dictionary [index].ToSelfContinuousMPRegenMax));
				toSelfContinuousSPD = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousSPDMin, SD_Dictionary [index].ToSelfContinuousSPDMax));
				toSelfContinuousPATK = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousPATKMin, SD_Dictionary [index].ToSelfContinuousPATKMax));
				toSelfContinuousMATK = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousMATKMin, SD_Dictionary [index].ToSelfContinuousMATKMax));
				toSelfContinuousPDEF = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousPDEFMin, SD_Dictionary [index].ToSelfContinuousPDEFMin));
				toSelfContinuousMDEF = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousMDEFMin, SD_Dictionary [index].ToSelfContinuousMDEFMax));
				toSelfContinuousACC = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousACCMin, SD_Dictionary [index].ToSelfContinuousACCMax));
				toSelfContinuousEVA = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousEVAMin, SD_Dictionary [index].ToSelfContinuousEVAMax));
				toSelfContinuousCRIT = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToSelfContinuousCRITMin, SD_Dictionary [index].ToSelfContinuousCRITMax));

				toOpponentContinuousHP = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary [index].ToOpponentContinuousHPMin, SD_Dictionary [index].ToOpponentContinuousHPMax));
				toOpponentContinuousHPRegen = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousHPRegenMin, SD_Dictionary[index].ToOpponentContinuousHPRegenMax));
				toOpponentContinuousMP = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousMPMin, SD_Dictionary[index].ToOpponentContinuousMPMax));
				toOpponentContinuousMPRegen = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousMPRegenMin, SD_Dictionary[index].ToOpponentContinuousMPRegenMax));
				toOpponentContinuousSPD = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousSPDMin, SD_Dictionary[index].ToOpponentContinuousSPDMax));
				toOpponentContinuousPATK = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousPATKMin, SD_Dictionary[index].ToOpponentContinuousPATKMax));
				toOpponentContinuousMATK = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousMATKMin, SD_Dictionary[index].ToOpponentContinuousMATKMax));
				toOpponentContinuousPDEF = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousPDEFMin, SD_Dictionary[index].ToOpponentContinuousPDEFMax));
				toOpponentContinuousMDEF = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousMDEFMin, SD_Dictionary[index].ToOpponentContinuousMDEFMax));
				toOpponentContinuousACC = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousACCMin, SD_Dictionary[index].ToOpponentContinuousACCMax));
				toOpponentContinuousEVA = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousEVAMin, SD_Dictionary[index].ToOpponentContinuousEVAMax));
				toOpponentContinuousCRIT = Convert.ToInt32 (Util.RandomDouble (SD_Dictionary[index].ToOpponentContinuousCRITMin, SD_Dictionary[index].ToOpponentContinuousCRITMax));

				if(toSelfContinuousHP != 0 || toSelfContinuousHPRegen != 0 || toSelfContinuousMP != 0 || toSelfContinuousMPRegen != 0 || 
					toSelfContinuousSPD != 0 || toSelfContinuousPATK != 0 || toSelfContinuousMATK != 0 || toSelfContinuousPDEF != 0 ||
					toSelfContinuousMDEF != 0 || toSelfContinuousACC != 0 || toSelfContinuousEVA != 0 || toSelfContinuousCRIT != 0)
				{

					monsterSource.ApplyBuff (new BuffData (toSelfContinuousHP, toSelfContinuousHPRegen, toSelfContinuousMP,
						toSelfContinuousMPRegen, toSelfContinuousPATK, toSelfContinuousMATK, toSelfContinuousPDEF, toSelfContinuousMDEF,
						toSelfContinuousSPD, toSelfContinuousACC, toSelfContinuousEVA, toSelfContinuousCRIT, round,
						SD_Dictionary [index].SkillName,SD_Dictionary [index].Category, SD_Dictionary [index].ToOpponentStatus,true));
				}

				if(toOpponentContinuousHP != 0 || toOpponentContinuousHPRegen != 0 || toOpponentContinuousMP != 0 || toOpponentContinuousMPRegen != 0 ||
					toOpponentContinuousSPD != 0 || toOpponentContinuousPATK != 0 || toOpponentContinuousMATK != 0 || toOpponentContinuousPDEF != 0 ||
					toOpponentContinuousMDEF != 0 || toOpponentContinuousACC != 0 || toOpponentContinuousEVA != 0 || toOpponentContinuousCRIT != 0)
				{

					monsterTarget.ApplyBuff (new BuffData (toOpponentContinuousHP, toOpponentContinuousHPRegen, toOpponentContinuousMP,
						toOpponentContinuousMPRegen, toOpponentContinuousPATK, toOpponentContinuousMATK, toOpponentContinuousPDEF, toOpponentContinuousMDEF,
						toOpponentContinuousSPD, toOpponentContinuousACC, toOpponentContinuousEVA, toOpponentContinuousCRIT, round,
						SD_Dictionary [index].SkillName,SD_Dictionary [index].Category, SD_Dictionary [index].ToOpponentStatus,true));
				}				
				goto case 1;
			
			default:
				break;
			}
		}
	}
}

