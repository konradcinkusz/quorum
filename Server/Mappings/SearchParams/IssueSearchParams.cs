namespace MR.Server.Mappings.SearchParamsMapping;

public static class IssueSearchParams
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

        AddSearchParamsToCommand(command, searchParams);
    }
}
