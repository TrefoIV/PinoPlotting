using AdvancedDataStructures.Extensions;
using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.TickGenerators;
using static MyPlotting.PlotUtils;

namespace MyPlotting
{
	public class DoubleSidedTimelinePlotBuilder : NumericalTimelinePlotBuilder
	{
		public bool LogRightY { get; set; }
		public int LogRightBase { get; set; } = 10;
		private LogTickGenerator? _rightYTickGen = null;

		private List<(IEnumerable<(DateTime, BoxWithAverage)>, string, Color?)> _rightTimelines = new();
		public Plot Plot => _plt;
		public DoubleSidedTimelinePlotBuilder(bool logX, bool logY, bool drawBoxes = true, bool logRightY = false) : base(logX, logY, drawBoxes)
		{
			LogRightY = logRightY;
		}


		public void AddRightTimeline(IDictionary<DateTime, double> data, string label = "", Color? color = null)
		{
			IOrderedEnumerable<KeyValuePair<DateTime, double>> p = data.OrderBy(x => x.Key);
			BoxWithAverage[] boxes = p.Select(x => new BoxWithAverage
			{
				Average = x.Value,
				Box = null
			}).ToArray();
			AddTimelineDates(data.Keys);
			_rightTimelines.Add((p.Select((date, i) => (date.Key, boxes[i])), label, color));
		}
		public void AddRightTimeline(IDictionary<DateTime, int> data, string label = "", Color? color = null)
		{
			AddRightTimeline(data.ToDictionary(x => x.Key, x => (double)x.Value), label, color);
		}

		public void AddRightTimeline(IDictionary<DateTime, IEnumerable<double>> data, string label = "", Color? color = null)
		{
			IOrderedEnumerable<KeyValuePair<DateTime, IEnumerable<double>>> p = data.OrderBy(x => x.Key);
			BoxWithAverage[] boxes = p.Select(x => GetPercentileBox(x.Value)).ToArray();

			AddTimelineDates(data.Keys);
			_rightTimelines.Add((p.Select((date, i) => (date.Key, boxes[i])), label, color));
		}

		protected void PlotAllRightTimelines()
		{
			if (LogRightY)
			{
				double min = _rightTimelines.SelectMany(t => t.Item1.Select(db => db.Item2.Box == null ? db.Item2.Average : db.Item2.Min)).Where(x => x > 0).OneIfEmpty(-1).Min();
				double max = _rightTimelines.SelectMany(t => t.Item1.Select(db => db.Item2.Box == null ? db.Item2.Average : db.Item2.Max)).Where(x => x > 0).OneIfEmpty(-1).Max();

				if (min != -1 && max != -1)
				{
					_rightYTickGen ??= new(min, max) { LogBase = LogRightBase, ShowZero = true };
					_rightTimelines.ForEach(t => t.Item1.ForEach(box =>
					{
						box.Item2.Average = _rightYTickGen.Log(box.Item2.Average);
						if (box.Item2.Box == null) return;
						box.Item2.Box.BoxMax = _rightYTickGen.Log(box.Item2.Box.BoxMax);
						box.Item2.Box.BoxMin = _rightYTickGen.Log(box.Item2.Box.BoxMin);
						if (box.Item2.Box.BoxMiddle != null) box.Item2.Box.BoxMiddle = _rightYTickGen.Log(box.Item2.Box.BoxMiddle.Value);
						if (box.Item2.Box.WhiskerMin != null) box.Item2.Box.WhiskerMin = _rightYTickGen.Log(box.Item2.Box.WhiskerMin.Value);
						if (box.Item2.Box.WhiskerMax != null) box.Item2.Box.WhiskerMax = _rightYTickGen.Log(box.Item2.Box.WhiskerMax.Value);
					}));
				}
			}

			Dictionary<DateTime, int> date2x = _allDates.Select((d, i) => (d, i + 1)).ToDictionary(x => x.d, x => x.Item2);
			foreach (var timeline in _rightTimelines)
			{
				AddRightScatter(timeline.Item1.Select(x => (date2x[x.Item1], x.Item2)).ToArray(), timeline.Item2, timeline.Item3);
			}
		}

		private void AddRightScatter((int, BoxWithAverage)[] timeline, string label, Color? color)
		{
			double[] yData = timeline.Select(c => c.Item2.Average).ToArray();

			double[] xs = timeline.Select(x => (double)x.Item1).ToArray(); //Enumerable.Range(1, data.Count).Select(n => (double)n).ToArray();

			var scatter = _plt.Add.Scatter(xs, yData, color: color);
			scatter.Axes.YAxis = _plt.Axes.Right;
			scatter.LegendText = label;

			//Aggiungi i segnalini di min e max per la "varianza"
			if (DrawBoxes)
				for (int i = 0; i < timeline.Length; i++)
				{
					if (timeline[i].Item2.Box is null) continue;
					timeline[i].Item2.Box.Position = xs[i];
					var b = _plt.Add.Box(timeline[i].Item2.Box);
					b.Axes.YAxis = _plt.Axes.Right;
					b.FillColor = Colors.Transparent;
					if (color is not null) b.LineColor = color.Value;
					b.LineWidth = 0.75f;
				}
		}

		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			PlotAllRightTimelines();
			if (LogRightY)
			{
				_rightYTickGen ??= new(1, 1) { LogBase = LogRightBase };
				_plt.Axes.Right.TickGenerator = _rightYTickGen;
				_plt.Axes.SetLimitsY(_rightYTickGen.ShowZero ? _rightYTickGen.Log(0) : Math.Floor(_rightYTickGen.Log(_rightYTickGen.Min)),
									Math.Ceiling(_rightYTickGen.Log(_rightYTickGen.Max)), _plt.Axes.Right);
			}
			else _plt.Axes.Right.TickGenerator = new NumericAutomatic() { LabelFormatter = NumericLabeling };

			base.SavePlot(outFile, xLabel, yLabel);
		}

		public void SavePlot(FileInfo outFile, string yRlabel, string xLabel, string yLabel)
		{
			_plt.Axes.Right.Label.Text = yRlabel;
			SavePlot(outFile, xLabel, yLabel);
		}
	}
}
