using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParticleParabolicShooterVisuals))]
public class ParticleParabolicShooterVisualsEditor : Editor
{
    private ParticleParabolicShooter projectileShooter;
    private const float cubeCapSize = 0.2f;

	private void OnEnable()
    {
		SerializedObject serializedObject = new SerializedObject(target);
		SerializedProperty particleParabolicShooter = serializedObject.FindProperty("particleParabolicShooter");
		projectileShooter = particleParabolicShooter.objectReferenceValue as ParticleParabolicShooter;
	}

	public virtual void OnSceneGUI()
    {
		Vector3 originPos = projectileShooter.projectileTrajectory.Origin;
        Vector3 targetPos = projectileShooter.projectileTrajectory.Target;
        Vector3 midPointPos = projectileShooter.projectileTrajectory.MidPoint;
        float maxRange = projectileShooter.projectileTrajectoryConstraints.maxRange;
        float minRange = projectileShooter.projectileTrajectoryConstraints.minRange;

        EditorGUI.BeginChangeCheck();

        Vector3 newTargetPos = Handles.PositionHandle(targetPos, Quaternion.identity);
        Vector3 newMidPointPos = Handles.PositionHandle(midPointPos, Quaternion.identity);

		Vector3 newMidPointPosOnLine = GetPointOnLine(originPos, targetPos, newMidPointPos);

		if (EditorGUI.EndChangeCheck() || newMidPointPosOnLine != newMidPointPos)
        {
            Undo.RecordObject(projectileShooter, "Projectile shooter changed positions");
            newTargetPos = GetPointInRange(originPos, minRange, maxRange, newTargetPos);
            projectileShooter.projectileTrajectory.Target = newTargetPos;
            projectileShooter.projectileTrajectory.MidPoint = newMidPointPosOnLine;
            projectileShooter.RecalculateAndCheckTrajectory();

        }

        DrawVisuals(projectileShooter.projectileTrajectory, projectileShooter.projectileTrajectoryConstraints);

    }

    private void DrawTrajectory(ParabolicTrajectory projectileTrajectory, bool satisfiesConstraints)
    {
        if (satisfiesConstraints) { 
            Handles.color = Color.green;
        }
        else
        {
            Handles.color = Color.red;
        }
        Vector3 origin = projectileTrajectory.Origin;
        List<Vector3> parabolaPoints = projectileTrajectory.GetPositions(1);
        int count = parabolaPoints.Count;
        for (int i = 0; i < count; i++)
        {
            Handles.DrawLine(origin, parabolaPoints[i]);
        }
    }

    private void DrawVisuals(ParabolicTrajectory parabolicTrajectory,
        ParabolicTrajectoryConstraints parabolicTrajectoryConstrains)
    {
        Vector3 originPos = parabolicTrajectory.Origin;
        Vector3 midPos = parabolicTrajectory.MidPoint;
        Vector3 targetPos = parabolicTrajectory.Target;

        float minRange = parabolicTrajectoryConstrains.minRange;
        float maxRange = parabolicTrajectoryConstrains.maxRange;
        float minEffordDistance = parabolicTrajectoryConstrains.minimalEffordDistance;

        DrawCubeCaps(originPos, midPos, targetPos);
        DrawWireDisc(originPos, minRange, Color.blue);
        DrawWireDisc(originPos, maxRange, Color.blue);
        DrawWireDisc(originPos, minEffordDistance, Color.cyan);
        DrawTrajectory(parabolicTrajectory, parabolicTrajectoryConstrains.satisfiesConstraints);
    }

    private void DrawWireDisc(Vector3 originPos, float radius, Color color)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Handles.color = color;
            Handles.DrawWireDisc(originPos, Vector3.up, radius);
        }
    }

    private void DrawCubeCaps(Vector3 originPos, Vector3 midPos, Vector3 targetPos)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Color cyan = Color.cyan;
            Color red = Color.red;
            Color yellow = Color.yellow;

            Handles.color = cyan;

            Handles.CubeHandleCap(
                0,
                midPos,
                Quaternion.identity,
                cubeCapSize,
                EventType.Repaint
            );

            Handles.color = red;

            Handles.CubeHandleCap(
                    1,
                    targetPos,
                    Quaternion.identity,
                    cubeCapSize,
                    EventType.Repaint
                );


            Handles.color = yellow;

            Handles.CubeHandleCap(
                    2,
                    originPos,
                    Quaternion.identity,
                    cubeCapSize,
                    EventType.Repaint
                );
        }
    }

    private Vector3 GetPointOnLine(Vector3 p1, Vector3 p2, Vector3 point)
    {
        Vector3 lineDirection = p2 - p1;
        Vector3 lineDirectionFlatY = lineDirection;
        lineDirectionFlatY.y = 0;

        float distToPointFlatY = Vector3.Distance(new Vector3(point.x, 0, point.z),
            new Vector3(p1.x, 0, p1.z));

        float pointY = point.y;

        Vector3 newPoint = p1 + (lineDirectionFlatY.normalized * distToPointFlatY);
        newPoint.y = pointY;
        return newPoint;
    }

    private Vector3 GetPointInRange(Vector3 origin, float minRange, float maxRange, Vector3 point)
    {
        Vector3 pointFlatY = new Vector3(point.x, 0, point.z);
        Vector3 originFlatY = new Vector3(origin.x, 0, origin.z);

        Vector3 directionToNewTargetFlatY = pointFlatY - originFlatY;
        float distanceToNewTargetFlatY = Vector3.Distance(pointFlatY, originFlatY);
        Vector3 pointInRange = point;
        float pointY = point.y;
        if (distanceToNewTargetFlatY > maxRange)
        {
            pointInRange = origin + (directionToNewTargetFlatY.normalized * maxRange);
        }
        else if(distanceToNewTargetFlatY < minRange)
        {
            pointInRange = origin + (directionToNewTargetFlatY.normalized * minRange);
        }
        pointInRange.y = pointY;


        return pointInRange;
    }
}
