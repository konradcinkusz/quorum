namespace MR.Server.Mappings;

public class SubscriptionProfile : Profile
{
    public SubscriptionProfile()
    {
        CreateMap<Subscription, SubscriptionDTO>()
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.ApplicationUser.Email))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive()))
            .ForMember(dest => dest.PaymentDTOs, opt => opt.MapFrom(src=>src.SubscriptionPayments.Select(x=>x.Payment).ToList()));
        CreateMap<SubscriptionDTO, Subscription>();

        CreateMap<Subscription, SubscriptionReadDTO>();
    }
}
