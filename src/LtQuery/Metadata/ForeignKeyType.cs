namespace LtQuery.Metadata;

public enum ForeignKeyType
{
    /// <summary>
    /// 1 to Unique 0..1(Child)
    /// </summary>
    OwnWithOne,

    /// <summary>
    /// 1 to Unique * (Children)
    /// </summary>
    OwnWithMany,

    /// <summary>
    /// 1 to 0..1
    /// </summary>
    ReferenceWithOne,

    /// <summary>
    /// 1 to *
    /// </summary>
    ReferenceWithMany,
}
