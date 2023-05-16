using MR.Shared.DTOs.Issue;

namespace MR.Shared.DTOs.Signature;

public class SignatureDTO : BaseDTO
{
    public IssueDTO? IssueDTO { get; set; }
}
