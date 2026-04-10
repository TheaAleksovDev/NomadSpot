using Dapper;
using Microsoft.Data.Sqlite;
using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NomadSpot.Model.Database
{
    public class SqliteLocationRepository : ILocationRepository
    {
        private readonly string _connectionString;

        public SqliteLocationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Location GetById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            var locationType = conn.QueryFirstOrDefault<string>(
                "SELECT LocationType FROM Locations WHERE Id = @Id", new { Id = id });
            if (locationType == "Indoor")
                return conn.QueryFirstOrDefault<IndoorLocation>("SELECT * FROM Locations WHERE Id = @Id", new { Id = id });
            else
                return conn.QueryFirstOrDefault<OutdoorLocation>("SELECT * FROM Locations WHERE Id = @Id", new { Id = id });
        }

        public IEnumerable<Location> GetAll()
        {
            using var conn = new SqliteConnection(_connectionString);
            var indoor = conn.Query<IndoorLocation>("SELECT * FROM Locations WHERE IsActive = 1 AND LocationType = 'Indoor'").Cast<Location>();
            var outdoor = conn.Query<OutdoorLocation>("SELECT * FROM Locations WHERE IsActive = 1 AND LocationType = 'Outdoor'").Cast<Location>();
            return indoor.Concat(outdoor);
        }

        public IEnumerable<Location> GetByFilter(Dictionary<string, object> filters)
        {
            using var conn = new SqliteConnection(_connectionString);
            var sql = new StringBuilder("SELECT * FROM Locations WHERE 1=1");
            var parameters = new DynamicParameters();

            filters.TryGetValue("LocationType", out var locationTypeFilter);

            foreach (var filter in filters)
            {
                var key = filter.Key;
                var colName = key.Replace("_min", "").Replace("_max", "");
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
                results.AddRange(conn.Query<IndoorLocation>(baseSql + " AND LocationType = 'Indoor'", parameters).Cast<Location>());

            if (locationTypeFilter?.ToString() != "Indoor")
                results.AddRange(conn.Query<OutdoorLocation>(baseSql + " AND LocationType = 'Outdoor'", parameters).Cast<Location>());

            return results;
        }

        public void Add(Location location)
        {
            using var conn = new SqliteConnection(_connectionString);
            if (location is IndoorLocation indoor)
            {
                conn.Execute(@"
                    INSERT INTO Locations
                    (Name, Address, Latitude, Longitude, Rating, NoiseLevel, WifiStrength, HasPowerOutlets, IsActive, LocationType,
                     ComfortLevel, PriceLevel, OpeningHours, IndoorType)
                    VALUES
                    (@Name, @Address, @Latitude, @Longitude, @Rating, @NoiseLevel, @WifiStrength, @HasPowerOutlets, @IsActive, @LocationType,
                     @ComfortLevel, @PriceLevel, @OpeningHours, @IndoorType)",
                    indoor);
            }
            else if (location is OutdoorLocation outdoor)
            {
                conn.Execute(@"
                    INSERT INTO Locations
                    (Name, Address, Latitude, Longitude, Rating, NoiseLevel, WifiStrength, HasPowerOutlets, IsActive, LocationType,
                     HasBenches, HasShade, PetFriendly, HasPublicToilet, NearShops, OutdoorType)
                    VALUES
                    (@Name, @Address, @Latitude, @Longitude, @Rating, @NoiseLevel, @WifiStrength, @HasPowerOutlets, @IsActive, @LocationType,
                     @HasBenches, @HasShade, @PetFriendly, @HasPublicToilet, @NearShops, @OutdoorType)",
                    outdoor);
            }
        }

        public void Update(Location location)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Execute(@"
                UPDATE Locations SET
                Name = @Name, Address = @Address, Rating = @Rating,
                NoiseLevel = @NoiseLevel, WifiStrength = @WifiStrength,
                HasPowerOutlets = @HasPowerOutlets
                WHERE Id = @Id", location);

            if (location is IndoorLocation indoor)
                conn.Execute(@"
                    UPDATE Locations SET ComfortLevel = @ComfortLevel, PriceLevel = @PriceLevel
                    WHERE Id = @Id", indoor);
        }

        public void SetInactive(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Execute("UPDATE Locations SET IsActive = 0 WHERE Id = @Id", new { Id = id });
        }
    }
}
