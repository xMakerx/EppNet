///////////////////////////////////////////////////////
/// Filename: AttributeFetcher.cs
/// Date: September 19, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

namespace EppNet.Utilities
{

    public static class AttributeFetcher
    {

        public class AttributeWrapper
        {
            public readonly Type Type;
            public readonly Attribute Attribute;

            public AttributeWrapper(Type type, Attribute attribute)
            {
                this.Type = type;
                this.Attribute = attribute;
            }
        }

        private static Dictionary<Type, SortedList<string, AttributeWrapper>> _dict = new Dictionary<Type, SortedList<string, AttributeWrapper>>();
        private static Dictionary<Type, Func<Type, bool>> _checkers = new Dictionary<Type, Func<Type, bool>>();
        private static bool _fetched = false;

        public static bool AddType<T>(Func<Type, bool> action = null) where T : Attribute
        {
            if (_dict.ContainsKey(typeof(T)))
                return false;

            _dict.Add(typeof(T), new SortedList<string, AttributeWrapper>());
            _checkers.Add(typeof(T), action);
            return true;
        }

        public static SortedList<string, AttributeWrapper> GetTypes<T>() where T : Attribute
        {
            TryFetchAll();
            _dict.TryGetValue(typeof(T), out var list);
            return list;
        }

        public static void TryFetchAll()
        {
            if (_fetched)
                return;

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (Attribute attr in type.GetCustomAttributes(false))
                {
                    _dict.TryGetValue(attr.GetType(), out var list);
                    
                    // If there is no list that means we don't care
                    // about this particular attribute
                    if (list == null)
                        continue;

                    // Fetches the "checker" function
                    _checkers.TryGetValue(attr.GetType(), out var f);

                    // Call the "checker" function if it exists and
                    // sees if the input passes. Otherwise, add by
                    // default.
                    if (f == null || (f != null && f.Invoke(type)))
                        list.Add(type.Name, new AttributeWrapper(type, attr));
                }
            }

            _fetched = true;
        }

    }

}
