using System.ComponentModel.DataAnnotations.Schema;

namespace Generator.API.Application
{
    public class Application : BaseEntity
    {
        public string? Name { get; set; }
        public string? GeneratorCommit { get; set; }
        public long? GeneratorId { get; set; }        
        internal MainDefinition? Definition { get; set; }

    }

    public class CreateItem
    {
        public string? Name { get; set; }
        public string? Application { get; set; }
    }

    public class InsertApplication : Application { }

    public class UpdateApplication : Application { }

    public class CreateEntity : CreateItem { }

    public class CreateComponent : CreateItem { }

    public class CreateFrontend : CreateItem { }

    public class CreateGateway : CreateItem
    {
        public string? Entity { get; set; }
    }

    public class CreatePage : CreateItem
    {
        public string? FrontendName { get; set; }
    }
}