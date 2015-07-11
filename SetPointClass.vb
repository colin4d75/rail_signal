
Imports System.Threading
Public Class SetPointClass


    Private t As Thread
    Private pointBlankList As New ArrayList

    Private m_parent As Form1
    Private nodeindex As Integer
    Private nodeID As String
    Private pointStatus As String
    Private setReverse As Boolean
    Private running As Boolean

    'put delegates here
    Delegate Sub PointSetCompleteCallbackDelegate(ByVal aThreadName As String, _
                                         ByVal theColor As Color, _
                                         ByVal nodeIndex As Integer, _
                                         ByVal nodeid As String)
    Delegate Sub PointSetBlankCallbackDelegate(ByVal aThreadName As String, _
                                         ByVal theColor As Color, _
                                         ByVal nodeIndex As Integer, _
                                         ByVal nodeid As String)
    Delegate Sub GetPointStatusCallbackDelegate(ByVal aThreadName As SetPointClass, _
                                   ByVal setNodeID As String, ByVal setReverse As Boolean)
    Delegate Sub UpdatePointStatusCallbackDelegate(ByVal aThreadName As SetPointClass, _
                                     ByVal setNodeID As String, ByVal setReverse As Boolean)


    Sub New(ByVal set_mparent As Form1, ByVal setNodeID As String, _
            ByVal setSetReverse As Boolean)
        t = New Thread(AddressOf SetPointDelay)
        nodeID = setNodeID
        m_parent = set_mparent
        setReverse = setSetReverse
        t.Start()
    End Sub


    Private Sub SetPointDelay()
        running = True
        RequestPointStatus(nodeID, setReverse)
        Console.WriteLine("Set point " & nodeID & " to be " & setReverse)
        '  m_parent.ClockDisplayLabel.Invoke(New PointSetCompleteCallbackDelegate(AddressOf m_parent.PointSetCompleteCallback), New Object() {"clockref", Color.Black, nodeindex, "1"})
        Console.WriteLine("point set delay")
        Thread.Sleep(1500)

        ' m_parent.ClockDisplayLabel.Invoke(New PointSetBlankCallbackDelegate(AddressOf m_parent.PointSetBlankCallback), New Object() {"clockref", Color.Black, nodeindex, "1"})
        '
        't.Abort()
        While running
            Console.WriteLine("point set delay loop")

            Thread.Sleep(2000)
        End While
        t.Abort()

    End Sub
    Private Sub RequestPointStatus(ByVal nodeID As String, ByVal setReverse As Boolean)
        m_parent.ClockDisplayLabel.Invoke(New GetPointStatusCallbackDelegate(AddressOf m_parent.GetPointStatusCallback), New Object() {Me, nodeID, setReverse})
    End Sub
    Private Sub UpdatePointStatus(ByVal nodeID As String, ByVal setReverse As Boolean)
        m_parent.ClockDisplayLabel.Invoke(New UpdatePointStatusCallbackDelegate(AddressOf m_parent.UpdatePointStatusCallback), New Object() {Me, nodeID, setReverse})
    End Sub


    Public Sub SetPointStatus(ByVal nodeID As String, ByVal status As String, ByVal setReverse As Boolean)
        Me.pointStatus = status
        'Console.WriteLine("do something now set " & setReverse & " for point " & nodeID & " is " & status)
        Dim leverID As String
        If status <> setReverse Then
            Console.WriteLine("Throw point" & nodeID)
            leverID = m_parent.nodeArrayList(m_parent.GetNodeIndex(nodeID)).getNodeLever()
             If setReverse Then
                If m_parent.LeverList(leverID) <> 0 Then
                    m_parent.LeverList(leverID) = 0
                    Console.WriteLine("Return lever " & leverID)
                  
                    Dim MoveLeverObj As New MoveLeverClass(m_parent, leverID, setReverse)
                    'UpdatePointStatus(nodeID, setReverse)

                End If
            Else
                If m_parent.LeverList(leverID) <> 1 Then
                    m_parent.LeverList(leverID) = 1
                    Console.WriteLine("Pull lever " & leverID)
                 
                    Dim MoveLeverObj As New MoveLeverClass(m_parent, leverID, setReverse)

                End If
                End If
         End If
        running = False
    End Sub
End Class
