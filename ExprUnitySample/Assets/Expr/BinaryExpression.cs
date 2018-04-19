namespace Entap.Expr
{
	/// <summary>
	/// 二項演算子を持つ式
	/// </summary>
	internal class BinaryExpression : IExpression
	{
		readonly string _op;
		readonly IExpression _left;
		readonly IExpression _right;

		/// <summary>
		/// <see cref="T:Entap.Expr.BinaryExpression"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="op">演算子</param>
		/// <param name="left">左のノード</param>
		/// <param name="right">右のノード</param>
		public BinaryExpression(string op, IExpression left, IExpression right)
		{
			_op = op;
			_left = left;
			_right = right;
		}

		/// <summary>
		/// この式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="binding">変数のバインディング</param>
		public Value Evaluate(BindingDelegate binding)
		{
			return Value.EvaluateBinaryOperator(_left.Evaluate(binding), _right.Evaluate(binding), _op);
		}
	}
}
