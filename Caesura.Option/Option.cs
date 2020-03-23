
namespace Caesura.Option
{
    using System;
    using System.Threading.Tasks;
    
    public readonly struct Option<T> : IDisposable, IAsyncDisposable
    {
        private readonly T value;
        private readonly bool hasValue;
        
        public readonly T Value => GetValue();
        public readonly bool HasValue => hasValue;
        public readonly bool NoValue => !hasValue;
        
        internal Option(T item)
        {
            value = item;
            if (item is null)
            {
                this.hasValue = false;
            }
            else if ((!typeof(T).IsValueType) && item!.Equals(default(T)!))
            {
                this.hasValue = false;
            }
            else
            {
                this.hasValue = true;
            }
        }
        
        internal Option(Unit unit)
        {
            value    = default!;
            hasValue = false;
        }
        
        internal Option(T item, bool has_value)
        {
            value    = item;
            hasValue = has_value;
        }
        
        public Option<T> Match(Func<T, T> some, Func<T> none)
        {
            return hasValue
                ? new Option<T>(some(value), true)
                : new Option<T>(none(), true);
        }
        
        public Option<T> Match(Func<T, T> some, Action none)
        {
            if (hasValue)
            {
                return new Option<T>(some(value), true);
            }
            else
            {
                none();
                return this;
            }
        }
        
        public Option<T> Match(Func<T, T> some)
        {
            return hasValue
                ? new Option<T>(some(value), true)
                : throw new NoneOptionException("Match function with unhandled none case.");
        }
        
        public Option<T> Match(Action<T> some, Action none)
        {
            if (hasValue)
            {
                some(value);
            }
            else
            {
                none();
            }
            return this;
        }
        
        public Option<T> Match(Action none)
        {
            if (!hasValue)
            {
                none();
            }
            return this;
        }
        
        public Option<T> Match(Action<T> some)
        {
            if (hasValue)
            {
                some(value);
            }
            else
            {
                throw new NoneOptionException("Match function with unhandled none case.");
            }
            return this;
        }
        
        public async Task<Option<T>> Match(Func<T, Task<T>> some, Func<Task> none, bool configure_await)
        {
            if (hasValue)
            {
                return new Option<T>(await some(value).ConfigureAwait(configure_await), true);
            }
            else
            {
                await none().ConfigureAwait(configure_await);
                return this;
            }
        }
        
        public async Task<Option<T>> Match(Func<Task> none, bool configure_await)
        {
            if (!hasValue)
            {
                await none().ConfigureAwait(configure_await);
            }
            return this;
        }
        
        public Option<TOut> Select<TOut>(Func<T, TOut> map)
        {
            return hasValue
                ? new Option<TOut>(map(value), true)
                : new Option<TOut>(default!, false);
        }
        
        public async Task<Option<TOut>> Select<TOut>(Func<T, Task<TOut>> map, bool configure_await)
        {
            return hasValue
                ? new Option<TOut>(await map(value).ConfigureAwait(configure_await), true)
                : new Option<TOut>(default!, false);
        }
        
        public Option<TOut> Bind<TOut>(Func<T, Option<TOut>> bind)
        {
            return hasValue
                ? bind(value)
                : new Option<TOut>(default!, false);
        }
        
        public async Task<Option<TOut>> Bind<TOut>(Func<T, Task<Option<TOut>>> bind, bool configure_await)
        {
            return hasValue
                ? await bind(value).ConfigureAwait(configure_await)
                : new Option<TOut>(default!, false);
        }
        
        public T GetValue()
        {
            return hasValue
                ? value
                : throw new NoneOptionException("Attempted to retrieve value from Option with no value.");
        }
        
        public void Deconstruct(out T item, out bool has_value)
        {
            item      = value;
            has_value = hasValue;
        }
        
        public static implicit operator Option<T>(T item)
        {
            return new Option<T>(item);
        }
        
        public static implicit operator Option<T>(Unit unit)
        {
            return new Option<T>(unit);
        }
        
        public static bool operator true (Option<T> option)
        {
            return option.hasValue;
        }
        
        public static bool operator false (Option<T> option)
        {
            return !option.hasValue;
        }
        
        public static bool operator == (Option<T> option1, Option<T> option2)
        {
            if (option1.hasValue && option2.hasValue)
            {
                return option1.value!.Equals(option2.value);
            }
            else if ((!option1.hasValue) && (!option2.hasValue))
            {
                return true;
            }
            
            return false;
        }
        
        public static bool operator != (Option<T> option1, Option<T> option2)
        {
            return !(option1 == option2);
        }
        
        public override bool Equals(object? obj)
        {
            if (obj is Option<T> option)
            {
                return this == option;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            if (hasValue)
            {
                return value!.GetHashCode();
            }
            return -1;
        }
        
        public override string ToString()
        {
            if (hasValue)
            {
                return $"Option Some: {value!.ToString()}";
            }
            return $"Option {Option.Unit.ToString()}";
        }
        
        public void Dispose()
        {
            if (value is IDisposable vid)
            {
                vid.Dispose();
            }
        }
        
        public async ValueTask DisposeAsync()
        {
            if (value is IAsyncDisposable viad)
            {
                await viad.DisposeAsync();
            }
        }
    }
    
    public static class Option 
    {
        private readonly static Unit _unit = new Unit();
        public static Unit Unit => _unit;
        
        public static Option<T> Some<T>(T item)
        {
            return new Option<T>(item, true);
        }
        
        public static Option<T> None<T>()
        {
            return new Option<T>(Unit);
        }
    }
}
