using ScottPlot.TickGenerators;

namespace MyPlotting.TickGenerators
{
	public class DateTimeWithFixedStartGenerator : DateTimeFixedInterval
	{
		public DateTime StartDate { get; private set; }

		public DateTimeWithFixedStartGenerator(DateTime startDate, ITimeUnit interval, int intervalsPerTick = 1, ITimeUnit? minorInterval = null, int minorIntervalsPerTick = 1)
			: base(interval, intervalsPerTick, minorInterval, minorIntervalsPerTick, null)
		{
			StartDate = startDate;
			GetIntervalStartFunc = GetIntervalStart;
		}

		public DateTime GetIntervalStart(DateTime _)
		{
			return StartDate;
		}
	}
}
