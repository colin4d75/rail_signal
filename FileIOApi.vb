Option Strict On
Option Explicit On

Imports Microsoft.Win32.SafeHandles
Imports System.Runtime.InteropServices

Namespace WinUsbDemo

    ''' <summary>
    ''' API declarations relating to file I/O.
    ''' </summary>

    Friend NotInheritable Class FileIO

		Friend Const FILE_ATTRIBUTE_NORMAL As Int32 = &H80
        Friend Const FILE_FLAG_OVERLAPPED As Int32 = &H40000000
		Friend Const FILE_SHARE_READ As Int32 = 1
		Friend Const FILE_SHARE_WRITE As Int32 = 2
		Friend Const GENERIC_READ As UInt32 = &H80000000UL
        Friend Const GENERIC_WRITE As UInt32 = &H40000000
        Friend Const INVALID_HANDLE_VALUE As Int32 = -1
		Friend Const OPEN_EXISTING As Int32 = 3

		<DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> Shared Function CreateFile _
		 (ByVal lpFileName As String, _
		 ByVal dwDesiredAccess As UInt32, _
		 ByVal dwShareMode As Int32, _
		 ByVal lpSecurityAttributes As IntPtr, _
		 ByVal dwCreationDisposition As Int32, _
		 ByVal dwFlagsAndAttributes As Int32, _
		 ByVal hTemplateFile As Int32) _
		 As SafeFileHandle
		End Function

    End Class

End Namespace
