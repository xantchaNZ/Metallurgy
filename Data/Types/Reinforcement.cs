namespace Data.Types
{
	public class Reinforcement
	{
		public Unit Unit { get; private set; }
		public int NumAvailable { get; private set; }

		private int numClaimed;

		public Reinforcement(Unit unit, int numAvailable)
		{
			Unit = unit;
			NumAvailable = numAvailable;
		}

		public void Reset()
		{
			numClaimed = 0;
		}

		public Unit ClaimReinforcement()
		{
			if (HasUnclaimedReinforcements() == false)
			{
				return null;
			}

			numClaimed++;
			return Unit;
		}
		
		public bool HasUnclaimedReinforcements()
		{
			return ((NumAvailable - numClaimed) > 0);
		}

		public override string ToString()
		{
			return string.Format("{0} x{1} {2}", Unit.Name, NumAvailable, numClaimed == 0 ? "" : string.Format("({0} Claimed)", numClaimed));
		}
	}
}
