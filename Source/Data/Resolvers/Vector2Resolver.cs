///////////////////////////////////////////////////////
/// Filename: Vector2Resolver.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;

namespace EppNet.Data
{

    public class Vector2Resolver : VectorResolverBase<Vector2>
    {

        public static readonly Vector2Resolver Instance = new();

        public Vector2Resolver() : base(autoAdvance: false)
        {
            this.Default = Vector2.Zero;
            this.UnitX = Vector2.UnitX;
            this.UnitY = Vector2.UnitY;
            this.UnitZ = this.UnitW = Vector2.Zero;
            this.NumComponents = 2;
        }

        public override Span<float> GetFloats(Vector2 input, scoped ref Span<float> floats)
        {
            floats[0] = input.X;
            floats[1] = input.Y;
            return floats;
        }

        public override Vector2 Put(Vector2 input, float value, int index)
        {
            input[index] = value;
            return input;
        }
    }

}
