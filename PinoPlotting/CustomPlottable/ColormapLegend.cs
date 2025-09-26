using ScottPlot;

namespace MyPlotting.CustomPlottable
{
	public class ColormapLegend : IHasColorAxis
	{
		public IColormap Colormap { get; set; }
		public ScottPlot.Range ManualRange { get; set; }

		public ColormapLegend(IColormap colormap, ScottPlot.Range range)
		{
			Colormap = colormap;
			ManualRange = range;
		}

		public ScottPlot.Range GetRange()
		{
			return ManualRange;
		}
	}
}
