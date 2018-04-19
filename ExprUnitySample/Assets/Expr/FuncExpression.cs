using System;
using System.Collections.Generic;
using System.Reflection;

namespace Entap.Expr
{
	internal class FuncExpression : IExpression
	{
		readonly string _func;
		readonly List<IExpression> _args;

		/// <summary>
		/// <see cref="T:Entap.Expr.FuncExpression"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="func">関数名</param>
		/// <param name="args">引数のノード</param>
		public FuncExpression(string func, List<IExpression> args)
		{
			_func = func;
			_args = args;
		}

		/// <summary>
		/// この式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="binding">変数のバインディング</param>
		public Value Evaluate(BindingDelegate binding)
		{
			var func = binding(_func) as Delegate;
			var p = func.Method.GetParameters();
			if (_args.Count < p.Length) {
				throw new ArgumentException("Wrong number of arguments: " + _func);
			}
			var args = new object[p.Length];
			for (var i = 0; i < _args.Count; i++) {
				args[i] = _args[i].Evaluate(binding).As(p[i].ParameterType);
			}
			return new Value(func.DynamicInvoke(args));
		}
	}
}
