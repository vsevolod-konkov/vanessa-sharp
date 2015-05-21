using System.Runtime.InteropServices;

namespace VanessaSharp.Proxy.Common.Interop
{
    /// <summary>
    /// COM-интерфейс 1С для COM-соединения.
    /// </summary>
    [Guid("BA4E52BD-DCB2-4BF7-BB29-84C1CA456A8F")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [ComImport]
    internal interface IV8ComConnector
    {
        [DispId(0xB)]
        [return: MarshalAs(UnmanagedType.IDispatch)] object Connect([In, MarshalAs(UnmanagedType.BStr)] string connectString);
    }
}
