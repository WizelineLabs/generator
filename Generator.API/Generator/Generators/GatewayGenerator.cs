using Generator.API.BaseGenerators;
using Reusable.CRUD.JsonEntities;

public class GatewayGenerator : EntityShared
{
    public GatewayDefinition Gateway { get; set; }

    public override string GeneratorName => "Gateway";

    private Dictionary<string, Archive> Templates;

    //public GatewayDefinition Parse(Dictionary<string, object> obj, string from)
    //{
    //    var json = JsonObject.Parse(from);
    //    var r = new GatewayDefinition
    //    {
    //        Name = json.Get("Name"),
    //        Entity = json.Get("Entity"),
    //        Dtos = new List<Dto>()
    //    };

    //    var stringItemsIndexes = new List<int>();

    //    json.Get<List<string>>("Dtos").Each((index, item) =>
    //    {
    //        if (item.StartsWith("{")) return;
    //        stringItemsIndexes.Add(index);

    //        var dtoName = GetDtoName(item);

    //        var dto = new Dto
    //        {
    //            Name = dtoName,
    //            Def = item,
    //            Routes = new List<DtoRoute> {
    //                            new DtoRoute {
    //                                HttpVerb = GetHttpVerb(item),
    //                                Path = GetPath(item)
    //                            }
    //                }
    //        };
    //        r.Dtos.Add(dto);
    //    });

    //    json.ArrayObjects("Dtos").Each((index, item) =>
    //    {
    //        if (stringItemsIndexes.Contains(index)) return;
    //        var dto = new Dto
    //        {
    //            Request = item.Object("Request"),
    //            Response = item.Object("Response")
    //        };
    //        item.Each(it =>
    //        {
    //            switch (it.Key)
    //            {
    //                case "Request":
    //                case "Response":
    //                    break;
    //                default:
    //                    dto.Routes = new List<DtoRoute>();
    //                    item.Get<List<string>>(it.Key).Each(route =>
    //                    {
    //                        dto.Routes.Add(new DtoRoute
    //                        {
    //                            HttpVerb = GetHttpVerb(route),
    //                            Path = GetPath(route)
    //                        });
    //                    });
    //                    dto.Name = GetDtoName(it.Key);
    //                    dto.Def = it.Key; // TODO
    //                    break;
    //            }
    //        });

    //        r.Dtos.Add(dto);
    //    });

    //    return r;
    //}

    public void SetApplication(Application app)
    {
        Application = app;
        MainDefinition = app.Definition;                
        Templates = new Dictionary<string, Archive>
            {
                {"gateway", new Archive { Content = File.ReadAllText(Configuration.GetValue<string>("GATEWAY_TEMPLATES_DIR"))} },
                {"dtos", new Archive { Content = File.ReadAllText(Configuration.GetValue<string>("GATEWAY_DTOS_TEMPLATES_DIR"))} }
            };
    }

    public void Setup(GatewayDefinition gateway, EntityDefinition entity)
    {
        Gateway = gateway;
        Entity = entity;

        Variables = new Dictionary<string, string>
            {
                { "entityName", entity.Name }
            };
    }

    override public List<Archive> Run(bool force = false)
    {
        if (string.IsNullOrWhiteSpace(Gateway.Name))
            throw new Exception("Invalid Gateway.Name");

        Log.Info($"Running Gateway: [{Gateway.Name}]");

        var result = new List<Archive>
            {
                GenerateGateway(Gateway, Entity, force)
            };

        return result;
    }

    public Archive GetEntityArchive(string toPath, string fileName, string type, ArchiveComparisionResult comparisionResult = ArchiveComparisionResult.Added)
    {
        var f = new Archive(MainDefinition!.ProjectName, type, null!, GetRelativePath(toPath, APPLICATIONS_DIRECTORY.CombineWith(Application!.Name), fileName), fileName, comparisionResult);
        f.RightPath = toPath;
        return f;
    }

