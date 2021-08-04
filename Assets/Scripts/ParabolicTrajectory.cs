using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParabolicTrajectory
{
	[HideInInspector] public Parabola parabola;

    public bool angleByMidPoint = false;
    public bool useNormalSpeed = true;
    public bool useSpeedCurve = false;
    [Space(10)]

    [DisableIf("angleByMidPoint")]
    [Range(-45, 89)]
    public float initialAngle = 45;

    [DisableIf("useNormalSpeed")]
    [Range(0, 500)]
    public float initialSpeed = 30;

    [EnableIf("useSpeedCurve")]
    public AnimationCurve speedByDistance;
    [Space(10)]

    [ReadOnly]
    public float flightTime;
    [ReadOnly]
    public float gravityMultiplier;
    [ReadOnly]
    public float distanceFlatY;

    [SerializeField] private Vector3 origin, target = Vector3.one, midPoint;

	private Vector3 direction;
	private Vector3 directionFlatY;
	private List<Vector3> ParabolaPositions = new List<Vector3>(1);
    private const float dx = 0.01f;

    public Vector3 Origin
    {
        get { return origin; }
        set
        {
            origin = value;
            CalculatePrivateParams();
        }
    }

    public Vector3 Target
    {
        get { return target; }
        set
        {
            target = value;
            CalculatePrivateParams();
        }
    }

    public Vector3 MidPoint { get => midPoint; set => midPoint = value; }

    public float GetMinimalAngle(float minAngleAddition = 1)
    {
        float angle = Vector3.SignedAngle(direction, directionFlatY, -GetSideDirection()) + minAngleAddition;
        return angle;
    }

    public ParabolicTrajectory()
    {
        parabola = new Parabola();
    }

    public Quaternion GetStartRotation()
    {
        return GetStartRotation(directionFlatY, initialAngle);
    }

    public List<Vector3> GetPositions(float xStep)
    {
        ParabolaPositions.Clear();

        for (float x = 0; x < distanceFlatY; x += xStep)
        {
            ParabolaPositions.Add(GetPositionByX(x));
        }
        return ParabolaPositions;
    }

    public ParabolicTrajectory SetPhysicsParabolicTrajectory()
    {
        return SetPhysicsParabolicTrajectory(origin, target, midPoint, initialAngle, initialSpeed, angleByMidPoint, useNormalSpeed);
    }

    public ParabolicTrajectory SetPhysicsParabolicTrajectory(
        Vector3 originPosition, Vector3 targetPosition, Vector3 midPointPosition,
        float initialAngle, float initialSpeed,
        bool angleByMidPoint, bool useNormalSpeed)
    {
        float gravity, height;
        Vector3 midPointLocal, originLocal, targetLocal;

        origin = originPosition;
        target = targetPosition;
        midPoint = midPointPosition;
        CalculatePrivateParams();

        Matrix4x4 originPosMatrix = Matrix4x4.identity;
        originPosMatrix.SetTRS(this.origin, Quaternion.identity, Vector3.one);

        originLocal = originPosMatrix.inverse.MultiplyPoint3x4(originPosition);
        targetLocal = originPosMatrix.inverse.MultiplyPoint3x4(targetPosition);
        midPointLocal = originPosMatrix.inverse.MultiplyPoint3x4(midPoint);

        height = parabola.GetHeight(this.origin, target);
        gravity = parabola.GetGravity();

        if (angleByMidPoint)
        {
            parabola.CalculateParabolaBy3Points(originLocal, targetLocal, midPointLocal);
            initialAngle = parabola.GetStartAngle();
        }
        else
        {
            parabola.CalculateParabolaBy2PointsAndAngle(originLocal, targetLocal, initialAngle);
            midPoint = GetPointOnExtremum(parabola, directionFlatY, origin);
        }

        if (useNormalSpeed)
        {
            initialSpeed = parabola.GetLaunchSpeedNormalGravity();
            gravityMultiplier = 1;
            flightTime = parabola.GetTimeOfFlight(initialSpeed, initialAngle,
                gravity * gravityMultiplier, height);
        }
        else
        {
            gravityMultiplier = parabola.GetGravityMultFromDesiredSpeed(initialSpeed);
            flightTime = parabola.GetTimeOfFlight(
                initialSpeed, initialAngle, gravity * gravityMultiplier, height);

        }
        this.initialSpeed = initialSpeed;
        this.initialAngle = initialAngle;

        return this;
    }

    /// <summary>
    /// returns new point on parabola
    /// </summary>
    public Vector3 GetCustomSpeedTrajectoryPoint(Vector3 point, out bool isEnd)
    {
        float currentDistanceToOrigin = GetDistanceToOriginFlatY(point);
        float currentProgress = GetDistanceFraction(currentDistanceToOrigin);

        float speedMultiplier = speedByDistance.Evaluate(currentProgress);
        speedMultiplier = (speedMultiplier <= 0) ? 0.01f : speedMultiplier;
        float currentSpeed = speedMultiplier * initialSpeed;

        float x = GetDistanceToOriginFlatY(point);
        Vector3 tangentDirection = GetTangentDirection(x);

		//Unity automaticaly sets Time.deltaTime equal to Time.fixedDeltaTime if method is called from FixedUpdate().
		//Might not work in older Unity versions.
		float deltaTime = Time.deltaTime;
		Vector3 newPosition = point + (tangentDirection.normalized * currentSpeed * deltaTime);

        Vector3 newPoint = GetPointProjectedOnParabola(newPosition);

        float newDistanceToOrigin = GetDistanceToOriginFlatY(newPoint);
        float progress = GetDistanceFraction(newDistanceToOrigin);
        isEnd = (progress >= 1);
        return newPoint;
    }

    public float GetDistanceToOriginFlatY(Vector3 point)
    {
        return Vector3.Distance(new Vector3(point.x, 0, point.z), new Vector3(origin.x, 0, origin.z));
    }

    private Vector3 GetPointOnExtremum(Parabola parabola, Vector3 directionFlatY, Vector3 origin)
    {
        Vector2 extremumXY = parabola.GetExtremumXY();
        Vector3 point = (origin + (directionFlatY.normalized * extremumXY.x));
        point.y = origin.y + extremumXY.y;

        return point;
    }

    private void CalculatePrivateParams()
    {
        direction = target - origin;
        directionFlatY = MathUtils.GetDirectionFlatY(origin, target);
        distanceFlatY = MathUtils.GetDistanceFlatY(origin, target);
    }

	private Vector3 GetSideDirection()
	{
		return Vector3.Cross(directionFlatY, Vector3.up);
	}

	private Quaternion GetStartRotation(Vector3 directionFlatY, float angle)
	{
		Quaternion rotation = Quaternion.LookRotation(directionFlatY, Vector3.up);
		rotation.eulerAngles = new Vector3(-angle, rotation.eulerAngles.y, rotation.eulerAngles.z);

		return rotation;
	}

	private Vector3 GetTangentDirection(float x)
	{
		Vector3 p1 = GetPositionByX(x);
		Vector3 p2 = GetPositionByX(x + dx);
		Vector3 direction = p2 - p1;
		return direction;
	}

	private float GetDistanceFraction(float distance)
	{
		distance = Mathf.Abs(distance);
		return distance / distanceFlatY;
	}

	private Vector3 GetPointProjectedOnParabola(Vector3 point)
	{
		float distanceFromOriginFlatY = Vector3.Distance(new Vector3(point.x, 0, point.z), new Vector3(origin.x, 0, origin.z));

		return GetPositionByX(distanceFromOriginFlatY);
	}

	private Vector3 GetPositionByX(float x)
	{
		return new Vector3(
				origin.x + direction.x * (x / distanceFlatY),
				origin.y + parabola.GetY(x),
				origin.z + direction.z * (x / distanceFlatY));
	}
}
