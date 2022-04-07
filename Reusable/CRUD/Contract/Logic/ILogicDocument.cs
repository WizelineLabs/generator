namespace Reusable.CRUD.Contract;

public interface IDocumentLogic<Entity> : ILogicWrite<Entity> where Entity : BaseDocument, new()
{
    void MakeRevision(Entity document);
    Entity UpdateAndMakeRevision(Entity entity);

    void Checkout(long id);
    void Checkin(Entity document);
    void CancelCheckout(long id);
    Entity CreateAndCheckout(Entity document);
}

public interface IDocumentLogicAsync<Entity> : ILogicWriteAsync<Entity> where Entity : BaseDocument, new()
{
    Task MakeRevisionAsync(Entity document);
    Task<Entity> UpdateAndMakeRevisionAsync(Entity entity);

    Task CheckoutAsync(long id);
    Task CheckinAsync(Entity document);
    Task CancelCheckoutAsync(long id);
    Task<Entity> CreateAndCheckoutAsync(Entity document);
}
