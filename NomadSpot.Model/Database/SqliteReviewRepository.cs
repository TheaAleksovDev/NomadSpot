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
                var key     = filter.Key;
                var colName = key.Replace("_min", "").Replace("_max", "");
                object value = filter.Value is string strVal
                    ? (int.TryParse(strVal, out int i) ? i
                        : double.TryParse(strVal, out double d) ? d
                        : bool.TryParse(strVal, out bool b) ? b
                        : (object)strVal)
                    : filter.Value;

                if (key.EndsWith("_min"))
                    sql.Append($" AND {colName} >= @{key}");
                else if (key.EndsWith("_max"))
                    sql.Append($" AND {colName} <= @{key}");
                else
                    sql.Append($" AND {colName} = @{key}");

                parameters.Add(key, value);
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