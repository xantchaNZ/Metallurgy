using System.Collections.Generic;
using System.Linq;
using Data.Types.Enums;

namespace Data.Types
{
	public class Ability
	{
		public string Name { get; set; }
		public double ProcChance { get; set; }

		public List<Effect> Effects { get; set; }

		public Ability()
		{
			Effects = new List<Effect>();
		}

		public Ability(string name, double procChance)
		{
			Name = name;
			ProcChance = procChance;

			Effects = new List<Effect>();
		}

		public Ability(string name, double procChance, params Effect[] effects )
		{
			Name = name;
			ProcChance = procChance;

			Effects = new List<Effect>();
			foreach (Effect t in effects)
			{
				AddEffect(t);
			}
		}

		public override string ToString()
		{
			return string.Format("{0} ({1}%)", Name, ProcChance * 100);
		}

		public void AddEffect(Effect e)
		{
			e.ParentAbilityProcChance = this.ProcChance;
			Effects.Add(e);
		}

		public void EnsureEffectProcs()
		{
			foreach (var effect in Effects)
			{
				effect.ParentAbilityProcChance = ProcChance;
			}
		}

		public bool HasEffect(EffectType type)
		{
			return (GetEffects(type).Count > 0);
		}

		public List<Effect> GetEffects(EffectType type)
		{
			return Effects.Where(x => x.Type == type).ToList();
		}

		public CombatResult CalculateAverageDamage(Force myForce, Force enemyForce, List<Effect> boosts, bool vsEpic = true)
		{
			EnsureEffectProcs();

			var result = new CombatResult();
			foreach (var effect in Effects)
			{
				result.Add(effect.CalculateAverageDamage(myForce, enemyForce, boosts, vsEpic));
			}
			return result;
		}

		public CombatResult CalculateReinforcedDamage(Force myForce, Force enemyForce, List<Effect> boosts, bool vsEpic = true)
		{
			EnsureEffectProcs();

			var result = new CombatResult();

			if (HasEffect(EffectType.Reinforce) == false)
			{
				return result;
			}

			foreach (var effect in GetEffects(EffectType.Reinforce))
			{
				result.Add(effect.CalculateReinforcedDamage(myForce, enemyForce, boosts, vsEpic));
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
	}
}
