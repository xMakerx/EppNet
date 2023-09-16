///////////////////////////////////////////////////////
/// Filename: ObjectRegistration.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Registers;
using EppNet.Sim;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EppNet.Objects
{

    public class ObjectRegistration<T> : Registration<T> where T : ISimUnit
    {

        protected internal Dictionary<string, ObjectMethodDefinition<T>> _methods;
        protected internal ObjectMethodDefinition<T>[] _indexed_methods;

        public ObjectRegistration() : base()
        {
            this._methods = new Dictionary<string, ObjectMethodDefinition<T>>();
            this._indexed_methods = null;
        }

        protected void _Internal_CompileMethods()
        {
            MethodInfo[] methods = typeof(T).GetMethods();
            
            foreach (MethodInfo method in methods)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(method);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is NetworkMethodAttribute netAttr)
                        _methods.Add(method.Name, new ObjectMethodDefinition<T>(method, netAttr));
                }
            }

            // Let's sort our methods and assign ids.
            string[] methodNames = _methods.Keys.ToArray<string>();
            Array.Sort(methodNames);

            // Now we can initialize the indexed methods array
            _indexed_methods = new ObjectMethodDefinition<T>[methodNames.Length];

            // Indices are simply the position of the method name in the
            // sorted method names list.
            for (int i = 0; i < methodNames.Length; i++)
            {
                var definition = _methods[methodNames[i]];
                definition.Index = i;
                _indexed_methods[i] = definition;
            }

        }

        /// <summary>
        /// Fetches the <see cref="ObjectMethodDefinition{T}"/> by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>

        public ObjectMethodDefinition<T> GetMethod(int index)
        {
            ObjectMethodDefinition<T> definition = null;

            if (-1 < index && index < _indexed_methods.Length)
                definition = _indexed_methods[index];

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
