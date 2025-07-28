using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
    public class DateSegmentDurationCDFPlotBuilder : CDFPLotBuilder
    {
        public DateSegmentDurationCDFPlotBuilder(bool logX = false, bool logY = false) : base(logX, logY) { }
        public void AddCDFToPlot(IEnumerable<TimeSpan> segments, string label = null, int steps = CDFUtils.DEFAULT_STEPS, Color? color = null)
        {
            AddCDFToPlot(segments.Select(x => x.TotalSeconds), label, steps, color);
        }

        public void SavePlot(FileInfo outFile, TimeSpan maxValue, string xLabel = "", string yLabel = "")
        {
            FinalizeSettings(xLabel, yLabel);
            BuildAxes(maxValue);


            _plt.Layout.Fixed(new PixelPadding(top: 10, right: 10, left: 75, bottom: 105));
            if (Constants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
                _plt.SavePng(outFile.FullName + Constants.ImageFormat, 800, 600);
            else if (Constants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
                _plt.SaveSvg(outFile.FullName + Constants.ImageFormat, 800, 600);
            else
            {
                Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
            }
        }

        public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
        {
            FinalizeSettings(xLabel, yLabel);
            BuildAxes();


            _plt.Layout.Fixed(new PixelPadding(top: 10, right: 10, left: 75, bottom: 105));
            if (Constants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
                _plt.SavePng(outFile.FullName + Constants.ImageFormat, 800, 600);
            else if (Constants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
                _plt.SaveSvg(outFile.FullName + Constants.ImageFormat, 800, 600);
            else
            {
                Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
            }
        }

        private void BuildAxes(TimeSpan? maxValue = null)
        {
            if (LogX)
            {
                _xGenerator ??= new LogTickGenerator(0, 0) { LogBase = LogBaseX, IsTimeSpan = true };
                if (maxValue.HasValue)
                {
                    _xGenerator.Max = maxValue.Value.TotalSeconds;

                }
                _xGenerator.IsTimeSpan = true;
                _plt.Axes.Bottom.TickGenerator = _xGenerator;

            }
            else
            {
                _plt.Axes.Bottom.TickGenerator = new NumericAutomatic()
                {
                    LabelFormatter = x => SpanLabeling(x)
                };
            }
            _plt.Axes.Left.TickGenerator = new NumericFixedInterval(10);
            _plt.Axes.Bottom.Label.OffsetY = 20f;
            _plt.Axes.Bottom.TickLabelStyle.FontSize = 14f;
            _plt.Axes.Left.TickLabelStyle.FontSize = 20f;
            _plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
            _plt.Axes.Bottom.TickLabelStyle.Rotation = -90;
        }

        private string SpanLabeling(double x)
        {
            TimeSpan span = TimeSpan.FromSeconds(x);
            string label = "";
            int years = 0;
            if (span.Days > 365)
            {
                years = span.Days / 365;
                span -= TimeSpan.FromDays(years * 365);
            }
            if (years > 0) label += $"{years}y";
            if (span.Days > 0) label += $"{span.Days}d";
            if (years == 0 && span.Hours > 0) label += $"{span.Hours}h";
            if (years == 0 && span.Days < 10 && span.Minutes > 0) label += $"{span.Minutes}m";
            if (years == 0 && span.Days < 10 && span.Seconds > 0) label += $"{span.Seconds}s";
            return label;
        }
    }
}
