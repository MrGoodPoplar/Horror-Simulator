using System;
using Audio_System;

namespace Surface_System
{
    [Serializable]
    public record SurfaceImpactSound
    {
        public ArraySoundData bulletImpactSounds;
        public ArraySoundData stepImpactSounds;
        public ArraySoundData landingImpactSounds;
        public ArraySoundData bulletDropImpactSounds;
    }
}