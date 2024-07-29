namespace Descindie.Legion.EventBus
{
  public interface IEventBusEvent
  {
    T GetContext<T>();
  }
}