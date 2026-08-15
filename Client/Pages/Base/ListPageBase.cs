namespace Quorum.Client.Pages.Base;

public abstract class ListPageBase<T, S> : ComponentBase 
    where T : BaseDTO
    where S : SearchParamsDTO
{
    protected S searchParamsDTO;
    protected PagedListDto<T>? PagedListDTO;
    protected ApiResponse<PagedListDto<T>> apiResponse = null;

    protected abstract void InitializeSearchParams();
    protected abstract Task<ApiResponse<PagedListDto<T>>> Get(S searchParamsDTO);

    protected override async Task OnInitializedAsync()
    {
        InitializeSearchParams();
        await Refresh();
    }

    protected virtual async Task Refresh()
    {
        apiResponse = await Get(searchParamsDTO);
        PagedListDTO = apiResponse.Data;
        searchParamsDTO.CurrentPage = searchParamsDTO.CurrentPage;
        searchParamsDTO.PageSize = searchParamsDTO.PageSize;
    }
}
