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
        /// Compares the n'th item of every single list in a collection and returns that value
        /// if they are all the same, or null. Do not use this method to compare lists of nulls.
        /// </summary>
        /// <typeparam name="T">Type of item in the collection of lists.</typeparam>
        /// <param name="source">Collection of lists to compare.</param>
        /// <param name="defaultValue">Value to use when the n'th item is not the same or does not exist.</param>
        /// <returns>A container list. This will have the same number of elements as the
        /// longest source list. </returns>
        /// <example>
        /// <code>
        ///     var m = ("monthber").Select(x => x).ToList();
        ///     var n = ("November").Select(x => x).ToList();
        ///     var d = ("December").Select(x => x).ToList();
        ///
        ///     var intersect = (new List<List<char>>() { m, n, d }).ListIntersect(" ");
        ///
        ///     Console.WriteLine(String.Join("", intersect));
        /// </code>
        /// Console output:
        ///      ber
        /// </example>
        public static IList<object> ListIntersect<T>(this List<List<T>> source, object defaultValue = null)
        {
            // The general outline of this function is to compare the n'th position
            // of every single list.

            // Find the shortest list from the inputs
            int minListLength = source.Min(x => x.Count());

            // Find the longest list from the inputs
            int maxListLength = source.Max(x => x.Count());
            
            // Index while traversing every single list
            int listIndex = 0;

            // Number of input lists
            int length = source.Count();

            // Result list
            List<object> results = new List<object>();

            // Look at the n'th item for every list. This only
            // needs to occur up to the shortest common list.
            for (; listIndex < minListLength; listIndex++)
            {
                // Pull out the n'th item from every list and store it here.
                List<object> toCompare = new List<object>();

                T first = default(T);
                bool firstSet = false;

                // Why is toCompare built in a for loop?
                // Well, there is a very simple select statement to build this:
                //
                //     var toCompare = source.Select(x => listIndex < x.Count() ? x[listIndex] : null);
                //
                // which results in a compile error. This is due to 'null' having an unspecified
                // type, and/or the compiler being unsure that 'T' can be coerced to a null value.
                // If this wasn't a generic function the above could be fixed with an explicit cast,
                // say, to a nullable type. See http://stackoverflow.com/a/18260915/1462295
                for (int sourceIndex = 0; sourceIndex < length; sourceIndex++)
                {
                    var list = source[sourceIndex];

                    // Pull out the n'th item, or null.
                    if (listIndex < list.Count())
                    {
                        toCompare.Add(list[listIndex]);

                        if (!firstSet)
                        {
                            firstSet = true;
                            first = list[listIndex];
                        }
                    }
                    else
                    {
                        toCompare.Add(defaultValue);
                    }
                }

                // If there's nothing to compare, just skip to the next position.
                if (!firstSet)
                {
                    results.Add(defaultValue);
                    continue;
                }

                // Look at every single n'th item. If it's null, or there's something that's different,
                // they are not all the same.
                var isDifferent = toCompare.Any(x => Object.ReferenceEquals(null, x) || !x.Equals(first));

                if (!isDifferent)
                {
                    results.Add(first);
                }
                else
                {
                    results.Add(defaultValue);
                }
            }

            // If the list lengths are not the same, fill out the remaining values with null.
            for (; listIndex < maxListLength; listIndex++)
            {
                results.Add(defaultValue);
            }

            return results;
        }
        
        /// <summary>
        /// Enumerates a collection performing an action on each item. The index of the item is 
        /// also passed as a paramter to the action.
        /// </summary>
        /// <typeparam name="T">Type of collection.</typeparam>
        /// <param name="source">Collection of objects to enumerate.</param>
        /// <param name="action">Action to perform on each object. Accepts two parameters, the object and the index.</param>
        /// <remarks>http://stackoverflow.com/a/43035/1462295</remarks>
        public static void ForEachWithIndex<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int index = 0;
            foreach (var item in source)
            {
                action(item, index);
                index++;
            }
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
