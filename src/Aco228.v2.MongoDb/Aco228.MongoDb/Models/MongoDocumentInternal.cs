namespace Aco228.MongoDb.Models;


public class MongoDocumentInternal
{
    private MongoTrackingObject? _trackingObject;

    public bool HasTracking() => _trackingObject?.HasTracking() == true;
    public MongoTrackingObject? GetTrackingObject() => _trackingObject;
    
    public MongoTrackingObject StartTracking()
    {
        _trackingObject = new MongoTrackingObject(this, GetType()).StartTracking();
        return _trackingObject;
    }
}