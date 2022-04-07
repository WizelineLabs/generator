namespace Reusable.CRUD.JsonEntities;

public static class Extensions
{
    public static Contact ToContact(this String str)
    {
        return new Contact
        {
            Email = str,
            Value = str,
            DisplayName = str
        };
    }
}