using UnityEngine;

[ExecuteInEditMode]
public class ParticleParabolicShooterVisuals : MonoBehaviour
{
    [SerializeField] private ParticleParabolicShooter particleParabolicShooter;

    private void OnEnable()
    {
        particleParabolicShooter = GetComponent<ParticleParabolicShooter>();
    }
}
