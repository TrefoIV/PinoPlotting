using ScottPlot;
using ScottPlot.Plottables;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;

namespace MyPlotting.CustomPlottable
{
	public class TextRectangle : Rectangle
	{
		public ScottPlot.Plottables.Text Text { get; set; }

		public TextRectangle(double left, double right, double bottom, double top, string text)
			: base()
		{
			X1 = left;
			X2 = right;
			Y1 = bottom;
			Y2 = top;
			Text = new ScottPlot.Plottables.Text()
			{
				LabelText = text,
				LabelBackgroundColor = Colors.Transparent,
				LabelBorderColor = Colors.Transparent,
				LabelFontColor = Colors.White,
				Location = new((X1 + X2) / 2, (Y1 + Y2) / 2),
				Alignment = Alignment.MiddleCenter
			};
		}

		public override void Render(RenderPack rp)
		{
			// Render rectangle first  
			base.Render(rp);

			// Get rectangle pixel dimensions  
			var rectPixels = Axes.GetPixelRect(CoordinateRect);

			// Calculate and set optimal font size  
			float optimalSize = CalculateOptimalFontSize(rectPixels, rp.Paint);
			Text.LabelFontSize = optimalSize;

			// Render text  
			Text.Render(rp);
		}

		private float CalculateOptimalFontSize(PixelRect rectPixels, SKPaint paint)
		{
			if (rectPixels.Width <= 0 || rectPixels.Height <= 0)
				return 1f;
			string text = Text.LabelText;
			// Define target area with some padding (80% of rectangle size)  
			float targetWidth = rectPixels.Width * 0.8f;
			float targetHeight = rectPixels.Height * 0.8f;

			// Binary search for optimal font size  
			float minSize = 1f;
			float maxSize = Math.Min(targetWidth, targetHeight); // Start with reasonable upper bound  
			float optimalSize = minSize;

			// First, find a reasonable upper bound by doubling until text is too large  
			MeasuredText measured;
			do
			{
				Text.LabelFontSize = maxSize;
				measured = Text.LabelStyle.Measure(text, paint);
				maxSize *= 2;
			} while (measured.Size.Width < targetWidth && measured.Size.Height < targetHeight);

			// Binary search between minSize and maxSize  
			while (maxSize - minSize > 0.5f)
			{
				float testSize = (minSize + maxSize) / 2f;
				Text.LabelFontSize = testSize;

				measured = Text.LabelStyle.Measure(text, paint);

				if (measured.Size.Width <= targetWidth && measured.Size.Height <= targetHeight)
				{
					optimalSize = testSize;
					minSize = testSize;
				}
				else
				{
					maxSize = testSize;
				}
			}

			return Math.Max(optimalSize, 1f); // Ensure minimum readable size  
		}
	}
}
