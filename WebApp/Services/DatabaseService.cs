﻿using Shared.DTOs;
using WebApp.Models;
using WebApp.Repository;
using System.Collections.Generic;

namespace WebApp.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IDatabaseRepository _databaseRepository;
        private readonly Dictionary<string, Database> _cache = new();

        public DatabaseService(IDatabaseRepository databaseRepository)
        {
            _databaseRepository = databaseRepository;
        }

        public void CreateDatabase(Database db)
        {
            
            _databaseRepository.CreateDatabase(db);

            if (! _cache.ContainsKey(db.Name))
            {
                _cache[db.Name] = db;
            }
        }


        public List<Database> GetDatabases()
        {
            List<Database> databases = new List<Database>();
            if (_cache.Count() == 0)
            {
                databases = _databaseRepository.ReadDatabases();
                if (databases != null)
                {
                    foreach (var database in databases)
                    {
                        _cache[database.Name] = database;
                    }
                }
            }
            else
                databases = _cache.Values.ToList();

            return databases;
        }
        public Database GetDatabase(string databaseName)
        {
            if (!_cache.TryGetValue(databaseName, out var database))
            {
                database = _databaseRepository.ReadDatabase(databaseName);
                if (database != null)
                    _cache[databaseName] = database;
            }

            return database;
        }

        public void UpdateDatabase(string dbName, Database db)
        {
            if (_cache.ContainsKey(dbName))
            {
                _cache[dbName] = db;
            }

            _databaseRepository.UpdateDatabase(db);
        }

        public void DeleteDatabase(string databaseName)
        {
            _databaseRepository.DeleteDatabase(databaseName);
            _cache.Remove(databaseName);
        }

    }
}
