using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Toolbox
{
    public static class Extensions
    {
        /// <summary>
        /// Shorthand to determine whether an object reference is null or not. Safe to call on null objects.
        /// </summary>
        /// <param name="source">Object to compare.</param>
        /// <returns>True if the object references null, false otherwise.</returns>
        public static bool ReferenceEqualsNull(this object source)
        {
            return object.ReferenceEquals(null, source);
        }
        
        /// <summary>
        /// Filters an enumerable collection and only reutrns items that are not null.
        /// </summary>
        /// <param name="source">Collection to filter.</param>
        /// <returns>The filtered collection.</returns>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source)
        {
            return source.Where(x => !Object.ReferenceEquals(null, x));
        }
        
        /// <summary>
        /// Method to cast to a different type.
        /// </summary>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="source">Object to cast.</param>
        /// <returns>Objected casted to new type.</returns>
        public static T As<T>(this object source) where T : class
        {
            return source as T;
        }

        /// <summary>
        /// Takes an enumerable set of objects and casts each one to a new type using "as".
        /// </summary>
        /// <remarks>
        /// Used for unboxing; would fail trying to convert doubles to int.
        /// </remarks>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="source">Enumerable set of objects to cast.</param>
        /// <returns>Enumerable set of objects casted to new type.</returns>
        public static IEnumerable<T> AsList<T>(this IEnumerable<object> source) where T : class
        {
            return source.Select(x => x as T);
        }

        /// <summary>
        /// Takes an enumerable set of objects and casts each one to a new type using an explicit cast.
        /// </summary>
        /// <remarks>
        /// This could be used to truncate a list of doubles to ints.
        /// </remarks>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="source">Enumerable set of objects to cast.</param>
        /// <returns>Enumerable set of objects casted to new type.</returns>
        public static IEnumerable<T> AsListExplicit<T>(this IEnumerable<object> source)
        {
            return source.Select(x => (T)x);
        }

        /// <summary>
        /// Takes an enumerable set of objects and calls ToString on each.
        /// </summary>
        /// <param name="source">Enumerable set of objects to cast.</param>
        /// <returns>Enumerable set of objects, each cast ToString.</returns>
        public static IEnumerable<string> ToStringList(this IEnumerable<object> source)
        {
            return source.Select(x => x.ToString());
        }

        /// <summary>
        /// Takes an enumerable set of objects and calls ToString on each.
        /// </summary>
        /// <typeparam name="T">Type to cast from.</typeparam>
        /// <param name="source">Enumerable set of objects to cast.</param>
        /// <returns>Enumerable set of objects, each cast ToString.</returns>
        public static IEnumerable<string> ToStringList<T>(this IEnumerable<T> source)
        {
            return source.Select(x => x.ToString());
        }

        /// <summary>
        /// Takes an enumerable and joins the elements into a string.
        /// </summary>
        /// <param name="collection">Enumberable set of objects to join.</param>
        /// <param name="joiner">String to join elements with.</param>
        /// <returns>String containing joined elements.</returns>
        public static string JoinAsString(this IEnumerable<object> collection, string joiner = ",")
        {
            return string.Join(joiner, collection);
        }

        /// <summary>
        /// Takes an enumerable and joins the elements into a string.
        /// </summary>
        /// <param name="collection">Enumberable set of objects to join.</param>
        /// <param name="joiner">String to join elements with.</param>
        /// <returns>String containing joined elements.</returns>
        public static string JoinAsString<T>(this IEnumerable<T> collection, string joiner = ",")
        {
            return string.Join(joiner, collection);
        }
        
        /// <summary>
        /// Gets a value indicating whether this object is between the start and end objects.
        /// Comparison is inclusive.
        /// </summary>
        /// <param name="start">Start object to compare against. Expected to be same type as T, but not enforced.</param>
        /// <param name="end">End object to compare against. Expected to be same type as T, but not enforced.</param>
        /// <returns>Whether this item is between the two given items.</returns>
        public static bool Between<T>(this T self, object start, object end) where T : IComparable
        {
            if (object.ReferenceEquals(null, self))
            {
                return false;
            }
            
            if (object.ReferenceEquals(null, start))
            {
                return false;
            }
            
            if (object.ReferenceEquals(null, end))
            {
                return false;
            }
            
            return self.CompareTo(start) >= 0 && self.CompareTo(end) <= 0;
        }

        /// <summary>
        /// Call JsonConvert to deserialize a string into an object.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into.</typeparam>
        /// <param name="s">Source to deserialize.</param>
        /// <returns>Newly created object.</returns>
        public static T DeserializeAs<T>(this string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }

        /// <summary>
        /// Call JsonConvert to deserialize a string into an object, starting at a root node.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into.</typeparam>
        /// <param name="s">Source to deserialize.</param>
        /// <param name="root">Root node to deserialize from.</param>
        /// <returns>Newly created object.</returns>
        public static T DeserializeAs<T>(this string s, string root)
        {
            JToken parsed = JObject.Parse(s);
            JToken rootNode = parsed[root];

            return JsonConvert.DeserializeObject<T>(rootNode.ToString());
        }

        /// <summary>
        /// Generic method to cast an object into another object with result status.
        /// </summary>
        /// <typeparam name="T">Type of object to cast into.</typeparam>
        /// <param name="source">Source object to cast.</param>
        /// <param name="res">Resulting object, if cast was successful.</param>
        /// <returns>True if the cast results in a non-null object, false otherwise.</returns>
        public static bool TryAs<T>(this object source, ref T res) where T : class
        {
            if (object.ReferenceEquals(null, source))
            {
                return false;
            }

            T casted = source as T;

            if (object.ReferenceEquals(null, casted))
            {
                return false;
            }

            res = casted;
            return true;
        }

        /// <summary>
        /// Cast an object to string, then parse into an int with result status.
        /// <param name="source">Source object to cast.</param>
        /// <param name="res">Resulting int, if cast was successful.</param>
        /// <returns>True if TryParse was successful, false otherwise.</returns>
        public static bool TryAs(this object source, ref int res)
        {
            if (object.ReferenceEquals(null, source))
            {
                return false;
            }

            int i = 0;
            if (int.TryParse(source.ToString(), out i))
            {
                res = i;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Cast an object to string, then parse into an double with result status.
        /// <param name="source">Source object to cast.</param>
        /// <param name="res">Resulting double, if cast was successful.</param>
        /// <returns>True if TryParse was successful, false otherwise.</returns>
        public static bool TryAs(this object source, ref double res)
        {
            if (object.ReferenceEquals(null, source))
            {
                return false;
            }

            double d = 0;
            if (double.TryParse(source.ToString(), out d))
            {
                res = d;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Cast an object to string, then parse into an DateTime with result status.
        /// <param name="source">Source object to cast.</param>
        /// <param name="res">Resulting DateTimes, if cast was successful.</param>
        /// <returns>True if TryParse was successful, false otherwise.</returns>
        public static bool TryAs(this object source, ref DateTime res)
        {
            if (object.ReferenceEquals(null, source))
            {
                return false;
            }

            DateTime d = DateTime.MinValue;
            if (DateTime.TryParse(source.ToString(), out d))
            {
                res = d;
                return true;
            }

            return false;
        }
    }
}
