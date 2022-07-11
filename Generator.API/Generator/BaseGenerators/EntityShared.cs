using Generator.API.BaseGenerators;
using Reusable.CRUD.JsonEntities;
using YamlDotNet.Serialization;

public abstract class EntityShared : BaseGenerator
{
    public EntityDefinition Entity { get; set; }

    public override string GeneratorName => "Entity";

    public string GetConstructor(EntityDefinition entity)
    {
        var newLine = "\n            ";
        var result = new List<string>();

        var propConfigs = GetPropertiesConfig(entity.Fields!);
        foreach (var config in propConfigs)
        {
            if (config.Type.ToLower() == "datetime" || config.Type.ToLower() == "date")
                result.Add(config.Property + " = DateTimeOffset.Now;");

            if (config.Type.Contains("List"))
                result.Add($"{config.Property} = new {config.Type}();");
        }

        return string.Join(newLine, result);
    }

    public string GetProperties(EntityDefinition entity)
    {
        var newLine = "\n        ";
        var result = new List<string>();

        //Properties:
        var propsConfigs = GetPropertiesConfig(entity.Fields!);
        foreach (var config in propsConfigs)
        {
            if (config.Options.ContainsKey("skip")) continue;

            var toInsert = "";

            if (config.Options.ContainsKey("skip-db"))
                toInsert += $"{newLine}[NotMapped]{newLine}[Ignore]{newLine}";

            if (config.Options.ContainsKey("unique"))
                toInsert += $"[Index(IsUnique = true)]{newLine}";
            if (config.MaxLength != null)
                toInsert += $"[MaxLength({config.MaxLength})]{newLine}";
            if (config.Options.ContainsKey("required"))
                toInsert += $"[Required]{newLine}";

            toInsert += $"public {GetCSharpType(config.Type)} {config.Property} {{ get; set; }}";

            result.Add(toInsert);
        }

        //Relationships:
        //One to Parent:
        entity.Relationships.Parent?.ForEach(rel =>
        {
            var config = GetRelationshipConfig(rel);
            result.Add($"{newLine}[Reference]");
            result.Add($"public {config.Entity} {config.Variable} {{ get; set; }}");
            result.Add($"public long {config.Variable}Id {{ get; set; }}");
        });

        //One to Many:
        entity.Relationships.OneToMany?.ForEach(rel =>
        {
            var config = GetRelationshipConfig(rel, "s");
            result.Add($"{newLine}[Reference]");
            result.Add($"public List<{config.Entity}> {config.Variable}s {{ get; set; }}");
        });

        //Many to One:
        entity.Relationships.ManyToOne?.ForEach(rel =>
        {
            var config = GetRelationshipConfig(rel);
            result.Add($"{newLine}[Reference]");
            result.Add($"public {config.Entity} {config.Variable} {{ get; set; }}");
            result.Add($"public long? {config.Variable}Id {{ get; set; }}");
        });

        return string.Join(newLine, result).Trim();
    }

    public RelationshipConfig GetRelationshipConfig(string relationship, string suffix = "")
    {
        var config = new RelationshipConfig();
        FromPipeOptions(relationship, options =>
        {
            foreach (var option in options)
            {
                if (new[] { "skip-db", "required" }.ToList().Contains(option))
                    config.Options[option] = true;
                else
                {
                    FromAlias(option, (prop, alias, arr) =>
                    {
                        config.Entity = arr[0];
                        config.Alias = alias + suffix;
                        config.Variable = alias.Replace(" ", "");
                    });
                }
            }
        });

        return config;
    }

    public class RelationshipConfig
    {
        public Dictionary<string, bool> Options { get; set; }
        public string Entity { get; set; }
        public string Alias { get; set; }
        public string Variable { get; set; }
    }

