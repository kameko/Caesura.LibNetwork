
namespace Caesura.Option
{
    using System;
    using System.Threading.Tasks;
    
    public readonly struct Result<TOk, TError>: IDisposable, IAsyncDisposable
    {
        private readonly TOk ok;
        private readonly TError error;
        private readonly bool isError;
        
        public readonly TOk Ok => GetOk();
        public readonly TError Error => GetError();
        public readonly bool IsOk => !isError;
        public readonly bool IsError => isError;
        
        internal Result(TOk ok_value, TError error_value, bool is_error)
        {
            ok      = ok_value;
            error   = error_value;
            isError = is_error;
            
            AssertBothTypesAreNotSame();
        }
        
        internal Result(TOk ok_value)
        {
            ok      = ok_value;
            error   = default!;
            isError = false;
            
            AssertBothTypesAreNotSame();
        }
        
        internal Result(TError error_value)
        {
            ok      = default!;
            error   = error_value;
            isError = true;
            
            AssertBothTypesAreNotSame();
        }
        
        private void AssertBothTypesAreNotSame()
        {
            /*
            // Not sure about this one.
            if (typeof(TOk) == typeof(TError))
            {
                throw new ResultGenericArgumentsMatchException("TOk and TError generic arguments cannot be the same type. Use an Option<T> instead.");
            }
            */
        }
        
        public Result<TOk, TError> Handle(Action<TOk> on_ok, Action<TError> on_error)
        {
            if (isError)
            {
                on_error(error);
            }
            else
            {
                on_ok(ok);
            }
            return this;
        }
        
        public async Task<Result<TOk, TError>> Handle(Func<TOk, Task> on_ok, Func<TError, Task> on_error, bool configure_await)
        {
            if (isError)
            {
                await on_error(error).ConfigureAwait(configure_await);
            }
            else
            {
                await on_ok(ok).ConfigureAwait(configure_await);
            }
            return this;
        }
        
        public Result<TOk, TError> Handle(Action<TError> on_error)
        {
            if (isError)
            {
                on_error(error);
            }
            return this;
        }
        
        public async Task<Result<TOk, TError>> Handle(Func<TError, Task> on_error, bool configure_await)
        {
            if (isError)
            {
                await on_error(error).ConfigureAwait(configure_await);
            }
            return this;
        }
        
        public Result<TOk, TError> Handle(Action<TOk> on_ok)
        {
            if (isError)
            {
                throw new UnhandledResultErrorException();
            }
            else
            {
                on_ok(ok);
            }
            return this;
        }
        
        public async Task<Result<TOk, TError>> Handle(Func<TOk, Task> on_ok, bool configure_await)
        {
            if (isError)
            {
                throw new UnhandledResultErrorException();
            }
            else
            {
                await on_ok(ok).ConfigureAwait(configure_await);
            }
            return this;
        }
        
        public TOk GetOk()
        {
            if (isError)
            {
                throw new ResultNotOkException();
            }
            return ok;
        }
        
        public TError GetError()
        {
            if (!isError)
            {
                throw new ResultNotErrorException();
            }
            return error;    
        }
        
        public Option<TOk> ToOption()
        {
            if (isError)
            {
                return Option.Unit;
            }
            else
            {
                return Option.Some(ok);
            }
        }
        
        public Option<TError> ErrorToOption()
        {
            if (isError)
            {
                return Option.Some(error);
            }
            else
            {
                return Option.Unit;
            }
        }
        
        public void Deconstruct(out TOk ok_value, out TError error_value)
        {
            ok_value    = ok;
            error_value = error;
        }
        
        public void Deconstruct(out TOk ok_value, out TError error_value, out bool is_ok)
        {
            ok_value    = ok;
            error_value = error;
            is_ok       = !isError;
        }
        
        public static implicit operator Result<TOk, TError>(TOk ok_value)
        {
            return new Result<TOk, TError>(ok_value);
        }
        
        public static implicit operator Result<TOk, TError>(TError error_value)
        {
            return new Result<TOk, TError>(error_value);
        }
        
        public static implicit operator Option<TOk>(Result<TOk, TError> result)
        {
            return result.ToOption();
        }
        
        public static bool operator true (Result<TOk, TError> result)
        {
            return !result.isError;
        }
        
        public static bool operator false (Result<TOk, TError> result)
        {
            return result.isError;
        }
        
        public static bool operator == (Result<TOk, TError> result1, Result<TOk, TError> result2)
        {
            if (result1.isError && result2.isError)
            {
                return result1.error!.Equals(result2.error);
            }
            else if ((!result1.isError) && (!result2.isError))
            {
                return result1.ok!.Equals(result2.ok);
            }
            
            return false;
        }
        
        public static bool operator != (Result<TOk, TError> result1, Result<TOk, TError> result2)
        {
            return !(result1 == result2);
        }
        
        public override bool Equals(object? obj)
        {
            if (obj is Result<TOk, TError> result)
            {
                return this == result;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            if (isError)
            {
                return error!.GetHashCode();
            }
            else
            {
                return ok!.GetHashCode();
            }
        }
        
        public override string ToString()
        {
            if (isError)
            {
                return $"Result Error: {error!.ToString()}";
            }
            else
            {
                return $"Result Ok: {ok!.ToString()}";
            }
        }
        
        public void Dispose()
        {
            if (ok is IDisposable okid)
            {
                okid.Dispose();
            }
            if (error is IDisposable errid)
            {
                errid.Dispose();
            }
        }
        
        public async ValueTask DisposeAsync()
        {
            if (ok is IAsyncDisposable okiad)
            {
                await okiad.DisposeAsync();
            }
            if (error is IAsyncDisposable erriad)
            {
                await erriad.DisposeAsync();
            }
        }
    }
    
    public static class Result
    {
        public static Result<TOk, TError> Ok<TOk, TError>(TOk ok_value)
        {
            return new Result<TOk, TError>(ok_value);
        }
        
        public static Result<TOk, TError> Error<TOk, TError>(TError error_value)
        {
            return new Result<TOk, TError>(error_value);
        }
        
        public static Result<TOk, Exception> Ok<TOk>(TOk ok_value)
        {
            return new Result<TOk, Exception>(ok_value);
        }
        
        public static Result<TOk, Exception> Error<TOk>(Exception error_value)
        {
            return new Result<TOk, Exception>(error_value);
        }
    }
}
