using MR.Shared.DTOs.Subscription;

namespace MR.Server.Mappings;

public class SubscriptionProfile : Profile
{
    public SubscriptionProfile()
    {
        CreateMap<Subscription, SubscriptionDTO>();
        CreateMap<SubscriptionDTO, Subscription>();  
    }
}
