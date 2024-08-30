///////////////////////////////////////////////////////
/// Filename: Vector3Resolver.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;

namespace EppNet.Data
{

    public class Vector3Resolver : VectorResolverBase<Vector3>
    {

        public static readonly Vector3Resolver Instance = new();

        public Vector3Resolver() : base(autoAdvance: false)
        {
            this.Default = Vector3.Zero;
            this.UnitX = Vector3.UnitX;
            this.UnitY = Vector3.UnitY;
            this.UnitZ = Vector3.UnitZ;
            this.UnitW = Vector3.Zero;
            this.NumComponents = 3;
        }

        public override Span<float> GetFloats(Vector3 input, scoped ref Span<float> floats)
        {
            floats[0] = input.X;
            floats[1] = input.Y;
            floats[2] = input.Z;
            return floats;
        }

        public override Vector3 Put(Vector3 input, float value, int index)
        {
            input[index] = value;
            return input;
        }
    }

}
