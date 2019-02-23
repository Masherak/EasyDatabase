using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDatabase.Interfaces
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}