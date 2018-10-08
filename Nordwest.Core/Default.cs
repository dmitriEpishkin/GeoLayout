namespace Nordwest {
    public static class Default<T> where T : new() {
        public static T Instance { get; } = new T();
    }
}
