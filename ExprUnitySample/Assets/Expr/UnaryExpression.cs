namespace Entap.Expr
{
	/// <summary>
	/// 単項演算子を持つ式
	/// </summary>
	internal class UnaryExpression : IExpression
	{
		readonly string _op;
		readonly IExpression _expression;

		/// <summary>
		/// <see cref="T:Entap.Expr.UnaryExpr"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="op">演算子</param>
		/// <param name="expression">式</param>
		public UnaryExpression(string op, IExpression expression)
		{
			_op = op;
			_expression = expression;
		}

		/// <summary>
		/// この式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="binding">変数のバインディング</param>
		public Value Evaluate(BindingDelegate binding)
		{
			return Value.EvaluateUnaryOperator(_expression.Evaluate(binding), _op);
		}
	}
}
