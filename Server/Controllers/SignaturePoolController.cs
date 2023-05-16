namespace MR.Server.Controllers;

[Authorize(Policy = Policies.RequireAdminRole)]
public class SignaturePoolController : MRBaseController
{
    public SignaturePoolController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet(nameof(GetSignaturePoolsBySearchParams))]
    public async Task<ActionResult<ApiResponse<SignaturePoolsPagedListDTO>>>
        GetSignaturePoolsBySearchParams([FromQuery] SignaturePoolsSearchParamsDTO searchParams)
    {
        try
        {
            var result = await Mediator.Send(new GetSignaturePoolsBySearchParamsQuery
            {
                Year = searchParams.Year,
                Quarter = searchParams.Quarter,
                ApplicationUserId = searchParams.ApplicationUserId,
                ApplicationUserEmail = searchParams.ApplicationUserEmail,
                Begin = searchParams.Begin,
                End = searchParams.End,
                SearchParams = new SearchParams
                {
                    CurrentPage = searchParams.CurrentPage,
                    PageSize = searchParams.PageSize
                },
                SortColumn = searchParams.SortColumn,
                SortOrder = (Microsoft.Data.SqlClient.SortOrder)searchParams.SortOrder
            });

            var SignaturePoolPagedListDto = new SignaturePoolsPagedListDTO
            {
                Items = _mapper.Map<List<SignaturePoolDTO>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<SignaturePoolsPagedListDTO>(SignaturePoolPagedListDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse<SignaturePoolsPagedListDTO>(new SignaturePoolsPagedListDTO()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }
}
