using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfiumViewer
{
    public struct RenderRange : IEquatable<RenderRange>
    {
        public static readonly RenderRange Invalid = new RenderRange(-1,-1);
        public RenderRange(int start, int end)
        {
            RenderStartIndex = start;
            RenderEndIndex = end;
        }
        public int RenderStartIndex { get; }
        public int RenderEndIndex { get; }

        public int RenderCenterPage => (RenderEndIndex - RenderStartIndex) / 2 + RenderStartIndex;
        public int Range => RenderEndIndex - RenderStartIndex + 1;
        public bool Equals(RenderRange other)
        {
            return this.RenderStartIndex == other.RenderStartIndex && this.RenderEndIndex == other.RenderEndIndex;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return this == (RenderRange)obj;
        }

        public override int GetHashCode()
        {
            int hc = RenderEndIndex.GetHashCode();
            hc = hc * 397 ^ RenderEndIndex.GetHashCode();
            return hc;
        }

        public static bool operator == (RenderRange left, RenderRange right)
        {
            return left.Equals(right);
        }

        public static bool operator != (RenderRange left, RenderRange right)
        {
            return !left.Equals(right);
        }
    }
}
