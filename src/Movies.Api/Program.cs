using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.Api;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(x=>{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x=>{
    x.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        ),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthConstants.AdminUserPolicyName, 
        p =>p.RequireClaim(AuthConstants.AdminUserClaimName, "true"))
    .AddPolicy(AuthConstants.TrustedMemberPolicyName, p =>p.RequireAssertion(c=>
            c.User.HasClaim(AuthConstants.AdminUserClaimName, "true") ||
            c.User.HasClaim(AuthConstants.TrustedMemberClaimName, "true")
        ));

builder.Services.AddApplication();
builder.Services.AddDatabase(builder.Configuration["ConnectionStrings:DefaultConnection"]!);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication()
   .UseAuthorization();

app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();