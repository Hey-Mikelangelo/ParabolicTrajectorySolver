using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[ExecuteInEditMode]
public class ParticleParabolicShooter : MonoBehaviour
{
    [SerializeField] private new ParticleSystem particleSystem;
	[Space(5)]
	public ParabolicTrajectoryConstraints projectileTrajectoryConstraints = new ParabolicTrajectoryConstraints();
    [Space(12)]
    public ParabolicTrajectory projectileTrajectory = new ParabolicTrajectory();

	[HideIf("@projectileTrajectory.angleByMidPoint == true")]
	[SerializeField] private bool setRandomAngle = false;

    [Tooltip("How many seconds more than calculated will be lifetime of the projectile")]
    [SerializeField] private float lifetimeAdd;

    [SerializeField, HideInInspector]
	private bool addedVisuals = false;

    private Vector3 prevOriginPosition;
    private bool isParticleMoving;
    private ParticleSystem.Particle[] ParticleToMove = new ParticleSystem.Particle[1];

    private Vector3 lastParticlePosition;

    public void SetOrigin(Transform originTransform)
    {
		Vector3 originPosition = originTransform.position;
        SetOrigin(originPosition);
    }

    public void SetTarget(Transform targetTransform)
    {
		Vector3 targetPosition = targetTransform.position;
		SetTarget(targetPosition);
    }

    public void SetOrigin(Vector3 newOriginPosition)
    {
		projectileTrajectory.Origin = newOriginPosition;
		RecalculateAndCheckTrajectory();
	}

	public void SetTarget(Vector3 newtargetPosition)
    {
        projectileTrajectory.Target = newtargetPosition;

        if (projectileTrajectory.GetDistanceToOriginFlatY(newtargetPosition)
            <= projectileTrajectoryConstraints.minimalEffordDistance)
        {
            if (projectileTrajectory.angleByMidPoint == false)
            {
                SetMinimalEffordAngle();
            }
        }
        else
        {
            if (projectileTrajectory.angleByMidPoint == false && setRandomAngle)
            {
                SetRandomAngle();
            }
        }
		RecalculateAndCheckTrajectory();
    }

    public void SetMidPoint(Vector3 newMidPoint)
    {
        projectileTrajectory.MidPoint = newMidPoint;
		RecalculateAndCheckTrajectory();
	}

	public void SetAngle(float newInitialAngle)
    {
        projectileTrajectory.initialAngle = newInitialAngle;
		RecalculateAndCheckTrajectory();
	}

	/// <summary>
	/// returns true if projectile trajectory satisfies constraints
	/// </summary>
	public bool RecalculateAndCheckTrajectory()
	{
		RecalculateTrajectory();
		projectileTrajectoryConstraints.CheckConstrains(projectileTrajectory);

		return projectileTrajectoryConstraints.satisfiesConstraints;

	}

	[Button("Shoot")]
	private void ShootFromThisToTarget()
	{
		SetTarget(projectileTrajectory.Target);
		SetOrigin(transform.position);
		ShootParticle();
	}

	[HideIf(nameof(addedVisuals))]
    [Button(nameof(AddVisuals))]
    private void AddVisuals()
    {
        if (!IsAddedVisuals())
        {
            gameObject.AddComponent<ParticleParabolicShooterVisuals>();
        }
        addedVisuals = true;
    }

	private bool IsAddedVisuals()
	{
		return TryGetComponent(out ParticleParabolicShooterVisuals particleParabolicShooterVisuals);
	}

    private void OnEnable()
    {
        isParticleMoving = false;
		addedVisuals = IsAddedVisuals();

		particleSystem = GetComponent<ParticleSystem>();
        SetupEssetialParticleSystemParams(particleSystem);

        Vector3 originPosition = transform.position;
        projectileTrajectory.Origin = originPosition;
        if (projectileTrajectory.Target == originPosition)
        {
            projectileTrajectory.Target = originPosition + new Vector3(20, 0, 0);
        }
        prevOriginPosition = originPosition;
    }

    private void Update()
    {
        CheckChangedOrigin();
        if (!Application.isPlaying)
        {
            CheckMoveManualy();
        }
    }

    private void FixedUpdate()
    {
        if (Application.isPlaying)
        {
            CheckMoveManualy();
        }
        particleSystem.GetParticles(ParticleToMove);
        lastParticlePosition = ParticleToMove[0].position;

    }

    private void OnValidate()
    {
        if (gameObject.activeSelf)
        {
            RecalculateAndCheckTrajectory();
        }
    }

    private void CheckChangedOrigin()
    {
        if (prevOriginPosition != transform.position)
        {
            RecalculateAndCheckTrajectory();
			projectileTrajectory.Origin = transform.position;
			prevOriginPosition = transform.position;
        }
    }

	private void RecalculateTrajectory()
	{
		projectileTrajectory.SetPhysicsParabolicTrajectory();
	}

