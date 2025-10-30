using AdvancedDataStructures.Extensions;
using MyPlotting.TickGenerators;
using ScottPlot;

namespace MyPlotting
{
	public class StackedBarsTimelinePlotBuilder<T> : TimelinePlotBuilder where T : notnull
	{
		public bool AddBarCountOnTop { get; set; } = false;
		protected List<(IEnumerable<(DateTime, Dictionary<T, double>)>, string)> _timelines = new();
		protected Dictionary<T, Color>? Colormap = null;

		public StackedBarsTimelinePlotBuilder(bool logX, bool logY) : base(logX, logY)
		{
		}

		public void AddTimeline(IEnumerable<(DateTime, Dictionary<T, double>)> timeline, string label = "", Dictionary<T, Color>? colormap = null)
		{
			foreach (var item in timeline.SelectMany(x => x.Item2.Keys).ToHashSet())
			{
				var col = colormap != null && colormap.TryGetValue(item, out var c) ? c : PlotUtils.GetHashColor(item);
				Colormap ??= new();
				Colormap.GetOrInsert(item, col);
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

			foreach ((var timeline, var label) in _timelines)
			{
				Bar[][] allTimelineBars = new Bar[timeline.Count()][];
				foreach (var item in timeline.Select((x, i) => (x, i)))
				{
					Bar[] bars = CreateTimestampBars(item.x.Item2, DatePositions[item.x.Item1]);
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

				if (AddBarCountOnTop)
				{
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

			PlotLegend();
		}

		private void PlotLegend()
		{
			Dictionary<Color, string> cmpLegend = GetColormapLegend();
			_plt.Legend.Alignment = Alignment.MiddleRight;
			foreach (var colorLegend in cmpLegend)
			{
				_plt.Legend.ManualItems.Add(new LegendItem()
				{
					LabelText = colorLegend.Value,
					FillColor = colorLegend.Key
				});
			}
			_plt.Legend.InterItemPadding = new PixelPadding(0.01f);
			if (cmpLegend.Count > 0 && cmpLegend.Count < 25)
				_plt.ShowLegend(Edge.Right);
			else _plt.Legend.IsVisible = false;
		}

		protected virtual Dictionary<Color, string> GetColormapLegend()
		{
			if (Colormap is null) return new();
			Dictionary<Color, string> cmap = new();
			foreach (var kv in Colormap)
			{
				string? label = cmap.GetOrInsert(kv.Value, "");
				label += kv.Key.ToString();
				cmap[kv.Value] = label;
			}
			return cmap;
		}

		private Bar[] CreateTimestampBars(Dictionary<T, double> barsData, double pos)
		{
			int bars = barsData.Count;
			Bar[] result = new Bar[bars];

			int i = 0;
			double baseValue = 0;
			foreach (T? stackObject in Colormap.Keys.OrderBy(key => key.GetHashCode()))
			{
				if (barsData.TryGetValue(stackObject, out double value))
				{
					result[i++] = new Bar()
					{
						Position = pos,
						ValueBase = baseValue,
						Value = baseValue + value,
						FillColor = Colormap[stackObject],
						LineColor = Colors.White
					};
					baseValue += value;
				}
			}
			return result;
		}
	}
}
