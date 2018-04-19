using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Entap.Expr
{
	using TokenList = List<Token>;

	/// <summary>
	/// 数式の字句解析
	/// </summary>
	internal class Lexer
	{
		string _expression;
		int _offset;

		/// <summary>
		/// <see cref="T:Entap.Expr.Lexer"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="expression">数式の文字列</param>
		public Lexer(string expression)
		{
			_expression = expression;
			_offset = 0;
		}

		/// <summary>
		/// 現在の読み込み位置から文字を読み込む。
		/// </summary>
		/// <returns>読み込んだ文字</returns>
		int PeekChar()
		{
			return _offset < _expression.Length ? _expression[_offset] : -1;
		}

		/// <summary>
		/// 指定された文字数だけ、文字を先読みする。
		/// </summary>
		/// <returns>読み込んだ文字列</returns>
		/// <param name="n">文字数</param>
		string PeekString(int n)
		{
			return _expression.Substring(_offset, System.Math.Min(_expression.Length - _offset, n));
		}

		/// <summary>
		/// 文字の読み込み位置を進める。
		/// </summary>
		void NextChar(int n = 1)
		{
			if (_offset + n <= _expression.Length) {
				_offset += n;
			}
		}

		/// <summary>
		/// 文字の読み込み位置を戻す。
		/// </summary>
		void BackChar()
		{
			if (_offset > 0) {
				_offset--;
			}
		}

		/// <summary>
		/// 指定された文字数だけ、条件が成立する間、文字を読み込む。
		/// </summary>
		/// <returns>読み込んだ文字列</returns>
		/// <param name="predicate">条件</param>
		/// <param name="n">文字数</param>
		string ReadWhile(Predicate<char> predicate, int n = int.MaxValue)
		{
			var start = _offset;
			var i = 0;
			while (i < n && (start + i) < _expression.Length && predicate(_expression[start + i])) {
				i++;
			}
			_offset += i;
			return _expression.Substring(start, i);
		}

		/// <summary>
		/// 次のトークンを読み込む。
		/// </summary>
		/// <returns>トークン</returns>
		public Token Read()
		{
			SkipWhiteSpace();
			var c = PeekChar();
			if (c == -1) {
				// 文字列終わり
				return null;
			}
			var token = new Token { Offset = _offset };
			if (IsDecDigit((char)c)) {
				// 数値リテラル
				token.Type = TokenType.Number;
				token.Value = ReadNumeric();
			} else if (c == '\"' || c == '\'') {
				// 文字列リテラル
				token.Type = TokenType.String;
				token.Value = ReadString();
			} else if (IsIdentifierStart((char)c)) {
				// 識別子
				token.Type = TokenType.Identifier;
				token.Value = ReadIdentifier();
			} else {
				// 区切り子
				token.Type = TokenType.Punctuator;
				token.Value = ReadPunctuator();
				if (token.Value == null) {
					throw new ExpressionSyntaxException("Unknown char: " + (char)c, _offset);
				}
			}
			token.Text = _expression.Substring(token.Offset, _offset - token.Offset);
			return token;
		}

		/// <summary>
		/// 数式を字句解析する。
		/// </summary>
		/// <returns>トークンの配列</returns>
		/// <param name="expression">数式の文字列</param>
		public static TokenList ReadAll(string expression)
		{
			return (new Lexer(expression)).ReadAll();
		}

		/// <summary>
		/// トークンを全て読み込む。
		/// </summary>
		/// <returns>トークンの配列</returns>
		public TokenList ReadAll()
		{
			var tokens = new TokenList();
			for (var token = Read(); token != null; token = Read()) {
				tokens.Add(token);
			}
			return tokens;
		}

		/// <summary>
		/// 空白文字か判定する。
		/// </summary>
		/// <returns>空白文字なら<c>true</c>、そうでないなら<c>false</c></returns>
		/// <param name="c">文字</param>
		static bool IsWhiteSpace(char c)
		{
			return
				c == '\x9' || c == '\xb' || c == '\xc' || c == '\x20' || c == '\xa0' || c == '\ufeff' ||
				char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator ||
				c == '\xa' || c == '\xd' || c == '\u2028' || c == '\u2029';
		}

		/// <summary>
		/// 空白を読み飛ばす。
		/// </summary>
		void SkipWhiteSpace()
		{
			ReadWhile(IsWhiteSpace);
		}

		/// <summary>
		/// 数字か判定する。
		/// </summary>
		/// <returns>数字なら<c>true</c>、そうでないなら<c>false</c></returns>
		/// <param name="c">文字</param>
		static bool IsDecDigit(char c)
		{
			return ('0' <= c && c <= '9');
		}

		/// <summary>
		/// 16進数の文字か判定する。
		/// </summary>
		/// <returns>16進数の文字なら<c>true</c>、そうでないなら<c>false</c></returns>
		/// <param name="c">文字</param>
		static bool IsHexDigit(char c)
		{
			return ('0' <= c && c <= '9') || ('a' <= c && c <= 'f') || ('A' <= c && c <= 'F');
		}

		/// <summary>
		/// 数値リテラルを読み込む。
		/// </summary>
		/// <returns>読み込んだ数値。long型かdouble型</returns>
		object ReadNumeric()
		{
			var prefix = PeekString(2);
			if (prefix == "0x" || prefix == "0X") {
				// 16進数の整数値
				NextChar(2);
				var hex = ReadWhile(IsHexDigit);
				if (hex.Length == 0) {
					throw new ExpressionSyntaxException("Invalid number format", _offset);
				}
				return Convert.ToDouble(Convert.ToInt64(hex, 16));
			}

			var s = new StringBuilder();

			// 整数部
			s.Append(ReadWhile(IsDecDigit));

			// 小数部
			var c1 = PeekChar();
			if (c1 == '.') {
				s.Append((char)c1);
				NextChar();
				var dec = ReadWhile(IsDecDigit);
				if (dec.Length == 0) {
					throw new ExpressionSyntaxException("Invalid number format", _offset);
				}
				s.Append(dec);
				c1 = PeekChar();
			}

			// 指数部
			if (c1 == 'e' || c1 == 'E') {
				s.Append((char)c1);
				NextChar();
				c1 = PeekChar();
				if (c1 == '+' || c1 == '-') {
					s.Append((char)c1);
					NextChar();
					c1 = PeekChar();
				}
				var exp = ReadWhile(IsDecDigit);
				if (exp.Length == 0) {
					throw new ExpressionSyntaxException("Invalid number format", _offset);
				}
				s.Append(exp);
			}

			return Convert.ToDouble(s.ToString());
		}

		/// <summary>
		/// 文字列リテラルを読み込む。
		/// </summary>
		/// <returns>読み込んだ文字列</returns>
		string ReadString()
		{
			var s = new StringBuilder();
			var quote = PeekChar();
			NextChar();
			while (true) {
				var c = PeekChar();
				if (c == -1) {
					throw new ExpressionSyntaxException("Unclosed quote", _offset);
				}
				NextChar();
				if (c == quote) {
					break;
				}
				if (c == '\\') {
					s.Append(ReadEscape());
				} else {
					s.Append((char)c);
				}
			}
			return s.ToString();
		}

		/// <summary>
		/// エスケープシーケンスを読み込む。
		/// </summary>
		/// <returns>読み込んだ文字</returns>
		char ReadEscape()
		{
			var c = PeekChar();
			NextChar();
			switch (c) {
				case 'b':
					return '\b';
				case 't':
					return '\t';
				case 'n':
					return '\n';
				case 'v':
					return '\v';
				case 'f':
					return '\f';
				case 'r':
					return '\r';
				case '\'':
					return '\'';
				case '\"':
					return '\"';
				case '\\':
					return '\\';
				case '\0':
					return '\0';
				case 'x':
					var x = ReadWhile(IsHexDigit, 2);
					if (x.Length == 0) {
						return 'x';
					}
					return Convert.ToChar(Convert.ToInt32(x, 16));
				case 'u':
					var u = ReadWhile(IsHexDigit, 4);
					if (u.Length == 0) {
						return 'u';
					}
					return Convert.ToChar(Convert.ToInt32(u, 16));
				default:
					return (char)c;
			}
		}

		/// <summary>
		/// 識別子の先頭の文字か判定する。
		/// </summary>
		/// <returns>識別子の先頭の文字なら<c>true</c>、そうでないなら<c>false</c></returns>
		/// <param name="c">文字</param>
		static bool IsIdentifierStart(char c)
		{
			var uc = char.GetUnicodeCategory(c);
			return
				c == '$' || c == '_' ||
				uc == UnicodeCategory.UppercaseLetter || uc == UnicodeCategory.LowercaseLetter ||
				uc == UnicodeCategory.TitlecaseLetter || uc == UnicodeCategory.ModifierLetter ||
				uc == UnicodeCategory.OtherLetter || uc == UnicodeCategory.LetterNumber;
		}

		/// <summary>
		/// 識別子の文字か判定する。
		/// </summary>
		/// <returns>識別子の文字なら<c>true</c>、そうでないなら<c>false</c></returns>
		/// <param name="c">文字</param>
		static bool IsIdentifierPart(char c)
		{
			var uc = char.GetUnicodeCategory(c);
			return
				c == '\u200c' || c == '\u200d' || IsIdentifierStart(c) ||
				uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.SpacingCombiningMark ||
				uc == UnicodeCategory.DecimalDigitNumber || uc == UnicodeCategory.ConnectorPunctuation;
		}

		/// <summary>
		/// 識別子を読み込む。
		/// </summary>
		/// <returns>読み込んだ識別子</returns>
		string ReadIdentifier()
		{
			return ReadWhile(IsIdentifierPart);
		}

		/// <summary>
		/// 演算子を読み込む。
		/// </summary>
		/// <returns>読み込んだ演算子</returns>
		string ReadPunctuator()
		{
			var s = PeekString(2);
			foreach (var p in Puctuators) {
				if (s.StartsWith(p, StringComparison.CurrentCulture)) {
					NextChar(p.Length);
					return p;
				}
			}
			return null;
		}

		/// <summary>
		/// 区切り子(長い順)
		/// </summary>
		static readonly string[] Puctuators = {
			// 2文字
			"<=", ">=", "==", "!=", "&&", "||", ">>", "<<", "**",
			// 1文字
			"+", "-", "*", "/", "%", "(", ")", ".", ",", "<", ">", "&", "|", "^", "!", "~"
		};
	}
}
