namespace Reusable.CRUD.Contract;

// using ServiceStack.Web;
using Reusable.EmailServices;

public interface ILogic
{
    // IRequest Request { get; set; } ServiceStack Request
    // IDbConnection Db { get; set; }
    // void Init(IDbConnection db, Service service);
    IEmailService? EmailService { get; set; }
}
