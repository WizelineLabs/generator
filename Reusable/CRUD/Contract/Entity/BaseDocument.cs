namespace Reusable.CRUD.Contract;

using Reusable.CRUD.Entities;

public abstract class BaseDocument : Trackable
{
    //Optimistic Concurrency:
    [Timestamp]
    public ulong RowVersion { get; set; }

    virtual public string? DocumentStatus { get; set; }

    public string? CheckedoutBy { get; set; }

    [Ignore]
    [NotMapped]
    public List<Revision>? Revisions { get; set; }

    public string? RevisionMessage { get; set; }
}
