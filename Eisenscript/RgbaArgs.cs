using System.Numerics;

namespace Eisenscript
{
    public delegate void RgbaEventHandler(object sender, RgbaArgs args);

    public class RgbaArgs : EventArgs
    {
        public RGBA Rgba { get; }

        public RgbaArgs(RGBA rgba)
        {
            Rgba = rgba;
        }
    }
}