﻿///////////////////////////////////////////////////////
/// Filename: IZone.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Numerics;

namespace EppNet.Zones
{

    public interface IZone
    {

        public Vector2 GetCenter() => (GetMinBounds() + GetMaxBounds()) / 2f;

        public Vector2 GetCellSize() => Vector2.Zero;
        public Vector2 GetMaxBounds() => GetMinBounds() + GetCellSize();
        public Vector2 GetMinBounds() => Vector2.Zero;
    }

}
