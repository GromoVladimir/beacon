using System;

namespace AshBeacon.Domain
{
    public enum Facing { Down, Right, Up, Left }

    public static class FacingExtensions
    {
        public static MoveVector Direction(this Facing facing)
        {
            switch (facing)
            {
                case Facing.Left: return new MoveVector(-1, 0);
                case Facing.Right: return new MoveVector(1, 0);
                case Facing.Up: return new MoveVector(0, 1);
                default: return new MoveVector(0, -1);
            }
        }
    }

    public readonly struct MoveVector
    {
        public MoveVector(float x, float y) { X = x; Y = y; }
        public float X { get; }
        public float Y { get; }
    }

    public sealed class PlayerMovement
    {
        public Facing Facing { get; private set; } = Facing.Down;

        public MoveVector Read(float x, float y)
        {
            if (float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y))
                return new MoveVector(0, 0);
            double length = Math.Sqrt((double)x * x + (double)y * y);
            if (length < 0.0001) return new MoveVector(0, 0);
            if (length > 1) { x /= (float)length; y /= (float)length; }
            bool horizontal = Facing == Facing.Left || Facing == Facing.Right;
            // 15 degrees beyond the diagonal before changing the dominant axis.
            const double threshold = 1.7320508075688772; // tan(45 + 15)
            if (horizontal && Math.Abs(y) > Math.Abs(x) * threshold) horizontal = false;
            else if (!horizontal && Math.Abs(x) > Math.Abs(y) * threshold) horizontal = true;
            Facing = horizontal ? (x < 0 ? Facing.Left : Facing.Right) : (y < 0 ? Facing.Down : Facing.Up);
            return new MoveVector(x, y);
        }
    }
}
