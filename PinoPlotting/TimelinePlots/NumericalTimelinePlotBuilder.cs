using AdvancedDataStructures.Extensions;
using MyPlotting.TimeUnits;
using ScottPlot;
using ScottPlot.Plottables;
using static MyPlotting.PlotUtils;

namespace MyPlotting
{
	public class NumericalTimelinePlotBuilder : TimelinePlotBuilder
	{

		public bool DrawBoxes { get; set; }
		public bool UseSignal { get; set; }
		protected List<(IEnumerable<(DateTime, BoxWithAverage)>, string, Color)> _timelines = new();
		private IPalette palette = new ScottPlot.Palettes.Category10();

		public NumericalTimelinePlotBuilder(bool logX, bool logY, bool drawBoxes = true) : base(logX, logY)
		{
			DrawBoxes = drawBoxes;
		}

		public void AddTimeline(IDictionary<DateTime, int> data, string label = "", Color? color = null)
		{
			AddTimeline(data.ToDictionary(x => x.Key, x => (double)x.Value), label, color);
		}

		public void AddTimeline(IDictionary<DateTime, double> data, string label = "", Color? color = null)
		{
			color ??= palette.GetColor(_timelines.Count);
			IOrderedEnumerable<KeyValuePair<DateTime, double>> p = data.OrderBy(x => x.Key);
			BoxWithAverage[] boxes = p.Select(x => new BoxWithAverage
			{
				Average = x.Value,
				Box = null
			}).ToArray();

			AddTimelineDates(data.Keys);

			_timelines.Add((p.Select((date, i) => (date.Key, boxes[i])), label, color.Value));
		}

		public void AddTimeline(IDictionary<DateTime, IEnumerable<double>> data, string label = "", Color? color = null)
		{
			color ??= _plt.Add.GetNextColor();
			IOrderedEnumerable<KeyValuePair<DateTime, IEnumerable<double>>> p = data.OrderBy(x => x.Key);
			BoxWithAverage[] boxes = p.Select(x => GetPercentileBox(x.Value)).ToArray();

			AddTimelineDates(data.Keys);

			_timelines.Add((p.Select((date, i) => (date.Key, boxes[i])), label, color.Value));
		}

		protected override void PlotAllTimelines()
		{

			if (LogY)
			{
				double min = _timelines.SelectMany(t => t.Item1.Select(db => db.Item2.Box == null ? db.Item2.Average : db.Item2.Min)).Where(x => x > 0).OneIfEmpty(-1).Min();
				double max = _timelines.SelectMany(t => t.Item1.Select(db => db.Item2.Box == null ? db.Item2.Average : db.Item2.Max)).Where(x => x > 0).OneIfEmpty(-1).Max();

				if (min != -1 && max != -1)
				{
					_yGenerator ??= new(min, max) { LogBase = LogBaseY, ShowZero = true };
					_yGenerator.Min = Math.Min(min, _yGenerator.Min);
					_yGenerator.Max = Math.Max(max, _yGenerator.Max);
					_timelines.ForEach(t => t.Item1.ForEach(box =>
					{
						box.Item2.Average = _yGenerator.Log(box.Item2.Average);
						if (box.Item2.Box == null) return;
						box.Item2.Box.BoxMax = _yGenerator.Log(box.Item2.Box.BoxMax);
						box.Item2.Box.BoxMin = _yGenerator.Log(box.Item2.Box.BoxMin);
						if (box.Item2.Box.BoxMiddle != null) box.Item2.Box.BoxMiddle = _yGenerator.Log(box.Item2.Box.BoxMiddle.Value);
						if (box.Item2.Box.WhiskerMin != null) box.Item2.Box.WhiskerMin = _yGenerator.Log(box.Item2.Box.WhiskerMin.Value);
						if (box.Item2.Box.WhiskerMax != null) box.Item2.Box.WhiskerMax = _yGenerator.Log(box.Item2.Box.WhiskerMax.Value);
					}));
				}
			}

			foreach ((IEnumerable<(DateTime, BoxWithAverage)>, string, Color) timeline in _timelines)
			{
				if (UseSignal)
				{
					AddSignal(timeline);

				}
				else
					AddScatter(timeline.Item1.Select(x => (DatePositions[x.Item1], x.Item2)).ToArray(), timeline.Item2, timeline.Item3);
			}
		}

		private void AddSignal((IEnumerable<(DateTime, BoxWithAverage)>, string, Color) timeline)
		{
			DateTime startTime = timeline.Item1.Select(x => x.Item1).Order().First();

			double[] yValues = Enumerable.Range(0, DatePositions.Count).Select(_ => double.NaN).ToArray();
			foreach (var value in timeline.Item1)
			{
				yValues[DatePositions[value.Item1] - 1] = value.Item2.Average;
			}
			double period = GetPeriod(timeline.Item1);
			var signal = _plt.Add.Signal(yValues, period: period);
			signal.Data.XOffset = startTime.ToOADate();
			signal.LineColor = timeline.Item3;
			signal.LegendText = timeline.Item2;
		}

		private double GetPeriod(IEnumerable<(DateTime, BoxWithAverage)> timeline)
		{
			switch (TimeUnit)
			{
				case DateTimeIntervalUnit.Second:
					return 1.0 / (24 * 60 * 60);
				case DateTimeIntervalUnit.Minute:
					return 1.0 / (24 * 60);
				case DateTimeIntervalUnit.Hour:
					return 1.0 / (24);
				case DateTimeIntervalUnit.Day:
					return 1.0;
				case DateTimeIntervalUnit.Week:
					return 7.0;
				case DateTimeIntervalUnit.Month:
					throw new InvalidOperationException("Cannot use a monthly period with signal timelines");
				case DateTimeIntervalUnit.Year:
					throw new InvalidOperationException("Cannot use a yearly period with signal timelines");
				case DateTimeIntervalUnit.RegularCustom:
					{
						TimeSpan span = timeline.Skip(1).First().Item1 - timeline.First().Item1;
						return span.TotalSeconds * (1.0 / (24 * 60 * 60));
					}
				default:
					throw new NotImplementedException($"Value {TimeUnit} not valid");

			}
		}

		protected Scatter AddScatter((int, BoxWithAverage)[] timeline, string label, Color color)
		{
			double[] yData = timeline.Select(c => c.Item2.Average).ToArray();

			double[] xs = timeline.Select(x => (double)x.Item1).ToArray(); //Enumerable.Range(1, data.Count).Select(n => (double)n).ToArray();

			var scatter = _plt.Add.Scatter(xs, yData, color: color);
			scatter.Axes.YAxis = _plt.Axes.Left;
			scatter.LineWidth *= 1.5f;
			scatter.MarkerSize *= 0.5f;
			scatter.LegendText = label;

			//Aggiungi i segnalini di min e max per la "varianza"
			if (DrawBoxes)
				for (int i = 0; i < timeline.Length; i++)
				{
					if (timeline[i].Item2.Box is null) continue;
					timeline[i].Item2.Box.Position = xs[i];
					var b = _plt.Add.Box(timeline[i].Item2.Box);
					b.Axes.YAxis = _plt.Axes.Left;
					b.FillColor = Colors.Transparent;
					b.LineColor = color;
					b.LineWidth = 0.75f;
				}
			return scatter;
		}

		protected override int BuildXaxis()
		{
			var n = base.BuildXaxis();
			_plt.Axes.SetLimitsX(0, DatePositions.Count + 1);
			return n;
		}
	}
}
