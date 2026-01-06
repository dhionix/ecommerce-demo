using ecommerce_api.Data;
using ecommerce_api.Services;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.AddControllers();
                    
                    // Configure Rate Limiting
                    services.AddMemoryCache();
                    services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
                    services.AddInMemoryRateLimiting();
                    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
                    
                    // Configure JWT Authentication
                    var jwtSecret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
                    var jwtIssuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
                    var jwtAudience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
                    
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                            ValidateIssuer = true,
                            ValidIssuer = jwtIssuer,
                            ValidateAudience = true,
                            ValidAudience = jwtAudience,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };
                    });

                    services.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-commerce API", Version = "v1" });
                        
                        // Add JWT authentication to Swagger
                        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                        {
                            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.ApiKey,
                            Scheme = "Bearer"
                        });
                        
                        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    
                    services.AddSingleton<CustomerService>();
                    services.AddSingleton<OrderService>();
                    services.AddSingleton<OrderDetailService>();
                    services.AddSingleton<ProductService>();
                    services.AddSingleton<AuthenticationService>();
                    services.AddSingleton<JwtService>();
                });
                webBuilder.Configure((context, app) =>
                {
                    var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                    if (env.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }
                    
                    // Add IP rate limiting middleware (except in test environment)
                    if (!env.IsEnvironment("Test"))
                    {
                        app.UseIpRateLimiting();
                    }
                    
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                    var customerService = app.ApplicationServices.GetRequiredService<CustomerService>();
                    var orderService = app.ApplicationServices.GetRequiredService<OrderService>();
                    var orderDetailService = app.ApplicationServices.GetRequiredService<OrderDetailService>();
                    var productService = app.ApplicationServices.GetRequiredService<ProductService>();
                    MockDataInitializer.Initialize(customerService, orderService, orderDetailService, productService);
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-commerce API V1");
                    });
                });
            });
}