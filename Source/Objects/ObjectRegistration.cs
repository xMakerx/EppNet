///////////////////////////////////////////////////////
/// Filename: ObjectRegistration.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Registers;
using EppNet.Utilities;

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
                Serilog.Log.Verbose($"[ObjectRegistration#Compile()] Compiling Method {method.Name}...");
                Attribute[] attributes = Attribute.GetCustomAttributes(method);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is NetworkMethodAttribute netAttr)
                    {
                        // Let's validate the attribute input
                        MethodInfo getterMthd = null;

                        if (netAttr.Flags.IsFlagSet(Core.NetworkFlags.Snapshot) && netAttr.Getter == null)
                        {
                            // Let's try to find a getter.
                            string methodName = method.Name.ToLower();

                            if (methodName.StartsWith("set"))
                            {
                                // Let's try to find the companion getter
                                methodName = "get" + methodName.Substring(3);
                                getterMthd = method.DeclaringType.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Instance);
                            }

                            if (getterMthd == null)
                            {
                                string msg = $"[ObjectRegistration#Compile()] Method {method.Name} with NetworkFlag SNAPSHOT must have a specified getter or a " +
                                    $"companion function in the declaring type named \"{methodName}\".";
                                throw new ArgumentException(msg);
                            }
                            else
                            {
                                // Let's validate the located getter
                                ParameterInfo[] paramInfos = method.GetParameters();
                                if (!(paramInfos.Length == 1 && paramInfos[0].ParameterType.IsAssignableFrom(getterMthd.ReturnType)
                                    && getterMthd.GetParameters().Length == 0))
                                {
                                    string msg = $"[ObjectRegistration#Compile()] Method {method.Name} with NetworkFlag SNAPSHOT must take 1 parameter with an assignable " +
                                        $"type from the return type of the companion getter in the declaring type. Example: void SetPosition(Vector3); Vector3 GetPosition()";
                                    throw new ArgumentException(msg);
                                }

                                Serilog.Log.Verbose($"[ObjectRegistration#Compile()] Located companion getter Method {getterMthd.Name} for Method {method.Name}");
                            }
                            
                        }

                        ObjectMethodDefinition mDef = new ObjectMethodDefinition(Type, method, netAttr, getterMthd);
                        sortedList.Add(method.Name, mDef);
                    }
                }
            }

            // Now we can initialize the indexed methods array
            _methods = sortedList.Values.ToArray();

            // Let's set the indices for the method definitions
            for (int i = 0; i < _methods.Length; i++)
                _methods[i].Index = i;

        }

        /// <summary>
        /// Fetches the <see cref="ObjectMethodDefinition"/> by index.
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

            Serilog.Log.Verbose($"[ObjectRegistration#Compile()] Compiling {GetRegisteredType().Name}...");
            _Internal_CompileConstructors();
            _Internal_CompileMethods();
            _compiled = true;

            return true;
        }

    }

}
