namespace Generator.Definition;

using System.Runtime.Serialization;

public abstract class HasYAML
{
    [IgnoreDataMember]
    public string? yaml { get; set; }
}