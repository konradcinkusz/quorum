using MR.Service.Features.Issues;

namespace MR.Server.Controllers;

public class IssueController : MRBaseController
{
    public IssueController(IMapper mapper) : base(mapper)
    {
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("GetIssuesBySearchParams")]
    public async Task<ActionResult<ApiResponse<IssuePagedListDTO>>>
        GetIssuesBySearchParams([FromQuery] IssueSearchParamsDTO searchParams)
    {
        try
        {
            var result = await Mediator.Send(new GetIssuesBySearchParamsQuery
            {
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
                Items = _mapper.Map<List<IssueDTO>>(result),
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
    [HttpPost("CreateIssue")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateIssue(IssueDTO issueDTO)
    {
        string uId = GetUserId();
        var paymentId = await Mediator.Send(new CreateIssueCommand
        {
            ApplicationUserId = uId,
            IssueStatus = (IssueStatus)issueDTO.IssueStatus,
            IsVerifyByAdmin = false,
            Question = issueDTO.Question,
            Title = issueDTO.Title,
        });

        return new ApiResponse<Guid> { Data = paymentId };
    }
}
