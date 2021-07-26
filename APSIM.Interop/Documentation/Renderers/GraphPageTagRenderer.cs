using APSIM.Interop.Graphing;
using APSIM.Interop.Markdown.Renderers;
using APSIM.Interop.Utility;
using APSIM.Services.Documentation;

namespace APSIM.Interop.Documentation.Renderers
{
    /// <summary>
    /// A class which can use a <see cref="PdfBuilder" /> to render a
    /// <see cref="GraphPage" /> to a PDF document.
    /// </summary>
    /// <typeparam name="T">The type of tag which this class can render.</typeparam>
    internal class GraphPageTagRenderer : TagRendererBase<GraphPage>
    {
        /// <summary>
        /// Render the given graph page to the PDF document.
        /// </summary>
        /// <param name="GraphPage">Graph page to be rendered.</param>
        /// <param name="renderer">PDF renderer to use for rendering the tag.</param>
        protected override void Render(GraphPage page, PdfBuilder renderer)
        {
            renderer.GetPageSize(out double width, out double height);
            // Let image width = half page width.
            width /= 2;
            // 6 graphs per page - 2 columns of 3 rows.
            // Therefore each graph gets 1/3 total page height.
            height /= 3;

            renderer.StartNewParagraph();
            foreach (Graph graph in page.Graphs)
                renderer.AppendImage(ImageUtilities.ResizeImage(graph.ToImage(900, 600), width, height));
            renderer.StartNewParagraph();
        }
    }
}
