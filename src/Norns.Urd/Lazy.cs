using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Norns.Urd
{
    internal enum LazyState
    {
        NoneViaFactory = 0,
        NoneException = 1,

        PublicationOnlyViaFactory = 2,
        PublicationOnlyWait = 3,
        PublicationOnlyException = 4,

        ExecutionAndPublicationViaFactory = 5,
        ExecutionAndPublicationException = 6,
    }

    internal class LazyHelper
    {
        internal static readonly LazyHelper NoneViaFactory = new LazyHelper(LazyState.NoneViaFactory);
        internal static readonly LazyHelper PublicationOnlyViaFactory = new LazyHelper(LazyState.PublicationOnlyViaFactory);
        internal static readonly LazyHelper PublicationOnlyWaitForOtherThreadToPublish = new LazyHelper(LazyState.PublicationOnlyWait);

        internal LazyState State { get; }

        private readonly ExceptionDispatchInfo _exceptionDispatch;

        internal LazyHelper(LazyState state)
        {
            State = state;
        }

        internal LazyHelper(LazyThreadSafetyMode mode, Exception exception)
        {
            switch (mode)
            {
                case LazyThreadSafetyMode.ExecutionAndPublication:
                    State = LazyState.ExecutionAndPublicationException;
                    break;

                case LazyThreadSafetyMode.None:
                    State = LazyState.NoneException;
                    break;

                case LazyThreadSafetyMode.PublicationOnly:
                    State = LazyState.PublicationOnlyException;
                    break;

                default:
                    break;
            }

            _exceptionDispatch = ExceptionDispatchInfo.Capture(exception);
        }

        internal void ThrowException()
        {
            _exceptionDispatch.Throw();
        }

        internal static LazyHelper Create(bool isThreadSafe)
        {
            return isThreadSafe
                // we need to create an object for ExecutionAndPublication because we use Monitor-based locking
                ? new LazyHelper(LazyState.ExecutionAndPublicationViaFactory)
                : NoneViaFactory;
        }
    }

    public class Lazy<T, R>
    {
        // _state, a volatile reference, is set to null after _value has been set
        private volatile LazyHelper _state;

        private Func<R, T> _factory;

        // _value eventually stores the lazily created value. It is valid when _state = null.
        private T _value;

        public Lazy(Func<R, T> valueFactory, bool isThreadSafe = false)
        {
            _factory = valueFactory;
            _state = LazyHelper.Create(isThreadSafe);
        }

        private void ViaFactory(LazyThreadSafetyMode mode, R r)
        {
            try
            {
                Func<R, T> factory = _factory;
                if (factory == null)
                    throw new InvalidOperationException("Lazy_Value_RecursiveCallsToValue");
                _factory = null;

                _value = factory(r);
                _state = null; // volatile write, must occur after setting _value
            }
            catch (Exception exception)
            {
                _state = new LazyHelper(mode, exception);
                throw;
            }
        }

        private void ExecutionAndPublication(LazyHelper executionAndPublication, R r)
        {
            lock (executionAndPublication)
            {
                // it's possible for multiple calls to have piled up behind the lock, so we need to check
                // to see if the ExecutionAndPublication object is still the current implementation.
                if (ReferenceEquals(_state, executionAndPublication))
                {
                    ViaFactory(LazyThreadSafetyMode.ExecutionAndPublication, r);
                }
            }
        }

        private void PublicationOnly(LazyHelper publicationOnly, T possibleValue)
        {
            LazyHelper previous = Interlocked.CompareExchange(ref _state, LazyHelper.PublicationOnlyWaitForOtherThreadToPublish, publicationOnly);
            if (previous == publicationOnly)
            {
                _factory = null;
                _value = possibleValue;
                _state = null; // volatile write, must occur after setting _value
            }
        }

        private void PublicationOnlyViaFactory(LazyHelper initializer, R r)
        {
            Func<R, T> factory = _factory;
            if (factory == null)
            {
                PublicationOnlyWaitForOtherThreadToPublish();
            }
            else
            {
                PublicationOnly(initializer, factory(r));
            }
        }

        private void PublicationOnlyWaitForOtherThreadToPublish()
        {
            SpinWait spinWait = default;
            while (_state != null)
            {
                // We get here when PublicationOnly temporarily sets _state to LazyHelper.PublicationOnlyWaitForOtherThreadToPublish.
                // This temporary state should be quickly followed by _state being set to null.
                spinWait.SpinOnce();
            }
        }

        private T CreateValue(R r)
        {
            // we have to create a copy of state here, and use the copy exclusively from here on in
            // so as to ensure thread safety.
            LazyHelper state = _state;
            if (state != null)
            {
                switch (state.State)
                {
                    case LazyState.NoneViaFactory:
                        ViaFactory(LazyThreadSafetyMode.None, r);
                        break;

                    case LazyState.PublicationOnlyViaFactory:
                        PublicationOnlyViaFactory(state, r);
                        break;

                    case LazyState.PublicationOnlyWait:
                        PublicationOnlyWaitForOtherThreadToPublish();
                        break;

                    case LazyState.ExecutionAndPublicationViaFactory:
                        ExecutionAndPublication(state, r);
                        break;

                    default:
                        state.ThrowException();
                        break;
                }
            }
            return GetValue(r);
        }

        public override string ToString()
        {
            return IsValueCreated ?
                _value.ToString() : // Throws NullReferenceException as if caller called ToString on the value itself
                "Lazy_ToString_ValueNotCreated";
        }

        public bool IsValueCreated => _state == null;

        public T GetValue(R r) => _state == null ? _value : CreateValue(r);
    }
}