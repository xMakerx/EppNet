///////////////////////////////////////////////////////
/// Filename: Registration.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using System.Reflection;

namespace EppNet.Registers
{

    public class Registration : IRegistration
    {

        public delegate object ObjectGenerator(params object[] args);

        public readonly Type Type;
        protected internal IDictionary<Type[], ObjectGenerator> _ctorsDict;

        protected internal bool _compiled;

        public Registration(Type type)
        {
            this.Type = type;
            this._ctorsDict = new Dictionary<Type[], ObjectGenerator>();
            this._compiled = false;
        }

        protected int _Internal_CompileConstructors()
        {
            ConstructorInfo[] ctors = Type.GetConstructors();

            for (int i = 0; i < ctors.Length; i++)
            {
                ConstructorInfo ctor = ctors[i];
                ParameterInfo[] paramsInfo = ctor.GetParameters();
                Type[] types = new Type[paramsInfo.Length];

                Type dType = ctor.DeclaringType;

                ParameterExpression param = Expression.Parameter(typeof(object[]), "args");
                Expression[] argsExp = new Expression[paramsInfo.Length];

                for (int k = 0; k < paramsInfo.Length; k++)
                {
                    Expression index = Expression.Constant(k);
                    Type paramType = paramsInfo[k].ParameterType;

                    Expression accessorExp = Expression.ArrayIndex(param, index);

                    argsExp[k] = Expression.Convert(accessorExp, paramType); // Was argsExp[i] before
                    types[k] = paramType;
                }

                NewExpression newExp = Expression.New(ctor, argsExp);
                LambdaExpression lambda = Expression.Lambda(typeof(ObjectGenerator), newExp, param);

                ObjectGenerator compiled = (ObjectGenerator)lambda.Compile();
                _ctorsDict.Add(types, compiled);
            }

            return ctors.Length;
        }

        public virtual CompilationResult Compile()
        {
            if (IsCompiled())
                return new();

            int nCompiled = 0;

            try
            {
                nCompiled = _Internal_CompileConstructors();
                _compiled = true;
            }
            catch (Exception ex)
            {
                return new(false, nCompiled, ex);
            }

            return new(_compiled, nCompiled, null);
        }

        public bool IsCompiled() => _compiled;

        public object NewInstance(params object[] args)
        {
            if (!_compiled)
                throw new Exception($"{this.GetType().Name} has not been compiled!");

            ObjectGenerator generator;

            foreach (Type[] ktArr in _ctorsDict.Keys)
            {
                if (ktArr.Length == args.Length)
                {
                    generator = _ctorsDict[ktArr];

                    for (int i = 0; i < ktArr.Length; i++)
                    {
                        Type a = ktArr[i];
                        Type b = args[i].GetType();

                        if (Nullable.GetUnderlyingType(a) != null)
                            a = Nullable.GetUnderlyingType(a);

                        if (!b.IsAssignableFrom(a))
                        {
                            generator = null;
                            break;
                        }
                    }

                    if (generator != null)
                        return generator.Invoke(args);
                }
            }

            return null;
        }

        public Type GetRegisteredType() => Type;

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

