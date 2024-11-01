using System.Collections;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Agience.Core
{
    public class DataJsonConverter : JsonConverter<Data>
    {
        public override Data Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Data { Raw = JsonDocument.ParseValue(ref reader).RootElement.Deserialize<string>() };
        }

        public override void Write(Utf8JsonWriter writer, Data data, JsonSerializerOptions options)
        {
            writer.WriteStringValue(data.Raw);
        }
    }

    [JsonConverter(typeof(DataJsonConverter))]
    public class Data : IEnumerable<KeyValuePair<string, string?>>
    {
        private Dictionary<string, string?> _structured = new();
        private string? _raw;

        [JsonPropertyName("Raw")]
        public string? Raw
        {
            get
            {
                return _raw ??= JsonSerializer.Serialize(_structured);
            }
            set
            {
                _raw = value;
                
                _structured.Clear();

                if (!string.IsNullOrEmpty(_raw) && _raw.StartsWith("{") && _raw.EndsWith("}"))
                {
                    try
                    {
                        var elements = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(_raw) ?? new Dictionary<string, JsonElement>();

                        foreach (var (key, element) in elements)
                        {
                            _structured[key] = element.ValueKind != JsonValueKind.String ? JsonSerializer.Serialize(element) : element.ToString();
                        }
                    }
                    catch (JsonException)
                    {
                        // do nothing
                    }
                    catch (InvalidOperationException)
                    {
                        // do nothing
                    }
                }
            }
        }

        public void Add(string key, string? value)
        {
            _structured.Add(key, value);
            _raw = null;
        }

        public string? this[string key]
        {
            get => _structured.TryGetValue(key, out var value) ? value : null;
            set
            {
                _structured[key] = value;
                _raw = null;
            }
        }

        public static implicit operator Data?(string? raw) => new Data() { Raw = raw };

        public static implicit operator string?(Data? data) => data?.Raw;

        public override string? ToString() => Raw;

        public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
        {
            return new ReadOnlyDictionary<string, string?>(_structured).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
