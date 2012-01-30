using System;
using System.Collections.Generic;
using System.Linq;
using Data.Types.Enums;

namespace Data.Types
{
	public class Effect
	{
		public EffectType Type { get; set; }
		public bool VsEpicOnly { get; set; }

		public int Min { get; set; }
		public int Max { get; set; }
        public int MinBonusVsEpic { get; set; }
        public int MaxBonusVsEpic { get; set; }

		public Classification TargetType { get; set; }
		public int EffectValue { get; set; }
		public Stat Stat { get; set; }
		
		public double ParentAbilityProcChance { get; set; }

		[NonSerialized]
		private string _parentsName;
		public string ParentsName
		{
			get { return _parentsName; }
			set { _parentsName = value; }
		}

		#region Effect Factories

		public static Effect CreateDamageEffect(int min, int max, bool vsEpic)
		{
			return new Effect
			{
				Type = EffectType.Damage,
				Min = min,
				Max = max,
				VsEpicOnly = vsEpic
			};
		}

		public static Effect CreateFlurryDamageEffect(int min, int max, int times, bool vsEpic)
		{
			return new Effect
			{
				Type = EffectType.FlurryDamage,
				Min = min,
				Max = max,
				EffectValue = times,
				VsEpicOnly = vsEpic
			};
		}

		public static Effect CreateConditionalDamageEffect(Classification target, int minPer, int maxPer)
		{
			return new Effect
			{
				Type = EffectType.ConditionalDamage,
				TargetType = target,
				Min = minPer,
				Max = maxPer,
			};
		}

		public static Effect CreateHealEffect(int min, int max, bool vsEpic)
		{
			return new Effect
			{
				Type = EffectType.Heal,
				Min = min,
				Max = max,
				VsEpicOnly = vsEpic
			};
		}

		public static Effect CreateAntiHealEffect(int min, int max, bool vsEpic)
		{
			return new Effect
			{
				Type = EffectType.AntiHeal,
				Min = min,
				Max = max,
				VsEpicOnly = vsEpic
			};
		}

		public static Effect CreateConditionalHealEffect(Classification target, int minPer, int maxPer)
		{
			return new Effect
			{
				Type = EffectType.ConditionalHeal,
				TargetType = target,
				Min = minPer,
				Max = maxPer,
			};
		}

		public static Effect CreateBoostEffect(Classification target, int amount)
		{
			return new Effect
			{
				Type = EffectType.Boost,
				TargetType = target,
				EffectValue = amount,
			};
		}

		public static Effect CreateRallyEffect(Classification target, int amount)
		{
			return new Effect
			{
				Type = EffectType.Rally,
				TargetType = target,
				EffectValue = amount,
			};
		}

		public static Effect CreateJamEffect(Classification target, int amount)
		{
			return new Effect
			{
				Type = EffectType.Jam,
				TargetType = target,
				EffectValue = amount,
			};
		}

		public static Effect CreatePreventJamEffect(int amount)
		{
			return new Effect
			{
				Type = EffectType.PreventJam,
				EffectValue = amount,
			};
		}

		public static Effect CreateControlEffect(Classification target, int amount)
		{
			return new Effect
			{
				Type = EffectType.Control,
				TargetType = target,
				EffectValue = amount,
			};
		}

		public static Effect CreateReinforceEffect(Classification target, int amount)
		{
			return new Effect
			{
				Type = EffectType.Reinforce,
				TargetType = target,
				EffectValue = amount,
			};
		}

		public static Effect CreateStatIncreaseEffect(Stat stat, int amount)
		{
			return new Effect
			{
				Type = EffectType.IncreaseStat,
				Stat = stat,
				EffectValue = amount,
			};
		}

		#endregion

		public bool IsDamageEffect()
		{
			return (Type == EffectType.Damage || Type == EffectType.FlurryDamage || Type == EffectType.ConditionalDamage);
		}

		public bool IsBoostEffect()
		{
			return (Type == EffectType.Boost || Type == EffectType.Rally);
		}

		public bool IsHealingEffect()
		{
			return (Type == EffectType.Heal || Type == EffectType.ConditionalHeal);
		}

		// boosts is populated with only effects that can boost the source abilities unit
		public CombatResult CalculateAverageDamage(Force myForce, Force enemyForce, List<Effect> boosts, bool vsEpic = true)
		{
			var result = new CombatResult();

			if (VsEpicOnly && (vsEpic == false))
			{
				return result;
			}

		    var min = Min + (vsEpic ? MinBonusVsEpic : 0);
		    var max = Max + (vsEpic ? MaxBonusVsEpic : 0);
			var avg = ((min + max)/2.0);
			if (IsDamageEffect())
			{
				if (Type == EffectType.FlurryDamage)
				{
					avg *= EffectValue;
				}
				if (Type == EffectType.ConditionalDamage && enemyForce != null)
				{
					avg *= enemyForce.AvgNumOfUnitTypeAfterReinforcements(TargetType);
				}

				result.Damage += (avg*ParentAbilityProcChance);

				// Apply Percentile Boosts
				if (boosts != null)
				{
					foreach (var boostEffect in boosts.Where(x => x.Type == EffectType.Boost))
					{
						var boostBonus = 1 + (boostEffect.ParentAbilityProcChance*(boostEffect.EffectValue*0.01));
						result.Damage *= boostBonus;
					}
				}
			}
			if (IsHealingEffect())
			{
				if (Type == EffectType.ConditionalHeal && myForce != null)
				{
					avg *= myForce.AvgNumOfUnitTypeAfterReinforcements(TargetType);
				}

				result.Healing += (avg*ParentAbilityProcChance);
			}
			if (Type == EffectType.AntiHeal)
			{
				result.AntiHeal += (avg*ParentAbilityProcChance);
			}

			return result;
		}

		public CombatResult CalculateReinforcedDamage(Guid claimerID, Force myForce, Force enemyForce, List<Effect> boosts, bool vsEpic)
		{
			var result = new CombatResult();

			foreach (var unit in myForce.ClaimReinforcements(claimerID, TargetType, EffectValue))
			{
				// Add each reinforced units total average damage potential (including reinforcements it might bring in too)
			    var baseDamage = unit.CalculateTotalDamageContribution(myForce, enemyForce, boosts, vsEpic);
                result.Add(baseDamage.AdjustToProcChance(ParentAbilityProcChance));
			}

			return result;
		}
	}
}
