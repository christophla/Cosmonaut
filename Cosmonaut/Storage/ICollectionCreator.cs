﻿using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Storage
{
    public interface ICollectionCreator
    {
        Task<bool> EnsureCreatedAsync<TEntity>(Database database, int collectionThroughput, IndexingPolicy indexingPolicy = null) where TEntity : class;
    }
}