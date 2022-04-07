namespace Generator.Definition;

public class EntityDefinition : HasYAML
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Table { get; set; }
    public Dictionary<string, string>? Fields { get; set; }
    public RelationshipsDefinition Relationships { get; set; } = new RelationshipsDefinition();
    public EntitySlots Slots { get; set; } = new EntitySlots();

    public EntryState EntryState { get; set; }
}

public class RelationshipsDefinition
{
    public List<string> Parent { get; set; } = new List<string>();
    public List<string> OneToMany { get; set; } = new List<string>();
    public List<string> ManyToOne { get; set; } = new List<string>();
}

public class EntitySlots
{
    public ModelSlots Model { get; set; } = new ModelSlots();
    public LogicSlots Logic { get; set; } = new LogicSlots();
    public GatewaySlots Gateway { get; set; } = new GatewaySlots();
    public ServiceSlots Service { get; set; } = new ServiceSlots();
}

public class ModelSlots
{
    public string? Imports { get; set; }
    public string? Extends { get; set; }
    public string? Ctor { get; set; }
    public string? Body { get; set; }
}

public class LogicSlots
{
    public string? Imports { get; set; }
    public string? Init { get; set; }
    public string? OnCreateInstance { get; set; }
    public string? OnGetList { get; set; }
    public string? OnGetSingle { get; set; }
    public string? BeforeSave { get; set; }
    public string? AfterSave { get; set; }
    public string? BeforeRemove { get; set; }
    public string? AdapterOut { get; set; }
    public string? Body { get; set; }
}

public class GatewaySlots
{
    public string? Imports { get; set; }
    public string? Endpoints { get; set; }
    public string? EndpointsRoutes { get; set; }
}

public class ServiceSlots
{
    public string? Dependencies { get; set; }
    public string? AdapterIn { get; set; }
    public string? AdapterOut { get; set; }
    public string? Body { get; set; }
}