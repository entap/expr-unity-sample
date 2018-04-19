using System;
using System.Collections.Generic;

namespace Entap.Expr
{
	internal class Value
	{
		object _value;

		/// <summary>
		/// NULLを表す定数値
		/// </summary>
		public static Value Null = new Value(null);

		/// <summary>
		/// Trueを表す定数値
		/// </summary>
		public static Value True = new Value(true);

		/// <summary>
		/// Falseを表す定数値
		/// </summary>
		public static Value False = new Value(false);

		/// <summary>
		/// <see cref="T:Entap.Expr.Value"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="value">Value.</param>
		public Value(object value = null)
		{
			Set(value);
		}

		/// <summary>
		/// 値を設定する。
		/// </summary>
		/// <param name="value">設定する値</param>
		public void Set(object value)
		{
			if (value is Value) {
				_value = ((Value)value)._value;
			} else if (value == null || value is bool || value is double || value is string) {
				_value = value; // 対応している型
			} else if (value is sbyte || value is byte || value is short || value is ushort || value is int ||
					   value is uint || value is long || value is ulong || value is float || value is decimal) {
				_value = Convert.ToDouble(value); // 整数・実数型なら、double型に変換
			} else {
				_value = value.ToString(); // 不明な型はToStringする。
			}
		}

		/// <summary>
		/// 値が真偽値か判定する。
		/// </summary>
		/// <returns>真偽値なら<c>true</c>、そうでないなら<c>false</c></returns>
		public bool IsBool()
		{
			return _value is bool;
		}

		/// <summary>
		/// 値が実数か判定する。
		/// </summary>
		/// <returns>値が実数なら<c>true</c>、そうでないなら<c>false</c></returns>
		public bool IsNumber()
		{
			return _value is double;
		}

		/// <summary>
		/// 値が文字列か判定する。
		/// </summary>
		/// <returns>値が文字列なら<c>true</c>、そうでないなら<c>false</c></returns>
		public bool IsString()
		{
			return _value is string;
		}

		/// <summary>
		/// 値が関数か判定する。
		/// </summary>
		/// <returns>値が関数なら<c>true</c>、そうでないなら<c>false</c></returns>
		public bool IsFunc()
		{
			return _value is Delegate;
		}

		/// <summary>
		/// 値を真偽値として取得する。
		/// </summary>
		/// <returns>真偽値</returns>
		public bool AsBool()
		{
			if (_value == null) {
				return false;
			}
			if (_value is bool) {
				return (bool)_value;
			}
			if (_value is double) {
				return !(double.IsNaN((double)_value) || IsZero());
			}
			if (_value is string) {
				return ((string)_value).Length >= 1;
			}
			throw new InvalidCastException();
		}

		/// <summary>
		/// 実数として取得する。
		/// </summary>
		/// <returns>結果</returns>
		public double AsNumber()
		{
			if (_value == null) {
				return 0;
			}
			if (_value is bool) {
				return (bool)_value ? 1 : 0;
			}
			if (_value is double) {
				return (double)_value;
			}
			if (_value is string) {
				double d;
				return double.TryParse((string)_value, out d) ? d : double.NaN;
			}
			throw new InvalidCastException();
		}

		/// <summary>
		/// 整数として取得する。
		/// </summary>
		/// <returns>結果</returns>
		public int AsInt()
		{
			var n = AsNumber();
			if (double.IsNaN(n) || double.IsInfinity(n) || IsZero()) {
				return 0;
			}
			return Convert.ToInt32(n);
		}

		/// <summary>
		/// 文字列として取得する。
		/// </summary>
		/// <returns>結果</returns>
		public string AsString()
		{
			return _value == null ? "null" : _value.ToString();
		}

		/// <summary>
		/// 指定された型に変換する。
		/// </summary>
		/// <returns>変換結果</returns>
		/// <param name="type">型</param>
		public object As(Type type)
		{
			if (type == typeof(int)) {
				return AsInt();
			}
			if (type == typeof(double)) {
				return AsNumber();
			}
			if (type == typeof(bool)) {
				return AsBool();
			}
			if (type == typeof(string)) {
				return AsString();
			}
			if (type == typeof(object)) {
				return _value;
			}
			if (type == typeof(Value)) {
				return this;
			}
			throw new InvalidCastException();
		}

