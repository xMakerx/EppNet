///////////////////////////////////////////////////////
/// Filename: Vector4Resolver.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;

namespace EppNet.Data
{

    public class Vector4Resolver : VectorResolverBase<Vector4>
    {

        public static readonly Vector4Resolver Instance = new();

        public Vector4Resolver() : base(autoAdvance: false)
        {
            this.Default = Vector4.Zero;
            this.UnitX = Vector4.UnitX;
            this.UnitY = Vector4.UnitY;
            this.UnitZ = Vector4.UnitZ;
            this.UnitW = Vector4.UnitW;
            this.NumComponents = 4;
        }

        public override Span<float> GetFloats(Vector4 input, scoped ref Span<float> floats)
        {
            floats[0] = input.X;
            floats[1] = input.Y;
            floats[2] = input.Z;
            floats[3] = input.W;
            return floats;
        }

        public override Vector4 Put(Vector4 input, float value, int index)
        {
            input[index] = value;
            return input;
        }
    }

}
