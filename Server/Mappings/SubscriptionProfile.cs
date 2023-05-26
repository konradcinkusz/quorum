namespace MR.Server.Mappings;

public class SubscriptionProfile : Profile
{
    public SubscriptionProfile()
    {
        CreateMap<Subscription, SubscriptionDTO>()
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.ApplicationUser.Email));
        CreateMap<SubscriptionDTO, Subscription>();

        CreateMap<Subscription, SubscriptionReadDTO>();
    }
}
