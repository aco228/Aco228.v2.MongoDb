using Aco228.MongoDb.Models;
using MongoDB.Bson;

namespace Aco228.MongoDb.Extensions;

public static class MongoDocumentExtensions
{
    internal static bool CheckIfNewAndPrepareForInsert(this MongoDocument document)
    {
        if (document.Id == ObjectId.Empty)
        {
            document.Id = ObjectId.GenerateNewId();
            document.CreatedUtc = DT.GetUnix();
            document.UpdatedUtc = DT.GetUnix();
            return true;
        }
        else
        {
            document.UpdatedUtc = DT.GetUnix();
            return false;
        }
    }
}