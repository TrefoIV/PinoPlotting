using ScottPlot;
using ScottPlot.TickGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlotting
{
    public class MultipleDistributionsPlotBuilder : AbstractPlot
    {
        public bool UseColorMap { get; private set; } = false;
        public bool CommonScale { get; set; } = false;

        private int _addedDistributions = 0;
        private List<string> _yLabels = new();

        private List<List<(double, double)>> _pdfsList = new();

        public MultipleDistributionsPlotBuilder(bool useColorMap, bool logX = false)
            : base(logX, false)
        { }


        public void AddDistribution(IEnumerable<double> data, string? yLabel = null)
        {
            if (!data.Any()) return;

            if (LogX)
            {
                _xGenerator ??= new(data.Min(), data.Max()) { LogBase = LogBaseX };
                _xGenerator.Min = Math.Min(_xGenerator.Min, data.Min());
                _xGenerator.Max = Math.Max(_xGenerator.Max, data.Max());

                data = data.Select(_xGenerator.Log);
            }

            var pdf = CDFUtils.MakePDF(data);
            _addedDistributions++;

            double y = _addedDistributions;
            var line = _plt.Add.HorizontalLine(y, color: Colors.DarkGrey, pattern: LinePattern.Dashed);
            line.LineWidth = 0.5f;
            _yLabels.Add(yLabel ?? $"{y}");
            _pdfsList.Add(pdf);



        }

        private void CreatePlots()
        {
            if (!_pdfsList.Any()) return;

            double? max = CommonScale ? _pdfsList.Max(pdf => pdf.Max(p => p.Item2)) : null;

            double[] ys = new double[_pdfsList.Count];
            string[] yLabels = new string[_pdfsList.Count];

            for (int i = 0; i < _pdfsList.Count; i++)
            {
                double y = _addedDistributions - i;
                max ??= _pdfsList[i].Max(p => p.Item2);

                foreach ((double x, double count) point in _pdfsList[i])
                {
                    var s = _plt.Add.Scatter(new Coordinates(point.x, y));
                    s.MarkerSize = (float)(point.count / max * 15);
                }

                ys[i] = y;
                yLabels[i] = _yLabels[i];
            }

            if (LogX)
            {
                _plt.Axes.Bottom.TickGenerator = _xGenerator;
            }
            _plt.Axes.Left.TickGenerator = new NumericManual(ys, yLabels);
        }

        public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
        {
            CreatePlots();

            _plt.Axes.Bottom.Label.Text = xLabel;
            _plt.Axes.Left.Label.Text = yLabel;

            if (Constants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
                _plt.SavePng(outFile.FullName + Constants.ImageFormat, 800, 600);
            else if (Constants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
                _plt.SaveSvg(outFile.FullName + Constants.ImageFormat, 800, 600);
            else
            {
                Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
            }
        }

    }
}
