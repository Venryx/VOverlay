using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

public static class ProtoBufNetExtensions
{
	// Array
	public static void MultiLoop(this Array s, Action<int[]> action) { s.RecursiveLoop(0, new int[s.Rank], action); }
	static void RecursiveLoop(this Array s, int level, int[] indices, Action<int[]> action)
	{
		if (level == s.Rank)
			action(indices);
		else
			for (indices[level] = 0; indices[level] < s.GetLength(level); indices[level]++)
				RecursiveLoop(s, level + 1, indices, action);
	}

	// for Array methods
	static ProtoMultiArray<T> ToProtoMultiArray_Base<T>(Array s)
	{
		// copy dimensions (to be used for reconstruction)
		var dimensions = new int[s.Rank];
		for (int i = 0; i < s.Rank; i++)
			dimensions[i] = s.GetLength(i);
		// copy the underlying data
		var data = new T[s.Length];
		var k = 0;
		s.MultiLoop(indices => data[k++] = (T)s.GetValue(indices));

		return new ProtoMultiArray<T> { dimensions = dimensions, data = data };
	}

	// T[,]
	public static ProtoMultiArray<T> ToProtoMultiArray<T>(this T[,] s) { return ToProtoMultiArray_Base<T>(s); }

	// ProtoMultiArray
	/*public static Array ToArray<T>(this ProtoMultiArray<T> s)
    {
        // initialize array dynamically
        var result = Array.CreateInstance(typeof(T), s.dimensions);
        // copy the underlying data
		var k = 0;
		result.MultiLoop(indices=>result.SetValue(s.data[k++], indices));

		return result;
    }*/
	public static T[,] ToMultiArray_2<T>(this ProtoMultiArray<T> s)
	{
		var result = new T[s.dimensions[0], s.dimensions[1]];
		var k = 0;
		result.MultiLoop(indices=>result.SetValue(s.data[k++], indices));
		return result;
	}
}

[ProtoContract] public class ProtoMultiArray<T>
{
	[ProtoMember(1)] public int[] dimensions;
	[ProtoMember(2)] public T[] data;
}