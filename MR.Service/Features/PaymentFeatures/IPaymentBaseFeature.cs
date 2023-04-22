namespace MR.Service.Features.PaymentFeatures;

public interface IPaymentBaseFeature
{
    PaymentStatus PaymentStatus { get; set; }
    string ApplicationUserId { get; set; }
    decimal PaymentValuePLN { get; set; }
    string PaymentMethod { get; set; } // the payment method used (e.g. credit card, PayPal, etc.)
    string ReferenceNumber { get; set; }// a reference number associated with the payment (e.g. transaction ID)

}
