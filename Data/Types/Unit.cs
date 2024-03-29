﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
				result.Add(ability.CalculateReinforcedDamage(this.ID, myForce, enemyForce, boosts, vsEpic));
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

		public double CalculateBoostBonus(Force myForce, Force enemyForce, Effect boostEffect, bool vsEpic = true)
		{
			// Find all effects that can boost this unit
			if (IsClassification(boostEffect.TargetType) == false)
			{
				return 0.0;
			}

			var bonus = 0.0;
			foreach (var unitAbility in Abilities)
			{
				bonus += unitAbility.CalculateBoostBonus(myForce, enemyForce, boostEffect, vsEpic);
			}

			return Math.Round(bonus, 2, MidpointRounding.AwayFromZero);
		}

		public string DamageReport(Force force, Force enemy, List<Effect> boosts, bool isEpicBoss, string prefix = "", bool hideNamePrefix = false, double reinforcementRate = 0.0)
		{
			var sb = new StringBuilder();
			if (reinforcementRate > 0.0) // TODO: Better fix for this
			{
				var contrib = CalculateTotalDamageContribution(force, enemy, boosts, isEpicBoss);
				sb.AppendFormat((hideNamePrefix ? "" : prefix) + "{0} [{1}] at {2}% = [{3}]\r\n", Name, contrib.ToSimpleString(), reinforcementRate * 100, contrib.AdjustToProcChance(reinforcementRate).ToSimpleString());
			}
			else
			{
				sb.AppendFormat((hideNamePrefix ? "" : prefix) + "{0} [{1}]\r\n", Name, CalculateTotalDamageContribution(force, enemy, boosts, isEpicBoss).ToSimpleString());
			}

			var useableBoosts = new List<Effect>();
			if (boosts != null)
			{
				useableBoosts.AddRange(boosts.Where(effect => effect.IsBoostEffect() && this.IsClassification(effect.TargetType)).ToList());
			}
			foreach (var ability in Abilities)
			{
				var dmg = ability.CalculateTotalDamageContribution(this.ID, force, enemy, useableBoosts, isEpicBoss);
				sb.AppendFormat(prefix + " * {0} [{1}]\r\n", ability.Name, dmg.ToSimpleString());
				if(dmg.DoneAnything() && useableBoosts.Count > 0)
				{
					sb.AppendFormat(prefix + "   * Base [{0}]\r\n", ability.CalculateTotalDamageContribution(this.ID, force, enemy, null, isEpicBoss).ToSimpleString());
				}
				foreach (var effect in useableBoosts)
				{
					if(IsClassification(effect.TargetType))
					{
						sb.AppendFormat(prefix + "   + {0} {1} {2}\r\n", ability.CalculateBoostBonus(force, enemy, effect, isEpicBoss), effect.ParentsName,  effect.Type == EffectType.Boost ? "Boost" : "Rally");
					}
				}
				var repeatedReinforcements = new List<Guid>();
				foreach (var reinforcement in force.GetReinforcedUnits(ID))
				{
					if (repeatedReinforcements.Contains(reinforcement.ID))
					{
						continue;
					}

					var count = force.GetReinforcedUnits(ID).Count(x => x.ID == reinforcement.ID);
					if (reinforcement.Abilities.Any(x => x.HasEffect(EffectType.Reinforce)) == false && count > 1)
					{
						repeatedReinforcements.Add(reinforcement.ID);
						sb.AppendFormat(prefix + "   * {0}x Reinforced - {1}", count, reinforcement.DamageReport(force, enemy, boosts, isEpicBoss, prefix + "\t  ", hideNamePrefix: true, reinforcementRate: 0.5)); // TODO: Better fix for this
					}
					else
					{
						sb.AppendFormat(prefix + "   * Reinforced - {0}", reinforcement.DamageReport(force, enemy, boosts, isEpicBoss, prefix + "\t  ", hideNamePrefix: true));
					}
				}
				if (ability.HasEffect(EffectType.Boost))
				{
					sb.AppendFormat(prefix + "   * Boosts units in this force for a total bonus of {0}\r\n", CalculateBoostToForce(force, enemy, isEpicBoss));
				}
				if (ability.HasEffect(EffectType.Rally))
				{
					sb.AppendFormat(prefix + "   * Rallies units in this force for a total bonus of {0}\r\n", CalculateBoostToForce(force, enemy, isEpicBoss));
				}
			}
			return sb.ToString();
		}

		/*
		Force [404.8]
		Commanders
		FM Riggs [14.8]
		 * Base [2.5]
		 * Reinforced - Photon Walker x2 @ 50% [0.5 * (12.3 + 12.3)] = [12.3]
			Photon Walker [12.3]
				* Base 9.0
				+ Omega Boost 2.7
				+ Beowulf Rally 0.3
				+ Rage Vindicator Rally 0.3
			Photon Walker [12.3]
				* Base 9.0
				+ Omega Boost 2.7
				+ Beowulf Rally 0.3
				+ Rage Vindicator Rally 0.3
		*/

	}
}
