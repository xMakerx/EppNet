///////////////////////////////////////////////////////
/// Filename: ObjectMemberDefinition.cs
/// Date: September 15, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Data;
using EppNet.Sim;

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EppNet.Objects
{

    public class ObjectMemberDefinition
    {

        public delegate void ObjectMethodCall(ISimUnit instance, params object[] args);
        public delegate object ObjectMethodGetter(ISimUnit instance);

        public readonly Type ClassType;
        public readonly string Name;
        public readonly NetworkMemberAttribute Attribute;
        public readonly SlottableEnum Flags;
        public readonly Type[] ParameterTypes;
        public readonly ObjectMethodCall Activator;

        public readonly ObjectMethodGetter GetterActivator;
        public readonly MemberInfo MemberInfo;

        /// <summary>
        /// The index of our member within the appropriate sorted member
        /// array of our object.
        /// </summary>
        public int Index { internal set; get; }

        public ObjectMemberDefinition(PropertyInfo propInfo, NetworkMemberAttribute netAttr) : this(propInfo, netAttr, propInfo.SetMethod, propInfo.GetMethod) { }

        public ObjectMemberDefinition(MethodInfo method, NetworkMemberAttribute netAttr, MethodInfo getterMethod = null) : this(method, netAttr, method, getterMethod) { }

        private ObjectMemberDefinition(MemberInfo memberInfo, NetworkMemberAttribute netAttr, MethodInfo method, MethodInfo getterMethod = null)
        {
            this.ClassType = memberInfo.DeclaringType;
            this.MemberInfo = memberInfo;
            this.Name = memberInfo.Name;
            this.Attribute = netAttr;
            this.Flags = netAttr.Flags;
            this.Index = -1;

            ParameterInfo[] paramsInfo = method.GetParameters();
            Type[] types = new Type[paramsInfo.Length];

            ParameterExpression argsExp = Expression.Parameter(typeof(object[]), "args");
            Expression[] parameters = new Expression[paramsInfo.Length];

            for (int i = 0; i < paramsInfo.Length; i++)
            {
                ParameterInfo param = paramsInfo[i];
                Type paramType = param.ParameterType;

                Expression indexExp = Expression.Constant(i);
                Expression accessorExp = Expression.ArrayIndex(argsExp, indexExp);
                Expression paramCastExp = Expression.Convert(accessorExp, paramType);

                parameters[i] = paramCastExp;
                types[i] = paramType;
            }

            var instanceExp = Expression.Parameter(ClassType);
            var callExp = Expression.Call(instanceExp, method, parameters);
            ParameterExpression[] outerParams = new[] { instanceExp, argsExp };
            this.ParameterTypes = types;

            ObjectMethodCall compiled = (ObjectMethodCall)Expression.Lambda(typeof(ObjectMethodCall),
                callExp, outerParams).Compile();
            this.Activator = compiled;

            if (getterMethod == null)
            {
                this.GetterActivator = null;
                return;
            }

            var getterCallExp = Expression.Call(instanceExp, getterMethod);
            ObjectMethodGetter compiledGetter = (ObjectMethodGetter)Expression.Lambda(
                typeof(ObjectMethodGetter), getterCallExp).Compile();

            this.GetterActivator = compiledGetter;
        }

        /// <summary>
        /// Invokes this method on the specified instance with the specified args.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        public void Invoke(ISimUnit instance, params object[] args) => Activator.Invoke(instance, args);
        public void Invoke(ObjectAgent instance, params object[] args) => Activator.Invoke(instance.UserObject, args);

        public object InvokeGetter(ISimUnit instance) => GetterActivator?.Invoke(instance);
        public object InvokeGetter(ObjectAgent instance) => GetterActivator?.Invoke(instance.UserObject);

        public bool IsMethod() => MemberInfo.MemberType == MemberTypes.Method;
        public bool IsProperty() => MemberInfo.MemberType == MemberTypes.Property;

    }

}

