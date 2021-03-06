﻿using System.Linq;
using Microsoft.Azure.Documents.Linq;

namespace Cosmonaut.Tests
{
    public interface IFakeDocumentQuery<T> : IDocumentQuery<T>, IOrderedQueryable<T>
    {

    }
}