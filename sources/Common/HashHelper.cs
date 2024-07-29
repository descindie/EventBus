namespace Descindie.Legion.EventBus
{
  internal static class HashHelper
  {
    public const int PRIME_MUL = 2;
    public const int PRIME_MIN = 2;

    public static int GenSize(int value)
    {
      if (value < PRIME_MIN)
        return PRIME_MIN;

      while (!IsPrime(value)) ++value;
      return value;
    }

    public static bool IsPrime(int value)
    {
      if (value < PRIME_MIN)
        return false;

      for (var i = PRIME_MIN; i * i <= value; i++)
      {
        if (value % i == 0)
          return false;
      }

      return true;
    }
  }
}