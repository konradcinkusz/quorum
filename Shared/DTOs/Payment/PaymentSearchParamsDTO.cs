namespace MR.Shared.DTOs.Payment;

public class PaymentSearchParamsDTO : SearchParams
{
    public string ApplicationUserId { get; set; } = string.Empty;
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SortColumn { get; set; } = "Id";
}

