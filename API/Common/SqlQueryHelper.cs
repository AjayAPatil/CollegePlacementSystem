using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace API.Common
{
    public interface ISqlQueryHelper
    {
        Task<List<T>> GetListAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);
        Task<T?> GetSingleAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);
        Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text);

        List<T> GetList<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);
        T? GetSingle<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);
        int Execute(string sql, object? parameters = null, CommandType commandType = CommandType.Text);
    }

    public class SqlQueryHelper : ISqlQueryHelper
    {
        private readonly string _connectionString;

        public SqlQueryHelper(IConfiguration configuration, string name = "DefaultConnection")
        {
            _connectionString = configuration.GetConnectionString(name) ?? throw new InvalidOperationException($"Connection string '{name}' not found in configuration.");
        }

        // Async: Get list of records
        public async Task<List<T>> GetListAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            await using SqlConnection conn = new(_connectionString);
            var result = await conn.QueryAsync<T>(sql, parameters, commandType: commandType);
            return result.AsList();
        }

        // Async: Get single record (or default/null)
        public async Task<T?> GetSingleAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            await using SqlConnection conn = new(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<T>(sql, parameters, commandType: commandType);
        }

        // Async: Execute non-query (INSERT/UPDATE/DELETE)
        public async Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            await using SqlConnection conn = new(_connectionString);
            return await conn.ExecuteAsync(sql, parameters, commandType: commandType);
        }

        // Synchronous helpers
        public List<T> GetList<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            using SqlConnection conn = new(_connectionString);
            var result = conn.Query<T>(sql, parameters, commandType: commandType);
            return result.AsList();
        }

        public T? GetSingle<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            using SqlConnection conn = new(_connectionString);
            return conn.QueryFirstOrDefault<T>(sql, parameters, commandType: commandType);
        }

        public int Execute(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            using SqlConnection conn = new(_connectionString);
            return conn.Execute(sql, parameters, commandType: commandType);
        }
    }
}
