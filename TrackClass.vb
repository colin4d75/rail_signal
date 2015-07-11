Public Class TrackClass

    Private mypen As Pen
    Private myWhitePen As Pen
    Private myBlackPen As Pen
    Private myBluePen As Pen
    Private mygreenPen As Pen
    Private myRedPen As Pen
    Private myPinkPen As Pen
    Private xStart As Integer
    Private yStart As Integer
    Private xEnd As Integer
    Private yEnd As Integer
    Private autoRouteID As String
    Private fringe As String
    Public trackLength As Integer
    Public lineSpeed As Integer
    Public line As String
    Public nodeFrom As String
    Public nodeTo As String
    Private trackStatus As Integer
    Public trackIsOccupied As Boolean
    Public tc As Integer
    Public trackID As Integer
    Private setRouteId As String
    Public routeStartNodeId As String
    Public routeEndNodeId As String
    Public routeStartNodeIndex As Integer
    Private pointNodeID As String
    Private lever As String
    Private leverSetReverse As String
    Private auto As Boolean
    Private m_parent As Form1

    Public platform As String
    Public location As String
    Public platType As String
    Public trackIsPoint As Boolean = False
    Public PointSetReverse As Boolean = True
    'List Delegates here
    Delegate Sub ChangeLine(ByVal aThreadName As String, ByVal aTextBox As ListBox, ByVal newText As String)

   

    Public Function GetTrackStatus() As Integer
        Return trackStatus

    End Function

    Public Function GetTrackOccupied() As Integer
        Return trackIsOccupied

    End Function

    Public Sub New(ByVal setm_parent As Form1, _
                   ByVal setNode As String, _
                   ByVal setNodeTo As String, _
                   ByVal setXStart As Integer, _
                   ByVal setYStart As Integer, _
                   ByVal setXEnd As Integer, _
                   ByVal setYEnd As Integer, _
                   ByVal trackCircuit As Integer, _
                   ByVal setTrackId As Integer, _
                   ByVal setLineSpeed As Integer, _
                   ByVal setTrackLength As Integer, _
                   ByVal setline As Integer, _
                   ByVal setLocation As String, _
                   ByVal setPlatform As String, _
                   ByVal setPlatType As String, _
                   ByVal setFringe As String, _
                   ByVal setAuto As Boolean, _
                   ByVal setAutoID As String, _
                   ByVal setLever As String, _
                   ByVal setLeverSetReverse As String
                )
        m_parent = setm_parent
        nodeFrom = setNode
        nodeTo = setNodeTo
        xStart = setXStart
        yStart = setYStart
        line = setline
        fringe = setFringe
        lineSpeed = setLineSpeed
        trackLength = setTrackLength
        location = setLocation
        platform = setPlatform
        platType = setPlatType
        auto = setAuto
        autoRouteID = setAutoID
        lever = setLever
        leverSetReverse = setLeverSetReverse
        trackIsOccupied = False
        Dim NodeType As String
        Dim nodeIndex As Integer
        nodeIndex = Form1.GetNodeIndex(nodeFrom)
        NodeType = Form1.GetNodeType(nodeFrom)
        If NodeType = "P" Or NodeType = "X" Then
            'it's a point
            trackIsPoint = True
            pointNodeID = nodeFrom
            'PointSetReverse = Form1.GetPointSetReverse(nodeFrom)
        End If
        NodeType = Form1.GetNodeType(nodeTo)
        If NodeType = "P" Or NodeType = "X" Then
            'it's a point
            pointNodeID = nodeTo
            trackIsPoint = True
        End If
        'Console.WriteLine("track " & trackID & " is point " & trackIsPoint)
        xEnd = setXEnd
        yEnd = setYEnd
        tc = trackCircuit
        trackStatus = 0
        trackID = setTrackId
        mypen = New Pen(Color.Gray, 4)
        myBlackPen = New Pen(Color.Black, 3)
        myWhitePen = New Pen(Color.White, 4)
        mygreenPen = New Pen(Color.Green, 4)
        myBluePen = New Pen(Color.Blue, 4)
        myRedPen = New Pen(Color.Red, 4)
        myPinkPen = New Pen(Color.Pink, 4)
        Form1.DrawTrack(mypen, xStart, yStart, xEnd, yEnd, fringe)



    End Sub


    Public Sub ClearSetRoute()
        'draw the line on the form using the pen object
        trackStatus = 0
        'Console.WriteLine("Clear Route " & setRouteId)
        Dim tempRoute As SetRouteClass
        tempRoute = New SetRouteClass(m_parent, auto)
        tempRoute.entryNodeId = routeStartNodeIndex
        tempRoute.RouteOccupiedCheckRoute(routeStartNodeId, routeEndNodeId)
        'Set Route
        If tempRoute.routeIsClear Then
            tempRoute.SetRoute()
            'check following route

        End If
        If trackIsPoint Then
            Form1.SetNodePointBlank(pointNodeID)
        End If
        Form1.UpdateDisplay()

    End Sub

    Public Sub SetAutoRoute()
        'draw the line on the form using the pen object
        trackStatus = 0
        Console.WriteLine("re set Route " & autoRouteID & " track " & trackID)
        Dim tempRoute As SetRouteClass
        tempRoute = New SetRouteClass(m_parent, auto)
        ' tempRoute.setRouteSetdBFlag(autoRouteID, "1")

        tempRoute.entryNodeId = routeStartNodeIndex
        tempRoute.RouteOccupiedCheckRoute(routeStartNodeId, routeEndNodeId)
        'Set Route
        'If tempRoute.routeIsClear Then
        tempRoute.SetRoute()
        tempRoute.setRouteSetdBFlag(autoRouteID, "1")

        'check following route

        'End If
        If trackIsPoint Then
            Form1.SetNodePointBlank(pointNodeID)
        End If
        Form1.UpdateDisplay()

    End Sub

    Public Sub RouteSet(ByVal routeID As String, _
                         ByVal setStartNodeId As String, _
                         ByVal setEndNodeId As String, _
                         ByVal setStartNodeIndex As Integer)

        RouteSet(routeID, setStartNodeId, setEndNodeId, setStartNodeIndex, auto)
    End Sub


    Public Sub RouteSet(ByVal routeID As String, _
                        ByVal setStartNodeId As String, _
                        ByVal setEndNodeId As String, _
                        ByVal setStartNodeIndex As Integer, _
                        ByVal setauto As Boolean)
        'draw the line on the form using the pen object
        trackStatus = 1
        setRouteId = routeID
        auto = setauto
        autoRouteID = routeID
        routeStartNodeId = setStartNodeId
        routeEndNodeId = setEndNodeId
        routeStartNodeIndex = setStartNodeIndex
        'Route Set
        If auto Then
            Form1.DrawTrack(mygreenPen, xStart, yStart, xEnd, yEnd, fringe)
            'Console.WriteLine("Set Auto Route " & nodeFrom & " to " & nodeTo & " " & trackStatus & " " & autoRouteID)

        Else
            Form1.DrawTrack(myWhitePen, xStart, yStart, xEnd, yEnd, fringe)
            'Console.WriteLine("Set Route " & nodeFrom & " to " & nodeTo & " " & trackStatus)
            'Console.WriteLine(xStart & " " & yStart & " " & xEnd & " " & yEnd)
        End If
        'Draw Nodes
        If nodeFrom = "s501" Then
            '

        End If

    End Sub

    Public Sub RoutePending(ByVal routeID As String, _
                        ByVal setStartNodeId As String, _
                        ByVal setEndNodeId As String, _
                        ByVal setStartNodeIndex As Integer, _
                        ByVal setauto As Boolean)
        'draw the line on the form using the pen object
        trackStatus = 1
        setRouteId = routeID
        auto = setauto
        autoRouteID = routeID
        routeStartNodeId = setStartNodeId
        routeEndNodeId = setEndNodeId
        routeStartNodeIndex = setStartNodeIndex

    End Sub


    Public Sub TrackOccupied(ByVal realNotVirtual As Boolean)
        trackIsOccupied = True
        Dim leverset As Byte
        If trackIsPoint Then
            Console.WriteLine("Point lever " & lever & " " & leverSetReverse)
            'if it's a point only draw the bit set
            ' a point has two possible settings
            'Only draw the one that is set 
            If trackStatus = 1 Then
                'Only set the track occupied if a route is set
                Console.WriteLine("Set track occupied " & nodeFrom & " to " & nodeTo & " " & trackStatus)
                trackStatus = 0
                'Route Set
                Form1.DrawTrack(myRedPen, xStart, yStart, xEnd, yEnd, fringe)
                Form1.UpdateDisplay()
            Else
                Console.WriteLine("Not setting track occupied " & nodeFrom & " to " & nodeTo & " " & trackStatus & " point set " & PointSetReverse)
                If realNotVirtual Then
                    'this info comes from a real track detection circuit,
                    'so draw it
                    If lever <> "     " Then
                        'this track is switchable. 
                        'Only set if lever is set
                        leverset = Form1.LeverList(lever)

                        If (leverSetReverse * leverSetReverse) = leverset Then
                            Console.WriteLine("lever set " & nodeFrom & " to " & nodeTo & " " & trackStatus & " point set " & PointSetReverse & " " & Form1.LeverList(lever))

                            Form1.DrawTrack(myPinkPen, xStart, yStart, xEnd, yEnd, fringe)
                            Form1.UpdateDisplay()
                        End If
                    Else
                        'not switchable so set
                        Console.WriteLine("No lever " & nodeFrom & " to " & nodeTo & " " & trackStatus & " point set " & PointSetReverse)

                        Form1.DrawTrack(myRedPen, xStart, yStart, xEnd, yEnd, fringe)
                        Form1.UpdateDisplay()

                    End If

                End If

                End If
        Else

                If trackStatus = 1 Then
                    trackStatus = 0
                    'Route Set
                    Console.WriteLine("Set track occupied " & nodeFrom & " to " & nodeTo & " " & trackStatus & " track " & trackID)

                    Form1.DrawTrack(myPinkPen, xStart, yStart, xEnd, yEnd, fringe)
                    Form1.UpdateDisplay()
                Else
                    'Don't set track on route that hasn't been set

                    'Test Addition
                    If realNotVirtual Then
                        Form1.DrawTrack(myRedPen, xStart, yStart, xEnd, yEnd, fringe)
                        Form1.UpdateDisplay()
                    End If
                End If

        End If
    End Sub
    Public Sub RouteClear()
        'draw the line on the form using the pen object
        If auto Then
            're-set the route as it's automatic
            trackStatus = 1
            'Route Clear, but auto
            Form1.DrawTrack(mygreenPen, xStart, yStart, xEnd, yEnd, fringe)

        Else
            trackStatus = 0
            'Route Clear
            Form1.DrawTrack(mypen, xStart, yStart, xEnd, yEnd, fringe)

        End If


    End Sub

    Public Sub RouteBlank()
        'draw the line on the form using the pen object
        If auto Then
            're-set the route as it's automatic
             'Route Clear, but auto
            Form1.DrawTrack(mygreenPen, xStart, yStart, xEnd, yEnd, fringe)

        Else
            'Route Clear
            Form1.DrawTrack(mypen, xStart, yStart, xEnd, yEnd, fringe)

        End If


    End Sub


    Public Sub TrackClear(ByVal realNotVirtual As Boolean)
        If auto Then

            trackIsOccupied = False
            trackStatus = 0
            'Route Clear
            Form1.DrawTrack(mygreenPen, xStart, yStart, xEnd, yEnd, fringe)
            Console.WriteLine("Auto route " & autoRouteID & " is clear at track " & trackID)
            'check signal status
            SetAutoRoute()

        Else
            trackIsOccupied = False
            trackStatus = 0
            'Route Clear
            Form1.DrawTrack(mypen, xStart, yStart, xEnd, yEnd, fringe)
        End If

        If trackIsPoint Then
            Form1.SetNodePointBlank(pointNodeID)
            Console.WriteLine("blank point " & pointNodeID)
        End If
        Form1.UpdateDisplay()


    End Sub
End Class
