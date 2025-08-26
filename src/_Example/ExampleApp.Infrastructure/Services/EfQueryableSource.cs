﻿using SecuritySystem.Services;

namespace ExampleWebApp.Infrastructure.Services;

public class EfQueryableSource(TestDbContext dbContext) : IQueryableSource
{
    public IQueryable<TDomainObject> GetQueryable<TDomainObject>()
        where TDomainObject : class
    {
        return dbContext.Set<TDomainObject>();
    }
}