using Dapper;
using Microsoft.Data.Sqlite;
using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
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
            using var conn = new SqliteConnection(_connectionString);
            return conn.Query<Review>(
                "SELECT * FROM Reviews WHERE LocationId = @LocationId",
                new { LocationId = locationId });
        }

        public IEnumerable<Review> GetByFilter(Dictionary<string, object> filters)
        {
            using var conn = new SqliteConnection(_connectionString);
            var sql = new StringBuilder("SELECT * FROM Reviews WHERE 1=1");
            var parameters = new DynamicParameters();

            foreach (var filter in filters)
            {
                sql.Append($" AND {filter.Key} = @{filter.Key}");
                if (filter.Value is string strVal)
                {
                    if (int.TryParse(strVal, out int intVal))
                        parameters.Add(filter.Key, intVal);
                    else if (double.TryParse(strVal, out double dblVal))
                        parameters.Add(filter.Key, dblVal);
                    else if (bool.TryParse(strVal, out bool boolVal))
                        parameters.Add(filter.Key, boolVal);
                    else
                        parameters.Add(filter.Key, strVal);
                }
                else
                    parameters.Add(filter.Key, filter.Value);
            }

            return conn.Query<Review>(sql.ToString(), parameters);
        }

        public void Add(Review review)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Execute(@"
                INSERT INTO Reviews (LocationId, Author, Comment, Rating, NoiseLevel, WifiStrength, ComfortLevel, PriceLevel, Cleanliness, Crowdedness, Date)
                VALUES (@LocationId, @Author, @Comment, @Rating, @NoiseLevel, @WifiStrength, @ComfortLevel, @PriceLevel, @Cleanliness, @Crowdedness, @Date)",
                review);
        }
    }
}