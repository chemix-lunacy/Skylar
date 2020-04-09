using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Skyling.Core.Concepts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Logic
{
    [DebuggerDisplay("Connection: {this.ReturnStatement}. Traits: {this.Traits}")]
    public class ReturnConnection : IInputConnection, IOutputConnection
    {
        public ReturnConnection(ReturnStatementSyntax ret, TraitCollection traits)
        {
            ReturnStatement = ret;
            Traits = traits;
        }

        TraitCollection Traits { get; set; } = new TraitCollection();

        ReturnStatementSyntax ReturnStatement { get; set; }
    }
}
