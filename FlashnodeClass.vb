Imports System.Threading



Public Class FlashnodeClass
    Private t As Thread
    Public m_parent As Form1
    Public nodeindex As Integer
    Public flashing As Boolean
    'put delegates here
    Delegate Sub DrawNodeCallbackDelegate(ByVal aThreadName As String, _
                                      ByVal theColor As Color, _
                                      ByVal nodeIndex As Integer, _
                                      ByVal nodeid As String)
    Delegate Sub DrawSignalCallbackDelegate(ByVal aThreadName As String, _
                                        ByVal theColor As Color, _
                                        ByVal nodeIndex As Integer, _
                                        ByVal nodeid As String)
    Delegate Sub DrawSignalPostCallbackDelegate(ByVal aThreadName As String, _
                                            ByVal theColor As Color, _
                                            ByVal nodeIndex As Integer, _
                                            ByVal nodeid As String)


    Sub New()
        t = New Thread(AddressOf FlashNode)
        flashing = True
        t.Start()
    End Sub

    Private Sub FlashNode()

        While flashing
            'Console.WriteLine("Flash " & nodeindex)
            If nodeindex > 0 Then


                m_parent.ClockDisplayLabel.Invoke(New DrawSignalPostCallbackDelegate(AddressOf m_parent.DrawSignalPostCallback), New Object() {"clockref", Color.White, nodeindex, "1"})
                Thread.Sleep(500)
                m_parent.ClockDisplayLabel.Invoke(New DrawSignalPostCallbackDelegate(AddressOf m_parent.DrawSignalPostCallback), New Object() {"clockref", Color.Gray, nodeindex, "1"})
                Thread.Sleep(500)
            End If
        End While
        KillFlash()
    End Sub


    Public Sub KillFlash()

        m_parent.ClockDisplayLabel.Invoke(New DrawSignalPostCallbackDelegate(AddressOf m_parent.DrawSignalPostCallback), New Object() {"clockref", Color.Gray, nodeindex, "1"})
        t.Abort()


    End Sub
End Class
