using GraphQL.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using SB.GraphQL.Server.Auth.Startup;

namespace Harness
{
    internal class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Allows authentication exception detail to be shown.  May want to leave off in production.
            IdentityModelEventSource.ShowPII = true;

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    // SaveToken must be set in order to later retrieve the AccessToken
                    options.SaveToken = true;
                    options.Authority = "https://localhost:19101";
                    // Do not remap inbound claims.  If set to true, claims like "sub" will get remapped to names like http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = false,
                        ValidTypes = new[] { "at+jwt" },

                        NameClaimType = "name",
                        RoleClaimType = "role",
                    };

                    options.Events = new JwtBearerEvents
                    {
                        // Retrieve additional Identity Claims by calling the IdentityServer.UserInfo endpoint
                        OnTokenValidated = JwtBearerOptionsExtensions.AddUserInfoToIdentityClaims
                    };
                });

            services.AddSingleton<HarnessQueries>();
            services.AddSingleton<_UserType>();
            services.AddSingleton<HarnessSchema>();
            services.AddSingleton<IAggregate, Aggregate>();

            //services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // extension method defined in this project
            services.AddGraphQLAuth((settings, provider) =>
            {
                settings.AddPolicy("AuthenticatedPolicy", p => p.RequireAuthenticatedUser());
                settings.AddPolicy("AdminPolicy", p => p.RequireClaim("sub", "qwlw8M-qHk--H7DnHV1IKA"));
                settings.AddPolicy("KycL1Policy", p => p.RequireClaim("kyc_status", "L1"));
                settings.AddPolicy("KycEmailPolicy", p => p.RequireClaim("kyc_status", "email", "L1"));
            });

            // claims principal must look something like this to allow access
            // var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("role", "Admin") }));

            services
                .AddGraphQL()
                .AddSystemTextJson()
                .AddUserContextBuilder(context => new GraphQLUserContext { User = context.User });
        }

        // Configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // app.UseSerilogRequestLogging();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            // Add authentication middleware to the pipeline so authentication will be performed automatically on every call into the host.
            // This verifies the token is valid, has not expired, etc.  It creates the Identity and sets IsAuthenticated as appropriate.
            // It also has the side effect of making the HttpContext available.
            app.UseAuthentication();

            // Add token challenging which verifies that the AccessToken is signed with the active key from the
            // IdentityServer.
            app.UseJwtChallenge();

            app.UseGraphQL<HarnessSchema>();
            app.UseGraphQLGraphiQL();
            app.UseGraphQLPlayground();
        }
    }
}
