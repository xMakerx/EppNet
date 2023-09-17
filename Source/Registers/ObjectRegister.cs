///////////////////////////////////////////////////////
/// Filename: ObjectRegister.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Objects;
using EppNet.Sim;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EppNet.Registers
{

    public class ObjectRegister : Register<int, ISimUnit>
    {

        public static readonly ObjectRegister Instance = new ObjectRegister();
        public static ObjectRegister Get() => Instance;

        public override bool Compile()
        {
            if (_compiled)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();

            Dictionary<string, ObjectRegistration> regs = new Dictionary<string, ObjectRegistration>();

            foreach (Type type in assembly.GetTypes())
            {

                if (!(type.IsClass && type is ISimUnit))
                    throw new ArgumentException("NetworkObjectAttributes must be attached to classes that inherit ISimUnit!");

                foreach (Attribute attr in type.GetCustomAttributes(false))
                {
                    if (attr is NetworkObjectAttribute netAttr)
                    {
                        ObjectRegistration r = new ObjectRegistration(type, netAttr);
                        r.Compile();

                        regs.Add(type.Name, r);
                    }
                }
            }

            string[] sortedNames = regs.Keys.ToArray();
            Array.Sort(sortedNames);

            for (int i = 0; i <  sortedNames.Length; i++)
                Add(i, regs[sortedNames[i]]);

            _compiled = true;
            return _compiled;
        }

    }

}
