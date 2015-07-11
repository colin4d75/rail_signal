Imports System.Threading

Public Class FlashPointBlankClass
    Private t As Thread
    Private pointBlankList As New ArrayList

    Public m_parent As Form1
    Public nodeindex As Integer
    Public flashing As Boolean
    'put delegates here
    Delegate Sub BlankPointCallbackDelegate(ByVal aThreadName As String, _
                                         ByVal theColor As Color, _
                                         ByVal nodeIndex As Integer, _
                                         ByVal nodeid As String)
    Delegate Sub UpdatePointStatusCallbackDelegate(ByVal aThreadName As SetPointClass, _
                                    ByVal setNodeID As String, ByVal setReverse As Boolean)

     Sub New(ByVal set_mparent As Form1)
        t = New Thread(AddressOf FlashBlank)
        flashing = False
        'nodeindex = setnodeIndex
        m_parent = set_mparent
        t.Start()
    End Sub

    Public Sub StartFlash()
        flashing = True

    End Sub


    Public Sub AddPointToBlankList(ByVal pointId As Integer)
        flashing = False
        If pointBlankList.Contains(pointId) Then

        Else
            pointBlankList.Add(pointId)
        End If
        flashing = True
    End Sub

    Private Sub SetList()


    End Sub


    Private Sub FlashBlank()
        While True
            If flashing Then
                m_parent.ClockDisplayLabel.Invoke(New BlankPointCallbackDelegate(AddressOf m_parent.BlankPointCallback), New Object() {"clockref", Color.White, 1, "1"})
                ' Console.WriteLine("delay 1")
                Thread.Sleep(500)
            End If
        End While

    End Sub

    Public Sub KillFlash()

        m_parent.ClockDisplayLabel.Invoke(New BlankPointCallbackDelegate(AddressOf m_parent.BlankPointCallback), New Object() {"clockref", Color.Black, nodeindex, "1"})
        t.Abort()


    End Sub
End Class
