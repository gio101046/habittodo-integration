using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habittodo.Data
{
    public class TableStorageClient
    {
        private CloudStorageAccount storageAccount { get; set; }
        private CloudTableClient tableClient { get; set; }

        /* TODO: change this so it reads from classes available instead of hardcoding */
        private static List<string> tableNames = new List<string>()
        {
            "habittodolink",
            "todohabitlink",
            "todochange",
            "todoistsync"
        };

        private Dictionary<string, CloudTable> tables = new Dictionary<string, CloudTable>();

        public TableStorageClient(string connectionString)
        {
            storageAccount = CloudStorageAccount.Parse(connectionString);
            tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            // initialize all tables to use
            foreach (var tableName in tableNames)
            {
                var table = tableClient.GetTableReference(tableName);
                table.CreateIfNotExists();
                tables[tableName] = table;
            }
        }

        public async Task<T> RetrieveRecord<T>(string partitionKey, string rowKey) where T : TableEntity, new()
        {
            var tableName = typeof(T).Name.ToLower();
            var table = tables[tableName];

            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(operation);

            return result.Result as T;
        }

        public async Task<T> InsertOrUpdate<T>(T entity) where T : TableEntity, new()
        {
            var tableName = typeof(T).Name.ToLower();
            var table = tables[tableName];

            var operation = TableOperation.InsertOrReplace(entity); // TODO: InsertOrReplace vs InsertOrMerge
            var result = await table.ExecuteAsync(operation);

            return result.Result as T; // TODO: might not work
        }

        public async Task<bool> ExistsAsync<T>(string partitionKey, string rowKey) where T : TableEntity, new()
        {
            var tableName = typeof(T).Name.ToLower();
            var table = tables[tableName];

            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(operation);

            return result.Result != null;
        }

        public bool Exists<T>(string partitionKey, string rowKey) where T : TableEntity, new()
        {
            return Query<T>().Where(x => x.PartitionKey == partitionKey && x.RowKey == rowKey).ToList().Any();
        }

        public TableQuery<T> Query<T>() where T : TableEntity, new()
        {
            var tableName = typeof(T).Name.ToLower();
            var table = tables[tableName];

            return table.CreateQuery<T>();
        }
    }
}
