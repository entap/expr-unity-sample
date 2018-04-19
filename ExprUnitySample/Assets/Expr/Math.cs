using System;
using System.Collections.Generic;

namespace Entap.Expr
{
	internal static class Math
	{
		/// <summary>
		/// 定数の定義
		/// </summary>
		static readonly Dictionary<string, object> Constants = new Dictionary<string, object> {
			// 定数値
			{ "true", true },
			{ "false", false },
			{ "Epsilon", double.Epsilon },
			{ "Infinity", double.PositiveInfinity },
			{ "NaN", double.NaN },
			// 数学定数
			{ "E", System.Math.E },
			{ "LN10", System.Math.Log(10) },
			{ "LN2", System.Math.Log(2) },
			{ "LOG10E", System.Math.Log(System.Math.E, 10) },
			{ "LOG2E", System.Math.Log(System.Math.E, 2) },
			{ "PI", System.Math.PI },
			{ "SQRT1_2", System.Math.Sqrt(0.5) },
			{ "SQRT2", System.Math.Sqrt(2) },
			// Unity
			{ "Rad2Deg", 180.0/System.Math.PI },
			{ "Deg2Rad", System.Math.PI/180.0 }
		};

		/// <summary>
		/// 関数の定義
		/// </summary>
		static readonly Dictionary<string, Delegate> Functions = new Dictionary<string, Delegate> {
			// 数学関数(よく使われるものだけ)
			{ "abs", new Func<double, double>(System.Math.Abs) },
			{ "acos", new Func<double, double>(System.Math.Acos) },
			{ "asin", new Func<double, double>(System.Math.Asin) },
			{ "atan", new Func<double, double>(System.Math.Atan) },
			{ "atan2", new Func<double, double, double>(System.Math.Atan2) },
			{ "ceil", new Func<double, double>(System.Math.Ceiling) },
			{ "cbrt", new Func<double, double>(x => System.Math.Pow(x, 1.0/3.0)) },
			{ "cos", new Func<double, double>(System.Math.Cos) },
			{ "cosh", new Func<double, double>(System.Math.Cosh) },
			{ "exp", new Func<double, double>(System.Math.Exp) },
			{ "floor", new Func<double, double>(System.Math.Floor) },
			{ "hypot", new Func<double, double, double>(Hypot) },
			{ "log", new Func<double, double, double>(System.Math.Log) },
			{ "log10", new Func<double, double>(System.Math.Log10) },
			{ "log2", new Func<double, double>(x => System.Math.Log(x, 2)) },
			{ "max", new Func<double, double, double>(System.Math.Max) },
			{ "min", new Func<double, double, double>(System.Math.Min) },
			{ "pow", new Func<double, double, double>(System.Math.Pow) },
			{ "round", new Func<double, double>(System.Math.Round) },
			{ "sign", new Func<double, double>(x => System.Math.Sign(x)) },
			{ "sin", new Func<double, double>(System.Math.Sin) },
			{ "sinh", new Func<double, double>(System.Math.Sinh) },
			{ "sqrt", new Func<double, double>(System.Math.Sqrt) },
			{ "tan", new Func<double, double>(System.Math.Tan) },
			{ "tanh", new Func<double, double>(System.Math.Tanh) },
			// Processingで使われるもの
			{ "sq", new Func<double, double>(x => x * x) },
			{ "constrain", new Func<double, double, double, double>(Constrain) },
			{ "mag", new Func<double, double, double>((x, y) => System.Math.Sqrt(x * x + y * y)) },
			{ "dist", new Func<double, double, double, double, double>((x1, y1, x2, y2) => Hypot(x1-x2, y1-y2)) },
			{ "lerp", new Func<double, double, double, double>(Lerp) },
			{ "norm", new Func<double, double, double, double>(Norm) },
//			{ "map", new Func<double, double, double, double, double, double>(Map) }
		};

		/// <summary>
		/// 斜辺の長さを求める。
		/// </summary>
		/// <returns>斜辺の長さ</returns>
		/// <param name="x">x座標</param>
		/// <param name="y">y座標</param>
		static double Hypot(double x, double y)
		{
			return System.Math.Sqrt(x * x + y * y);
		}

		/// <summary>
		/// 値を範囲内に収める。
		/// </summary>
		/// <returns>結果</returns>
		/// <param name="x">値</param>
		/// <param name="a">最小値</param>
		/// <param name="b">最大値</param>
		static double Constrain(double x, double a, double b)
		{
			return x < a ? a : (x > b ? b : x);
		}

		/// <summary>
		/// 値を範囲内に収める。
		/// </summary>
		/// <returns>結果</returns>
		/// <param name="a">範囲の下限値</param>
		/// <param name="b">範囲の上限値</param>
		/// <param name="x">値</param>
		static double Lerp(double a, double b, double x)
		{
			return a * (1 - x) + b * x;
		}

		/// <summary>
		/// ある範囲の値を正規化する。
		/// </summary>
		/// <returns>結果</returns>
		/// <param name="x">値</param>
		/// <param name="a">範囲の下限値</param>
		/// <param name="b">範囲の上限値</param>
		static double Norm(double x, double a, double b)
		{
			return Map(x, a, b, 0, 1);
		}

		/// <summary>
		/// ある範囲の値を別の範囲に写像する。
		/// </summary>
		/// <returns>結果</returns>
		/// <param name="x">値</param>
		/// <param name="a">写像元範囲の下限値</param>
		/// <param name="b">写像元範囲の上限値</param>
		/// <param name="c">写像先範囲の下限値</param>
		/// <param name="d">写像先範囲の上限値</param>
		static double Map(double x, double a, double b, double c, double d)
		{
			return (a == b) ? c : (x - a) * (d - c) / (b - a) + c;
		}

		/// <summary>
		/// 数学ライブラリを呼び出すバインディングを生成する。
		/// </summary>
		/// <returns>結果</returns>
		/// <param name="defaultBinding">デフォルトのバインディング</param>
		public static BindingDelegate Binding(BindingDelegate defaultBinding)
		{
			return (name) => {
				if (Constants.ContainsKey(name)) {
					return Constants[name];
				}
				if (Functions.ContainsKey(name)) {
					return Functions[name];
				}
				if (defaultBinding != null) {
					return defaultBinding(name);
				}
				return 0.0;
			};
		}
	}
}
