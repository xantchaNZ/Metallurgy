using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Types
{
	public class Reinforcement
	{
		public Unit Unit { get; private set; }
		public int NumAvailable { get; private set; }

		private Dictionary<Guid, int> claimed;

		public Reinforcement(Unit unit, int numAvailable)
		{
			Unit = unit;
			NumAvailable = numAvailable;
		}

		public void Reset()
		{
			claimed = new Dictionary<Guid, int>();
		}

		public Unit ClaimReinforcement(Guid claimerID)
		{
			if (HasUnclaimedReinforcements() == false)
			{
				return null;
			}

			if (claimed.ContainsKey(claimerID))
			{
				claimed[claimerID]++;
			}
			else
			{
				claimed.Add(claimerID, 1);
			}
			return Unit;
		}

		private int NumClaimed { get { return claimed.Values.Sum(); } }
		
		public bool HasUnclaimedReinforcements()
		{
			return (NumClaimed < NumAvailable);
		}

		public override string ToString()
		{
			return string.Format("{0} x{1} {2}", Unit.Name, NumAvailable, NumClaimed == 0 ? "" : string.Format("({0} Claimed)", NumClaimed));
		}

		public int GetCountClaimedBy(Guid claimerID)
		{
			if(claimed.ContainsKey(claimerID) == false)
			{
				return 0;
			}

			return claimed[claimerID];
		}
	}
}
