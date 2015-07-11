Public Class USBHandlerClass


    Private Const WINUSB_DEMO_GUID_STRING As String = "{58D07210-27C1-11DD-BD0B-0800200C9a66}"
    Private deviceNotificationHandle As IntPtr
    Private myDeviceDetected As Boolean = False
    Private deviceID As Integer
    Private myDeviceManagement As New WinUsbDemo.DeviceManagement()
    Private myDevicePathName As String = ""
    Private myWinUsbDevice As New WinUsbDemo.WinUsbDevice(m_parent)
    Public m_parent As Form1

    Dim toggleVal As Byte = 0

    ''' <summary>
    ''' Define a class of delegates with the same parameters as 
    ''' WinUsbDevice.ReadViaBulkTransfer and WinUsbDevice.ReadViaInterruptTransfer.
    ''' Used for asynchronous reads from the device.
    ''' </summary>

    Private Delegate Sub ReadFromDeviceDelegate _
     (ByVal pipeID As Byte, _
     ByVal bufferLength As UInt32, _
     ByRef buffer() As Byte, _
     ByRef lengthTransferred As UInt32, _
     ByRef success As Boolean)



    ''' <summary>
    ''' If a device with the specified device interface GUID hasn't been previously detected,
    ''' look for it. If found, open a handle to the device.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' True if the device is detected, False if not detected.
    ''' </returns>

    Private Function FindMyDevice() As Boolean

        Dim deviceFound As Boolean
        Dim devicePathName As String = ""
        Dim success As Boolean

        Try

            If Not myDeviceDetected Then

                ' Convert the device interface GUID string to a GUID object: 

                Dim winUsbDemoGuid As New System.Guid(WINUSB_DEMO_GUID_STRING)

                'Fill an array with the device path names of all attached devices with matching GUIDs.

                deviceFound = myDeviceManagement.FindSpecificDeviceFromGuid _
                    (winUsbDemoGuid, _
                    devicePathName, _
                    deviceID)

                If deviceFound = True Then

                    success = myWinUsbDevice.GetDeviceHandle(devicePathName)

                    If (success) Then

                        myDeviceDetected = True

                        'Save DevicePathName so OnDeviceChange() knows which name is my device.

                        myDevicePathName = devicePathName

                    Else
                        'There was a problem in retrieving the information.

                        myDeviceDetected = False
                        myWinUsbDevice.CloseDeviceHandle()

                    End If

                End If

                If myDeviceDetected Then

                    'The device was detected.
                    'Register to receive notifications if the device is removed or attached.

                    success = myDeviceManagement.RegisterForDeviceNotifications _
                        (myDevicePathName, _
                        m_parent.Handle, _
                        winUsbDemoGuid, _
                        deviceNotificationHandle)

                    If success Then

                        myWinUsbDevice.InitializeDevice()

                        ' Commented out due to unreliable response from WinUsb_QueryDeviceInformation.
                        ' DisplayDeviceSpeed()

                    End If

                Else
                    Console.WriteLine("Device not found.")
 
                End If

            Else
                Console.WriteLine("Device detected.")
            End If

              Return myDeviceDetected

        Catch ex As Exception
            Throw
        End Try

    End Function



    ''' <summary>
    ''' Called when a WM_DEVICECHANGE message has arrived,
    ''' indicating that a device has been attached or removed.
    ''' </summary>
    ''' 
    ''' <param name="m"> A message with information about the device. </param>

    Friend Sub OnDeviceChange(ByVal m As Message)

        Try
            If (m.WParam.ToInt32 = WinUsbDemo.DeviceManagement.DBT_DEVICEARRIVAL) Then

                ' If WParam contains DBT_DEVICEARRIVAL, a device has been attached.
                ' Find out if it's the device we're communicating with.

                If myDeviceManagement.DeviceNameMatch(m, myDevicePathName) Then
                    Console.WriteLine("My device attached.")
                    myWinUsbDevice.InitializeDevice()


                    'FindMyDevice()

                End If

            ElseIf (m.WParam.ToInt32 = WinUsbDemo.DeviceManagement.DBT_DEVICEREMOVECOMPLETE) Then

                ' If WParam contains DBT_DEVICEREMOVAL, a device has been removed.
                ' Find out if it's the device we're communicating with.

                If myDeviceManagement.DeviceNameMatch(m, myDevicePathName) Then

                    Console.WriteLine("My device removed.")
                    myWinUsbDevice.CloseDeviceHandle()
                    ' Set MyDeviceDetected False so on the next data-transfer attempt,
                    ' FindMyDevice() will be called to look for the device 
                    ' and get a new handle.

                    '  frmMy.myDeviceDetected = False

                End If
            End If


        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub Startup()

        Try
            myWinUsbDevice = New WinUsbDemo.WinUsbDevice(m_parent)
            'InitializeDisplay()

        Catch ex As Exception
            Throw
        End Try

    End Sub

    Private Sub SendAndReceiveViaInterruptTransfer()

        Dim bytesToSend As UInt32 ' winusb_readpipe requires this parameter to be a UINT32.
        Dim comboBoxText As String = ""
        Dim dataBuffer(1) As Byte
        Dim formText As String = ""
        Dim success As Boolean

        Try
            ' Get bytes to send from comboboxes.

            ' Get the value to send as a hex string.

            '   comboBoxText = _
            'System.Convert.ToString(cboInterruptOutByte0.SelectedItem).TrimEnd(Nothing)

            ' Convert the string to a byte.

            'dataBuffer(0) = _
            'Convert.ToByte(String.Format("{0:X2}", comboBoxText), 16)

            ' Get the value to send as a hex string.

            ' comboBoxText = _
            'System.Convert.ToString(cboInterruptOutByte1.SelectedItem).TrimEnd(Nothing)

            ' Convert the string to a byte.

            'dataBuffer(1) = _
            'Convert.ToByte(String.Format("{0:X2}", comboBoxText), 16)

            'bytesToSend = Convert.ToUInt32(dataBuffer.Length)

            'If the device hasn't been detected, was removed, or timed out on a previous attempt
            'to access it, look for the device.

            myDeviceDetected = FindMyDevice()

            If myDeviceDetected Then

                success = myWinUsbDevice.SendViaInterruptTransfer(dataBuffer, bytesToSend)

                If success Then
                    formText = "Data sent via interrupt transfer."
                Else
                    formText = "Interrupt OUT transfer failed."
                End If

                ReadDataViaInterruptTransfer()

            End If

        Catch ex As Exception
            Throw
        End Try

    End Sub

    ''' <summary>
    ''' Initiates a read operation from an interrupt IN endpoint.
    ''' To enable reading without blocking the main thread, uses an asynchronous delegate.
    ''' </summary>
    ''' 
    ''' <remarks>
    ''' To enable reading more than 2 bytes (with device firmware support), increase bytesToRead.
    ''' </remarks>

    Private Sub ReadDataViaInterruptTransfer()

        Dim ar As IAsyncResult
        Dim buffer(16) As Byte
        Dim bytesRead As UInt32
        Dim bytesToRead As UInt32 = 16
        Dim success As Boolean

        Try
            ' Define a delegate for the ReadViaInterruptTransfer method of WinUsbDevice.

            Dim MyReadFromDeviceDelegate As _
                New ReadFromDeviceDelegate(AddressOf myWinUsbDevice.ReadViaInterruptTransfer)

            ' The BeginInvoke method calls MyWinUsbDevice.ReadViaInterruptTransfer to attempt 
            ' to read data. The method has the same parameters as ReadViaInterruptTransfer,
            ' plus two additional parameters:
            ' GetReceivedInterruptData is the callback routine that executes when 
            ' ReadViaInterruptTransfer returns.
            ' MyReadFromDeviceDelegate is the asynchronous delegate object.

            ar = MyReadFromDeviceDelegate.BeginInvoke _
             (myWinUsbDevice.myDevInfo.interruptInPipe, _
             bytesToRead, _
             buffer, _
             bytesRead, _
             success, _
             New AsyncCallback(AddressOf GetReceivedInterruptData), _
             MyReadFromDeviceDelegate)

        Catch ex As Exception
            Throw
        End Try

    End Sub

    ''' <summary>
    ''' Retrieves received data from an interrupt endpoint.
    ''' This routine is called automatically when myWinUsbDevice.ReadViaInterruptTransfer
    ''' returns. The routine calls several marshaling routines to access the main form.
    ''' </summary>
    ''' 
    ''' <param name="ar"> An object containing status information about the 
    ''' asynchronous operation.</param>

    Private Sub GetReceivedInterruptData(ByVal ar As IAsyncResult)

        Dim byteValue As String = ""
        Dim bytesRead As UInt32
        Dim receivedDataBuffer As Byte()
        Dim success As Boolean

        Try
            receivedDataBuffer = Nothing

            'Define a delegate using the IAsyncResult object.

            Dim deleg As ReadFromDeviceDelegate = _
                DirectCast(ar.AsyncState, ReadFromDeviceDelegate)

            'Get the IAsyncResult object and the values of other paramaters that the
            'BeginInvoke method passed ByRef.

            deleg.EndInvoke(receivedDataBuffer, bytesRead, success, ar)

            'Display the received data in the form's list box.

            If (ar.IsCompleted And success) Then

                'MyMarshalToForm("AddItemToListBox", "Data received via interrupt transfer:")

                'MyMarshalToForm("AddItemToListBox", " Received Data:")
                Console.WriteLine("received")


                For i As Int32 = 0 To receivedDataBuffer.GetUpperBound(0)

                    ' Convert the byte value to a 2-character hex string.

                    byteValue = String.Format("{0:X2} ", receivedDataBuffer(i))

                    '  MyMarshalToForm("AddItemToListBox", " " & byteValue)

                Next i

            Else

                ' MyMarshalToForm("AddItemToListBox", "The attempt to read interrupt data has failed.")

                myDeviceDetected = False

            End If

            ' MyMarshalToForm("ScrollToBottomOfListBox", "")

            'Enable requesting another transfer.

            ' MyMarshalToForm("EnableCmdSendandReceiveViaInterruptTransfers", "")

        Catch ex As Exception
            Throw
        End Try
        System.Threading.Thread.Sleep(1000)
    End Sub




    Sub New(ByVal set_m_parent As Form1, ByVal setDeviceID As Integer)
        m_parent = set_m_parent
        deviceID = setDeviceID
        Dim databuffer(2) As Byte
        Dim success As Integer
        Startup()
        FindMyDevice()
        SendAndReceiveViaInterruptTransfer()
        databuffer(0) = 72
        databuffer(1) = 0

        success = myWinUsbDevice.SendViaInterruptTransfer(databuffer, 2)
        databuffer(1) = 255

        success = myWinUsbDevice.SendViaInterruptTransfer(databuffer, 2)



    End Sub
End Class
