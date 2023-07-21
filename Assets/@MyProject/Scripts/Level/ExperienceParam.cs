using JetBrains.Annotations;

namespace MyProject
{
    public struct ExperienceParam
    {
        public uint tick;
        [CanBeNull] public object source; // 로컬 환경에서만 사용해야 합니다.
        public int? sourceNetworkObjectId; // source가 network object인 경우, 이곳에 id를 담습니다.
        public int experience;
    }
}