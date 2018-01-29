using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Topshelf.Leader.MongoDb
{
    public class MongoDbLock : ILockManager
    {
        private readonly IMongoCollection<BsonDocument> collection;
        private bool isInitialised = false;

        public MongoDbLock(IMongoCollection<BsonDocument> collection)
        {
            this.collection = collection;
        }

        public async Task<bool> AcquireLock(string nodeId, CancellationToken token)
        {
            await Initialise();

            var filter = Builders<BsonDocument>.Filter;
            var filterDef = filter.Eq("_id", FieldConstants.DocumentIdField) & filter.Lt(FieldConstants.LockExpiryField, DateTime.UtcNow);
            var update = Builders<BsonDocument>.Update;
            var updateDef = update.Set("_id", FieldConstants.DocumentIdField).Set("nodeId", nodeId).Set(FieldConstants.LockExpiryField, DateTime.UtcNow.AddMinutes(5));

            var result = await collection.FindOneAndUpdateAsync(filterDef, updateDef, new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = false, ReturnDocument = ReturnDocument.After }, token);

            return result["nodeId"].ToString() == nodeId;
        }

        public async Task<bool> RenewLock(string nodeId, CancellationToken token)
        {
            await Initialise();

            var filter = Builders<BsonDocument>.Filter;
            var filterDef = filter.Eq("_id", FieldConstants.DocumentIdField) & filter.Eq("nodeId", nodeId);
            var update = Builders<BsonDocument>.Update;
            var updateDef = update.Set("_id", FieldConstants.DocumentIdField).Set(FieldConstants.LockExpiryField, DateTime.UtcNow.AddMinutes(5));

            var result = await collection.FindOneAndUpdateAsync(filterDef, updateDef, new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = false, ReturnDocument = ReturnDocument.After }, token);

            return result["nodeId"].ToString() == nodeId;
        }

        private async Task Initialise()
        {
            if (isInitialised)
            {
                return;
            }

            var insertDocFilter = Builders<BsonDocument>.Filter.Eq("_id", FieldConstants.DocumentIdField);
            var update = Builders<BsonDocument>.Update;
            var insertDocUpdateFilter = update.Set("_id", FieldConstants.DocumentIdField);
            var updateDocFilter = Builders<BsonDocument>.Filter.Eq("_id", FieldConstants.DocumentIdField) & Builders<BsonDocument>.Filter.Exists("nodeId", false);
            var updateDocUpdateFilter = update.Set("nodeId", string.Empty).Set(FieldConstants.LockExpiryField, DateTime.UtcNow.AddDays(-1));   //TODO: get server datetime

            // this adds the document if it doesn't exist
            await collection.UpdateOneAsync(insertDocFilter, insertDocUpdateFilter, new UpdateOptions { IsUpsert = true });

            // this defaults values where they haven't been set
            await collection.UpdateOneAsync(updateDocFilter, updateDocUpdateFilter, new UpdateOptions { IsUpsert = false });

            isInitialised = true;
        }
    }

}
