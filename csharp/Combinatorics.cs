public static int GetMaxState(int[] arr, int upto)
{
    int len = arr.Length;
    int max = 0;
    for (int i=0; i<len && i<upto; i++)
    {
        max = arr[i] > max ? arr[i] : max;
    }
    
    return max;
}

public static bool IncrementState(ref int[] arr)
{
    int max;
    int maxNext;
    int len = arr.Length;
    var copy = new int[len];
    
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

public static IEnumerable<IEnumerable<List<T>>> GetPartitions<T>(List<T> source)
{
    var len = source.Count;
    var state = new int[len];
    bool canIterate = true;

    do {
        var partitionMeta = state.Select((x,i) => Tuple.Create(x,i)).ToList();
        
        var dict = new Dictionary<int, List<T>>();
        
        for (int i=0; i<len; i++)
        {
            var t = partitionMeta[i];
            List<T> listy = null;
            
            if (!dict.TryGetValue(t.Item1, out listy))
            {
                listy = new List<T>();
                dict[t.Item1] = listy;
            }
            
            listy.Add(source[t.Item2]);
        }
        
        yield return dict.Select(x => x.Value);
        
        canIterate = IncrementState(ref state);
        
    } while(canIterate);
}

public static void FlatPrint<T>(IEnumerable<List<T>> lists)
{
    var sets = new List<string>();
    
    foreach (var list in lists)
    {
        sets.Add($"{{{String.Join(",", list)}}}");
    }
    
    Console.WriteLine(String.Join(" ", sets));
}

// foreach (IEnumerable<List<int>> p in GetPartitions(new List<int>() { 1, 2, 3 })) { FlatPrint<int>(p); }
