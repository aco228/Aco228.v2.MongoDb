using System.Collections.Concurrent;
using System.Text.Json;
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

    private static readonly ConcurrentDictionary<Type, Func<object, object>> _cloners = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object, bool>> _comparers = new();

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
            _originalValues[entry.PropertyInfo.Name] = StoreValue(value);
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
    private static object? StoreValue(object? value)
    {
        if (value == null)
            return null;

        var valueType = value.GetType();

        if (valueType.IsValueType || valueType == typeof(string) || valueType == typeof(Guid) || valueType == typeof(ObjectId))
            return value;

        // Serialize everything else (collections and complex objects) as JSON
        return JsonSerializer.Serialize(value);
    }

    private static bool AreValuesEqual(object? oldValue, object? newValue)
    {
        if (oldValue == null && newValue == null)
            return true;

        if (oldValue == null || newValue == null)
            return false;

        // oldValue was stored as JSON string for complex objects
        if (oldValue is string oldJson && newValue is not string)
        {
            var newJson = JsonSerializer.Serialize(newValue);
            return oldJson == newJson;
        }

        var valueType = oldValue.GetType();

        if (typeof(System.Collections.ICollection).IsAssignableFrom(valueType) && valueType != typeof(string))
        {
            var comparer = _comparers.GetOrAdd(valueType, BuildComparer);
            return comparer(oldValue, newValue);
        }

        if (valueType.IsValueType || valueType == typeof(string))
            return oldValue.Equals(newValue);

        // Both are complex reference types — serialize and compare
        return JsonSerializer.Serialize(oldValue) == JsonSerializer.Serialize(newValue);
    }

    private static Func<object, object> BuildCloner(Type collectionType)
    {
        if (collectionType.IsArray)
            return col => ((Array)col).Clone();

        if (!collectionType.IsGenericType)
            return col => col;

        var genericDef = collectionType.GetGenericTypeDefinition();
        var args = collectionType.GetGenericArguments();

        if (genericDef == typeof(List<>))
            return col => CloneList(col, args[0]);

        if (genericDef == typeof(Dictionary<,>))
            return col => CloneDictionary(col, args);

        if (genericDef == typeof(HashSet<>))
            return col => CloneHashSet(col, args[0]);

        return col => col;
    }

    private static Func<object, object, bool> BuildComparer(Type collectionType)
    {
        return (old, new_) => CompareCollections(old, new_);
    }

    private static object CloneList(object collection, Type itemType)
    {
        var listType = typeof(List<>).MakeGenericType(itemType);
        var newList = (System.Collections.IList)Activator.CreateInstance(listType)!;
        foreach (var item in (System.Collections.IEnumerable)collection)
            newList.Add(item);
        return newList;
    }

    private static object CloneDictionary(object collection, Type[] args)
    {
        var dictType = typeof(Dictionary<,>).MakeGenericType(args);
        var newDict = (System.Collections.IDictionary)Activator.CreateInstance(dictType)!;
        foreach (System.Collections.DictionaryEntry entry in (System.Collections.IDictionary)collection)
            newDict.Add(entry.Key, entry.Value);
        return newDict;
    }

    private static object CloneHashSet(object collection, Type itemType)
    {
        var setType = typeof(HashSet<>).MakeGenericType(itemType);
        var newSet = (System.Collections.IEnumerable)Activator.CreateInstance(setType)!;
        var addMethod = setType.GetMethod("Add")!;
        foreach (var item in (System.Collections.IEnumerable)collection)
            addMethod.Invoke(newSet, new[] { item });
        return newSet;
    }

    private static bool CompareCollections(object? oldValue, object? newValue)
    {
        var oldList = (System.Collections.ICollection)oldValue!;
        var newList = (System.Collections.ICollection)newValue!;

        if (oldList.Count != newList.Count)
            return false;

        var oldArray = new object[oldList.Count];
        var newArray = new object[newList.Count];
        
        oldList.CopyTo(oldArray, 0);
        newList.CopyTo(newArray, 0);

        return oldArray.SequenceEqual(newArray);
    }
}