    public Archive GenerateArchive(GatewayDefinition gateway, EntityDefinition entity, string templateName, string relativePath, string fileName, bool force = false, Dictionary<string, string> Variables = null)
    {
        var template = Templates[templateName];
        var directoryPath = APPLICATIONS_DIRECTORY.CombineWith(Application!.Name, relativePath, entity.Name);
        var toPath = directoryPath.CombineWith(fileName);

        if (Variables != null)
            foreach (var (key, value) in Variables)
                this.Variables.Add(key, value);

        var content = InterpolateVariables(template.Content!, new Dictionary<string, object> { { "entity", entity }, { "gateway", gateway } });

        var f = GetEntityArchive(toPath, fileName, ToVariable(templateName));

        var fileInfo = new FileInfo(toPath);
        if (fileInfo.Exists)
        {
            f.Generator = "generator-ssr";

            var diffModel = CompareContent(content, File.ReadAllText(toPath), f, fileInfo.Extension, force);
            //f.Diff = ConflictLogic.ResolveConflicts(diffModel, f, force);
            f.ComparisionResult = f.ComparisionResult.Distinct().ToList();
            WriteFile(f.Content, toPath, fileName);
        }
        else
            WriteFile(content, toPath, fileName, true);

        return f;
    }

    public Archive GenerateGateway(GatewayDefinition gateway, EntityDefinition entity, bool force = false)
    {
        Log.Info($"Generating Gateway: [{gateway.Name}]");
        var (dtos, services) = GetDtos(gateway);

        return GenerateArchive(
            gateway,
            entity,
            gateway.Dtos.Count > 0 ? "dtos" : "gateway",
            "backend/MyApp.API",
            gateway.Name + "Service.cs",
            force,
            new Dictionary<string, string> {
                    { "dtos", dtos},
                    { "services", services},
            });
    }

    List<string> predefinedDtos = new List<string> {
            // Post
            "Post", "Create", "Insert", "Add", "DirectInsert",
            "CreateInstance",
            // Patch
            "Patch",
            // Put
            "Put", "Update",
            // Delete
            "Delete", "DeleteById",
            // Get
            "GetById", "ReadById",
            "GetAll", "ReadAll", "Get", "Read",
            "GetSingleWhere",
            "Query", "GetPaged", "AutoQuery",
            "Seed"
        };

    public GatewayGenerator(GeneratorContext DbContext, ILog logger, IConfiguration configuration) : base(DbContext, logger, configuration)
    {
    }

    public GatewayDefinition Parse(GatewayDefinition fromYaml)
    {
        return fromYaml;
    }
    public (string, string) GetDtos(GatewayDefinition gateway)
    {
        var requests = new List<string>();
        var services = new List<string>();
        var newLine = "\n        ";

        foreach (var item in gateway.Dtos)
        {
            var (requestFields, responseFields) = GetDtosProperties(item);

            var lastHttpVerb = item.Routes[0].HttpVerb;
            // if (item.Routes != null && item.Routes.Count > 0)
            // {
            //     lastHttpVerb = item.Routes[0].HttpVerb;
            //     // var firstRoute = item.Routes[0];
            //     // var splitRoute = firstRoute.Split(' ');
            //     // lastHttpVerb = GetHttpVerb(splitRoute[0]);
            // }
            // else
            //     lastHttpVerb = GetHttpVerb(splitName[0]);

            #region Request DTO
            var toInsertRequests = "";
            toInsertRequests += GetDtoRoutes(item) + newLine;
            toInsertRequests += GetDtoRequest(item.Name!, item.Def!, requestFields);
            requests.Add(toInsertRequests);
            #endregion

            #region Response DTO
            var toInsertResponses = GetDtoResponse(item, item.Name!, item.Def!, responseFields);
            requests.Add(toInsertResponses);
            #endregion

            #region Services
            var toInsertServices = GetDtoService(item.Name!, item.Def!);
            services.Add(toInsertServices);
            #endregion
        }

        return (
            string.Join(newLine,requests).Trim(),
            string.Join(newLine, services).Trim()
        );
    }

    public string GetDtoRoutes(Dto dto)
    {
        var newLine = "\n        ";

        if (predefinedDtos.Contains(dto.Name!))
        {
            var path = "";
            switch (dto.Name)
            {
                case "CreateInstance":
                    path = "/CreateInstance";
                    break;
                case "DeleteById":
                    path = "/{Id}";
                    break;
                case "Query":
                    path = "/Query";
                    break;
                case "AutoQuery":
                    path = "/AutoQuery";
                    break;
                case "GetById":
                case "ReadById":
                    path = "{Id}";
                    break;
                case "GetSingleWhere":
                    var getSingleWhereRoute = "";
                    path = "/GetSingleWhere";
                    getSingleWhereRoute += $"[Route(\"/{Entity.Name}{path}\", \"{GetHttpVerb(dto.Name).ToUpper()}\")]{newLine}";
                    path = "/GetSingleWhere/{Property}/{Value}";
                    getSingleWhereRoute += $"[Route(\"/{Entity.Name}{path}\", \"{GetHttpVerb(dto.Name).ToUpper()}\")]";
                    return getSingleWhereRoute;
                case "GetPaged":
                    path = "/GetPaged/{Limit}/{Page}";
                    break;
                case "Seed":
                    path = "/CurrentDataToDBSeed";
                    break;
                case "DirectInsert":
                    path = "/DirectInsert";
                    break;
                default:
                    path = "";
                    break;
            }
            return $"[Route(\"/{Entity.Name}{path}\", \"{GetHttpVerb(dto.Name!).ToUpper()}\")]";
        }

        var routes = "";
        foreach (var route in dto.Routes)
            routes += GetRoute(route);

        return routes;
    }

