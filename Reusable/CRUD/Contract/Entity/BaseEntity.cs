namespace Reusable.CRUD.Contract;

using ServiceStack.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public abstract class BaseEntity : IEntity
{
    [AutoIncrement]
    public virtual long Id { get; set; }

    [Ignore]
    [NotMapped]
    public string EntityName { get { return GetType().Name; } }

    [Ignore]
    [NotMapped]
    public EntryState Entry_State { get; set; }
}
