using System;
using System.Runtime.InteropServices;

namespace MSEnchant.Helper;

public static class MemoryHelper
{
    public static byte[] StructureToByteArray(object obj)
    {
        var length = Marshal.SizeOf(obj);

        var array = new byte[length];

        var pointer = Marshal.AllocHGlobal(length);

        Marshal.StructureToPtr(obj, pointer, true);
        Marshal.Copy(pointer, array, 0, length);
        Marshal.FreeHGlobal(pointer);

        return array;
    }
    
    public static object? ByteArrayToStructure(byte[] bytes, Type type)
    {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
        }
        finally
        {
            handle.Free();
        }
    }
    
    public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free();
        }
    }
}