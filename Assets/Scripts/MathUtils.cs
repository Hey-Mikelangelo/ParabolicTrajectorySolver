using UnityEngine;

public static class MathUtils
{
	public static Vector3 GetDirectionFlatY(Vector3 p1, Vector3 p2)
	{
		p1.y = 0;
		p2.y = 0;
		return p2 - p1;
	}

	public static float GetDistanceFlatY(Vector3 p1, Vector3 p2)
	{
		p1.y = 0;
		p2.y = 0;
		return Vector3.Distance(p1, p2);
	}
}
