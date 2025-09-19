using ScottPlot;

namespace MyPlotting
{
	public class StackedBarsCDFTimelinePlotBuilder : StackedBarsTimelinePlotBuilder<int>
	{
		public int Buckets { get; private set; }
		public Dictionary<int, Color>? Colormap { get; private set; } = null;
		public float HUE { get; set; } = Colors.LightBlue.ToHSL().h;
		public bool LogCDF { get; set; } = false;

		public StackedBarsCDFTimelinePlotBuilder(int buckets = 20) : base(false, false)
		{
			Buckets = Math.Max(1, buckets);
		}

		public void AddCDFTimeline(IEnumerable<(DateTime timestamp, IEnumerable<double> data)> dataTimeline, double maxReference, string label = "")
		{
			if (Colormap == null) BuildRainbowColormap();
			List<(DateTime timestamp, Dictionary<int, double> deltas)> timeline = dataTimeline.Select(x =>
			{
				var cdf = x.data.Any() ? GetCDF(maxReference, x) : new List<(double, double)>();
				var deltas = cdf.Select((v, i) => (v, i)).Skip(1).Select(x => (x.i, x.v.Item2 - cdf[x.i - 1].Item2)).ToDictionary(x => x.i, x => x.Item2);
				return (x.timestamp, deltas);
			}).ToList();
			AddTimeline(timeline, label, Colormap);
		}

		private List<(double, double)> GetCDF(double maxReference, (DateTime timestamp, IEnumerable<double> data) x)
		{
			List<(double, double)> cdf;
			if (LogCDF)
			{
				IEnumerable<double> logData = x.data.Where(v => v > 0).Select(v => Math.Log(v));
				cdf = CDFUtils.MakeCDF(logData, range: (0, Math.Log(maxReference)), steps: Buckets);
			}
			else cdf = CDFUtils.MakeCDF(x.data, range: (0, maxReference), steps: Buckets);
			return cdf;
		}

		private void BuildColormap()
		{
			Console.WriteLine($"Building colormap with {HUE} hue");
			Colormap = new Dictionary<int, Color>(Buckets + 1);
			float step = 0.8f / Buckets;
			float lightness = 0.1f;
			for (int i = 0; i <= Buckets; i++, lightness += step)
				Colormap[i] = Colors.LightBlue.WithLightness(0.9f - lightness);
		}

		private void BuildRainbowColormap()
		{
			Colormap = new Dictionary<int, Color>(Buckets + 1);
			int shades = 3;
			Color[] raimbow = new Color[] { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue, Colors.Indigo, Colors.Violet };
			int currentShade = 0;
			for (int i = 0; i < 21; i++)
			{
				Color c = raimbow[i / shades];
				(float h, float s, float l) = c.ToHSL();
				c = Color.FromHSL(h, 0.3f + 0.3f * currentShade, l);
				Colormap[i] = c;
				currentShade = (currentShade + 1) % shades;
			}

		}
	}
}
