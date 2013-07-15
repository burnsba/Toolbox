using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Toolbox
{
    /// <summary>
    /// Collection of generic utility functions
    /// </summary>
    public static class Helper
    {
        #region Methods

        /// <summary>
        /// Compares the values of two objects. If either can be enumerated, the values of each are compared (recursively). Comparing nulls returns true.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool ObjectsHaveSameValues(object first, object second)
        {
            // quick nulls check
            if (object.Equals(first, null) && object.Equals(second, null))
                return true;

            if (object.Equals(first, null) || object.Equals(second, null))
                return false;

            // quick type check
            {
                Type firstType = first.GetType();
                Type secondType = second.GetType();

                if (firstType != secondType)
                    return false;
            }

            // strings are enumerable, hurrrrrrrrrrrrr
            if (first is IEnumerable)
            {
                var firstEnumerator = ((IEnumerable)first).GetEnumerator();
                var secondEnumerator = ((IEnumerable)second).GetEnumerator();

                int firstCount = 0;
                int secondCount = 0;

                bool keepGoing = true;

                // keep going until ...
                while (keepGoing)
                {
                    keepGoing = firstEnumerator.MoveNext();
                    if (keepGoing == false)
                        break;
                    firstCount++;

                    keepGoing = secondEnumerator.MoveNext();
                    if (keepGoing == false)
                        break;
                    secondCount++;

                    // if the first element is itself an enumerable, recurse!
                    if (firstEnumerator.Current is IEnumerable)
                    {
                        if (!(ObjectsHaveSameValues(firstEnumerator.Current, secondEnumerator.Current)))
                            return false;
                    }
                    // not a list of sorts, so check all the properties
                    else
                    {
                        List<PropertyInfo> firstProperties = firstEnumerator.Current.GetType().GetProperties().ToList();
                        List<PropertyInfo> secondProperties = secondEnumerator.Current.GetType().GetProperties().ToList();

                        for (int i = 0; i < firstProperties.Count; i++)
                        {
                            if (firstProperties[i].GetValue(firstEnumerator.Current) is IEnumerable)
                            {
                                // if the first property is itself an enumerable, recurse!
                                if (!(ObjectsHaveSameValues(firstProperties[i].GetValue(firstEnumerator.Current), secondProperties[i].GetValue(secondEnumerator.Current))))
                                    return false;
                            }
                            else
                            {
                                // it's true kids, sometimes 0 != 0, which is why you should always use object.Equals
                                if (!(object.Equals(firstProperties[i].GetValue(firstEnumerator.Current), secondProperties[i].GetValue(secondEnumerator.Current))))
                                    return false;
                            }
                        }

                        // if there weren't any properties, compare values, maybe this is a string
                        if (firstProperties.Count == 0)
                            if (!(object.Equals(firstEnumerator.Current, secondEnumerator.Current)))
                                return false;
                    }
                }

                if (firstCount != secondCount)
                    return false;

                // all the children checked out
                return true;
            }
            // not an enumerable, just compare
            else
                return object.Equals(first, second);
        }

        #endregion
    }
}
