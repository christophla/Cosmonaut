﻿using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class CosmosStoreSettings
    {
        public const int DefaultCollectionThroughput = 400;

        public readonly string DatabaseName;

        public readonly string AuthKey;

        public readonly Uri EndpointUrl;

        public readonly ConnectionPolicy ConnectionPolicy;

        public readonly IndexingPolicy IndexingPolicy;

        public readonly int CollectionThroughput;

        public readonly bool AllowAttributesToConfigureThroughput;

        public readonly bool AdjustCollectionThroughputOnStartup;

        public readonly bool ScaleCollectionRUsAutomatically;

        public readonly int MaximumUpscaleRequestUnits;

        public CosmosStoreSettings(
            string databaseName, 
            string endpointUrl, 
            string authKey, 
            ConnectionPolicy connectionPolicy = null, 
            IndexingPolicy indexingPolicy = null,
            int? collectionThroughput = null,
            bool allowAttributesToConfigureThroughput = false,
            bool adjustCollectionThroughputOnStartup = false,
            bool scaleCollectionRUsAutomatically = false,
            int maximumUpscaleRequestUnits = 10000) 
            : this(databaseName, 
                  new Uri(endpointUrl), 
                  authKey,connectionPolicy,
                  indexingPolicy,
                  collectionThroughput,
                  allowAttributesToConfigureThroughput,
                  adjustCollectionThroughputOnStartup,
                  scaleCollectionRUsAutomatically,
                  maximumUpscaleRequestUnits)
        {
        }

        public CosmosStoreSettings(
            string databaseName, 
            Uri endpointUrl, 
            string authKey,
            ConnectionPolicy connectionPolicy = null,
            IndexingPolicy indexingPolicy = null,
            int? collectionThroughput = null,
            bool allowAttributesToConfigureThroughput = false,
            bool adjustCollectionThroughputOnStartup = false,
            bool scaleCollectionRUsAutomatically = false,
            int maximumUpscaleRequestUnits = 10000)
        {
            DatabaseName = databaseName;
            AuthKey = authKey;
            EndpointUrl = endpointUrl;
            ConnectionPolicy = connectionPolicy;
            CollectionThroughput = collectionThroughput ?? DefaultCollectionThroughput;
            AllowAttributesToConfigureThroughput = allowAttributesToConfigureThroughput;
            AdjustCollectionThroughputOnStartup = adjustCollectionThroughputOnStartup;
            ScaleCollectionRUsAutomatically = scaleCollectionRUsAutomatically;
            MaximumUpscaleRequestUnits = maximumUpscaleRequestUnits;
            IndexingPolicy = indexingPolicy;
        }
    }
}