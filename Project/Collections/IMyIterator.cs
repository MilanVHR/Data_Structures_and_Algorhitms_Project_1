namespace Project.Collections
{
    public interface IMyIterator<T>
    {
        bool HasNext();
        T Next();
        void Reset();
    }
}
