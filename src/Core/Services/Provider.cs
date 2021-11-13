namespace CraigStars
{
    /// <summary>
    /// Default implementation of a Provider to return an item each time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Provider<T> : IProvider<T>
    {
        private readonly T item;

        public Provider(T item)
        {
            this.item = item;
        }

        public T Item => item;
    }
}