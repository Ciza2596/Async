using System.Collections.Generic;

namespace CizaAsync
{
	/// <summary>
	/// Collection pool over <see cref="T:System.Collections.Generic.List`1" /> objects.
	/// </summary>
	/// <typeparam name="T">Type of the pooled list items.</typeparam>
	public class AsyncListPool<T> : AsyncCollectionPool<List<T>, T> { }
}