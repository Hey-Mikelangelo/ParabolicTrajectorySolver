using System.Collections.Generic;
using UnityEngine;
using static MathUtils;

[System.Serializable]
public class Parabola
{
    public float a, b, c;

    private float dx = 0.002f;

    /// <summary>
    /// sets values a, b, c of this parabola. parameter "origin" should be Vector.zero
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="angle"></param>
    public void CalculateParabolaBy2PointsAndAngle(Vector3 origin, Vector3 target, float angle)
    {
        Vector3 directionFlatY = GetDirectionFlatY(origin, target);
        Vector3 midPoint = GetPointToFormAngle(angle, origin, directionFlatY);
        CalculateParabolaBy3Points(origin, target, midPoint);
    }

    public void CalculateParabolaBy3Points(Vector3 origin, Vector3 target, Vector3 midPoint)
    {
        Vector3 direction = target - origin;

        Vector3 normalToDir = Vector3.Cross(direction, Vector3.up);

        Vector3 p1FlatZ, p2FlatZ, midPointFlatZ;
        p1FlatZ = GetPointFlatZLocal(origin, normalToDir, origin);
        p2FlatZ = GetPointFlatZLocal(target, normalToDir, origin);
        midPointFlatZ = GetPointFlatZLocal(midPoint, normalToDir, origin);
        GetParabolaBy3Points(p1FlatZ, midPointFlatZ, p2FlatZ, out a, out b, out c);
    }

    public Vector3 GetPointToFormAngle(float angle, Vector3 firstPoint, Vector3 directionFlatY)
    {
        Vector3 secondPoint = firstPoint + (directionFlatY.normalized * dx);
        float angleRadians = angle * Mathf.Deg2Rad;
        float y = Mathf.Tan(angleRadians) * dx;
        secondPoint.y = y;
        return secondPoint;
    }

    public float GetStartAngle()
    {
        return GetStartAngle(a, b, c);
    }

	public Vector2 GetTangent(float a, float b, float c, float x)
	{
		Vector2 parabolaPointX = new Vector2(x, GetParabolaY(x, a, b, c));
		Vector2 parabolaPoinXPlusDx = new Vector2(x + dx, GetParabolaY(x + dx, a, b, c));
		Vector2 tangentDirection = parabolaPoinXPlusDx - parabolaPointX;
		return tangentDirection;
	}

	public float GetStartAngle(float a, float b, float c)
	{
		Vector2 xAxisDirection = new Vector2(1, 0);
		Vector2 ZeroXTangent = GetTangent(a, b, c, 0);
		return Vector2.Angle(ZeroXTangent, xAxisDirection);
	}

	
    public float GetXAxisIntersection()
    {
        return GetParabolaXAxisIntersection(a, b, c);
    }

    public float GetParabolaXAxisIntersection(float a, float b, float c)
    {
        float x1 = (-b + Mathf.Sqrt(b * b - (4 * a * c))) / (2 * a);
        if (x1 != 0)
        {
            return x1;
        }
        float x2 = (-b - Mathf.Sqrt(b * b - (4 * a * c))) / (2 * a);

        return x2;
    }

    public Vector2 GetExtremumXY()
    {
        float x = GetExtremumX();
        return new Vector2(x, GetY(x));
    }

    public float GetExtremumX()
    {
        return GetParabolaExtremumX(a, b);
    }

    public float GetParabolaExtremumX(float a, float b)
    {
        return -b / (2 * a);
    }

    public float GetGravity()
    {
        return Physics.gravity.y;
    }

    public float GetGravityMultFromDesiredSpeed(float desiredSpeed)
    {
        return GetGravityMultBySpeed(GetLaunchSpeedNormalGravity(), desiredSpeed);
    }

    public float GetGravityMultBySpeed(float calculatedSpeed, float desiredSpeed)
    {
        float speedDifference = desiredSpeed / calculatedSpeed;
        float gravityMult = speedDifference * speedDifference;
        return gravityMult;
    }

