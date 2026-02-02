using System;
using System.Collections.Generic;
using System.Threading;

namespace CizaAsync
{
	public readonly struct AsyncToken : IEquatable<AsyncToken>
	{
		/// <summary>
		/// A none token.
		/// </summary>
		public static readonly AsyncToken NONE = new(CancellationToken.None);

		/// <summary>
		/// A canceled token.
		/// </summary>
		public static readonly AsyncToken CANCELED = new(new CancellationToken(true));

		/// <summary>
		/// A completed token.
		/// </summary>
		public static readonly AsyncToken COMPLETED = new(CancellationToken.None, new CancellationToken(true));

		/// <summary>
		/// Source token for cancellation scenario.
		/// </summary>
		public CancellationToken CancellationToken { get; }

		/// <summary>
		/// Source token for completion scenario.
		/// </summary>
		public CancellationToken CompletionToken { get; }

		/// <summary>
		/// Whether cancellation of the async operation is requested.
		/// </summary>
		public bool Canceled => CancellationToken.IsCancellationRequested;

		/// <summary>
		/// Whether completion of the async operation is requested as soon as possible, but it's not canceled.
		/// </summary>
		public bool Completed => !Canceled && CompletionToken.IsCancellationRequested;

		/// <param name="cancellationToken">Source token for the cancellation scenario.</param>
		/// <param name="completionToken">Source token for the completion scenario.</param>
		public AsyncToken(CancellationToken cancellationToken, CancellationToken completionToken = default)
		{
			CancellationToken = cancellationToken;
			CompletionToken = completionToken;
		}

		/// <summary>
		/// Throws <see cref="AsyncOperationCanceledException"/> in case cancellation is requested
		/// or <see cref="AsyncOperationDestroyedException"/> in case specified Unity object is destroyed.
		/// </summary>
		public void ThrowIfCanceled(UnityEngine.Object obj = null)
		{
			if (Canceled) throw new AsyncOperationCanceledException(this);
			if (obj is not null && !obj) throw new AsyncOperationDestroyedException(obj);
		}

		/// <summary>
		/// Throws <see cref="AsyncOperationCanceledException"/> in case cancellation is requested
		/// or <see cref="AsyncOperationDestroyedException"/> in case specified Unity object is destroyed;
		/// otherwise returns true.
		/// </summary>
		public bool EnsureNotCanceled(UnityEngine.Object obj = null)
		{
			ThrowIfCanceled(obj);
			return true;
		}

		/// <summary>
		/// Throws <see cref="AsyncOperationCanceledException"/> in case cancellation is requested;
		/// or <see cref="AsyncOperationDestroyedException"/> in case specified Unity object is destroyed;
		/// otherwise returns whether completion is not requested.
		/// </summary>
		public bool EnsureNotCanceledOrCompleted(UnityEngine.Object obj = null)
		{
			ThrowIfCanceled(obj);
			return !Completed;
		}

		// public static implicit operator AsyncToken(CancellationToken token) => new(token);

		public static bool operator ==(AsyncToken left, AsyncToken right) => left.Equals(right);

		public static bool operator !=(AsyncToken left, AsyncToken right) => !(left == right);

		public override bool Equals(object obj) => obj is AsyncToken token && Equals(token);

		public bool Equals(AsyncToken other)
		{
			return EqualityComparer<CancellationToken>.Default.Equals(CancellationToken, other.CancellationToken) && EqualityComparer<CancellationToken>.Default.Equals(CompletionToken, other.CompletionToken);
		}

		public override int GetHashCode()
		{
			int hashCode = -1518707389;
			hashCode = hashCode * -1521134295 + CancellationToken.GetHashCode();
			hashCode = hashCode * -1521134295 + CompletionToken.GetHashCode();
			return hashCode;
		}
	}
}