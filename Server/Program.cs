using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Server.Models;
using Server.Authorization.Jwt;
using Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using System.Text.Json.Serialization;
using Stripe;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<FcmsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FcmsContext") ?? throw new InvalidOperationException("Connection string 'FcmsContext' not found.")));

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;

        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedAccount = false;
       
    })
    .AddEntityFrameworkStores<FcmsContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

})
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSection.GetValue<string>("Issuer"),
            ValidateIssuer = true,
            ValidAudience = jwtSection.GetValue<string>("Audience"),
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection.GetValue<string>("SecretKey"))),
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if(context.Request.Cookies.ContainsKey("Bearer"))
                {
                    context.Token = context.Request.Cookies["Bearer"]; 
                }
                else if(context.Request.Headers.ContainsKey("Authorization"))
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault(value => value.StartsWith("Bearer"));
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        var authHeaderValueArr = authHeader.Split(" "); 
                        if(authHeaderValueArr.Length > 1)
                        {
                            context.Token = authHeaderValueArr[1];
                        }
                    }
                }

                return Task.CompletedTask;
            }
        };

    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .RequireClaim("UserId")
                            .Build();

    options.AddPolicy("Admin",
        policy => policy.RequireRole("Admin"));
});

builder.Services.AddSingleton<JwtTokenGenerator>();
builder.Services.AddSingleton<MailService>();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//for stripe services
builder.Services.AddSingleton<IStripeClient>(
    new StripeClient(builder.Configuration["Stripe:SecretKey"]));


//add stripe api key
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(options => 
    options.SetIsOriginAllowed(origin => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
app.UseStaticFiles();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
