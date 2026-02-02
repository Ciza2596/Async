using System;

namespace CizaAsync
{
	/// <summary>A disposable wrapper over a pooled object.</summary>
	/// <remarks>
	/// .NET doesn't box disposable structs inside "using" context:
	/// https://github.com/dotnet/csharplang/discussions/8337.
	/// </remarks>
	/// <summary>A disposable wrapper over a pooled object.</summary>
	/// <remarks>
	/// .NET doesn't box disposable structs inside "using" context:
	/// https://github.com/dotnet/csharplang/discussions/8337.
	/// </remarks>
	public readonly struct AsyncPooledObject<T> : IDisposable where T : class
	{
		private readonly T _obj;
		private readonly AsyncObjectPool<T> _pool;

		public AsyncPooledObject(T obj, AsyncObjectPool<T> pool)
		{
			_obj = obj;
			_pool = pool;
		}

		public void Dispose() => _pool.Return(_obj);
	}
}