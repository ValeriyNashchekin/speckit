using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FamilyLibrary.Plugin.Infrastructure.Http
{
    /// <summary>
    /// Helper class for HTTP operations with retry logic and exponential backoff.
    /// Provides resilience against transient network failures.
    /// </summary>
    public static class RetryHelper
    {
        private static readonly Random Jitter = new Random();

        /// <summary>
        /// Default maximum number of retry attempts.
        /// </summary>
        public const int DefaultMaxRetries = 3;

        /// <summary>
        /// Default initial delay in milliseconds before first retry.
        /// </summary>
        public const int DefaultInitialDelayMs = 500;

        /// <summary>
        /// Default backoff multiplier for exponential delay.
        /// </summary>
        public const double DefaultBackoffMultiplier = 2.0;

        /// <summary>
        /// Executes an async operation with retry logic and exponential backoff.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
        /// <param name="initialDelayMs">Initial delay in milliseconds (default: 500).</param>
        /// <param name="backoffMultiplier">Multiplier for exponential backoff (default: 2.0).</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="HttpRequestException">Thrown when all retry attempts fail.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is cancelled.</exception>
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = DefaultMaxRetries,
            int initialDelayMs = DefaultInitialDelayMs,
            double backoffMultiplier = DefaultBackoffMultiplier,
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            int attempt = 0;
            int delay = initialDelayMs;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    return await operation().ConfigureAwait(false);
                }
                catch (HttpRequestException ex) when (attempt < maxRetries && IsTransientError(ex))
                {
                    attempt++;

                    // Add jitter to avoid thundering herd problem
                    int jitteredDelay = delay + Jitter.Next(0, Math.Max(1, delay / 2));

                    // Wait before retry
                    await Task.Delay(jitteredDelay, cancellationToken).ConfigureAwait(false);

                    // Increase delay for next attempt
                    delay = (int)(delay * backoffMultiplier);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (attempt < maxRetries && IsTransientException(ex))
                {
                    attempt++;

                    int jitteredDelay = delay + Jitter.Next(0, Math.Max(1, delay / 2));
                    await Task.Delay(jitteredDelay, cancellationToken).ConfigureAwait(false);
                    delay = (int)(delay * backoffMultiplier);
                }
                catch (Exception)
                {
                    // Non-transient error, do not retry
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes an async operation without return value with retry logic.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
        /// <param name="initialDelayMs">Initial delay in milliseconds (default: 500).</param>
        /// <param name="backoffMultiplier">Multiplier for exponential backoff (default: 2.0).</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation.</param>
        /// <returns>A task representing the async operation.</returns>
        public static async Task ExecuteWithRetryAsync(
            Func<Task> operation,
            int maxRetries = DefaultMaxRetries,
            int initialDelayMs = DefaultInitialDelayMs,
            double backoffMultiplier = DefaultBackoffMultiplier,
            CancellationToken cancellationToken = default)
        {
            await ExecuteWithRetryAsync(
                async () =>
                {
                    await operation().ConfigureAwait(false);
                    return true;
                },
                maxRetries,
                initialDelayMs,
                backoffMultiplier,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a synchronous operation with retry logic.
        /// Uses Task.Run to avoid blocking the calling thread.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
        /// <param name="initialDelayMs">Initial delay in milliseconds (default: 500).</param>
        /// <param name="backoffMultiplier">Multiplier for exponential backoff (default: 2.0).</param>
        /// <returns>The result of the operation.</returns>
        public static T ExecuteWithRetry<T>(
            Func<T> operation,
            int maxRetries = DefaultMaxRetries,
            int initialDelayMs = DefaultInitialDelayMs,
            double backoffMultiplier = DefaultBackoffMultiplier)
        {
            return ExecuteWithRetryAsync(
                () => Task.FromResult(operation()),
                maxRetries,
                initialDelayMs,
                backoffMultiplier).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes a synchronous operation without return value with retry logic.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
        /// <param name="initialDelayMs">Initial delay in milliseconds (default: 500).</param>
        /// <param name="backoffMultiplier">Multiplier for exponential backoff (default: 2.0).</param>
        public static void ExecuteWithRetry(
            Action operation,
            int maxRetries = DefaultMaxRetries,
            int initialDelayMs = DefaultInitialDelayMs,
            double backoffMultiplier = DefaultBackoffMultiplier)
        {
            ExecuteWithRetryAsync(
                () =>
                {
                    operation();
                    return Task.CompletedTask;
                },
                maxRetries,
                initialDelayMs,
                backoffMultiplier).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Determines if an HTTP error is transient and should be retried.
        /// </summary>
        /// <param name="ex">The HTTP request exception.</param>
        /// <returns>True if the error is transient, false otherwise.</returns>
        private static bool IsTransientError(HttpRequestException ex)
        {
            // In .NET Framework 4.8, HttpRequestException does not have StatusCode property
            // We treat all HttpRequestException as potentially transient for network errors
            // Non-transient errors (4xx client errors) should be handled by the caller
            var message = ex.Message?.ToLowerInvariant() ?? string.Empty;

            // Check for clearly non-transient client errors in message
            if (message.Contains("400") || message.Contains("401") ||
                message.Contains("403") || message.Contains("404"))
            {
                return false;
            }

            // Check for transient HTTP status codes in message
            if (message.Contains("500") || message.Contains("502") ||
                message.Contains("503") || message.Contains("504") ||
                message.Contains("429") || message.Contains("408"))
            {
                return true;
            }

            // Network errors (no status code) are typically transient
            return true;
        }

        /// <summary>
        /// Determines if a general exception is transient and should be retried.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        /// <returns>True if the exception is transient, false otherwise.</returns>
        private static bool IsTransientException(Exception ex)
        {
            // TaskCanceledException without cancellation token = timeout
            if (ex is TaskCanceledException && !ex.Message.Contains("cancellationToken"))
            {
                return true;
            }

            // Socket/Network related exceptions
            var innerException = ex.InnerException;
            if (innerException != null)
            {
                var typeName = innerException.GetType().Name;
                if (typeName.Contains("Socket") ||
                    typeName.Contains("Network") ||
                    typeName.Contains("Timeout") ||
                    typeName.Contains("Connection"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
