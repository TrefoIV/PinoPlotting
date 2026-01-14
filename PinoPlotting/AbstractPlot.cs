using MyPlotting.TickGenerators;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using ScottPlot;
using SkiaSharp;

namespace MyPlotting
{
	public abstract class AbstractPlot : IDisposable
	{
		protected Plot _plt;
		protected bool _disposed;
		public bool LogX { get; protected set; } = false;
		public bool LogY { get; protected set; } = false;
		public int LogBaseX { get; set; } = 10;
		public int LogBaseY { get; set; } = 10;
		public Alignment? LegendAlignment { get; set; } = null;

		protected LogTickGenerator? _xGenerator;
		protected LogTickGenerator? _yGenerator;

		protected List<(double x, Color? c, string label)> _verticalBars = [];

		public AbstractPlot(bool logX, bool logY)
		{
			_plt = new();
			_disposed = false;
			LogX = logX;
			LogY = logY;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				//Free managed resources here. Assign large managed object references to null to make them more likely to be unreachable
			}
			_plt.Dispose();

			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public abstract void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "");

		public void AddVerticalBar(double x, Color? color = null, string label = "")
		{
			_verticalBars.Add((x, color, label));
		}

		public void SavePdf(string outputPath, int width, int height)
		{
			string svgContent = _plt.GetSvgXml(width, height);
			var svg = new Svg.Skia.SKSvg();
			svg.FromSvg(svgContent);

			// Create PDF document
			using var document = new PdfDocument();
			var page = document.AddPage();
			page.Width = svg.Picture.CullRect.Width;
			page.Height = svg.Picture.CullRect.Height;

			using var gfx = XGraphics.FromPdfPage(page);
			using var bitmap = new SKBitmap((int)page.Width, (int)page.Height);
			using var canvas = new SKCanvas(bitmap);

			// Render SVG onto the bitmap
			canvas.DrawPicture(svg.Picture);
			canvas.Flush();

			// Convert the bitmap to image for PDF
			using MemoryStream imageStream = new MemoryStream();
			using SKImage image = SKImage.FromBitmap(bitmap);
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
			data.SaveTo(imageStream);

			// Draw image into PDF
			imageStream.Position = 0;
			var pdfImage = XImage.FromStream(imageStream);
			gfx.DrawImage(pdfImage, 0, 0, page.Width, page.Height);

			// Save PDF
			document.Save(outputPath);

		}

	}
}
