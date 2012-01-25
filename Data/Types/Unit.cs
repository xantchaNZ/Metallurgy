using System;
using System.Collections.Generic;
using System.Linq;
using Data.Types.Enums;

namespace Data.Types
{
	public class Unit
	{
		public Guid ID { get; set; }
		public string Name { get; set; }

		public int Attack { get; set; }
		public int Defence { get; set; }
		
		public bool IsUnique { get; set; }
		public bool IsEpicBossForceUnit { get; set; }

		public List<Classification> Classifications  { get; set; }
		public List<Ability> Abilities  { get; set; }

		public string Source { get; set; }

		public Unit()
		{
			ID = new Guid();
			Classifications = new List<Classification>();
			Abilities = new List<Ability>();
		}

		public override string ToString()
		{
			return Name;
		}

		public bool IsClassification(Classification classification)
		{
			return Classifications.Contains(classification);
		}

		public CombatResult CalculateAverageDamage(Force myForce, Force enemyForce, List<Effect> boosts, bool vsEpic = true)
		{
			var result = new CombatResult();

			var useableBoosts = new List<Effect>();
			if(boosts != null)
			{
				useableBoosts.AddRange(boosts.Where(effect => effect.IsBoostEffect() && this.IsClassification(effect.TargetType)).ToList());
			}
			foreach (var ability in Abilities)
			{
				result.Add(ability.CalculateAverageDamage(myForce, enemyForce, useableBoosts, vsEpic));
			}

			return result;
		}

		public CombatResult CalculateReinforcedDamage(Force myForce, Force enemyForce, List<Effect> boosts, bool vsEpic = true)
		{
			var result = new CombatResult();

			foreach (var ability in Abilities.Where(x => x.HasEffect(EffectType.Reinforce)))
			{
				result.Add(ability.CalculateReinforcedDamage(myForce, enemyForce, boosts, vsEpic));
			}

			return result;
		}

		public CombatResult CalculateTotalDamageContribution(Force myForce, Force enemyForce, List<Effect> boosts, bool vsEpic = true)
		{
			var result = new CombatResult();

			result.Add(this.CalculateAverageDamage(myForce, enemyForce, boosts, vsEpic));
			result.Add(this.CalculateReinforcedDamage(myForce, enemyForce, boosts, vsEpic));

			return result;
		}

		public double CalculateBoostToForce(Force myForce, Force enemyForce, bool vsEpic = true)
		{
			var bonus = 0.0;

			foreach (var ability in Abilities)
			{
				foreach (var unit in myForce.GetUnits()) // TODO: Get this to count Reinforced units
				{
					bonus += unit.CalculateBoostBonus(myForce, enemyForce, ability, vsEpic);
				}
			}

			return Math.Round(bonus, 2, MidpointRounding.AwayFromZero);
		}

		public double CalculateBoostBonus(Force myForce, Force enemyForce, Ability boostAbility, bool vsEpic = true)
		{
			// Find all effects that can boost this unit
			var effects = boostAbility.Effects.Where(effect => effect.IsBoostEffect() && IsClassification(effect.TargetType)).ToList();
			if (effects.Count == 0)
			{
				return 0.0;
			}

			var bonus = 0.0;

			foreach (var boostEffect in effects)
			{
				if (boostEffect.Type == EffectType.Boost)
				{
					var boostBonus = (boostEffect.ParentAbilityProcChance * (boostEffect.EffectValue * 0.01));
					bonus += (CalculateAverageDamage(myForce, enemyForce, null, vsEpic).Damage * boostBonus);
				}

				if (boostEffect.Type == EffectType.Rally)
				{
					foreach (var unitAbility in Abilities)
					{
						var abilBonus = boostEffect.ParentAbilityProcChance * boostEffect.EffectValue * unitAbility.ProcChance;
						var flurryEffects = unitAbility.Effects.Where(x => x.Type == EffectType.FlurryDamage).ToList();
						if (flurryEffects.Count > 0)
						{
							var baseBonus = abilBonus;
							abilBonus = 0;
							foreach (var flurryEffect in flurryEffects)
							{
								abilBonus += baseBonus * flurryEffect.EffectValue;
							}
						}
						bonus += abilBonus;
					}
				}
			}

			return Math.Round(bonus, 2, MidpointRounding.AwayFromZero);
		}


	}
}
