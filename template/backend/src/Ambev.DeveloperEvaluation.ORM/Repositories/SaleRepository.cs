using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore; // Required for EF Core operations
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Sale sale)
    {
        await _context.Sales.AddAsync(sale);
        // Note: Unit of Work pattern is often implemented here or at the service layer
        // For simplicity, we'll assume SaveChangesAsync is called elsewhere (e.g., by a UnitOfWork filter/middleware or explicitly in service)
        // await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Sale>> GetAllAsync()
    {
        // Include related items to prevent lazy loading issues or N+1 queries
        return await _context.Sales
                             .Include(s => s.Items) // Ensure items are loaded
                             .AsNoTracking() // Use AsNoTracking for read-only queries
                             .ToListAsync();
    }

    public async Task<Sale?> GetByIdAsync(Guid id)
    {
        // Include related items when fetching a single sale as well
        return await _context.Sales
                             .Include(s => s.Items)
                             .FirstOrDefaultAsync(s => s.Id == id);
    }

    public Task UpdateAsync(Sale sale)
    {
        // EF Core tracks changes, so just marking the entity as Modified is sufficient
        // if it was retrieved and modified within the same context lifetime.
        // If the entity became detached, you might need _context.Sales.Update(sale);
        _context.Entry(sale).State = EntityState.Modified;

        // Again, assuming SaveChangesAsync is handled elsewhere
        // await _context.SaveChangesAsync();
        return Task.CompletedTask; // Update is synchronous in terms of EF state tracking
    }

    // Optional: Implement DeleteAsync if hard deletes are needed
    // public async Task DeleteAsync(Guid id)
    // {
    //     var sale = await _context.Sales.FindAsync(id);
    //     if (sale != null)
    //     {
    //         _context.Sales.Remove(sale);
    //         // await _context.SaveChangesAsync();
    //     }
    // }
}
