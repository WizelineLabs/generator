using Reusable.Rest;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace Reusable.Utils
{
    public class SortedTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _innerTypeInspector;

        public SortedTypeInspector(ITypeInspector innerTypeInspector) => _innerTypeInspector = innerTypeInspector;

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            var properties = _innerTypeInspector.GetProperties(type, container).OrderBy(x => x.Order);
            List<IPropertyDescriptor> orderedProperties = new();
            foreach(var t in type.GetAllFields())
            {
                var propertyName = t.Name.IndexOf('>') > -1 ? t.Name[1..t.Name.IndexOf('>')].ToCamelCase() : t.Name.ToCamelCase();
                orderedProperties.Add(properties.First(x => x.Name == propertyName));
            }

            // Shouldn't happen but just in case
            if ( properties.Count() != orderedProperties.Count )
            {
                throw new KnownError("Missing property at serialized.");
            }

            return orderedProperties;
        }
    }
}