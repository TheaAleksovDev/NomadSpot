using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NomadSpot.Model.Database
{
    public class SqliteReviewRepository : IReviewRepository
    {
        private readonly string _connectionString;

        public SqliteReviewRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Review> GetByLocationId(int locationId)
        {
            throw new NotImplementedException();
        }

        public void Add(Review review)
        {
            throw new NotImplementedException();
        }
    }
}
