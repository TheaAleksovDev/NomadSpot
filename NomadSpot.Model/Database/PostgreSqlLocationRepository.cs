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

            // pull out LocationType before building shared SQL so we can skip the wrong type query
            filters.TryGetValue("LocationType", out var locationTypeFilter);

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
                {
                    parameters.Add(filter.Key, filter.Value);
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
