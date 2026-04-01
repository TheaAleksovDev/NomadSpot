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
            var indoor = conn.Query<IndoorLocation>("SELECT * FROM Locations WHERE IsActive = TRUE AND LocationType = 'Indoor'").Cast<Location>();
            var outdoor = conn.Query<OutdoorLocation>("SELECT * FROM Locations WHERE IsActive = TRUE AND LocationType = 'Outdoor'").Cast<Location>();
            return indoor.Concat(outdoor);
        }

        public IEnumerable<Location> GetByFilter(Dictionary<string, object> filters)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var sql = new StringBuilder("SELECT * FROM Locations WHERE IsActive = TRUE");
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
                {
                    parameters.Add(filter.Key, filter.Value);
                }
            }

            var indoor = conn.Query<IndoorLocation>(sql.ToString() + " AND LocationType = 'Indoor'", parameters).Cast<Location>();
            var outdoor = conn.Query<OutdoorLocation>(sql.ToString() + " AND LocationType = 'Outdoor'", parameters).Cast<Location>();
            return indoor.Concat(outdoor);
        }

        public Location GetById(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var locationType = conn.QueryFirstOrDefault<string>(
                "SELECT LocationType FROM Locations WHERE Id = @Id", new { Id = id });

            if (locationType == "Indoor")
                return conn.QueryFirstOrDefault<IndoorLocation>(
                    "SELECT * FROM Locations WHERE Id = @Id", new { Id = id });
            else
                return conn.QueryFirstOrDefault<OutdoorLocation>(
                    "SELECT * FROM Locations WHERE Id = @Id", new { Id = id });
        }

        public void Add(Location location)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Execute(@"
                INSERT INTO Locations 
                (Name, Address, Latitude, Longitude, Rating, NoiseLevel, HasWifi, HasPowerOutlets, LastVerified, IsActive, LocationType)
                VALUES 
                (@Name, @Address, @Latitude, @Longitude, @Rating, @NoiseLevel, @HasWifi, @HasPowerOutlets, @LastVerified, @IsActive, @LocationType)",
                location);
        }

        public void Update(Location location)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Execute(@"
                UPDATE Locations SET 
                Name = @Name, Address = @Address, Rating = @Rating,
                NoiseLevel = @NoiseLevel, HasWifi = @HasWifi,
                HasPowerOutlets = @HasPowerOutlets, LastVerified = @LastVerified
                WHERE Id = @Id", location);
        }

        public void SetInactive(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Execute("UPDATE Locations SET IsActive = FALSE WHERE Id = @Id", new { Id = id });
        }
    }
}