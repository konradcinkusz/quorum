namespace MR.Service.Features.Issues.PDF
{
    using iTextSharp.text.pdf;
    using iTextSharp.text;

    public class GeneratePDFForAnIssueCommand : IRequest<string>
    {
        readonly Guid _issueId;

        public GeneratePDFForAnIssueCommand(Guid issueId)
        {
            _issueId = issueId;
        }

        internal sealed class GeneratePDFForAnIssueCommandHandler : CommandHandlerBase<GeneratePDFForAnIssueCommand, string>
        {
            public GeneratePDFForAnIssueCommandHandler(IApplicationDbContext context, ILogger<GeneratePDFForAnIssueCommand> logger) : base(context, logger)
            {
            }

            public override async Task<string> Handle(GeneratePDFForAnIssueCommand request, CancellationToken cancellationToken)
            {
                var issue = await _context.Issues.Include(x => x.CreatedBy).Include(x => x.IssueProcessingHistories)
                    .FirstOrDefaultAsync(x => x.Id == request._issueId);

                GeneratePDF(issue, "90010413150", "Konrad Cinkusz", "C://Repos");

                return "C://Repos";
            }

            void GeneratePDF(Issue issue, string pesel, string signature, string outputPath)
            {
                // Create the document and set the page size
                Document document = new Document();
                document.SetPageSize(PageSize.A4);

                // Get the current execution path where the project files are located
                string currentExecutionPath = AppDomain.CurrentDomain.BaseDirectory;
                // Create a PDF writer to write the document to a file in the current execution path
                string outputFile = Path.Combine(currentExecutionPath, "output.pdf");
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));

                // Open the document
                document.Open();

                // Create a table to hold the information
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;

                // Add the title to the table
                PdfPCell titleCell = new PdfPCell(new Phrase(issue.Title));
                titleCell.Colspan = 2;
                titleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                titleCell.PaddingBottom = 10;
                table.AddCell(titleCell);

                // Add the question to the table
                PdfPCell questionCell = new PdfPCell(new Phrase(issue.Question));
                questionCell.Colspan = 2;
                questionCell.PaddingBottom = 10;
                table.AddCell(questionCell);

                // Add the email and verification status to the table
                PdfPCell emailCell = new PdfPCell(new Phrase(issue.CreatedBy.Email));
                PdfPCell verificationCell = new PdfPCell(new Phrase(issue.IsVerifyByAdmin ? "Verified" : "Not Verified"));
                table.AddCell(emailCell);
                table.AddCell(verificationCell);

                // Add the rating value to the table
                PdfPCell ratingCell = new PdfPCell(new Phrase(issue.RatingValue.ToString()));
                table.AddCell(ratingCell);

                // Add the PESEL and signature to the table
                PdfPCell peselCell = new PdfPCell(new Phrase(pesel));
                PdfPCell signatureCell = new PdfPCell(new Phrase(signature));
                table.AddCell(peselCell);
                table.AddCell(signatureCell);

                // Add the processing histories to the table
                if (issue.IssueProcessingHistories != null && issue.IssueProcessingHistories.Count > 0)
                {
                    foreach (var history in issue.IssueProcessingHistories)
                    {
                        PdfPCell processCell = new PdfPCell(new Phrase(history.IssueProcess.ToString()));
                        PdfPCell dateCell = new PdfPCell(new Phrase(history.CreatedAt.ToString("g", CultureInfo.InvariantCulture)));
                        table.AddCell(processCell);
                        table.AddCell(dateCell);
                    }
                }

                // Add the table to the document
                document.Add(table);

                // Close the document
                document.Close();
            }
        }
    }
}