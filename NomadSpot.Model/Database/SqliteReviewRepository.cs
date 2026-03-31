using Dapper;
using Microsoft.Data.Sqlite;
using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using System.Collections.Generic;

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
            using var conn = new SqliteConnection(_connectionString);
            return conn.Query<Review>(
                "SELECT * FROM Reviews WHERE LocationId = @LocationId",
                new { LocationId = locationId });
        }

        public void Add(Review review)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Execute(@"
                INSERT INTO Reviews (LocationId, Author, Comment, Rating, Date)
                VALUES (@LocationId, @Author, @Comment, @Rating, @Date)",
                review);
        }
    }
}