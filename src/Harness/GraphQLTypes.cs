using System;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1591
namespace Harness
{
    public class _UserType : ObjectGraphType<User>
    {
        public _UserType()
        {
            Name = "User";
            Field(x => x.Id);
            Field(x => x.Name);
        }
    }

    public class HarnessQueries : ObjectGraphType
    {
        public HarnessQueries(IAggregate aggregate) : base()
        {
            Name = "Query";

            FieldAsync<_UserType>("viewer",
                resolve: async context =>
                {
                    var svc = (IServiceProvider)context.Schema;
                    var userContextService = svc.GetService<IUserContextBuilder>();
                    var httpContext = svc.GetService<IHttpContextAccessor>().HttpContext;
                    var principle = httpContext.User;
                    var user = await userContextService.BuildUserContext(httpContext);
                    Console.WriteLine(user.Count);
                    var uc = context.UserContext;
                    var pcpUser = ((IProvideClaimsPrincipal)uc).User;
                    Console.WriteLine(uc.Count.ToString());
                    var result = aggregate.Viewer();
                    return result;
                }
            ).AuthorizeWith("KycEmailPolicy").AuthorizeWith("AuthenticatedPolicy");

            FieldAsync<ListGraphType<_UserType>>("users",
                resolve: async context =>
                {
                    var svc = (IServiceProvider)context.Schema;
                    var userContextService = svc.GetService<IUserContextBuilder>();
                    var httpContext = svc.GetService<IHttpContextAccessor>().HttpContext;
                    var principle = httpContext.User;
                    var user = await userContextService.BuildUserContext(httpContext);
                    Console.WriteLine(user.Count);
                    var uc = context.UserContext;
                    Console.WriteLine(uc.Count.ToString());
                    var result = aggregate.Users();
                    return result;
                }
            ).AuthorizeWith("KycL1Policy");
            //).AuthorizeWith("AuthenticatedPolicy").AuthorizeWith("AdminPolicy");
        }
    }

    public class HarnessSchema : Schema
    {
        public HarnessSchema(IAggregate aggregate, IServiceProvider provider) : base(provider)
        {
            Query = new HarnessQueries(aggregate);
        }
    }
}
