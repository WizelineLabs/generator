namespace Generator.Definition;

using System.Runtime.Serialization;

public abstract class HasYAML
{
    [IgnoreDataMember]
    internal string? yaml { get; set; }
}