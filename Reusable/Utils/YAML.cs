using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;

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

            // var serializer = YamlSerializer.Default;

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

        public class SortedTypeInspector : TypeInspectorSkeleton
        {
            private readonly ITypeInspector _innerTypeInspector;

            public SortedTypeInspector(ITypeInspector innerTypeInspector)
            {
                _innerTypeInspector = innerTypeInspector;
            }

            public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
            {
                return _innerTypeInspector.GetProperties(type, container).OrderBy(x => x.Name);
            }
        }

        public static class YamlSerializer
        {
            public static readonly Serializer Default =
                (Serializer)new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
                                       .WithTypeInspector(x => new SortedTypeInspector(x))
                                       .Build();
        }

    }
}
