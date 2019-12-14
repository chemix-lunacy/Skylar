using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Skyling.Core.Parser.TreeWalkers
{
    public class TypeWalker : BaseLogicWalker
    {
        public List<ConstructorWalker> Constructors { get; } = new List<ConstructorWalker>();

        public List<MethodWalker> Methods { get; } = new List<MethodWalker>();
        
        public List<DestructorWalker> Destructors { get; } = new List<DestructorWalker>();

        public List<OperatorWalker> Operators { get; } = new List<OperatorWalker>();

        public List<TypeWalker> InnerClasses { get; } = new List<TypeWalker>();

        public List<StructWalker> InnerStructs { get; } = new List<StructWalker>();

        public List<EnumWalker> InnerEnums { get; } = new List<EnumWalker>();

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            ISymbol declarationSymbol = semanticModel.GetDeclaredSymbol(node);
            Identity = declarationSymbol;
            if (!SymbolEqualityComparer.Default.Equals(Identity, declarationSymbol))
            {
                StructWalker walker = this.CreateSyntaxWalker<StructWalker>();
                walker.Visit(node);
                this.InnerStructs.Add(walker);
            }
            
            base.VisitStructDeclaration(node);
        }

        #region Declarations

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            ISymbol declarationSymbol = semanticModel.GetDeclaredSymbol(node);
            Identity = declarationSymbol;
            if (!SymbolEqualityComparer.Default.Equals(Identity, semanticModel.GetDeclaredSymbol(node)))
            {
                EnumWalker walker = this.CreateSyntaxWalker<EnumWalker>();
                walker.Visit(node);
                this.InnerEnums.Add(walker);
            }

            base.VisitEnumDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            ISymbol declarationSymbol = semanticModel.GetDeclaredSymbol(node);
            Identity = declarationSymbol;
            if (!SymbolEqualityComparer.Default.Equals(Identity, declarationSymbol))
            {
                TypeWalker walker = this.CreateSyntaxWalker<TypeWalker>();
                walker.Visit(node);
                this.InnerClasses.Add(walker);
            }

            base.VisitClassDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            ConstructorWalker walker = this.CreateSyntaxWalker<ConstructorWalker>();
            walker.Visit(node);
            this.Constructors.Add(walker);

            base.VisitConstructorDeclaration(node);
        }

        public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            DestructorWalker walker = this.CreateSyntaxWalker<DestructorWalker>();
            walker.Visit(node);
            this.Destructors.Add(walker);

            base.VisitDestructorDeclaration(node);
        }
        
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            MethodWalker walker = this.CreateSyntaxWalker<MethodWalker>();
            walker.Visit(node);
            this.Methods.Add(walker);

            base.VisitMethodDeclaration(node);
        }

        public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            OperatorWalker walker = this.CreateSyntaxWalker<OperatorWalker>();
            walker.Visit(node);
            this.Operators.Add(walker);

            base.VisitOperatorDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            base.VisitPropertyDeclaration(node);
        }

        #endregion
    }
}
