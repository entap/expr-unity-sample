using System;
using System.Collections.Generic;

namespace Entap.Expr
{
	using TokenList = List<Token>;

	/// <summary>
	/// 数式の構文解析
	/// </summary>
	internal class Parser
	{
		TokenList _tokens;
		int _index;

		/// <summary>
		/// <see cref="T:Entap.Expr.ExprParser"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="tokens">トークンの配列</param>
		public Parser(TokenList tokens)
		{
			_tokens = tokens;
		}

		/// <summary>
		/// 現在の位置のトークンを読み込む。
		/// </summary>
		/// <returns>現在の位置のトークン</returns>
		Token PeekToken()
		{
			return _index < _tokens.Count ? _tokens[_index] : Token.Terminal;
		}

		/// <summary>
		/// トークンの読み込み位置を進める。
		/// </summary>
		void NextToken()
		{
			if (_index < _tokens.Count) {
				_index++;
			}
		}

		/// <summary>
		/// 構文解析を行う。
		/// </summary>
		/// <returns>構文木の根</returns>
		public static IExpression Parse(TokenList tokens)
		{
			return (new Parser(tokens)).Parse();
		}

		/// <summary>
		/// 構文解析を行う。
		/// </summary>
		/// <returns>構文木の根</returns>
		public IExpression Parse()
		{
			var node = ParseLogicalOr();
			var token = PeekToken();
			if (!token.IsTerminal()) {
				throw new ExpressionSyntaxException("Unexpected token: " + token.Text, token.Offset);
			}
			return node;
		}

		/// <summary>
		/// 構文解析を行うデリゲート
		/// </summary>
		delegate IExpression ParseDelegate();

		/// <summary>
		/// ２項演算子に関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		/// <param name="childParser">優先順位の高い構文解析関数</param>
		/// <param name="operators">演算子</param>
		IExpression ParseBinary(ParseDelegate childParser, params string[] operators)
		{
			var node = childParser();
			Token operatorToken;
			while ((operatorToken = PeekToken()).IsOperator(operators)) {
				NextToken();
				node = new BinaryExpression(operatorToken.Text, node, childParser());
			}
			return node;
		}

		/// <summary>
		/// 論理ORに関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseLogicalOr()
		{
			return ParseBinary(ParseLogicalAnd, "||");
		}

		/// <summary>
		/// 論理ANDに関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseLogicalAnd()
		{
			return ParseBinary(ParseBitwiseOr, "&&");
		}

		/// <summary>
		/// ビットごとのORに関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseBitwiseOr()
		{
			return ParseBinary(ParseBitwiseXor, "|");
		}

		/// <summary>
		/// ビットごとのXORに関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseBitwiseXor()
		{
			return ParseBinary(ParseBitwiseAnd, "^");
		}

		/// <summary>
		/// ビットごとのANdに関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseBitwiseAnd()
		{
			return ParseBinary(ParseEquality, "&");
		}

		/// <summary>
		/// 等価比較に関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseEquality()
		{
			return ParseBinary(ParseRelational, "==", "!=");
		}

		/// <summary>
		/// 大小比較に関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseRelational()
		{
			return ParseBinary(ParseShift, "<", ">", "<=", ">=");
		}

		/// <summary>
		/// ビットシフトに関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseShift()
		{
			return ParseBinary(ParseTerm, "<<", ">>");
		}

		/// <summary>
		/// 加算・減算に関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseTerm()
		{
			return ParseBinary(ParseFactor, "+", "-");
		}

		/// <summary>
		/// 乗算・除算・剰余に関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseFactor()
		{
			return ParseBinary(ParseExponential, "*", "/", "%");
		}

		/// <summary>
		/// 累乗に関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseExponential()
		{
			return ParseBinary(ParseUnary, "**");
		}

		/// <summary>
		/// 単項演算子に関する構文解析
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseUnary()
		{
			var operatorToken = PeekToken();
			if (operatorToken.IsOperator("!", "~", "+", "-")) {
				NextToken();
				return new UnaryExpression(operatorToken.Text, ParseGroup());
			}
			return ParseGroup();
		}

		/// <summary>
		/// 構文解析：括弧
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParseGroup()
		{
			if (PeekToken().IsOperator("(")) {
				NextToken();
				var node = ParseLogicalOr();
				var token = PeekToken();
				if (token.IsOperator(")")) {
					NextToken();
					return node;
				}
				throw new ExpressionSyntaxException("Unclosed bracket", token.Offset);
			}
			return ParsePrimary();
		}

		/// <summary>
		/// 構文解析：定数値もしくは変数値
		/// </summary>
		/// <returns>構文木</returns>
		IExpression ParsePrimary()
		{
			var token = PeekToken();
			NextToken();
			if (token.IsConstant()) {
				return new ConstantExpression(new Value(token.Value));
			}
			if (token.IsIdentifier()) {
				if (PeekToken().IsOperator("(")) {
					NextToken();
					return new FuncExpression(token.Text, ParseArguments());
				}
				return new VariableExpression((string)token.Value);
			}
			throw new ExpressionSyntaxException("Invalid token: " + token.Text, token.Offset);
		}

		/// <summary>
		/// 構文解析：引数リスト
		/// </summary>
		/// <returns>引数リスト</returns>
		List<IExpression> ParseArguments()
		{
			var args = new List<IExpression>();
			if (!PeekToken().IsOperator(")")) {
				while (true) {
					args.Add(ParseLogicalOr());
					var token = PeekToken();
					if (PeekToken().IsOperator(",")) {
						NextToken();
					} else if (token.IsOperator(")")) {
						break;
					} else if (token.IsTerminal()) {
						throw new ExpressionSyntaxException("Unclosed bracket", PeekToken().Offset);
					} else {
						throw new ExpressionSyntaxException("Invalid token: " + PeekToken().Text, PeekToken().Offset);
					}
				}
			}
			NextToken();
			return args;
		}
	}
}