    public float GetAngle(float distToXAxisIntersection, float gravity, float speed)
    {
        float angleRadians = 0.5f * Mathf.Asin((-gravity * distToXAxisIntersection) / (speed * speed));
        return angleRadians * Mathf.Rad2Deg;
    }

    public float GetLaunchSpeedNormalGravity()
    {
        float distToXAxisIntersection, gravity, angle;
        distToXAxisIntersection = GetXAxisIntersection();
        gravity = GetGravity();
        angle = GetStartAngle();
        return GetLaunchSpeed(distToXAxisIntersection, gravity, angle);
    }

	public float GetLaunchSpeed(float distToXAxisIntersection, float gravity, float angle)
	{
		float angleRadians = Mathf.Deg2Rad * angle;
		return Mathf.Sqrt((Mathf.Abs(gravity) * Mathf.Abs(distToXAxisIntersection)) / Mathf.Sin(Mathf.Abs(angleRadians) * 2));
	}

    public float GetLaunchSpeed(float timeOfFlight, float gravityMult, Vector3 p1, Vector3 p2)
    {
        float angle, gravity, heightDiff;
        gravity = GetGravity() * gravityMult;
        angle = GetStartAngle();
        heightDiff = GetHeight(p1, p2);
        return GetLaunchSpeed(timeOfFlight, angle, gravity, heightDiff);
    }

    public float GetLaunchSpeed(float timeOfFlight, float gravity, float angle, float heightDiff)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        timeOfFlight = Mathf.Abs(timeOfFlight);
        float speed = (timeOfFlight * gravity - (2 * heightDiff)) / (2 * timeOfFlight * Mathf.Sin(angleRadians));
        return speed;
    }

    public float GetHeight(Vector3 p1, Vector3 p2)
    {
        return p1.y - p2.y;
    }

    public float GetTimeOfFlight(float speed, float angle, float gravity, float height)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        return (speed * Mathf.Sin(angleRadians) + Mathf.Sqrt(Mathf.Pow(speed * Mathf.Sin(angleRadians), 2) + (2 * Mathf.Abs(gravity) * height))) / Mathf.Abs(gravity);
    }

    public float CalculateRangeOfProjectile(float speed, float angle, float gravity, float height)
    {
        float t = GetTimeOfFlight(speed, angle, gravity, height);
        float angleRadians = angle * Mathf.Deg2Rad;
        return speed * Mathf.Cos(angleRadians) * t;
    }

    public Vector2 GetPointFlatZLocal(Vector3 worldPos, Vector3 planeNormal, Vector3 zeroPositionWorld)
    {
        Quaternion xAxisToDirectionRotation = Quaternion.LookRotation(planeNormal, Vector3.up);
        Matrix4x4 matrixTrans = Matrix4x4.identity;
        matrixTrans.SetTRS(zeroPositionWorld, xAxisToDirectionRotation, Vector3.one);
        Vector3 pointFlatZ = matrixTrans.inverse.MultiplyPoint3x4(worldPos);
        pointFlatZ.z = 0;
        return pointFlatZ;
    }

    public void GetParabolaBy3Points(Vector2 P1, Vector2 P2, Vector2 P3, out float a, out float b, out float c)
    {
        float A1 = -(P1.x * P1.x) + (P2.x * P2.x);
        float B1 = -P1.x + P2.x;
        float D1 = -P1.y + P2.y;
        float A2 = -(P2.x * P2.x) + (P3.x * P3.x);
        float B2 = -P2.x + P3.x;
        float D2 = -P2.y + P3.y;
        float Bmult = -(B2 / B1);
        float A3 = Bmult * A1 + A2;
        float D3 = Bmult * D1 + D2;
        a = D3 / A3;
        b = (D1 - (A1 * a)) / B1;
        c = P1.y - a * (P1.x * P1.x) - b * P1.x;
    }

    public float GetY(float x)
    {
        return GetParabolaY(x, a, b, c);
    }

    public float GetParabolaY(float x, float a, float b, float c)
    {
        return a * (x * x) + b * x + c;
    }
}
