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

    public class InsertApplication : Application { }

    public class UpdateApplication : Application { }
}