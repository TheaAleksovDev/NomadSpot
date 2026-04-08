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
            var sql = new StringBuilder("SELECT * FROM locations WHERE isactive = TRUE");
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
                {
                    parameters.Add(filter.Key, filter.Value);
                }
            }

            var indoor = conn.Query<IndoorLocation>(sql.ToString() + " AND locationtype = 'Indoor'", parameters).Cast<Location>();
            var outdoor = conn.Query<OutdoorLocation>(sql.ToString() + " AND locationtype = 'Outdoor'", parameters).Cast<Location>();
            return indoor.Concat(outdoor);
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
                    (name, address, latitude, longitude, rating, noiselevel, haswifi, haspoweroutlets, lastverified, isactive, locationtype,
                     comfortlevel, pricelevel, openinghours, indoortype)
                    VALUES
                    (@Name, @Address, @Latitude, @Longitude, @Rating, @NoiseLevel, @HasWifi, @HasPowerOutlets, @LastVerified, @IsActive, @LocationType,
                     @ComfortLevel, @PriceLevel, @OpeningHours, @IndoorType)",
                    indoor);
            }
            else if (location is OutdoorLocation outdoor)
            {
                conn.Execute(@"
                    INSERT INTO locations
                    (name, address, latitude, longitude, rating, noiselevel, haswifi, haspoweroutlets, lastverified, isactive, locationtype,
                     hasbenches, hasshade, petfriendly, haspublictoilet, nearshops)
                    VALUES
                    (@Name, @Address, @Latitude, @Longitude, @Rating, @NoiseLevel, @HasWifi, @HasPowerOutlets, @LastVerified, @IsActive, @LocationType,
                     @HasBenches, @HasShade, @PetFriendly, @HasPublicToilet, @NearShops)",
                    outdoor);
            }
        }

        public void Update(Location location)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Execute(@"
                UPDATE locations SET
                name = @Name, address = @Address, rating = @Rating,
                noiselevel = @NoiseLevel, haswifi = @HasWifi,
                haspoweroutlets = @HasPowerOutlets, lastverified = @LastVerified
                WHERE id = @Id", location);
        }

        public void SetInactive(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Execute("UPDATE locations SET isactive = FALSE WHERE id = @Id", new { Id = id });
        }
    }
}