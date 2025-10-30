using System.Globalization;

namespace MyPlotting
{

	public enum DateTimeLabelingStrategy
	{
		WeeklyDay,
		Montly,
		Yearly,
		FullDate,
		Hours
	}

	public static class DateTimeLabelingStrategyExtension
	{
		public static Func<DateTime, string> GetFormatFunc(this DateTimeLabelingStrategy strategy)
		{
			return strategy switch
			{
				DateTimeLabelingStrategy.WeeklyDay => DayDateLabeling,
				DateTimeLabelingStrategy.Montly => MonthDateLabeling,
				DateTimeLabelingStrategy.Yearly => dt => dt.ToString("yyyy"),
				DateTimeLabelingStrategy.FullDate => FullDateLabeling,
				DateTimeLabelingStrategy.Hours => HoursLabeling,
				_ => throw new NotImplementedException()
			};
		}

		public static string HoursLabeling(DateTime time)
		{
			return time.ToString("HH/mm/ss");
		}

		public static string DayDateLabeling(DateTime date)
		{
			return date.DayOfWeek == DayOfWeek.Monday ? date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : $"{date.DayOfWeek.ToString()[0]}";
		}
		public static string MonthDateLabeling(DateTime date)
		{
			return date.ToString("MMM/yyyy", CultureInfo.InvariantCulture);
		}
		public static string FullDateLabeling(DateTime date)
		{
			return date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
		}
	}
}
