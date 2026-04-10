using Dapper;
using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
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
            using var conn = new NpgsqlConnection(_connectionString);
            var indoor = conn.Query<IndoorLocation>("SELECT * FROM locations WHERE isactive = TRUE AND locationtype = 'Indoor'").Cast<Location>();
            var outdoor = conn.Query<OutdoorLocation>("SELECT * FROM locations WHERE isactive = TRUE AND locationtype = 'Outdoor'").Cast<Location>();
            return indoor.Concat(outdoor);
        }

        public IEnumerable<Location> GetByFilter(Dictionary<string, object> filters)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var sql = new StringBuilder("SELECT * FROM locations WHERE 1=1");
            var parameters = new DynamicParameters();

            filters.TryGetValue("LocationType", out var locationTypeFilter);

            foreach (var filter in filters)
            {
                var key = filter.Key;
                var colName = key.Replace("_min", "").Replace("_max", "").ToLower();
                object value = filter.Value is string strVal
                    ? (int.TryParse(strVal, out int i) ? i : double.TryParse(strVal, out double d) ? d : bool.TryParse(strVal, out bool b) ? b : (object)strVal)
                    : filter.Value;

                if (key.EndsWith("_min"))
                {
                    sql.Append($" AND {colName} >= @{key}");
                    parameters.Add(key, value);
                }
                else if (key.EndsWith("_max"))
                {
                    sql.Append($" AND {colName} <= @{key}");
                    parameters.Add(key, value);
                }
                else
                {
                    sql.Append($" AND {colName} = @{key}");
                    parameters.Add(key, value);
                }
            }

            string baseSql = sql.ToString();
            var results = new List<Location>();

            if (locationTypeFilter?.ToString() != "Outdoor")
                results.AddRange(conn.Query<IndoorLocation>(baseSql + " AND locationtype = 'Indoor'", parameters).Cast<Location>());

            if (locationTypeFilter?.ToString() != "Indoor")
                results.AddRange(conn.Query<OutdoorLocation>(baseSql + " AND locationtype = 'Outdoor'", parameters).Cast<Location>());

            return results;
        }

        public Location GetById(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var locationType = conn.QueryFirstOrDefault<string>(
                "SELECT locationtype FROM locations WHERE id = @Id", new { Id = id });

            if (locationType == "Indoor")
                return conn.QueryFirstOrDefault<IndoorLocation>(
                    "SELECT * FROM locations WHERE id = @Id", new { Id = id });
            else
                return conn.QueryFirstOrDefault<OutdoorLocation>(
                    "SELECT * FROM locations WHERE id = @Id", new { Id = id });
        }

        public void Add(Location location)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            if (location is IndoorLocation indoor)
            {
                conn.Execute(@"
                    INSERT INTO locations
                    (name, address, latitude, longitude, rating, noiselevel, wifistrength, haspoweroutlets, isactive, locationtype,
                     comfortlevel, pricelevel, openinghours, indoortype)
                    VALUES
                    (@Name, @Address, @Latitude, @Longitude, @Rating, @NoiseLevel, @WifiStrength, @HasPowerOutlets, @IsActive, @LocationType,
                     @ComfortLevel, @PriceLevel, @OpeningHours, @IndoorType)",
                    indoor);
            }
            else if (location is OutdoorLocation outdoor)
            {
                conn.Execute(@"
                    INSERT INTO locations
                    (name, address, latitude, longitude, rating, noiselevel, wifistrength, haspoweroutlets, isactive, locationtype,
                     hasbenches, hasshade, petfriendly, haspublictoilet, nearshops, outdoortype)
                    VALUES
                    (@Name, @Address, @Latitude, @Longitude, @Rating, @NoiseLevel, @WifiStrength, @HasPowerOutlets, @IsActive, @LocationType,
                     @HasBenches, @HasShade, @PetFriendly, @HasPublicToilet, @NearShops, @OutdoorType)",
                    outdoor);
            }
        }

        public void Update(Location location)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Execute(@"
                UPDATE locations SET
                name = @Name, address = @Address, rating = @Rating,
                noiselevel = @NoiseLevel, wifistrength = @WifiStrength,
                haspoweroutlets = @HasPowerOutlets
                WHERE id = @Id", location);

            if (location is IndoorLocation indoor)
                conn.Execute(@"
                    UPDATE locations SET comfortlevel = @ComfortLevel, pricelevel = @PriceLevel
                    WHERE id = @Id", indoor);
        }

        public void SetInactive(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Execute("UPDATE locations SET isactive = FALSE WHERE id = @Id", new { Id = id });
        }
    }
}
