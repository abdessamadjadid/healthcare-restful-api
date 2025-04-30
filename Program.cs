var builder = WebApplication.CreateBuilder(args);

// Register OrgHierarchyService as a singleton
builder.Services.AddSingleton<OrgHierarchyService>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

