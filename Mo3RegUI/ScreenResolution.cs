using System;
using System.Collections.Generic;
using System.Linq;

namespace Mo3RegUI.Tasks
{
    /// <summary>
    /// A single screen resolution.
    /// </summary>
    public sealed record ScreenResolution : IComparable<ScreenResolution>
    {

        /// <summary>
        /// The width of the resolution in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the resolution in pixels.
        /// </summary>
        public int Height { get; }

        public ScreenResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public ScreenResolution(string resolution)
        {
            List<int> resolutionList = resolution.Trim().Split('x').Take(2).Select(int.Parse).ToList();
            Width = resolutionList[0];
            Height = resolutionList[1];
        }

        public static implicit operator ScreenResolution(string resolution) => new(resolution);

        public sealed override string ToString() => Width + "x" + Height;

        public static implicit operator string(ScreenResolution resolution) => resolution.ToString();

        public void Deconstruct(out int width, out int height)
        {
            width = this.Width;
            height = this.Height;
        }

        public static implicit operator ScreenResolution(Tuple<int, int> resolutionTuple) => new(resolutionTuple.Item1, resolutionTuple.Item2);

        public static implicit operator Tuple<int, int>(ScreenResolution resolution) => new(resolution.Width, resolution.Height);

        public bool Fits(ScreenResolution child) => this.Width >= child.Width && this.Height >= child.Height;

        public int CompareTo(ScreenResolution? other)
        {
            if (other is null)
                return 1;

            int widthComparison = this.Width.CompareTo(other.Width);
            if (widthComparison != 0)
                return widthComparison;

            int heightComparison = this.Height.CompareTo(other.Height);
            return heightComparison;
        }
        public static ScreenResolution GetDesktopScreenResolution()
        {
            // Note: declare DPI awareness in the manifest file, otherwise the result is wrong
            int monitor_width = NativeMethods.GetSystemMetrics(NativeConstants.SM_CXSCREEN);
            int monitor_height = NativeMethods.GetSystemMetrics(NativeConstants.SM_CYSCREEN);

            return new ScreenResolution(monitor_width, monitor_height);
        }
    }
}
