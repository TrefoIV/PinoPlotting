using ScottPlot;
using ScottPlot.AxisPanels;

namespace MyPlotting.Axis
{
	public class RotatedLabelAdaptableAxis : XAxisBase
	{
		public override Edge Edge => Edge.Bottom;

		public RotatedLabelAdaptableAxis(ITickGenerator tickGenerator)
		{
			TickGenerator = tickGenerator;
		}

		public override float Measure()
		{
			if (!IsVisible)
				return 0;

			if (!Range.HasBeenSet)
				return SizeWhenNoData;

			// Calculate tick mark height  
			float tickHeight = MajorTickStyle.Length;

			// For rotated labels, we need to account for both width and height  
			float maxTickLabelDimension = 0;
			if (TickGenerator.Ticks.Length > 0)
			{
				foreach (var tick in TickGenerator.Ticks)
				{
					MeasuredText labelSize = TickLabelStyle.Measure();

					// For rotated labels, calculate the projected height  
					float rotation = Math.Abs(TickLabelStyle.Rotation);
					float radians = rotation * (float)Math.PI / 180f;

					float projectedHeight = labelSize.Width * (float)Math.Sin(radians) +
										  labelSize.Height * (float)Math.Cos(radians);

					maxTickLabelDimension = Math.Max(maxTickLabelDimension, projectedHeight);
				}
			}

			// Calculate axis label space  
			float axisLabelHeight = string.IsNullOrEmpty(LabelStyle.Text) && LabelStyle.Image is null
				? EmptyLabelPadding.Vertical
				: LabelStyle.Measure().Height
					+ PaddingBetweenTickAndAxisLabels.Vertical
					+ PaddingOutsideAxisLabels.Vertical;

			return tickHeight + maxTickLabelDimension + axisLabelHeight;
		}
	}
}
