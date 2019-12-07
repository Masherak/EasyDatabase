using System;
using EasyDatabase.Core.Interfaces;

namespace EasyDatabase.Repository.Tests.Entities
{
    public class Test : IEntity
    {
        public Test(string testProperty)
        {
            TestProperty = testProperty;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        public string TestProperty { get; }
    }
}
