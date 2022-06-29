using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Reusable.Utils
{
    public class YAML
    {
        public static T DeserializeYAML<T>(string yamlName)
        {
            using (StreamReader sr = new StreamReader(yamlName))
            {
                var content = sr.ReadToEnd();
                var input = new StringReader(content);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
                return deserializer.Deserialize<T>(input);
            }
        }

        public static string ToYaml(object from)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return serializer.Serialize(from);
        }

        public static string ParseYML(object from)
        {
            string yaml = ToYaml(from);
            var lines = yaml.Split(new char[] { '\n' })
                .Where(x => !x.StartsWith("entryState:"))
                .ToList();

            return string.Join('\n', lines);
        }

    }
}
