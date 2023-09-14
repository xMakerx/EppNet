///////////////////////////////////////////////////////
/// Filename: Registration.cs
/// Date: September 13, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using System.Reflection;

namespace EppNet.Registers
{

    public class Registration<T> : IRegistration<T>
    {

        public delegate T ObjectActivator<T>(params object[] args);

        public readonly Type Type;
        protected internal IDictionary<Type[], ObjectActivator<T>> _ctorsDict;

        protected internal bool _compiled;

        public Registration()
        {
            this.Type = typeof(T);
            this._ctorsDict = new Dictionary<Type[], ObjectActivator<T>>();
            this._compiled = false;
        }

        protected void _Internal_CompileConstructors()
        {
            ConstructorInfo[] ctors = typeof(T).GetConstructors();

            for (int i = 0; i < ctors.Length; i++)
            {
                ConstructorInfo ctor = ctors[i];
                ParameterInfo[] paramsInfo = ctor.GetParameters();
                Type[] types = new Type[paramsInfo.Length];

                for (int j = 0; j < paramsInfo.Length; j++)
                    types[j] = paramsInfo[j].ParameterType;

                Type dType = ctor.DeclaringType;

                ParameterExpression param = Expression.Parameter(typeof(object[]), "args");
                Expression[] argsExp = new Expression[paramsInfo.Length];

                for (int k = 0; k < paramsInfo.Length; k++)
                {
                    Expression index = Expression.Constant(k);
                    Type paramType = types[k];

                    Expression accessorExp = Expression.ArrayIndex(param, index);

                    Expression paramCastExp = Expression.Convert(accessorExp, paramType);
                    argsExp[i] = paramCastExp;
                }

                NewExpression newExp = Expression.New(ctor, argsExp);
                LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

                ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
                _ctorsDict.Add(types, compiled);
            }
        }

        public virtual bool Compile()
        {
            if (IsCompiled())
                return false;

            _Internal_CompileConstructors();
            _compiled = true;

            return true;
        }

        public bool IsCompiled() => _compiled;

        public object NewInstance(params object[] args)
        {
            if (!_compiled)
                throw new Exception($"{this.GetType().Name} has not been compiled!");

            ObjectActivator<T> activator;

            foreach (Type[] ktArr in _ctorsDict.Keys)
            {
                if (ktArr.Length == args.Length)
                {
                    activator = _ctorsDict[ktArr];

                    for (int i = 0; i < ktArr.Length; i++)
                    {
                        Type a = ktArr[i];
                        Type b = args[i].GetType();

                        if (Nullable.GetUnderlyingType(a) != null)
                            a = Nullable.GetUnderlyingType(a);

                        if (!b.IsAssignableFrom(a))
                        {
                            activator = null;
                            break;
                        }
                    }

                    if (activator != null)
                        return activator.Invoke(args);
                }
            }

            return null;
        }

        public T Instance(params object[] args) => (T)NewInstance(args);

        /// <summary>
        /// Disposes of compiled expressions.
        /// </summary>

        public void Dispose()
        {
            _ctorsDict.Clear();
            _compiled = false;
        }
    }

}
