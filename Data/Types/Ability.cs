using System.Collections.Generic;

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

		public void AddEffect(Effect e)
		{
			e.ParentAbilityProcChance = this.ProcChance;
			Effects.Add(e);
		}

		public override string ToString()
		{
			return string.Format("{0} ({1}%)", Name, ProcChance * 100);
		}
	}
}
