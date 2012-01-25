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

			result.Add(this.CalculateAverageDamage(myForce, enemyForce, vsEpic));
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
	}
}
