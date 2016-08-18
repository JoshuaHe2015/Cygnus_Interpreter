﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cygnus.SymbolTable;
using Cygnus.Errors;
namespace Cygnus.SyntaxTree
{
    public class CallExpression : Expression
    {
        public string Name { get; private set; }
        public Expression[] Arguments { get; private set; }
        public CallExpression(string Name, Expression[] Arguments)
        {
            this.Name = Name;
            this.Arguments = Arguments;
        }
        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Call;
            }
        }
        public override string ToString()
        {
            return "(Function Call: " + Name + ")";
        }
        public override Expression Eval(Scope scope)
        {
            if (FunctionExpression.functionTable.ContainsKey(Name))
            {
                var func = FunctionExpression.functionTable[Name].Update(Arguments, scope);
                return func.Eval(func.funcScope);
            }
            else if (FunctionExpression.builtInMethodTable.ContainsKey(Name))
            {
                return FunctionExpression.builtInMethodTable[Name](Arguments, scope);
            }
            Expression funcExpr;
            if (scope.TryGetValue(Name, out funcExpr))
            {
                if (funcExpr.NodeType == ExpressionType.Call)
                    return new CallExpression((funcExpr as CallExpression).Name, Arguments).Eval(scope);
            }
            throw new NotDefinedException(Name);
        }
    }
}
