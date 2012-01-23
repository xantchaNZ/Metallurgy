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

		public double CalculateAverageDamage(bool vsEpic = true)
		{
			var total = 0.0;

			foreach (var ability in Abilities)
			{
				foreach (var effect in ability.Effects.Where(x => x.IsDamageEffect()))
				{
					if ((effect.VsEpicOnly && (vsEpic == false)))
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

		public double CalculateReinforcedDamage(List<Reinforcement> reinforcements, bool vsEpic = true)
		{
			var total = 0.0;

			foreach (var ability in Abilities)
			{
				foreach (var effect in ability.Effects.Where(x => x.Type == EffectType.Reinforce))
				{
					var bringingIn = new List<Unit>();
					for (int i = 0; i < reinforcements.Count && bringingIn.Count < effect.EffectValue; i++)
					{
						var reinforcement = reinforcements[i];
						if (reinforcement.Unit.IsClassification(effect.TargetType) == false)
						{
							continue;
						}

						while (reinforcement.HasUnclaimedReinforcements() && bringingIn.Count < effect.EffectValue)
						{
							bringingIn.Add(reinforcement.ClaimReinforcement());
						}
					}


					foreach (var unit in bringingIn)
					{
						total += unit.CalculateAverageDamage(vsEpic) * ability.ProcChance;
					}
				}
			}

			return Math.Round(total, 2, MidpointRounding.AwayFromZero);
		}

		public double CalculateBoostedReinforcedDamage(List<Reinforcement> reinforcements, List<Ability> boostAbilities, bool vsEpic = true)
		{
			var total = 0.0;

			foreach (var ability in Abilities)
			{
				foreach (var effect in ability.Effects.Where(x => x.Type == EffectType.Reinforce))
				{
					var bringingIn = new List<Unit>();
					for (int i = 0; i < reinforcements.Count && bringingIn.Count < effect.EffectValue; i++)
					{
						var reinforcement = reinforcements[i];
						if (reinforcement.Unit.IsClassification(effect.TargetType) == false)
						{
							continue;
						}

						while (reinforcement.HasUnclaimedReinforcements() && bringingIn.Count < effect.EffectValue)
						{
							bringingIn.Add(reinforcement.ClaimReinforcement());
						}
					}


					foreach (var unit in bringingIn)
					{
						total += unit.CalculateBoostedDamage(boostAbilities, vsEpic) * ability.ProcChance;
					}
				}
			}

			return Math.Round(total, 2, MidpointRounding.AwayFromZero);
		}

		public double CalculateBoostBonus(Ability boostAbility, bool vsEpic = true)
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
					bonus += (CalculateAverageDamage(vsEpic) * boostBonus);
				}

				if (boostEffect.Type == EffectType.Rally)
				{
					foreach (var unitAbility in Abilities)
					{
						var abilBonus = boostEffect.ParentAbilityProcChance * boostEffect.EffectValue * unitAbility.ProcChance;
						var flurryEffects = unitAbility.Effects.Where(x => x.Type == EffectType.FlurryDamage).ToList();
						if(flurryEffects.Count > 0)
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

		public double CalculateBoostedDamage(List<Ability> boostAbilities, bool vsEpic = true)
		{
			// Find all Boost effects & apply them multiplicatively
			// Then find and apply all rally effects

			var damage = CalculateAverageDamage(vsEpic);
			var boostEffects = new List<Effect>();

			foreach (var ability in boostAbilities)
			{
				boostEffects.AddRange(ability.Effects.Where(effect => effect.IsBoostEffect() && IsClassification(effect.TargetType)).ToList());
			}

			foreach (var boostEffect in boostEffects.Where(x => x.Type == EffectType.Boost))
			{
				var boostBonus = 1 + (boostEffect.ParentAbilityProcChance * (boostEffect.EffectValue * 0.01));
				damage *= boostBonus;
			}

			foreach (var boostEffect in boostEffects.Where(x => x.Type == EffectType.Rally))
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
					damage += abilBonus;
				}
			}

			return Math.Round(damage, 2, MidpointRounding.AwayFromZero);
		}

		public double CalculateTotalBoostedDamage(Force force, bool vsEpic = true)
		{
			var boostAbilities = force.GetBoostAbilities();

			var baseValue = CalculateBoostedDamage(boostAbilities, vsEpic);
			var reinforcementsValue = CalculateBoostedReinforcedDamage(force.Reinforcements, boostAbilities, vsEpic);

			return Math.Round(baseValue + reinforcementsValue, 2, MidpointRounding.AwayFromZero);
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
