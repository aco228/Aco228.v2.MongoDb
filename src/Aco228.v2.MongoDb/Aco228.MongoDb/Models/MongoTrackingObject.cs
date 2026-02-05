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

    private static readonly HashSet<string> IgnoreProperties = new()
    {
        nameof(MongoDocument.Id),
        nameof(MongoDocument.CreatedUtc),
        nameof(MongoDocument.UpdatedUtc),
    };

    public MongoTrackingObject(object document, Type documentType)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _documentType = documentType;
        _properties = MongoDocumentPropertyMap.MapThrough(_documentType).ToArray();
    }

    public MongoTrackingObject StartTracking()
    {
        foreach (var entry in _properties)
        {
            if (IgnoreProperties.Contains(entry.PropertyInfo.Name))
                continue;
            
            var value = entry.PropertyInfo.GetValue(_document);
            _originalValues[entry.PropertyInfo.Name] = StoreValue(value, entry.PropertyInfo.PropertyType);
        }

        return this;
    }
    
    public List<ChangedField> GetChangedFields()
        => EnumerateChangedFields().ToList();
    
    public bool AnyChanges()
        => EnumerateChangedFields().Any();

    private IEnumerable<ChangedField> EnumerateChangedFields()
    {
        if (_originalValues.Count == 0)
            throw new InvalidOperationException("Document is not tracked. Call StartTracking() first.");

        foreach (var entry in _properties)
        {
            var prop = entry.PropertyInfo;
            if (IgnoreProperties.Contains(prop.Name))
                continue;
            
            var currentValue = prop.GetValue(_document);

            if (!_originalValues.TryGetValue(prop.Name, out var originalValue))
                continue;

            if (!AreValuesEqual(originalValue, currentValue))
                yield return new ChangedField(entry.ColumnName, originalValue, currentValue);
        }
    }

    public object? GetOriginalValue(string propertyName)
    {
        return _originalValues.TryGetValue(propertyName, out var value) ? value : null;
    }

    public bool HasTracking() => _originalValues.Count > 0;

    public void ResetTracking()
    {
        ClearTracking();
        StartTracking();
    }

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
            return CloneCollection(value);

        if (valueType.IsValueType || valueType == typeof(string))
            return value;

        if (valueType == typeof(Guid) || valueType == typeof(ObjectId))
            return value;

        return value;
    }

    private static object CloneCollection(object collection)
    {
        var collectionType = collection.GetType();
        var genericDef = collectionType.GetGenericTypeDefinition();

        if (typeof(System.Collections.Generic.IList<>).IsAssignableFrom(genericDef))
        {
            var itemType = collectionType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(itemType);
            var newList = (System.Collections.IList)Activator.CreateInstance(listType)!;

            foreach (var item in (System.Collections.IEnumerable)collection)
                newList.Add(item);

            return newList;
        }

        if (typeof(System.Collections.Generic.ISet<>).IsAssignableFrom(genericDef))
        {
            var itemType = collectionType.GetGenericArguments()[0];
            var setType = typeof(HashSet<>).MakeGenericType(itemType);
            var newSet = (System.Collections.IEnumerable)Activator.CreateInstance(setType)!;
            var addMethod = setType.GetMethod("Add");

            foreach (var item in (System.Collections.IEnumerable)collection)
                addMethod!.Invoke(newSet, new[] { item });

            return newSet;
        }

        if (typeof(System.Collections.Generic.IDictionary<,>).IsAssignableFrom(genericDef))
        {
            var args = collectionType.GetGenericArguments();
            var dictType = typeof(Dictionary<,>).MakeGenericType(args);
            var newDict = (System.Collections.IEnumerable)Activator.CreateInstance(dictType)!;
            var addMethod = dictType.GetMethod("Add");

            foreach (System.Collections.DictionaryEntry item in (System.Collections.IEnumerable)collection)
                addMethod!.Invoke(newDict, new[] { item.Key, item.Value });

            return newDict;
        }

        if (collectionType.IsArray)
        {
            return ((Array)collection).Clone();
        }

        return collection;
    }

    private static bool AreValuesEqual(object? oldValue, object? newValue)
    {
        if (oldValue == null && newValue == null)
            return true;

        if (oldValue == null || newValue == null)
            return false;

        var valueType = oldValue.GetType();

        if (typeof(System.Collections.ICollection).IsAssignableFrom(valueType) && valueType != typeof(string))
        {
            var oldList = (System.Collections.ICollection)oldValue;
            var newList = (System.Collections.ICollection)newValue;
        
            if (oldList.Count != newList.Count)
                return false;

            var oldEnum = oldList.GetEnumerator();
            var newEnum = newList.GetEnumerator();

            while (oldEnum.MoveNext() && newEnum.MoveNext())
            {
                if (!AreValuesEqual(oldEnum.Current, newEnum.Current))
                    return false;
            }

            return true;
        }

        if (valueType.IsValueType)
            return oldValue.Equals(newValue);

        if (oldValue is string)
            return oldValue.Equals(newValue);

        return ReferenceEquals(oldValue, newValue);
    }
}