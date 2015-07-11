Option Strict On
Option Explicit On
Imports Microsoft.Win32.SafeHandles
Imports System.Runtime.InteropServices

Namespace WinUsbDemo

	Partial Friend NotInheritable Class WinUsbDevice

		'''<remarks>
		''' These declarations are translated from the C declarations in various files
		''' in the Windows DDK. The files are:
		''' 
		''' winddk\6001\inc\api\usb.h
		''' winddk\6001\inc\api\usb100.h
		''' winddk\6001\inc\api\winusbio.h
		''' 
		''' (your home directory and release number may vary)
		'''</remarks>

		Friend Const DEVICE_SPEED As UInt32 = 1
		Friend Const USB_ENDPOINT_DIRECTION_MASK As Byte = &H80

		Friend Enum POLICY_TYPE As UInt32
			SHORT_PACKET_TERMINATE = 1
			AUTO_CLEAR_STALL
			PIPE_TRANSFER_TIMEOUT
			IGNORE_SHORT_PACKETS
			ALLOW_PARTIAL_READS
			AUTO_FLUSH
			RAW_IO
		End Enum

		Friend Enum USBD_PIPE_TYPE
			UsbdPipeTypeControl
			UsbdPipeTypeIsochronous
			UsbdPipeTypeBulk
			UsbdPipeTypeInterrupt
		End Enum

		Friend Enum USB_DEVICE_SPEED
			UsbLowSpeed = 1
			UsbFullSpeed
			UsbHighSpeed
		End Enum

		Friend Structure USB_CONFIGURATION_DESCRIPTOR
			Friend bLength As Byte
			Friend bDescriptorType As Byte
			Friend wTotalLength As UShort
			Friend bNumInterfaces As Byte
			Friend bConfigurationValue As Byte
			Friend iConfiguration As Byte
			Friend bmAttributes As Byte
			Friend MaxPower As Byte
		End Structure

		Friend Structure USB_INTERFACE_DESCRIPTOR
			Friend bLength As Byte
			Friend bDescriptorType As Byte
			Friend bInterfaceNumber As Byte
			Friend bAlternateSetting As Byte
			Friend bNumEndpoints As Byte
			Friend bInterfaceClass As Byte
			Friend bInterfaceSubClass As Byte
			Friend bInterfaceProtocol As Byte
			Friend iInterface As Byte
		End Structure

		Friend Structure WINUSB_PIPE_INFORMATION
			Friend PipeType As USBD_PIPE_TYPE
			Friend PipeId As Byte
			Friend MaximumPacketSize As UShort
			Friend Interval As Byte
		End Structure

		<StructLayout(LayoutKind.Sequential, Pack:=1)> _
		Friend Structure WINUSB_SETUP_PACKET
			Friend RequestType As Byte
			Friend Request As Byte
			Friend Value As UShort
			Friend Index As UShort
			Friend Length As UShort
		End Structure

		<DllImport("winusb.dll", SetLastError:=True)> _
		Friend Shared Function WinUsb_ControlTransfer _
		 (ByVal InterfaceHandle As IntPtr, _
		 ByVal SetupPacket As WINUSB_SETUP_PACKET, _
		 ByVal buffer() As Byte, _
		 ByVal BufferLength As UInt32, _
		 ByRef LengthTransferred As UInt32, _
		 ByVal Overlapped As IntPtr) _
		 As Boolean
		End Function

		<DllImport("winusb.dll", SetLastError:=True)> Friend Shared Function WinUsb_Free _
		 (ByVal InterfaceHandle As IntPtr) _
		As Boolean
		End Function

		<DllImport("winusb.dll", SetLastError:=True)> Friend Shared Function WinUsb_Initialize _
		 (ByVal DeviceHandle As SafeFileHandle, _
		 ByRef InterfaceHandle As IntPtr) _
		 As Boolean
		End Function

		' winusb.h
		' In the C declaration, Buffer is a pvoid, with no equivalent in Visual Basic.
		' Use this declaration to retrieve DEVICE_SPEED (the only currently defined InformationType).

		<DllImport("winusb.dll", SetLastError:=True)> Friend Shared Function WinUsb_QueryDeviceInformation _
		 (ByVal InterfaceHandle As IntPtr, _
		 ByVal InformationType As UInt32, _
		 ByRef BufferLength As UInt32, _
		 ByRef Buffer As Byte) _
		 As Boolean
		End Function

		<DllImport("winusb.dll", SetLastError:=True)> Friend Shared Function WinUsb_QueryInterfaceSettings _
		 (ByVal InterfaceHandle As IntPtr, _
		 ByVal AlternateInterfaceNumber As Byte, _
		 ByRef UsbAltInterfaceDescriptor As USB_INTERFACE_DESCRIPTOR) _
		 As Boolean
		End Function

		<DllImport("winusb.dll", SetLastError:=True)> Friend Shared Function WinUsb_QueryPipe _
		  (ByVal InterfaceHandle As IntPtr, _
		  ByVal AlternateInterfaceNumber As Byte, _
		 ByVal PipeIndex As Byte, _
		 ByRef PipeInformation As WINUSB_PIPE_INFORMATION) _
		 As Boolean
		End Function

		<DllImport("winusb.dll", SetLastError:=True)> Friend Shared Function WinUsb_ReadPipe _
		(ByVal InterfaceHandle As IntPtr, _
		 ByVal PipeID As Byte, _
		 ByVal Buffer() As Byte, _
		 ByVal BufferLength As UInt32, _
		 ByRef LengthTransferred As UInt32, _
		 ByVal Overlapped As IntPtr) _
		 As Boolean
		End Function

		' Two declarations for WinUsb_SetPipePolicy. 
		' Use this one when the returned Value is a byte (all except PIPE_TRANSFER_TIMEOUT):

		<DllImport("winusb.dll", SetLastError:=True)> Friend Shared Function WinUsb_SetPipePolicy _
		 (ByVal InterfaceHandle As IntPtr, _
		 ByVal PipeID As Byte, _
		 ByVal PolicyType As UInt32, _
		 ByVal ValueLength As UInt32, _
		 ByRef Value As Byte) _
		 As Boolean
		End Function

		' Use this alias when the returned Value is a UInt32 (PIPE_TRANSFER_TIMEOUT only):

		<DllImport("winusb.dll", EntryPoint:="WinUsb_SetPipePolicy", SetLastError:=True)> Friend Shared Function WinUsb_SetPipePolicy1 _
		 (ByVal InterfaceHandle As IntPtr, _
		 ByVal PipeID As Byte, _
		 ByVal PolicyType As UInt32, _
		 ByVal ValueLength As UInt32, _
		 ByRef Value As UInt32) _
		 As Boolean
		End Function

		<DllImport("winusb.dll", SetLastError:=True)> Friend Shared Function WinUsb_WritePipe _
		 (ByVal InterfaceHandle As IntPtr, _
		 ByVal PipeID As Byte, _
		 ByVal Buffer() As Byte, _
		 ByVal BufferLength As UInt32, _
		 ByRef LengthTransferred As UInt32, _
		 ByVal Overlapped As IntPtr) _
		 As Boolean
		End Function

	End Class

End Namespace
