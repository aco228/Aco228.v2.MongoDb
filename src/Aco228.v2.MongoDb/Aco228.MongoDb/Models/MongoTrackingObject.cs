using Aco228.MongoDb.Helpers;
using MongoDB.Bson;

namespace Aco228.MongoDb.Models;

public record ChangedField(string PropertyName, object? OldValue, object? NewValue);

public class MongoTrackingObject
{
    private readonly object _document;
    private readonly Type _documentType;
    private readonly MongoDocumentPropertyEntry[] _properties;
    private Dictionary<string, object?> _originalValues = new();

    public MongoTrackingObject(object document, Type documentType)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _documentType = documentType;
        _properties = MongoDocumentMap.MapThrough(_documentType).ToArray();
    }

    public MongoTrackingObject StartTracking()
    {
        foreach (var entry in _properties)
        {
            var value = entry.PropertyInfo.GetValue(_document);
            _originalValues[entry.PropertyInfo.Name] = StoreValue(value, entry.PropertyInfo.PropertyType);
        }

        return this;
    }

    public List<ChangedField> GetChangedFields()
    {
        if (_originalValues.Count == 0)
            throw new InvalidOperationException("Document is not tracked. Call StartTracking() first.");

        var changes = new List<ChangedField>(_properties.Length);

        foreach (var entry in _properties)
        {
            var prop = entry.PropertyInfo;
            var currentValue = prop.GetValue(_document);

            if (!_originalValues.TryGetValue(prop.Name, out var originalValue))
                continue;

            if (!AreValuesEqual(originalValue, currentValue))
            {
                changes.Add(new ChangedField(prop.Name, originalValue, currentValue));
            }
        }

        return changes;
    }

    public object? GetOriginalValue(string propertyName)
    {
        return _originalValues.TryGetValue(propertyName, out var value) ? value : null;
    }

    public bool HasTracking() => _originalValues.Count > 0;

    public MongoTrackingObject ClearTracking()
    {
        _originalValues.Clear();
        return this;
    }

    private static object? StoreValue(object? value, Type propertyType)
    {
        if (value == null)
            return null;

        var valueType = value.GetType();

        if (typeof(System.Collections.ICollection).IsAssignableFrom(valueType) && valueType != typeof(string))
            return value;

        if (valueType.IsValueType || valueType == typeof(string))
            return value;

        if (valueType == typeof(Guid) || valueType == typeof(ObjectId))
            return value;

        return value;
    }

    private static bool AreValuesEqual(object? oldValue, object? newValue)
    {
        if (oldValue == null && newValue == null)
            return true;

        if (oldValue == null || newValue == null)
            return false;

        var valueType = oldValue.GetType();

        if (typeof(System.Collections.ICollection).IsAssignableFrom(valueType) && valueType != typeof(string))
            return ReferenceEquals(oldValue, newValue);

        if (valueType.IsValueType)
            return oldValue.Equals(newValue);

        if (oldValue is string)
            return oldValue.Equals(newValue);

        return ReferenceEquals(oldValue, newValue);
    }
}