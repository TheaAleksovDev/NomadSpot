using Dapper;
using Microsoft.Data.Sqlite;
using NomadSpot.Model.Entities;
using NomadSpot.Model.Repositories;
using System.Collections.Generic;
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

        public IEnumerable<Location> GetAll()
        {
            using var conn = new SqliteConnection(_connectionString);
            return conn.Query<Location>("SELECT * FROM Locations WHERE IsActive = 1");
        }

        public Location GetById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            return conn.QueryFirstOrDefault<Location>(
                "SELECT * FROM Locations WHERE Id = @Id", new { Id = id });
        }

        public IEnumerable<Location> GetByFilter(Dictionary<string, object> filters)
        {
            using var conn = new SqliteConnection(_connectionString);
            var sql = new StringBuilder("SELECT * FROM Locations WHERE IsActive = 1");
            foreach (var filter in filters)
                sql.Append($" AND {filter.Key} = @{filter.Key}");
            return conn.Query<Location>(sql.ToString(), filters);
        }

        public void Add(Location location)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Execute(@"
                INSERT INTO Locations 
                (Name, Address, Latitude, Longitude, Rating, NoiseLevel, HasWifi, HasPowerOutlets, LastVerified, IsActive, LocationType)
                VALUES 
                (@Name, @Address, @Latitude, @Longitude, @Rating, @NoiseLevel, @HasWifi, @HasPowerOutlets, @LastVerified, @IsActive, @LocationType)",
                location);
        }

        public void Update(Location location)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Execute(@"
                UPDATE Locations SET 
                Name = @Name, Address = @Address, Rating = @Rating,
                NoiseLevel = @NoiseLevel, HasWifi = @HasWifi,
                HasPowerOutlets = @HasPowerOutlets, LastVerified = @LastVerified
                WHERE Id = @Id", location);
        }

        public void SetInactive(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Execute("UPDATE Locations SET IsActive = 0 WHERE Id = @Id", new { Id = id });
        }
    }
}