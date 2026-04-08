using Dapper;
using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using Npgsql;
using System.Collections.Generic;

namespace NomadSpot.Model.Database
{
    public class PostgreSqlReviewRepository : IReviewRepository
    {
        private readonly string _connectionString;

        public PostgreSqlReviewRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Review> GetByLocationId(int locationId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return conn.Query<Review>(
                "SELECT * FROM reviews WHERE locationid = @LocationId",
                new { LocationId = locationId });
        }

        public void Add(Review review)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Execute(@"
                INSERT INTO reviews (locationid, author, comment, rating, date)
                VALUES (@LocationId, @Author, @Comment, @Rating, @Date)",
                review);
        }
    }
}