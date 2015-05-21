using System.Runtime.InteropServices;

namespace VanessaSharp.Proxy.Common.Interop
{
    /// <summary>
    /// COM-интерфейс 1С для COM-соединения версии 2.
    /// </summary>
    [Guid("687CB41E-3FBC-4096-9BAA-9065F2546D8F")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [ComImport]
    internal interface IV8ComConnector2 : IV8ComConnector
    {
        [DispId(0xB)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        new object Connect([In, MarshalAs(UnmanagedType.BStr)] string connectString);
        
        [DispId(1)]
        uint PoolCapacity
        {
            [return: MarshalAs(UnmanagedType.U4)]
            get;

            [param: MarshalAs(UnmanagedType.U4)]
            set;
        }

        [DispId(2)]
        uint PoolTimeout
        {
            [return: MarshalAs(UnmanagedType.U4)]
            get;

            [param: MarshalAs(UnmanagedType.U4)]
            set;
        }
    }
}