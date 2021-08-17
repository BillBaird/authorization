using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using SB.GraphQL.Server.Auth;

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
                    var claimsPrinciple = context.ClaimsPrincipal()!;
                    //Console.WriteLine(claimsPrinciple!.Identity!.Name);
                    var result = aggregate.Viewer();
                    return await Task.FromResult(result);
                }
            ).AuthorizeWith("KycEmailPolicy").AuthorizeWith("AuthenticatedPolicy");

            FieldAsync<ListGraphType<_UserType>>("users",
                resolve: async context =>
                {
                    var claimsPrinciple = context.ClaimsPrincipal()!;
                    //Console.WriteLine(claimsPrinciple!.Identity!.Name);
                    var result = aggregate.Users();
                    return await Task.FromResult(result);
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
