using Reusable.CRUD.Implementations.EF;
using Reusable.Utils;

namespace Generator.API.Application;

public class ApplicationLogic : WriteLogic<Application>, ILogicWriteAsync<Application>
{
    public ApplicationLogic(GeneratorContext DbContext, Log<ApplicationLogic> logger) : base(DbContext, logger)
    {
    }
}