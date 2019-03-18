namespace UltimateTerrains
{
    internal interface IPriorizedBlockingQueue<T>
    {
        bool IsEmpty { get; }
        void Add(T item);
        void AddHighPriority(T item);
        T BlockingTake();
        void Close();
    }
}