namespace MR.Server.Mappings.SearchParamsMapping;

public static class SearchParamsExtension
{
    public static void AddSearchParamsToCommand<T>(T command, SearchParamsDTO searchParamsDTO) where T : QueryBase
    {
        command.SearchParams = new SearchParams
        {
            CurrentPage = searchParamsDTO.CurrentPage,
            PageSize = searchParamsDTO.PageSize
        };
        command.SortColumn = searchParamsDTO.SortColumn;
        command.SortOrder = (Microsoft.Data.SqlClient.SortOrder)searchParamsDTO.SortOrder;
    }

    public static void AddIssueSearchParamsToCommand(GetIssuesBySearchParamsQuery command, IssueSearchParamsDTO searchParams)
    {
        command.IssueId = searchParams.IssueId;
        command.CreatedByEmail = searchParams.CreatedByEmail;
        command.Title = searchParams.Title;
        command.Question = searchParams.Question;
        command.IsVerifyByAdmin = searchParams.IsVerifyByAdmin;
        command.IssueVisibility = (IssueVisibility?)searchParams.IssueVisibility;
        command.RatingValue = searchParams.RatingValue;
        command.HasInitialPayment = searchParams.PaymentOptions != null ? searchParams.PaymentOptions == IssuePaymentOptions.WithInitialPayment : null;
        command.QuarterNumber = searchParams.QuarterNumber;
        command.QuarterYear = searchParams.QuarterYear;
        command.IsDeleted = searchParams.IsDeleted;

        AddSearchParamsToCommand(command, searchParams);
    }

    public static void AddSignaturePoolAdminSearchParamsToCommand(GetSignaturePoolsBySearchParamsQuery command, SignaturePoolAdminSearchParamsDTO searchParams)
    {
        command.Year = searchParams.Year;
        command.Quarter = searchParams.Quarter;
        command.ApplicationUserId = searchParams.ApplicationUserId;
        command.ApplicationUserEmail = searchParams.ApplicationUserEmail;
        command.Begin = searchParams.Begin;
        command.End = searchParams.End;

        AddSearchParamsToCommand(command, searchParams);
    }

    public static void AddSignaturePoolSearchParamsToCommand(GetUserSignaturePools command, SignaturePoolSearchParamsDTO searchParams, string applicationUserId)
    {
        command.Year = searchParams.Year;
        command.Quarter = searchParams.Quarter;
        command.ApplicationUserId = applicationUserId;

        AddSearchParamsToCommand(command, searchParams);
    }

    public static void AddPaymentSearchParamsToCommand(GetPaymentsBySearchParamsQuery command, PaymentSearchParamsDTO searchParams)
    {

        command.PaymentId = searchParams.PaymentId;
        command.ApplicationUserEmail = searchParams.ApplicationUserEmail;
        command.MinPaymentValuePLN = searchParams.MinPaymentValuePLN;
        command.MaxPaymentValuePLN = searchParams.MaxPaymentValuePLN;
        command.OnlyInitialPayment = searchParams.OnlyInitialPayment;

        AddSearchParamsToCommand(command, searchParams);
    }

    public static void AddQuarterSearchParamsToCommand(GetQuartersBySearchParamsQuery command, QuarterSearchParamsDTO searchParams)
    {
        command.QuarterNumber = searchParams.Quarter;
        command.Year = searchParams.Year;
        command.Begin = searchParams.Begin;
        command.End = searchParams.End;

        AddSearchParamsToCommand(command, searchParams);
    }

    public static void AddSubscriptionsSearchParamsToCommand(GetSubscriptionsBySearchParamsQuery command, SubscriptionSearchParamsDTO searchParams)
    {
        command.ApplicationUserId = searchParams.ApplicationUserId;
        command.ApplicationUserEmail = searchParams.ApplicationUserEmail;
        command.Begin = searchParams.Begin;
        command.End = searchParams.End;
        command.Activity = (GetSubscriptionsBySearchParamsQuery.ActivityEnum?)searchParams.Activity;

        AddSearchParamsToCommand(command, searchParams);
    }
}
