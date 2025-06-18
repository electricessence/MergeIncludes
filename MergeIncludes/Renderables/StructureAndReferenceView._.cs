using Spectre.Console.Rendering;

namespace MergeIncludes.Renderables;

/// <summary>
/// Renders the side-by-side folder structure and reference trees view using Tree widgets
/// Implementation split across partial classes:
/// - StructureAndReferenceView.Core.cs: Core functionality and table creation
/// - StructureAndReferenceView.ReferenceTree.cs: Reference tree building logic
/// - StructureAndReferenceView.FolderTree.cs: Folder tree building logic  
/// - StructureAndReferenceView.SmartContext.cs: Smart folder context and display logic
/// </summary>
public sealed partial class StructureAndReferenceView : IRenderable
{
}
