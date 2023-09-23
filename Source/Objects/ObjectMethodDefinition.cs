///////////////////////////////////////////////////////
/// Filename: ObjectMethodDefinition.cs
/// Date: September 15, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Core;

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EppNet.Objects
{

    public class ObjectMethodDefinition
    {

        public delegate void ObjectMethodCall(object instance, params object[] args);
        public delegate object ObjectMethodGetter(object instance);

        public readonly Type ClassType;
        public readonly string Name;
        public readonly NetworkMethodAttribute Attribute;
        public readonly NetworkFlags Flags;
        public readonly Type[] ParameterTypes;
        public readonly ObjectMethodCall Activator;

        public readonly ObjectMethodGetter GetterActivator;

        /// <summary>
        /// The index of our method within the sorted names of our object's
        /// network methods.
        /// </summary>
        public int Index { internal set; get; }

        public ObjectMethodDefinition(Type classType, MethodInfo method, NetworkMethodAttribute netAttr, MethodInfo getterMethod = null)
        {
            this.ClassType = classType;
            this.Name = method.Name;
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
        public void Invoke(object instance, params object[] args) => Activator.Invoke(instance, args);

        public object InvokeGetter(object instance)
        {
            if (GetterActivator == null)
                return Attribute.Getter.Invoke();

            return GetterActivator.Invoke(instance);
        }

    }

}

