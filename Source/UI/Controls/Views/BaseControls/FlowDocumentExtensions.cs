using System.Globalization;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace Sanguosha.UI.Controls;

public static class FlowDocumentExtensions
{
    private static IEnumerable<TextElement> GetRunsAndParagraphs(FlowDocument doc)
    {
        for (TextPointer position = doc.ContentStart;
          position != null && position.CompareTo(doc.ContentEnd) <= 0;
          position = position.GetNextContextPosition(LogicalDirection.Forward))
        {
            if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
            {
                if (position.Parent is Run run)
                {
                    yield return run;
                }
                else
                {
                    if (position.Parent is Paragraph para)
                    {
                        yield return para;
                    }
                }
            }
        }
    }

    public static FormattedText GetFormattedText(this FlowDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);
        var output = new FormattedText(
          GetText(doc),
          CultureInfo.CurrentCulture,
          doc.FlowDirection,
          new Typeface(doc.FontFamily, doc.FontStyle, doc.FontWeight, doc.FontStretch),
          doc.FontSize,
          doc.Foreground,
          1.25);

        int offset = 0;

        foreach (var el in GetRunsAndParagraphs(doc))
        {
            if (el is Run run)
            {
                int count = run.Text.Length;

                output.SetFontFamily(run.FontFamily, offset, count);
                output.SetFontStyle(run.FontStyle, offset, count);
                output.SetFontWeight(run.FontWeight, offset, count);
                output.SetFontSize(run.FontSize, offset, count);
                output.SetForegroundBrush(run.Foreground, offset, count);
                output.SetFontStretch(run.FontStretch, offset, count);
                output.SetTextDecorations(run.TextDecorations, offset, count);

                offset += count;
            }
            else
            {
                offset += Environment.NewLine.Length;
            }
        }

        return output;
    }

    private static string GetText(FlowDocument doc)
    {
        var sb = new StringBuilder();

        foreach (var el in GetRunsAndParagraphs(doc))
        {
            sb.Append(el is not Run run ? Environment.NewLine : run.Text);
        }
        return sb.ToString();
    }
}
