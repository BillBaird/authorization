using System;
using System.Collections.Generic;

namespace Harness
{
    /// <summary>
    /// CLR type to map to the 'Query' graph type.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Resolver for 'Query.viewer' field.
        /// </summary>
        public User Viewer() => new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" };

        /// <summary>
        /// Resolver for 'Query.users' field.
        /// </summary>
        public List<User> Users() => new List<User> { new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" } };
    }

    /// <summary>
    /// CLR type to map to the 'User' graph type.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Resolver for 'User.id' field. Just a simple property.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Resolver for 'User.name' field. Just a simple property.
        /// </summary>
        public string Name { get; set; }
    }
}
