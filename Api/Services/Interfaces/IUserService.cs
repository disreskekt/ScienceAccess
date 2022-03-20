using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models.Dtos;

namespace Api.Services.Interfaces;

public interface IUserService
{
    public Task ChangePassword(ChangePassword changePasswordModel, int userId);

    public Task<List<AllUsersDto>> GetAll();

    public Task<UserDto> GetUser(int id);

    public Task<GetMyselfDto> GetMyself(int userId);
}