    public string GetRoute(DtoRoute route)
    {
        return $"[Route(\"/{Entity.Name}/{route.Path}\", \"{route.HttpVerb!.ToUpper()}\")]";
    }

    public string GetDtoName(string from)
    {
        var dtoName = from.Split('/')[0].Split(':')[0];
        var splitName = dtoName.Split(' ');
        if (splitName.Count() > 1)
            dtoName = string.Join("", splitName[1]);
        return dtoName;
    }

    public string GetHttpVerb(string from)
    {
        var dtoName = from.Split('/')[0].Split(':')[0];
        var splitName = dtoName.Split(' ');
        var result = "";
        switch (splitName[0].ToLower())
        {
            case "post":
            case "create":
            case "insert":
            case "add":
            case "createinstance":
            case "directinsert":
                result = "Post";
                break;
            case "patch":
                result = "Patch";
                break;
            case "put":
            case "update":
                result = "Put";
                break;
            case "delete":
            case "deleteById":
                result = "Delete";
                break;
            case "Get":
            case "GetById":
            case "GetAll":
            case "Read":
            case "ReadById":
            case "ReadAll":
            case "Query":
            case "autoQuery":
            case "seed":
            default:
                result = "Get";
                break;
        }
        return result;
    }

    public string GetPath(string from)
    {
        var path = from;
        var split = from.Split(' ');
        if (split.Length > 1)
            path = String.Join("", split[1]);
        return path;
    }

    public string GetDtoRequest(string dtoName, string def, string requestFields)
    {
        var newLine = "\n        ";     
        switch (def)
        {
            case "Post":
            case "Create":
            case "Insert":
            case "Add":
                return $"public class Insert{Entity.Name} : {Entity.Name} {{{requestFields}{newLine}}}{newLine}";
            case "DirectInsert":
                return $"public class {Entity.Name}DirectInsert : {Entity.Name} {{{requestFields}{newLine}}}{newLine}";
            case "CreateInstance":
                return $"public class Create{Entity.Name}Instance : {Entity.Name} {{{requestFields}{newLine}}}{newLine}";
            case "Patch":
                return $"public class Patch{Entity.Name} : {Entity.Name} {{{requestFields}{newLine}}}{newLine}";
            case "Put":
            case "Update":
                return $"public class Update{Entity.Name} : {Entity.Name} {{{requestFields}{newLine}}}{newLine}";
            case "Delete":
                return $"public class Delete{Entity.Name} : {Entity.Name} {{{requestFields}{newLine}}}{newLine}";
            case "DeleteById":
                return $"public class DeleteById{Entity.Name} : {Entity.Name} {{{requestFields}{newLine}}}{newLine}";
            case "Query":
                return $@"public class Query{Entity.Name}s : QueryDb<{Entity.Name}> {{
                            public string FilterGeneral {{ get; set; }}
                            public int Limit {{ get; set; }}
                            public int Page {{ get; set; }}

                            public bool RequiresKeysInJsons {{ get; set; }}
                            {requestFields}
                        }}{newLine}";
            case "AutoQuery":
                return $"public class {Entity.Name}AutoQuery : QueryDb<{Entity.Name}> {{{requestFields}{newLine}}}{newLine}";
            case "GetById":
            case "ReadById":
                return $"public class Get{Entity.Name}ById : GetSingleById<{Entity.Name}> {{{requestFields}{newLine}}}{newLine}";
            case "GetAll":
            case "ReadAll":
                return $"public class GetAll{Entity.Name}s : GetAll<{Entity.Name}> {{{requestFields}{newLine}}}{newLine}";
            case "GetSingleWhere":
                return $"public class Get{Entity.Name}Where : GetSingleWhere<{Entity.Name}> {{{requestFields}{newLine}}}{newLine}";
            case "GetPaged":
                return $@"public class GetPaged{Entity.Name}s : QueryDb<{Entity.Name}> {{
                            public string FilterGeneral {{ get; set; }}
                            public int Limit {{ get; set; }}
                            public int Page {{ get; set; }}

