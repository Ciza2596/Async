using System;

namespace CizaAsync
{
	/// <summary>
	/// Thrown upon cancellation of an async operation via <see cref="AsyncToken"/>.
	/// </summary>
	public class AsyncOperationCanceledException : OperationCanceledException
	{
		public AsyncOperationCanceledException(AsyncToken token) : base(token.CancellationToken)
		{
			if (!token.Canceled) throw new ArgumentException("Specified token is not canceled.", nameof(token));
		}

		protected AsyncOperationCanceledException() { }
	}

	/// <summary>
	/// Thrown upon cancellation of an async operation due to Unity object being destroyed.
	/// </summary>
	public class AsyncOperationDestroyedException : AsyncOperationCanceledException
	{
		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
		public AsyncOperationDestroyedException(UnityEngine.Object obj)
		{
			if (obj) throw new ArgumentException("Specified object is not destroyed.", nameof(obj));
		}
	}
}