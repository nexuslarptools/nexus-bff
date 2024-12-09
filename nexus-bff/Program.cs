using Duende.Bff.Yarp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddBff()
    .AddRemoteApis();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "cookie";
        options.DefaultChallengeScheme = "oidc";
        options.DefaultSignOutScheme = "oidc";
    })
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "__Host-Standalone-bff";
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = Environment.GetEnvironmentVariable("AUTH_AUTHORITY");
        options.ClientId = Environment.GetEnvironmentVariable("AUTH_CLIENT_ID");
        options.ClientSecret = Environment.GetEnvironmentVariable("AUTH_CLIENT_SECRET");
        options.ResponseType = "code";
        options.ResponseMode = "query";
        options.UsePkce = true;

        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;
        options.SaveTokens = true;

        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("recipe_management");

        options.TokenValidationParameters = new()
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseBff();
app.UseAuthorization();

app.MapBffManagementEndpoints();

app.MapControllers()
    .RequireAuthorization()
    .AsBffApiEndpoint();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRemoteBffApiEndpoint("/api", "https://localhost:5375/api")
        .RequireAccessToken();
});

await app.RunAsync();