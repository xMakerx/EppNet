///////////////////////////////////////////////////////
/// Filename: ObjectRegister.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Objects;
using EppNet.Sim;
using EppNet.Utilities;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;

using static EppNet.Utilities.AttributeFetcher;

namespace EppNet.Registers
{

    public class ObjectRegister : Register<int, ISimUnit>
    {

        public static readonly ObjectRegister Instance = new ObjectRegister();
        public static ObjectRegister Get() => Instance;

        protected List<ObjectRegistration> _clientObjs = new List<ObjectRegistration>();
        protected List<ObjectRegistration> _serverObjs = new List<ObjectRegistration>();

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
                Add(i, r);

                if (attr.Dist == NetworkObjectAttribute.Distribution.Server)
                    _serverObjs.Add(r);
                else if (attr.Dist == NetworkObjectAttribute.Distribution.Client)
                    _clientObjs.Add(r);
            }

            if (_clientObjs.Count != _serverObjs.Count)
                Log.Fatal($"[ObjectRegister#Compile()] Objects not marked as Distribution.Both must have an individual client and server representation.");

            _compiled = true;
            return _compiled;
        }

    }

}
