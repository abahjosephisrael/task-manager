using AutoMapper;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Projects.Commands;
using TaskManager.Application.Features.Tasks.Commands;
using TaskManager.Application.Features.Users.Commands;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<CreateProjectCommand, Project>().ReverseMap();
            CreateMap<ProjectResponse, Project>().ReverseMap();
            CreateMap<ProjectResponse, UpdateProjectCommand>().ReverseMap();
            CreateMap<Project, UpdateProjectCommand>().ReverseMap();
            CreateMap<ProjectResponse, CreateProjectCommand>().ReverseMap();
            CreateMap<CreateUserCommand, CreateUser>().ReverseMap();
            CreateMap<UpdateUserCommand, UpdateUser>().ReverseMap();
            CreateMap<LoginCommand, Login>().ReverseMap();
            CreateMap<CreateTaskCommand, Domain.Entities.Task>().ReverseMap();
            CreateMap<TaskResponse, Domain.Entities.Task>().ReverseMap();
            //CreateMap<,>().ReverseMap();
        }
    }
}
