using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
    /// <summary>
    /// This class builds a graph of clusters of DatedSegments. Each cluster can be composed by SubClusters.
    /// Each data entry is a double array: the first dimension indicates the number of subcluster for each cluster; the second dimension is the number of segments for each
    /// sub-cluster of the cluster.
    /// The number of data entries added is the number of clusters.
    /// 
    /// Each cluster is assigned to a y-coordinate range, that is as large as the number of SubClusters.
    /// Each subcluster is separated by the next one by 0.1 on the y.
    /// At the end of all subclusters for a given cluster, the y-coordinate is incremented by 1 to start the new cluster. Here is graphic example with 3 cluster:
    /// one composed by 1 subcluster, one by 2 and one by 3:
    /// 
    /// --------  ----- --------
    /// --------  ----- --------
    /// -------- ------ ---------
    /// 
    /// ----      --------------
    /// 
    /// --- ------ ------  -----
    /// --- ------ -----   ------
    /// </summary>
    public class DatedSegmentsTimeLinePlotBuilder : IDisposable
    {

        private Plot _plt;
        private bool _disposed;

        private List<(DatedSegment[][] groups, string yTick, string label)> _data;
        private List<DatedSegment> _verticalBars;


        public DatedSegmentsTimeLinePlotBuilder()
        {
            _plt = new Plot();
            _verticalBars = new();
            _data = new();
        }

        public void AddSegmentsGroup(string yTickLabel, DatedSegment[][] groups, string groupLabel = "")
        {
            _data.Add((groups, yTickLabel, groupLabel));
        }

        public void AddVerticalBar(DatedSegment segment)
        {
            _verticalBars.Add(segment);
        }

        public void SavePlot(FileInfo outFile, string labelX = "", string labelY = "")
        {
            double startY = 0, endY;
            double[] yTicks = new double[_data.Count];
            string[] yLabels = new string[_data.Count];
            List<LegendItem> legendItems = new List<LegendItem>();

            foreach ((var (groups, yTickLabel, groupLabel), int index) in _data.Select((x, i) => (x, i)))
            {
                Color color = Color.RandomHue();
                startY += 1;
                endY = startY;

                foreach (DatedSegment[] seq in groups)
                {
                    foreach (DatedSegment freq in seq)
                    {
                        if (freq.Duration < TimeSpan.Zero)
                        {
                            throw new Exception("Negative duration??");
                        }
                        var line = _plt.Add.Line(freq.Start.ToOADate(), endY, freq.End.ToOADate(), endY);
                        line.Color = color;
                    }
                    endY += 0.1;
                }
                double yTick = (startY + endY) / 2;
                yTicks[index] = yTick;
                yLabels[index] = yTickLabel;

                legendItems.Add(new LegendItem()
                {
                    LineColor = color,
                    MarkerStyle = new MarkerStyle(MarkerShape.None, 0),
                    LineStyle = new LineStyle() { Color = color },
                    LabelText = groupLabel
                });
            }

            foreach (DatedSegment vertical in _verticalBars)
            {
                _plt.Add.VerticalLine(vertical.Start.ToOADate(), color: Colors.Red);
                _plt.Add.VerticalLine(vertical.End.ToOADate(), color: Colors.Red);
            }

            _plt.Axes.DateTimeTicksBottom();
            _plt.Legend.ManualItems = legendItems;
            _plt.Legend.IsVisible = true;
            _plt.XLabel(labelX);
            _plt.YLabel(labelY);
            _plt.Axes.Left.TickGenerator = new NumericManual(yTicks, yLabels);
            _plt.Axes.Bottom.TickLabelStyle.Rotation = 45;
            _plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            _plt.Layout.Fixed(new PixelPadding(50, 50, 150, 20));
            if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
                _plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
            else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
                _plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
            else
            {
                Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
            }
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

    }
}
