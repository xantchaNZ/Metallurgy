using System.Collections.Generic;
using Data.Types.Enums;

namespace Data.Types
{
	public class Effect
	{
		public EffectType Type { get; set; }
		public bool VsEpicOnly { get; set; }

		public int Min { get; set; }
		public int Max { get; set; }

		public Classification TargetType { get; set; }
		public int EffectValue { get; set; }
		public Stat Stat { get; set; }
		
		public double ParentAbilityProcChance { get; set; }

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

		public CombatResult CalculateAverageDamage(Force myForce, Force enemyForce, List<Ability> boosts, bool vsEpic = true)
		{
			var result = new CombatResult();

			if (VsEpicOnly && (vsEpic == false))
			{
				return result;
			}

			var avg = ((Min + Max) / 2.0);
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

				result.Damage += (avg * ParentAbilityProcChance);
			}
			if (IsHealingEffect())
			{
				if (Type == EffectType.ConditionalHeal && myForce != null)
				{
					avg *= myForce.AvgNumOfUnitTypeAfterReinforcements(TargetType);
				}

				result.Healing += (avg * ParentAbilityProcChance);
			}
			if (Type == EffectType.AntiHeal)
			{
				result.AntiHeal += (avg * ParentAbilityProcChance);
			}

			return result;
		}
	}
}
