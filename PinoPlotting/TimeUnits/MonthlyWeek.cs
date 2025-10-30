using ScottPlot.TickGenerators;

namespace MyPlotting.TimeUnits
{
	internal class MonthlyWeek : ITimeUnit
	{
		public IReadOnlyList<int> Divisors => new int[] { 1, 7 };

		public TimeSpan MinSize => TimeSpan.FromDays(1);

		internal static DateTime GetIntervalStart(DateTime arg)
		{
			DateTime firstMonday = GetFirstMonday(arg.Year, arg.Month);
			return firstMonday;
		}

		public static DateTime GetFirstMonday(int year, int month)
		{
			DateTime d = new(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
			while (d.DayOfWeek != DayOfWeek.Monday) d += TimeSpan.FromDays(1);
			return d;
		}

		public string GetDateTimeFormatString()
		{
			return "dd/MM/yyyy";
		}

		public DateTime Next(DateTime dateTime, int increment = 1)
		{
			DateTime next = dateTime;
			if (increment > 0)
			{
				for (int i = 0; i < increment; i++)
				{
					next = NextDayOnMontlyWeek(next);
				}
			}
			else if (increment < 0)
			{
				for (int i = 0; i < -1 * increment; i++)
				{
					next = PreviousDayOnMonthlyWeek(next);
				}
			}
			return next;
		}



		public DateTime Snap(DateTime dateTime)
		{
			return GetFirstMonday(dateTime.Year, dateTime.Month);
		}


		private DateTime PreviousDayOnMonthlyWeek(DateTime time)
		{
			time -= TimeSpan.FromDays(1);
			if (time.DayOfWeek == DayOfWeek.Sunday && time.Day < 7)
			{
				time = time.AddMonths(-1);
				var firstMonday = GetFirstMonday(time.Year, time.Month);
				time = time.AddDays(6);
			}
			return time;
		}

		private static DateTime NextDayOnMontlyWeek(DateTime time)
		{
			//Add a Day
			time += TimeSpan.FromDays(1);
			//If it is the second Monday, add enough weeks to go to the first monday of the next month
			if (time.DayOfWeek == DayOfWeek.Monday && time.Day > 7)
			{
				//Add four weeks
				time = time.AddMonths(1);
				time = GetFirstMonday(time.Year, time.Month);
			}
			return time;
		}
	}
}