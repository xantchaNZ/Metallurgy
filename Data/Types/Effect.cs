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
	}

	/*
		Effect
			Base (Epic Only)

			* DamageEffect (Amount || Min, Max) [3, 7]
			* FlurryDamageEffect (Amount || Min, Max, Count) [2, 6, 6]
			HealEffect (Amount || Min, Max) [5, 10]
			AntiHealEffect (Amount || Min, Max) [20]
			* JamEfect (Type, Count) [Xeno, 2]
			PreventJamEffect (Count) [2]
			ControlEffect (Type, Count) [Assualt, 1]
			ReinforceEffect (Type, Count) [Infantry, 4]
			ConditionalDamageEffect(Type, Amount || Min, Max) [Armoured, 2, 4] (Jammed, Reinforced)
			ConditionalHealEffect(Type, Amount || Min, Max) [Bloodthirsty, 1, 3]
			* RallyEffect(Type, Amount) [Robotic, 1]
			* BoostEffect(Type, Amount) [Robotic, 50%]
			IncreaseStatEffect(Attack/Defense, Percentage) [Attack, 25%]
	 */
}
