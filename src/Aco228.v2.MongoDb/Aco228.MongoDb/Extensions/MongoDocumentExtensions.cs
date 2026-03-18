using Aco228.MongoDb.Models;
using MongoDB.Bson;

namespace Aco228.MongoDb.Extensions;

public static class MongoDocumentExtensions
{
    internal static bool CheckIfNewAndPrepareForInsert(this MongoDocument document)
    {
        if (document.Id == ObjectId.Empty)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    internal static void SetDocumentDefaultValues(MongoDocument document)
    {
        if (document.CreatedUtc == 0)
            document.CreatedUtc = DT.GetUnix();
        
        if (document.UpdatedUtc == 0)
            document.UpdatedUtc = DT.GetUnix();
        
        if (document.Id == ObjectId.Empty)
        {
            document.Id = ObjectId.GenerateNewId();
            document.CreatedUtc = DT.GetUnix();
            document.UpdatedUtc = DT.GetUnix();
        }
        else
        {
            document.UpdatedUtc = DT.GetUnix();
        }
    }
}