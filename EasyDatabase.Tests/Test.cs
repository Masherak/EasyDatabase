using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyDatabase.Tests
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            // Arrange
            var db = StorageFactory.GetStorage();
            var testPropertyValues = new List<string> { "Hello", "World", "!" };

            // Act
            var testEntities = testPropertyValues.Select(_ => new Entities.Test(_)).ToList();
            await db.AddOrUpdate<Entities.Test>(testEntities);
            var result = (await db.Get<Entities.Test>()).ToList();
            await db.Delete<Entities.Test>(result);
            var nextResult = (await db.Get<Entities.Test>()).ToList();

            // Assert
            Assert.IsFalse(result.Any(_ => _.Id == Guid.Empty));
            testPropertyValues.ForEach(_ => Assert.IsTrue(result.Any(entity => entity.TestProperty == _)));
            testPropertyValues.ForEach(_ => Assert.IsFalse(nextResult.Any(entity => entity.TestProperty == _)));
        }
    }
}