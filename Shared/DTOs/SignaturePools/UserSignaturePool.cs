namespace MR.Shared.DTOs.SignaturePools;

public class UserSignaturePool : BaseDTO
{
    public int Year { get; set; }
    public int QuarterNumber { get; set; }
    public List<SignatureDTO> SignatureDTOs { get; set; }
}