

namespace Web;

public static class Startup
{
    private const string CorsPolicyName = "CorsPolicy";

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        #region CORS Policy

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName,
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
        });

        #endregion

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        #region Default DI Services

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        #endregion

        #region BsonSerializer 

        BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeSerializer(MongoDB.Bson.BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(MongoDB.Bson.BsonType.String));

        #endregion

        #region DBContext
        var mongoDbSettings = configuration.GetSection("MongoDbSettings");
        var connectionString = mongoDbSettings["ConnectionString"];
        var databaseName = mongoDbSettings["DatabaseName"];
        services.AddScoped(m => new ApplicationDbContext(connectionString!, databaseName!));
        #endregion

        #region Identity

        var mongoDbIdentityConfig = new MongoDbIdentityConfiguration
        {
            MongoDbSettings = new MongoDbSettings { ConnectionString = connectionString, DatabaseName = databaseName },
            IdentityOptionsAction = options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireLowercase = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;

                options.User.RequireUniqueEmail = true;
            }
        };

        services.ConfigureMongoDbIdentity<ApplicationUser, ApplicationRole, Guid>(mongoDbIdentityConfig)
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddSignInManager<SignInManager<ApplicationUser>>()
                .AddRoleManager<RoleManager<ApplicationRole>>()
                .AddDefaultTokenProviders();

        #endregion

        #region JWT
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"])),
                ClockSkew = TimeSpan.Zero
            };
        });
        #endregion

        #region Custom DI Services

        services.AddTransient<IIdentityService, IdentityService>();

        #endregion
    }

    public static void Configure(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseMiddleware<ErrorHandlingMiddleware>();

        app.UseRouting();

        app.UseCors(CorsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.SeedRolesToDatabase().Wait();
    }

    #region Seed SuperAdmin Role and User

    public static async Task SeedRolesToDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roles = new[] { "Admin", "User", "SuperAdmin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new ApplicationRole(role));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{role}'.");
                }
            }
        }

        var superAdmin = await userManager.FindByNameAsync("SuperAdmin");
        if (superAdmin == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "SuperAdmin",
                Email = "superadmin@example.com"
            };

            var adminPassword = "Admin.123$";

            var createAdminResult = await userManager.CreateAsync(admin, adminPassword);
            if (createAdminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "SuperAdmin");
            }
            else
            {
                throw new Exception($"Failed to create SuperAdmin user: {string.Join(",", createAdminResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    #endregion
}
