﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain;

namespace Otus.Teaching.PromoCodeFactory.DataAccess.Repositories;

public class EfRepository<T>
    : IRepository<T>
    where T : BaseEntity {
    private readonly DataContext _dataContext;

    public EfRepository(DataContext dataContext) => _dataContext = dataContext;

    public async Task<IEnumerable<T>> GetAllAsync() {
        List<T> entities = await _dataContext.Set<T>().ToListAsync();

        return entities;
    }

    public async Task<T> GetByIdAsync(Guid id) {
        T entity = await _dataContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

        return entity;
    }

    public async Task<IEnumerable<T>> GetRangeByIdsAsync(List<Guid> ids) {
        List<T> entities = await _dataContext.Set<T>().Where(x => ids.Contains(x.Id)).ToListAsync();
        return entities;
    }

    public async Task AddAsync(T entity) {
        await _dataContext.Set<T>().AddAsync(entity);

        await _dataContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity) => await _dataContext.SaveChangesAsync();

    public async Task DeleteAsync(T entity) {
        _dataContext.Set<T>().Remove(entity);
        await _dataContext.SaveChangesAsync();
    }
}