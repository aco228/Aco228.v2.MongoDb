using MongoDB.Driver;
using Polly;

namespace Aco228.MongoDb.Constants;

internal static class PollyConfiguration
{
    public static readonly AsyncPolicy Async = Policy
        .Handle<MongoConnectionException>()
        .Or<MongoNodeIsRecoveringException>()
        .Or<MongoNotPrimaryException>()
        .Or<ObjectDisposedException>()
        .OrInner<IOException>()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    public static readonly ISyncPolicy Sync = Policy
        .Handle<MongoConnectionException>()
        .Or<MongoNodeIsRecoveringException>()
        .Or<ObjectDisposedException>()
        .OrInner<IOException>()
        .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}