﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cygnus.Expressions;
using Cygnus.Extensions;
using Cygnus.AssemblyImporter;
using Cygnus.LexicalAnalyzer;
using Cygnus.SyntaxAnalyzer;
using Cygnus.Errors;
using Cygnus.Executors;
using System.IO;
using Cygnus.Settings;
using Utility = Cygnus.Extensions.UtilityFunctions;
using Cygnus.DataStructures;
namespace Cygnus.Libraries
{
    public static class BuiltInFunctions
    {
        public static Expression Print(Expression[] args, Scope scope)
        {
            var obj = args.Single().GetValue(scope);
            if (obj is IDisplayable)
            {
                (obj as IDisplayable).Display(scope);
                Console.WriteLine();
            }
            else
                Console.WriteLine(obj);
            return Expression.Void();
        }
        public static Expression InitList(Expression[] args, Scope scope)
        {
            return new CygnusList(args.Select(i => i.AsConstant(scope).Value).ToList());
        }
        public static Expression InitTable(Expression[] args, Scope scope)
        {
            throw new NotImplementedException();
            //return new TableExpression(args.Cast<ParameterExpression>()
            //    .Select(i => new KeyValuePair<string, Expression>(i.Name, Expression.Null())).ToArray());
        }
        //public static Expression InitVector(Expression[] args, Scope scope)
        //{
        //    var values = args.Map(i => i.AsConstant(scope).GetDouble());
        //    return new VectorExpression(values);
        //}
        public static Expression InitMatrix(Expression[] args, Scope scope)
        {
            throw new NotImplementedException();
            //var rows = new double[args.Length][];
            //for (int i = 0; i < args.Length; i++)
            //{
            //    rows[i] = args[i].AsArray(scope).Values.Map(j => j.AsConstant(scope).GetDouble());
            //}
            //return new MatrixExpression(rows);
        }
        public static Expression Length(Expression[] args, Scope scope)
        {
            throw new NotImplementedException();
            //return (args.Single().GetValue(scope) as IIndexable).Length;
        }
        public static Expression Import(Expression[] args, Scope scope)
        {
            (args.Length == 2).OrThrows<ParameterException>();
            var path = Utility.GetFilePath(Directory.GetCurrentDirectory(), args[0].AsString(scope));
            var info = args[1].AsString(scope);//Namespace.Class
            new CSharpAssembly(path, info).Import();
            return Expression.Void();
        }

        //public static Expression SetParent(Expression[] args, Scope scope)
        //{
        //    (args.Length == 2).OrThrows<ParameterException>();
        //    var table = args[0].AsTable(scope);
        //    var parent_table = args[1].GetValue<TableExpression>(ExpressionType.Table, scope);
        //    table.Parent = parent_table;
        //    return Expression.Void();
        //}
        public static Expression Range(Expression[] args, Scope scope)
        {
            switch (args.Length)
            {
                case 1:
                    return
               Expression.IEnumerable(
               Enumerable.Range(0, args[0].As<int>(scope))
               .Select(i => new CygnusInteger(i)));
                case 2:
                    {
                        int start = args[0].As<int>(scope);
                        int end = args[1].As<int>(scope);
                        return
                       Expression.IEnumerable(
                           Enumerable.Range(start, end - start)
                           .Select(i => new CygnusInteger(i)));
                    }
                case 3:
                    {
                        int start = args[0].As<int>(scope);
                        int end = args[1].As<int>(scope);
                        int step = args[2].As<int>(scope);
                        return
                            Expression.IEnumerable(
                                GetRange(start, end, step)
                                .Select(i => new CygnusInteger(i)));
                    }
                default:
                    throw new ParameterException();
            }
        }
        private static IEnumerable<int> GetRange(int start, int end, int step)
        {
            if (step == 0) throw new ParameterException("The step cannot be zero");
            if (step > 0)
                for (int i = start; i < end; i += step)
                    yield return i;
            else
                for (int i = start; i > end; i += step)
                    yield return i;
        }
        public static Expression ExecuteFile(Expression[] args, Scope scope)
        {
            (args.Length == 1 || args.Length == 2).OrThrows<ParameterException>();
            var FilePath = Utility.GetFilePath(WorkSpace.CurrentWorkSpace, args[0].AsString(scope));
            var encoding = args.Length == 2 ? Utility.GetEncoding(args[1].AsString(scope)) : Encoding.Default;
            return new ExecuteFromFile(FilePath, encoding, scope).Run();
        }
        public static Expression GetWorkingDirectory(Expression[] args, Scope scope)
        {
            return WorkSpace.CurrentWorkSpace;
        }
        public static Expression SetWorkingDirectory(Expression[] args, Scope scope)
        {
            WorkSpace.CurrentWorkSpace = args.Single().AsString(scope);
            return Expression.Void();
        }

        public static Expression Scan(Expression[] args, Scope scope)
        {
            (args.Length == 0 || args.Length == 1).OrThrows<ParameterException>();
            if (args.Length == 1)
                Console.Write(args.Single().AsString(scope));
            return Console.ReadLine();
        }
        public static Expression Throw(Expression[] args, Scope scope)
        {
            throw new Exception(args.Single().AsString(scope));
        }
        public static Expression Delete(Expression[] args, Scope scope)
        {
            foreach (var item in args.Cast<ParameterExpression>().Select(i => i.Name))
            {
                if (!scope.Delete(item))
                {
                    throw new NotDefinedException(item);
                }
            }
            return Expression.Void();
        }
        public static Expression DisplayScope(Expression[] args, Scope scope)
        {
            Console.WriteLine(scope.GlobalScope);
            return Expression.Void();
        }
        public static Expression Exit(Expression[] args, Scope scope)
        {
            Environment.Exit(0);
            return Expression.Void();
        }
    }
}
