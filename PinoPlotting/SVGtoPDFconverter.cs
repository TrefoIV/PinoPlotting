using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SkiaSharp;

namespace MyPlotting
{
	internal class SVGtoPDFconverter
	{
		static void ConvertSvgToPdf(string svgPath, string pdfPath, int renderDpi = 300)
		{
			// Load SVG
			using var svgStream = File.OpenRead(svgPath);
			var svg = new SkiaSharp.Extended.Svg.SKSvg();
			svg.Load(svgStream);

			// svg.CanvasSize gives logical size in pixels (as defined in SVG or defaults)
			var svgSize = svg.CanvasSize;
			if (svgSize.Width <= 0 || svgSize.Height <= 0)
				throw new InvalidOperationException("SVG has invalid size; ensure viewBox or width/height are present.");

			// Calculate pixel dimensions for desired DPI.
			// Many SVGs assume 96 DPI when expressed in px; scale accordingly:
			// targetPixels = svgSize * (renderDpi / 96)
			float scale = renderDpi / 96f;
			int pixelWidth = Math.Max(1, (int)Math.Ceiling(svgSize.Width * scale));
			int pixelHeight = Math.Max(1, (int)Math.Ceiling(svgSize.Height * scale));

			// Render to bitmap
			var info = new SKImageInfo(pixelWidth, pixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var bitmap = new SKBitmap(info);
			using var canvas = new SKCanvas(bitmap);
			canvas.Clear(SKColors.Transparent);

			// Apply scaling so the picture fills the target pixel dimensions
			float sx = pixelWidth / svgSize.Width;
			float sy = pixelHeight / svgSize.Height;
			canvas.Scale(sx, sy);
			if (svg.Picture != null)
				canvas.DrawPicture(svg.Picture);

			canvas.Flush();

			// Encode bitmap to PNG in memory
			using var image = SKImage.FromBitmap(bitmap);
			using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
			using var pngStream = new MemoryStream();
			encoded.SaveTo(pngStream);
			pngStream.Seek(0, SeekOrigin.Begin);

			// Create PDF and embed the PNG
			using var doc = new PdfDocument();
			var page = doc.AddPage();

			// Compute page size in PDF points (72 points = 1 inch).
			// We rendered at renderDpi; convert pixels -> points: points = pixels * 72 / renderDpi
			double pointWidth = pixelWidth * 72.0 / renderDpi;
			double pointHeight = pixelHeight * 72.0 / renderDpi;
			page.Width = PageSizeConverter.ToSize(page.Size).Width; // keep default, will override explicit below
			page.Width = new XUnit(pointWidth, XGraphicsUnit.Point);
			page.Height = new XUnit(pointHeight, XGraphicsUnit.Point);

			using var xg = XGraphics.FromPdfPage(page);
			pngStream.Seek(0, SeekOrigin.Begin);
			using var ximg = XImage.FromStream(pngStream);
			// Draw image to fill the entire page
			xg.DrawImage(ximg, 0, 0, pointWidth, pointHeight);

			// Save
			using var outStream = File.Create(pdfPath);
			doc.Save(outStream);
		}
	}
}

