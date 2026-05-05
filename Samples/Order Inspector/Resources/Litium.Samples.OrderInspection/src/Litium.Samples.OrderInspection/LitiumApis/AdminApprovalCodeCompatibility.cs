using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

internal static class AdminJsonCompatibility
{
    public static void RegisterConverters(JsonSerializerSettings settings)
    {
        if (!settings.Converters.OfType<ApprovalCodeCompatibilityConverter>().Any())
        {
            settings.Converters.Add(new ApprovalCodeCompatibilityConverter());
        }

        if (!settings.Converters.OfType<ReturnReasonCompatibilityConverter>().Any())
        {
            settings.Converters.Add(new ReturnReasonCompatibilityConverter());
        }
    }
}

internal sealed class ApprovalCodeCompatibilityConverter : JsonConverter
{
    public override bool CanConvert(System.Type objectType)
    {
        return objectType == typeof(ApprovalCode);
    }

    public override object ReadJson(JsonReader reader, System.Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null!;
        }

        if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
        {
            return new ApprovalCode
            {
                Value = Convert.ToInt32(reader.Value),
                Name = reader.Value?.ToString(),
            };
        }

        if (reader.TokenType == JsonToken.String)
        {
            var raw = reader.Value?.ToString();
            if (int.TryParse(raw, out var numericValue))
            {
                return new ApprovalCode
                {
                    Value = numericValue,
                    Name = raw,
                };
            }

            return new ApprovalCode
            {
                Name = raw,
            };
        }

        if (reader.TokenType == JsonToken.StartObject)
        {
            var jsonObject = JObject.Load(reader);
            var approvalCode = new ApprovalCode
            {
                Name = jsonObject["name"]?.Value<string>(),
            };

            var valueToken = jsonObject["value"];
            if (valueToken != null && valueToken.Type != JTokenType.Null)
            {
                approvalCode.Value = valueToken.Value<int>();
            }

            foreach (var property in jsonObject.Properties())
            {
                if (string.Equals(property.Name, "name", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(property.Name, "value", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                approvalCode.AdditionalProperties[property.Name] = property.Value.ToObject<object?>(serializer);
            }

            return approvalCode;
        }

        throw new JsonSerializationException($"Unsupported ApprovalCode token '{reader.TokenType}' at path '{reader.Path}'.");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var approvalCode = value as ApprovalCode;

        if (approvalCode == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("name");
        writer.WriteValue(approvalCode.Name);
        writer.WritePropertyName("value");
        writer.WriteValue(approvalCode.Value);

        foreach (var property in approvalCode.AdditionalProperties)
        {
            writer.WritePropertyName(property.Key);
            serializer.Serialize(writer, property.Value);
        }

        writer.WriteEndObject();
    }
}

internal sealed class ReturnReasonCompatibilityConverter : JsonConverter
{
    public override bool CanConvert(System.Type objectType)
    {
        return objectType == typeof(ReturnReason);
    }

    public override object ReadJson(JsonReader reader, System.Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null!;
        }

        if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
        {
            return new ReturnReason
            {
                Value = Convert.ToInt32(reader.Value),
                Name = reader.Value?.ToString(),
            };
        }

        if (reader.TokenType == JsonToken.String)
        {
            var raw = reader.Value?.ToString();
            if (int.TryParse(raw, out var numericValue))
            {
                return new ReturnReason
                {
                    Value = numericValue,
                    Name = raw,
                };
            }

            return new ReturnReason
            {
                Name = raw,
            };
        }

        if (reader.TokenType == JsonToken.StartObject)
        {
            var jsonObject = JObject.Load(reader);
            var returnReason = new ReturnReason
            {
                Name = jsonObject["name"]?.Value<string>(),
            };

            var valueToken = jsonObject["value"];
            if (valueToken != null && valueToken.Type != JTokenType.Null)
            {
                returnReason.Value = valueToken.Value<int>();
            }

            foreach (var property in jsonObject.Properties())
            {
                if (string.Equals(property.Name, "name", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(property.Name, "value", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                returnReason.AdditionalProperties[property.Name] = property.Value.ToObject<object?>(serializer);
            }

            return returnReason;
        }

        throw new JsonSerializationException($"Unsupported ReturnReason token '{reader.TokenType}' at path '{reader.Path}'.");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var returnReason = value as ReturnReason;

        if (returnReason == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("name");
        writer.WriteValue(returnReason.Name);
        writer.WritePropertyName("value");
        writer.WriteValue(returnReason.Value);

        foreach (var property in returnReason.AdditionalProperties)
        {
            writer.WritePropertyName(property.Key);
            serializer.Serialize(writer, property.Value);
        }

        writer.WriteEndObject();
    }
}

public partial class Sales_sales_orderClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        AdminJsonCompatibility.RegisterConverters(settings);
    }
}

public partial class Sales_shipmentClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        AdminJsonCompatibility.RegisterConverters(settings);
    }
}

public partial class Sales_paymentClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        AdminJsonCompatibility.RegisterConverters(settings);
    }
}

public partial class Sales_sales_return_orderClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        AdminJsonCompatibility.RegisterConverters(settings);
    }
}

public partial class Sales_return_authorizationClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        AdminJsonCompatibility.RegisterConverters(settings);
    }
}
