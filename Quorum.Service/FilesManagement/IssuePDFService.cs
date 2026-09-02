namespace Quorum.Service.FilesManagement
{
    using MigraDoc.DocumentObjectModel;
    using MigraDoc.DocumentObjectModel.Tables;
    using MigraDoc.Rendering;
    using PdfSharp.Fonts;
    using PdfSharp.WPFonts;

    internal interface IIssuePDFService
    {
        byte[] GeneratePdfBytes(Issue issue);
        string GetIssuePDFFileName(Issue issue);
    }

    /// <summary>
    /// Resolves every requested typeface to a Segoe WP face embedded in the PDFsharp package.
    /// </summary>
    /// <remarks>
    /// <para>
    /// PDFsharp 6's cross-platform build has no font of its own and does not read the host's
    /// font directory. Without a resolver it throws at render time, so the container would
    /// fail to produce a sheet while every unit test that never rendered one passed. Binding
    /// to <c>PdfSharp.WPFonts</c> — which ships the font bytes as embedded resources inside
    /// the package — removes the host from the question entirely: the output is identical on
    /// a developer's machine, in CI, and in the deployed image, and installing or removing a
    /// system font cannot change it.
    /// </para>
    /// <para>
    /// The resolver is deliberately <i>total</i>. It never returns <c>null</c>, so an
    /// unexpected family name yields a readable document rather than an exception on a code
    /// path nothing exercises. There is exactly one font family here and no italic face, so
    /// italics are simulated by PDFsharp; that is a cosmetic compromise on a document that
    /// currently uses neither.
    /// </para>
    /// </remarks>
    internal sealed class EmbeddedSegoeFontResolver : IFontResolver
    {
        internal const string FamilyName = "Segoe WP";

        const string Regular = "SegoeWP";
        const string Bold = "SegoeWPBold";

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            => new FontResolverInfo(isBold ? Bold : Regular, false, isItalic);

        public byte[] GetFont(string faceName)
            => faceName == Bold ? FontDataHelper.SegoeWPBold : FontDataHelper.SegoeWP;
    }

    internal class IssuePDFService : IIssuePDFService
    {
        /// <summary>
        /// PDFsharp requires the font resolver to be installed once, before any font operation.
        /// A <see cref="Lazy{T}"/> gives that guarantee under the concurrent requests a web
        /// application serves; assigning the property directly from the constructor would race,
        /// and this service is registered per scope.
        /// </summary>
        static readonly Lazy<bool> FontResolverInstalled = new(() =>
        {
            GlobalFontSettings.FontResolver = new EmbeddedSegoeFontResolver();
            return true;
        });

        public string GetIssuePDFFileName(Issue issue)
        {
            // Generate the timestamp and format it as a string
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Remove invalid characters from the issue title and replace spaces with underscores
            string issueTitle = string.Join("_", issue.Title.Split(Path.GetInvalidFileNameChars()));

            return $"{timestamp}_{issueTitle}.pdf";
        }

        public byte[] GeneratePdfBytes(Issue issue)
        {
            _ = FontResolverInstalled.Value;

            var document = new Document();
            document.Info.Title = issue?.Title ?? string.Empty;
            document.Styles.Normal.Font.Name = EmbeddedSegoeFontResolver.FamilyName;
            document.Styles.Normal.Font.Size = Unit.FromPoint(11);

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;

            section.Add(BuildTable(issue));

            var renderer = new PdfDocumentRenderer();
            renderer.Document = document;
            renderer.RenderDocument();

            // Restate the title on the rendered document rather than relying on MigraDoc to
            // carry Document.Info across. It is the one field a reader of the finished file can
            // check without decoding an embedded font's encoding out of a compressed content
            // stream, so the tests assert on it and it should be true by construction.
            renderer.PdfDocument.Info.Title = document.Info.Title;

            using var pdfStream = new MemoryStream();

            // false: leave the stream open so the bytes can be read back out of it. The
            // MemoryStream is disposed by the using above, after ToArray has copied them.
            renderer.Save(pdfStream, false);

            return pdfStream.ToArray();
        }

        static Table BuildTable(Issue issue)
        {
            var table = new Table();
            table.Borders.Width = Unit.FromPoint(0.75);

            // A4 is 21 cm and MigraDoc's default side margins are 2.5 cm each, so 16 cm is the
            // full measure. Splitting it 6/10 keeps the label column narrow enough that the
            // history dates below it do not wrap.
            table.AddColumn(Unit.FromCentimeter(6));
            table.AddColumn(Unit.FromCentimeter(10));

            if (issue == null)
            {
                AddFullWidthRow(table, "Issue data is missing.");
                return table;
            }

            var titleCell = table.AddRow()[0];
            titleCell.MergeRight = 1;
            titleCell.Format.Alignment = ParagraphAlignment.Center;
            titleCell.Format.Font.Bold = true;
            titleCell.Format.Font.Size = Unit.FromPoint(16);
            titleCell.AddParagraph(issue.Title ?? string.Empty);

            AddFullWidthRow(table, issue.Question ?? string.Empty);

            // The denormalised column is preferred over the navigation on purpose, and this
            // is the site where the difference matters most: this document is printed, signed
            // by hand, and submitted. It must show the address of whoever filed the initiative
            // at the time of filing, not whatever that account's email happens to be when the
            // PDF is regenerated. See Issue.CreatedByEmail and ADR 0001.
            //
            // Previously this dereferenced a CreatedBy navigation with no null check, so any
            // issue whose creator had been removed threw here rather than producing a sheet.
            AddLabelledRow(table, "Filed by", issue.CreatedByEmail ?? string.Empty);
            AddLabelledRow(table, "Verification", issue.IsVerifyByAdmin ? "Verified" : "Not Verified");
            AddLabelledRow(table, "Rating", issue.RatingValue.ToString(CultureInfo.InvariantCulture));

            if (issue.IssueProcessingHistories != null)
            {
                foreach (var history in issue.IssueProcessingHistories)
                {
                    AddLabelledRow(
                        table,
                        history.IssueProcess.ToString(),
                        history.CreatedAt.ToString("g", CultureInfo.InvariantCulture));
                }
            }

            return table;
        }

        static void AddFullWidthRow(Table table, string text)
        {
            var cell = table.AddRow()[0];
            cell.MergeRight = 1;
            cell.AddParagraph(text);
        }

        static void AddLabelledRow(Table table, string label, string value)
        {
            var row = table.AddRow();
            row[0].AddParagraph(label);
            row[1].AddParagraph(value);
        }
    }
}
