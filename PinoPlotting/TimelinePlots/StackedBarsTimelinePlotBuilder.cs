using AdvancedDataStructures.Extensions;
using MyPlotting.TickGenerators;
using ScottPlot;

namespace MyPlotting
{
	public class StackedBarsTimelinePlotBuilder<T> : TimelinePlotBuilder where T : notnull
	{
		protected List<(IEnumerable<(DateTime, Dictionary<T, double>)>, string)> _timelines = new();
		protected Dictionary<T, Color> _stackedObjects = new();

		public StackedBarsTimelinePlotBuilder(bool logX, bool logY) : base(logX, logY)
		{
		}

		public void AddTimeline(IEnumerable<(DateTime, Dictionary<T, double>)> timeline, string label = "", Dictionary<T, Color>? colormap = null)
		{
			foreach (var item in timeline.SelectMany(x => x.Item2.Keys).ToHashSet())
			{
				var col = colormap != null && colormap.TryGetValue(item, out var c) ? c : PlotUtils.GetHashColor(item);
				_stackedObjects.GetOrInsert(item, col);
			}
			AddTimelineDates(timeline.Select(x => x.Item1));
			_timelines.Add((timeline, label));
		}

		protected override void PlotAllTimelines()
		{
			if (LogY)
			{
				(double minY, double maxY) = _timelines.SelectMany(x => x.Item1.SelectMany(y => y.Item2.Values.Where(z => z != 0).OneIfEmpty(-1))).MinMax();
				if (minY <= 0 || maxY <= 0) throw new Exception("Negative values not supported for log axis");
				_yGenerator ??= new LogTickGenerator(minY, maxY) { LogBase = LogBaseY, NaturalLog = false, IsTimeSpan = false };
			}

			var date2pos = _allDates.Select((date, i) => (date, i + 1)).ToDictionary(x => x.date, x => x.Item2);
			foreach ((var timeline, var label) in _timelines)
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

				foreach (Bar[] barsColumn in allTimelineBars)
				{
					if (barsColumn.Length == 0) continue;
					double maxY = barsColumn[^1].Value;
					double posX = barsColumn[0].Position;
					var text = _plt.Add.Text($"{PlotUtils.NumericLabeling(barsColumn.Length)}", posX, maxY);
					text.LabelFontSize = 8;
					text.LabelFontColor = Colors.Black;
					text.Alignment = Alignment.LowerCenter;
				}
			}
		}

		private Bar[] CreateTimestampBars(Dictionary<T, double> barsData, int pos)
		{
			int bars = barsData.Count;
			Bar[] result = new Bar[bars];

			int i = 0;
			double baseValue = 0;
			foreach (T? stackObject in _stackedObjects.Keys.OrderBy(key => key.GetHashCode()))
			{
				if (barsData.TryGetValue(stackObject, out double value))
				{
					result[i++] = new Bar()
					{
						Position = pos,
						ValueBase = baseValue,
						Value = baseValue + value,
						FillColor = _stackedObjects[stackObject],
						LineColor = Colors.White
					};
					baseValue += value;
				}
			}
			return result;
		}
	}
}
