namespace MR.Service.Features.SubscriptionFeatures;

public interface ISubscriptionBaseCommand
{
    string ApplicationUserId { get; set; }
    DateTime? Begin { get; set; }
    DateTime? End { get; set; }
}
