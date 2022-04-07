namespace Reusable.CRUD.Implementations.SS;
// namespace Reusable.CRUD.Contract;

using Reusable.CRUD.Entities;

//Cannot inherit from DocumentLogic because causes a circular reference
public class RevisionLogic : WriteLogic<Revision>
{
    public override void Init(IDbConnection db, Service service)
    {
        base.Init(db, service);
    }

    #region Overrides
    public override Revision Add(Revision entity)
    {
        entity.CreatedAt = DateTimeOffset.Now;
        entity.CreatedBy = Auth?.UserName;

        return base.Add(entity);
    }

    public override async Task<Revision> AddAsync(Revision entity)
    {
        entity.CreatedAt = DateTimeOffset.Now;
        entity.CreatedBy = Auth?.UserName;

        return await base.AddAsync(entity);
    }
    #endregion

    #region Specific Operations
    public List<Revision> GetRevisionsForEntity(long ForeignKey, string ForeignType)
    {
        // var query = Db.From<Revision>()
        //     .Where(e => e.ForeignKey == ForeignKey && e.ForeignType == ForeignType)
        //     .OrderByDescending(e => e.CreatedAt);

        // return Db.LoadSelect(query).ToList();
        return new List<Revision>(); // TODO
    }
    #endregion
}
