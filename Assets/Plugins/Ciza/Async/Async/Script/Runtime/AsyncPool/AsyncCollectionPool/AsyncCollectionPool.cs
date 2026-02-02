using System;
using System.Collections.Generic;

namespace CizaAsync
{
	/// <summary>Base class for collection pools.</summary>
	/// <typeparam name="TCollection">Type of the pooled collection.</typeparam>
	/// <typeparam name="TItem">Type of the pooled collection items.</typeparam>
	public class AsyncCollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
	{
		internal static readonly AsyncObjectPool<TCollection> pool = new AsyncObjectPool<TCollection>(() => new TCollection(), new AsyncObjectPool<TCollection>.Options()
		{
			OnReturn = (Action<TCollection>)(c => c.Clear())
		});

		/// <summary>Rents a collection from the pool.</summary>
		public static TCollection Rent() => pool.Rent();

		/// <summary>
		/// Rents a collection from the pool and creates a disposable wrapper for auto-return.
		/// </summary>
		/// <param name="obj">The rented collection.</param>
		/// <returns>Disposable wrapper over the rented collection, which will return the collection on dispose.</returns>
		public static AsyncPooledObject<TCollection> Rent(out TCollection obj) =>
			pool.Rent(out obj);

		/// <summary>
		/// Returns specified previously rented collection back to the pool,
		/// so that it can be re-used later without allocations.
		/// </summary>
		/// <param name="obj">Collection to return.</param>
		public static void Return(TCollection obj) => pool.Return(obj);

		/// <summary>
		/// Drops the pooled collections, allowing them to be reclaimed by the garbage collector.
		/// </summary>
		public static void Drop() => pool.Drop();
	}
}