using System.Collections.Concurrent;
using System.Reflection;
using Aco228.MongoDb.Extensions.MongoDocuments;
using Aco228.MongoDb.Models;
using MongoDB.Bson;

namespace Aco228.MongoDb.Infrastructure;


public class MongoTransactionCollection
{
    private static MethodInfo OpenMethod = typeof(MongoDocumentsExtensions).GetMethod(nameof(MongoDocumentsExtensions.InsertOrUpdateSingleAsync));
    private ConcurrentDictionary<Type, object> _current = new();

    public T InsertOrUpdate<T>(T obj) where T : MongoDocument
    {
        obj.IgnoreTrackingObject = true;
        obj.Id = ObjectId.GenerateNewId();
        obj.CreatedUtc = DT.GetUnix();
        _current.TryAdd(obj.GetType(), obj);
        return obj;
    }


    public async Task Execute()
    {
        foreach (var (type, document) in _current.ToDictionary())
        {
            var genericMethod = OpenMethod?.MakeGenericMethod(type);
            await (Task)genericMethod?.Invoke(null, new object[] { document });
            _current.TryRemove(type, out _);
        }
    }
    
}