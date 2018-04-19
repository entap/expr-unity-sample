namespace Entap.Expr
{
	/// <summary>
	/// 定数値を持つ式
	/// </summary>
	internal class ConstantExpression : IExpression
	{
		readonly Value _value;

		/// <summary>
		/// <see cref="T:Entap.Expr.ConstantExpr"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="value">定数値</param>
		public ConstantExpression(Value value)
		{
			_value = value;
		}

		/// <summary>
		/// この式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="binding">変数のバインディング</param>
		public Value Evaluate(BindingDelegate binding)
		{
			return _value;
		}
	}
}
