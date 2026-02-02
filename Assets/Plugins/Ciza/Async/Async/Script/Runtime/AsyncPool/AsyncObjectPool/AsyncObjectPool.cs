#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CizaAsync
{
	/// <summary>
	/// Controls behaviour of the <see cref="AsyncObjectPool{T}" /> instances.
	/// </summary>
	public class AsyncObjectPool
	{
		/// <summary>
		/// Whether the pool is active, ie reuses pooled objects and invokes specified
		/// rent, return and drop hooks on the pooled objects.
		/// </summary>
		/// <remarks>
		/// Use to globally disable the pooling behaviour for edge cases, such as unit tests,
		/// where MOQ verification is not possible when the objects are pooled.
		/// </remarks>
		public static bool PoolingEnabled { get; set; } = true;
	}

	/// <summary>
	/// Allows re-using object instances to limit heap allocations.
	/// </summary>
	/// <typeparam name="T">Type of the pooled objects.</typeparam>
	public sealed class AsyncObjectPool<T> : AsyncObjectPool, IDisposable where T : class
	{
		private readonly Stack<T> _pool;
		private readonly Func<T> _factory;
		private readonly Options _options;

		private T? lastReturned;

		/// <summary>The total number of objects managed by the pool.</summary>
		public int Total { get; private set; }

		/// <summary>Number of rented objects, ie rented and not returned.</summary>
		public int Rented => Total - Available;

		/// <summary>
		/// Number of "free" objects, ie rented and returned, available for rent w/o allocation.
		/// </summary>
		public int Available => _pool.Count + (lastReturned != null ? 1 : 0);

		/// <param name="factory">Factory function to create pooled objects.</param>
		/// <param name="options">Options to configure the pool behaviour.</param>
		public AsyncObjectPool(Func<T> factory, Options? options = null)
		{
			_factory = factory;
			_options = options ?? new Options();
			_pool = new Stack<T>(_options.MinSize);
		}

		/// <summary>Rents an object from the pool.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Rent()
		{
			if (!PoolingEnabled)
				return _factory();
			T obj;
			if (lastReturned != null)
			{
				obj = lastReturned;
				lastReturned = default(T);
			}
			else if (_pool.Count == 0)
			{
				obj = _factory();
				++Total;
			}
			else
				obj = _pool.Pop();

			var onRent = _options.OnRent;
			if (onRent != null)
				onRent(obj);
			return obj;
		}

		/// <summary>
		/// Rents an object from the pool and creates a disposable wrapper for auto-return.
		/// </summary>
		/// <param name="obj">The rented object.</param>
		/// <returns>Disposable wrapper over the rented object, which will return the object on dispose.</returns>
		public AsyncPooledObject<T> Rent(out T obj) => new AsyncPooledObject<T>(obj = Rent(), this);

		/// <summary>
		/// Returns specified previously rented object back to the pool,
		/// so that it can be re-used later without allocations.
		/// </summary>
		/// <param name="obj">Object to return.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Return(T obj)
		{
			if (!PoolingEnabled)
				return;
			Action<T>? onReturn = _options.OnReturn;
			if (onReturn != null)
				onReturn(obj);

			if (lastReturned == null)
				lastReturned = obj;

			else if (Available < _options.MaxSize)
				_pool.Push(obj);

			else
			{
				--Total;
				var onDrop = _options.OnDrop;
				if (onDrop != null)
					onDrop(obj);
			}
		}

		/// <summary>
		/// Drops the pooled objects, allowing them to be reclaimed by the garbage collector.
		/// </summary>
		public void Drop()
		{
			var onDrop = _options.OnDrop;
			if (onDrop != null)
			{
				foreach (T obj in _pool)
					onDrop(obj);
				if (lastReturned != null)
					onDrop(lastReturned);
			}

			Total = 0;
			_pool.Clear();
			lastReturned = default(T);
		}

		public void Dispose() => Drop();

		/// <summary>Configures the pool behaviour.</summary>
		public sealed class Options
		{
			/// <summary>The initial size of the pool.</summary>
			public int MinSize { get; set; } = 10;

			/// <summary>
			/// The pool size limit, at which point it'll overflow and ignore
			/// returned objects, allowing them to be garbage-collected.
			/// </summary>
			public int MaxSize { get; set; } = 10000;

			/// <summary>Callback to invoke on the rented objects.</summary>
			public Action<T>? OnRent { get; set; }

			/// <summary>Callback to invoke on the returned objects.</summary>
			public Action<T>? OnReturn { get; set; }

			/// <summary>
			/// Callback to invoke on the dropped objects: ignored on return due
			/// to overflow or objects dropped on <see cref="AsyncObjectPool{T}.Drop" />.
			/// </summary>
			public Action<T>? OnDrop { get; set; }
		}
	}
}