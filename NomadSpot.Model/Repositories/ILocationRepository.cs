using NomadSpot.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NomadSpot.Model.Repositories
{
    public interface ILocationRepository
    {
        IEnumerable<Location> GetAll();
        IEnumerable<Location> GetByFilter(Dictionary<string, object> filters);
        Location GetById(int id);
        void Add(Location location);
        void Update(Location location);
        void SetInactive(int id);
    }
}
