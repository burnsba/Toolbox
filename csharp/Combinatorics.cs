/// <summary>
/// Helper function for GetPartitions.
/// </summary>
/// <remarks>
/// Helper function for GetPartitions
/// </remarks>
private static int GetMaxState(int[] arr, int upto)
{
    // This is faster than using linq:
    // return arr.Take(upto).Max();
    
    int len = arr.Length;
    int max = 0;
    for (int i=0; i<len && i<upto; i++)
    {
        max = arr[i] > max ? arr[i] : max;
    }
    
    return max;
}

/// <summary>
/// Helper function for GetPartitions.
/// </summary>
/// <remarks>
/// Helper function for GetPartitions
/// </remarks>
private static bool IncrementState(ref int[] arr)
{
    int max;
    int maxNext;
    int len = arr.Length;
    var copy = new int[len];
    
    // This algorithm trickles down the carry, but in case of "overflow"
    // (last iteration) work with a copy to preserve original contents.
    Array.Copy(arr, copy, len);

    for (int i=len - 1; i > 0; i--)
    {
        max = GetMaxState(copy, i);
        maxNext = max + 1;
        
        if (copy[i] < maxNext)
        {
            copy[i]++;
            Array.Copy(copy, arr, len);
            return true;
        }
        else
        {
            copy[i] = 0;
        }
    }
    
    return false;
}

/// <summary>
/// Enumerates all possible partitions of a list.
/// </summary>
public static IEnumerable<IEnumerable<List<T>>> GetPartitions<T>(List<T> source)
{
    // This function enumerates partitions then maps that back to the source.
    //
    // For example, a three element set can be divided into the following partitions:
    // 000
    // 001
    // 010
    // 011
    // 012
    
    // state is the array that maps elements to partitions.
    var len = source.Count;
    var state = new int[len];
    bool canIterate = true;

    do {
        // Create tuples of (partition, source element index)
        var partitionMeta = state.Select((x,i) => Tuple.Create(x,i)).ToList();
        
        var dict = new Dictionary<int, List<T>>();
        
        for (int i=0; i<len; i++)
        {
            var t = partitionMeta[i];
            List<T> listy = null;
            
            // Get the list for this partition.
            if (!dict.TryGetValue(t.Item1, out listy))
            {
                listy = new List<T>();
                dict[t.Item1] = listy;
            }
            
            // Add source element to the list for this partition.
            listy.Add(source[t.Item2]);
        }
        
        // The source collection is now divided into lists, one for each partition.
        // Return all these lists.
        yield return dict.Select(x => x.Value);

        canIterate = IncrementState(ref state);
    } while(canIterate);
}

/// <summary>
/// Friendly output for lists of lists.
/// </summary>
/// <example>
/// FlatPrint(new List<List<int>>() { new List<int>() { 1, 2, 3 }, new List<int>() { 4, 5, 6 } })
/// {1,2,3} {4,5,6}
/// </exmaple>
public static void FlatPrint<T>(IEnumerable<List<T>> lists)
{
    var sets = new List<string>();
    
    foreach (var list in lists)
    {
        sets.Add($"{{{String.Join(",", list)}}}");
    }
    
    Console.WriteLine(String.Join(" ", sets));
}

/// <summary>
/// Enumerate all combinations of source collection.
/// </summary>
public static IEnumerable<List<T>> Combinations<T>(List<T> source)
{
    bool canIterate = !(Object.ReferenceEquals(null, source) || source.Count < 1);
    int len = canIterate ? source.Count : 0;
    var state = new int[len];
    
    while(canIterate)
    {
        bool inc = false;
        var ret = new List<T>();
        
        for (int i=len - 1; i>=0; i--)
        {
            if (state[i] == 0)
            {
                state[i]++;
                inc = true;
                break;
            }
            else
            {
                state[i] = 0;
            }
        }
        
        if (inc == false)
        {
            canIterate = false;
            break;
        }
        
        for (int i=0; i<len; i++)
        {
            if (state[i] == 1)
            {
                ret.Add(source[i]);
            }
        }
        
        yield return ret;
    }
}

/// <summary>
/// Treats a string as a collection and enumerates combinations of elements.
/// Adjacent elements are grouped together and returned as one.
/// </summary>
/// <example>
/// FlatPrint(AdjacentCombinations("123"))
///     {3} {2} {23} {1} {1,3} {12} {123}
/// </example>
public static IEnumerable<List<string>> AdjacentCombinations(string source)
{
    bool canIterate = !(string.IsNullOrEmpty(source) || source.Length < 1);
    int len = canIterate ? source.Length : 0;
    var state = new int[len];
    
    while(canIterate)
    {
        bool inc = false;
        var ret = new List<string>();
        
        for (int i=len - 1; i>=0; i--)
        {
            if (state[i] == 0)
            {
                state[i]++;
                inc = true;
                break;
            }
            else
            {
                state[i] = 0;
            }
        }
        
        if (inc == false)
        {
            canIterate = false;
            break;
        }
        
        int startRun = -1;
        int runLen = 0;
        
        for (int i=0; i<len; i++)
        {
            if (state[i] == 1)
            {
                if (startRun < 0)
                {
                    startRun = i;
                }
                
                runLen++;
            }
            else
            {
                if (startRun >= 0)
                {
                    ret.Add(source.Substring(startRun, runLen));
                    startRun = -1;
                    runLen = 0;
                }
            }
        }
        
        if (startRun >= 0)
        {
            ret.Add(source.Substring(startRun, runLen));
            startRun = -1;
            runLen = 0;
        }
        
        yield return ret;
    }
}
