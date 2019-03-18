namespace UltimateTerrains
{
    internal interface ITask
    {
        void Execute(ThreadSpecificParams threadParams);
    }
}