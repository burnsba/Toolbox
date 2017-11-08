using System.Globalization;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

// 2017-11-08 
// Not sure how I feel about the current Extension class, starting a new file.
// Change name from Toolbox to ToolBox

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

    }

}
