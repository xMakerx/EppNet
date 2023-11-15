///////////////////////////////////////////////////////
/// Filename: ObjectRegistration.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Registers;
using EppNet.Utilities;

using Microsoft.Diagnostics.Tracing.Parsers.ClrPrivate;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Notify = EppNet.Utilities.LoggingExtensions;

namespace EppNet.Objects
{

    public class ObjectRegistration : Registration
    {

        public readonly NetworkObjectAttribute ObjectAttribute;
        protected internal SortedList<string, ObjectMemberDefinition> _methods;
        protected internal SortedList<string, ObjectMemberDefinition> _props;

        public ObjectRegistration(Type type, NetworkObjectAttribute attribute) : base(type)
        {
            this.ObjectAttribute = attribute;
            this._methods = null;
            this._props = null;
        }

        protected void _Internal_CompileMembers()
        {
            Type checkType = Type.BaseType;
            List<MemberInfo> members = new List<MemberInfo>();

            List<Type> types = new List<Type>() { Type };

            // Let's traverse the tree for base type network objects
            if (checkType != typeof(object))
            {
                // This type has at least one ancestor.

                // Calm down, calm down. This isn't an infinite loop
                // if you read the code below.
                while (true)
                {
                    if (!(checkType != null && checkType.IsDefined(typeof(NetworkObjectAttribute), false)))
                        // The type isn't a network object.
                        break;

                    // Ancestor has the network object attribute, let's add it and continue
                    // traversing the tree.
                    types.Add(checkType);
                    checkType = checkType.BaseType;
                }
            }

            Dictionary<string, List<MethodInfo>> name2Method = new Dictionary<string, List<MethodInfo>>();
            Dictionary<string, PropertyInfo> name2Property = new Dictionary<string, PropertyInfo>();

            for (int i = types.Count - 1; i > -1; i--)
            {
                Type type = types[i];

                foreach (MethodInfo method in type.GetMethods())
                {
                    // We only care about methods with the network member attribute.
                    if (!method.IsDefined(typeof(NetworkMemberAttribute), false))
                        continue;

                    // Let's consider if this method should be added as it's unique or replaced.
                    if (name2Method.TryGetValue(method.Name, out List<MethodInfo> results))
                    {
                        ParameterInfo[] mParams = method.GetParameters();
                        bool shouldAdd = true;

                        for (int j = 0; j < results.Count; j++)
                        {
                            MethodInfo result = results[j];
                            ParameterInfo[] rParams = result.GetParameters();

                            // Let's consider if we need to replace the existing result with this new one.
                            if (result.ReturnType == method.ReturnType && rParams.Length == mParams.Length)
                            {
                                int numMatches = 0;
                                for (int k = 0; k < rParams.Length; k++)
                                {
                                    if (rParams[k].ParameterType == mParams[k].ParameterType)
                                        numMatches++;
                                }

                                // Let's check if the method signature matches.
                                if (numMatches == mParams.Length)
                                {
                                    // They match. Let's replace the existing method in the array.
                                    results[j] = method;
                                    shouldAdd = false;
                                }
                            }
                        }

                        // Let's add the method if it's unique.
                        if (shouldAdd)
                            results.Add(method);

                        // Let's continue looping through the methods.
                        continue;
                    }

                    // This is the first entry of that method, let's add it.
                    name2Method.Add(method.Name, new List<MethodInfo> { method });
                }

                foreach (PropertyInfo prop in type.GetProperties())
                {
                    // We only care about properties with the network member attribute.
                    if (!prop.IsDefined(typeof(NetworkMemberAttribute), false))
                        continue;

                    // Properties must be readable and writeable
                    if (!(prop.CanWrite && prop.CanRead))
                    {
                        Notify.Error($"Property {prop.Name} in Type {prop.DeclaringType.Name} must be readable and writeable!");
                        continue;
                    }

                    name2Property[prop.Name] = prop;
                }
            }

            // Add all the methods
            foreach (KeyValuePair<string, List<MethodInfo>> kvp in name2Method)
                members.AddRange(kvp.Value);

            // Add all the properties
            members.AddRange(name2Property.Values);

            foreach (MemberInfo member in members)
            {

                Attribute[] attributes = Attribute.GetCustomAttributes(member, false);

                foreach (Attribute attribute in attributes)
                {

                    if (attribute is NetworkMemberAttribute netAttr)
                    {
                        bool isMethod = member.MemberType == MemberTypes.Method;
                        string memberTypeName = nameof(member.MemberType);
                        MethodInfo getterMthd = (member is PropertyInfo) ? ((PropertyInfo)member).GetMethod : (netAttr as NetworkMethodAttribute).Getter;

                        Notify.Verbose($"Compiling {memberTypeName} {member.Name}...");

                        // Properties must have a public setter and getter
                        if (!isMethod)
                        {
                            PropertyInfo propInfo = member as PropertyInfo;

                            ObjectMemberDefinition pDef = new ObjectMemberDefinition(propInfo, netAttr);

                            if (_props == null)
                                _props = new SortedList<string, ObjectMemberDefinition>(StringComparer.OrdinalIgnoreCase);

                            _props.Add(propInfo.Name, pDef);
                            continue;
                        }

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
                                    Notify.Error($"Method {method.Name} with NetworkFlag SNAPSHOT must have a specified getter or a " +
                                        $"companion function in the declaring type named \"{methodName}\".", out string msg);
                                    throw new ArgumentException(msg);
                                }
                                else
                                {
                                    // Let's validate the located getter
                                    ParameterInfo[] paramInfos = method.GetParameters();
                                    if (!(paramInfos.Length == 1 && paramInfos[0].ParameterType.IsAssignableFrom(getterMthd.ReturnType)
                                        && getterMthd.GetParameters().Length == 0))
                                    {
                                        Notify.Error($"Method {method.Name} with NetworkFlag SNAPSHOT must take 1 parameter with an assignable " +
                                            $"type from the return type of the companion getter in the declaring type. Example: void SetPosition(Vector3); Vector3 GetPosition()",
                                            out string msg);
                                        throw new ArgumentException(msg);
                                    }

                                    Notify.Verbose($"Located companion getter Method {getterMthd.Name} for Method {member.Name}");
                                }
                            }

                            ObjectMemberDefinition mDef = new ObjectMemberDefinition(method, netAttr, getterMthd);

                            if (_methods == null)
                                _methods = new SortedList<string, ObjectMemberDefinition>(StringComparer.OrdinalIgnoreCase);

                            _methods.Add(method.Name, mDef);
                        }

                    }
                }
            }

            if (_methods != null)
            {
                for (int i = 0; i < _methods.Values.Count; i++)
                {
                    ObjectMemberDefinition def = _methods.GetValueAtIndex(i);
                    def.Index = i;
                }
            }

            if (_props != null)
            {
                for (int i = 0; i < _props.Values.Count; i++)
                {
                    ObjectMemberDefinition def = _props.GetValueAtIndex(i);
                    def.Index = i;
                }
            }

        }

        protected ObjectMemberDefinition _GetMember(int index, ref SortedList<string, ObjectMemberDefinition> list)
        {
            ObjectMemberDefinition definition = null;

            if (-1 < index && index < list.Count)
                definition = list.GetValueAtIndex(index);

            return definition;
        }

        public ObjectMemberDefinition GetMemberByName(string name)
        {
            if (name == null || name?.Length == 0)
                return null;

            _methods.TryGetValue(name, out ObjectMemberDefinition def);

            if (def == null)
                _props.TryGetValue(name, out def);

            return def;
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

            Notify.Verbose($"Compiling {GetRegisteredType().Name}...");
            _Internal_CompileConstructors();
            _Internal_CompileMembers();
            _compiled = true;

            return true;
        }

    }

}
