using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;

namespace CizaAsync
{
	/// <summary>
	/// Signals async task completion to the associated tokens and allows awaiting the completion.
	/// </summary>
	public class AsyncSource : IDisposable
	{
		// VARIABLE: -----------------------------------------------------------------------------

		protected CancellationTokenSource _tokenSource = new CancellationTokenSource();

		// PUBLIC VARIABLE: ---------------------------------------------------------------------

		/// <summary>
		/// Cancellation token associated with the current task state.
		/// Will cancel on <see cref="Complete"/> or <see cref="Reset"/>.
		/// </summary>
		public virtual CancellationToken Token => _tokenSource.Token;

		/// <summary>
		/// Whether the current task has completed.
		/// </summary>
		public virtual bool IsComplete => _tokenSource.IsCancellationRequested;


		[Preserve]
		public AsyncSource() : this(false) { }

		/// <param name="isCompleted">Whether the source should be created in the completed state.</param>
		[Preserve]
		public AsyncSource(bool isCompleted)
		{
			if (isCompleted) Complete();
		}

		/// <summary>
		/// Resets the source to the default uncompleted state and recreates the tokens source.
		/// Will complete the previous state and associated tokens source in case it was not completed.
		/// </summary>
		public virtual void Reset()
		{
			Complete();
			_tokenSource.Dispose();
			_tokenSource = new CancellationTokenSource();
		}

		/// <summary>
		/// Transitions the source into the completed state and notifies associates tokens.
		/// Has no effect when the source is already in completed state.
		/// </summary>
		public virtual void Complete()
		{
			if (!IsComplete)
				_tokenSource.Cancel();
		}

		/// <summary>
		/// Waits until the current task is <see cref="Complete"/> or <see cref="Reset"/>.
		/// </summary>
		public async Awaitable WaitCompletionAsync(AsyncToken asyncToken)
		{
			var token = Token;
			while (!token.IsCancellationRequested && asyncToken.EnsureNotCanceledOrCompleted())
				await Awaitable.NextFrameAsync();
		}

		public void Dispose()
		{
			Complete();
			_tokenSource.Dispose();
		}
	}

	/// <inheritdoc/>
	public class AsyncSource<T> : AsyncSource
	{
		// PUBLIC VARIABLE: ---------------------------------------------------------------------

		/// <summary>
		/// Result of the completed task, or default when not completed.
		/// </summary>
		public virtual T Result { get; protected set; }


		// CONSTRUCTOR: ------------------------------------------------------------------------

		[Preserve]
		public AsyncSource() { }

		/// <summary>
		/// Creates the source in the completed state with the specified result.
		/// </summary>
		public AsyncSource(T result) : base(true)
		{
			Result = result;
		}

		// PUBLIC METHOD: ----------------------------------------------------------------------

		public override void Reset()
		{
			Result = default;
			base.Reset();
		}

		/// <inheritdoc cref="Complete"/>
		public virtual void Complete(T result)
		{
			Result = result;
			Complete();
		}

		/// <inheritdoc cref="AsyncSource.WaitCompletionAsync"/>
		public virtual async Awaitable<T> WaitResultAsync(AsyncToken asyncToken)
		{
			await WaitCompletionAsync(asyncToken);
			return Result;
		}
	}
}