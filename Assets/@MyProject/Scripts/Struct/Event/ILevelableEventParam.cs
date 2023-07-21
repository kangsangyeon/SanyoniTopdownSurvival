namespace MyProject.Event
{
    [System.Serializable]
    public struct ILevelable_CurrentExperienceOrLevelChanged_EventParam
    {
        public int currentExperience;
        public int currentLevel;
        public int addedExperience;
        public bool becomeNewLevel;
    }
}