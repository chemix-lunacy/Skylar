using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Parser
{
    public abstract class ReturnsRewriter : CSharpSyntaxRewriter
    {
        protected bool ShouldRewrite(ReturnStatementSyntax ret)
        {
            if (ret == null)
                return false;
            
            ExpressionSyntax returnExpression = ret.Expression;
            return ret.Expression != null && !(returnExpression is IdentifierNameSyntax);
        }

    }
}
