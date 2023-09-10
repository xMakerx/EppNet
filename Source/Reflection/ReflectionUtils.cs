///////////////////////////////////////////////////////
/// Filename: ReflectionUtils.cs
/// Date: January 22, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
/// System.Reflection will quickly bring the game
/// server and the client to its knees. This utility
/// utilizes the power of compiled LINQ expressions to
/// complete reflection-esque operations without such a
/// performance hit.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace EppNet.Reflection
{

    public static class Reflection
    {

        private static Dictionary<Type, IObjectFactory> _activators;

        public delegate T ObjectActivator<T>(params object[] args);
        static Reflection()
        {
            _activators = new Dictionary<Type, IObjectFactory>();
        }

        public static void Register<T>()
        {
            Type type = typeof(T);
            if (!_activators.ContainsKey(type))
            {
                _activators.Add(type, new ObjectFactory<T>());
            }
        }

        public static object NewInstance(Type type, params object[] args)
        {
            IObjectFactory objFact;

            if (!_activators.TryGetValue(type, out objFact))
            {
                throw new InvalidOperationException($"Did you forget to register Type {type}?");
            }

            return objFact.MakeNewGeneric(args);
        }

        public static T NewInstance<T>(params object[] args)
        {
            Type t = typeof(T);
            IObjectFactory objFact;

            if (!_activators.TryGetValue(t, out objFact))
            {
                objFact = new ObjectFactory<T>();
                _activators.Add(t, objFact);
            }

            ObjectFactory<T> crtFact = (ObjectFactory<T>) objFact;
            return crtFact.New(args);

        }

        public static void Clear()
        {
            _activators.Clear();
        }

        public static ObjectActivator<T> GetActivator<T>(ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp = new Expression[paramsInfo.Length];

            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp = Expression.ArrayIndex(param, index);

                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExp[i] = paramCastExp;
            }

            NewExpression newExp = Expression.New(ctor, argsExp);
            LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

            ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
            return compiled;
        }

    }

}
