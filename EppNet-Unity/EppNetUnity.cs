#define EPPNET_UNITY

using EppNet.Data;
using EppNet.Data.Unity;

namespace EppNet
{

    public static class EppNetUnity
    {

        static EppNetUnity()
        {
            BytePayload.AddResolver(typeof(UnityEngine.Vector4), UnityVector4Resolver.Instance);
            BytePayload.AddResolver(typeof(UnityEngine.Vector3), UnityVector3Resolver.Instance);
            BytePayload.AddResolver(typeof(UnityEngine.Vector3Int), UnityVector3IntResolver.Instance);
            BytePayload.AddResolver(typeof(UnityEngine.Vector2), UnityVector2Resolver.Instance);
            BytePayload.AddResolver(typeof(UnityEngine.Vector2Int), UnityVector2IntResolver.Instance);
            BytePayload.AddResolver(typeof(UnityEngine.Quaternion), UnityQuaternionResolver.Instance);
        }

    }

}