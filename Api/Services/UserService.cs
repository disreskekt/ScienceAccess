using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using TicketTask = Api.Models.Task;

namespace Api.Services;

public class UserService
{
    private readonly Context _db;
    private readonly IMapper _mapper;
    
    public UserService(Context db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task ChangePassword(ChangePassword changePasswordModel, int userId)
    {
        User user = await _db.Users.FindAsync(userId);

        if (user is null)
        {
            throw new Exception("User doesn't exist");
        }

        if (user.Password != changePasswordModel.OldPassword.GenerateVerySecretHash(user.Email))
        {
            throw new Exception("Invalid password");
        }

        user.Password = changePasswordModel.NewPassword.GenerateVerySecretHash(user.Email);

        await _db.SaveChangesAsync();
    }

    public async Task<List<AllUsersDto>> GetAll()
    {
        List<User> usersList = await _db.Users
            .Include(user => user.Tickets)
            .ThenInclude(ticket => ticket.Task)
            .ToListAsync();

        List<AllUsersDto> userDtosList = _mapper.Map<List<AllUsersDto>>(usersList);

        return userDtosList;
    }

    public async Task<UserDto> GetUser(int id)
    {
        User user = await _db.Users
            .Include(user => user.Role)
            .Include(user => user.Tickets)
            .ThenInclude(ticket => ticket.Task)
            .FirstOrDefaultAsync(user => user.Id == id);

        if (user is null)
        {
            throw new Exception("User doesn't exist");
        }

        UserDto userDto = _mapper.Map<UserDto>(user);

        return userDto;
    }

    public async Task<GetMyselfDto> GetMyself(int userId)
    {
        User user = await _db.Users
            .Include(user => user.Role)
            .Include(user => user.Tickets)
            .ThenInclude(ticket => ticket.Task)
            .FirstOrDefaultAsync(user => user.Id == userId);

        if (user is null)
        {
            throw new Exception("User doesn't exist");
        }

        GetMyselfDto getMyselfDto = _mapper.Map<GetMyselfDto>(user);

        return getMyselfDto;
    }
}