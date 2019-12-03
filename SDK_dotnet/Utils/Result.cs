namespace SDK_dotnet.Utils
{
    public sealed class Result<T>
    {
        public System.Guid ID { get; } = System.Guid.NewGuid();
        public bool IsSuccess { get; set; }
        public bool IsFailure
        {
            get { return !IsSuccess; }
        }
        public T Data { get; set; }
    }
}
