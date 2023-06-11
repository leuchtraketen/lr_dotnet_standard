using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LR.Standard;

public static class Tasks
{
    [Obsolete("Use Task.CompletedTask instead. (The standard library provides this now.)")]
    public static readonly Task Completed = Task.Delay(0);

    public static void Forget(this Task? task, ILogger? logger = null)
    {
        task?.ContinueWith((t) =>
        {
            if (t.Exception is Exception ex)
            {
                logger?.LogError(ex, $"Error in Fire-and-forget Task");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static void Forget<T>(this Task<T>? task, ILogger? logger = null)
    {
        task?.ContinueWith((t) =>
        {
            if (t.Exception is Exception ex)
            {
                logger?.LogError(ex, $"Error in Fire-and-forget Task");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static void Forget(this Func<Task>? action, ILogger? logger = null)
    {
        action?.Invoke().ContinueWith((t) =>
        {
            if (t.Exception is Exception ex)
            {
                logger?.LogError(ex, $"Error in Fire-and-forget Task");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static void WhenFaulted(this Task? task, Action<Exception> exception_callback)
    {
        task?.ContinueWith((t) =>
        {
            if (t.Exception is Exception ex)
            {
                if (ex is AggregateException ex_aggregate && ex_aggregate.InnerExceptions is ReadOnlyCollection<Exception> exceptions_inner && exceptions_inner.Count == 1)
                {
                    ex = exceptions_inner.Single();
                }

                exception_callback(ex);
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static void WhenFaulted<T>(this Task<T>? task, Action<Exception> exception_callback)
    {
        task?.ContinueWith((t) =>
        {
            if (t.Exception is Exception ex)
            {
                if (ex is AggregateException ex_aggregate && ex_aggregate.InnerExceptions is ReadOnlyCollection<Exception> exceptions_inner && exceptions_inner.Count == 1)
                {
                    ex = exceptions_inner.Single();
                }

                exception_callback(ex);
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static void WhenFaulted(this Func<Task>? action, Action<Exception> exception_callback)
    {
        action?.Invoke().ContinueWith((t) =>
        {
            if (t.Exception is Exception ex)
            {
                if (ex is AggregateException ex_aggregate && ex_aggregate.InnerExceptions is ReadOnlyCollection<Exception> exceptions_inner && exceptions_inner.Count == 1)
                {
                    ex = exceptions_inner.Single();
                }

                exception_callback(ex);
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// Executes a task with a timeout.
    /// </summary>
    /// <returns>
    /// It will return the result of the task, or it will throw a <c>System.TimeoutException</c> if the timeout is  reached.
    /// </returns>
    /// <exception cref="System.TimeoutException">Thrown when the timeout is reached.</exception>
    public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        using CancellationTokenSource cts_timeout = new();
        Task? completed_task = await Task.WhenAny(task, Task.Delay(timeout, cts_timeout.Token));
        if (completed_task == task)
        {
            cts_timeout.Cancel();
            return await task;
        }
        else
        {
            throw new TimeoutException("The operation has timed out.");
        }
    }

    /// <summary>
    /// Executes a task with a timeout.
    /// </summary>
    /// <returns>
    /// It will return nothing, or it will throw a <c>System.TimeoutException</c> if the timeout is  reached.
    /// </returns>
    /// <exception cref="System.TimeoutException">Thrown when the timeout is reached.</exception>
    public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
    {
        using CancellationTokenSource cts_timeout = new();
        Task? completed_task = await Task.WhenAny(task, Task.Delay(timeout, cts_timeout.Token));
        if (completed_task == task)
        {
            cts_timeout.Cancel();
            await task;
        }
        else
        {
            throw new TimeoutException("The operation has timed out.");
        }
    }

    /// <summary>
    /// Executes a task with a timeout.
    /// </summary>
    /// <returns>
    /// It will return the result of the task, or it will throw a <c>System.TimeoutException</c> if the timeout is  reached.
    /// </returns>
    /// <exception cref="System.TimeoutException">Thrown when the timeout is reached.</exception>
    public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationToken ct)
    {
        if (ct == default)
        {
            using CancellationTokenSource cts_timeout = new();
            Task? completed_task = await Task.WhenAny(task, Task.Delay(timeout, cts_timeout.Token));
            if (completed_task == task)
            {
                cts_timeout.Cancel();
                return await task;
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
        else
        {
            ct.ThrowIfCancellationRequested();
            using CancellationTokenSource cts_timeout = new();
            Task? completed_task = await Task.WhenAny(task, Task.Delay(timeout, cts_timeout.Token), Task.Delay(timeout, ct));
            if (completed_task == task)
            {
                cts_timeout.Cancel();
                return await task;
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }

    /// <summary>
    /// Executes a task with a timeout.
    /// </summary>
    /// <returns>
    /// It will return nothing, or it will throw a <c>System.TimeoutException</c> if the timeout is  reached.
    /// </returns>
    /// <exception cref="System.TimeoutException">Thrown when the timeout is reached.</exception>
    public static async Task TimeoutAfter(this Task task, TimeSpan timeout, CancellationToken ct)
    {
        if (ct == default)
        {
            using CancellationTokenSource cts_timeout = new();
            Task? completed_task = await Task.WhenAny(task, Task.Delay(timeout, cts_timeout.Token));
            if (completed_task == task)
            {
                cts_timeout.Cancel();
                await task;
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
        else
        {
            ct.ThrowIfCancellationRequested();
            using CancellationTokenSource cts_timeout = new();
            Task? completed_task = await Task.WhenAny(task, Task.Delay(timeout, cts_timeout.Token), Task.Delay(timeout, ct));
            if (completed_task == task)
            {
                cts_timeout.Cancel();
                await task;
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }
}
