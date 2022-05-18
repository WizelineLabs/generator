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

    public static string CombineWith(this String str,params string [] paths)
    {
        return Path.Combine(paths.Prepend(str).ToArray());
    }
    
}