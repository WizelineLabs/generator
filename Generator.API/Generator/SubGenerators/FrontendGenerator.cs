namespace Generator.API.Generator.SubGenerators
{
    public class FrontendGenerator
    {
        public FrontendDefinition Parse(FrontendDefinition fromYaml)
        {
            if (fromYaml.DisplayName == null)
                fromYaml.DisplayName = fromYaml.Name;
            return fromYaml;
        }
    }
}
