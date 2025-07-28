using MyPlotting.Extensions;
using MyPlotting.TickGenerators;
using ScottPlot;

namespace MyPlotting
{
    public class DoubleSidedLinePlotBuilder : LinePlotBuilder
    {
        private LogTickGenerator? _rightTickGen = null;
        public double RightYMax { get; private set; }
        public bool LogRightY { get; private set; }

        public DoubleSidedLinePlotBuilder(bool logX = false, bool logY = false, bool logRightY = false) :
            base(logX, logY)
        {
            LogRightY = logRightY;
        }

        public void AddRightSidePlot(double[] data, string label = null, Color? color = null, float size = 5f, LinePattern? linePattern = null, MarkerShape marker = MarkerShape.FilledCircle)
        {
            linePattern ??= LinePattern.Solid;

            double[] xs = data.Select((x, i) => (double)i + 1).ToArray();
            if (LogRightY && data.Length != 0)
            {
                _rightTickGen ??= new(data.Min(), data.Max()) { LogBase = LogBaseY };
                _rightTickGen.Min = Math.Min(_rightTickGen.Min, data.Min());
                _rightTickGen.Max = Math.Max(_rightTickGen.Max, data.Max());
                data.Apply(_rightTickGen.Log);
                //for (int i = 0; i < data.Length; i++)
                //{
                //	data[i] = data[i] > 0 ? Math.Log10(data[i]) : 0;
                //}
            }

            var scatter = _plt.Add.Scatter(xs, data, color: color);
            scatter.Axes.YAxis = _plt.Axes.Right;
            scatter.LegendText = label;
            scatter.LinePattern = linePattern.Value;
            scatter.MarkerStyle.Shape = marker;
            scatter.MarkerStyle.Size = size;

            var yMax = data.Max();
            var xMax = xs[^1];
            if (yMax > RightYMax) RightYMax = yMax;
            if (xMax > this.xMax) this.xMax = xMax;
        }

        public void SavePlot(FileInfo outFile, string rightYLabel, string xLabel = "", string yLabel = "")
        {
            if (!LogRightY)
            {
                _plt.Axes.Right.Max = RightYMax;
                _plt.Axes.Right.Min = 0;
                _plt.Axes.Right.IsVisible = true;
                _plt.Axes.Right.Label.Text = rightYLabel;
            }

            if (LogRightY)
            {
                _plt.Axes.Right.TickGenerator = _rightTickGen ?? new(0, 1);
                //_plt.Axes.Right.TickGenerator = new NumericAutomatic()
                //{
                //	MinorTickGenerator = new LogMinorTickGenerator(),
                //	LabelFormatter = x => $"{Math.Pow(10, x):N0}"
                //};
            }

            base.SavePlot(outFile, xLabel, yLabel);
        }
    }
}
