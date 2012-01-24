using System;
using System.Collections.Generic;
using Data.Types.Enums;
using System.Linq;

namespace Data.Types
{
	public class Force
	{
		private static int MAX_REINFORCEMENTS = 24;
	
		public string Name { get; set; }
	
		public int AttackPower { get; set; }
		public int DefencePower { get; set; }

		public Formation Formation { get; set; }

		public List<Unit> Commanders { get; set; }
		public List<Unit> AssaultUnits { get; set; }
		public List<Unit> Structures { get; set; }
		public List<Unit> Vindicators { get; set; }
		
		public List<Reinforcement> Reinforcements { get; set; }
		
		public List<Unit> Jammed { get; set; }

		public Force()
		{
			Commanders = new List<Unit>();
			AssaultUnits = new List<Unit>();
			Structures = new List<Unit>();
			Vindicators = new List<Unit>();

			Reinforcements = new List<Reinforcement>();
			Jammed = new List<Unit>();
		}

		public bool AddUnit(Unit unit)
		{
			if (Formation == null)
			{
				return false;
			}

			if (unit.IsClassification(Classification.Commander))
			{
				return AddUnit(Commanders, Formation.CommanderSlots, unit);
			}

			if (unit.IsClassification(Classification.Assault))
			{
				return AddUnit(AssaultUnits, Formation.AssaultSlots, unit);
			}

			if (unit.IsClassification(Classification.Structure))
			{
				return AddUnit(Structures, Formation.StructureSlots, unit);
			}

			if (unit.IsClassification(Classification.Vindicator))
			{
				return AddUnit(Vindicators, Formation.VindicatorSlots, unit);
			}

			return false; // Unknown
		}

		private bool AddUnit(List<Unit> unitList, int slotsAvailable, Unit unit)
		{
			if (unitList.Count >= slotsAvailable)
			{
				return false;
			}

			if (unit.IsUnique && unitList.Count(x => (x.Name == unit.Name)) > 0)
			{
				return false;
			}

			unitList.Add(unit);
			return true;
		}

		public bool AddReinforcement(Unit unit, int count)
		{
			if (Reinforcements.Count >= MAX_REINFORCEMENTS)
			{
				return false; // Reinforcements full
			}

			if (Reinforcements.Count(x => (x.Unit.Name == unit.Name)) > 0)
			{
				return false; // Already in List
			}

			if (unit.IsUnique)
			{
				count = 1;
			}

			Reinforcements.Add(new Reinforcement(unit, count));
			return true;
		}

		public void ResetCombat()
		{
			Reinforcements.ForEach(x => x.Reset());
			Jammed = new List<Unit>();
		}

		public double CalculateAverageForceDamage(bool vsEpic)
		{
			var total = 0.0;
			foreach (var unit in GetUnits())
			{
				total += unit.CalculateTotalBoostedDamage(this, vsEpic);
			}

			return Math.Round(total, 2, MidpointRounding.AwayFromZero);
		}

		public List<Unit> GetUnits()
		{
			var units = new List<Unit>();
			units.AddRange(Commanders);
			units.AddRange(AssaultUnits);
			units.AddRange(Structures);
			units.AddRange(Vindicators);
			return units;
		}

		public List<Ability> GetBoostAbilities()
		{
			var boostAbilities = new List<Ability>();
			boostAbilities.AddRange(Formation.Abilities.Where(x => x.Effects.Count(effect => effect.IsBoostEffect()) > 0));
			foreach (var unit in GetUnits())
			{
				boostAbilities.AddRange(unit.Abilities.Where(x => x.Effects.Count(effect => effect.IsBoostEffect()) > 0));
			}

			return boostAbilities;
		}

		public double AvgNumOfUnitTypeAfterReinforcements(Classification classification)
		{
			ResetCombat();

			var units = GetUnits();
			var baseUnits = units.Count(x => x.IsClassification(classification));
			var reinforcedUnits = 0.0;
			foreach (var unit in units)
			{
				foreach (var ability in unit.Abilities)
				{
					foreach (var effect in ability.Effects.Where(z => z.Type == EffectType.Reinforce && z.TargetType == classification))
					{
						reinforcedUnits += effect.ParentAbilityProcChance * effect.EffectValue;
					}
				}
			}
			//TODO: First level only, does not account for units that will reinforce after being brought in

			return baseUnits + reinforcedUnits;
		}

		public List<Unit> ClaimReinforcements(Classification classification, int amount)
		{
			var bringingIn = new List<Unit>();
			for (int i = 0; i < Reinforcements.Count && bringingIn.Count < amount; i++)
			{
				var reinforcement = Reinforcements[i];
				if (reinforcement.Unit.IsClassification(classification) == false)
				{
					continue;
				}

				while (reinforcement.HasUnclaimedReinforcements() && bringingIn.Count < amount)
				{
					bringingIn.Add(reinforcement.ClaimReinforcement());
				}
			}

			return bringingIn;
		}
	}
}
