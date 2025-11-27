using MyPlotting.Extensions;
using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting.TimeUnits
{

	public enum DateTimeIntervalUnit
	{
		Second,
		Minute,
		Hour,
		Day,
		Week,
		Month,
		MonthlyWeek,
		Year,
		RegularCustom,
		None
	}
	public static class TimeUnitExtension
	{
		public static ITickGenerator GetTickGenerator(this DateTimeIntervalUnit timeUnit, DateTimeLabelingStrategy? labelingStrategy, DateTime startTime, TimeSpan? interval = null)
		{
			ITimeUnit timeUnitType = timeUnit.GetTimeUnit();

			Func<DateTime, DateTime>? intervalStartFunc = timeUnit switch
			{
				DateTimeIntervalUnit.Second => dt => dt.Floor(TimeSpan.FromSeconds(1)),
				DateTimeIntervalUnit.Minute => dt => dt.Floor(TimeSpan.FromMinutes(1)),
				DateTimeIntervalUnit.Hour => dt => dt.Floor(TimeSpan.FromHours(1)),
				DateTimeIntervalUnit.Day => dt => dt.Floor(TimeSpan.FromDays(1)),
				DateTimeIntervalUnit.Week => dt => dt.ToDay(),
				DateTimeIntervalUnit.Month => dt => new DateTime(dt.Year, dt.Month, 01),
				DateTimeIntervalUnit.MonthlyWeek => MonthlyWeek.GetIntervalStart,
				DateTimeIntervalUnit.Year => dt => new DateTime(dt.Year, 01, 01),
				DateTimeIntervalUnit.RegularCustom => throw new NotImplementedException(),
				DateTimeIntervalUnit.None => throw new NotImplementedException(),
				_ => throw new ArgumentException($"Invalid time unit {timeUnit}")
			};


			var tickGen = new DateTimeWithFixedStartGenerator(startTime, timeUnitType, 1, null, 1)
			{
				LabelFormatter = labelingStrategy?.GetFormatFunc()
			};
			return tickGen;
		}

		public static ITimeUnit GetTimeUnit(this DateTimeIntervalUnit timeUnit)
		{
			return timeUnit switch
			{
				DateTimeIntervalUnit.Second => new ScottPlot.TickGenerators.TimeUnits.Second(),
				DateTimeIntervalUnit.Minute => new ScottPlot.TickGenerators.TimeUnits.Minute(),
				DateTimeIntervalUnit.Hour => new ScottPlot.TickGenerators.TimeUnits.Hour(),
				DateTimeIntervalUnit.Day => new ScottPlot.TickGenerators.TimeUnits.Day(),
				DateTimeIntervalUnit.Week => new Week(),
				DateTimeIntervalUnit.Month => new ScottPlot.TickGenerators.TimeUnits.Month(),
				DateTimeIntervalUnit.MonthlyWeek => new MonthlyWeek(),
				DateTimeIntervalUnit.Year => new ScottPlot.TickGenerators.TimeUnits.Year(),
				DateTimeIntervalUnit.RegularCustom => throw new NotImplementedException(),
				DateTimeIntervalUnit.None => throw new NotImplementedException(),
				_ => throw new ArgumentException($"Invalid time unit {timeUnit}")

			};
		}

		public static int GetIdealMajorTickSpacing(this DateTimeIntervalUnit timeUnit, CoordinateRange coordinateRange)
		{
			return timeUnit switch
			{
				DateTimeIntervalUnit.Second => Math.Max(1, (int)coordinateRange.Length / 10),
				DateTimeIntervalUnit.Minute => Math.Max(1, (int)coordinateRange.Length / 10),
				DateTimeIntervalUnit.Hour => Math.Max(1, (int)coordinateRange.Length / 10),
				DateTimeIntervalUnit.Day => Math.Max(1, (int)coordinateRange.Length / 10),
				DateTimeIntervalUnit.Week => Math.Max(1, (int)coordinateRange.Length / 10),
				DateTimeIntervalUnit.MonthlyWeek => 7,
				DateTimeIntervalUnit.Month => Math.Max(1, Math.Min(6, (int)coordinateRange.Length / 3)),
				DateTimeIntervalUnit.Year => Math.Max(1, Math.Min(6, (int)coordinateRange.Length / 3)),
				DateTimeIntervalUnit.RegularCustom => throw new NotImplementedException(),
				DateTimeIntervalUnit.None => throw new NotImplementedException(),
				_ => throw new ArgumentException($"Invalid time unit {timeUnit}")
			};
		}

		public static DateTimeLabelingStrategy? GetLabelingStrategy(this DateTimeIntervalUnit timeUnit)
		{
			return timeUnit switch
			{
				DateTimeIntervalUnit.Second => DateTimeLabelingStrategy.Hours,
				DateTimeIntervalUnit.Minute => DateTimeLabelingStrategy.Hours,
				DateTimeIntervalUnit.Hour => DateTimeLabelingStrategy.FullDate,
				DateTimeIntervalUnit.Day => DateTimeLabelingStrategy.FullDate,
				DateTimeIntervalUnit.Week => DateTimeLabelingStrategy.FullDate,
				DateTimeIntervalUnit.MonthlyWeek => DateTimeLabelingStrategy.WeeklyDay,
				DateTimeIntervalUnit.Month => DateTimeLabelingStrategy.Montly,
				DateTimeIntervalUnit.Year => DateTimeLabelingStrategy.Yearly,
				DateTimeIntervalUnit.RegularCustom => throw new NotImplementedException(),
				DateTimeIntervalUnit.None => DateTimeLabelingStrategy.FullDate,
				_ => throw new ArgumentException($"Invalid time unit {timeUnit}")
			};
		}
	}
}
