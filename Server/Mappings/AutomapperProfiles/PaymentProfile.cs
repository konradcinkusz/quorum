namespace Quorum.Server.Mappings.AutomapperProfiles;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentStatusHistory, PaymentStatusHistoryDTO>();
        CreateMap<Payment, PaymentDTO>()
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.ApplicationUser.Email))
            .ForMember(dest => dest.RelatedIssueGuid, opt => opt.MapFrom(src => src.RelatedIssue != null ? src.RelatedIssue.Id : Guid.Empty));

        CreateMap<PaymentStatusHistoryDTO, PaymentStatusHistory>();
        CreateMap<PaymentDTO, Payment>()
            .ForMember(dest => dest.PaymentStatusHistories, opt => opt.MapFrom(src => src.PaymentStatusHistories));
    }
}
