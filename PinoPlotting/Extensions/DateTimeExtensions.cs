namespace MyPlotting.Extensions
{
	public static class DateTimeExtensions
	{
		public static uint ToUnixTimeSeconds(this DateTime date)
		{
			return Convert.ToUInt32((date - DateTime.UnixEpoch).TotalSeconds);
		}

		public static DateTime ToDay(this DateTime date)
		{
			return date.Floor(TimeSpan.FromDays(1));
		}
		public static DateTime Floor(this DateTime dateTime, TimeSpan interval)
		{
			return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
		}

		public static DateTime Ceiling(this DateTime dateTime, TimeSpan interval)
		{
			var overflow = dateTime.Ticks % interval.Ticks;

			return overflow == 0 ? dateTime : dateTime.AddTicks(interval.Ticks - overflow);
		}

		public static DateTime Round(this DateTime dateTime, TimeSpan interval)
		{
			if (interval.Ticks == 0)
			{
				return dateTime;
			}

			if (dateTime.Ticks / interval.Ticks > interval.Ticks / 2)
			{
				return new DateTime(Convert.ToInt64(Math.Ceiling(Convert.ToDouble(dateTime.Ticks / interval.Ticks))) * interval.Ticks);
			}
			else
			{
				return new DateTime(Convert.ToInt64(Math.Floor(Convert.ToDouble(dateTime.Ticks / interval.Ticks))) * interval.Ticks);
			}
		}

		public static DateTime OtherIfPrevoius(this DateTime dateTime, DateTime other)
		{
			if (other < dateTime) return other;
			return dateTime;
		}
		public static DateTime OtherIfNext(this DateTime dateTime, DateTime other)
		{
			if (other > dateTime) return other;
			return dateTime;
		}

		public static DateTime Avg(this IEnumerable<DateTime> dateList)
		{
			double avg = dateList.Select(x => (long)x.ToUnixTimeSeconds()).Average();
			return UnixTimeStampToDateTime(avg);
		}

		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			return DateTime.UnixEpoch.AddSeconds(unixTimeStamp);
		}
	}
}