	private void SetRandomAngle()
    {
        float angle = Random.Range(projectileTrajectoryConstraints.minAngle, projectileTrajectoryConstraints.maxAngle);
        float clampedAngle = Mathf.Clamp(angle, projectileTrajectory.GetMinimalAngle(5), projectileTrajectoryConstraints.maxAngle);

        projectileTrajectory.initialAngle = clampedAngle;
    }

    private void SetMinimalEffordAngle()
    {
        float angle = projectileTrajectory.GetMinimalAngle(projectileTrajectoryConstraints.minEffordAngle);
        float clampedAngle = Mathf.Clamp(angle, projectileTrajectoryConstraints.minAngle, projectileTrajectoryConstraints.maxAngle);

        projectileTrajectory.initialAngle = clampedAngle;
    }

    private void CheckMoveManualy()
    {
        if (isParticleMoving && projectileTrajectory.useSpeedCurve)
        {
            bool isEnd;
            particleSystem.GetParticles(ParticleToMove);
            Vector3 currentPosition = ParticleToMove[0].position;
            Vector3 newPosition = projectileTrajectory.GetCustomSpeedTrajectoryPoint(currentPosition, out isEnd);
            Vector3 direction = newPosition - currentPosition;
            float speed = Vector3.Distance(newPosition, currentPosition) / Time.deltaTime;
            ParticleToMove[0].velocity = direction * speed;
            ParticleToMove[0].position = newPosition;
            particleSystem.SetParticles(ParticleToMove, 1);
            if (isEnd)
            {
                particleSystem.GetParticles(ParticleToMove);
                ParticleSystemHelper.SetParticleSystemGravity(projectileTrajectory.gravityMultiplier, particleSystem);
                Vector3 velocityVector = (newPosition - currentPosition) / Time.deltaTime;
                ParticleToMove[0].velocity = velocityVector;
                particleSystem.SetParticles(ParticleToMove, 1);

                isParticleMoving = false;
            }
        }
    }

    private void SetValuesInPartileSystem(ParticleSystem particleSystem,
        Vector3 position, Quaternion rotation, float initialSpeed, float gravityMult, float lifetime)
    {
        Transform particleSystemTransform = particleSystem.transform;
        ParticleSystemHelper.SetParticleSystemGravity(gravityMult, particleSystem);
        ParticleSystemHelper.SetParticleSystemSpeed(initialSpeed, particleSystem);
        ParticleSystemHelper.SetParticleSystemLifetime(lifetime, particleSystem);
        particleSystemTransform.position = position;
        particleSystemTransform.rotation = rotation;
    }

    private void ShootParticle()
    {
        isParticleMoving = false;
        if (!projectileTrajectory.useSpeedCurve)
        {
            ShootParticleWithPhysics();
        }
        else
        {
            ShootParticleWithCustomSpeed();
        }
    }

    private void ShootParticleWithCustomSpeed()
    {
        if (!isParticleMoving)
        {
            ParticleSystemHelper.SetParticleSystemLifetime(projectileTrajectory.flightTime + lifetimeAdd, particleSystem);
            ParticleSystemHelper.SetParticleSystemSpeed(0, particleSystem);
            ParticleSystemHelper.SetParticleSystemGravity(0, particleSystem);

            ParticleSystemHelper.PlayParticleSystemOneParticle(particleSystem);
            particleSystem.GetParticles(ParticleToMove);
            ParticleToMove[0].position = projectileTrajectory.Origin;
            particleSystem.SetParticles(ParticleToMove, 1);
            isParticleMoving = true;
        }

    }

    private void ShootParticleWithPhysics()
    {
        if (!isParticleMoving)
        {
            isParticleMoving = true;
            Quaternion rotation = projectileTrajectory.GetStartRotation();

            SetValuesInPartileSystem(
                particleSystem,
                projectileTrajectory.Origin,
                rotation,
                projectileTrajectory.initialSpeed,
                projectileTrajectory.gravityMultiplier,
                projectileTrajectory.flightTime + lifetimeAdd);

            if (float.IsNaN(projectileTrajectory.flightTime))
            {
                isParticleMoving = false;
            }
            else
            {
                StartCoroutine(ResetIsParticleMoving(projectileTrajectory.flightTime));
            }
            ParticleSystemHelper.PlayParticleSystemOneParticle(particleSystem);
        }

    }

    private IEnumerator ResetIsParticleMoving(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        isParticleMoving = false;
    }

    private void SetupEssetialParticleSystemParams(ParticleSystem particleSystem)
    {
        var partSysMainModule = particleSystem.main;
        partSysMainModule.playOnAwake = false;
        partSysMainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        partSysMainModule.simulationSpeed = 1;

        var partSysCollisionModule = particleSystem.collision;
        partSysCollisionModule.sendCollisionMessages = true;

		var partSysShapeModule = particleSystem.shape;

		partSysShapeModule.shapeType = ParticleSystemShapeType.Cone;
		partSysShapeModule.angle = 0;
		partSysShapeModule.radius = 0.0001f;


	}

}
