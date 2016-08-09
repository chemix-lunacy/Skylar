using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace Skyling.Core.Parser.TreeWalkers
{
    /// <summary>
    /// Walks the file finding all type declarations.
    /// </summary>
    [DebuggerDisplay("{File ?? Identity?.Name ?? this.GetType().ToString()}")]
    public class FileWalker : BaseLogicWalker
    {
        public string File { get; set; }

        public List<TypeWalker> Classes { get; } = new List<TypeWalker>();

        public List<StructWalker> Structs { get; } = new List<StructWalker>();

        public List<EnumWalker> Enums { get; } = new List<EnumWalker>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            TypeWalker typeWalker = this.CreateSyntaxWalker<TypeWalker>();
            typeWalker.Visit(node);
            Classes.Add(typeWalker);

            base.VisitClassDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            StructWalker walker = this.CreateSyntaxWalker<StructWalker>();
            walker.Visit(node);
            this.Structs.Add(walker);

            base.VisitStructDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            EnumWalker walker = this.CreateSyntaxWalker<EnumWalker>();
            walker.Visit(node);
            this.Enums.Add(walker);

            base.VisitEnumDeclaration(node);
        }
    }
}
