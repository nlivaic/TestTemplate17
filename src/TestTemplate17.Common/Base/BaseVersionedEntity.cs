namespace TestTemplate17.Common.Base;

public abstract class BaseVersionedEntity<T> : BaseEntity<T>, IVersionedEntity
{
    /// <summary>
    /// Gets the concurrency token used for version tracking.
    /// </summary>
    public byte[] RowVersion { get; private set; }
    public void SetRowVersion(byte[] rowVersion) => RowVersion = rowVersion;
}