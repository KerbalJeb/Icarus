using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponFx : MonoBehaviour
{
    public GameObject trace;
    public GameObject hit;

    private readonly List<ParticleSystem> hitParticleSystems   = new List<ParticleSystem>();
    private readonly List<ParticleSystem> traceParticleSystems = new List<ParticleSystem>();

    private void OnDestroy()
    {
        foreach (ParticleSystem system in hitParticleSystems.Where(system => system != null))
            Destroy(system.gameObject);

        foreach (ParticleSystem system in traceParticleSystems.Where(system => system != null))
            Destroy(system.gameObject);
    }

    public void ApplyFX(Vector3 startPos, Vector3 endPos, Vector3 hitPos)
    {
        ParticleLine(startPos, hitPos, traceParticleSystems, trace);
        if (hitPos != endPos) ParticleLine(hitPos, endPos, hitParticleSystems, hit);
    }

    private void ParticleLine(Vector3 start, Vector3 end, List<ParticleSystem> systems, GameObject template)
    {
        ParticleSystem particle = systems.FirstOrDefault(system => !system.IsAlive());

        if (particle is null)
        {
            GameObject go = Instantiate(template);
            particle = go.GetComponent<ParticleSystem>();
            systems.Add(particle);
        }

        Transform lineTransform = particle.transform;

        Vector3 dir         = end - start;
        float   traceLength = dir.magnitude * 0.5f;
        lineTransform.position = start + 0.5f * dir;
        lineTransform.rotation = transform.rotation * Quaternion.Euler(0, 0, 90);

        ParticleSystem.ShapeModule    shape    = particle.shape;
        ParticleSystem.EmissionModule emission = particle.emission;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, (ushort) (traceLength * 10)));
        shape.scale = new Vector3(traceLength, 1, 1);
        particle.Clear();
        particle.Play();
    }
}
