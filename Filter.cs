//   Copyright 2013 Benjamin Burns
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.


// Class to do simple (string) comparison of properties. Properties can
// have sub-properties, and comparisons can be strung together, sort of.
// There's not really an order of operation, phrases are just parsed
// left to right.
// 
// The idea is to make a filter, and then test an object -- pass it through
// the filter. True or false will be returned depending on whether the object
// had matching properties and values.
//
// There are several different methods of value comparison. 
// - Exactly match all property value(s)
// - Exactly match any property value(s)
// - Property contains a substring of all property value(s)
// - Property contains a substring of at least one of the property value(s)
//
/// <example>
///             FilterClause clause = new FilterClause();
///             
///             FilterNode authorNode = new FilterNode(clause);
///             authorNode.PropertyName = "Author";
///             
///             FilterValuePair agencyValue = new FilterValuePair(authorNode);
///             agencyValue.Comparer = FilterValueComparer.ExactlyMatchAll;
///             agencyValue.PropertyName = "Name";
///             
///             agencyValue.AddValue("Frank");
///             
///             authorNode.SetChild(agencyValue);
///             
///             FilterValuePair bookValue = new FilterValuePair(clause);
///             bookValue.Comparer = FilterValueComparer.ExactlyMatchAll;
///             bookValue.PropertyName = "Books";
///             bookValue.AddValues("Gone With the Wind", "Back With the Tide");
///             
///             clause.Left = authorNode;
///             clause.Operator = FilterOperator.Intersect;
///             clause.Right = bookValue;
///             
///             bool b = clause.Test(obj);
///             
///             // will compare object obj to a filter such that
///             // a property "Author" has a property "Name" which has a value "Frank"
///             // AND
///             // the same object has a property "Books" containing both values "Gone With the Wind" and "Back With the Tide"
///
/// </example>

// Ben Burns
// July 26, 2013

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;

namespace Toolbox.Filter
{
    /// <summary>
    /// How to compare a property to a value or values
    /// </summary>
    public enum FilterValueComparer
    {
        Unknown,

        /// <summary>
        /// Contains at least one of the specified values, exactly.
        /// </summary>
        ExactlyMatchAny,

        /// <summary>
        /// Contains all of the specified values, but not necessarily in the same order. The values must match exactly.
        /// </summary>
        ExactlyMatchAll,

        /// <summary>
        /// Text compare looking to see if at least one of the values is a sub-string.
        /// </summary>
        ContainsAny,

        /// <summary>
        /// Text comparing looking to see if all of the values are found as a sub-string.
        /// </summary>
        ContainsAll,
    }

    /// <summary>
    /// How to apply multiple filtering criteria
    /// </summary>
    public enum FilterOperator
    {
        Unknown,

        /// <summary>
        /// Union. Similar to logical OR
        /// </summary>
        Union,

        /// <summary>
        /// Intersect. Similar to logical AND
        /// </summary>
        Intersect

        // not supported:
        /* XOR */
        /* NOT */
    }

    /// <summary>
    /// Helper to convert the enums to strings
    /// </summary>
    public static class FilterEnumExtensions
    {
        /// <summary>
        /// Converts the enum operators into "logical" operators
        /// </summary>
        /// <param name="fo"></param>
        /// <returns></returns>
        public static string ToShortString(this FilterOperator fo)
        {
            switch (fo)
            {
                case FilterOperator.Intersect:
                    return "&&";
                case FilterOperator.Union:
                    return "||";
                default:
                    return "unknown";
            }
        }

        /// <summary>
        /// Converts the enum comparison descriptor into text
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        public static string ToShortString(this FilterValueComparer fc)
        {
            switch (fc)
            {
                case FilterValueComparer.ExactlyMatchAny:
                    return "exactly at least one of";
                case FilterValueComparer.ExactlyMatchAll:
                    return "==";
                case FilterValueComparer.ContainsAny:
                    return "contains at least one sub-string";
                case FilterValueComparer.ContainsAll:
                    return "contains all sub-strings";
                default:
                    return "unknown";
            }
        }
    }