		/// <summary>
		/// 値が0か判定する
		/// </summary>
		/// <returns>値が0なら<c>true</c>、そうでないなら<c>false</c></returns>
		public bool IsZero()
		{
			return _value is double && System.Math.Abs((double)_value) < double.Epsilon;
		}

		/// <summary>
		/// 値がNULLか判定する
		/// </summary>
		/// <returns>値がNullなら<c>true</c>、そうでないなら<c>false</c></returns>
		public bool IsNull()
		{
			return _value == null;
		}

		/// <summary>
		/// ２つの値を比較する。
		/// </summary>
		/// <returns>xがyより小さいなら<c>-1</c>、xがyより大きいなら<c>1</c>、等価なら<c>0</c></returns>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		static public int Compare(Value x, Value y)
		{
			if (x.IsString() && y.IsString()) {
				return System.Math.Sign(string.Compare((string)x._value, (string)y._value));
			}
			return System.Math.Sign(x.AsNumber() - y.AsNumber());
		}

		/// <summary>
		/// 二項演算子の関数
		/// </summary>
		delegate Value BinaryOperator(Value x, Value y);

		/// <summary>
		/// 二項演算子の辞書
		/// </summary>
		static Dictionary<string, BinaryOperator> BinaryOperators = new Dictionary<string, BinaryOperator> {
			{ "||", LogicalOr },
			{ "&&", LogicalAnd },
			{ "|", BitwiseOr },
			{ "^", BitwiseXor },
			{ "&", BitwiseAnd },
			{ "==", Equal },
			{ "!=", NotEqual },
			{ "<", LessThan },
			{ ">", GreaterThan },
			{ "<=", LessThanOrEqual },
			{ ">=", GreaterThanOrEqual },
			{ "<<", LeftShift },
			{ ">>", RightShift },
			{ "**", Power },
			{ "+", Add },
			{ "-", Subtract },
			{ "*", Multiply },
			{ "/", Divide },
			{ "%", Modulo }
		};

		/// <summary>
		/// 二項演算子の演算を行う。
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">演算子の左の値</param>
		/// <param name="y">演算子の右の値</param>
		/// <param name="op">演算子の文字列</param>
		public static Value EvaluateBinaryOperator(Value x, Value y, string op)
		{
			return BinaryOperators[op](x, y);
		}

		/// <summary>
		/// 加算
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value Add(Value x, Value y)
		{
			if (x.IsString() || y.IsString()) {
				return new Value(x.AsString() + y.AsString());
			}
			return new Value(x.AsNumber() + y.AsNumber());
		}

		/// <summary>
		/// 減算
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value Subtract(Value x, Value y)
		{
			return new Value(x.AsNumber() - y.AsNumber());
		}

		/// <summary>
		/// 乗算
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value Multiply(Value x, Value y)
		{
			return new Value(x.AsNumber() * y.AsNumber());
		}

		/// <summary>
		/// 除算
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value Divide(Value x, Value y)
		{
			return new Value(x.AsNumber() / y.AsNumber());
		}

		/// <summary>
		/// 剰余
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value Modulo(Value x, Value y)
		{
			return new Value(x.AsNumber() % y.AsNumber());
		}

		/// <summary>
		/// ビット演算のOR
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value BitwiseOr(Value x, Value y)
		{
			return new Value(x.AsInt() | y.AsInt());
		}

		/// <summary>
		/// ビット演算のXOR
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value BitwiseXor(Value x, Value y)
		{
			return new Value(x.AsInt() ^ y.AsInt());
		}

		/// <summary>
		/// ビット演算のAND
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value BitwiseAnd(Value x, Value y)
		{
			return new Value(x.AsInt() & y.AsInt());
		}

		/// <summary>
		/// 左シフト
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value LeftShift(Value x, Value y)
		{
			return new Value(x.AsInt() << y.AsInt());
		}

