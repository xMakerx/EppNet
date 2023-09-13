///////////////////////////////////////////////////////
/// Filename: ObjectFactory.cs
/// Date: January 22, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;

namespace EppNet.Reflection
{

    public interface IObjectFactory : IDisposable
    {
        object MakeNewGeneric(params object[] args);
    }

    public class ObjectFactory<T> : IObjectFactory
    {

        private Type _type;
        private Dictionary<Type[], Reflection.ObjectActivator<T>> _ctorsDict;
        private bool _setup;

        public ObjectFactory()
        {
            this._type = typeof(T);
            this._ctorsDict = new Dictionary<Type[], Reflection.ObjectActivator<T>>();
            this._setup = false;

            __Setup();
        }

        public object MakeNewGeneric(params object[] args) => New(args);

        public T New(params object[] args)
        {
            Reflection.ObjectActivator<T> activator = null;

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
                        {
                            a = Nullable.GetUnderlyingType(a);
                        }

                        if (!b.IsAssignableFrom(a))
                        {
                            activator = null;
                            break;
                        }
                    }

                    if (activator != null)
                    {
                        return activator.Invoke(args);
                    }
                }
            }

            return default(T);

        }

        private void __Setup()
        {
            if (_setup)
                return;

            ConstructorInfo[] ctors = _type.GetConstructors();

            for (int i = 0; i < ctors.Length; i++)
            {
                ConstructorInfo ctor = ctors[i];
                ParameterInfo[] paramsInfo = ctor.GetParameters();
                Type[] tArr = new Type[paramsInfo.Length];

                for (int j = 0; j < paramsInfo.Length; j++)
                {
                    tArr[j] = paramsInfo[j].ParameterType;
                }

                Reflection.ObjectActivator<T> activator = Reflection.GetActivator<T>(ctor);
                _ctorsDict.Add(tArr, activator);
            }

            _setup = true;
        }

        public void Dispose()
        {
            this._ctorsDict.Clear();
            this._ctorsDict = null;
        }

    }

}
