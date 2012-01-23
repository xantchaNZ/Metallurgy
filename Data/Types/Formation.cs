using System;
using System.Collections.Generic;
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

		public double CalculateAverageDamage(bool vsEpic = true)
		{
			var total = 0.0;

			foreach (var ability in Abilities)
			{
				foreach (var effect in ability.Effects)
				{
					if (effect.IsDamageEffect() == false || (effect.VsEpicOnly && (vsEpic == false)))
					{
						continue;
					}

					var avg = ((effect.Min + effect.Max) / 2.0);
					if (effect.Type == EffectType.FlurryDamage)
					{
						avg *= effect.EffectValue;
					}

					total += (avg * ability.ProcChance);
				}
			}

			return total;
		}

		public double CalculateBoostToForce(Force force, bool vsEpic = true)
		{
			var bonus = 0.0;

			foreach (var ability in Abilities)
			{
				foreach (var unit in force.GetUnits())
				{
					bonus += unit.CalculateBoostBonus(ability);
				}
			}

			return Math.Round(bonus, 2, MidpointRounding.AwayFromZero);
		}
	}
}
