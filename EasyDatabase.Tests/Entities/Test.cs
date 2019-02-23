using System;
using EasyDatabase.Interfaces;

namespace EasyDatabase.Tests.Entities
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
