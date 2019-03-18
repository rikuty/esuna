namespace UltimateTerrains
{
    internal sealed class PersistOperationsTask : ITask
    {
        private UltimateOperationsManager operationManager;

        public void Init(UltimateOperationsManager operationManager)
        {
            this.operationManager = operationManager;
        }

        public void Execute(ThreadSpecificParams threadParams)
        {
            operationManager.PersistModifiedRegions();
        }
    }
}