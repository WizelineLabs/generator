public class GeneratorContext : DbContext
{
    private readonly IConfiguration configuration;

    public GeneratorContext(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connString = configuration["ConnectionStrings:Default"];
        optionsBuilder.UseNpgsql(connString);
    }
    public DbSet<Application>? Applications { get; set; }
}