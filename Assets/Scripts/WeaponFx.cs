using UnityEngine;

public class WeaponFx : MonoBehaviour
{
    public  GameObject     trace;
    public  GameObject     hit;
    private ParticleSystem hitParticleSystem;
    private ParticleSystem traceParticleSystem;

    private void Start()
    {
        trace               = Instantiate(trace);
        traceParticleSystem = trace.GetComponent<ParticleSystem>();
        hit                 = Instantiate(hit);
        hitParticleSystem   = hit.GetComponent<ParticleSystem>();
    }

    public void ApplyFX(Vector3 startPos, Vector3 endPos, Vector3 hitPos)
    {
        ParticleLine(trace.transform, startPos, hitPos, traceParticleSystem);
        if (hitPos != endPos) ParticleLine(hit.transform, hitPos, endPos, hitParticleSystem);
    }

    private void ParticleLine(Transform lineTransform, Vector3 start, Vector3 end, ParticleSystem particleSystem)
    {
        Vector3 dir         = end - start;
        float   traceLength = dir.magnitude * 0.5f;
        lineTransform.position = start + 0.5f * dir;
        lineTransform.rotation = transform.rotation * Quaternion.Euler(0, 0, 90);

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = new Vector3(traceLength, 1, 1);
        particleSystem.Clear();
        particleSystem.Play();
    }
}