    public string GetCSharpType(string type)
    {
        if (type == null) return "";

        var lowerType = type.ToLower();

        switch (lowerType)
        {
            case "string":
            case "string-identifier":
            case "text":
            case "text-unique":
            case "textarea":
            case "wig":
            case "email":
            case "attachment":
            case "image":
            case "phone":
            case "password":
            case "json":
                return "string";

            case "number":
            case "int":
                return "int";

            case "number?":
            case "int?":
                return "int?";

            case "long":
                return "long";

            case "long?":
                return "long?";

            case "double":
                return "double";

            case "double?":
                return "double?";

            case "currency":
            case "float":
            case "decimal":
                return "decimal";

            case "currency?":
            case "float?":
            case "decimal?":
                return "decimal?";

            case "date":
            case "datetime":
                return "DateTimeOffset";

            case "date?":
            case "datetime?":
                return "DateTimeOffset?";

            case "checkbox":
            case "boolean":
            case "bool":
                return "bool";

            case "checkbox?":
            case "boolean?":
            case "bool?":
                return "bool?";

            case "binary":
                return "byte[]";

            default:
                return type;
        }
    }

    public List<PropConfig> GetPropertiesConfig(Dictionary<string, string> Fields)
    {
        var result = new List<PropConfig>();
        if (Fields == null) return result;

        foreach (var field in Fields)
        {
            var config = new PropConfig();
            FromAlias(field.Key, (property, alias, arr) =>
            {
                config.Property = property;
                config.Alias = alias;
                config.PropertyAsDefined = arr[0];
            });

            FromPipeOptions(field.Value, options =>
            {
                config.Type = options[0];

                foreach (var opt in options)
                {
                    if (PropConfig.AvailableOptions.Contains(opt))
                        config.Options[opt] = true;
                    else if (int.TryParse(opt, out int maxLength))
                        config.MaxLength = maxLength;
                }

                foreach (var type in AvailablePropTypes)
                {
                    if (options.Contains(type))
                    {
                        config.Type = type;
                        break;
                    }
                }
            });

            result.Add(config);
        }

        return result;
    }

    public class PropConfig
    {
        public string Property { get; set; }
        public string Alias { get; set; }
        public string Type { get; set; }
        public string PropertyAsDefined { get; set; }
        public Dictionary<string, bool> Options = new Dictionary<string, bool>();
        public int? MaxLength { get; set; }
        public static List<string> AvailableOptions = new[] { "skip", "skip-db", "required", "unique" }.ToList();
    }

    public static List<string> AvailablePropTypes = new List<string>
        {
            "attachment",
            "binary",
            "bool",
            "bool?",
            "boolean",
            "boolean?",
            "checkbox",
            "checkbox?",
            "currency",
            "currency?",
            "date",
            "date?",
            "datetime",
            "datetime?",
            "decimal",
            "decimal?",
            "double",
            "double?",
            "email",
            "float",
            "float?",
            "image",
            "int",
            "int?",
            "json",
            "long",
            "long?",
            "number",
            "number?",
            "password",
            "phone",
            "string",
            "string-identifier",
            "text",
            "text-unique",
            "textarea",
            "wig"
        };

    protected EntityShared(GeneratorContext DbContext, ILog logger, IConfiguration configuration) : base(DbContext, logger, configuration)
    {
    }

    public EntityDefinition GetDefinition(EntityDefinition entity)
    {
        var cacheKey = $"GetDefinition_App:{Application!.Name}_Entity:{entity.Name}";
        var cache = Cache.Get<EntityDefinition>(cacheKey);
        if (cache != null)
            return cache;

        var directoryPath = APPLICATIONS_DIRECTORY.CombineWith(Application.Name, "definition/entities");
        var path = directoryPath.CombineWith(entity.Name);

        if (File.Exists(path))
            entity = YAML.DeserializeYAML<EntityDefinition>(path);
        else
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(entity);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            File.WriteAllText(path, yaml);
        }

        Cache.Set(cacheKey, entity);
        return entity;
    }

}

