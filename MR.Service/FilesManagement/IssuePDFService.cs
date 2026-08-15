namespace MR.Service.FilesManagement
{
    using iTextSharp.text.pdf;
    using iTextSharp.text;

    internal interface IIssuePDFService
    {
        byte[] GeneratePdfBytes(Issue issue);
        string GetIssuePDFFileName(Issue issue);
        string WrtiePDFDocumentToFile(Issue issue);
    }

    internal class IssuePDFService : IIssuePDFService
    {
        public string GetIssuePDFFileName(Issue issue)
        {
            // Generate the timestamp and format it as a string
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Remove invalid characters from the issue title and replace spaces with underscores
            string issueTitle = string.Join("_", issue.Title.Split(Path.GetInvalidFileNameChars()));

            // Create a PDF writer to write the document to a file in the current execution path
            return $"{timestamp}_{issueTitle}.pdf";
        }

        PdfPTable GeneratePdfPTable(Issue issue)
        {
            // Create a table to hold the information
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;

            if (issue == null)
            {
                PdfPCell errorCell = new PdfPCell(new Phrase("Issue data is missing."));
                errorCell.Colspan = 2;
                table.AddCell(errorCell);
                return table;
            }

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

            // Add the email and verification status to the table.
            //
            // The denormalised column is preferred over the navigation on purpose, and this
            // is the site where the difference matters most: this document is printed, signed
            // by hand, and submitted. It must show the address of whoever filed the initiative
            // at the time of filing, not whatever that account's email happens to be when the
            // PDF is regenerated. See Issue.CreatedByEmail and ADR 0001.
            //
            // Previously this dereferenced issue.CreatedBy.Email with no null check, so any
            // issue whose creator had been removed threw here rather than producing a sheet.
            var createdByEmail = issue.CreatedByEmail ?? issue.CreatedBy?.Email ?? string.Empty;
            PdfPCell emailCell = new PdfPCell(new Phrase(createdByEmail));
            PdfPCell verificationCell = new PdfPCell(new Phrase(issue.IsVerifyByAdmin ? "Verified" : "Not Verified"));
            table.AddCell(emailCell);
            table.AddCell(verificationCell);

            // Add the rating value to the table
            PdfPCell ratingCell = new PdfPCell(new Phrase(issue.RatingValue.ToString()));
            table.AddCell(ratingCell);

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

            return table;
        }

        public byte[] GeneratePdfBytes(Issue issue)
        {
            using (MemoryStream pdfStream = new MemoryStream())
            {
                using (Document document = new Document())
                {
                    // Create a PDF writer to write the document to the memory stream
                    PdfWriter writer = PdfWriter.GetInstance(document, pdfStream);

                    document.SetPageSize(PageSize.A4);

                    // Open the document
                    document.Open();

                    // Generate the PDF table using the IssuePDFService
                    var table = GeneratePdfPTable(issue);

                    // Check if the table is empty
                    if (table.Rows.Count == 0)
                    {
                        // Return an empty byte array or an error message, depending on your requirement
                        // For example, you can throw an exception or return a specific error message.
                        // Here, we'll return an empty byte array.
                        return new byte[0];
                    }

                    // Add the table to the document
                    document.Add(table);
                    document.Close();
                }
                // Rest of your PDF generation code goes here...

                // Convert the PDF memory stream to a byte array
                byte[] pdfBytes = pdfStream.ToArray();

                return pdfBytes;
            }
        }

        public string WrtiePDFDocumentToFile(Issue issue)
        {
            // Create the document and set the page size
            Document document = new Document();
            document.SetPageSize(PageSize.A4);

            // Open the document
            document.Open();

            // Generate the PDF table using the IssuePDFService
            var table = GeneratePdfPTable(issue);

            // Add the table to the document
            document.Add(table);
            // Get the current execution path where the project files are located
            string currentExecutionPath = AppDomain.CurrentDomain.BaseDirectory;

            // Create a PDF writer to write the document to a file in the current execution path
            string outputFilePath = Path.Combine(currentExecutionPath, GetIssuePDFFileName(issue));
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputFilePath, FileMode.Create));

            writer.Close();
            document.Close();

            return outputFilePath;
        }
    }
}