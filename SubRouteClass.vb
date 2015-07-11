
Imports System.Threading

Public Class SubRouteClass
    Private t As Thread
    Public m_parent As Form1

    Public routeCount As Integer
    'Stick the delegates here
    Delegate Sub DrawLineDelegate(ByVal aThreadName As String, ByVal aTextBox As Label, ByVal newText As Integer)

    Sub New()
        t = New Thread(AddressOf Me.updateRoute)
        t.Start()
    End Sub

    Sub updateRoute()
        For count1 As Integer = 0 To (routeCount - 1)
            m_parent.ClockDisplayLabel.Invoke(New DrawLineDelegate(AddressOf m_parent.SetRoute), New Object() {"clockref", m_parent.ClockDisplayLabel, count1})

            'Form1.TrackArrayList(count1).routeSet()
            Thread.Sleep(20)
        Next
        t.abort()

    End Sub


End Class
