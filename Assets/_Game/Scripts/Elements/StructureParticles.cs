using UnityEngine;

public class StructureParticles : MonoBehaviour
{
    [field: SerializeField] public ParticleSystem Burst { get; private set; }
    [field: SerializeField] public ParticleSystem Constant { get; private set; }
}
