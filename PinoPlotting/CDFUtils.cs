using ScottPlot;

namespace MyPlotting
{
	public static class CDFUtils
	{
		public const int DEFAULT_STEPS = 15;

		public static List<(int, double)> MakeCDF(IEnumerable<int> inputData, int steps = DEFAULT_STEPS)
		{
			int maxCommon = inputData.Max();
			int minCommon = inputData.Min();

			int p = maxCommon - minCommon;
			int bucketDimension = p / steps;

			if (bucketDimension == 0)
			{
				bucketDimension = 1;
			}

			List<(int, double)> results = new();

			for (int i = minCommon; i <= maxCommon; i += bucketDimension)
			{
				results.Add((i, inputData.Where(x => x <= i).Count() / (double)inputData.Count() * 100));
			}

			return results;
		}

		public static List<(double, double)> MakeCDF(IEnumerable<double> inputData, (double, double)? range = null, int steps = DEFAULT_STEPS)
		{
			double maxCommon = range == null ? inputData.Max() : range.Value.Item2;
			double minCommon = range == null ? inputData.Min() : range.Value.Item1;

			double p = maxCommon - minCommon;
			double bucketDimension = p / steps;

			if (bucketDimension == 0)
			{
				if (maxCommon == 0)
				{
					return new List<(double, double)> { (0, 100) };
				}
				bucketDimension = maxCommon / steps;
				minCommon -= bucketDimension * steps;
			}

			List<(double, double)> results = new()
			{
				(minCommon, 0)
			};

			double i;
			for (i = minCommon + bucketDimension; i <= maxCommon; i += bucketDimension)
			{
				results.Add((i, inputData.Where(x => x <= i).Count() / (double)inputData.Count() * 100));
			}
			if ((i - bucketDimension) != maxCommon)
			{
				results.Add((maxCommon, 100));
			}

			return results;
		}

		public static List<((double, double) bin, double)> MakePDF(IEnumerable<double> inputData, int steps = DEFAULT_STEPS)
		{
			if (!inputData.Any()) return new List<((double, double) bin, double)> { ((0, 0), 0) };

			double maxCommon = inputData.Max();
			double minCommon = inputData.Min();

			double p = maxCommon - minCommon;
			double bucketDimension = p / steps;

			List<((double, double) bin, double)> results = new();

			double i;
			double totalData = (double)inputData.Count();
			for (i = minCommon + bucketDimension; i <= maxCommon; i += bucketDimension)
			{
				results.Add(((i - bucketDimension, i), inputData.Where(x => x > (i - bucketDimension) && x <= i).Count() / totalData * 100));
			}
			//if (i != maxCommon)
			//{
			//	results.Add((i - bucketDimension, inputData.Where(x => x > (i - bucketDimension) && x <= maxCommon).Count()));
			//}

			return results;
		}

		public static List<(int, double)> MakeCDFLogIntegerBuckets(IEnumerable<int> inputData, int steps = DEFAULT_STEPS)
		{
			int maxCommon = inputData.Max();
			int minCommon = inputData.Min();

			double p = maxCommon / (double)minCommon;
			//Double bucketDimension = p / (Double)steps;

			IEnumerable<int> buckets = Enumerable.Range(0, steps + 1).Select(i => minCommon * Math.Pow(p, i / (double)steps)).Select(x => (int)x).Distinct();

			List<(int, double)> results = new();

			foreach (var b in buckets)
			{
				results.Add((b, inputData.Where(x => x <= b).Count() / (double)inputData.Count() * 100));
			}

			return results;
		}


	}
}

