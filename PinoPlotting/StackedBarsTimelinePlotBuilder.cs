using AdvancedDataStructures.Extensions;
using MyPlotting.TickGenerators;
using ScottPlot;

namespace MyPlotting
{
	public class StackedBarsTimelinePlotBuilder<T> : TimelinePlotBuilder where T : class
	{
		protected List<(IEnumerable<(DateTime, Dictionary<T, double>)>, string, Color?)> _timelines = new();
		protected Dictionary<T, Color> _stackedObjects = new();

		public StackedBarsTimelinePlotBuilder(bool logX, bool logY) : base(logX, logY)
		{
		}

		public void AddTimeline(IEnumerable<(DateTime, Dictionary<T, double>)> timeline, string label = "", Color? color = null)
		{
			foreach (var item in timeline.SelectMany(x => x.Item2.Keys)) _stackedObjects.GetOrInsert(item, PlotUtils.GetRandomColor());
			AddTimelineDates(timeline.Select(x => x.Item1));
			_timelines.Add((timeline, label, color));
		}

		protected override void PlotAllTimelines()
		{
			if (LogY)
			{
				(double minY, double maxY) = _timelines.SelectMany(x => x.Item1.SelectMany(y => y.Item2.Values.Where(z => z != 0).OneIfEmpty(-1))).MinMax();
				if (minY <= 0 || maxY <= 0) throw new Exception("Negative values not supported for log axis");
				_yGenerator ??= new LogTickGenerator(minY, maxY) { LogBase = LogBaseY, NaturalLog = false, IsTimeSpan = false };
			}

			var date2pos = _allDates.Select((date, i) => (date, (i + 1))).ToDictionary(x => x.date, x => x.Item2);
			foreach ((var timeline, var label, var color) in _timelines)
			{
				Bar[][] allTimelineBars = new Bar[timeline.Count()][];
				foreach (var item in timeline.Select((x, i) => (x, i)))
				{
					Bar[] bars = CreateTimestampBars(item.x.Item2, date2pos[item.x.Item1]);
					allTimelineBars[item.i] = bars;

				}
				if (LogY)
				{
					foreach (var bars in allTimelineBars)
					{
						foreach (var bar in bars)
						{
							bar.ValueBase = _yGenerator.Log(bar.ValueBase);
							bar.Value = _yGenerator.Log(bar.Value);
						}
					}
				}
				var barPlotted = _plt.Add.Bars(allTimelineBars.SelectMany(x => x).ToArray());
				barPlotted.LegendText = label;
			}
		}

		private Bar[] CreateTimestampBars(Dictionary<T, double> barsData, int pos)
		{
			int bars = barsData.Count;
			Bar[] result = new Bar[bars];

			int i = 0;
			double baseValue = 0;
			foreach (var stackObject in _stackedObjects.Keys)
			{
				if (barsData.TryGetValue(stackObject, out double value))
				{
					result[i++] = new Bar()
					{
						Position = pos,
						ValueBase = baseValue,
						Value = baseValue + value,
						FillColor = _stackedObjects[stackObject]
					};
					baseValue += value;
				}
			}
			return result;
		}
	}
}
