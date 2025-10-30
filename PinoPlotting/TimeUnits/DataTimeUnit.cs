using MyPlotting.Extensions;
using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting.TimeUnits
{

	public enum DataTimeUnit
	{
		Second,
		Minute,
		Hour,
		Day,
		Week,
		Month,
		MonthlyWeek,
		Year,
		RegularCustom
	}
	public static class TimeUnitExtension
	{
		public static ITickGenerator GetTickGenerator(this DataTimeUnit timeUnit, DateTimeLabelingStrategy? labelingStrategy, DateTime startTime, TimeSpan? interval = null)
		{
			ITimeUnit timeUnitType = timeUnit.GetTimeUnit();

			Func<DateTime, DateTime>? intervalStartFunc = timeUnit switch
			{
				DataTimeUnit.Second => dt => dt.Floor(TimeSpan.FromSeconds(1)),
				DataTimeUnit.Minute => dt => dt.Floor(TimeSpan.FromMinutes(1)),
				DataTimeUnit.Hour => dt => dt.Floor(TimeSpan.FromHours(1)),
				DataTimeUnit.Day => dt => dt.Floor(TimeSpan.FromDays(1)),
				DataTimeUnit.Week => dt => dt.ToDay(),
				DataTimeUnit.Month => dt => new DateTime(dt.Year, dt.Month, 01),
				DataTimeUnit.MonthlyWeek => MonthlyWeek.GetIntervalStart,
				DataTimeUnit.Year => dt => new DateTime(dt.Year, 01, 01),
				DataTimeUnit.RegularCustom => throw new NotImplementedException(),
				_ => throw new ArgumentException($"Invalid time unit {timeUnit}")
			};


			var tickGen = new DateTimeWithFixedStartGenerator(startTime, timeUnitType, 1, null, 1)
			{
				LabelFormatter = labelingStrategy?.GetFormatFunc()
			};
			return tickGen;
		}

		public static ITimeUnit GetTimeUnit(this DataTimeUnit timeUnit)
		{
			return timeUnit switch
			{
				DataTimeUnit.Second => new ScottPlot.TickGenerators.TimeUnits.Second(),
				DataTimeUnit.Minute => new ScottPlot.TickGenerators.TimeUnits.Minute(),
				DataTimeUnit.Hour => new ScottPlot.TickGenerators.TimeUnits.Hour(),
				DataTimeUnit.Day => new ScottPlot.TickGenerators.TimeUnits.Day(),
				DataTimeUnit.Week => new Week(),
				DataTimeUnit.Month => new ScottPlot.TickGenerators.TimeUnits.Month(),
				DataTimeUnit.MonthlyWeek => new MonthlyWeek(),
				DataTimeUnit.Year => new ScottPlot.TickGenerators.TimeUnits.Year(),
				DataTimeUnit.RegularCustom => throw new NotImplementedException(),
				_ => throw new ArgumentException($"Invalid time unit {timeUnit}")

			};
		}

		public static int GetIdealMajorTickSpacing(this DataTimeUnit timeUnit, CoordinateRange coordinateRange)
		{
			return timeUnit switch
			{
				DataTimeUnit.Second => Math.Max(1, (int)coordinateRange.Length / 10),
				DataTimeUnit.Minute => Math.Max(1, (int)coordinateRange.Length / 10),
				DataTimeUnit.Hour => Math.Max(1, (int)coordinateRange.Length / 10),
				DataTimeUnit.Day => Math.Max(1, (int)coordinateRange.Length / 10),
				DataTimeUnit.Week => Math.Max(1, (int)coordinateRange.Length / 10),
				DataTimeUnit.MonthlyWeek => 7,
				DataTimeUnit.Month => Math.Max(1, Math.Min(6, (int)coordinateRange.Length / 3)),
				DataTimeUnit.Year => Math.Max(1, Math.Min(6, (int)coordinateRange.Length / 3)),
				DataTimeUnit.RegularCustom => throw new NotImplementedException(),
				_ => throw new ArgumentException($"Invalid time unit {timeUnit}")
			};
		}

		public static DateTimeLabelingStrategy? GetLabelingStrategy(this DataTimeUnit timeUnit)
		{
			return timeUnit switch
			{
				DataTimeUnit.Second => DateTimeLabelingStrategy.Hours,
				DataTimeUnit.Minute => DateTimeLabelingStrategy.Hours,
				DataTimeUnit.Hour => DateTimeLabelingStrategy.FullDate,
				DataTimeUnit.Day => DateTimeLabelingStrategy.FullDate,
				DataTimeUnit.Week => DateTimeLabelingStrategy.FullDate,
				DataTimeUnit.MonthlyWeek => DateTimeLabelingStrategy.WeeklyDay,
				DataTimeUnit.Month => DateTimeLabelingStrategy.Montly,
				DataTimeUnit.Year => DateTimeLabelingStrategy.Yearly,
				DataTimeUnit.RegularCustom => throw new NotImplementedException(),
				_ => throw new ArgumentException($"Invalid time unit {timeUnit}")
			};
		}
	}
}
