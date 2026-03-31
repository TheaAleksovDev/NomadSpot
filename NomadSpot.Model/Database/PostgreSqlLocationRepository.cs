using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NomadSpot.Model.Database
{
    public class PostgreSqlLocationRepository : ILocationRepository
    {
        private readonly string _connectionString;

        public PostgreSqlLocationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Location> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Location> GetByFilter(Dictionary<string, object> filters)
        {
            throw new NotImplementedException();
        }

        public Location GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(Location location)
        {
            throw new NotImplementedException();
        }

        public void Update(Location location)
        {
            throw new NotImplementedException();
        }

        public void SetInactive(int id)
        {
            throw new NotImplementedException();
        }
    }
}
