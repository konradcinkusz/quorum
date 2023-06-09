namespace MR.Shared.DTOs.Subscription;

public class SubscriptionReadDTO
{
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public PaymentStatusEnum? PaymentStatus { get; set; }
    public DateTime? PaymentDate { get; set; }
    public bool IsActive { get; set; }
    public SubscriptionViewStatusEnum SubscriptionViewStatusEnum { get; set; } = SubscriptionViewStatusEnum.YouDontHaveActiveSub;
}

public enum SubscriptionViewStatusEnum
{
    YouDontHaveActiveSub,
    //"You have bought subscription and we are waiting for your payment."
    SubBoughtAndWaitingForPayment,
    //"You have bought subscription but something went wrong with payment acceptation, contact administrator."
    SubBoughtButSomethingHappendWithAPayment,
    //"You have an active subscription. If you still see InActive in header, refresh your page."
    YouHaveAnActiveSub,
    //"Payment has been accepted. Waiting for admin activation."
    PaymentHasBeenAcceptedWaitingForAdminActivation,
    NoPaymentYouHaveToBuySubscription
}