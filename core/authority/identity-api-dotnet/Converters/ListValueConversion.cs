using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Agience.Authority.Identity.Converters
{
    public static class ListValueConversion
    {
        public static ValueConverter<List<string>, string> GetValueConverter()
        {
            return new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), // Serialize to JSON
                v => string.IsNullOrEmpty(v)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>() // Deserialize from JSON
            );
        }

        public static ValueComparer<List<string>> GetValueComparer()
        {
            return new ValueComparer<List<string>>(
                (l1, l2) => l1 != null && l2 != null && l1.SequenceEqual(l2), // Equality comparison
                l => l.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Hash code generation
                l => l != null ? l.ToList() : new List<string>() // Snapshot for tracking
            );
        }
    }
}

