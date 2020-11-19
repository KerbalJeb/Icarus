using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
///     A class to display particle effects
/// </summary>
public class WeaponFx : MonoBehaviour
{
    /// <value>
    ///     A gameobject with the particle effect for the weapon until it hits an object
    /// </value>
    public GameObject trace;

    /// <value>
    ///     A gameobject with the particle effect for the weapon after it hits an object
    /// </value>
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

    /// <summary>
    ///     Displays the weapon effects
    /// </summary>
    /// <param name="startPos">The origin of the effect</param>
    /// <param name="endPos">The end position of the effect</param>
    /// <param name="hitPos">Where the weapon hit its target</param>
    public void ApplyFX(Vector3 startPos, Vector3 endPos, Vector3 hitPos)
    {
        ParticleLine(startPos, hitPos, traceParticleSystems, trace);
        if (hitPos != endPos) ParticleLine(hitPos, endPos, hitParticleSystems, hit);
    }

    /// <summary>
    ///     Scales a gameobject with a line particle effect to reach between two points. Will get or create a new gameobject
    /// </summary>
    /// <param name="start">The start position of the line</param>
    /// <param name="end">The end position of the line</param>
    /// <param name="systems">The particle systems available to use</param>
    /// <param name="template">The template to use if a new gameobject is needed</param>
    private void ParticleLine(Vector3 start, Vector3 end, ICollection<ParticleSystem> systems, GameObject template)
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
