using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyDatabase.Core;
using EasyDatabase.Core.Interfaces;
using EasyDatabase.Repository.Tests.Helpers;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Azure.KeyVault.KeyVaultClient;

namespace EasyDatabase.Repository.Tests
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            // Arrange
            var repositories = new List<IRepository>
            {
                new FileRepository.FileRepository(),
                new AzureBlobRepository.AzureBlobRepository(await KeyVaultHelper.GetSecret("StorageConnectionString"), "easy-database")
            };

            var testPropertyValues = new List<string> { "Hello", "World", "!" };

            foreach (var repository in repositories)
            {
                var storage = StorageFactory.GetStorage(repository, new Core.Configurations.CacheConfiguration());

                // Act
                try
                {
                    //clean
                    await storage.Delete<Entities.Test>(await storage.Get<Entities.Test>());
                }
                catch(FileNotFoundException)
                {
                    // intentionally empty
                }

                var testEntities = testPropertyValues.Select(_ => new Entities.Test(_)).ToList();
                await storage.AddOrUpdate(testEntities);
                var result = (await storage.Get<Entities.Test>()).ToList();
                await storage.Delete<Entities.Test>(result);
                var nextResult = (await storage.Get<Entities.Test>()).ToList();

                // Assert
                Assert.IsFalse(result.Any(_ => _.Id == Guid.Empty));
                testPropertyValues.ForEach(_ => Assert.IsTrue(result.Any(entity => entity.TestProperty == _)));
                testPropertyValues.ForEach(_ => Assert.IsFalse(nextResult.Any(entity => entity.TestProperty == _)));
            }
        }
    }
}