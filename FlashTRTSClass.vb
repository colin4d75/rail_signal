Imports System.Threading


Public Class FlashTRTSClass
        Private t As Thread
        Public m_parent As Form1
        Public nodeindex As Integer
        Public flashing As Boolean
        'put delegates here
    Delegate Sub DrawTRTSCallbackDelegate(ByVal aThreadName As String, _
                                          ByVal isFlash As Boolean, _
                                          ByVal nodeIndex As Integer)
       

        Sub New()
        t = New Thread(AddressOf FlashTRTS)
            flashing = True
            t.Start()
        End Sub

    Private Sub FlashTRTS()

        While flashing
            'Console.WriteLine("Flash " & nodeindex)
            If nodeindex > 0 Then


                m_parent.ClockDisplayLabel.Invoke(New DrawTRTSCallbackDelegate(AddressOf m_parent.DrawTRTSCallback), New Object() {"clockref", True, nodeindex})
                Thread.Sleep(500)
                m_parent.ClockDisplayLabel.Invoke(New DrawTRTSCallbackDelegate(AddressOf m_parent.DrawTRTSCallback), New Object() {"clockref", False, nodeindex})
                Thread.Sleep(500)
            End If
        End While
        KillFlash()
    End Sub

    Public Sub KillFlash()
     m_parent.ClockDisplayLabel.Invoke(New DrawTRTSCallbackDelegate(AddressOf m_parent.DrawTRTSCallback), New Object() {"clockref", False, nodeindex})
        t.Abort()

    End Sub



    End Class
