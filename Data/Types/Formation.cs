using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
				result.Add(ability.CalculateReinforcedDamage(this.ID, myForce, enemyForce, boosts, vsEpic));
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

			var boostEffects = new List<Effect>();
			foreach (var ability in Abilities)
			{
				boostEffects.AddRange(ability.GetEffects(EffectType.Rally));
				boostEffects.AddRange(ability.GetEffects(EffectType.Boost));
			}

			foreach (var effect in boostEffects)
			{
				foreach (var unit in myForce.GetUnits()) // TODO: Get this to count Reinforced units
				{
					bonus += unit.CalculateBoostBonus(myForce, enemyForce, effect, vsEpic);
				}
			}

			return Math.Round(bonus, 2, MidpointRounding.AwayFromZero);
		}

		public string DamageReport(Force force, Force enemy, List<Effect> boosts, bool isEpicBoss, string prefix = "")
		{
			var sb = new StringBuilder();
			sb.AppendFormat(prefix + "{0} [{1}]\r\n", Name, CalculateTotalDamageContribution(force, enemy, boosts, isEpicBoss).ToSimpleString());
			foreach (var ability in Abilities)
			{
				sb.AppendFormat(prefix + " * {0} [{1}]\r\n", ability.Name, ability.CalculateTotalDamageContribution(this.ID, force, enemy, null, isEpicBoss).ToSimpleString());
				foreach (var reinforcement in force.GetReinforcedUnits(ID))
				{
					sb.AppendFormat(prefix + "   * Reinforced - {0}", reinforcement.DamageReport(force, enemy, boosts, isEpicBoss, prefix + "\t  "));
				}
				if(ability.HasEffect(EffectType.Boost))
				{
					sb.AppendFormat(prefix + "   * Boosts units in this force for a total bonus of {0}\r\n", CalculateBoostToForce(force, enemy, isEpicBoss));
				}
				if(ability.HasEffect(EffectType.Rally))
				{
					sb.AppendFormat(prefix + "   * Rallies units in this force for a total bonus of {0}\r\n", CalculateBoostToForce(force, enemy, isEpicBoss));
				}
			}
			return sb.ToString();
		}
	}
}
