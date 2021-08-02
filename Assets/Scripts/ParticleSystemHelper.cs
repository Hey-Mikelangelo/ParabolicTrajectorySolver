using UnityEngine;

public static class ParticleSystemHelper
{
    public static void PlayAndSetOneParticle(ParticleSystem particleSystem, Vector3 initialPosition)
    {
        ParticleSystem.Particle[] ParticlesToMove = new ParticleSystem.Particle[1];
        PlayParticleSystemOneParticle(particleSystem);
        particleSystem.GetParticles(ParticlesToMove);
        ParticlesToMove[0].position = initialPosition;
        particleSystem.SetParticles(ParticlesToMove, 1);
    }

    public static void SetParticleSystemSpeed(float speed, ParticleSystem particleSystem)
    {
        var partSysMainModule = particleSystem.main;
        partSysMainModule.startSpeed = new ParticleSystem.MinMaxCurve(speed);
    }

    public static void SetParticleSystemGravity(float mult, ParticleSystem particleSystem)
    {
        var partSysMainModule = particleSystem.main;
        partSysMainModule.gravityModifier = new ParticleSystem.MinMaxCurve(Mathf.Abs(mult));
    }

    public static void SetParticleSystemLifetime(float lifetime, ParticleSystem particleSystem)
    {
        var partSysMainModule = particleSystem.main;
        partSysMainModule.startLifetime = new ParticleSystem.MinMaxCurve(Mathf.Abs(lifetime));
    }

    public static void PlayParticleSystemOneParticle(ParticleSystem particleSystem)
    {
        particleSystem.Clear();
        particleSystem.Emit(1);
    }
    public static void StopAndResetParticleSystem(ParticleSystem particleSystem)
    {
        particleSystem.Clear();

    }
}
