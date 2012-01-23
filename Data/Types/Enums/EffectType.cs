using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Types.Enums
{
	public enum EffectType
	{
		Damage,
		FlurryDamage,
		ConditionalDamage,

		Heal,
		ConditionalHeal,

		AntiHeal,

		Jam,
		Control,
		PreventJam,
		Reinforce,

		Rally,
		Boost,
		IncreaseStat,
	}
}
