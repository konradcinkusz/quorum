namespace MR.Server.Controllers;

public class IssueController : MRBaseController
{
    public IssueController(IMapper mapper) : base(mapper)
    {
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("get-issues-by-search-params")]
    public async Task<ActionResult<ApiResponse<IssuePagedListDTO>>>
        GetIssuesBySearchParams([FromQuery] IssueSearchParamsDTO searchParams)
    {
        try
        {
            var result = await Mediator.Send(new GetIssuesBySearchParamsQuery
            {
                CreatedByEmail = searchParams.CreatedByEmail,
                SearchParams = new SearchParams
                {
                    CurrentPage = searchParams.CurrentPage,
                    PageSize = searchParams.PageSize
                },
                SortColumn = searchParams.SortColumn,
                SortOrder = (Microsoft.Data.SqlClient.SortOrder)searchParams.SortOrder
            });

            var IssuePoolPagedListDto = new IssuePagedListDTO
            {
                Items = _mapper.Map<List<IssueReadDTO>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<IssuePagedListDTO>(IssuePoolPagedListDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IssuePagedListDTO>(new IssuePagedListDTO()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("create-issue-by-admin")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateIssue(IssueAdminCreateDTO issueDTO)
    {
        var paymentId = await Mediator.Send(new CreateIssueCommand
        {
            ApplicationUserId = string.IsNullOrEmpty(issueDTO.ApplicationUserId) ? GetUserId() : issueDTO.ApplicationUserId,
            IssueStatus = (IssueStatus)issueDTO.IssueStatus,
            IsVerifyByAdmin = issueDTO.IsVerifyByAdmin,
            RatingValue = issueDTO.RatingValue,
            Question = issueDTO.Question,
            Title = issueDTO.Title,
            Icon = issueDTO.Icon,
            BackgroundColor = issueDTO.BackgroundColor,
        });

        return new ApiResponse<Guid> { Data = paymentId };
    }
}
