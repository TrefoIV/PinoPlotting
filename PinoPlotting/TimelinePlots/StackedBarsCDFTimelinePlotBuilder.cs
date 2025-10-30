using AdvancedDataStructures.Extensions;
using ScottPlot;

namespace MyPlotting
{
	public class StackedBarsCDFTimelinePlotBuilder : StackedBarsTimelinePlotBuilder<int>
	{
		public int Buckets { get; private set; }
		public bool LogCDF { get; set; } = false;
		private Dictionary<int, (double, double)> _bucketsValues = new();

		public StackedBarsCDFTimelinePlotBuilder(int buckets = 20) : base(false, false)
		{
			Buckets = Math.Max(1, buckets);
		}

		public void AddCDFTimeline(IEnumerable<(DateTime timestamp, IEnumerable<double> data)> dataTimeline, string label = "")
		{
			if (Colormap == null) BuildRainbowColormap();
			(double minReference, double maxReference) = dataTimeline.SelectMany(x => x.data).Where(x => !LogCDF || x > 0).DefaultIfEmpty(double.NaN).MinMax();
			if (LogCDF && maxReference == double.NaN) return;

			if (LogCDF)
			{
				minReference = Math.Log(minReference);
				maxReference = Math.Log(maxReference);
			}
			double bucketSize = (maxReference - minReference) / Buckets;
			for (int i = 0; i <= Buckets; i++)
			{
				_bucketsValues[i] = (bucketSize * i, bucketSize * (i + 1));
			}

			List<(DateTime timestamp, Dictionary<int, double> deltas)> timeline = dataTimeline.Select(x =>
			{
				var cdf = x.data.Any() ? GetCDF(minReference, maxReference, x) : new List<(double, double)>();
				Dictionary<int, double> deltas = cdf.Select((v, i) => (v, i)).Skip(1).Select(x => (x.i, x.v.Item2 - cdf[x.i - 1].Item2)).ToDictionary(x => x.i - 1, x => x.Item2);
				return (x.timestamp, deltas);
			}).ToList();
			base.AddTimeline(timeline, label, Colormap);
		}

		private List<(double, double)> GetCDF(double minReference, double maxReference, (DateTime timestamp, IEnumerable<double> data) x)
		{
			List<(double, double)> cdf;
			if (LogCDF)
			{
				IEnumerable<double> logData = x.data.Where(v => v > 0).Select(v => Math.Log(v));
				cdf = CDFUtils.MakeCDF(logData, range: (minReference, maxReference), steps: Buckets);
			}
			else cdf = CDFUtils.MakeCDF(x.data, range: (minReference, maxReference), steps: Buckets);
			return cdf;
		}

		protected override Dictionary<Color, string> GetColormapLegend()
		{
			if (Colormap is null) return new();
			Dictionary<Color, string> result = new();

			foreach (var kvp in Colormap.OrderByDescending(k => k.Key))
			{
				if (result.TryGetValue(kvp.Value, out var label))
				{
					label += " / ";
				}
				label ??= "";
				double min = LogCDF ? Math.Pow(10, _bucketsValues[kvp.Key].Item1) : _bucketsValues[kvp.Key].Item1;
				double max = LogCDF ? Math.Pow(10, _bucketsValues[kvp.Key].Item2) : _bucketsValues[kvp.Key].Item2;
				label += $"({PlotUtils.NumericLabeling(min)} - {PlotUtils.NumericLabeling(max)})";
				result[kvp.Value] = label;
			}
			return result;
		}

		private void BuildRainbowColormap()
		{
			Colormap = new Dictionary<int, Color>();
			int shades = 3;
			Color[] raimbow = new Color[] { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue, Colors.Violet, Colors.Indigo };
			int currentShade = 0;
			for (int i = 0; i <= Buckets; i++)
			{
				Color c = raimbow[i / shades];
				(float h, float s, float l) = c.ToHSL();
				c = Color.FromHSL(h, 0.9f - 0.3f * currentShade, l);
				Colormap[i] = c;
				currentShade = (currentShade + 1) % shades;
			}

		}

		protected override void BuilYAxis()
		{
			yLabelFormatter = PlotUtils.PercentagesFormatter;
			base.BuilYAxis();
		}

	}
}
