using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Data.Types.Enums;
using System.Linq;

namespace Data.Types
{
	public class Force
	{
		private static int MAX_REINFORCEMENTS = 24;

		public string Name { get; set; }
		public bool IsEpicBoss { get; set; }

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

		public CombatResult CalculateAverageForceDamage(Force enemy)
		{
			var total = new CombatResult();
			foreach (var unit in GetUnits())
			{
				total.Add(unit.CalculateTotalDamageContribution(this, enemy, this.GetBoostAbilities(), enemy.IsEpicBoss));
			}

			return total;
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

		public List<Effect> GetBoostAbilities()
		{
			// TODO: Make this check for boosts in reinforcements?
			var boostEffects = new List<Effect>();
			foreach (var ability in Formation.Abilities)
			{
				var formationEffects = new List<Effect>();
				formationEffects.AddRange(ability.GetEffects(EffectType.Rally));
				formationEffects.AddRange(ability.GetEffects(EffectType.Boost));

				foreach (var formationEffect in formationEffects)
				{
					formationEffect.ParentsName = Formation.Name;
					boostEffects.Add(formationEffect);
				}
			}
			foreach (var unit in GetUnits())
			{
				foreach (var ability in unit.Abilities)
				{
					var unitEffects = new List<Effect>();
					unitEffects.AddRange(ability.GetEffects(EffectType.Rally));
					unitEffects.AddRange(ability.GetEffects(EffectType.Boost));

					foreach (var unitEffect in unitEffects)
					{
						unitEffect.ParentsName = unit.Name;
						boostEffects.Add(unitEffect);
					}
				}
			}

			return boostEffects;
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
					foreach (var effect in ability.Effects.Where(z => z.Type == EffectType.Reinforce && z.TargetType == classification)
						)
					{
						reinforcedUnits += effect.ParentAbilityProcChance*effect.EffectValue;
					}
				}
			}
			//TODO: First level only, does not account for units that will reinforce after being brought in

			return baseUnits + reinforcedUnits;
		}

		public List<Unit> ClaimReinforcements(Guid claimerID, Classification classification, int amount)
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
					bringingIn.Add(reinforcement.ClaimReinforcement(claimerID));
				}
			}

			return bringingIn;
		}

		public List<Unit> GetReinforcedUnits(Guid claimerID)
		{
			var units = new List<Unit>();
			foreach (var reinforcement in Reinforcements)
			{
				for (var i = 0; i < reinforcement.GetCountClaimedBy(claimerID); i++)
				{
					units.Add(reinforcement.Unit);
				}
			}

			return units;
		}

		public string ForceReport(Force enemy)
		{
			ResetCombat();
			var boosts = GetBoostAbilities();

			var commandersString = new StringBuilder();
			foreach (var unit in Commanders)
			{
				commandersString.Append(unit.DamageReport(this, enemy, boosts, enemy.IsEpicBoss, "  "));
			}

			var assaultString = new StringBuilder();
			foreach (var unit in AssaultUnits)
			{
				assaultString.Append(unit.DamageReport(this, enemy, boosts, enemy.IsEpicBoss, "  "));
			}

			var structureString = new StringBuilder();
			foreach (var unit in Structures)
			{
				structureString.Append(unit.DamageReport(this, enemy, boosts, enemy.IsEpicBoss, "  "));
			}

			var vindiFormString = new StringBuilder();
			foreach (var unit in Vindicators)
			{
				vindiFormString.Append(unit.DamageReport(this, enemy, boosts, enemy.IsEpicBoss, "  "));
			}
			vindiFormString.Append("\r\n");
			vindiFormString.Append(Formation.DamageReport(this, enemy, boosts, enemy.IsEpicBoss, "  "));

			ResetCombat();
			var total = CalculateAverageForceDamage(enemy);

			var report = string.Format(ReportTemplate, Name, Formation, commandersString, assaultString, structureString, vindiFormString, 
				total.Damage, total.Healing, total.AntiHeal, total.Damage * 5);

			return report;
		}

		private static string ReportTemplate =
@"Force Report for '{0}'

Formation: {1}

Commanders
----------
{2}
Assualt Units
-------------
{3}
Structures
----------
{4}
Vindicators / Formation
------------------------
{5}

Total:
-------------------------------------
  Average Damage: {6}
  Average Heal: {7}
  Average Heal Prevention: {8}

  Estimated Flurry: {9}
-------------------------------------
";

	}
}
