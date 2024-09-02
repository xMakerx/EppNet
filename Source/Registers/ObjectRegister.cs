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
using System.Linq;

using static EppNet.Utilities.AttributeFetcher;

namespace EppNet.Registers
{

    public sealed class ObjectRegister : Register<int, ISimUnit>
    {

        public static readonly ObjectRegister Instance = new();
        public static ObjectRegister Get() => Instance;
        private ObjectRegister() { }

        public override CompilationResult Compile()
        {

            if (_compiled)
                return new();

            var list = AttributeFetcher.GetTypes<NetworkObjectAttribute>();
            AttributeWrapper[] wrappers = list.Values.ToArray();
            int compiledCount = 0;

            try
            {
                for (int i = 0; i < wrappers.Length; i++)
                {
                    AttributeWrapper wrapper = wrappers[i];
                    NetworkObjectAttribute attr = wrapper.Attribute as NetworkObjectAttribute;

                    ObjectRegistration r = new ObjectRegistration(wrapper.Type, attr);
                    Log.Verbose("[ObjectRegister#Compile()] Compiling {name}...", wrapper.Type.Name);
                    r.Compile();

                    _Internal_TryRegister(i, r);
                    compiledCount++;
                }

                _compiled = true;
            }
            catch (Exception ex)
            {
                return new(false, compiledCount, ex);
            }

            return new(_compiled, compiledCount, null);
        }

    }

}
