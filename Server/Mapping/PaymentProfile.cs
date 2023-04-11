using MR.Domain.Entities;
using MR.Domain.Enums;
using MR.Shared.ViewModel;

namespace MR.Server.Mapping;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, PaymentModel>();

        CreateMap<Payment, Infrastructure.ViewModel.PaymentViewModel>()
            .ForMember(dest => dest.PaymentStatus,
                       opt => opt.MapFrom(src => Enum.Parse<PaymentStatus>(src.PaymentStatus)))
            .ReverseMap();
    }
}