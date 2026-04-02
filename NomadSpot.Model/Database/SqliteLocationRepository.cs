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


        public Location GetById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            return conn.QueryFirstOrDefault<Location>(
                "SELECT * FROM Locations WHERE Id = @Id", new { Id = id });
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
            var sql = new StringBuilder("SELECT * FROM Locations WHERE IsActive = 1");
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

            var indoor = conn.Query<IndoorLocation>(sql.ToString() + " AND LocationType = 'Indoor'", parameters).Cast<Location>();
            var outdoor = conn.Query<OutdoorLocation>(sql.ToString() + " AND LocationType = 'Outdoor'", parameters).Cast<Location>();
            return indoor.Concat(outdoor);
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