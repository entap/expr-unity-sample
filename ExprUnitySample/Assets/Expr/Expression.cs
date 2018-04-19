using System;
using System.Collections.Generic;

namespace Entap.Expr
{
	/// <summary>
	/// 変数のバインディング
	/// </summary>
	public delegate object BindingDelegate(string name);

	/// <summary>
	/// 数式
	/// </summary>
	public class Expression
	{
		IExpression _expression;

		/// <summary>
		/// <see cref="T:Entap.Expr.Expression"/> クラスのインスタンスを初期化する。
		/// </summary>
		public Expression()
		{
			_expression = null;
		}

		/// <summary>
		/// <see cref="T:Entap.Expr.Expression"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="expression">数式.</param>
		public Expression(string expression)
		{
			Compile(expression);
		}

		/// <summary>
		/// 数式を設定する。
		/// </summary>
		/// <param name="expression">数式</param>
		public void Compile(string expression)
		{
			var lexer = Lexer.ReadAll(expression);
			_expression = Parser.Parse(lexer);
		}

		/// <summary>
		/// 数式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="expression">数式</param>
		/// <param name="vars">変数の辞書</param>
		static public T Evaluate<T>(string expression, Dictionary<string, object> vars = null)
		{
			return (new Expression(expression)).Evaluate<T>(vars);
		}

		/// <summary>
		/// 数式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="expression">数式</param>
		/// <param name="binding">変数のバインディング</param>
		static public T Evaluate<T>(string expression, BindingDelegate binding)
		{
			return (new Expression(expression)).Evaluate<T>(binding);
		}

		/// <summary>
		/// 数式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="vars">変数の辞書</param>
		public T Evaluate<T>(Dictionary<string, object> vars = null)
		{
			return Evaluate<T>(name => vars != null ? vars[name] : Value.Null);
		}

		/// <summary>
		/// 数式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="binding">変数のバインディング</param>
		public T Evaluate<T>(BindingDelegate binding)
		{
			binding = Math.Binding(binding);
			return (T)_expression.Evaluate(binding).As(typeof(T));
		}

		/// <summary>
		/// 数式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="vars">変数の辞書</param>
		Value Evaluate(Dictionary<string, object> vars = null)
		{
			return Evaluate(name => vars != null ? vars[name] : Value.Null);
		}

		/// <summary>
		/// 数式を評価する。
		/// </summary>
		/// <returns>評価結果</returns>
		/// <param name="binding">変数のバインディング</param>
		Value Evaluate(BindingDelegate binding)
		{
			binding = Math.Binding(binding);
			return _expression.Evaluate(binding);
		}
	}
}
