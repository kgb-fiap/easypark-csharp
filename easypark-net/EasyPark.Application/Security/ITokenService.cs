using EasyPark.Api.Dtos;
using EasyPark.Api.Models;

namespace EasyPark.Application.Security;

public interface ITokenService
{
    AuthResponseDto CreateToken(Usuario usuario);
}
