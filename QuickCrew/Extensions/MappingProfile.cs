// QuickCrew/Extensions/MappingProfile.cs
using AutoMapper;
using QuickCrew.Data.Entities;
using QuickCrew.Shared.Models;

namespace QuickCrew.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity → DTO
            CreateMap<JobPosting, JobPostingDto>();

            CreateMap<JobPostingDto, JobPosting>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore());

            CreateMap<Category, CategoryDto>().ReverseMap(); // TODO: Currently one-way mapping, should be upgraded!

            CreateMap<Application, ApplicationDto>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobPosting.Title))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<ApplicationDto, Application>()
                .ForMember(dest => dest.JobPosting, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.AppliedAt, opt => opt.Ignore()); // Set server-side


            CreateMap<Location, LocationDto>()
                .ForMember(dest => dest.FullAddress,
                    opt => opt.MapFrom(src => $"{src.Address}, {src.City}, {src.State} {src.ZipCode}"));

            CreateMap<LocationDto, Location>();

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.Reviewer.Name))
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobPosting.Title))
                .ForMember(dest => dest.ReviewedAt, opt => opt.MapFrom(src => src.ReviewedAt.ToLocalTime()));

            CreateMap<ReviewDto, Review>()
                .ForMember(dest => dest.Reviewer, opt => opt.Ignore())
                .ForMember(dest => dest.JobPosting, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedAt, opt => opt.Ignore()); // Задава се автоматично
        }
    }
}