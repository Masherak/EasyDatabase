using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDatabase.Interfaces
{
    public interface IRepository
    {
        Task<T> ReadEntity<T>(Guid id) where T : IEntity;
        Task<IEnumerable<T>> ReadEntities<T>() where T : IEntity;
        Task WriteEntity<T>(T entity) where T : IEntity;
        Task DeleteEntity<T>(Guid id) where T : IEntity;
    }
}
