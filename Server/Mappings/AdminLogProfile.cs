namespace MR.Server.Mappings;

public class AdminLogProfile : Profile
{
    public AdminLogProfile()
    {
        CreateMap<AdminLog, AdminLogDto>();
        CreateMap<AdminLogDto, AdminLog>();
    }
}

