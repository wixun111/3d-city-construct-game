using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public class ParticleManager : Singleton<ParticleManager>
    {
        [SerializeField] List<ParticleSystem> particleSystems;

        public ParticleSystem GetParticleSystem(int index)
        {
            return particleSystems[index];
        }
    }
}