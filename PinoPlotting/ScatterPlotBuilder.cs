using ScottPlot;
using ScottPlot.TickGenerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyPlotting
{
    public class ScatterPlotBuilder : AbstractPlot
    {

        private double minX = 0;
        private double maxX = 0;
        private double minY = 0;
        private double maxY = 0;

        private HashSet<(double x, double y)> _points = new();

        public ScatterPlotBuilder(bool logX = false, bool logY = false)
            : base(logX, logY)
        {
        }

        public void AddDataToPlot(IEnumerable<(double x, double y)> inputData)
        {
            if (!inputData.Any()) return;
            if (LogX)
            {
                inputData = inputData.Where(p => p.x > 0).Select(p => (Math.Log10(p.x), p.y)).ToList();
                if (!inputData.Any())
                {
                    return;
                }

            }

            double[] xs = inputData.Select(f => f.x).ToArray();
            double[] ys = inputData.Select(f => f.y).ToArray();


            if (LogY)
            {
                xs = xs.Where((x, i) => ys[i] > 0).ToArray();
                ys = ys.Where(y => y > 0).Select(y => Math.Log10(y)).ToArray();
            }
            foreach (var p in xs.Select((x, i) => (x, ys[i]))) _points.Add(p);
        }

        public void AddDataToPlot(IEnumerable<(int x, int y)> inputData)
        {
            if (LogX || LogY)
            {
                AddDataToPlot(inputData.Select(p => ((double)p.x, (double)p.y)));
            }
            if (!inputData.Any()) return;

            int[] xs = inputData.Select(f => f.x).ToArray();
            int[] ys = inputData.Select(f => f.y).ToArray();
            CheckMinMax(xs.Select(x => (double)x), ys.Select(x => (double)x));
            foreach (var p in xs.Select((x, i) => (x, ys[i]))) _points.Add(p);
        }

        private void CheckMinMax(IEnumerable<double> xs, IEnumerable<double> ys)
        {
            int minx = (int)xs.Min() + 1;
            int maxx = (int)xs.Max() + 1;
            maxX = maxx > maxX ? maxx : maxX;
            minX = minx < minX ? minx : minX;

            int miny = (int)ys.Min() + 1;
            int maxy = (int)ys.Max() + 1;
            maxY = maxy > maxY ? maxy : maxY;
            minY = miny < minY ? miny : minY;
        }


        public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
        {
            var scatter = _plt.Add.Markers(_points.Select(p => p.x).ToArray(), _points.Select(p => p.y).ToArray(), size: 3);

            if (LogX)
            {
                maxX = (int)Math.Log10(maxX) + 1;
                _plt.Axes.Bottom.TickGenerator = new NumericAutomatic()
                {
                    MinorTickGenerator = new LogMinorTickGenerator(),
                    LabelFormatter = x => $"{Math.Pow(10, x):N}",
                };

            }
            else
            {
                double exp = Math.Floor(Math.Log10(maxX / 2));
                if (exp > 1)
                    _plt.Axes.Bottom.TickGenerator = new NumericFixedInterval((int)Math.Pow(10, exp) / 2);
                else
                    _plt.Axes.Bottom.TickGenerator = new NumericAutomatic();
            }

            if (LogY)
            {
                maxY = (int)Math.Log10(maxY) + 1;
                _plt.Axes.Left.TickGenerator = new NumericAutomatic()
                {
                    MinorTickGenerator = new LogMinorTickGenerator(),
                    LabelFormatter = x => $"{Math.Pow(10, x):N3}"
                };
            }
            else
            {
                var exp = Math.Floor(Math.Log10(maxY / 2));
                if (exp > 1)
                    _plt.Axes.Left.TickGenerator = new NumericFixedInterval((int)Math.Pow(10, exp) / 2);
                else
                    _plt.Axes.Left.TickGenerator = new NumericAutomatic();
            }


            _plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperCenter;
            _plt.Axes.SetLimitsY(bottom: minY, top: maxY);
            _plt.Axes.SetLimitsX(left: minX, right: maxX);
            _plt.Grid.MajorLineWidth = 1;
            _plt.Grid.MajorLineColor = Colors.Black.WithLightness(0.7f);

            _plt.Layout.Fixed(new PixelPadding(top: 15, right: 10, left: 150, bottom: 50));

            _plt.XLabel(xLabel);
            _plt.YLabel(yLabel);

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
