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

        public readonly Type ClassType;
        public readonly string Name;
        public readonly NetworkMethodAttribute Attribute;
        public readonly NetworkFlags Flags;
        public readonly Type[] ParameterTypes;
        public readonly ObjectMethodCall Activator;

        /// <summary>
        /// The index of our method within the sorted names of our object's
        /// network methods.
        /// </summary>
        public int Index { internal set; get; }

        public ObjectMethodDefinition(Type classType, MethodInfo method, NetworkMethodAttribute netAttr)
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

        }

        /// <summary>
        /// Invokes this method on the specified instance with the specified args.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        public void Invoke(object instance, params object[] args) => Activator.Invoke(instance, args);

    }

}