    /// <summary>
    /// Abstract base class
    /// </summary>
    public abstract class FilterGrammarBase
    {
        #region Fields

        /// <summary>
        /// Describes what type of structure the object is, related to the syntax.
        /// </summary>
        /// <remarks>
        /// Similar to "noun, verb, adjective, adverb, pronoun" etc
        /// </remarks>
        private readonly FilterGrammar grammarObject;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="grammarType"></param>
        protected FilterGrammarBase(FilterGrammar grammarType)
        {
            this.grammarObject = grammarType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the parent of the current node
        /// </summary>
        public FilterGrammarBase Parent 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// Returns whether the node has a parent
        /// </summary>
        public bool HasParent
        {
            get
            {
                return this.Parent != null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns whether the part is well-formed
        /// </summary>
        /// <returns></returns>
        public abstract bool IsValid();

        /// <summary>
        /// Test whether the filter matches a given object
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public abstract bool Test(object o, bool ignoreCase = true);

        /// <summary>
        /// Throws specific exceptions depending on validation errors
        /// </summary>
        public abstract void Validate();

        /// <summary>
        /// Quickly determines type
        /// </summary>
        /// <remarks>
        /// Without using reflection
        /// </remarks>
        /// <returns></returns>
        public bool IsValuePair()
        {
            return (this.grammarObject == FilterGrammar.Value);
        }

        /// <summary>
        /// Quickly determines type
        /// </summary>
        /// <<remarks>
        /// Without using reflection
        /// </remarks>
        /// <returns></returns>
        public bool IsClause()
        {
            return (this.grammarObject == FilterGrammar.Clause);
        }

        /// <summary>
        /// Quickly determines type
        /// </summary>
        /// <remarks>
        /// Without using reflection
        /// </remarks>
        /// <returns></returns>
        public bool IsTreeBranch()
        {
            return (this.grammarObject == FilterGrammar.TreeBranch);
        }

        /// <summary>
        /// Helper function. 1) Controls what can access setting the parent. 2) avoids null checks everywhere
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        protected static void SetParent(FilterGrammarBase node, FilterGrammarBase parent)
        {
            if (node == null)
                return;

            // nothing to do if the parent is already the parent
            if (Object.ReferenceEquals(node.Parent, parent))
                return;

            // make sure the parent node isn't already in the tree
            if (FilterGrammarBase.IsAncestor(node, parent))
                throw new RecursiveTree("Can not attache parent node, infinite loop detected");
            if (FilterGrammarBase.IsAncestor(parent, node))
                throw new RecursiveTree("Can not attache parent node, infinite loop detected");

            node.Parent = parent;
        }

        /// <summary>
        /// Need a way to check for infinite loops.
        /// </summary>
        /// <param name="node">Base node</param>
        /// <param name="ancestorCheck">Node to search for in relation to the base node</param>
        protected static bool IsAncestor(FilterGrammarBase node, FilterGrammarBase ancestorCheck)
        {
            if (node == null || ancestorCheck == null)
                return false;

            FilterGrammarBase p = node.Parent as FilterGrammarBase;
            while (p != null)
            {
                if (Object.ReferenceEquals(p, ancestorCheck))
                    return true;

                p = p.Parent;
            }

            return false;
        }

        #endregion

        #region Enums

        /// <summary>
        /// Type of structure within the filter sentence
        /// </summary>
        protected enum FilterGrammar
        {
            Unknown,

            Value,
            Clause,
            TreeBranch
        }
        
        #endregion
    }

    /// <summary>
    /// Abstract base class for nodes
    /// </summary>
    public abstract class FilterNodeBase : FilterGrammarBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fg"></param>
        protected FilterNodeBase(FilterGrammar fg)
            : base(fg)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of property to match
        /// </summary>
        public string PropertyName { get; set; }
        
        #endregion

        #region Methods

        /// <summary>
        /// Test whether the filter matches a given object
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public abstract override bool Test(object o, bool ignoreCase);

        /// <summary>
        /// Returns whether the part is well-formed
        /// </summary>
        /// <returns></returns>
        public abstract override bool IsValid();

        /// <summary>
        /// Throws specific exceptions depending on validation errors
        /// </summary>
        public abstract override void Validate();

        #endregion
    }

    /// <summary>
    /// A basic node. Can have a child of either another node, 
    /// or a value pair
    /// </summary>
    public class FilterNode : FilterNodeBase
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public FilterNode(FilterGrammarBase parent = null)
            : base(FilterGrammar.TreeBranch)
        {
            FilterGrammarBase.SetParent(this, parent);
            SetChild(null);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the child of the current node
        /// </summary>
        public FilterNodeBase Child
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns whether the node has a child
        /// </summary>
        public bool HasChild
        {
            get
            {
                return this.Child != null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns whether the part is well-formed
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            if (String.IsNullOrEmpty(this.PropertyName))
                return false;

            return this.HasChild;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";

            if (string.IsNullOrEmpty(this.PropertyName))
                result += "(Propertyname empty)";
            else
                result += this.PropertyName;

            if (this.HasChild)
            {
                result += " > " + this.Child.ToString();
            }

            return result;
        }

        /// <summary>
        /// Sets the child node
        /// </summary>
        /// <param name="c"></param>
        public void SetChild(FilterNodeBase c)
        {
            if (this.Child == c)
                return;

            // otherwise, unlink existing child
            FilterNode currentChild = this.Child as FilterNode;
            if (currentChild != null)
                this.RemoveChild();

            this.Child = c;
            FilterGrammarBase.SetParent(this.Child, this);
        }

        /// <summary>
        /// Test whether the filter matches a given object
        /// </summary>
        /// <param name="o">Object to apply property/values to</param>
        /// <param name="ignoreCase">Wether to ignore case in string compare</param>
        /// <returns></returns>
        public override bool Test(object o, bool ignoreCase = true)
        {
            if (Object.Equals(o, null))
                throw new NullReferenceException("Object to match against must not be null.");
            if (!this.IsValid())
                this.Validate();

            PropertyInfo p = o.GetType().GetProperty(this.PropertyName);
            if (p == null)
                return false;

            var v = p.GetValue(o, null);

            return this.Child.Test(v, ignoreCase);
        }

        /// <summary>
        /// Helper function to elucidate the exact nature of validation failure
        /// </summary>
        public override void Validate()
        {
            if (String.IsNullOrEmpty(this.PropertyName))
                throw new MissingProperty("The filter tree node does not have a property to compare against");

            if (!this.HasChild)
                throw new MissingChild("The filter tree node does not have a child/values");
        }

        /// <summary>
        /// Recursively removes child nodes
        /// </summary>
        private void RemoveChild()
        {
            if (!this.HasChild)
                return;
            FilterNode c = this.Child as FilterNode;
            if (c == null)
                return;
            c.RemoveChild();

            c = null;
        }

        #endregion
    }

    /// <summary>
    /// Contains values associated with a property
    /// </summary>
    public class FilterValuePair : FilterNodeBase
    {
        #region Fields

        /// <summary>
        /// Value or values of property to match
        /// </summary>
        private List<string> propertyValues;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public FilterValuePair(FilterGrammarBase parent = null)
            : base(FilterGrammar.Value)
        {
            FilterGrammarBase.SetParent(this, parent);
            this.propertyValues = new List<string>();
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Value or values of property to match
        /// </summary>
        public ReadOnlyCollection<string> PropertyValues
        {
            get
            {
                return this.propertyValues.AsReadOnly();
            }
        }

        /// <summary>
        /// How to compare values to the property
        /// </summary>
        public FilterValueComparer Comparer { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns whether the part is well-formed
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            if (String.IsNullOrEmpty(this.PropertyName))
                return false;

            if (this.propertyValues == null)
                return false;
            if (this.propertyValues.Count < 1)
                return false;
            if (this.Comparer == FilterValueComparer.Unknown)
                return false;

            return true;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";

            if (string.IsNullOrEmpty(this.PropertyName))
                result += "(Propertyname empty)";
            else
                result += this.PropertyName;

            result += " " + this.Comparer.ToShortString() + " ";

            if (this.propertyValues.Count > 1)
                result += "(";
            else
                result += "\"";

            result += String.Join(",", this.propertyValues);

            if (this.propertyValues.Count > 1)
                result += ")";
            else
                result += "\"";

            return result;
        }

        /// <summary>
        /// Shorthand to add value
        /// </summary>
        /// <param name="s"></param>
        public void AddValue(string s)
        {
            if (!String.IsNullOrEmpty(s))
                this.propertyValues.Add(s);
        }

        /// <summary>
        /// Shorthand to add values
        /// </summary>
        /// <param name="ls"></param>
        public void AddValues(List<String> ls)
        {
            if (ls == null)
                return;
            if (ls.Count < 1)
                return;
            foreach (string s in ls)
                if (!String.IsNullOrEmpty(s))
                    this.propertyValues.Add(s);
        }

        /// <summary>
        /// Shorthand to add values
        /// </summary>
        /// <param name="ls"></param>
        public void AddValues(params string[] ls)
        {
            if (ls == null)
                return;
            if (ls.Length < 1)
                return;
            foreach (string s in ls)
                if (!String.IsNullOrEmpty(s))
                    this.propertyValues.Add(s);
        }

        /// <summary>
        /// Test whether the filter matches a given object
        /// </summary>
        /// <param name="o">Object to apply property/values to</param>
        /// <param name="ignoreCase">Wether to ignore case in string compare</param>
        /// <returns></returns>
        public override bool Test(object o, bool ignoreCase = true)
        {
            if (Object.Equals(o, null))
                throw new NullReferenceException("Object to match against must not be null.");
            if (!this.IsValid())
                this.Validate();

            PropertyInfo p = o.GetType().GetProperty(this.PropertyName);
            if (p == null)
                return false;

            var v = p.GetValue(o, null);

            switch (this.Comparer)
            {
                case FilterValueComparer.ExactlyMatchAny:
                    return ContainsAnyExact(v);
                case FilterValueComparer.ExactlyMatchAll:
                    return ContainsAllExact(v);
                case FilterValueComparer.ContainsAny:
                    return ContainsAnySubString(v);
                case FilterValueComparer.ContainsAll:
                    return ContainsAllSubString(v);
                case FilterValueComparer.Unknown: /* fall through */
                default:
                    return false;
            }
        }

        /// <summary>
        /// Helper function to elucidate the exact nature of validation failure
        /// </summary>
        public override void Validate()
        {
            if (String.IsNullOrEmpty(this.PropertyName))
                throw new MissingProperty("The value pair does not have a property to compare against");

            if (this.propertyValues == null)
                throw new MissingValue("The value pair does not have any values");
            if (this.propertyValues.Count < 1)
                throw new MissingValue("The value pair does not have any values");

            if (this.Comparer == FilterValueComparer.Unknown)
                throw new BadOperator("The value pair operator is invalid: " + this.Comparer.ToString());
        }

        /// <summary>
        /// Helper function. Searches property values for any values, exactly.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool ContainsAnyExact(object o, bool ignoreCase = true)
        {
            if (Object.Equals(o, null))
                return false;

            if (o is IEnumerable && !(o is String))
            {
                var en = ((IEnumerable)o).GetEnumerator();
                while (en.MoveNext())
                {
                    foreach (string s in this.propertyValues)
                    {
                        // just need to find one value ...
                        if (String.Compare(en.Current.ToString(), s, ignoreCase) == 0)
                            return true;
                    }
                }
                return false;
            }
            // could be a string
            else
            {
                foreach (string s in this.propertyValues)
                {
                    // just need to find one value ...
                    if (String.Compare(o.ToString(), s, ignoreCase) == 0)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Helper function. Searches property values to match all values, exactly.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        private bool ContainsAllExact(object o, bool ignoreCase = true)
        {
            if (Object.Equals(o, null))
                return false;

            if (o is IEnumerable && !(o is String))
            {
                var en = ((IEnumerable)o).GetEnumerator();

                // copy list of values
                List<string> itemsLeftToMatch = new List<string>(this.propertyValues);

                while (en.MoveNext())
                {
                    foreach (string s in itemsLeftToMatch)
                    {
                        if (String.Compare(en.Current.ToString(), s, ignoreCase) == 0)
                        {
                            // everytime a value matches, remove it from the list
                            itemsLeftToMatch.Remove(en.Current.ToString());
                            break;
                        }
                    }
                }
                // as long as all the values were found, it's a match
                if (itemsLeftToMatch.Count > 0)
                    return false;
                return true;
            }
            // could be a string.
            else
            {
                // should have at least one property value set ...
                foreach (string s in this.propertyValues)
                {
                    // if it's not found, fail
                    if (String.Compare(o.ToString(), s, ignoreCase) != 0)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Helper function. Searches property values for sub-strings of any values.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        private bool ContainsAnySubString(object o, bool ignoreCase = true)
        {
            if (Object.Equals(o, null))
                return false;

            if (o is IEnumerable && !(o is String))
            {
                var en = ((IEnumerable)o).GetEnumerator();
                while (en.MoveNext())
                {
                    foreach (string s in this.propertyValues)
                    {
                        // just need to find one value ...
                        if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(en.Current.ToString(), s, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) > -1)
                            return true;
                    }
                }
                return false;
            }
            // could be a string
            else
            {
                foreach (string s in this.propertyValues)
                {
                    // just need to find one value ...
                    if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(o.ToString(), s, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) > -1)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Helper function. Searches property for sub-strings of all values.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        private bool ContainsAllSubString(object o, bool ignoreCase = true)
        {
            if (Object.Equals(o, null))
                return false;

            if (o is IEnumerable && !(o is String))
            {
                var en = ((IEnumerable)o).GetEnumerator();

                // copy list of values
                List<string> itemsLeftToMatch = new List<string>(this.propertyValues);

                while (en.MoveNext())
                {
                    foreach (string s in itemsLeftToMatch)
                    {
                        if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(o.ToString(), s, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) > -1)
                        {
                            // everytime a value matches, remove it from the list
                            itemsLeftToMatch.Remove(en.Current.ToString());
                            break;
                        }
                    }
                }
                // as long as all the values were found, it's a match
                if (itemsLeftToMatch.Count > 0)
                    return false;
                return true;
            }
            // could be a string.
            else
            {
                // should have at least one property value set ...
                foreach (string s in this.propertyValues)
                {
                    // if it's not found, fail
                    if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(o.ToString(), s, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) == -1)
                        return false;
                }
                return true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Basic filter sentence is built with this. Can contain members of the same type.
    /// </summary>
    public class FilterClause : FilterGrammarBase
    {
        #region Fields

        private FilterGrammarBase left;
        private FilterGrammarBase right;

        #endregion

        #region Constructors

        public FilterClause()
            : base(FilterGrammar.Clause)
        {
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Left hand side. Required
        /// </summary>
        public FilterGrammarBase Left
        {
            get
            {
                return this.left;
            }
            set
            {
                // unlink old parent, then set the new one
                FilterGrammarBase.SetParent(this.left, null);
                this.left = value;
                FilterGrammarBase.SetParent(this.left, this);
            }
        }

        /// <summary>
        /// Operator to relate left and right hand side. Only required
        /// if right hand side is set.
        /// </summary>
        public FilterOperator Operator
        { 
            get; 
            set;
        }

        /// <summary>
        /// Right hand side. Related to left hand side via operator.
        /// </summary>
        public FilterGrammarBase Right
        {
            get
            {
                return this.right;
            }
            set
            {
                // unlink old parent, then set the new one
                FilterGrammarBase.SetParent(this.right, null);
                this.right = value;
                FilterGrammarBase.SetParent(this.right, this);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns whether the part is well-formed
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            if (this.Left == null)
                return false;
            if (this.Left.IsClause())
                if (this.Left.IsValid() == false)
                    return false;

            // don't need an operator unless there's a right hand side
            if (this.Right != null)
            {
                if (this.Operator == FilterOperator.Unknown)
                    return false;
            }

            // don't need a right hand side unless there's an operator
            if (this.Operator != FilterOperator.Unknown)
            {
                if (this.Right == null)
                    return false;
                if (this.Right.IsClause())
                    if (this.Right.IsValid() == false)
                        return false;
            }

            return true;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";
            if (!this.IsValid())
                return result;

            result += this.Left.ToString();

            if (this.Right != null)
            {
                result += " " + this.Operator.ToShortString() + " ";

                result += this.Right.ToString();
            }

            return result;
        }

        /// <summary>
        /// Test whether the filter matches a given object
        /// </summary>
        /// <param name="o">Object to apply property/values to</param>
        /// <param name="ignoreCase">Wether to ignore case in string compare</param>
        /// <returns></returns>
        public override bool Test(object o, bool ignoreCase = true)
        {
            if (Object.Equals(o, null))
                throw new NullReferenceException("Object to match against must not be null.");
            if (!this.IsValid())
                this.Validate();

            // check if there's only a left hand side...
            if (this.Right == null)
            {
                return this.Left.Test(o, ignoreCase);
            }

            // else, there's a right hand side as well
            switch (this.Operator)
            {
                case FilterOperator.Intersect:
                    return this.Left.Test(o, ignoreCase) && this.Right.Test(o, ignoreCase);
                case FilterOperator.Union:
                    return this.Left.Test(o, ignoreCase) || this.Right.Test(o, ignoreCase);

                case FilterOperator.Unknown: /* fall through */
                default:
                    throw new ArgumentException("Unsupported operator: " + this.Operator.ToString());
            }
        }

        /// <summary>
        /// Helper function to elucidate the exact nature of validation failure
        /// </summary>
        public override void Validate()
        {
            if (this.Left == null)
                throw new MissingChild("The clause is missing the left side phrase");
            if (this.Left.IsValid() == false)
                this.Left.Validate();
            

            // don't need an operator unless there's a right hand side
            if (this.Right != null)
            {
                if (this.Operator == FilterOperator.Unknown)
                    throw new BadOperator("The clause operator is invalid");
            }

            // don't need a right hand side unless there's an operator
            if (this.Operator != FilterOperator.Unknown)
            {
                if (this.Right == null)
                    throw new MissingChild("The operator is set, but the clause is missing the right side phrase");
                if (this.Right.IsValid() == false)
                    this.Right.Validate();
            }
        }

        #endregion
    }

    /// <summary>
    /// Generic filter grammar exception
    /// </summary>
    [Serializable]
    public class MalFormedExpression : Exception
    {
        public MalFormedExpression()
        { }

        public MalFormedExpression(string message)
            : base(message)
        { }

        public MalFormedExpression(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Exception for missing child node
    /// </summary>
    [Serializable]
    public class MissingChild : MalFormedExpression
    {
        public MissingChild()
        { }

        public MissingChild(string message)
            : base(message)
        { }

        public MissingChild(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Exception for bad property name
    /// </summary>
    [Serializable]
    public class MissingProperty : MalFormedExpression
    {
        public MissingProperty()
        { }

        public MissingProperty(string message)
            : base(message)
        { }

        public MissingProperty(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Exception for lack of values
    /// </summary>
    [Serializable]
    public class MissingValue : MalFormedExpression
    {
        public MissingValue()
        { }

        public MissingValue(string message)
            : base(message)
        { }

        public MissingValue(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Exception for operator
    /// </summary>
    [Serializable]
    public class BadOperator : MalFormedExpression
    {
        public BadOperator()
        { }

        public BadOperator(string message)
            : base(message)
        { }

        public BadOperator(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Exception for family tree
    /// </summary>
    [Serializable]
    public class RecursiveTree : MalFormedExpression
    {
        public RecursiveTree()
        { }

        public RecursiveTree(string message)
            : base(message)
        { }

        public RecursiveTree(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
