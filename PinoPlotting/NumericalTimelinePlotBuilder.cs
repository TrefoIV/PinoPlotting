using MyPlotting.Extensions;
using ScottPlot;
using ScottPlot.Plottables;
using static MyPlotting.PlotUtils;

namespace MyPlotting
{
	public class NumericalTimelinePlotBuilder : TimelinePlotBuilder
	{

		public bool DrawBoxes { get; set; }
		protected List<(IEnumerable<(DateTime, BoxWithAverage)>, string, Color?)> _timelines = new();

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
			if (LogY && boxes.Length > 0)
			{
				double min = boxes.Min(b => b.Average);
				double max = boxes.Max(b => b.Average);
				_logTickGen ??= new(min, max) { LogBase = LogBaseY };
				_logTickGen.Min = Math.Min(min, _logTickGen.Min);
				_logTickGen.Max = Math.Max(max, _logTickGen.Max);
				boxes.Apply(b => { b.Average = _logTickGen.Log(b.Average); return b; });
			}
			_timelines.Add((p.Select((date, i) => (date.Key, boxes[i])), label, color));
		}

		public void AddTimeline(IDictionary<DateTime, IEnumerable<double>> data, string label = "", Color? color = null)
		{
			IOrderedEnumerable<KeyValuePair<DateTime, IEnumerable<double>>> p = data.OrderBy(x => x.Key);
			BoxWithAverage[] boxes = p.Select(x => GetPercentileBox(x.Value)).ToArray();

			foreach (var date in data.Keys) _allDates.Add(date);

			if (LogY && boxes.Length > 0)
			{
				double min = boxes.Min(b => b.Min);
				double max = boxes.Max(b => b.Max);
				_logTickGen ??= new(min, max) { LogBase = LogBaseY };
				_logTickGen.Min = Math.Min(min, _logTickGen.Min);
				_logTickGen.Max = Math.Max(max, _logTickGen.Max);
				boxes.Apply(box =>
				{
					box.Average = _logTickGen.Log(box.Average);
					box.Box.BoxMax = _logTickGen.Log(box.Box.BoxMax);
					box.Box.BoxMin = _logTickGen.Log(box.Box.BoxMin);
					if (box.Box.BoxMiddle is not null) box.Box.BoxMiddle = _logTickGen.Log(box.Box.BoxMiddle.Value);
					if (box.Box.WhiskerMin is not null) box.Box.WhiskerMin = _logTickGen.Log(box.Box.WhiskerMin.Value);
					if (box.Box.WhiskerMax is not null) box.Box.WhiskerMax = _logTickGen.Log(box.Box.WhiskerMax.Value);
					return box;
				});
			}

			_timelines.Add((p.Select((date, i) => (date.Key, boxes[i])), label, color));
		}

		protected override void PlotAllTimelines()
		{
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
			scatter.LegendText = label;

			//Aggiungi i segnalini di min e max per la "varianza"
			if (DrawBoxes)
				for (int i = 0; i < timeline.Length; i++)
				{
					if (timeline[i].Item2.Box is null) continue;
					timeline[i].Item2.Box.Position = xs[i];
					var b = _plt.Add.Box(timeline[i].Item2.Box);
					b.FillColor = Colors.Transparent;
					if (color is not null) b.LineColor = color.Value;
					b.LineWidth = 0.75f;
				}
			return scatter;
		}





	}
}
