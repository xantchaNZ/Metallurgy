using System;
using System.Text;

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

		public override string ToString()
		{
			return string.Format("{0} Damage, {1} Healing, {2} Healing Prevented", Damage.ToString("N1"), Healing.ToString("N1"), AntiHeal.ToString("N2"));
		}

		public string ToSimpleString()
		{
			if (DoneAnything() == false)
			{
				return "0.0";
			}

			var sb = new StringBuilder();
			var seperator = "";
			if (Damage > 0)
			{
				sb.AppendFormat("{0}{1}", seperator, Damage.ToString("N2"));
				seperator = ", ";
			}
			if (Healing > 0)
			{
				sb.AppendFormat("{0}{1}H", seperator, Healing.ToString("N2"));
				seperator = ", ";
			}
			if (AntiHeal > 0)
			{
				sb.AppendFormat("{0}{1}AH", seperator, AntiHeal.ToString("N2"));
			}

			return sb.ToString();
		}

		public bool DoneAnything()
		{
			return (Damage > 0.01 || Healing > 0.01 || AntiHeal > 0.01);
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
