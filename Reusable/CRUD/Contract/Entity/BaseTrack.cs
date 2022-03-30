namespace Reusable.CRUD.Contract;

public abstract class Trackable : BaseCatalog
{
    public Trackable()
    {
        CreatedAt = DateTimeOffset.Now;
    }

    virtual public bool IsDeleted { get; set; }

    virtual public DateTimeOffset CreatedAt { get; set; }
    virtual public DateTimeOffset? UpdatedAt { get; set; }
    virtual public DateTimeOffset? RemovedAt { get; set; }
    virtual public DateTimeOffset? UsedAt { get; set; }

    virtual public string? CreatedBy { get; set; }
    virtual public string? UpdatedBy { get; set; }
    virtual public string? RemovedBy { get; set; }
    virtual public string? AssignedTo { get; set; }
    virtual public string? AssignedBy { get; set; }
}
