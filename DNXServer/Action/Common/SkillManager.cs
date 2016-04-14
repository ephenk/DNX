using System;
using DNXServer.Action;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;

namespace DNXServer
{
	public class SkillManager : BaseAction
	{
		//public static List <SkillData> ListSkill = new List<SkillData>();
		public static Dictionary <int, SkillData> SkillDictionary = new Dictionary<int,SkillData>();

		public SkillManager ()
		{	

		}

		public void Initialize ()
		{
			using (Command ("SELECT skill_id FROM skills")) 
			{
				foreach(StringDictionary query in SilentExecuteRead() )
				{
					int index = Convert.ToInt32 (query ["skill_id"]);
					SkillData skill = new SkillData(Convert.ToInt32 (query ["skill_id"]));
					//ListSkill.Add (skill);
					SkillDictionary.Add (Convert.ToInt32 (query ["skill_id"]), skill);
				}
			}
		}

		public string CalculateBattleDamage(int skill, int item, BattleMonsterData monsterSource, BattleMonsterData monsterTarget)
		{
			string parameters = string.Empty;

			bool isMonsterSourceCritical;
			int toMonsterSourceHP = 0;
			int toMonsterSourceHPRegen = 0;
			int toMonsterSourceMP = 0;
			int toMonsterSourceMPRegen = 0;
			int toMonsterSourcePATK = 0;
			int toMonsterSourcePDEF = 0;
			int toMonsterSourceMATK = 0;
			int toMonsterSourceMDEF = 0;
			int toMonsterSourceSPD = 0;
			int toMonsterSourceACC = 0;
			int toMonsterSourceEVA = 0;
			int toMonsterSourceCRIT = 0;
			int toMonsterTargetHP = 0;
			int toMonsterTargetMP = 0;
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
			int round = 0;

			if(monsterSource.isStunned || monsterSource.isParalyzed)
			{
				parameters += "0~0~0~0~0~0~0~0~";
			}
			else
			{
				if (item == -2 && skill > -1) 
				{
					bool isMonsterSourceHit = isHit (monsterSource, SkillDictionary[skill]);
					bool isMonsterSourceEvade = isEvade (monsterSource);

					bool isActionExecuted = (isMonsterSourceHit == true && isMonsterSourceEvade == false) ? true : false;
					Log.Info ("isActionExecuted: " + isActionExecuted);

					parameters += SkillDictionary[skill].Category + "~";
					parameters += isMonsterSourceHit ? "1~" : "0~";
					parameters += isMonsterSourceEvade ? "1~" : "0~";

					parameters += (int)Util.RandomDouble (SkillDictionary [skill].ToSelfHPMin, SkillDictionary [skill].ToSelfHPMax) + "~";
					parameters += (int)Util.RandomDouble (SkillDictionary [skill].ToSelfMPMin, SkillDictionary [skill].ToSelfMPMax) + "~";

					switch (SkillDictionary[skill].Category) {
					case 1:
						isMonsterSourceCritical = isCritical (monsterSource);
						parameters += isMonsterSourceCritical ? "1~" : "0~";

						toMonsterTargetHP = CalculateDamage (
							(int)Util.RandomDouble (SkillDictionary [skill].ToOpponentHPMin, SkillDictionary [skill].ToOpponentHPMin),
							SkillDictionary [skill].Type == 0 ? 
							(float)Util.RandomDouble (SkillDictionary [skill].ToSelfPATKMin, SkillDictionary [skill].ToSelfPATKMax) :
							(float)Util.RandomDouble (SkillDictionary [skill].ToSelfMATKMin, SkillDictionary [skill].ToSelfMATKMax),
							SkillDictionary [skill].Type == 0 ?
							monsterSource.PATK :
							monsterSource.MATK,
							isMonsterSourceCritical,
							(float)Util.RandomDouble (SkillDictionary [skill].ToSelfCRITMin, SkillDictionary [skill].ToSelfCRITMax),
							SkillDictionary [skill].Type == 0 ?
							monsterTarget.PDEF :
							monsterTarget.MDEF);

						parameters += toMonsterTargetHP.ToString () + "~";

						toMonsterTargetMP = CalculateDamage (
							(int)Util.RandomDouble (SkillDictionary [skill].ToOpponentMPMin, SkillDictionary [skill].ToOpponentMPMax),
							SkillDictionary [skill].Type == 0 ? 
							(float)Util.RandomDouble (SkillDictionary [skill].ToSelfPATKMin, SkillDictionary [skill].ToSelfPATKMax) :
							(float)Util.RandomDouble (SkillDictionary [skill].ToSelfMATKMin, SkillDictionary [skill].ToSelfMATKMax),
							SkillDictionary [skill].Type == 0 ?
							monsterSource.PATK :
							monsterSource.MATK,
							isMonsterSourceCritical,
							(float)Util.RandomDouble (SkillDictionary [skill].ToSelfCRITMin, SkillDictionary [skill].ToSelfCRITMax),
							SkillDictionary [skill].Type == 0 ?
							monsterTarget.PDEF :
							monsterTarget.MDEF);

						parameters += toMonsterTargetMP.ToString () + "~";

						if(isActionExecuted)
						{
							monsterTarget.HP += toMonsterTargetHP;
							monsterTarget.MP += toMonsterTargetMP;
						}

						if (SkillDictionary[skill].Category == 12) {
							goto case 2;
						}
						break;
					case 2:

						round = SkillDictionary [skill].EffectLifetime;

						toSelfContinuousHPRegen = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfHPRegenMin, SkillDictionary [skill].ToSelfHPRegenMax);
						toSelfContinuousMPRegen = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfMPRegenMin, SkillDictionary [skill].ToSelfMPRegenMax);
						toSelfContinuousSPD = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfEffectSPDMin, SkillDictionary [skill].ToSelfEffectSPDMax);
						toSelfContinuousPATK = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfEffectPATKMin, SkillDictionary [skill].ToSelfEffectPATKMax);
						toSelfContinuousMATK = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfEffectMATKMin, SkillDictionary [skill].ToSelfEffectMATKMax);
						toSelfContinuousPDEF = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfEffectPDEFMin, SkillDictionary [skill].ToSelfEffectPDEFMax);
						toSelfContinuousMDEF = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfEffectMDEFMin, SkillDictionary [skill].ToSelfEffectMDEFMax);
						toSelfContinuousACC = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfEffectACCMin, SkillDictionary [skill].ToSelfEffectACCMin);
						toSelfContinuousEVA = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfEffectEVAMin, SkillDictionary [skill].ToSelfEffectEVAMax);
						toSelfContinuousCRIT = (int)Util.RandomDouble (SkillDictionary [skill].ToSelfEffectCRITMin, SkillDictionary [skill].ToSelfEffectCRITMin);

						toOpponentContinuousHPRegen = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentHPRegenMin, SkillDictionary [skill].ToOpponentHPRegenMax);
						toOpponentContinuousMPRegen = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentMPRegenMin, SkillDictionary [skill].ToOpponentMPRegenMax);
						toOpponentContinuousSPD = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentSPDMin, SkillDictionary [skill].ToOpponentSPDMax);
						toOpponentContinuousPATK = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentPATKMin, SkillDictionary [skill].ToOpponentPATKMax);
						toOpponentContinuousMATK = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentMATKMin, SkillDictionary [skill].ToOpponentMATKMax);
						toOpponentContinuousPDEF = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentPDEFMin, SkillDictionary [skill].ToOpponentPDEFMax);
						toOpponentContinuousMDEF = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentMDEFMin, SkillDictionary [skill].ToOpponentMDEFMax);
						toOpponentContinuousACC = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentACCMin, SkillDictionary [skill].ToOpponentACCMin);
						toOpponentContinuousEVA = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentEVAMin, SkillDictionary [skill].ToOpponentEVAMax);
						toOpponentContinuousCRIT = (int)Util.RandomDouble (SkillDictionary [skill].ToOpponentCRITMin, SkillDictionary [skill].ToOpponentCRITMin);

						if(isActionExecuted)
						{
							if(toSelfContinuousHPRegen != 0 || toSelfContinuousMPRegen != 0 || toSelfContinuousSPD != 0 || 
								toSelfContinuousPATK != 0 || toSelfContinuousMATK != 0 || toSelfContinuousPDEF != 0 ||
								toSelfContinuousMDEF != 0 || toSelfContinuousACC != 0 || toSelfContinuousEVA != 0 || toSelfContinuousCRIT != 0)
							{
								monsterSource.ApplyBuff (new BuffData (toSelfContinuousHP, toSelfContinuousHPRegen, toSelfContinuousMP,
									toSelfContinuousMPRegen, toSelfContinuousPATK, toSelfContinuousMATK, toSelfContinuousPDEF, toSelfContinuousMDEF,
									toSelfContinuousSPD, toSelfContinuousACC, toSelfContinuousEVA, toSelfContinuousCRIT, round,
									SkillDictionary[skill].SkillName, SkillDictionary[skill].Category, SkillDictionary[skill].ToOpponentStatus,
									false));
							}

							if(toOpponentContinuousHP != 0 || toSelfContinuousHPRegen != 0 || toOpponentContinuousSPD != 0 || 
								toOpponentContinuousPATK != 0 || toOpponentContinuousMATK != 0 || toOpponentContinuousPDEF != 0 ||
								toOpponentContinuousMDEF != 0 || toOpponentContinuousACC != 0 || toOpponentContinuousEVA != 0 || toOpponentContinuousCRIT != 0)
							{
								monsterTarget.ApplyBuff (new BuffData (toOpponentContinuousHP, toOpponentContinuousHPRegen, toOpponentContinuousMP,
									toOpponentContinuousMPRegen, toOpponentContinuousPATK, toOpponentContinuousMATK, toOpponentContinuousPDEF, toOpponentContinuousMDEF,
									toOpponentContinuousSPD, toOpponentContinuousACC, toOpponentContinuousEVA, toOpponentContinuousCRIT, round,
									SkillDictionary[skill].SkillName, SkillDictionary[skill].Category, SkillDictionary[skill].ToOpponentStatus,
									false));
							}
						}

						parameters += toSelfContinuousHPRegen + "~" + toSelfContinuousMPRegen+ "~" + toSelfContinuousSPD + "~" + toSelfContinuousPATK+ "~" + 
							toSelfContinuousMATK + "~" + toSelfContinuousPDEF + "~" + toSelfContinuousMDEF + "~" + 
							toSelfContinuousACC + "~" + toSelfContinuousEVA + "~" + toSelfContinuousCRIT + "~" + 
							toOpponentContinuousHP + "~" + toOpponentContinuousHPRegen + "~" + toOpponentContinuousMP + "~" +
							toOpponentContinuousMPRegen+ "~" + toOpponentContinuousSPD+ "~" + toOpponentContinuousPATK+ "~" +
							toOpponentContinuousMATK + "~" + toOpponentContinuousPDEF+ "~" + toOpponentContinuousMDEF+ "~" +
							toOpponentContinuousACC + "~" +	toOpponentContinuousEVA + "~" + toOpponentContinuousCRIT + "~";
						break;

					case 4:
						isMonsterSourceCritical = isCritical (monsterSource);
						parameters += isMonsterSourceCritical ? "1~" : "0~";

						toMonsterTargetHP = Convert.ToInt32 (Util.RandomDouble (SkillDictionary [skill].ToOpponentHPMin, SkillDictionary [skill].ToOpponentHPMax) *
							(isMonsterSourceCritical ? Util.RandomDouble (SkillDictionary [skill].ToSelfCRITMin, SkillDictionary [skill].ToSelfCRITMax) : 1));

						toMonsterTargetMP = Convert.ToInt32 (Util.RandomDouble (SkillDictionary [skill].ToOpponentMPMin, SkillDictionary [skill].ToOpponentMPMax) *
							(isMonsterSourceCritical ? Util.RandomDouble (SkillDictionary [skill].ToSelfCRITMin, SkillDictionary [skill].ToSelfCRITMax) : 1));

						if(isActionExecuted)
						{
							monsterTarget.HP += toMonsterTargetHP;
							monsterTarget.MP += toMonsterTargetMP;
						}

						parameters += toMonsterTargetHP.ToString () + "~" + toMonsterTargetMP.ToString () + "~";
						break;

					case 5:
						goto case 1;

					case 8:
						goto case 1;

					case 9:
						round = Convert.ToInt32 (SkillDictionary[skill].EffectLifetime);
						toSelfContinuousHP = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousHPMin, SkillDictionary[skill].ToSelfContinuousHPMax));
						toSelfContinuousHPRegen = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousHPRegenMin,SkillDictionary[skill].ToSelfContinuousHPRegenMax));
						toSelfContinuousMP = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousMPMin, SkillDictionary[skill].ToSelfContinuousMPMax));
						toSelfContinuousMPRegen = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousMPRegenMin, SkillDictionary[skill].ToSelfContinuousMPRegenMax));
						toSelfContinuousSPD = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousSPDMin, SkillDictionary[skill].ToSelfContinuousSPDMax));
						toSelfContinuousPATK = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousPATKMin, SkillDictionary[skill].ToSelfContinuousPATKMax));
						toSelfContinuousMATK = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousMATKMin, SkillDictionary[skill].ToSelfContinuousMATKMax));
						toSelfContinuousPDEF = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousPDEFMin, SkillDictionary[skill].ToSelfContinuousPDEFMin));
						toSelfContinuousMDEF = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousMDEFMin, SkillDictionary[skill].ToSelfContinuousMDEFMax));
						toSelfContinuousACC = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousACCMin, SkillDictionary[skill].ToSelfContinuousACCMax));
						toSelfContinuousEVA = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousEVAMin, SkillDictionary[skill].ToSelfContinuousEVAMax));
						toSelfContinuousCRIT = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToSelfContinuousCRITMin, SkillDictionary[skill].ToSelfContinuousCRITMax));

						toOpponentContinuousHP = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousHPMin, SkillDictionary[skill].ToOpponentContinuousHPMax));
						toOpponentContinuousHPRegen = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousHPRegenMin, SkillDictionary[skill].ToOpponentContinuousHPRegenMax));
						toOpponentContinuousMP = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousMPMin, SkillDictionary[skill].ToOpponentContinuousMPMax));
						toOpponentContinuousMPRegen = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousMPRegenMin, SkillDictionary[skill].ToOpponentContinuousMPRegenMax));
						toOpponentContinuousSPD = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousSPDMin, SkillDictionary[skill].ToOpponentContinuousSPDMax));
						toOpponentContinuousPATK = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousPATKMin, SkillDictionary[skill].ToOpponentContinuousPATKMax));
						toOpponentContinuousMATK = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousMATKMin, SkillDictionary[skill].ToOpponentContinuousMATKMax));
						toOpponentContinuousPDEF = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousPDEFMin, SkillDictionary[skill].ToOpponentContinuousPDEFMax));
						toOpponentContinuousMDEF = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousMDEFMin, SkillDictionary[skill].ToOpponentContinuousMDEFMax));
						toOpponentContinuousACC = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousACCMin, SkillDictionary[skill].ToOpponentContinuousACCMax));
						toOpponentContinuousEVA = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousEVAMin, SkillDictionary[skill].ToOpponentContinuousEVAMax));
						toOpponentContinuousCRIT = Convert.ToInt32 (Util.RandomDouble (SkillDictionary[skill].ToOpponentContinuousCRITMin, SkillDictionary[skill].ToOpponentContinuousCRITMax));

						if(isActionExecuted)
						{
							if(toSelfContinuousHP != 0 || toSelfContinuousHPRegen != 0 || toSelfContinuousMP != 0 || toSelfContinuousMPRegen != 0 || 
								toSelfContinuousSPD != 0 || toSelfContinuousPATK != 0 || toSelfContinuousMATK != 0 || toSelfContinuousPDEF != 0 ||
								toSelfContinuousMDEF != 0 || toSelfContinuousACC != 0 || toSelfContinuousEVA != 0 || toSelfContinuousCRIT != 0)
							{
								monsterSource.ApplyBuff (new BuffData (toSelfContinuousHP, toSelfContinuousHPRegen, toSelfContinuousMP,
									toSelfContinuousMPRegen, toSelfContinuousPATK, toSelfContinuousMATK, toSelfContinuousPDEF, toSelfContinuousMDEF,
									toSelfContinuousSPD, toSelfContinuousACC, toSelfContinuousEVA, toSelfContinuousCRIT, round,
									SkillDictionary[skill].SkillName, SkillDictionary[skill].Category, SkillDictionary[skill].ToOpponentStatus,
									true));
							}

							if(toOpponentContinuousHP != 0 || toOpponentContinuousHPRegen != 0 || toOpponentContinuousMP != 0 || toOpponentContinuousMPRegen != 0 ||
								toOpponentContinuousSPD != 0 || toOpponentContinuousPATK != 0 || toOpponentContinuousMATK != 0 || toOpponentContinuousPDEF != 0 ||
								toOpponentContinuousMDEF != 0 || toOpponentContinuousACC != 0 || toOpponentContinuousEVA != 0 || toOpponentContinuousCRIT != 0)
							{
								monsterTarget.ApplyBuff (new BuffData (toOpponentContinuousHP, toOpponentContinuousHPRegen, toOpponentContinuousMP,
									toOpponentContinuousMPRegen, toOpponentContinuousPATK, toOpponentContinuousMATK, toOpponentContinuousPDEF, toOpponentContinuousMDEF,
									toOpponentContinuousSPD, toOpponentContinuousACC, toOpponentContinuousEVA, toOpponentContinuousCRIT, round,
									SkillDictionary[skill].SkillName, SkillDictionary[skill].Category, SkillDictionary[skill].ToOpponentStatus,
									true));
							}
						}

						parameters += toSelfContinuousHP + "~" + toSelfContinuousHPRegen + "~" + toSelfContinuousMP + "~" 
							+ toSelfContinuousMPRegen+ "~" + toSelfContinuousSPD + "~" + toSelfContinuousPATK+ "~" 
							+ toSelfContinuousMATK + "~" + toSelfContinuousPDEF + "~" + toSelfContinuousMDEF + "~" 
							+ toSelfContinuousACC + "~" + toSelfContinuousEVA + "~" + toSelfContinuousCRIT + "~" 
							+ toOpponentContinuousHP + "~" + toOpponentContinuousHPRegen + "~" + toOpponentContinuousMP + "~" 
							+ toOpponentContinuousMPRegen + "~" + toOpponentContinuousSPD + "~" + toOpponentContinuousPATK+ "~" 
							+ toOpponentContinuousMATK + "~" + toOpponentContinuousPDEF + "~" + toOpponentContinuousMDEF + "~" 
							+ toOpponentContinuousACC + "~" + toOpponentContinuousEVA + "~" + toOpponentContinuousCRIT + "~";

						goto case 1;

					case 12:
						goto case 1;

					default:
						break;
					}
				}
				else if (item != -2)
				{
					parameters += monsterSource.itemDict[item].ItemCategory + "~";

					switch(monsterSource.itemDict[item].ItemCategory)
					{
					case 1:
						toMonsterSourceHP = monsterSource.itemDict [item].ToSelfHP;
						toMonsterSourceMP = monsterSource.itemDict [item].ToSelfMP;
						parameters += toMonsterSourceHP + "~" + toMonsterSourceMP + "~";

						monsterSource.HP += toMonsterSourceHP;
						monsterSource.MP += toMonsterSourceMP;
						break;

					case 2:
						round = monsterSource.itemDict [item].EffectLifetime;
						toMonsterSourcePATK = monsterSource.itemDict [item].ToSelfPATK;
						toMonsterSourceMATK = monsterSource.itemDict [item].ToSelfMATK;
						toMonsterSourcePDEF = monsterSource.itemDict [item].ToSelfPDEF;
						toMonsterSourceMDEF = monsterSource.itemDict [item].ToSelfMDEF;
						toMonsterSourceACC = monsterSource.itemDict [item].ToSelfACC;
						toMonsterSourceEVA = monsterSource.itemDict [item].ToSelfEVA;
						toMonsterSourceCRIT = monsterSource.itemDict [item].ToSelfCRIT;
						toMonsterSourceSPD = monsterSource.itemDict [item].ToSelfSPD;
						toMonsterSourceHPRegen = monsterSource.itemDict [item].ToSelfHPRegen;
						toMonsterSourceMPRegen = monsterSource.itemDict [item].ToSelfMPRegen;

						monsterSource.ApplyBuff(new BuffData (toMonsterSourceHP, toMonsterSourceHPRegen, toMonsterSourceMP,
							toMonsterSourceMPRegen, toMonsterSourcePATK, toMonsterSourceMATK, toMonsterSourcePDEF, toMonsterSourceMDEF,
							toMonsterSourceSPD, toMonsterSourceACC, toMonsterSourceEVA, toMonsterSourceCRIT, round,
							monsterSource.itemDict [item].ItemName, monsterSource.itemDict [item].ItemCategory, 0, false));

						parameters += toMonsterSourcePATK + "~" + toMonsterSourceMATK + "~" +
							toMonsterSourcePDEF + "~" +	toMonsterSourceMDEF + "~" +
							toMonsterSourceACC + "~" + toMonsterSourceEVA + "~" +
							toMonsterSourceCRIT + "~" + toMonsterSourceSPD + "~" +
							toMonsterSourceHPRegen + "~" + toMonsterSourceMPRegen + "~";
						break;

					case 9:
						toMonsterSourceHP = monsterSource.itemDict [item].ToSelfContinuousHP;
						toMonsterSourceHPRegen = monsterSource.itemDict [item].ToSelfContinuousHPRegen;
						toMonsterSourceMP = monsterSource.itemDict [item].ToSelfContinuousMP;
						toMonsterSourceMPRegen = monsterSource.itemDict [item].ToSelfContinuousMPRegen;
						toMonsterSourceSPD = monsterSource.itemDict [item].ToSelfContinuousSPD;
						toMonsterSourcePATK = monsterSource.itemDict [item].ToSelfContinuousPATK;
						toMonsterSourceMATK = monsterSource.itemDict [item].ToSelfContinuousMATK;
						toMonsterSourcePDEF = monsterSource.itemDict [item].ToSelfContinuousPDEF;
						toMonsterSourceMDEF = monsterSource.itemDict [item].ToSelfContinuousMDEF;
						toMonsterSourceACC = monsterSource.itemDict [item].ToSelfContinuousACC;
						toMonsterSourceEVA = monsterSource.itemDict [item].ToSelfContinuousEVA;
						toMonsterSourceCRIT = monsterSource.itemDict [item].ToSelfContinuousCRIT;

						monsterSource.ApplyBuff (new BuffData (toMonsterSourceHP, toMonsterSourceHPRegen, toMonsterSourceMP,
							toMonsterSourceMPRegen, toMonsterSourcePATK, toMonsterSourceMATK, toMonsterSourcePDEF, toMonsterSourceMDEF,
							toMonsterSourceSPD, toMonsterSourceACC, toMonsterSourceEVA, toMonsterSourceCRIT, round,
							monsterSource.itemDict [item].ItemName, monsterSource.itemDict [item].ItemCategory, 0, true));

						parameters += toMonsterSourceHP + "~" +	toMonsterSourceHPRegen + "~" +
							toMonsterSourceMP + "~" + toMonsterSourceMPRegen + "~" +
							toMonsterSourceSPD + "~" + toMonsterSourcePATK + "~" +
							toMonsterSourceMATK + "~" + toMonsterSourcePDEF + "~" +
							toMonsterSourceMDEF + "~" +	toMonsterSourceACC + "~" +
							toMonsterSourceEVA + "~" + toMonsterSourceCRIT+ "~";
						break;

					default:
						break;
					}
				}
			}


			return parameters;
		}

		private bool isCritical(BattleMonsterData mon)
		{
			return Util.RandomInt (0, 100) < mon.CRIT;
		}

		private bool isEvade(BattleMonsterData mon)
		{
			if(!mon.isStunned && !mon.isParalyzed)
			{
				return Util.RandomInt(0,100) < mon.EVA;
			}

			return false;
		}

		private bool isHit(BattleMonsterData mon , SkillData skill)
		{
			float treshold = (mon.ACC + Util.RandomInt (Convert.ToInt32(skill.ToSelfACCMin), Convert.ToInt32(skill.ToSelfACCMax))) / 2;
			return Util.RandomInt (0, 100) < treshold;
		}
		public string CalculateItemUse(BattleMonsterData monster)
		{
			return string.Empty;
		}

		private int CalculateDamage (
			int baseDamage, float skillMonsterAttackMultiplier, int monsterMultiplier,
			bool isCritical, float criticalMultiplier,
			int defense) {
			int damage = baseDamage * (int)Math.Round(skillMonsterAttackMultiplier * monsterMultiplier);
			return isCritical ?
				(int)Math.Round((float)damage / (float)defense * criticalMultiplier) :
				(int)Math.Round((float)damage / (float)defense);
		}
	}
}

