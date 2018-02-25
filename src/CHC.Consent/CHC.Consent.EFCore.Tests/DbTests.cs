﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests
{
    [Collection(DatabaseCollection.Name)]
    public abstract class DbTests : IDisposable
    {
        protected IDbContextTransaction transaction;
        protected ITestOutputHelper outputHelper;
        protected DatabaseFixture fixture;
        protected ConsentContext Context { get; }

        protected DbTests(ITestOutputHelper outputHelper, DatabaseFixture fixture)
        {
            this.outputHelper = outputHelper;
            this.fixture = fixture;
            Context = fixture.GetContext(outputHelper);
            transaction = Context.Database.BeginTransaction();
        }

        protected ConsentContext CreateNewContextInSameTransaction()
        {
            var newContext = fixture.GetContext(outputHelper, Context.Database.GetDbConnection());
            newContext.Database.UseTransaction(transaction.GetDbTransaction());
            return newContext;
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            transaction?.Dispose();
            Context?.Dispose();
            
        }
    }
}