using NomadSpot.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NomadSpot.Model.Database
{
    public enum DatabaseType
    {
        PostgreSQL,
        SQLite
    }

    public class RepositoryFactory
    {
        private readonly string _connectionString;
        private readonly DatabaseType _dbType;

        public RepositoryFactory(DatabaseType dbType, string connectionString)
        {
            _dbType = dbType;
            _connectionString = connectionString;
        }

        public ILocationRepository CreateLocationRepository()
        {
            return _dbType switch
            {
                DatabaseType.PostgreSQL => new PostgreSqlLocationRepository(_connectionString),
                DatabaseType.SQLite => new SqliteLocationRepository(_connectionString),
                _ => throw new NotSupportedException($"Database type {_dbType} is not supported.")
            };
        }

        public IReviewRepository CreateReviewRepository()
        {
            return _dbType switch
            {
                DatabaseType.PostgreSQL => new PostgreSqlReviewRepository(_connectionString),
                DatabaseType.SQLite => new SqliteReviewRepository(_connectionString),
                _ => throw new NotSupportedException($"Database type {_dbType} is not supported.")
            };
        }
    }
}
