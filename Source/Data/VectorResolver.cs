///////////////////////////////////////////////////////
/// Filename: VectorResolver.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Numerics;

namespace EppNet.Data
{

    public class Vector2Resolver : Resolver<Vector2>
    {
        protected override bool _Internal_Read(BytePayload payload, out Vector2 output)
        {
            float x = payload.ReadFloat();
            float y = payload.ReadFloat();
            output = new Vector2(x, y);
            return true;
        }

        protected override bool _Internal_Write(BytePayload payload, Vector2 input)
        {
            payload.WriteFloat(input.X);
            payload.WriteFloat(input.Y);
            return true;
        }
    }

    public class Vector3Resolver : Resolver<Vector3>
    {
        protected override bool _Internal_Read(BytePayload payload, out Vector3 output)
        {
            float x = payload.ReadFloat();
            float y = payload.ReadFloat();
            float z = payload.ReadFloat();
            output = new Vector3(x, y, z);
            return true;
        }

        protected override bool _Internal_Write(BytePayload payload, Vector3 input)
        {
            payload.WriteFloat(input.X);
            payload.WriteFloat(input.Y);
            payload.WriteFloat(input.Z);
            return true;
        }
    }

    public class Vector4Resolver : Resolver<Vector4>
    {
        protected override bool _Internal_Read(BytePayload payload, out Vector4 output)
        {
            float x = payload.ReadFloat();
            float y = payload.ReadFloat();
            float z = payload.ReadFloat();
            float w = payload.ReadFloat();
            output = new Vector4(x, y, z, w);
            return true;
        }

        protected override bool _Internal_Write(BytePayload payload, Vector4 input)
        {
            payload.WriteFloat(input.X);
            payload.WriteFloat(input.Y);
            payload.WriteFloat(input.Z);
            payload.WriteFloat(input.W);
            return true;
        }
    }

}
