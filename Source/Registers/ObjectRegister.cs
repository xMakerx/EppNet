///////////////////////////////////////////////////////
/// Filename: ObjectRegister.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Core;
using EppNet.Objects;
using EppNet.Sim;
using EppNet.Utilities;

using Serilog;

using System.Linq;

using static EppNet.Utilities.AttributeFetcher;

namespace EppNet.Registers
{

    public class ObjectRegister : Register<int, ISimUnit>
    {

        public static readonly ObjectRegister Instance = new ObjectRegister();
        public static ObjectRegister Get() => Instance;

        public ObjectRegister() : base() { }

        public override bool Compile()
        {
            if (_compiled)
                return false;

            var list = AttributeFetcher.GetTypes<NetworkObjectAttribute>();
            AttributeWrapper[] wrappers = list.Values.ToArray();

            for (int i = 0; i < wrappers.Length; i++)
            {
                AttributeWrapper wrapper = wrappers[i];
                NetworkObjectAttribute attr = wrapper.Attribute as NetworkObjectAttribute;

                ObjectRegistration r = new ObjectRegistration(wrapper.Type, attr);
                Log.Verbose($"[ObjectRegister#Compile()] Compiling {wrapper.Type.Name}...");
                r.Compile();

                if (attr.Dist == Distribution.Shared || attr.Dist == Simulation.Get().DistroType)
                    Add(i, r);
            }

            _compiled = true;
            return _compiled;
        }

    }

}
