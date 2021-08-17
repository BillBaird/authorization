using System.Collections.Generic;

#pragma warning disable CS1591
namespace Harness
{
    public interface IAggregate
    {
        User Viewer();
        List<User> Users();
    }

    public class Aggregate : IAggregate
    {
        public Query QueryInstance = new Query();

        public User Viewer() => QueryInstance.Viewer();
        public List<User> Users() => QueryInstance.Users();
    }
}
