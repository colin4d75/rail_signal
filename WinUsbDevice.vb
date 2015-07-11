Option Strict On
Option Explicit On

Imports Microsoft.Win32.SafeHandles
Imports System.Runtime.InteropServices
Imports System.Threading
Namespace WinUsbDemo


    '''<summary>
    ''' Routines for the WinUsb driver supported by Windows Vista and Windows XP.
    ''' </summary>
    ''' 
	Partial Friend NotInheritable Class WinUsbDevice


        Public m_parent As Form1

       
		Friend Structure devInfo
			Friend deviceHandle As SafeFileHandle
			Friend winUsbHandle As IntPtr
			Friend bulkInPipe As Byte
			Friend bulkOutPipe As Byte
			Friend interruptInPipe As Byte
			Friend interruptOutPipe As Byte
			Friend devicespeed As UInt32
        End Structure

        Delegate Sub setTrackCircuitCallbackDelegate(ByVal athread As String,
                               ByVal tcIndex As String,
                               ByVal setClear As Boolean,
                               ByVal realNotVirtual As Boolean)



		Friend myDevInfo As New devInfo

		''' <summary>
		''' Closes the device handle obtained with CreateFile and frees resources.
		''' </summary>
		''' 
		Friend Sub CloseDeviceHandle()

			Try
				WinUsb_Free(myDevInfo.winUsbHandle)

				If Not (myDevInfo.deviceHandle Is Nothing) Then
					If Not (myDevInfo.deviceHandle.IsInvalid) Then
						myDevInfo.deviceHandle.Close()
					End If
				End If

			Catch ex As System.AccessViolationException
				MsgBox("System.AccessViolationException")
			Catch ex As Exception
				Throw
			End Try

		End Sub


		''' <summary>
		''' Initiates a Control Read transfer. Data stage is device to host.
		''' </summary>
		'''
		''' <param name="dataStage"> The received data. </param>
		''' 
		''' <returns>
		''' True on success, False on failure.
		''' </returns>

		Friend Function Do_Control_Read_Transfer(ByRef dataStage As Byte()) As Boolean

			Dim bytesReturned As UInt32
			Dim setupPacket As WINUSB_SETUP_PACKET
			Dim success As Boolean

			Try
				' Vendor-specific request to an interface with device-to-host Data stage.

				setupPacket.RequestType = &HC1

				' The request number that identifies the specific request.

				setupPacket.Request = 2

				' Command-specific value to send to the device.

				setupPacket.Index = 0

				' Number of bytes in the request's Data stage.

				setupPacket.Length = Convert.ToUInt16(dataStage.Length)

				' Command-specific value to send to the device.

				setupPacket.Value = 0

				'***
				' winusb function 

				' summary
				' Initiates a control transfer.

				' paramaters
				' Device handle returned by WinUsb_Initialize.
				' WINUSB_SETUP_PACKET structure 
				' Buffer to hold the returned Data-stage data.
				' Number of data bytes to read in the Data stage.
				' Number of bytes read in the Data stage.
				' Null pointer for non-overlapped.

				' returns
				' True on success.
				' ***            

				success = WinUsb_ControlTransfer _
				 (myDevInfo.winUsbHandle, _
				 setupPacket, _
				 dataStage, _
				 Convert.ToUInt16(dataStage.Length), _
				 bytesReturned, _
				 IntPtr.Zero)

				Return success

			Catch ex As Exception
				Throw
			End Try
		End Function

		''' <summary>
		''' Initiates a Control Write transfer. Data stage is host to device.
		''' </summary>
		''' 
		''' <param name="dataStage"> The data to send. </param>
		''' 
		''' <returns>
		''' True on success, False on failure.
		''' </returns>

		Friend Function Do_Control_Write_Transfer(ByVal dataStage As Byte()) As Boolean

			Dim bytesReturned As UInt32
			Dim index As UShort = 0
			Dim setupPacket As WINUSB_SETUP_PACKET
			Dim success As Boolean
			Dim value As UShort = 0

			Try
				' Vendor-specific request to an interface with host-to-device Data stage.

				setupPacket.RequestType = &H41

				' The request number that identifies the specific request.

				setupPacket.Request = 1

				' Command-specific value to send to the device.

				setupPacket.Index = index

				' Number of bytes in the request's Data stage.

				setupPacket.Length = Convert.ToUInt16(dataStage.Length)

				' Command-specific value to send to the device.

				setupPacket.Value = value

				'***
				' winusb function 

				' summary
				' Initiates a control transfer.

				' parameters
				' Device handle returned by WinUsb_Initialize.
				' WINUSB_SETUP_PACKET structure 
				' Buffer containing the Data-stage data.
				' Number of data bytes to send in the Data stage.
				' Number of bytes sent in the Data stage.
				' Null pointer for non-overlapped.

				' Returns
				' True on success.
				' ***

				success = WinUsb_ControlTransfer _
				 (myDevInfo.winUsbHandle, _
				 setupPacket, _
				 dataStage, _
				 Convert.ToUInt16(dataStage.Length), _
				 bytesReturned, _
				 IntPtr.Zero)

				Return success

			Catch ex As Exception
				Throw
			End Try

		End Function

		''' <summary>
		''' Requests a handle with CreateFile.
		''' </summary>
		''' 
		''' <param name="devicePathName"> Returned by SetupDiGetDeviceInterfaceDetail 
		''' in an SP_DEVICE_INTERFACE_DETAIL_DATA structure. </param>
		''' 
		''' <returns>
		''' The handle.
		''' </returns>

		Friend Function GetDeviceHandle(ByVal devicePathName As String) As Boolean

			'***
			'API function

			' summary
			' Retrieves a handle to a device.

			' parameters 
			' Device path name returned by SetupDiGetDeviceInterfaceDetail
			' Type of access requested (read/write).
			' FILE_SHARE attributes to allow other processes to access the device while this handle is open.
			' Security structure or IntPtr.Zero.
			' Creation disposition value. Use OPEN_EXISTING for devices.
			' Flags and attributes for files. The winsub driver requires FILE_FLAG_OVERLAPPED.
			' Handle to a template file. Not used.

			' Returns
			' Handle
			'***

			myDevInfo.deviceHandle = FileIO.CreateFile _
			 (devicePathName, _
			 FileIO.GENERIC_WRITE Or FileIO.GENERIC_READ, _
			 FileIO.FILE_SHARE_READ Or FileIO.FILE_SHARE_WRITE, _
			 IntPtr.Zero, _
			 FileIO.OPEN_EXISTING, _
			 FileIO.FILE_ATTRIBUTE_NORMAL Or FileIO.FILE_FLAG_OVERLAPPED, _
			 0)

			If Not (myDevInfo.deviceHandle.IsInvalid) Then
				Return True
			Else
				Return False
			End If

		End Function

		''' <summary>
		''' Initializes a device interface and obtains information about it.
		''' Calls these winusb API functions:
		'''   WinUsb_Initialize
		'''   WinUsb_QueryInterfaceSettings
		'''   WinUsb_QueryPipe
		''' </summary>
		''' 
		''' <returns>
		''' True on success, False on failure.
		''' </returns>

		Friend Function InitializeDevice() As Boolean

			Dim ifaceDescriptor As USB_INTERFACE_DESCRIPTOR
			Dim pipeInfo As WINUSB_PIPE_INFORMATION
			Dim pipeTimeout As UInt32 = 2000
			Dim success As Boolean

			Try
				'***
				' winusb function 

				' summary
				' get a handle for communications with a winusb device        '

				' parameters
				' Handle returned by CreateFile.
				' Device handle to be returned.

				' returns
				' True on success.
				' ***

				success = WinUsb_Initialize _
				  (myDevInfo.deviceHandle, _
				  myDevInfo.winUsbHandle)

				If success Then

					'***
					' winusb function 

					' summary
					' Get a structure with information about the device interface.

					' parameters
					' handle returned by WinUsb_Initialize
					' alternate interface setting number
					' USB_INTERFACE_DESCRIPTOR structure to be returned.

					' returns
					' True on success.

					success = WinUsb_QueryInterfaceSettings _
						(myDevInfo.winUsbHandle, _
						0, _
						ifaceDescriptor)

					If success Then

						' Get the transfer type, endpoint number, and direction for the interface's
						' bulk and interrupt endpoints. Set pipe policies.

						'***
						' winusb function 

						' summary
						' returns information about a USB pipe (endpoint address)

						' parameters
						' Handle returned by WinUsb_Initialize
						' Alternate interface setting number
						' Number of an endpoint address associated with the interface. 
						' (The values count up from zero and are NOT the same as the endpoint address
						' in the endpoint descriptor.)
						' WINUSB_PIPE_INFORMATION structure to be returned

						' returns
						' True on success   
						'***

						For i As Int32 = 0 To ifaceDescriptor.bNumEndpoints - 1

							WinUsb_QueryPipe(myDevInfo.winUsbHandle, 0, Convert.ToByte(i), pipeInfo)

							If ((pipeInfo.PipeType = _
							 USBD_PIPE_TYPE.UsbdPipeTypeBulk) And _
							 UsbEndpointDirectionIn(pipeInfo.PipeId)) Then

								myDevInfo.bulkInPipe = pipeInfo.PipeId
                                Console.WriteLine("Bulk inpipe  " & i)

								SetPipePolicy _
								 (myDevInfo.bulkInPipe, _
								 POLICY_TYPE.IGNORE_SHORT_PACKETS, _
								 Convert.ToByte(False))

								SetPipePolicy _
								 (myDevInfo.bulkInPipe, _
								 POLICY_TYPE.PIPE_TRANSFER_TIMEOUT, _
								 pipeTimeout)

							ElseIf ((pipeInfo.PipeType = _
							 USBD_PIPE_TYPE.UsbdPipeTypeBulk) And _
							 UsbEndpointDirectionOut(pipeInfo.PipeId)) Then

								myDevInfo.bulkOutPipe = pipeInfo.PipeId
                                Console.WriteLine("Bulk outpipe  " & i)
								SetPipePolicy _
								 (myDevInfo.bulkOutPipe, _
								 POLICY_TYPE.IGNORE_SHORT_PACKETS, _
								 Convert.ToByte(False))

								SetPipePolicy _
								 (myDevInfo.bulkOutPipe, _
								 POLICY_TYPE.PIPE_TRANSFER_TIMEOUT, _
								 pipeTimeout)

							ElseIf (pipeInfo.PipeType = _
							 USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) And _
							 UsbEndpointDirectionIn(pipeInfo.PipeId) Then

								myDevInfo.interruptInPipe = pipeInfo.PipeId
                                Console.WriteLine("Interrupt Inpipe  " & i)

								SetPipePolicy _
								 (myDevInfo.interruptInPipe, _
								 POLICY_TYPE.IGNORE_SHORT_PACKETS, _
								 Convert.ToByte(False))

								SetPipePolicy _
								 (myDevInfo.interruptInPipe, _
								 POLICY_TYPE.PIPE_TRANSFER_TIMEOUT, _
								 pipeTimeout)

							ElseIf (pipeInfo.PipeType = _
							 USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) And _
							 UsbEndpointDirectionOut(pipeInfo.PipeId) Then

								myDevInfo.interruptOutPipe = pipeInfo.PipeId
                                Console.WriteLine("Interrupt outpipe  " & i)

								SetPipePolicy _
								 (myDevInfo.interruptOutPipe, _
								 POLICY_TYPE.IGNORE_SHORT_PACKETS, _
								 Convert.ToByte(False))

								SetPipePolicy _
								 (myDevInfo.interruptOutPipe, _
								 POLICY_TYPE.PIPE_TRANSFER_TIMEOUT, _
								 pipeTimeout)
							End If
						Next i
					Else
						success = False
					End If
				End If

				Return success

			Catch ex As Exception
				Throw
			End Try

		End Function

		''' <summary>
		''' Is the current operating system Windows XP or later?
		''' The WinUSB driver requires Windows XP or later.
		''' </summary>
		'''
		''' <returns>
		''' True if Windows XP or later, False if not.
		''' </returns>

		Friend Function IsWindowsXpOrLater() As Boolean

			Try
				Dim myEnvironment As OperatingSystem = Environment.OSVersion

				' Windows XP is version 5.1.

				Dim versionXP As New System.Version(5, 1)

				If (Version.op_GreaterThanOrEqual(myEnvironment.Version, versionXP) = True) Then
					Return True
				Else
					Return False
				End If

			Catch ex As Exception
				Throw
			End Try

		End Function

		''' <summary>
		''' Gets a value that corresponds to a USB_DEVICE_SPEED. 
		''' Unfortunatly, WinUsb_QueryDeviceInformation isn't reliable 
		''' and might return 1 (low speed) for a full-speed device.
		''' </summary>

		Friend Function QueryDeviceSpeed() As Boolean

			Dim speed(0) As Byte
			Dim success As Boolean

			'***
			' winusb function 

			' summary
			' Get the device speed. 
			' (Normally not required but can be nice to know.)

			' parameters
			' Handle returned by WinUsb_Initialize
			' Requested information type.
			' Number of bytes to read.
			' Information to be returned.

			' returns
			' True on success.
			'***

			success = WinUsb_QueryDeviceInformation _
			 (myDevInfo.winUsbHandle, _
			 DEVICE_SPEED, _
			 Convert.ToUInt32(speed.Length), _
			 speed(0))

			If success Then
				myDevInfo.devicespeed = speed(0)
			End If

			Return success

		End Function
		''' <summary>
		''' Attempts to read data from a bulk IN endpoint.
		''' </summary>
		''' 
		''' <param name="PipeID"> Endpoint address. </param>
		''' <param name="bytesToRead"> Number of bytes to read. </param>
		''' <param name="Buffer"> Buffer for storing the bytes read. </param>
		''' <param name="bytesRead"> Number of bytes read. </param>
		''' <param name="success"> Success or failure status. </param>
		''' 
		Friend Sub ReadViaBulkTransfer _
		 (ByVal pipeID As Byte, _
		 ByVal bytesToRead As UInt32, _
		 ByRef buffer() As Byte, _
		 ByRef bytesRead As UInt32, _
		 ByRef success As Boolean)

            Dim bytesToWrite As UInt32
            '  Dim bytesWritten As UInt32

            bytesToWrite = 2
            buffer(0) = 8
            buffer(1) = 2
            buffer(2) = 9
			Try
				'***
				' winusb function 

				' summary
				' Attempts to read data from a device interface.

				' parameters
				' Device handle returned by WinUsb_Initialize.
				' Endpoint address.
				' Buffer to store the data.
				' Maximum number of bytes to return.
				' Number of bytes read.
				' Null pointer for non-overlapped.

				' Returns
				' True on success.
				'***
                '      success = WinUsb_WritePipe _
                '(myDevInfo.winUsbHandle, _
                'pipeID, _
                'buffer, _
                'bytesToWrite, _
                'bytesWritten, _
                'IntPtr.Zero)



				success = WinUsb_ReadPipe _
				 (myDevInfo.winUsbHandle, _
				  pipeID, _
				  buffer, _
				  bytesToRead, _
				  bytesRead, _
				  IntPtr.Zero)

				If Not success Then

					CloseDeviceHandle()

				End If

			Catch ex As System.AccessViolationException
				MsgBox("System.AccessViolationException")
			Catch ex As Exception
				Throw
			End Try

		End Sub

		''' <summary>
		''' Attempts to read data from an interrupt IN endpoint. 
		''' </summary>
		''' 
		''' <param name="PipeID"> Endpoint address. </param>
		''' <param name="bytesToRead"> Number of bytes to read. </param>
		''' <param name="Buffer"> Buffer for storing the bytes read. </param>
		''' <param name="bytesRead"> Number of bytes read. </param>
		''' <param name="success"> Success or failure status. </param>
		''' 
		Friend Sub ReadViaInterruptTransfer _
		 (ByVal pipeID As Byte, _
		 ByVal bytesToRead As UInt32, _
		 ByRef buffer() As Byte, _
		 ByRef bytesRead As UInt32, _
		 ByRef success As Boolean)


            Dim oldval As Byte
            Dim byteNumber As Byte
            ' Dim binary As String
            'Dim invert As Int32
            'Dim bitset As Int32
            'Dim setunset As Int32
            Dim TC As String
            Dim TCsetUnset As Boolean



			Try
				'***
				' winusb function 

				' summary
				' Attempts to read data from a device interface.

				' parameters
				' Device handle returned by WinUsb_Initialize.
				' Endpoint address.
				' Buffer to store the data.
				' Maximum number of bytes to return.
				' Number of bytes read.
				' Null pointer for non-overlapped.

				' Returns
				' True on success.
                '***

                oldval = 0
                While True


                    success = WinUsb_ReadPipe _
                     (myDevInfo.winUsbHandle, _
                     pipeID, _
                     buffer, _
                     bytesToRead, _
                     bytesRead, _
                     IntPtr.Zero)
                    If success Then
                        byteNumber = 0
                        While (byteNumber < (bytesRead - 3))
                            Console.WriteLine("Track Circuit " & buffer(byteNumber) & " is " & buffer(byteNumber + 1))
                            ' Form1.setTrackCircuit(buffer(byteNumber), CBool(buffer(byteNumber + 1)))
                            TC = CStr(buffer(byteNumber))
                            TCsetUnset = CBool(buffer(byteNumber + 1))
                            m_parent.ClockDisplayLabel.Invoke(New setTrackCircuitCallbackDelegate(AddressOf m_parent.setTrackCircuitCallback), New Object() {"123", TC, TCsetUnset, True})


                            byteNumber = CByte(byteNumber + 2)

                        End While
                        Console.WriteLine("command " & buffer(byteNumber) & " is " & buffer(byteNumber + 1))

                        ' If buffer(0) <> oldval Then
                        ' bitset = buffer(0) Xor oldval
                        ' setunset = Not (buffer(0) And bitset)
                        ' invert = buffer(0)
                        ' binary = Convert.ToString(invert, 2)
                        'Console.WriteLine("Got data " & bytesToRead & " " & bytesRead & " " & buffer(0) & " " & buffer(1) & " " & binary)
                        'oldval = buffer(0)
                        ' End If

                    End If
                    System.Threading.Thread.Sleep(200)
                    If (myDevInfo.deviceHandle.IsClosed) Then
                        Console.WriteLine("Balls")
                        Thread.Sleep(1000000)

                    End If
                End While


                If Not success Then

                    CloseDeviceHandle()

                End If

            Catch ex As System.AccessViolationException
                MsgBox("System.AccessViolationException")
            Catch ex As Exception
                Throw
            End Try

		End Sub


		''' <summary>
		''' Attempts to send data via a bulk OUT endpoint.
		''' </summary>
		''' 
		''' <param name="buffer"> Buffer containing the bytes to write. </param>
		''' <param name="bytesToWrite"> Number of bytes to write. </param>
		''' 
		''' <returns>
		''' True on success, False on failure.
		''' </returns>

		Friend Function SendViaBulkTransfer _
		 (ByVal buffer As Byte(), _
		 ByVal bytesToWrite As UInt32) _
		 As Boolean

			Dim bytesWritten As UInt32
			Dim success As Boolean

			Try
				'***
				' winusb function 

				' summary
				' Attempts to write data to a device interface.

				' parameters
				' Device handle returned by WinUsb_Initialize.
				' Endpoint address.
				' Buffer with data to write.
				' Number of bytes to write.
				' Number of bytes written.
				' IntPtr.Zero for non-overlapped I/O.

				' Returns
				' True on success.
				' ***

				success = WinUsb_WritePipe _
				 (myDevInfo.winUsbHandle, _
				 myDevInfo.bulkOutPipe, _
				 buffer, _
				 bytesToWrite, _
				 bytesWritten, _
				 IntPtr.Zero)


				If Not success Then

					CloseDeviceHandle()

				End If
				Return success

			Catch ex As System.AccessViolationException
				MsgBox("System.AccessViolationException")
			Catch ex As Exception
				Throw
			End Try

		End Function

		''' <summary>
		''' Attempts to send data via an interrupt OUT endpoint.
		''' </summary>
		''' 
		''' <param name="buffer"> Buffer containing the bytes to write. </param>
		''' <param name="bytesToWrite"> Number of bytes to write. </param>
		''' 
		''' <returns>
		''' True on success, False on failure.
		''' </returns>

		Friend Function SendViaInterruptTransfer _
		 (ByVal buffer As Byte(), _
		 ByVal bytesToWrite As UInt32) _
		 As Boolean

			Dim bytesWritten As UInt32
			Dim success As Boolean
            Dim bufferbyte(3) As Byte

            'bufferbyte(0) = 71
            'bufferbyte(1) = 4
            'bufferbyte(2) = 1
            'bytesToWrite = 3

			Try
				'***
				' winusb function 

				' summary
				' Attempts to write data to a device interface.

				' parameters
				' Device handle returned by WinUsb_Initialize.
				' Endpoint address.
				' Buffer with data to write.
				' Number of bytes to write.
				' Number of bytes written.
				' IntPtr.Zero for non-overlapped I/O.

				' Returns
				' True on success.
				' ***

                success = WinUsb_WritePipe _
                 (myDevInfo.winUsbHandle, _
                 myDevInfo.interruptOutPipe, _
                 buffer, _
                 bytesToWrite, _
                 bytesWritten, _
                 IntPtr.Zero)
                success = WinUsb_WritePipe _
                              (myDevInfo.winUsbHandle, _
                              myDevInfo.interruptOutPipe, _
                              buffer, _
                              bytesToWrite, _
                              bytesWritten, _
                              IntPtr.Zero)
                success = WinUsb_WritePipe _
                              (myDevInfo.winUsbHandle, _
                              myDevInfo.interruptOutPipe, _
                              buffer, _
                              bytesToWrite, _
                              bytesWritten, _
                              IntPtr.Zero)
                success = WinUsb_WritePipe _
                              (myDevInfo.winUsbHandle, _
                              myDevInfo.interruptOutPipe, _
                              buffer, _
                              bytesToWrite, _
                              bytesWritten, _
                              IntPtr.Zero)
                success = WinUsb_WritePipe _
                              (myDevInfo.winUsbHandle, _
                              myDevInfo.interruptOutPipe, _
                              buffer, _
                              bytesToWrite, _
                              bytesWritten, _
                              IntPtr.Zero)
                success = WinUsb_WritePipe _
                              (myDevInfo.winUsbHandle, _
                              myDevInfo.interruptOutPipe, _
                              buffer, _
                              bytesToWrite, _
                              bytesWritten, _
                              IntPtr.Zero)
                success = WinUsb_WritePipe _
                              (myDevInfo.winUsbHandle, _
                              myDevInfo.interruptOutPipe, _
                              buffer, _
                              bytesToWrite, _
                              bytesWritten, _
                              IntPtr.Zero)
                success = WinUsb_WritePipe _
                              (myDevInfo.winUsbHandle, _
                              myDevInfo.interruptOutPipe, _
                              buffer, _
                              bytesToWrite, _
                              bytesWritten, _
                              IntPtr.Zero)
                success = WinUsb_WritePipe _
                              (myDevInfo.winUsbHandle, _
                              myDevInfo.interruptOutPipe, _
                              buffer, _
                              bytesToWrite, _
                              bytesWritten, _
                              IntPtr.Zero)

				If Not success Then

                    CloseDeviceHandle()
                Else
                    'frmMain.lstResults.Items.Add("My device Toggled.")


                End If
          

				Return success
			Catch ex As System.AccessViolationException
				MsgBox("System.AccessViolationException")
			Catch ex As Exception
				Throw
			End Try

		End Function

		''' <summary>
		''' Sets pipe policy.
		''' Used when the value parameter is a byte (all except PIPE_TRANSFER_TIMEOUT).
		''' </summary>
		''' 
		''' <param name="pipeId"> Pipe to set a policy for. </param>
		''' <param name="policyType"> POLICY_TYPE member. </param>
		''' <param name="value"> Policy value. </param>
		''' 
		''' <returns>
		''' True on success, False on failure.
		''' </returns>
		''' 
		Private Function SetPipePolicy(ByVal pipeId As Byte, ByVal policyType As UInt32, ByVal value As Byte) _
		 As Boolean

			Dim success As Boolean

			Try
				'***
				' winusb function 

				' summary
				' sets a pipe policy 

				' parameters
				' handle returned by WinUsb_Initialize
				' identifies the pipe
				' POLICY_TYPE member.
				' length of value in bytes
				' value to set for the policy.

				' returns
				' True on success 
				'***

				success = WinUsb_SetPipePolicy _
				 (myDevInfo.winUsbHandle, _
				 pipeId, _
				 policyType, _
				 1, _
				 value)

				Return success

			Catch ex As Exception
				Throw
			End Try

		End Function

		''' <summary>
		''' Sets pipe policy.
		''' Used when the value parameter is a UInt32 (PIPE_TRANSFER_TIMEOUT only).
		''' </summary>
		''' 
		''' <param name="pipeId"> Pipe to set a policy for. </param>
		''' <param name="policyType"> POLICY_TYPE member. </param>
		''' <param name="value"> Policy value. </param>
		''' 
		''' <returns>
		''' True on success, False on failure.
		''' </returns>
		''' 
		Private Function SetPipePolicy(ByVal pipeId As Byte, ByVal policyType As UInt32, ByVal value As UInt32) _
		 As Boolean

			Dim success As Boolean

			Try

				'***
				' winusb function 

				' summary
				' sets a pipe policy 

				' parameters
				' handle returned by WinUsb_Initialize
				' identifies the pipe
				' POLICY_TYPE member.
				' length of value in bytes
				' value to set for the policy.

				' returns
				' True on success 
				'***

				success = WinUsb_SetPipePolicy1 _
				 (myDevInfo.winUsbHandle, _
				 pipeId, _
				 policyType, _
				 4, _
				 value)

				Return success

			Catch ex As Exception
				Throw
			End Try

		End Function


		''' <summary>
		''' Is the endpoint's direction IN (device to host)?
		''' </summary>
		''' 
		''' <param name="addr"> The endpoint address. </param>
		''' <returns>
		''' True if IN (device to host), False if OUT (host to device)
		''' </returns> 
		''' 
		Private Function UsbEndpointDirectionIn(ByVal addr As Int32) As Boolean

			Try
				If ((addr And &H80) = &H80) Then
					UsbEndpointDirectionIn = True
				Else
					UsbEndpointDirectionIn = False
				End If

			Catch ex As Exception
				Throw
			End Try
		End Function

		''' <summary>
		''' Is the endpoint's direction OUT (host to device)?
		''' </summary>
		''' 
		''' <param name="addr"> The endpoint address. </param>
		''' 
		''' <returns>
		''' True if OUT (host to device, False if IN (device to host)
		''' </returns>

		Private Function UsbEndpointDirectionOut(ByVal addr As Int32) As Boolean

			Try
				If ((addr And &H80) = 0) Then
					UsbEndpointDirectionOut = True
				Else
					UsbEndpointDirectionOut = False
				End If

			Catch ex As Exception
				Throw
			End Try

		End Function

        Public Sub New(ByVal set_m_parent As Form1)
            m_parent = set_m_parent
        End Sub
    End Class
End Namespace
