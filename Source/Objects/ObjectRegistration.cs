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
        protected internal ObjectMemberDefinition[] _methods;
        protected internal ObjectMemberDefinition[] _props;

        public ObjectRegistration(Type type, NetworkObjectAttribute attribute) : base(type)
        {
            this.ObjectAttribute = attribute;
            this._methods = null;
            this._props = null;
        }

        protected void _Internal_CompileMembers()
        {
            PropertyInfo[] props = Type.GetProperties();
            MethodInfo[] methods = Type.GetMethods();

            MemberInfo[] members = new MemberInfo[props.Length + methods.Length];

            int index = 0;

            do
            {

                if (index < props.Length)
                {
                    members[index] = props[index];
                    index++;
                }
                else
                {
                    members[index] = methods[index - props.Length];
                    index++;
                }

            } while (index < members.Length);

            SortedList<string, ObjectMemberDefinition> sortedMethods = new SortedList<string, ObjectMemberDefinition>();
            SortedList<string, ObjectMemberDefinition> sortedProps = new SortedList<string, ObjectMemberDefinition>();

            foreach (MemberInfo member in members)
            {

                Attribute[] attributes = Attribute.GetCustomAttributes(member, false);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is NetworkMemberAttribute netAttr)
                    {
                        bool isMethod = member.MemberType == MemberTypes.Method;
                        string memberTypeName = member.MemberType == MemberTypes.Method ? "Method" : "Property";
                        MethodInfo getterMthd = (member is PropertyInfo) ? ((PropertyInfo)member).GetMethod : (netAttr as NetworkMethodAttribute).Getter;

                        Serilog.Log.Verbose($"[ObjectRegistration#Compile()] Compiling {memberTypeName} {member.Name}...");

                        // Properties must have a public setter and getter
                        if (!isMethod)
                        {
                            PropertyInfo propInfo = member as PropertyInfo;
                            if (!(propInfo.CanRead && propInfo.CanWrite))
                            {
                                string msg = $"[ObjectRegistration#Compile()] Property {propInfo.Name} must be readable and writable!";
                                throw new ArgumentException(msg);
                            }

                            ObjectMemberDefinition pDef = new ObjectMemberDefinition(propInfo, netAttr);
                            sortedProps.Add(propInfo.Name, pDef);
                        }
                        else
                        {
                            MethodInfo method = (member as MethodInfo);

                            if (netAttr.Flags.IsFlagSet(Core.NetworkFlags.Snapshot))
                            {

                                if (getterMthd == null)
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

                                        Serilog.Log.Verbose($"[ObjectRegistration#Compile()] Located companion getter Method {getterMthd.Name} for Method {member.Name}");
                                    }
                                }

                            }

                            ObjectMemberDefinition mDef = new ObjectMemberDefinition(method, netAttr, getterMthd);
                            sortedMethods.Add(method.Name, mDef);
                        }

                    }
                }
            }

            // Now we can initialize the indexed methods array
            _methods = sortedMethods.Values.ToArray();
            _props = sortedProps.Values.ToArray();

            // Let's set the indices for the method definitions
            for (int i = 0; i < _methods.Length; i++)
                _methods[i].Index = i;

            // Let's set the indices for the property definitions
            for (int i = 0; i < _props.Length; i++)
                _props[i].Index = i;

        }

        protected ObjectMemberDefinition _GetMember(int index, ref ObjectMemberDefinition[] array)
        {
            ObjectMemberDefinition definition = null;

            if (-1 < index && index < array.Length)
                definition = array[index];

            return definition;
        }

        /// <summary>
        /// Fetches a method's <see cref="ObjectMemberDefinition"/> by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>

        public ObjectMemberDefinition GetMethod(int index) => _GetMember(index, ref _methods);

        /// <summary>
        /// Fetches a property's <see cref="ObjectMemberDefinition"/> by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ObjectMemberDefinition GetProperty(int index) => _GetMember(index, ref _props);

        public override bool Compile()
        {
            if (IsCompiled())
                return false;

            Serilog.Log.Verbose($"[ObjectRegistration#Compile()] Compiling {GetRegisteredType().Name}...");
            _Internal_CompileConstructors();
            _Internal_CompileMembers();
            _compiled = true;

            return true;
        }

    }

}
