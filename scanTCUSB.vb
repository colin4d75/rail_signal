Imports System.Threading
Public Class scanTCUSB
    Private t As Thread
    Public m_parent As Form1


    Sub New()

        t = New Thread(AddressOf ScanUSB)
        t.IsBackground = True


    End Sub

    Private Sub ScanUSB()


    End Sub
End Class
