using System.Globalization;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ToolBox
{
    
    public static class Extensions
    {

        private static Dictionary<Type, MethodInfo> _validParseAsNullableMethod2 = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> _validParseAsNullableMethod4 = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// Parse a string into a nullable type.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <returns>Parsed value on success, null otherwise.</returns>
        public static Nullable<T> ParseAsNullable<T>(this string s) 
            where T : struct, IConvertible
        {
            var type = typeof(T);
            var ret = new Nullable<T>();
            
            // Using a hashset here is approximately
            // the same speed as this if statement.
            if (type == typeof(bool) ||
                    type == typeof(sbyte) || 
                    type == typeof(byte) || 
                    type == typeof(Int16) || 
                    type == typeof(UInt16) || 
                    type == typeof(Int32) || 
                    type == typeof(UInt32) || 
                    type == typeof(Int64) || 
                    type == typeof(UInt64) || 
                    type == typeof(int) || 
                    type == typeof(Single) || 
                    type == typeof(double) || 
                    type == typeof(decimal) || 
                    type == typeof(DateTime) ||
                    type == typeof(char))
            {
                // Caching the method goes from ~350 ticks to ~30 ticks 
                // on subsequent invocations.
                MethodInfo method = null;
                if (!_validParseAsNullableMethod2.TryGetValue(type, out method))
                {
                    method = type.GetMethods().Single(x => x.Name == "TryParse" && x.GetParameters().Length == 2);
                    _validParseAsNullableMethod2[type] = method;
                }
                
                // The last parameter is a reference to the out parameter in TryParse.
                // If the parse succeeds, the reference will point to a valid object.
                var parameters = new object[] { s, null };
                var result = (bool)method.Invoke(null, parameters);

                if (result)
                {
                    // The result is already the correct type but the compiler
                    // doesn't know that. 
                    ret = (T)parameters[1];
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            
            return ret;
        }

        /// <summary>
        /// Parse a string into a nullable type.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <param name="style">Style information.</param>
        /// <param name="provider">Format provider.</param>
        /// <returns>Parsed value on success, null otherwise.</returns>
        public static Nullable<T> ParseAsNullable<T>(this string s, NumberStyles style, IFormatProvider provider) 
            where T : struct, IConvertible
        {
            var type = typeof(T);
            var ret = new Nullable<T>();
            
            // no bool
            // no DateTime
            // no char
            if (type == typeof(sbyte) || 
                    type == typeof(byte) || 
                    type == typeof(Int16) || 
                    type == typeof(UInt16) || 
                    type == typeof(Int32) || 
                    type == typeof(UInt32) || 
                    type == typeof(Int64) || 
                    type == typeof(UInt64) || 
                    type == typeof(int) || 
                    type == typeof(Single) || 
                    type == typeof(double) || 
                    type == typeof(decimal))
            {
                MethodInfo method = null;
                if (!_validParseAsNullableMethod4.TryGetValue(type, out method))
                {
                    method = type.GetMethods().Single(x => x.Name == "TryParse" && x.GetParameters().Length == 4);
                    _validParseAsNullableMethod4[type] = method;
                }
                
                // The last parameter is a reference to the out paramter in TryParse.
                // If the parse succeeds, the reference will point to a valid object.
                var parameters = new object[] { s, style, provider, null };
                var result = (bool)method.Invoke(null, parameters);

                if (result)
                {
                    // The result is already the correct type but the compiler
                    // doesn't know that. 
                    ret = (T)parameters[3];
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            
            return ret;
        }

        /// <summary>
        /// Parse a string into a nullable type.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <param name="styles">Style information.</param>
        /// <param name="provider">Format provider.</param>
        /// <returns>Parsed value on success, null otherwise.</returns>
        public static Nullable<T> ParseAsNullable<T>(this string s, DateTimeStyles styles, IFormatProvider provider) 
            where T : struct, IConvertible
        {
            var type = typeof(T);
            var ret = new Nullable<T>();
            
            if (type == typeof(DateTime))
            {
                DateTime x;
                // DateTime TryParse swaps the provider and styles arguments
                // compared to the order for the other types above.
                if (DateTime.TryParse(s, provider, styles, out x))
                {
                    // The result is already the correct type but the compiler
                    // doesn't know that. This cast could also be written as
                    // ret = (T)((object)x);
                    ret = (T)Convert.ChangeType(x, type);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            
            return ret;
        }
        
        /// <summary>
        /// Attempts to get the value associated with the specified key from the <see
        /// cref="Dictionary{TKey,TValue}"/>. If not found, action will be executed,
        /// and the dictionary will be updated with the returned value.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, <paramref name="value"/> contains the object from
        /// the
        /// <see cref="Dictionary{TKey,TValue}"/> with the specified key or the default value of
        /// <typeparamref name="TValue"/>, if the operation failed.</param>
        /// <param name="action">Action to perform if the specified key is not found.</param>
        /// <returns>true if the key was found in the <see cref="ConcurrentDictionary{TKey,TValue}"/>;
        /// otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null 
        /// reference.</exception>
        public static void TryGetValueAction<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, out TValue value, Func<TKey, TValue> action)
        {               
            if (!dict.TryGetValue(key, out value))
            {
                value = action(key);
                dict[key] = value;
            }
        }

    }

}
