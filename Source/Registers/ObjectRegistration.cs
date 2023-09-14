///////////////////////////////////////////////////////
/// Filename: ObjectRegistration.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Sim;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EppNet.Registers
{

    public class ObjectRegistration<T> : Registration<T> where T : ISimUnit
    {

        public Dictionary<string, ObjectMethodCall<T>> _method_activators;

        public ObjectRegistration() : base()
        {
            this._method_activators = new Dictionary<string, ObjectMethodCall<T>>();
        }

        protected void _Internal_CompileMethods()
        {
            MethodInfo[] methods = typeof(T).GetMethods();

            foreach (MethodInfo method in methods)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(method);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is NetworkAttribute net_attr)
                    {

                        /*
                        var parameters = method.GetParameters();
                        var thisParam = Expression.Parameter(typeof(T));
                        var valueParams = parameters.Select(Expression.Constant).ToList();
                        var call = Expression.Call(thisParam, method, valueParams);
                        var func = Expression.Lambda(call, Expression.Parameter(typeof(T))).Compile();
                        */
                        
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
                        }

                        var instanceExp = Expression.Parameter(typeof(T));
                        var callExp = Expression.Call(instanceExp, method, parameters);
                        ParameterExpression[] outerParams = new[] { instanceExp, argsExp };

                        ObjectMethodCall<T> compiled = (ObjectMethodCall<T>)Expression.Lambda(typeof(ObjectMethodCall<T>), callExp, new[] { instanceExp, argsExp }).Compile();
                        _method_activators.Add(method.Name, compiled);
                    }
                }
            }

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
