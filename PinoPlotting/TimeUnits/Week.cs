using MyPlotting.Extensions;
using ScottPlot.TickGenerators;

namespace MyPlotting.TimeUnits
{
	internal class Week : ITimeUnit
	{
		public IReadOnlyList<int> Divisors => new int[] { 1, 7 };

		public TimeSpan MinSize => TimeSpan.FromDays(7);

		public string GetDateTimeFormatString()
		{
			return "dd/MM/yyyy";
		}

		public DateTime Next(DateTime dateTime, int increment = 1)
		{
			return dateTime.AddDays(increment * 7);
		}

		public DateTime Snap(DateTime dateTime)
		{
			return dateTime.ToDay();
		}
	}
}