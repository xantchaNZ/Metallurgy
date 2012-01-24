using System;
using System.Collections.Generic;
using System.Linq;
using Data.Types.Enums;

namespace Data.Types
{
	public class Formation
	{
		public Guid ID { get; set; }
		public string Name { get; set; }
		
		public int CommanderSlots { get; set; }
		public int AssaultSlots { get; set; }
		public int StructureSlots { get; set; }
		public int VindicatorSlots { get; set; }

		public List<Ability> Abilities { get; set; }

		public Formation()
		{
			ID = Guid.NewGuid();
			Abilities = new List<Ability>();
		}

		public override string ToString()
		{
			return string.Format("{0} [{1}-{2}-{3}-{4}]", Name, CommanderSlots, AssaultSlots, StructureSlots, VindicatorSlots);
		}

		/*
			Damage (Flurry, Cond)
			Heal (Cond)
			Anti-Heal
			Reinforce (Control)
			Boost (Rally)
		 */

		public CombatResult CalculateAverageDamage(Force myForce, Force enemyForce, bool vsEpic = true)
		{
			var result = new CombatResult();

			foreach (var ability in Abilities)
			{
				// Formations can never recieve Boosts
				result.Add(ability.CalculateAverageDamage(myForce, enemyForce, null, vsEpic));
			}

			return result;
		}

		public double CalculateReinforcedDamage(Force force, List<Ability> boosts, bool vsEpic = true)
		{
			var total = 0.0;

			foreach (var ability in Abilities.Where(x => x.HasEffect(EffectType.Reinforce)))
			{
				foreach (var effect in ability.GetEffects(EffectType.Reinforce))
				{
					foreach (var unit in force.ClaimReinforcements(effect.TargetType, effect.EffectValue))
					{
						total += unit.CalculateBoostedDamage(boosts, vsEpic) * ability.ProcChance;
					}
				}
			}

			return Math.Round(total, 2, MidpointRounding.AwayFromZero);
		}

		public double CalculateBoostToForce(Force force, bool vsEpic = true)
		{
			var bonus = 0.0;

			var units = force.GetUnits();
			foreach (var ability in Abilities)
			{
				foreach (var unit in units)
				{
					bonus += unit.CalculateBoostBonus(ability);
				}
			}

			return Math.Round(bonus, 2, MidpointRounding.AwayFromZero);
		}
	}
}
