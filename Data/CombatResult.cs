namespace Data
{
	public class CombatResult
	{
		public double Damage { get; set; }
		public double Healing { get; set; }
		public double AntiHeal { get; set; }

		public void Add(CombatResult other)
		{
			Damage += other.Damage;
			Healing += other.Healing;
			AntiHeal += other.AntiHeal;
		}
	}
}
