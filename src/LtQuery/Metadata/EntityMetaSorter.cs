namespace LtQuery.Metadata
{
    /// <summary>
    /// Topological Sort
    /// Kahn's Method
    /// </summary>
    class EntityMetaSorter
    {
        public IReadOnlyList<EntityMeta> Sort(IReadOnlyList<EntityMeta> source)
        {
            var source2 = source.ToList();
            var list = new List<EntityMeta>();
            while (source2.Count != 0)
            {
                var list2 = new List<EntityMeta>();
                foreach (var item in source2)
                {
                    var isDependent = false;
                    foreach (var property in item.Properties)
                    {
                        var foreignKey = property as ForeignKeyMeta;
                        if (foreignKey == null)
                            continue;
                        var type = foreignKey.Navigation.Type;
                        if (source2.Select(_ => _.Type).Contains(type))
                            isDependent = true;
                    }
                    if (!isDependent)
                        list2.Add(item);
                }
                list.AddRange(list2);
                foreach (var item in list2)
                    source2.Remove(item);
            }
            return list;
        }
    }
}
