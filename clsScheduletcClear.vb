Imports System.Threading
Public Class clsScheduletcClear
    Private m_parent As Form1
    Private t As Thread
    Private delay As Single
    Private track As Integer


    Delegate Sub clearTrackTcCallbackDelegate(ByVal aThreadName As String, _
                                      ByVal track As Integer)

    Sub New(ByVal set_m_parent As Form1, _
            ByVal setTrack As Integer, _
            ByVal setDelay As Integer)
        m_parent = set_m_parent
        delay = setDelay
        track = setTrack
        t = New Thread(AddressOf ScheduleTCClear)
        t.IsBackground = True
        t.Start()
        t.Name = "Service 1"
    End Sub

    Private Sub ScheduleTCClear()
        Thread.Sleep(delay * 1000)
        ClearTrackTc(track)
        t.Abort()

    End Sub

    Private Sub ClearTrackTc(ByVal track As Integer)
        m_parent.ClockDisplayLabel.Invoke(New clearTrackTcCallbackDelegate(AddressOf m_parent.ClearTrackTcCallback), New Object() {"clockref", track})
    End Sub
End Class
