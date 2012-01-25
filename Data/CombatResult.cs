using System;

namespace Data
{
	public class CombatResult
	{
		public double Damage { get; set; }
		public double Healing { get; set; }
		public double AntiHeal { get; set; }

		public CombatResult()
		{
			Damage = 0.0;
			Healing = 0.0;
			AntiHeal = 0.0;
		}

		public void Add(CombatResult other)
		{
			Damage = Math.Round(Damage + other.Damage, 2, MidpointRounding.AwayFromZero);
			Healing = Math.Round(Healing + other.Healing, 2, MidpointRounding.AwayFromZero);
			AntiHeal = Math.Round(AntiHeal + other.AntiHeal, 2, MidpointRounding.AwayFromZero);
		}

		public CombatResult AdjustToProcChance(double procRate)
		{
			return new CombatResult
			{
				Damage = Math.Round(this.Damage *procRate, 2, MidpointRounding.AwayFromZero),
				Healing = Math.Round(this.Healing * procRate, 2, MidpointRounding.AwayFromZero),
				AntiHeal = Math.Round(this.AntiHeal * procRate, 2, MidpointRounding.AwayFromZero),
			};
		}
	}
}
