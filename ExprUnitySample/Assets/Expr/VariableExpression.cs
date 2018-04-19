using System;
namespace Entap.Expr
{
	/// <summary>
	/// 変数を持つ式
	/// </summary>
	internal class VariableExpression : IExpression
	{
		readonly string _name;

		/// <summary>
		/// <see cref="T:Entap.Expr.VariableExpression"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="name">変数名</param>
		public VariableExpression(string name)
		{
			_name = name;
		}

		/// <summary>
		/// このノードを評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="binding">変数のバインディング</param>
		public Value Evaluate(BindingDelegate binding)
		{
			return new Value(binding(_name));
		}
	}
}
