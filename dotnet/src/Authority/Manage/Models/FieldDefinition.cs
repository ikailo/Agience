using Agience.Core.Models.Entities.Abstract;
using System.Reflection;

namespace Agience.Authority.Manage.Models
{
    public enum FieldType
    {
        Text,
        Lookup,
        Checkbox,
        DropDown,
        MultiSelect,
        Button,
        Upload
    }

    public class FieldDefinition
    {
        // Events

        public Func<BaseEntity, object?, Task>? OnValueChanged { get; set; }

        // Basic Field Properties
        public FieldType Type { get; set; }
        public string? FieldName { get; set; }
        public string? Label { get; set; } = string.Empty;
        public bool ReadOnly { get; set; }
        public string? Placeholder { get; set; }
        public string? Tooltip { get; set; }
        public bool Required { get; set; } = false;
        public object? DefaultValue { get; set; }

        // DropDown Specific Options
        public Dictionary<string, string> DropDownOptions { get; set; } = new();

        // Relationship Handling
        public string? RelatedEntityApiName { get; set; }
        public string? RelatedEntityFieldName { get; set; }
        public Func<Task<IEnumerable<DescribedEntity>>>? RelatedEntityDataSource { get; set; }

        // Binding Helper Methods
        public object? GetValue(object record)
        {
            if (record == null)
            {
                return DefaultValue;
            }

            if (Type == FieldType.Lookup)
            {
                if (RelatedEntityFieldName == "this")
                {
                    // Return the record itself as it represents the entire entity
                    return record;
                }
                else
                {
                    // Return the related entity
                    return GetPropertyValue(record, RelatedEntityFieldName);
                }
            }
            else
            {
                // For other field types, return the field value
                return GetPropertyValue(record, FieldName);
            }
        }

        public object? GetText(object record)
        {
            if (record == null)
            {
                return null;
            }

            if (Type == FieldType.Lookup)
            {
                if (RelatedEntityFieldName == "this")
                {
                    // Get the Name directly from the top-level object
                    return GetPropertyValue(record, "Name");
                }
                else
                {
                    // Retrieve the nested entity and return its Name
                    var relatedObject = GetPropertyValue(record, RelatedEntityFieldName);
                    return relatedObject != null ? GetPropertyValue(relatedObject, "Name") : null;
                }
            }

            // Fallback for non-Lookup fields
            return GetValue(record)?.ToString();
        }

        public async void SetValue(object record, object? value)
        {
            if (record == null)
            {
                return;
            }

            if (Type == FieldType.Lookup)
            {
                if (RelatedEntityFieldName == "this")
                {
                    if (value == null)
                    {
                        // If value is null, clear all properties in the record
                        foreach (var property in record.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (property.CanWrite)
                            {
                                var defaultValue = property.PropertyType.IsValueType
                                    ? Activator.CreateInstance(property.PropertyType)
                                    : null;

                                property.SetValue(record, defaultValue);
                            }
                        }
                    }
                    else
                    {
                        // Copy all properties from value to record
                        foreach (var property in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (property.CanRead)
                            {
                                var targetProperty = record.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);

                                if (targetProperty != null && targetProperty.CanWrite)
                                {
                                    var propertyValue = property.GetValue(value);
                                    targetProperty.SetValue(record, propertyValue);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Set the related entity and its ID
                    SetPropertyValue(record, RelatedEntityFieldName, value);
                    SetPropertyValue(record, FieldName, value != null ? GetPropertyValue(value, "Id") : null);
                }
            }
            else
            {
                SetPropertyValue(record, FieldName, value);
            }

            if (OnValueChanged != null)
            {
                await OnValueChanged.Invoke((BaseEntity)record, value);
            }
        }

        private object? GetPropertyValue(object source, string propertyPath)
        {
            var parts = propertyPath.Split('.');
            var currentObject = source;

            // Handle Metadata case
            if (parts[0] == "Metadata" && currentObject is BaseEntity baseEntity)
            {
                if (baseEntity.Metadata == null)
                {
                    throw new InvalidOperationException("Metadata dictionary is not initialized.");
                }

                // Access the Metadata dictionary
                var metadataKey = string.Join(".", parts.Skip(1));
                if (baseEntity.Metadata.TryGetValue(metadataKey, out var metadataValue))
                {
                    return metadataValue;
                }

                return null; // Key not found
            }

            // Traverse the property path for regular properties
            foreach (var part in parts)
            {
                if (currentObject == null)
                    return null;

                var property = currentObject.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                    return null;

                currentObject = property.GetValue(currentObject);
            }

            return currentObject;
        }


        private void SetPropertyValue(object record, string propertyPath, object? value)
        {
            var parts = propertyPath.Split('.');
            var currentObject = record;

            // Handle Metadata case
            if (parts[0] == "Metadata" && currentObject is BaseEntity baseEntity)
            {
                if (baseEntity.Metadata == null)
                {
                    baseEntity.Metadata = new Dictionary<string, object?>();
                }

                var metadataKey = string.Join(".", parts.Skip(1));
                baseEntity.Metadata[metadataKey] = value;
                return;
            }

            // Traverse the property path for regular properties
            PropertyInfo? property = null;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                property = currentObject?.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);

                if (property == null || currentObject == null)
                    return;

                if (i == parts.Length - 1) // Last part of the path
                {
                    if (property.CanWrite)
                    {
                        if (property.PropertyType == typeof(List<string>) && value is IEnumerable<string> listValue)
                        {
                            // Directly set the value for List<string>
                            property.SetValue(currentObject, listValue.ToList());
                        }
                        else if (property.PropertyType.IsEnum)
                        {
                            if (Enum.TryParse(property.PropertyType, value?.ToString(), out var convertedValue))
                            {
                                property.SetValue(currentObject, convertedValue);
                            }
                            else
                            {
                                throw new ArgumentException($"Cannot convert '{value}' to {property.PropertyType}");
                            }
                        }
                        else
                        {
                            // Fallback to Convert.ChangeType for other types
                            var convertedValue = value != null
                                ? Convert.ChangeType(value, property.PropertyType)
                                : null;

                            property.SetValue(currentObject, convertedValue);
                        }
                    }
                }
                else
                {
                    // Traverse to the next object in the path
                    currentObject = property.GetValue(currentObject);
                }
            }
        }


    }
}
