using EasyPark.Api.Data;
using EasyPark.Api.Models;
using EasyPark.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Infrastructure.Repositories;

public class UsuarioRepository : IUserRepository
{
    private readonly EasyParkContext _context;

    public UsuarioRepository(EasyParkContext context)
    {
        _context = context;
    }

    public void Add(Usuario usuario) => _context.Usuarios.Add(usuario);

    public Task<Usuario?> FindByIdAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default)
        => (asNoTracking ? _context.Usuarios.AsNoTracking() : _context.Usuarios)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> FindByEmailAsync(string email, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return (asNoTracking ? _context.Usuarios.AsNoTracking() : _context.Usuarios)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
    }
}
