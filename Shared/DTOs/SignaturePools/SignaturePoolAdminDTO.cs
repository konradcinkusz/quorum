namespace MR.Shared.DTOs.SignaturePools;

public class SignaturePoolAdminDTO : BaseDTO
{
    public string ApplicationUserId { get; set; }
    public string ApplicationUserEmail { get; set; }
    public List<SignatureDTO> SignatureDTOs { get; set; }
    public QuarterDTO QuarterDTO { get; set; }
}