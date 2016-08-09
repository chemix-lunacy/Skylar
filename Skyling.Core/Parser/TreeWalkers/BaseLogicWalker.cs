using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Skyling.Core.Resolvers;
using System.Diagnostics;
using System.Linq;

namespace Skyling.Core.Parser.TreeWalkers
{
    [DebuggerDisplay("{Identity?.Name ?? this.GetType().ToString()}")]
    public abstract class BaseLogicWalker : CSharpSyntaxWalker
    {
        private ISymbol identity = null;

        /// <summary>
        /// The identity of a walker is simply the first significant block we find: method, type, interface, etc.
        /// </summary>
        public ISymbol Identity
        {
            get
            {
                return this.identity;
            }

            protected set
            {
                if (identity == null)
                {
                    this.identity = value;
                }
            }
        }
        
        public bool Incomplete { get; set; } = false;

        public CSharpCompilation Compilation { get; private set; }

        public SolutionResolver AnalysisComposite { get; private set; }

        public void Initalize(CSharpCompilation compilation, SolutionResolver composite)
        {
            this.Compilation = compilation;
            this.AnalysisComposite = composite;
        }

        /// <summary>
        /// Fully initalizes a syntax walker including harvesting variables.
        /// </summary>
        protected T CreateSyntaxWalker<T>() where T : BaseLogicWalker, new()
        {
            T syntaxWalker = new T();
            syntaxWalker.Initalize(this.Compilation, this.AnalysisComposite);
            return syntaxWalker;
        }

        public ISymbol GetAssociatedVariable(SyntaxNode node)
        {
            ISymbol symbol = null;
            SyntaxNode currentNode = node;
            SemanticModel currentModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            while (symbol == null && currentNode != null)
            {
                IdentifierNameSyntax identifier = currentNode as IdentifierNameSyntax;
                if (identifier != null)
                {
                    SymbolInfo info = currentModel.GetSymbolInfo(identifier);
                    symbol = info.Symbol;
                }

                AssignmentExpressionSyntax assignment = currentNode as AssignmentExpressionSyntax;
                if (assignment != null)
                {
                    SymbolInfo info = currentModel.GetSymbolInfo(assignment);
                    symbol = info.Symbol;
                }

                VariableDeclaratorSyntax variableDeclare = currentNode as VariableDeclaratorSyntax;
                if (variableDeclare != null)
                {
                    symbol = currentModel.GetDeclaredSymbol(variableDeclare);
                }

                ArgumentSyntax argSyntax = currentNode as ArgumentSyntax;
                if (argSyntax != null)
                {
                    AnalysisContext context = GetAnalysisContext(node.Parent.Parent.Parent);
                    MethodWalker walker = this.AnalysisComposite.AnalyzeMethod(context);
                    ArgumentListSyntax list = argSyntax.Parent as ArgumentListSyntax;
                    if (context.MethodSymbol != null && list != null)
                    {
                        int ordinal = list.Arguments.IndexOf(argSyntax);
                        symbol = context.MethodSymbol.Parameters.FirstOrDefault(val => val.Ordinal == ordinal);
                    }
                }

                currentNode = currentNode.Parent;
            }

            return symbol;
        }

        public AnalysisContext GetAnalysisContext(SyntaxNode node)
        {
            SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            TypeInfo typeInfo = default(TypeInfo);
            SymbolInfo methodInfo = default(SymbolInfo);

            InvocationExpressionSyntax invocationExpression = node as InvocationExpressionSyntax;
            if (invocationExpression != null)
            {
                MemberAccessExpressionSyntax exp = invocationExpression.Expression as MemberAccessExpressionSyntax;
                if (exp != null)
                {
                    typeInfo = semanticModel.GetTypeInfo(exp.Expression);
                    methodInfo = semanticModel.GetSymbolInfo(exp.Name);
                }
            }

            ObjectCreationExpressionSyntax objCreation = node as ObjectCreationExpressionSyntax;
            if (objCreation != null)
            {
                methodInfo = semanticModel.GetSymbolInfo(node);
                typeInfo = semanticModel.GetTypeInfo(node);
            }

            ITypeSymbol typeSymbol = typeInfo.Type;
            IMethodSymbol methodSymbol = methodInfo.Symbol as IMethodSymbol;
            if (methodInfo.CandidateReason == CandidateReason.OverloadResolutionFailure && methodInfo.CandidateSymbols.Length == 1)
                methodSymbol = methodInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

            return GetAnalysisContext(typeSymbol, methodSymbol);
        }

        public AnalysisContext GetAnalysisContext(ITypeSymbol typeSymbol, IMethodSymbol methodSymbol)
        {
            AnalysisContext ident = new AnalysisContext();

            ILocalSymbol localSymbol = typeSymbol as ILocalSymbol;
            if (localSymbol != null)
                typeSymbol = localSymbol.Type;

            IParameterSymbol paramSymbol = typeSymbol as IParameterSymbol;
            if (paramSymbol != null)
                typeSymbol = paramSymbol.Type;

            IFieldSymbol fieldSymbol = typeSymbol as IFieldSymbol;
            if (fieldSymbol != null)
                typeSymbol = fieldSymbol.Type;

            IPropertySymbol propertySymbol = typeSymbol as IPropertySymbol;
            if (propertySymbol != null)
                typeSymbol = propertySymbol.Type;

            if (typeSymbol != null)
            {
                MetadataReference refer = this.Compilation.GetMetadataReference(typeSymbol.ContainingAssembly);
                ident.AssemblyPath = refer?.Display;
                ident.TypeSymbol = typeSymbol;
            }

            if (methodSymbol != null)
            {
                MetadataReference refer = this.Compilation.GetMetadataReference(methodSymbol.ContainingAssembly);
                ident.AssemblyPath = refer?.Display;
                ident.MethodSymbol = methodSymbol;
            }

            return ident;
        }
    }
}
