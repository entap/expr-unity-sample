using System;

namespace Entap.Expr
{
	[System.Serializable]
	public class ExpressionSyntaxException : Exception
	{
		readonly int _offset;

		/// <summary>
		/// 数式の中の位置
		/// </summary>
		public int Offset {
			get {
				return _offset;
			}
		}

		/// <summary>
		/// <see cref="T:ExpressionSyntaxException"/> クラスのインスタンスを初期化する。
		/// </summary>
		public ExpressionSyntaxException()
		{
		}

		/// <summary>
		/// <see cref="T:ExpressionSyntaxException"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="message">メッセージ</param>
		/// <param name="message">オフセット</param>
		public ExpressionSyntaxException(string message, int offset) : base(message)
		{
			_offset = offset;
		}
	}
}
