namespace Descindie.Legion.EventBus
{
  using System.Runtime.CompilerServices;
  using System;
  using static System.Runtime.CompilerServices.MethodImplOptions;

  /// <summary>
  /// Low-allocating multicast delegate.
  /// </summary>
  public abstract class BaseMulticast<T> : IMulticast<T>, IDisposable
  where T : Delegate
  {
    protected ref struct Enumerator
    {
      private readonly BaseMulticast<T> _multicast;
      private readonly Entry[] _entries;
      private int _index;
      private int _count;

      [MethodImpl(AggressiveInlining)]
      internal Enumerator(BaseMulticast<T> multicast)
      {
        _entries = multicast._entries;
        _count = multicast._count;
        _multicast = multicast;
        _index = 0;
        _multicast._isEnumerating = true;
      }

      [MethodImpl(AggressiveInlining)]
      public void Dispose()
      {
        _multicast._isEnumerating = false;
      }

      [MethodImpl(AggressiveInlining)]
      public bool MoveNext(out T current)
      {
        var inRange = _index < _count;
        current = inRange
          ? _entries[_index++].Invocation
          : null;

        return inRange;
      }
    }

    private struct Entry
    {
      public T Invocation;
      public int Hash;
      public int PrevIndex;

      [MethodImpl(AggressiveInlining)]
      public void Set(T invocation, int hash = 0, int prevIndex = -1)
      {
        Invocation = invocation;
        Hash = hash;
        PrevIndex = prevIndex;
      }
    }

    public const int CAPACITY_DEFAULT = 7;

    #region Exception messages
    private const string ALREADY_ERR_MESSAGE = "The handler is already subscribed.";
    private const string MODIFIED_ERR_MESSAGE = "The collection was modified after the enumerator was created.";
    private const string EXCEEDED_ERR_MESSAGE = "The number of collisions has been exceeded.";
    #endregion

    public int Capacity => _capacity;
    public int Count => _count;

    private int[] _buckets;
    private Entry[] _entries;
    private int _capacity;
    private int _count;
    private bool _isDisposed;
    private bool _isEnumerating;

    public BaseMulticast(int capacity = CAPACITY_DEFAULT)
    {
      _capacity = HashHelper.GenSize(capacity);
      _buckets = new int[_capacity];
      _entries = new Entry[_capacity];
    }

    ~BaseMulticast() => Dispose(isDisposing: false);
    public void Dispose() => Dispose(isDisposing: true);

    /// <summary>
    /// This is an O(1) operation (or O(n) when resizing).
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="ArgumentException" />
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ObjectDisposedException" />
    public void Subscribe(T invocation)
    {
      if (_isDisposed)
        throw new ObjectDisposedException(GetType().FullName);
      if (_isEnumerating)
        throw new InvalidOperationException(MODIFIED_ERR_MESSAGE);
      if (invocation == null)
        throw new ArgumentNullException(nameof(invocation));

      if (_count >= _capacity)
        EnsureCapacity();

      var hash = invocation.ComputeHash();
      ref var bucketRef = ref GetBucketRef(hash);

      var collisionCount = 0;
      var bucketValue = bucketRef - 1;
      var currIndex = bucketValue;
      while (currIndex >= 0)
      {
        ref var entryRef = ref _entries[currIndex];
        if (entryRef.Invocation == invocation)
          throw new ArgumentException(ALREADY_ERR_MESSAGE);

        currIndex = entryRef.PrevIndex;
        ++collisionCount;
        if (collisionCount > _capacity)
          throw new InvalidOperationException(EXCEEDED_ERR_MESSAGE);
      }

      _entries[_count].Set(invocation, hash, prevIndex: bucketValue);
      bucketRef = ++_count;
    }

    /// <summary>
    /// This is an O(1) operation.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ObjectDisposedException" />
    public void Unsubscribe(T invocation)
    {
      if (_isDisposed)
        throw new ObjectDisposedException(GetType().FullName);
      if (_isEnumerating)
        throw new InvalidOperationException(MODIFIED_ERR_MESSAGE);
      if (invocation == null)
        throw new ArgumentNullException(nameof(invocation));

      var hash = invocation.ComputeHash();
      ref var bucketRef = ref GetBucketRef(hash);

      var prevIndex = -1;
      var collisionCount = 0;
      var currIndex = bucketRef - 1;
      while (currIndex >= 0)
      {
        ref var entryRef = ref _entries[currIndex];
        if (entryRef.Invocation == invocation)
        {
          if (prevIndex < 0)
            bucketRef = entryRef.PrevIndex + 1;
          else
            _entries[prevIndex].PrevIndex = entryRef.PrevIndex;

          --_count;
          if (currIndex == _count)
          {
            entryRef.Set(null);
          }
          else
          {
            ref var lastEntryRef = ref _entries[_count];
            GetBucketRef(lastEntryRef.Hash) = currIndex;
            entryRef = lastEntryRef;
            lastEntryRef.Set(null);
          }

          return;
        }

        prevIndex = currIndex;
        currIndex = entryRef.PrevIndex;
        ++collisionCount;
        if (collisionCount > _capacity)
          throw new InvalidOperationException(EXCEEDED_ERR_MESSAGE);
      }
    }

    /// <summary>
    /// Copies the invocations of the <see cref="BaseMulticast{T}"/> to a new array.
    /// </summary>
    /// <exception cref="ObjectDisposedException" />
    public T[] ToArray()
    {
      if (_isDisposed)
        throw new ObjectDisposedException(GetType().FullName);

      var array = new T[_count];
      for (var i = 0; i < _count; i++)
        array[i] = _entries[i].Invocation;

      return array;
    }

    protected virtual void Dispose(bool isDisposing)
    {
      if (_isDisposed)
        return;

      if (_count > 0)
        Array.Clear(_entries, 0, _count);

      _count = 0;
      _entries = null;
      _buckets = null;

      if (isDisposing)
        GC.SuppressFinalize(this);

      _isDisposed = true;
    }

    [MethodImpl(AggressiveInlining)]
    protected Enumerator GetEnumerator() => new Enumerator(this);

    [MethodImpl(AggressiveInlining)]
    private ref int GetBucketRef(int hash) => ref _buckets[(uint)hash % (uint)_capacity];

    private void EnsureCapacity()
    {
      var source = _entries;

      _capacity = HashHelper.GenSize(_capacity * HashHelper.PRIME_MUL);
      _buckets = new int[_capacity];
      _entries = new Entry[_capacity];

      for (var i = 0; i < _count; i++)
      {
        ref var entryRef = ref _entries[i];
        entryRef = source[i];

        ref var bucketRef = ref GetBucketRef(entryRef.Hash);
        entryRef.PrevIndex = bucketRef - 1;
        bucketRef = i + 1;
      }
    }
  }
}