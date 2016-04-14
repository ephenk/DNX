using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace DNXServer.Action
{
	public class SkillData : BaseAction
	{
		public int SkillID;
		public string SkillName;
		public string Description;
		public int Type;
		public int Category;
		public string MissMessage;
		public string ToSelfIncrease;
		public string ToOpponentReduce;
		public int ToOpponentStatus;
		public double ToSelfHPMin;
		public double ToSelfHPMax;
		public double ToSelfHPRegenMin;
		public double ToSelfHPRegenMax;
		public double ToSelfMPMin;
		public double ToSelfMPMax;
		public double ToSelfMPRegenMin;
		public double ToSelfMPRegenMax;
		public double ToSelfSPDMin;
		public double ToSelfSPDMax;
		public double ToSelfPATKMin;
		public double ToSelfPATKMax;
		public double ToSelfMATKMin;
		public double ToSelfMATKMax;
		public double ToSelfPDEFMin;
		public double ToSelfPDEFMax;
		public double ToSelfMDEFMin;
		public double ToSelfMDEFMax;
		public double ToSelfACCMin;
		public double ToSelfACCMax;
		public double ToSelfEVAMin;
		public double ToSelfEVAMax;
		public double ToSelfCRITMin;
		public double ToSelfCRITMax;
		public double ToSelfEffectSPDMin;
		public double ToSelfEffectSPDMax;
		public double ToSelfEffectPATKMin;
		public double ToSelfEffectPATKMax;
		public double ToSelfEffectMATKMin;
		public double ToSelfEffectMATKMax;
		public double ToSelfEffectPDEFMin;
		public double ToSelfEffectPDEFMax;
		public double ToSelfEffectMDEFMin;
		public double ToSelfEffectMDEFMax;
		public double ToSelfEffectACCMin;
		public double ToSelfEffectACCMax;
		public double ToSelfEffectEVAMin;
		public double ToSelfEffectEVAMax;
		public double ToSelfEffectCRITMin;
		public double ToSelfEffectCRITMax;
		public double ToOpponentHPMin;
		public double ToOpponentHPMax;
		public double ToOpponentHPRegenMin;
		public double ToOpponentHPRegenMax;
		public double ToOpponentMPMin;
		public double ToOpponentMPMax;
		public double ToOpponentMPRegenMin;
		public double ToOpponentMPRegenMax;
		public double ToOpponentSPDMin;
		public double ToOpponentSPDMax;
		public double ToOpponentPATKMin;
		public double ToOpponentPATKMax;
		public double ToOpponentMATKMin;
		public double ToOpponentMATKMax;
		public double ToOpponentPDEFMin;
		public double ToOpponentPDEFMax;
		public double ToOpponentMDEFMin;
		public double ToOpponentMDEFMax;
		public double ToOpponentACCMin;
		public double ToOpponentACCMax;
		public double ToOpponentEVAMin;
		public double ToOpponentEVAMax;
		public double ToOpponentCRITMin;
		public double ToOpponentCRITMax;
		public double ToSelfContinuousHPMin;
		public double ToSelfContinuousHPMax;
		public double ToSelfContinuousHPRegenMin;
		public double ToSelfContinuousHPRegenMax;
		public double ToSelfContinuousMPMin;
		public double ToSelfContinuousMPMax;
		public double ToSelfContinuousMPRegenMin;
		public double ToSelfContinuousMPRegenMax;
		public double ToSelfContinuousSPDMin;
		public double ToSelfContinuousSPDMax;
		public double ToSelfContinuousPATKMin;
		public double ToSelfContinuousPATKMax;
		public double ToSelfContinuousMATKMin;
		public double ToSelfContinuousMATKMax;
		public double ToSelfContinuousPDEFMin;
		public double ToSelfContinuousPDEFMax;
		public double ToSelfContinuousMDEFMin;
		public double ToSelfContinuousMDEFMax;
		public double ToSelfContinuousACCMin;
		public double ToSelfContinuousACCMax;
		public double ToSelfContinuousEVAMin;
		public double ToSelfContinuousEVAMax;
		public double ToSelfContinuousCRITMin;
		public double ToSelfContinuousCRITMax;
		public double ToOpponentContinuousHPMin;
		public double ToOpponentContinuousHPMax;
		public double ToOpponentContinuousHPRegenMin;
		public double ToOpponentContinuousHPRegenMax;
		public double ToOpponentContinuousMPMin;
		public double ToOpponentContinuousMPMax;
		public double ToOpponentContinuousMPRegenMin;
		public double ToOpponentContinuousMPRegenMax;
		public double ToOpponentContinuousSPDMin;
		public double ToOpponentContinuousSPDMax;
		public double ToOpponentContinuousPATKMin;
		public double ToOpponentContinuousPATKMax;
		public double ToOpponentContinuousMATKMin;
		public double ToOpponentContinuousMATKMax;
		public double ToOpponentContinuousPDEFMin;
		public double ToOpponentContinuousPDEFMax;
		public double ToOpponentContinuousMDEFMin;
		public double ToOpponentContinuousMDEFMax;
		public double ToOpponentContinuousACCMin;
		public double ToOpponentContinuousACCMax;
		public double ToOpponentContinuousEVAMin;
		public double ToOpponentContinuousEVAMax;
		public double ToOpponentContinuousCRITMin;
		public double ToOpponentContinuousCRITMax;
		public int EffectLifetime;
		public int SkillCooldown;
		public string AnimationName;
		public string ReactionName;
		public string SelfEffectName;
		public int SelfEffectAttachedTo;
		public int SelfEffectPlayAt;
		public string OpponentEffectName;
		public int OpponentEffectAttachedTo;

		public SkillData(int skillID)
		{
			StringDictionary skill;
			using (Command ("SELECT * FROM skills WHERE skill_id = :skill")) 
			{
				SilentAddParameter("skill", skillID);
				skill = SilentExecuteRow();
			}

			this.SkillID = skillID;
			this.SkillName = skill ["skill_name"];
			this.Description = skill ["description"];
			this.Type = Convert.ToInt32(skill["type"]);
			this.Category = Convert.ToInt32 (skill ["category"]);
			this.MissMessage = skill["miss_message"];
			this.ToSelfIncrease = skill ["to_self_increase"];
			this.ToOpponentReduce = skill ["to_opponent_reduce"];
			this.ToOpponentStatus = Convert.ToInt32(skill ["to_opponent_status"]);
			this.ToSelfHPMin = Convert.ToDouble (skill ["to_self_hp_min"]);
			this.ToSelfHPMax = Convert.ToDouble (skill ["to_self_hp_max"]);
			this.ToSelfHPRegenMin = Convert.ToDouble (skill ["to_self_hp_regen_min"]);
			this.ToSelfHPRegenMax = Convert.ToDouble (skill ["to_self_hp_regen_max"]);
			this.ToSelfMPMin = Convert.ToDouble (skill ["to_self_mp_min"]);
			this.ToSelfMPMax = Convert.ToDouble (skill ["to_self_mp_max"]);
			this.ToSelfMPRegenMin = Convert.ToDouble (skill ["to_self_mp_regen_min"]);
			this.ToSelfMPRegenMax = Convert.ToDouble (skill ["to_self_mp_regen_max"]);
			this.ToSelfSPDMin = Convert.ToDouble (skill ["to_self_spd_min"]);
			this.ToSelfSPDMax = Convert.ToDouble (skill ["to_self_spd_max"]);
			this.ToSelfPATKMin = Convert.ToDouble (skill ["to_self_patk_min"]);
			this.ToSelfPATKMax = Convert.ToDouble (skill ["to_self_patk_max"]);
			this.ToSelfMATKMin = Convert.ToDouble (skill ["to_self_matk_min"]);
			this.ToSelfMATKMax = Convert.ToDouble (skill ["to_self_matk_max"]);
			this.ToSelfPDEFMin = Convert.ToDouble (skill ["to_self_pdef_min"]);
			this.ToSelfPDEFMax = Convert.ToDouble (skill ["to_self_pdef_max"]);
			this.ToSelfMDEFMin = Convert.ToDouble (skill ["to_self_mdef_min"]);
			this.ToSelfMDEFMax = Convert.ToDouble (skill ["to_self_mdef_max"]);
			this.ToSelfACCMin = Convert.ToDouble (skill ["to_self_acc_min"]);
			this.ToSelfACCMax = Convert.ToDouble (skill ["to_self_acc_max"]);
			this.ToSelfEVAMin = Convert.ToDouble (skill ["to_self_eva_min"]);
			this.ToSelfEVAMax = Convert.ToDouble (skill ["to_self_eva_max"]);
			this.ToSelfCRITMin = Convert.ToDouble (skill ["to_self_cri_min"]);
			this.ToSelfCRITMax = Convert.ToDouble (skill ["to_self_cri_max"]);
			this.ToSelfEffectSPDMin = Convert.ToDouble (skill ["to_self_effect_spd_min"]);
			this.ToSelfEffectSPDMax = Convert.ToDouble (skill ["to_self_effect_spd_max"]);
			this.ToSelfEffectPATKMin = Convert.ToDouble (skill ["to_self_effect_patk_min"]);
			this.ToSelfEffectPATKMax = Convert.ToDouble (skill ["to_self-effect_patk_max"]);
			this.ToSelfEffectMATKMin = Convert.ToDouble (skill ["to_self_effect_matk_min"]);
			this.ToSelfEffectMATKMax = Convert.ToDouble (skill ["to_self_effect_matk_max"]);
			this.ToSelfEffectPDEFMin = Convert.ToDouble (skill ["to_self_effect_pdef_min"]);
			this.ToSelfEffectPDEFMax = Convert.ToDouble (skill ["to_self_effect_pdef_max"]);
			this.ToSelfEffectMDEFMin = Convert.ToDouble (skill ["to_self_effect_mdef_min"]);
			this.ToSelfEffectMDEFMax = Convert.ToDouble (skill["to_self_effect_mdef_max"]);
			this.ToSelfEffectACCMin = Convert.ToDouble (skill["to_self_effect_acc_min"]);
			this.ToSelfEffectACCMax = Convert.ToDouble (skill ["to_self_effect_acc_max"]);
			this.ToSelfEffectEVAMin = Convert.ToDouble (skill ["to_self_effect_eva_min"]);
			this.ToSelfEffectEVAMax = Convert.ToDouble (skill ["to_self_effect_eva_max"]);
			this.ToSelfEffectCRITMin = Convert.ToDouble (skill ["to_self_effect_cri_min"]);
			this.ToSelfEffectCRITMax = Convert.ToDouble (skill ["to_self_effect_cri_max"]);
			this.ToOpponentHPMin = Convert.ToDouble (skill ["to_opponent_hp_min"]);
			this.ToOpponentHPMax = Convert.ToDouble (skill ["to_opponent_hp_max"]);
			this.ToOpponentHPRegenMin = Convert.ToDouble (skill ["to_opponent_hp_regen_min"]);
			this.ToOpponentHPRegenMax = Convert.ToDouble (skill ["to_opponent_hp_regen_max"]);
			this.ToOpponentMPMin = Convert.ToDouble (skill ["to_opponent_mp_min"]);
			this.ToOpponentMPMax = Convert.ToDouble (skill ["to_opponent_mp_max"]);
			this.ToOpponentMPRegenMin = Convert.ToDouble (skill ["to_opponent_mp_regen_min"]);
			this.ToOpponentMPRegenMax = Convert.ToDouble (skill ["to_opponent_mp_regen_min"]);
			this.ToOpponentSPDMin = Convert.ToDouble (skill ["to_opponent_spd_min"]);
			this.ToOpponentSPDMax = Convert.ToDouble (skill ["to_opponent_spd_max"]);
			this.ToOpponentPATKMin = Convert.ToDouble (skill ["to_opponent_patk_min"]);
			this.ToOpponentPATKMax = Convert.ToDouble (skill ["to_opponent_patk_max"]);
			this.ToOpponentMATKMin = Convert.ToDouble (skill ["to_opponent_matk_min"]);
			this.ToOpponentMATKMax = Convert.ToDouble (skill ["to_opponent_matk_max"]);
			this.ToOpponentPDEFMin = Convert.ToDouble (skill ["to_opponent_pdef_min"]);
			this.ToOpponentPDEFMax = Convert.ToDouble (skill ["to_opponent_pdef_max"]);
			this.ToOpponentMDEFMin = Convert.ToDouble (skill ["to_opponent_mdef_min"]);
			this.ToOpponentMDEFMax = Convert.ToDouble (skill ["to_opponent_mdef_max"]);
			this.ToOpponentACCMin = Convert.ToDouble (skill ["to_opponent_acc_min"]);
			this.ToOpponentACCMax = Convert.ToDouble (skill ["to_opponent_acc_max"]);
			this.ToOpponentEVAMin = Convert.ToDouble (skill ["to_opponent_eva_min"]);
			this.ToOpponentEVAMax = Convert.ToDouble (skill ["to_opponent_eva_max"]);
			this.ToOpponentCRITMin = Convert.ToDouble (skill ["to_opponent_cri_min"]);
			this.ToOpponentCRITMax = Convert.ToDouble (skill ["to_opponent_cri_max"]);
			this.ToSelfContinuousHPMin = Convert.ToDouble (skill ["to_self_continuous_hp_min"]);
			this.ToSelfContinuousHPMax = Convert.ToDouble (skill ["to_self_continuous_hp_max"]);
			this.ToSelfContinuousHPRegenMin = Convert.ToDouble (skill ["to_slef_continuous_hp_regen_min"]);
			this.ToSelfContinuousHPRegenMax = Convert.ToDouble (skill ["to_self_continuous_hp_regen_max"]);
			this.ToSelfContinuousMPMin = Convert.ToDouble (skill ["to_self_continuous_mp_min"]);
			this.ToSelfContinuousMPMax = Convert.ToDouble (skill ["to_self_continuous_mp_max"]);
			this.ToSelfContinuousMPRegenMin = Convert.ToDouble (skill ["to_self_continuous_mp_regen_min"]);
			this.ToSelfContinuousMPRegenMax = Convert.ToDouble (skill ["to_self_continuous_mp_regen_max"]);
			this.ToSelfContinuousSPDMin = Convert.ToDouble (skill ["to_self_continuous_spd_min"]);
			this.ToSelfContinuousSPDMax = Convert.ToDouble (skill ["to_self_continuous_spd_max"]);
			this.ToSelfContinuousPATKMin = Convert.ToDouble (skill ["to_self_continuous_patk_min"]);
			this.ToSelfContinuousPATKMax = Convert.ToDouble (skill ["to_self_continuous_patk_max"]);
			this.ToSelfContinuousMATKMin = Convert.ToDouble (skill ["to_self_continuous_matk_min"]);
			this.ToSelfContinuousMATKMax = Convert.ToDouble (skill ["to_self_continuous_matk_max"]);
			this.ToSelfContinuousPDEFMin = Convert.ToDouble (skill ["to_self_continuous_pdef_min"]);
			this.ToSelfContinuousPDEFMax = Convert.ToDouble (skill ["to_self_continuous_pdef_max"]);
			this.ToSelfContinuousMDEFMin = Convert.ToDouble (skill ["to_self_continuous_mdef_min"]);
			this.ToSelfContinuousMDEFMax = Convert.ToDouble (skill ["to_self_continuous_mdef_max"]);
			this.ToSelfContinuousACCMin = Convert.ToDouble (skill ["to_self_continuous_acc_min"]);
			this.ToSelfContinuousACCMax = Convert.ToDouble (skill ["to_self_continuous_acc_max"]);
			this.ToSelfContinuousEVAMin = Convert.ToDouble (skill ["to_self_continuous_eva_min"]);
			this.ToSelfContinuousEVAMax = Convert.ToDouble (skill ["to_self_continuous_eva_max"]);
			this.ToSelfContinuousCRITMin = Convert.ToDouble (skill ["to_self_continuous_cri_min"]);
			this.ToSelfContinuousCRITMax = Convert.ToDouble (skill ["to_self_continuous_cri_max"]);
			this.ToOpponentContinuousHPMin = Convert.ToDouble (skill ["to_opponent_continuous_hp_min"]);
			this.ToOpponentContinuousHPMax = Convert.ToDouble (skill ["to_opponent_continuous_hp_max"]);
			this.ToOpponentContinuousHPRegenMin = Convert.ToDouble(skill["to_opponent_continuous_hp_regen_min"]);
			this.ToOpponentContinuousHPRegenMax = Convert.ToDouble (skill ["to_opponent_continuous_hp_regen_max"]);
			this.ToOpponentContinuousMPMin = Convert.ToDouble (skill ["to_opponent_continuous_mp_min"]);
			this.ToOpponentContinuousMPMax = Convert.ToDouble (skill ["to_opponent_contiuous_mp_max"]);
			this.ToOpponentContinuousMPRegenMin = Convert.ToDouble (skill ["to_opponent_continuous_mp_regen_min"]);
			this.ToOpponentContinuousMPRegenMax = Convert.ToDouble (skill ["to_opponent_continuous_mp_regen_max"]);
			this.ToOpponentContinuousSPDMin = Convert.ToDouble (skill ["to_opponent_continuous_spd_min"]);
			this.ToOpponentContinuousSPDMax = Convert.ToDouble (skill ["to_opponent_continuous_spd_max"]);
			this.ToOpponentContinuousPATKMin = Convert.ToDouble (skill ["to_opponent_continuous_patk_min"]);
			this.ToOpponentContinuousPATKMax = Convert.ToDouble (skill ["to_opponent_continuous_patk_max"]);
			this.ToOpponentContinuousMATKMin = Convert.ToDouble (skill ["to_opponent_continuous_matk_min"]);
			this.ToOpponentContinuousMATKMax = Convert.ToDouble (skill ["to_opponent_continuous_matk_max"]);
			this.ToOpponentContinuousPDEFMin = Convert.ToDouble (skill ["to_opponent_continuous_pdef_min"]);
			this.ToOpponentContinuousPDEFMax = Convert.ToDouble (skill ["to_opponent_continuous_pdef_max"]);
			this.ToOpponentContinuousMDEFMin = Convert.ToDouble (skill ["to_opponent_continuous_mdef_min"]);
			this.ToOpponentContinuousMDEFMax = Convert.ToDouble (skill ["to_opponent_continuous_mdef_max"]);
			this.ToOpponentContinuousACCMin = Convert.ToDouble (skill ["to_opponent_continuous_acc_min"]);
			this.ToOpponentContinuousACCMax = Convert.ToDouble (skill ["to_opponent_continuous_acc_max"]);
			this.ToOpponentContinuousEVAMin = Convert.ToDouble (skill ["to_opponent_contionuous_eva_min"]);
			this.ToOpponentContinuousEVAMax = Convert.ToDouble (skill ["to_opponent_continuous_eva_max"]);
			this.ToOpponentContinuousCRITMin = Convert.ToDouble (skill ["to_opponent_continuous_cri_min"]);
			this.ToOpponentContinuousCRITMax = Convert.ToDouble (skill ["to_opponent_continuous_cri_max"]);
			this.EffectLifetime = Convert.ToInt32 (skill ["effect_lifetime"]);
			this.SkillCooldown = Convert.ToInt32 (skill ["skill_cooldown"]);
			this.AnimationName = skill ["animation_name"];
			this.ReactionName = skill ["reaction_name"];
			this.SelfEffectName = skill ["self_effect_name"];
			this.SelfEffectAttachedTo = Convert.ToInt32 (skill ["self_effect_attached_to"]);
			this.SelfEffectPlayAt = Convert.ToInt32 (skill ["self_effect_play_at"]);
			this.OpponentEffectName = skill ["opponent_effect_name"];
			this.OpponentEffectAttachedTo = Convert.ToInt32 (skill ["opponent_effect_attached_to"]);
		}

//		public int GetToSelfACC()
//		{
//			return Util.RandomInt (Convert.ToInt32(ToSelfACCMin), Convert.ToInt32(ToSelfACCMax));
//		}s

//		public void GetAll(object obj)
//		{
//			foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
//			{
//				Console.WriteLine (propertyInfo.Name + ": " + propertyInfo.GetValue (propertyInfo, null));
//			}
//		}
//
//		public string DisplayObjectInfo(Object o)
//		{
//			StringBuilder sb = new StringBuilder();
//
//			// Include the type of the object
//			System.Type type = o.GetType();
//			sb.Append("Type: " + type.Name);
//
//			// Include information for each Field
//			sb.Append("\r\n\r\nFields:");
//			System.Reflection.FieldInfo[] fi = type.GetFields();
//			if (fi.Length > 0)
//			{
//				foreach (FieldInfo f in fi)
//				{
//					sb.Append("\r\n " + f.ToString() + " = " + f.GetValue(o));
//				}
//			}
//			else
//				sb.Append("\r\n None");
//
//			// Include information for each Property
//			sb.Append("\r\n\r\nProperties:");
//			System.Reflection.PropertyInfo[] pi = type.GetProperties();
//			if (pi.Length > 0)
//			{
//				foreach (PropertyInfo p in pi)
//				{
//					sb.Append("\r\n " + p.ToString() + " = " +
//						p.GetValue(o, null));
//				}
//			}
//			else
//				sb.Append("\r\n None");
//
//			return sb.ToString();
//		}
	}
}

