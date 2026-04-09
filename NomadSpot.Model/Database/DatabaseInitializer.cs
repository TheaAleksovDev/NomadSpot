using Npgsql;
using Microsoft.Data.Sqlite;
using System;

namespace NomadSpot.Model.Database
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly DatabaseType _dbType;

        public DatabaseInitializer(DatabaseType dbType, string connectionString)
        {
            _dbType = dbType;
            _connectionString = connectionString;
        }

        public void Initialize()
        {
            if (_dbType == DatabaseType.PostgreSQL)
                InitializePostgreSQL();
            else
                InitializeSQLite();
        }

        private void InitializePostgreSQL()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(GetCreateTablesSQL(), conn);
            cmd.ExecuteNonQuery();
        }

        private void InitializeSQLite()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(GetCreateTablesSQL(), conn);
            cmd.ExecuteNonQuery();
        }

        private string GetCreateTablesSQL()
        {
            return @"
                CREATE TABLE IF NOT EXISTS Locations (
                    Id SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    Address VARCHAR(200),
                    Latitude DOUBLE PRECISION NOT NULL,
                    Longitude DOUBLE PRECISION NOT NULL,
                    Rating DOUBLE PRECISION DEFAULT 0,
                    NoiseLevel INTEGER DEFAULT 0,
                    WifiStrength INTEGER DEFAULT 0,
                    HasPowerOutlets BOOLEAN DEFAULT FALSE,
                    IsActive BOOLEAN DEFAULT TRUE,
                    LocationType VARCHAR(20) NOT NULL,
                    ComfortLevel INTEGER DEFAULT 0,
                    PriceLevel INTEGER DEFAULT 0,
                    OpeningHours VARCHAR(50),
                    IndoorType INTEGER DEFAULT 0,
                    HasBenches BOOLEAN DEFAULT FALSE,
                    HasShade BOOLEAN DEFAULT FALSE,
                    PetFriendly BOOLEAN DEFAULT FALSE,
                    HasPublicToilet BOOLEAN DEFAULT FALSE,
                    NearShops BOOLEAN DEFAULT FALSE,
                    OutdoorType INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Reviews (
                    Id SERIAL PRIMARY KEY,
                    LocationId INTEGER REFERENCES Locations(Id),
                    Author VARCHAR(100),
                    Comment TEXT,
                    Rating INTEGER DEFAULT 0,
                    NoiseLevel INTEGER DEFAULT 0,
                    WifiStrength INTEGER DEFAULT 0,
                    ComfortLevel INTEGER DEFAULT 0,
                    PriceLevel INTEGER DEFAULT 0,
                    Cleanliness INTEGER DEFAULT 0,
                    Crowdedness INTEGER DEFAULT 0,
                    Date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
            ";
        }
    }
}
