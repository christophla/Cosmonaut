﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Exceptions;
using Cosmonaut.Storage;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Xunit;
using Cosmonaut.Extensions;

namespace Cosmonaut.Tests
{
    public class CollectionCreatorTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;

        public CollectionCreatorTests()
        {
            _mockDocumentClient = MockHelpers.GetMockDocumentClient();
        }


        [Fact]
        public async Task EnsureCreatedCreatesCollectionIfMissing()
        {
            // Arrange
            IOrderedQueryable<DocumentCollection> queryable = new EnumerableQuery<DocumentCollection>(new List<DocumentCollection>());

            _mockDocumentClient.Setup(x =>
                x.CreateDocumentCollectionQuery(It.IsAny<string>(), It.IsAny<FeedOptions>())).Returns(queryable);
            var collection = new DocumentCollection {Id = "collection"};
            var collectionResponse = MockHelpers.CreateResourceResponse(collection, HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.CreateDocumentCollectionAsync(It.IsAny<string>(),
                It.IsAny<DocumentCollection>(),
                It.IsAny<RequestOptions>())).ReturnsAsync(collectionResponse);

            var creator = new CosmosCollectionCreator(_mockDocumentClient.Object);

            // Act
            var result = await creator.EnsureCreatedAsync<Dummy>(new Database {Id = "databaseName"}, 500);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetSharedCollectionNameReturnsName()
        {
            // Arrange
            var sharedCollectionDummy = new DummySharedCollection();
            var expectedName = "shared";

            // Act
            var name = sharedCollectionDummy.GetType().GetSharedCollectionName();

            // Assert
            name.Should().Be(expectedName);
        }

        [Fact]
        public void GetSharedCollectionNameEmptyNameThrowsException()
        {
            // Arrange
            var sharedCollectionDummy = new DummySharedCollectionEmpty();

            // Act
            var action = new Action(() => sharedCollectionDummy.GetType().GetSharedCollectionName());

            // Assert
            action.Should().Throw<SharedCollectionNameMissingException>();
        }

        [Fact]
        public void ThroughputLessThanAllowedThrowsException()
        {
            // Arrange
            var dummy = new Dummy();

            // Act
            var actionWithout = new Action(()=> dummy.GetType().GetCollectionThroughputForEntity(false, 300));
            var actionWith = new Action(() => dummy.GetType().GetCollectionThroughputForEntity(true, 300));

            // Assert
            actionWith.Should().Throw<IllegalCosmosThroughputException>();
            actionWithout.Should().Throw<IllegalCosmosThroughputException>();
        }

        [Fact]
        public void UsesSharedCollectionWithAttributeNoImpl()
        {
            // Arrange
            var dummy = new DummyWithAttributeNoImpl();

            // Act
            var action = new Action(() => dummy.GetType().UsesSharedCollection());

            // Assert
            action.Should().Throw<SharedEntityDoesNotImplementExcepction>();
        }

        [Fact]
        public void UsesSharedCollectionWithImplNoAttribute()
        {
            // Arrange
            var dummy = new DummyImplNoAttribute();

            // Act
            var action = new Action(()=> dummy.GetType().UsesSharedCollection());

            // Assert
            action.Should().Throw<SharedEntityDoesNotHaveAttribute>();
        }
    }
}