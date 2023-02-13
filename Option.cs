public interface IOption<T>
{
    TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone);
}

class Some<T> : IOption<T>
{
    private T _data;

    private Some(T data)
    {
        _data = data;
    }

    public static IOption<T> Of(T data) => new Some<T>(data);

    public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> _) =>
        onSome(_data);
}

class None<T> : IOption<T>
{
    public TResult Match<TResult>(Func<T, TResult> _, Func<TResult> onNone) =>
        onNone();
}