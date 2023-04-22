namespace MR.Server.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentStatusHistory, PaymentStatusHistoryDTO>();
        CreateMap<Payment, PaymentDTO>();

        CreateMap<PaymentStatusHistoryDTO, PaymentStatusHistory>();
        CreateMap<PaymentDTO, Payment>()
            .ForMember(dest => dest.PaymentStatusHistories, opt => opt.MapFrom(src => src.PaymentStatusHistories));

        CreateMap<PaymentUpdateDTO, EditPaymentCommand>();
    }
}