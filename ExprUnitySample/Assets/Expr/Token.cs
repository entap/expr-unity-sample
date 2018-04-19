using System;

namespace Entap.Expr
{
	/// <summary>
	/// 数式のトークンの種類
	/// </summary>
	internal enum TokenType
	{
		Number,
		String,
		Identifier,
		Punctuator,
		Terminal
	}

	/// <summary>
	/// 数式のトークン
	/// </summary>
	internal class Token
	{
		/// <summary>
		/// 終端のトークン
		/// </summary>
		public static Token Terminal = new Token { Type = TokenType.Terminal, Text = "(end)" };

		/// <summary>
		/// トークンの種類
		/// </summary>
		public TokenType Type;

		/// <summary>
		/// 数式内の文字列
		/// </summary>
		public string Text;

		/// <summary>
		/// 数式内の文字位置
		/// </summary>
		public int Offset;

		/// <summary>
		/// 値
		/// </summary>
		public object Value;

		/// <summary>
		/// 識別子か判定する
		/// </summary>
		/// <returns>定数値なら<c>true</c>、そうでないなら<c>false</c></returns>
		public bool IsIdentifier()
		{
			return Type == TokenType.Identifier;
		}

		/// <summary>
		/// 定数値か判定する
		/// </summary>
		/// <returns>定数値なら<c>true</c>、そうでないなら<c>false</c></returns>
		public bool IsConstant()
		{
			return Type == TokenType.Number || Type == TokenType.String;
		}

		/// <summary>
		/// 演算子か判定する
		/// </summary>
		/// <returns>指定された演算子なら<c>true</c>、そうでないなら<c>false</c></returns>
		/// <param name="operators">演算子</param>
		public bool IsOperator(params string[] operators)
		{
			if (!(Type == TokenType.Punctuator || Type == TokenType.Identifier)) {
				return false;
			}
			foreach (var op in operators) {
				if (op == (string)Value) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 終端か判定する
		/// </summary>
		/// <returns>終端なら<c>true</c>、そうでないなら<c>false</c></returns>
		public bool IsTerminal()
		{
			return Type == TokenType.Terminal;
		}
	}
}