		/// <summary>
		/// 右シフト
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value RightShift(Value x, Value y)
		{
			return new Value(x.AsInt() >> y.AsInt());
		}

		/// <summary>
		/// 論理AND
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value LogicalAnd(Value x, Value y)
		{
			return new Value(x.AsBool() ? y._value : x._value);
		}

		/// <summary>
		/// 論理OR
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value LogicalOr(Value x, Value y)
		{
			return new Value(x.AsBool() ? x._value : y._value);
		}

		/// <summary>
		/// 等価
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value Equal(Value x, Value y)
		{
			// 型が同じ場合
			if (x.IsNull() || y.IsNull()) {
				return new Value(true);
			}
			if (x.IsBool() && y.IsBool()) {
				return new Value((bool)x._value == (bool)y._value);
			}
			if (x.IsNumber() && y.IsNumber()) {
				return new Value((double)x._value == (double)y._value);
			}
			if (x.IsString() && y.IsString()) {
				return new Value((string)x._value == (string)y._value);
			}

			// 文字列と数値の組み合わせの場合、数字に変換して比較する。
			if ((x.IsString() || y.IsString()) && (x.IsNumber() || y.IsNumber())) {
				return new Value(x.AsNumber() == y.AsNumber());
			}

			// 真偽値とその他の組み合わせの場合、数字に変換して比較する。
			if (x.IsBool() || y.IsBool()) {
				return new Value(x.AsNumber() == y.AsNumber());
			}

			// その他はfalse
			return new Value(false);
		}

		/// <summary>
		/// 等価でない
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value NotEqual(Value x, Value y)
		{
			return new Value(!Equal(x, y).AsBool());
		}

		/// <summary>
		/// 小さい
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value LessThan(Value x, Value y)
		{
			return new Value(Compare(x, y) < 0);
		}

		/// <summary>
		/// 大きい
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value GreaterThan(Value x, Value y)
		{
			return new Value(Compare(x, y) > 0);
		}

		/// <summary>
		/// 小さい
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value LessThanOrEqual(Value x, Value y)
		{
			return new Value(Compare(x, y) <= 0);
		}

		/// <summary>
		/// 大きい
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value GreaterThanOrEqual(Value x, Value y)
		{
			return new Value(Compare(x, y) >= 0);
		}

		/// <summary>
		/// 累乗
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">左の値</param>
		/// <param name="y">右の値</param>
		static Value Power(Value x, Value y)
		{
			return new Value(System.Math.Pow(x.AsNumber(), y.AsNumber()));
		}

		/// <summary>
		/// 単項演算子の関数
		/// </summary>
		delegate Value UnaryOperator(Value x);

		/// <summary>
		/// 単項演算子の辞書
		/// </summary>
		static Dictionary<string, UnaryOperator> UnaryOperators = new Dictionary<string, UnaryOperator> {
			{ "!", LogicalNot },
			{ "~", BitwiseNot },
			{ "+", UnaryPlus },
			{ "-", Negate }
		};

		/// <summary>
		/// 単項演算子の演算を行う。
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">対象の値</param>
		/// <param name="op">演算子の文字列</param>
		public static Value EvaluateUnaryOperator(Value x, string op)
		{
			return UnaryOperators[op](x);
		}

		/// <summary>
		/// 単項プラス
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">対象の値</param>
		static Value UnaryPlus(Value x)
		{
			return new Value(x.AsNumber());
		}

		/// <summary>
		/// 単項マイナス
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">対象の値</param>
		static Value Negate(Value x)
		{
			return new Value(-x.AsNumber());
		}

		/// <summary>
		/// ビット演算のNOT
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">対象の値</param>
		static Value BitwiseNot(Value x)
		{
			return new Value(~x.AsInt());
		}

		/// <summary>
		/// 論理NOT
		/// </summary>
		/// <returns>演算結果</returns>
		/// <param name="x">対象の値</param>
		static Value LogicalNot(Value x)
		{
			return new Value(!x.AsBool());
		}
	}
}