                            public bool RequiresKeysInJsons {{ get; set; }}
                            {requestFields}
                        }}{newLine}";
            case "Seed":
                return $"public class CurrentDataToDBSeed{Entity.Name}s {{}}{newLine}";
            case "Get":
            case "Read":
            default:
                return $"public class {dtoName} {{{requestFields}{newLine}}}{newLine}";
        }



    }

    public string GetDtoResponse(Dto item, string dtoName, string def, string responseFields)
    {
        var newLine = "\n        ";
        if (item.Response == null)
        {
            if (predefinedDtos.Contains(def))
            {
                switch (def)
                {
                    case "Post":
                    case "Create":
                    case "Insert":
                    case "Add":
                        if (string.IsNullOrWhiteSpace(responseFields))
                            return $@"public class Insert{Entity.Name}Response
                                {{
                                    public {Entity.Name} Result {{ get; set; }}
                                    public ResponseStatus ResponseStatus {{ get; set; }}
                                }}";
                        else
                        {
                            return $@"public class Insert{Entity.Name}Response
                                {{
                                    {responseFields}
                                    public ResponseStatus ResponseStatus {{ get; set; }}
                                }}";
                        }
                    case "DirectInsert":
                    case "CreateInstance":
                    case "Patch":
                    case "Put":
                    case "Update":
                    case "Delete":
                    case "DeleteById":
                    case "Query":
                    case "AutoQuery":
                    case "GetById":
                    case "ReadById":
                    case "GetAll":
                    case "ReadAll":
                    case "GetSingleWhere":
                    case "GetPaged":
                    case "Seed":
                    case "Get":
                    case "Read":
                    default:
                        return "";
                }
            }
        }
        else
        {
            var result = "";
            result += $"public class {dtoName}Response {newLine}{{{newLine}";
            if (item.Response.Count > 0)
                result += $"{responseFields}{newLine}}}";
            else
                result += $"public string Result {{ get; set; }}{newLine}}}";

            return result;
        }
        return "";
    }

    public string GetDtoService(string dtoName, string def)
    {
        var newLine = "\n        ";
        if (predefinedDtos.Contains(def))
        {
            switch (def)
            {
                case "Post":
                case "Create":
                case "Insert":
                case "Add":
                    return $@"public object Post(Insert{Entity.Name} request)
                        {{
                            return InTransaction(db => Logic.Add(request),
                                result => new Insert{Entity.Name}Response {{ Result = result as {Entity.Name} }});
                        }}";
                case "DirectInsert":
                    return $@"public object Post({Entity.Name}DirectInsert request)
                        {{
                            var result = Db.Insert(({Entity.Name})request);
                            if (result > 0)
                                return new CommonResponse();

                            throw new Exception(""Could not insert: "" + request.BillTo);
                        }}";
                case "CreateInstance":
                    return $@"public object Post(Create{Entity.Name}Instance request)
                        {{
                            return WithDb(db =>
                            {{
                                return new HttpResult(new CommonResponse(Logic.CreateInstance(request)))
                                {{
                                    ResultScope = () => JsConfig.With(new Config {{ IncludeNullValues = true }})
                                }};
                            }});
                        }}";
                case "Patch":
                    return "";
                case "Put":
                case "Update":
                    return $@"public object Put(Update{Entity.Name} request)
                        {{
                            InTransaction(db => Logic.Update(request));
                            return WithDb(db => new CommonResponse(Logic.GetById(request.Id)));
                        }}";
                case "Delete":
                    return $@"public object Delete(Delete{Entity.Name} request)
                        {{
                            return InTransaction(db =>
                            {{
                                Logic.Remove(request);
                                return new CommonResponse();
                            }});
                        }}";
                case "DeleteById":
                    return $@"public object Delete(DeleteById{Entity.Name} request)
                        {{
                            return InTransaction(db =>
                            {{
                                Logic.RemoveById(request.Id);
                                return new CommonResponse();
                            }});
                        }}";
                case "Query":
                    return $@"public async Task<object> Get(Query{Entity.Name}s request)
                        {{
                            var query = AutoQuery.CreateQuery(request, Request);

                            if (!request.FilterGeneral.IsNullOrEmpty())
                            {{
                                var splitGeneralFilter = request.FilterGeneral
                                    .Split(new string[] {{ "" "" }}, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(e => e.Trim().ToUpper());

                                {getQueryServiceCriteria(Entity)}
                            }}

                            return await WithDbAsync(async db => await Logic.GetPagedAsync(
                                request.Limit,
                                request.Page,
                                null,
                                query,
                                requiresKeysInJsons: false
                                ));
                        }}";
                case "AutoQuery":
                    return "";
                case "GetById":
                case "ReadById":
                    return $@"public object Get(Get{Entity.Name}ById request)
                        {{
                            return WithDb(db => Logic.GetById(request.Id));
                        }}";
                case "GetAll":
                case "ReadAll":
                    return $@"public object Get(GetAll{Entity.Name}s request)
                        {{
                            return WithDb(db => Logic.GetAll());
                        }}";
                case "GetSingleWhere":
                    return $@"public object Get(Get{Entity.Name}Where request)
                        {{
                            return WithDb(db => Logic.GetSingleWhere(request.Property, request.Value));
                        }}";
                case "GetPaged":
                    return $@"public async Task<object> Get(GetPaged{Entity.Name}s request)
                        {{
                            var query = AutoQuery.CreateQuery(request, Request);

                            if (!request.FilterGeneral.IsNullOrEmpty())
                            {{
                                var splitGeneralFilter = request.FilterGeneral
                                    .Split(new string[] {{ "" "" }}, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(e => e.Trim().ToUpper());

                                {getQueryServiceCriteria(Entity)}
                            }}

                            return await WithDbAsync(async db => await Logic.GetPagedAsync(
                                request.Limit,
                                request.Page,
                                null,
                                query,
                                requiresKeysInJsons: false
                                ));
                        }}";
                case "Seed":
                    return $@"public object Get(CurrentDataToDBSeed{Entity.Name}s request)
                        {{
                            WithDb(db => Logic.CurrentDataToDBSeed());
                            return new CommonResponse().Success();
                        }}";
                case "Get":
                case "Read":
                    return "";
                default:
                    return "";
            }
        }
        else
        {
            var result = "";
            result += $"public object Any({dtoName} request){{{newLine}";
            result += $"return WithDb(db => Logic.{dtoName}());}}{newLine}";
            return result;
        }
    }

    public string getQueryServiceCriteria(EntityDefinition entity)
    {
        var newLine = "\n        ";
        var stringFields = GetPropertiesConfig(entity.Fields!).Where(prop => GetCSharpType(prop.Type) == "string");
        if (stringFields.Count() > 0)
        {
            List<string> result = new List<string>();
            foreach (var propConfig in stringFields)
            {
                result.Add($"e.{propConfig.Property}.ToUpper().Contains(criteria)");
            }
            var criteria = string.Join($"{newLine}                                  || ", result);
            return $@"foreach (var criteria in splitGeneralFilter)
                                    query.Where(e => {criteria});";
        }
        return "";
    }

    public (string, string) GetDtosProperties(Dto entity)
    {
        var newLine = "\n        ";
        var requestProperties = new List<string>();
        var responseProperties = new List<string>();

        // DTO Request Properties:
        var requestPropsConfigs = GetPropertiesConfig(entity.Request);
        foreach (var config in requestPropsConfigs)
        {
            if (config.Options.ContainsKey("skip")) continue;

            var toInsert = "";

            // if (config.MaxLength != null)
            //     toInsert += $"[MaxLength({config.MaxLength})]{newLine}";
            // if (config.Options.ContainsKey("required"))
            //     toInsert += $"[Required]{newLine}";

            toInsert += $"public {GetCSharpType(config.Type)} {config.Property} {{ get; set; }}";

            requestProperties.Add(toInsert);
        }

        // DTO Request Properties:
        var responsePropsConfigs = GetPropertiesConfig(entity.Response);
        foreach (var config in responsePropsConfigs)
        {
            if (config.Options.ContainsKey("skip")) continue;

            var toInsert = "";

            // if (config.MaxLength != null)
            //     toInsert += $"[MaxLength({config.MaxLength})]{newLine}";
            // if (config.Options.ContainsKey("required"))
            //     toInsert += $"[Required]{newLine}";

            toInsert += $"public {GetCSharpType(config.Type)} {config.Property} {{ get; set; }}";

            responseProperties.Add(toInsert);
        }

        return (
            string.Join(newLine, requestProperties).Trim(),
            string.Join(newLine, responseProperties).Trim()
        );
    }
}
