using ScottPlot.TickGenerators;

namespace MyPlotting.TimeUnits
{
	internal class RegularCustom : ITimeUnit
	{
		private TimeSpan? interval;

		public RegularCustom(TimeSpan? interval)
		{
			this.interval = interval;
		}

		public IReadOnlyList<int> Divisors => throw new NotImplementedException();

		public TimeSpan MinSize => throw new NotImplementedException();

		public string GetDateTimeFormatString()
		{
			throw new NotImplementedException();
		}

		public DateTime Next(DateTime dateTime, int increment = 1)
		{
			throw new NotImplementedException();
		}

		public DateTime Snap(DateTime dateTime)
		{
			throw new NotImplementedException();
		}
	}
}