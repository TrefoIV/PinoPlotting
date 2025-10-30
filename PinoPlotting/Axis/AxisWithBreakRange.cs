using ScottPlot;
using ScottPlot.AxisPanels;

namespace MyPlotting.Axis
{
	public class AxisWithBreakRange : YAxisBase
	{
		private double breakStart;
		private double breakEnd;
		public override Edge Edge => Edge.Left;

		public AxisWithBreakRange(double breakStart, double breakEnd)
		{
			this.breakStart = breakStart;
			this.breakEnd = breakEnd;
		}

		public new float GetPixel(double position, PixelRect dataArea)
		{
			Console.WriteLine("Computing position");
			// Compress the coordinate space by removing the break range  
			double adjustedPosition = position;
			double adjustedHeight = Height - (breakEnd - breakStart);

			if (position > breakEnd)
				adjustedPosition = position - (breakEnd - breakStart);
			else if (position > breakStart)
				adjustedPosition = breakStart; // Clamp to break start  

			double pxPerUnit = dataArea.Height / adjustedHeight;
			double unitsFromMinValue = adjustedPosition - Min;
			float pxFromEdge = (float)(unitsFromMinValue * pxPerUnit);
			return dataArea.Bottom - pxFromEdge;
		}
	}
}
