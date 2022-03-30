namespace Reusable.CRUD.Entities;

using Reusable.CRUD.Contract;

public class Revision : BaseDocument
{
    public string? ForeignType { get; set; }
    public long ForeignKey { get; set; }
}
