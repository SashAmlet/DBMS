﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;
using Shared.DTOs;


namespace Desktop
{
    internal class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
        }


        public async Task CreateDatabaseAsync(CreateDatabaseDTO dto)
        {
            string url = Path.Combine(Constants.ApiUrl, "api/Database");
            var jsonData = JsonConvert.SerializeObject(dto);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get database: {response.ReasonPhrase}");
            }
        }

        public async Task<DatabaseDTO?> GetDatabaseAsync(string dbName)
        {
            string url = Path.Combine(Constants.ApiUrl, $"api/Database/{dbName}");

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                DatabaseDTO? database = JsonConvert.DeserializeObject<DatabaseDTO>(jsonResponse);

                return database;
            }
            else
            {
                throw new Exception($"Failed to get database: {response.ReasonPhrase}");
            }
        }

        public async Task DeleteDatabaseAsync(string dbName)
        {
            string url = Path.Combine(Constants.ApiUrl, $"api/Database/{dbName}");

            HttpResponseMessage response = await _httpClient.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to delete database: {response.ReasonPhrase}");
            }
        }

    }
}
