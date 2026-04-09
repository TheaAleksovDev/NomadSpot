using Dapper;
using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using Npgsql;
using System.Collections.Generic;
using System.Text;

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

        public IEnumerable<Review> GetByFilter(Dictionary<string, object> filters)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var sql = new StringBuilder("SELECT * FROM reviews WHERE 1=1");
            var parameters = new DynamicParameters();

            foreach (var filter in filters)
            {
                sql.Append($" AND {filter.Key.ToLower()} = @{filter.Key}");
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
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Execute(@"
                INSERT INTO reviews (locationid, author, comment, rating, noiselevel, wifistrength, comfortlevel, pricelevel, cleanliness, crowdedness, date)
                VALUES (@LocationId, @Author, @Comment, @Rating, @NoiseLevel, @WifiStrength, @ComfortLevel, @PriceLevel, @Cleanliness, @Crowdedness, @Date)",
                review);
        }
    }
}