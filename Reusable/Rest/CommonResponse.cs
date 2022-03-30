namespace Reusable.Rest;

public class CommonResponse
{
    public bool ErrorThrown { get; set; }
    public string ResponseDescription { get; set; }
    public object? Result { get; set; }
    public object? AdditionalData { get; set; }
    public string? ErrorType { get; set; }

    public CommonResponse(object result)
    {
        ErrorThrown = false;
        ResponseDescription = "";
        Result = result;
    }

    public CommonResponse()
    {
        ErrorThrown = false;
        ResponseDescription = "";
        Result = null;
    }

    public CommonResponse Error(KnownError knownError)
    {
        ErrorThrown = true;
        ResponseDescription = knownError.Message;
        Result = knownError;
        if (knownError.Type == KnownError.TypeError.INCIDENT)
        {
            ErrorType = "INCIDENT";
        }
        else
        {
            ErrorType = "MESSAGE";
        }
        return this;
    }

    public CommonResponse Error(string sError, object result)
    {
        ErrorThrown = true;
        ResponseDescription = sError;
        Result = result;
        ErrorType = "MESSAGE";
        return this;
    }

    public CommonResponse Error(string sError)
    {
        ErrorThrown = true;
        ResponseDescription = sError;
        ErrorType = "MESSAGE";
        return this;
    }

    public CommonResponse Success(object oResult, string sMessage = "OK")
    {
        ErrorThrown = false;
        Result = oResult;
        ResponseDescription = sMessage;
        return this;
    }

    public CommonResponse Success(object oResult, object oAdditionalData)
    {
        ErrorThrown = false;
        Result = oResult;
        ResponseDescription = "OK";
        AdditionalData = oAdditionalData;
        return this;
    }

    public CommonResponse Success(string sMessage = "OK")
    {
        ErrorThrown = false;
        ResponseDescription = sMessage;
        return this;
    }
}

public class ValidationResult
{
    public long EntityId { get; set; }
    public string? EntityKind { get; set; }
    public string? FriendlyIdentifier { get; set; }
    public string? Description { get; set; }
}

public class KnownError : Exception
{
    public KnownError(string message) : base(message)
    {
        Type = TypeError.MESSAGE;
    }

    public KnownError(string message, TypeError type) : base(message)
    {
        Type = type;
    }

    public enum TypeError
    {
        MESSAGE, //Just popup an error message
        INCIDENT //Needs front-end specific handling
    }

    public TypeError Type { get; set; }

}

public class CannotCreateBecauseAlreadyExists : Exception
{
    public object ExistingEntity { get; set; }

    public CannotCreateBecauseAlreadyExists(object ExistingEntity)
    {
        this.ExistingEntity = ExistingEntity;
    }
}
