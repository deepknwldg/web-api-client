using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Xml.Serialization;

namespace WebApiClient;

public static class SerializeFunctions
{
    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = true,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        return options;
    }

    public static string SerializeToXmlAsString(object obj, string nameSpace = "")
    {
        var bytes = SerializeToXmlAsBytes(obj, nameSpace);
        return Encoding.UTF8.GetString(bytes);
    }

    public static byte[] SerializeToXmlAsBytes(object obj, string nameSpace = "")
    {
        var serializer = new XmlSerializer(obj.GetType(), nameSpace);
        using var stream = new MemoryStream();

        using var sw = new StreamWriter(stream, new UTF8Encoding(false)); // БЕЗ BOM
        serializer.Serialize(sw, obj);

        return stream.ToArray();
    }

    public static string SerializeToJson(object obj)
    {
        var options = GetJsonSerializerOptions();
        var serializeObj = JsonSerializer.SerializeToUtf8Bytes(obj, options);

        return Encoding.UTF8.GetString(serializeObj);
    }

    public static T DeserializeFromXml<T>(string xmlObject, string nameSpace = "")
    {
        return DeserializeFromBytes<T>(Encoding.UTF8.GetBytes(xmlObject), nameSpace);
    }

    public static T DeserializeFromBytes<T>(byte[] bytesObject, string nameSpace = "")
    {
        var serializer = new XmlSerializer(typeof(T), nameSpace);
        using var stream = new MemoryStream(bytesObject);
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        var obj = serializer.Deserialize(streamReader);

        return (T) obj;
    }

    public static T DeserializeJsonFromBytes<T>(byte[] bytesObject)
    {
        var serializeObj = new ReadOnlySpan<byte>(bytesObject);
        return JsonSerializer.Deserialize<T>(serializeObj);
    }

    public static T DeserializeFromJson<T>(string jsonObject)
    {
        if (string.IsNullOrWhiteSpace(jsonObject))
        {
            return default;
        }

        var options = GetJsonSerializerOptions();
        var serializeObj = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(jsonObject));

        return JsonSerializer.Deserialize<T>(serializeObj, options);
    }
}