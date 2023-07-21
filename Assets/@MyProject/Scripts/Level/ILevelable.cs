using MyProject.Event;

namespace MyProject
{
    public interface ILevelable
    {
        int currentLevel { get; }
        int maxLevel { get; }
        int currentExperience { get; }
        int requiredExperienceToNextLevel { get; }

        event System.Action<ILevelable_CurrentExperienceOrLevelChanged_EventParam> onCurrentExperienceOrLevelChanged;

        void AddExperience(in ExperienceParam _param, out int _addedExperience);
    }
}