///////////////////////////////////////////////////////
/// Filename: ObjectRegistration.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Registers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EppNet.Objects
{

    public class ObjectRegistration : Registration
    {

        public readonly NetworkObjectAttribute ObjectAttribute;
        protected internal ObjectMethodDefinition[] _methods;

        public ObjectRegistration(Type type, NetworkObjectAttribute attribute) : base(type)
        {
            this.ObjectAttribute = attribute;
            this._methods = null;
        }

        protected void _Internal_CompileMethods()
        {
            MethodInfo[] methods = Type.GetMethods();
            SortedList<string, ObjectMethodDefinition> sortedList = new SortedList<string, ObjectMethodDefinition>();
            
            foreach (MethodInfo method in methods)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(method);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is NetworkMethodAttribute netAttr)
                        sortedList.Add(method.Name, new ObjectMethodDefinition(Type, method, netAttr));
                }
            }

            // Now we can initialize the indexed methods array
            _methods = sortedList.Values.ToArray();

        }

        /// <summary>
        /// Fetches the <see cref="ObjectMethodDefinition{T}"/> by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>

        public ObjectMethodDefinition GetMethod(int index)
        {
            ObjectMethodDefinition definition = null;

            if (-1 < index && index < _methods.Length)
                definition = _methods[index];

            return definition;
        }

        public override bool Compile()
        {
            if (IsCompiled())
                return false;

            _Internal_CompileConstructors();
            _Internal_CompileMethods();
            _compiled = true;

            return true;
        }

    }

}
