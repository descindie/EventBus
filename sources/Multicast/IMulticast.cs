namespace Descindie.Legion.EventBus
{
  public interface IMulticast<T>
  {
    int Count { get; }
    int Capacity { get; }

    T[] ToArray();
  }
}