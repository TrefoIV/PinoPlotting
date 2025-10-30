using ScottPlot;
using SkiaSharp;

namespace MyPlotting.TickGenerators
{
	public class BrokenAxisTickGenerator : ITickGenerator
	{

		private readonly double breakStart;
		private readonly double breakEnd;
		private readonly ITickGenerator baseGenerator;

		public Tick[] Ticks { get; set; }
		public int MaxTickCount { get; set; }

		public BrokenAxisTickGenerator(double breakStart, double breakEnd, ITickGenerator baseGenerator = null)
		{
			this.breakStart = breakStart;
			this.breakEnd = breakEnd;
			this.baseGenerator = baseGenerator ?? new ScottPlot.TickGenerators.NumericAutomatic();
		}

		public void Regenerate(CoordinateRange range, Edge edge, PixelLength size, SKPaint paint, LabelStyle labelStyle)
		{
			// Generate ticks for the lower range (below break)  
			var lowerRange = new CoordinateRange(range.Min, Math.Min(range.Max, breakStart));
			var lowerTicks = new List<Tick>();

			if (lowerRange.Span > 0)
			{
				baseGenerator.Regenerate(lowerRange, edge, size, paint, labelStyle);
				lowerTicks.AddRange(baseGenerator.Ticks.Where(t => t.Position < breakStart));
			}

			// Generate ticks for the upper range (above break)  
			var upperRange = new CoordinateRange(Math.Max(range.Min, breakEnd), range.Max);
			var upperTicks = new List<Tick>();

			if (upperRange.Span > 0)
			{
				baseGenerator.Regenerate(upperRange, edge, size, paint, labelStyle);
				upperTicks.AddRange(baseGenerator.Ticks.Where(t => t.Position > breakEnd));
			}

			// Combine and sort all ticks  
			var allTicks = new List<Tick>();
			allTicks.AddRange(lowerTicks);
			allTicks.AddRange(upperTicks);

			Ticks = allTicks.OrderBy(t => t.Position).ToArray();
		}

	}
}
