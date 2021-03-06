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
        
        /* **********************************************************************

        // An alternative implementation to convert to a nullable type.
        //
        // Advantages: 
        // - A bit simpler.
        // - No global static variables.
        //
        // Disadvantages:
        // - This takes about 5x longer than the TryParse method.
        // - There is an overload to accept a CultureInfo, but it also
        //   requires a type converter context, so this is a bit harder
        //   to setup if you wanted to do something like change the
        //   decimal seperator in a double.
        //
        // I posted this on SO: https://stackoverflow.com/a/47189518/1462295

        using System.ComponentModel;

        public static Nullable<T> ToNullable<T>(this string s) where T : struct
        {
            var ret = new Nullable<T>();
            var conv = TypeDescriptor.GetConverter(typeof(T));

            if (!conv.CanConvertFrom(typeof(string)))
            {
                throw new NotSupportedException();
            }

            if (conv.IsValid(s))
            {
                ret = (T)conv.ConvertFrom(s);
            }

            return ret;
        }

        ********************************************************************** */
        
        /// <summary>
        /// Attempts to get the value associated with the specified key from the <see
        /// cref="Dictionary{TKey,TValue}"/>. If not found, action will be executed,
        /// and the dictionary will be updated with the returned value.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, <paramref name="value"/> contains the object from
        /// the
        /// <see cref="Dictionary{TKey,TValue}"/> with the specified key or the result from
        /// the action if the key was not found.</param>
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
        
        /// <summary>
        /// Splits a string and then parses each substring to int. Items that do not
        /// parse correctly are silently dropped.
        /// </summary>
        /// <param name="input">Input string to split and parse.</param>
        /// <param name="splitChar">Character used to split the string.</param>
        /// <returns>A list of ints that were successfully parsed.</returns>
        public IEnumerable<int> SplitParse(this string input, char splitChar)
        {
            var results = new List<int>();

            if (string.IsNullOrEmpty(input))
            {
                return results;
            }

            var items = input.Split(splitChar);

            foreach (var x in items)
            {
                int i;
                if (int.TryParse(x, out i))
                {
                    results.Add(i);
                }
            }

            return results.AsEnumerable();
        }
    }
}
