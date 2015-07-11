
Imports System.Threading
Public Class SetRoutePointsClass
    Private pointsSetList As New ArrayList()
    Private pointsReverseList As New ArrayList()
    Private t As Thread
    Private pointStatus As String
    Private localSubRouteList As New ArrayList()
    Private localRouteId As String
    Private localEntryNode As String
    Private localExitNode As String
    Private localEntryNodeID As String
    Private localAuto As Boolean
    Private localRouteObj As SetRouteClass



    Public m_parent As Form1
    Delegate Sub GetPointStatusCallbackDelegate(ByVal aThreadName As SetRoutePointsClass, _
                                       ByVal setNodeID As String, ByVal setReverse As Boolean)
    Delegate Sub ReturnRouteSetCallbackDelegate(ByVal aThreadName As SetRoutePointsClass, _
                                                ByVal localSubRouteList As ArrayList, _
                                                ByVal localRouteId As String, _
                                                ByVal localEntryNode As String, _
                                                ByVal localExitNode As String, _
                                                ByVal localEntryNodeID As String, _
                                                ByVal localAuto As Boolean, _
                                                ByVal localRouteObj As SetRouteClass)

    Sub New(ByVal set_mparent As Form1, ByVal setPointsSetList As ArrayList, _
            ByVal setPointsReverseList As ArrayList, _
                                       ByVal setSubRouteList As ArrayList, _
                                       ByVal setRouteId As String, _
                                       ByVal setEntryNode As String, _
                                       ByVal setExitNode As String, _
                                       ByVal setEntryNodeID As String, _
                                       ByVal setAuto As Boolean, _
                                       ByVal setRouteObj As SetRouteClass)
        t = New Thread(AddressOf RunPointsSet)
        pointsSetList = setPointsSetList
        pointsReverseList = setPointsReverseList
        m_parent = set_mparent
        localSubRouteList = setSubRouteList
        localRouteId = setRouteId
        localEntryNode = setEntryNode
        localExitNode = setExitNode
        localEntryNodeID = setEntryNodeID
        localAuto = setAuto
        localRouteObj = setRouteObj
        t.Start()
    End Sub

    Private Sub RunPointsSet()
        Dim point As String
        Dim loopCount As Byte
        loopCount = 0
        While (pointsSetList.Count + pointsReverseList.Count > 0)
            For i As Integer = pointsSetList.Count - 1 To 0 Step -1
                point = pointsSetList(i)
                'Console.WriteLine("Check " & point & " set")
                RequestPointStatus(point, True)
            Next
            For i As Integer = pointsReverseList.Count - 1 To 0 Step -1
                point = pointsReverseList(i)
                'Console.WriteLine("Check " & point & " reverse")
                RequestPointStatus(point, False)
            Next
            loopCount = loopCount + 1
            'Console.WriteLine("points loop" & loopCount)
            If loopCount > 40 Then
                Console.WriteLine("count timeout" & loopCount)

                Exit While
            End If
            'Console.WriteLine("delay 2")

            Thread.Sleep(300)
        End While
        'Thread.Sleep(10000)
        Console.WriteLine("all points set")
        m_parent.ClockDisplayLabel.Invoke(New ReturnRouteSetCallbackDelegate(AddressOf m_parent.ReturnRouteSetCallback), New Object() {Me, localSubRouteList, localRouteId, localEntryNode, _
                       localExitNode, localEntryNodeID, _
                       localAuto, localRouteObj})


        t.Abort()
    End Sub


    Private Sub RequestPointStatus(ByVal nodeID As String, ByVal setReverse As Boolean)
        ' Console.WriteLine("Check1 " & nodeID)

        m_parent.ClockDisplayLabel.Invoke(New GetPointStatusCallbackDelegate(AddressOf m_parent.GetPointStatusCallback), New Object() {Me, nodeID, setReverse})
    End Sub
    Public Sub SetPointStatus(ByVal nodeID As String, ByVal status As String, ByVal setReverse As Boolean)
        Me.pointStatus = status
        'Console.WriteLine("Check2  " & nodeID & " is " & pointStatus & " should be " & setReverse)
        If pointStatus = setReverse Then
            'Console.WriteLine("Remove " & nodeID)
            If setReverse Then
                pointsSetList.Remove(nodeID)
            Else
                pointsReverseList.Remove(nodeID)
            End If
        End If
    End Sub

End Class
