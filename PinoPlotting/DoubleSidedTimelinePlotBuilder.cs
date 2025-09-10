using MyPlotting.Extensions;
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

            foreach (var date in data.Keys) _allDates.Add(date);
            if (LogRightY && boxes.Length > 0)
            {
                double min = boxes.Min(b => b.Average);
                double max = boxes.Max(b => b.Average);
                _rightYTickGen ??= new(min, max) { LogBase = LogRightBase };
                _rightYTickGen.Min = Math.Min(min, _rightYTickGen.Min);
                _rightYTickGen.Max = Math.Max(max, _rightYTickGen.Max);
                boxes.Apply(b => { b.Average = _rightYTickGen.Log(b.Average); return b; });
            }
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

            foreach (var date in data.Keys) _allDates.Add(date);

            if (LogRightY && boxes.Length > 0)
            {
                double min = boxes.Min(b => b.Min);
                double max = boxes.Max(b => b.Max);
                _rightYTickGen ??= new(min, max) { LogBase = LogRightBase };
                _rightYTickGen.Min = Math.Min(min, _rightYTickGen.Min);
                _rightYTickGen.Max = Math.Max(max, _rightYTickGen.Max);
                boxes.Apply(box =>
                {
                    box.Average = _rightYTickGen.Log(box.Average);
                    box.Box.BoxMax = _rightYTickGen.Log(box.Box.BoxMax);
                    box.Box.BoxMin = _rightYTickGen.Log(box.Box.BoxMin);
                    if (box.Box.BoxMiddle is not null) box.Box.BoxMiddle = _rightYTickGen.Log(box.Box.BoxMiddle.Value);
                    if (box.Box.WhiskerMin is not null) box.Box.WhiskerMin = _rightYTickGen.Log(box.Box.WhiskerMin.Value);
                    if (box.Box.WhiskerMax is not null) box.Box.WhiskerMax = _rightYTickGen.Log(box.Box.WhiskerMax.Value);
                    return box;
                });
            }

            _rightTimelines.Add((p.Select((date, i) => (date.Key, boxes[i])), label, color));
        }

        protected void PlotAllRightTimelines()
        {
            Dictionary<DateTime, int> date2x = _allDates.Select((d, i) => (d, i + 1)).ToDictionary(x => x.d, x => x.Item2);
            foreach (var timeline in _rightTimelines)
            {
                var scatter = AddScatter(timeline.Item1.Select(x => (date2x[x.Item1], x.Item2)).ToArray(), timeline.Item2, timeline.Item3);
                scatter.Axes.YAxis = _plt.Axes.Right;
            }
        }

        public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
        {
            PlotAllRightTimelines();

            if (LogRightY) _plt.Axes.Right.TickGenerator = _rightYTickGen ?? new(0, 1) { LogBase = LogRightBase };
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
