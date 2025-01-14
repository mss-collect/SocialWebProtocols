using System.Text.Json;
using System.Text.Json.Serialization;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityStreams.Serialization;

/// <summary>
/// Converter for the <see cref="ASLink"/> base class.
/// </summary>
public class ASLinkConverter : JsonConverter<ASLink>
{
    /// <inheritdoc/>
    public override ASLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Tried to parse ASLink item that didn't start with a StartObject!");

        var obj = JsonDocument.ParseValue(ref reader);
        if (!obj.RootElement.TryGetProperty("type", out var typeProp))
            throw new JsonException("ASLink has no type!");

        return typeProp.GetString()! switch
        {
            "Image" => obj.Deserialize<ASImage>(options: options),
            "Document" or "Link" => obj.Deserialize<ASDocument>(options: options),
            "Mention" => obj.Deserialize<ASMention>(options: options),
            "PropertyValue" => obj.Deserialize<ASPropertyValue>(options: options),
            "Hashtag" => obj.Deserialize<ASHashtag>(options: options),
            "Emoji" => obj.Deserialize<ASEmoji>(options: options),
            
            _ => new ASLink()
            {
                Type = typeProp.GetString()!
            }
        };
    }
    
    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ASLink value, JsonSerializerOptions options)
    {
        // Skip writing values we don't know about.
        if (value.GetType() == typeof(ASLink))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue(value.Type);
            writer.WriteEndObject();
            return;
        }
        
        var obj = JsonSerializer.SerializeToElement(value, value.GetType());
        obj.WriteTo(writer);
    }
}