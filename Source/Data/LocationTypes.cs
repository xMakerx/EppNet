///////////////////////////////////////////////////////
/// Filename: LocationTypes.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Data
{

    public struct Loc3
    {

        public static Loc3 operator +(Loc3 lhs, Loc3 rhs) =>
            new Loc3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);

        public static Loc3 operator -(Loc3 lhs, Loc3 rhs) =>
            new Loc3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);

        public static Loc3 operator /(Loc3 lhs, Loc3 rhs)
            => new Loc3(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z);

        public static Loc3 operator *(Loc3 lhs, Loc3 rhs)
            => new Loc3(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z);

        public static bool operator ==(Loc3 lhs, Loc3 rhs) => lhs.Equals(rhs);

        public static bool operator !=(Loc3 lhs, Loc3 rhs) => !lhs.Equals(rhs);

        public static implicit operator Loc2(Loc3 a) => new Loc2(a.X, a.Y);

        public float X, Y, Z;

        public Loc3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Returns the Manhattan Distance between this location and
        /// the specified one.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>

        public float ManDist(Loc3 b) =>
            Math.Abs(this.X - b.X) + Math.Abs(this.Y - b.Y) + Math.Abs(this.Z - b.Z);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Loc3 b)
                return X == b.X && Y == b.Y && Z == b.Z;

            return false;
        }

        public override int GetHashCode() => 
            X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();

    }

    public class Loc3Resolver : Resolver<Loc3>
    {
        protected override bool _Internal_Read(BytePayload payload, out Loc3 output)
        {
            float x = payload.ReadFloat();
            float y = payload.ReadFloat();
            float z = payload.ReadFloat();
            output = new Loc3(x, y, z);
            return true;
        }

        protected override bool _Internal_Write(BytePayload payload, Loc3 input)
        {
            payload.WriteFloat(input.X);
            payload.WriteFloat(input.Y);
            payload.WriteFloat(input.Z);
            return true;
        }
    }

    public struct Loc2
    {

        public static Loc2 operator +(Loc2 lhs, Loc2 rhs) =>
            new Loc2(lhs.X + rhs.X, lhs.Y + rhs.Y);

        public static Loc2 operator -(Loc2 lhs, Loc2 rhs) =>
            new Loc2(lhs.X - rhs.X, lhs.Y - rhs.Y);

        public static Loc2 operator /(Loc2 lhs, Loc2 rhs)
            => new Loc2(lhs.X / rhs.X, lhs.Y / rhs.Y);

        public static Loc2 operator *(Loc2 lhs, Loc2 rhs)
            => new Loc2(lhs.X * rhs.X, lhs.Y * rhs.Y);

        public static bool operator ==(Loc2 lhs, Loc2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(Loc2 lhs, Loc2 rhs) => !lhs.Equals(rhs);

        public static implicit operator Loc3(Loc2 a) => new Loc3(a.X, a.Y, 0);

        public float X, Y;

        public Loc2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Returns the Manhattan Distance between this location and
        /// the specified one.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>

        public float ManDist(Loc2 b) =>
            Math.Abs(this.X - b.X) + Math.Abs(this.Y - b.Y);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Loc2 b)
                return X == b.X && Y == b.Y;

            return false;
        }

        public override int GetHashCode() =>
            X.GetHashCode() + Y.GetHashCode();

    }

    public class Loc2Resolver : Resolver<Loc2>
    {
        protected override bool _Internal_Read(BytePayload payload, out Loc2 output)
        {
            float x = payload.ReadFloat();
            float y = payload.ReadFloat();
            output = new Loc2(x, y);
            return true;
        }

        protected override bool _Internal_Write(BytePayload payload, Loc2 input)
        {
            payload.WriteFloat(input.X);
            payload.WriteFloat(input.Y);
            return true;
        }
    }

}
