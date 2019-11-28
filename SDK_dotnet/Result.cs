namespace GluwProSDK
{
    public sealed class Result<T, E> where E : IError
    {
        public System.Guid ID { get; } = System.Guid.NewGuid();
        public bool IsSuccess { get; set; }
        public bool IsFailure
        {
            get { return !IsSuccess; }
        }
        public T Data { get; set; }
        public E Error { get; set; }
    }

    public sealed class ResultWithNoError<T>
    {
        public System.Guid ID { get; } = System.Guid.NewGuid();
        public bool IsSuccess { get; set; }
        public bool IsFailure
        {
            get { return !IsSuccess; }
        }
        public T Data { get; set; }
    }

    public sealed class Result<E> where E : IError
    {
        public System.Guid ID { get; } = System.Guid.NewGuid();
        public bool IsSuccess { get; set; }
        public bool IsFailure
        {
            get { return !IsSuccess; }
        }
        public E Error { get; set; }
    }
}
