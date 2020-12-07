// (c) Johannes Wolfgruber, 2020

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SystemAudioRecordingSoftware.Common.UI.Controls
{
    internal enum LineType
    {
        MainWaveformLine,
        OverviewWaveformLine
    }

    internal sealed class LineContainer
    {
        public LineContainer(TimeSpan timestamp, Line mainWaveformLine, Line overviewWaveformLine)
        {
            Timestamp = timestamp;
            MainWaveformLine = mainWaveformLine;
            OverviewWaveformLine = overviewWaveformLine;

            MainWaveformLine.MouseEnter += (_, _) => Mouse.OverrideCursor = Cursors.SizeWE;
            MainWaveformLine.MouseLeave += (_, _) => Mouse.OverrideCursor = Cursors.Arrow;
            OverviewWaveformLine.MouseEnter += (_, _) => Mouse.OverrideCursor = Cursors.Hand;
            OverviewWaveformLine.MouseLeave += (_, _) => Mouse.OverrideCursor = Cursors.Arrow;
        }

        public LineContainer(TimeSpan timestamp)
            : this(timestamp, new Line(), new Line())
        {
        }

        public TimeSpan Timestamp { get; set; } // TODO: rework

        public Line MainWaveformLine { get; }

        public Line OverviewWaveformLine { get; }

        public Line GetLine(LineType lineType)
        {
            return lineType switch
            {
                LineType.MainWaveformLine => MainWaveformLine,
                LineType.OverviewWaveformLine => OverviewWaveformLine,
                _ => throw new ArgumentOutOfRangeException(nameof(lineType), lineType, "Unknown line type.")
            };
        }

        public void SetStroke(Brush brush)
        {
            MainWaveformLine.Stroke = brush;
            OverviewWaveformLine.Stroke = brush;
        }

        public void UpdateMainWaveformLineX(Func<TimeSpan, double> timeToX, double min, double max)
        {
            var x = timeToX(Timestamp);
            MainWaveformLine.X1 = x;
            MainWaveformLine.X2 = x;
            MainWaveformLine.Visibility = x > min && x < max ? Visibility.Visible : Visibility.Collapsed;
        }

        public void UpdateOverviewWaveformLineX(Func<TimeSpan, double> timeToX)
        {
            var x = timeToX(Timestamp);
            OverviewWaveformLine.X1 = x;
            OverviewWaveformLine.X2 = x;
        }
    }
}