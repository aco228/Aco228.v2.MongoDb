using MongoDB.Driver;
using Polly;

namespace Aco228.MongoDb.Constants;

internal static class PollyConfiguration
{
    public static readonly AsyncPolicy Async = Policy
        .Handle<MongoConnectionException>()
        .Or<MongoNodeIsRecoveringException>()
        .Or<MongoNotPrimaryException>()
        .OrInner<IOException>() // This catches the "I/O error occurred"
        .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // Exponential backoff: 2s, 4s, 8s
        );
    
    public static readonly ISyncPolicy Sync = Policy
        .Handle<MongoConnectionException>()
        .Or<MongoNodeIsRecoveringException>()
        .OrInner<IOException>()
        .WaitAndRetry(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
        );
}