using System;

namespace EasyDatabase.Core.Interfaces
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}