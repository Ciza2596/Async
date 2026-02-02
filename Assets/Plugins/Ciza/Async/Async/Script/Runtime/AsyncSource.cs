using System;
using System.Threading;
using UnityEngine;

namespace CizaAsync
{
	/// <summary>
	/// Signals async task completion to the associated tokens and allows awaiting the completion.
	/// </summary>
	public class AsyncSource : IDisposable
	{
		private CancellationTokenSource cs = new CancellationTokenSource();

		/// <summary>
		/// Cancellation token associated with the current task state.
		/// Will cancel on <see cref="Complete"/> or <see cref="Reset"/>.
		/// </summary>
		public CancellationToken Token => cs.Token;

		/// <summary>
		/// Whether the current task has completed.
		/// </summary>
		public bool Completed => cs.IsCancellationRequested;


		/// <param name="completed">Whether the source should be created in the completed state.</param>
		public AsyncSource(bool completed = false)
		{
			if (completed) Complete();
		}

		/// <summary>
		/// Transitions the source into the completed state and notifies associates tokens.
		/// Has no effect when the source is already in completed state.
		/// </summary>
		public void Complete()
		{
			if (!Completed) cs.Cancel();
		}

		/// <summary>
		/// Resets the source to the default uncompleted state and recreates the tokens source.
		/// Will complete the previous state and associated tokens source in case it was not completed.
		/// </summary>
		public virtual void Reset()
		{
			Complete();
			cs.Dispose();
			cs = new CancellationTokenSource();
		}

		/// <summary>
		/// Waits until the current task is <see cref="Complete"/> or <see cref="Reset"/>.
		/// </summary>
		public async Awaitable WaitCompletion(AsyncToken token = default)
		{
			var src = Token;
			while (!src.IsCancellationRequested && token.EnsureNotCanceledOrCompleted())
				await Awaitable.NextFrameAsync();
		}

		public void Dispose()
		{
			Complete();
			cs.Dispose();
		}
	}

	/// <inheritdoc/>
	public class AsyncSource<T> : AsyncSource
	{
		/// <summary>
		/// Result of the completed task, or default when not completed.
		/// </summary>
		public T Result { get; private set; }

		public AsyncSource() { }

		/// <summary>
		/// Creates the source in the completed state with the specified result.
		/// </summary>
		public AsyncSource(T result) : base(true)
		{
			Result = result;
		}

		/// <inheritdoc cref="Complete"/>
		public void Complete(T result)
		{
			Result = result;
			Complete();
		}

		/// <inheritdoc cref="AsyncSource.WaitCompletion"/>
		public async Awaitable<T> WaitResult(AsyncToken token = default)
		{
			await WaitCompletion(token);
			return Result;
		}

		public override void Reset()
		{
			Result = default;
			base.Reset();
		}
	}
}