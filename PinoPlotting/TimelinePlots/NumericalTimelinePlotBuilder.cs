using AdvancedDataStructures.Extensions;
using ScottPlot;
using ScottPlot.Plottables;
using static MyPlotting.PlotUtils;

namespace MyPlotting
{
	public class NumericalTimelinePlotBuilder : TimelinePlotBuilder
	{

		public bool DrawBoxes { get; set; }
		private List<(IEnumerable<(DateTime, BoxWithAverage)>, string, Color?)> _timelines = new();

		public NumericalTimelinePlotBuilder(bool logX, bool logY, bool drawBoxes = true) : base(logX, logY)
		{
			DrawBoxes = drawBoxes;
		}

		public void AddTimeline(IDictionary<DateTime, int> data, string label = "", Color? color = null)
		{
			IOrderedEnumerable<KeyValuePair<DateTime, int>> p = data.OrderBy(x => x.Key);
			BoxWithAverage[] boxes = p.Select(x => new BoxWithAverage
			{
				Average = x.Value,
				Box = null
			}).ToArray();

			AddTimelineDates(data.Keys);

			_timelines.Add((p.Select((date, i) => (date.Key, boxes[i])), label, color));
		}

		public void AddTimeline(IDictionary<DateTime, IEnumerable<double>> data, string label = "", Color? color = null)
		{
			IOrderedEnumerable<KeyValuePair<DateTime, IEnumerable<double>>> p = data.OrderBy(x => x.Key);
			BoxWithAverage[] boxes = p.Select(x => GetPercentileBox(x.Value)).ToArray();

			AddTimelineDates(data.Keys);

			_timelines.Add((p.Select((date, i) => (date.Key, boxes[i])), label, color));
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

			Dictionary<DateTime, int> date2x = _allDates.Select((d, i) => (d, i + 1)).ToDictionary(x => x.d, x => x.Item2);
			foreach ((IEnumerable<(DateTime, BoxWithAverage)>, string, Color?) timeline in _timelines)
			{
				AddScatter(timeline.Item1.Select(x => (date2x[x.Item1], x.Item2)).ToArray(), timeline.Item2, timeline.Item3);
			}
		}

		protected Scatter AddScatter((int, BoxWithAverage)[] timeline, string label, Color? color)
		{
			double[] yData = timeline.Select(c => c.Item2.Average).ToArray();

			double[] xs = timeline.Select(x => (double)x.Item1).ToArray(); //Enumerable.Range(1, data.Count).Select(n => (double)n).ToArray();

			var scatter = _plt.Add.Scatter(xs, yData, color: color);
			scatter.Axes.YAxis = _plt.Axes.Left;
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
					if (color is not null) b.LineColor = color.Value;
					b.LineWidth = 0.75f;
				}
			return scatter;
		}


	}
}
