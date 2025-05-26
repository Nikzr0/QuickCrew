using AutoMapper;
using QuickCrew.Data.Entities;
using QuickCrew.Shared.Models;
using System.Linq;

namespace QuickCrew.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Location, LocationDto>()
                .ForMember(dest => dest.FullAddress,
                                   opt => opt.MapFrom(src => $"{src.Address}, {src.City}, {src.State} {src.ZipCode}"));
            CreateMap<LocationDto, Location>();

            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>();

            CreateMap<JobPosting, JobPostingDto>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.Name))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Reviews.Any() ? src.Reviews.Average(r => r.Rating) : 0.0));

            CreateMap<JobPostingDto, JobPosting>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore());

            CreateMap<Application, ApplicationDto>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobPosting.Title))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<ApplicationDto, Application>()
                .ForMember(dest => dest.JobPosting, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.AppliedAt, opt => opt.Ignore());

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.Reviewer.Name))
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobPosting.Title))
                .ForMember(dest => dest.ReviewedAt, opt => opt.MapFrom(src => src.ReviewedAt.ToLocalTime()));

            CreateMap<ReviewDto, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Reviewer, opt => opt.Ignore())
                .ForMember(dest => dest.JobPosting, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedAt, opt => opt.Ignore());
        }
    }
}