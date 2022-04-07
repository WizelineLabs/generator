using Reusable.CRUD.Implementations.EF;
using Microsoft.EntityFrameworkCore;

namespace Generator.API.Application;

public class ApplicationLogic : WriteLogic<Application>, ILogicWriteAsync<Application>
{
    public ApplicationLogic(GeneratorContext DbContext) : base(DbContext)
    {
    }
}