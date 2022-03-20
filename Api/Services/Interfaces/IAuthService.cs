using System.Threading.Tasks;
using Api.Models.Dtos;

namespace Api.Services.Interfaces;

public interface IAuthService
{
    public Task Register(Register register);
    public Task<string> Login(Login login);
}