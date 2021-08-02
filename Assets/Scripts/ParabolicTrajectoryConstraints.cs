using UnityEngine;

[System.Serializable]
public class ParabolicTrajectoryConstraints
{
    [HideInInspector] public bool satisfiesConstraints;
    [Range(-45, 89)]
    public float minAngle = 0;
    [Range(-45, 89)]
    public float maxAngle = 60;
    [Range(0, 500)]
    public float minRange = 10;
    [Range(0, 500)]
    public float maxRange = 100;
    [Range(0, 500)]
    public float minSpeed = 10;
    [Range(0, 500)]
    public float maxSpeed = 100;
    [Range(0, 500)]
    public float minGravityMult = 0;
    [Range(0, 500)]
    public float maxGravityMult = 5;
    [Range(0, 100)]
    public float minimalEffordDistance = 5;
    [Range(1, 90)]
    public float minEffordAngle = 2;


    /// <summary>
    /// returns true if constrains of the parabolic trajectory are preserved
    /// </summary>
    public bool CheckConstrains(
        ParabolicTrajectory parabolicTrajectory)
    {
        float angle = parabolicTrajectory.initialAngle;
        float range = parabolicTrajectory.distanceFlatY;
        float speed = parabolicTrajectory.initialSpeed;
        float gravityMult = parabolicTrajectory.gravityMultiplier;

        if (
            angle < minAngle || angle > maxAngle ||
            range < minRange || range > maxRange ||
            speed < minSpeed || speed > maxSpeed ||
            gravityMult < minGravityMult || gravityMult > maxGravityMult)
        {
            satisfiesConstraints = false;
        }
        else
        {
            satisfiesConstraints = true;
        }
        return satisfiesConstraints;
    }
}
