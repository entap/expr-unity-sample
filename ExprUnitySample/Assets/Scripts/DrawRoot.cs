using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Entap.Expr;

public class DrawRoot : MonoBehaviour
{
	[SerializeField] InputField MinParam;
	[SerializeField] InputField MaxParam;
	[SerializeField] InputField ExprX;
	[SerializeField] InputField ExprY;
	[SerializeField] InputField ExprZ;
	float angle = 0.0f;

	// 開始
	void Start()
	{
		MinParam.text = "0.0";
		MaxParam.text = "1.0";
		ExprX.text = "t";
		ExprY.text = "t";
		ExprZ.text = "t";
		Debug.Log(Expression.Evaluate<double>("t"));
		Render();
	}
	
	// 更新
	void Update()
	{
		angle += Time.deltaTime * 90.0f;
		gameObject.transform.localRotation = Quaternion.Euler(0, angle, 0);
	}

	// 描画を行う
	public void Render()
	{
		try {
			var positions = new Vector3[101];
			var t0 = float.Parse(MinParam.text);
			var t1 = float.Parse(MaxParam.text);
			var exprX = new Expression(ExprX.text);
			var exprY = new Expression(ExprY.text);
			var exprZ = new Expression(ExprZ.text);
			var vars = new Dictionary<string, object>();
			for (var i = 0; i <= 100; i++) {
				var t = Mathf.Lerp(t0, t1, i / 100.0f);
				vars["t"] = (double)t;
				float x = (float)exprX.Evaluate<double>(vars);
				float y = (float)exprY.Evaluate<double>(vars);
				float z = (float)exprZ.Evaluate<double>(vars);
				positions[i] = new Vector3(x, y, z);
			}
			var r = GetLineRederer();
			r.positionCount = positions.Length;
			r.SetPositions(positions);
		} catch (Exception e) {
			Debug.Log(e.Message);
		}
	}

	// LineRendererを取得する
	LineRenderer GetLineRederer()
	{
		return GetComponent<LineRenderer>();
	}
}
