using System.Runtime.CompilerServices;
using System;

public static class ObjectIdentifier
{
    private static readonly ConditionalWeakTable<object, RefId> _ids = new();

    public static Guid GetRefId<T>(this T obj) where T: class
    {
        if (obj == null)
            return default;

        return _ids.GetOrCreateValue(obj).Id;
    }


    private class RefId
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
}