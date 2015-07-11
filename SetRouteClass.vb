Imports System.Data.SqlServerCe
Imports System.Threading


Public Class SetRouteClass

    Private RouteId As String
    Private routelist As New ArrayList()
    Private subRouteList As New ArrayList()
    Private followingRouteList As New ArrayList()
    Private pointsSetList As New ArrayList()
    Private pointsReverseList As New ArrayList()
    Private pointsLockList As New ArrayList()
    ' Private precedingRouteList As New ArrayList()
    Private routeIsSet As Boolean = False
    Private subSidRoute As Boolean = True
    Public subSidSet As Boolean = False
    Public routeIsClear As Boolean = False
    Private exitNode As String
    Private entryNode As String
    Public entryNodeId As Integer
    Private setClear As Boolean = False
    Private routeHasOccupiedTrack As Boolean = False
    Private trackOccupied As Boolean
    Private m_parent As Form1
    Private auto As Boolean = False


    Public Sub New(ByVal setm_parent As Form1, ByVal setAuto As Boolean)
        RouteId = "none"
        routelist = New ArrayList()
        subRouteList = New ArrayList()
        followingRouteList = New ArrayList
        pointsLockList = New ArrayList
        pointsSetList = New ArrayList
        pointsReverseList = New ArrayList
        auto = setAuto
        m_parent = setm_parent
        'precedingRouteList = New ArrayList


    End Sub


    Private Sub UpdateRoute(ByVal routeID As String)
        Dim signalId As String
        Dim signalIndex As Integer
        Dim mypen As Pen
        Dim signalStatus As Integer
        signalId = GetEntrySignal(routeID)
        signalIndex = Form1.GetNodeIndex(signalId)
        mypen = New Pen(Color:=Color.Green, Width:=4)
            mypen.Color = Color.LawnGreen
        'Form1.DrawSignal(mypen, signalIndex)
        If setClear Then
            Dim exitSignalStatus As Integer
            exitSignalStatus = Form1.GetSignalStatus(exitNode)
            Form1.UpdateSignal(signalIndex, 1, exitSignalStatus + 1)
            'Console.WriteLine("Cancel Route " & routeID & " start " & signalId & " end " & exitNode & " status " & exitSignalStatus)

        Else
            Dim exitSignalStatus As Integer
            exitSignalStatus = Form1.GetSignalStatus(exitNode)
            Form1.UpdateSignal(signalIndex, 1, exitSignalStatus + 1)

            'Console.WriteLine("set Route " & routeID & " start " & signalId & " end " & exitNode & " status " & exitSignalStatus)

            'Form1.UpdateSignal(signalIndex, 0, 2)
        End If
        signalStatus = Form1.GetSignalStatus(signalId)

        If signalStatus > 0 Then
            UpdateOtherRoutes(signalId)
        End If
    End Sub


    Private Sub ReversePoints()
        For Each point In pointsReverseList
            If m_parent.nodeArrayList(m_parent.GetNodeIndex(point)).GetPointSetReverse = True Then
                'Point isn't already reversed
                ' Form1.ReversePoint(Form1.GetNodeIndex(point))
                'm_parent.UnLockPoint(m_parent.GetNodeIndex(point))
                ' m_parent.AddPointToBlankList(m_parent.GetNodeIndex(point))

                Dim SetPointDelayobj As New SetPointClass(m_parent, point, False)
            End If
        Next

    End Sub



    Private Sub SetPoints()
        For Each point In pointsSetList
            If m_parent.nodeArrayList(m_parent.GetNodeIndex(point)).GetPointSetReverse = False Then
                'only do anything if point not set the right way
                'm_parent.AddPointToBlankList(m_parent.GetNodeIndex(point))
                'm_parent.UnLockPoint(m_parent.GetNodeIndex(point))
                Console.WriteLine("Add " & point)
                Dim SetPointDelayobj As New SetPointClass(m_parent, point, True)
            End If


            'Form1.SetPoint(Form1.GetNodeIndex(point))
        Next
        ReversePoints()
        'Thread.Sleep(10000)
    End Sub

    Private Sub UpdateOtherRoutes(ByVal setEntryNode As String)
        Dim precedingRouteList As New ArrayList()
        precedingRouteList = New ArrayList

        GetPrecdedingRoute(setEntryNode, precedingRouteList)
        CheckPrecedingRoute(precedingRouteList)

    End Sub
    Public Sub SetRoute()
        Dim mypen As Pen
        setClear = False
        mypen = New Pen(Color:=Color.Green, Width:=4)
        If CheckFollowingRoute() Then
            'following route is set
            Dim exitSignalStatus As Integer
            exitSignalStatus = Form1.GetSignalStatus(exitNode)
            Form1.UpdateSignal(entryNodeId, 1, exitSignalStatus + 1)
        Else
            'otherwise it's not set, so set it to 1
            Form1.UpdateSignal(entryNodeId, 1, 1)
        End If

        UpdateOtherRoutes(entryNode)

    End Sub

    Public Sub ClearRoute()
        setClear = True
        'Console.WriteLine("clear signal " & entryNodeId)
        Form1.UpdateSignal(entryNodeId, 1, 0)
        UpdateOtherRoutes(entryNode)
        Form1.UpdateDisplay()
    End Sub

    Public Function CheckRouteExists(ByVal fromNodeId As String, _
                                ByVal toNodeId As String) As Boolean

        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)

        exitNode = toNodeId
        entryNode = fromNodeId

        Try
            routesdfDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route sdf dBconnection")
            Return False
        End Try


        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [routedB] where node = '" & _
            fromNodeId & "' AND nodeTo = '" & toNodeId & "'"

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "routedB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("routedB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader
        'sdfRouteReader.
        Dim route As String = Nothing


        subRouteList.Clear()

        While sdfRouteReader.Read
            route = sdfRouteReader(0).ToString
            routelist.Add(route)
        End While
        If routelist.Count > 0 Then
            Return True
        Else
            Return (False)
        End If


    End Function

    Public Function CheckRouteElementsSet() As Boolean
        Dim multipleRouteSet As Boolean = True
        subSidSet = False
        For Each routeID As String In routelist
            If multipleRouteSet Then
                routeIsSet = CheckRouteSet(routeID)
                 GetSubRoute(routeID)

                If routeIsSet Then
                    'Route is already set
                    For Each trackArrayIndex As Integer In subRouteList
                        Form1.TrackArrayList(trackArrayIndex).RouteClear()
                    Next
                    ClearRoute()
                    'routeIsClear = True
                    setRouteSetdBFlag(routeID, "0")
                    multipleRouteSet = False
                Else
                    'Check for a subsidiary route
                    If CheckSubSidSet(routeID) Then
                        'Clear subsid route up to occupied point
                        For Each trackArrayIndex As Integer In subRouteList
                            If (Form1.TrackArrayList(trackArrayIndex).getTrackoccupied = False) Then
                                Form1.TrackArrayList(trackArrayIndex).RouteClear()
                            Else
                                Exit For
                            End If
                        Next
                        ClearRoute()
                        setSubsidSetdBFlag(routeID, "0")
                        routeIsClear = True
                        Console.WriteLine("Set subsid route")
                        subSidSet = True
                    Else
                        'No Main or subsidiary route
                        If routeIsClear Then
                            If routeHasOccupiedTrack Then
                                If subSidRoute Then
                                    MsgBox("call on route")
                                    SetPoints()
                                    'Now we need to wait uptil the points are set

                                    For Each trackArrayIndex As Integer In subRouteList
                                        Console.WriteLine("track occupied ? " & trackArrayIndex & " is " & Form1.TrackArrayList(trackArrayIndex).getTrackoccupied)
                                        If (Form1.TrackArrayList(trackArrayIndex).getTrackoccupied = False) Then
                                            Form1.TrackArrayList(trackArrayIndex).RouteSet(routeID, entryNode, exitNode, entryNodeId)
                                        Else
                                            Exit For
                                        End If
                                    Next
                                    'subSidSet = True
                                    setSubsidSetdBFlag(routeID, "1")
                                    Form1.setSubsidSignalState(entryNodeId, True)

                                Else
                                    MsgBox("Route is occupied")

                                End If

                            Else
                                'Route is clear and can be set
                                SetPoints()
                                For Each trackArrayIndex As Integer In subRouteList
                                    Form1.TrackArrayList(trackArrayIndex).RouteClear()         
                                    Form1.TrackArrayList(trackArrayIndex).RoutePending(routeID, entryNode, exitNode, entryNodeId, auto)
                                Next
                                '9999999
                                Dim SetRoutePointsobj As New SetRoutePointsClass(m_parent, pointsSetList, _
                                                                                 pointsReverseList, subRouteList, _
                                                                                 routeID, entryNode, exitNode, _
                                                                                 entryNodeId, auto, Me)

                               

                                'For Each trackArrayIndex As Integer In subRouteList
                                'Form1.TrackArrayList(trackArrayIndex).RouteSet(routeID, entryNode, exitNode, entryNodeId, auto)
                                'Next
                                'setRouteSetdBFlag(routeID, "1")
                                'Form1.setSubsidSignalState(entryNodeId, False)

                                'Console.WriteLine("Set start" & routeID)

                                ''here oot
                                'multipleRouteSet = False
                                'routeIsClear = True
                            End If
                        End If
                    End If
                End If
            End If
        Next
    End Function



   

    Public Sub RouteOccupiedCheckRoute(ByVal fromNodeId As String, _
                                ByVal toNodeId As String)

        'Console.WriteLine("check tc cancelled route")
        If CheckRouteExists(fromNodeId, toNodeId) Then


            Dim multipleRouteSet As Boolean = True
            For Each routeID As String In routelist
                If multipleRouteSet Then
                    routeIsSet = CheckRouteSet(routeID)

                    GetSubRoute(routeID)

                    If routeIsSet Then
                        Console.WriteLine("tc has caused route " & routeID & " to clear")
                        ClearRoute()
                        'routeIsClear = True
                        setRouteSetdBFlag(routeID, "0")
                        multipleRouteSet = False



                    End If
                End If
            Next
        End If
    End Sub


    Private Sub accessTrack(ByVal nodeFrom As String, ByVal nodeTo As String, ByVal currentRoute As String, ByVal requires As String)
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)
        Try
            routesdfDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route sdf dBconnection")

        End Try


        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [trackdB] where (node = '" & _
              nodeFrom & "' AND nodeto = '" & nodeTo & "' ) OR ( node = '" & _
          nodeTo & "' AND nodeto = '" & nodeFrom & "' ) "

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "trackDB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("TrackDB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader

        ' routeIsClear = True
        Dim route As String = Nothing
        Dim node As String
        Dim nodeID As String
        Dim tc As String
        Dim trackArrayIndex As Integer
        Dim trackStatus As Integer
        Dim trackID As Integer
        trackOccupied = False
        While sdfRouteReader.Read
            route = sdfRouteReader(0).ToString
            node = sdfRouteReader(1).ToString
            nodeID = sdfRouteReader(2).ToString
            tc = sdfRouteReader(4).ToString
            trackID = sdfRouteReader(9).ToString

            'we want to check the track circuits for all the
            'tracks in the route, but only draw the routes on the subroute list


            For tcCount As Integer = 0 To (Form1.trackCircuitArray(tc).Count - 1)
                trackArrayIndex = Form1.trackCircuitArray(tc)(tcCount)
                trackStatus = Form1.TrackArrayList(trackArrayIndex).GetTrackStatus()
                If (Form1.TrackArrayList(trackArrayIndex).GetTrackOccupied()) Then

                    trackOccupied = True
                    ' Console.WriteLine("Track " & trackID & " is occupied")
                End If




                subRouteList.Add(trackID)

                If trackStatus > 0 Then
                    'subroute is occupied
                    ' Console.WriteLine("Can't set Route " & currentRoute & " as " & tc & " is already set")
                    routeIsClear = False
                End If
            Next
            'accessTrack(node, nodeID)
            'routelist.Add(route)
        End While
    End Sub


    Private Function GetEntrySignal(ByVal routeID As String) As String


        Dim sqlACTDetailsSelect As String = "SELECT  " & _
                     "routedB.node FROM routedB " & _
                       "WHERE (routedB.route = '" & _
                     routeID & "')"
        Dim ACTOccElements As New ArrayList

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder
        Dim nofActions As Integer = 0
        Dim nofDRs As Integer = 0
        Dim DetachIndex As Integer = 0
        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        'Console.WriteLine(sConnection.ConnectionString)
        Dim objConn As New SqlCeConnection(sConnection.ConnectionString)

        Try
            objConn.Open()
        Catch ex As Exception
            MessageBox.Show("can't open connection")
        End Try

        Dim dr As SqlCeDataReader

        Dim TrainTabledaDetailsSelectCommand As New SqlCeCommand(sqlACTDetailsSelect, objConn)
        dr = TrainTabledaDetailsSelectCommand.ExecuteReader()
        Dim entrySignal As String = 0
        While (dr.Read())
            entrySignal = dr(0).ToString
        End While
       
        Return entrySignal
    End Function


    Private Function CheckRouteSet(ByVal routeID As String) As Boolean


        Dim sqlACTDetailsSelect As String = "SELECT  " & _
                     "routedB.isSet FROM routedB " & _
                       "WHERE (routedB.route = '" & _
                     routeID & "')"
        Dim ACTOccElements As New ArrayList

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder
        Dim nofActions As Integer = 0
        Dim nofDRs As Integer = 0
        Dim DetachIndex As Integer = 0
        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        ' Console.WriteLine(sConnection.ConnectionString)
        Dim objConn As New SqlCeConnection(sConnection.ConnectionString)

        Try
            objConn.Open()
        Catch ex As Exception
            MessageBox.Show("can't open connection")
        End Try

        Dim dr As SqlCeDataReader

        Dim TrainTabledaDetailsSelectCommand As New SqlCeCommand(sqlACTDetailsSelect, objConn)
        dr = TrainTabledaDetailsSelectCommand.ExecuteReader()
        Dim isSet As String = 0
        While (dr.Read())
            isSet = dr(0).ToString
        End While
        If isSet = "1" Then
            Return True
        Else
            Return False
        End If
        objConn.Close()
    End Function

    Private Function CheckSubSidSet(ByVal routeID As String) As Boolean


        Dim sqlACTDetailsSelect As String = "SELECT  " & _
                     "routedB.subsidSet FROM routedB " & _
                       "WHERE (routedB.route = '" & _
                     routeID & "')"
        Dim ACTOccElements As New ArrayList

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder
        Dim nofActions As Integer = 0
        Dim nofDRs As Integer = 0
        Dim DetachIndex As Integer = 0
        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        ' Console.WriteLine(sConnection.ConnectionString)
        Dim objConn As New SqlCeConnection(sConnection.ConnectionString)

        Try
            objConn.Open()
        Catch ex As Exception
            MessageBox.Show("can't open connection")
        End Try

        Dim dr As SqlCeDataReader

        Dim TrainTabledaDetailsSelectCommand As New SqlCeCommand(sqlACTDetailsSelect, objConn)
        dr = TrainTabledaDetailsSelectCommand.ExecuteReader()
        Dim isSet As String = 0
        While (dr.Read())
            isSet = dr(0).ToString
        End While
        If isSet = "1" Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub GetSubRoute(ByVal routeID As String)


        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)
        Try
            routesdfDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route sdf dBconnection")

        End Try


        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [subroutedB] where route = '" & _
              routeID & "'"
        Console.WriteLine("SELECT * FROM [subroutedB] where route = '" & _
                       routeID & "'")

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "subroutedB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("subroutedB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader
        'sdfRouteReader.
        Dim route As String = Nothing
        Dim node As String
        Dim nodeID As String
        Dim requires As String
        routehasOccupiedtrack = False
        While sdfRouteReader.Read
            route = sdfRouteReader(0).ToString
            node = sdfRouteReader(1).ToString
            nodeID = sdfRouteReader(2).ToString
            requires = sdfRouteReader(3).ToString
            routeID = route
            accessTrack(node, nodeID, route, requires)
            If trackOccupied Then
                routeHasOccupiedTrack = True
            End If
            If requires = "R" Then
                If m_parent.nodeArrayList(m_parent.GetNodeIndex(node)).getNodeType = "P" Then
                    If Not pointsReverseList.Contains(node) Then
                        'Add the point to the reverse list
                        'But don't add it if it's already on
                        pointsReverseList.Add(node)
                    End If
                    pointsSetList.Remove(node)
                    'remove it if it's been on the set list
                End If
                If m_parent.nodeArrayList(m_parent.GetNodeIndex(nodeID)).getNodeType = "P" Then
                    If Not pointsReverseList.Contains(nodeID) Then
                        'Add the point to the reverse list
                        'But don't add it if it's already on
                        pointsReverseList.Add(nodeID)
                    End If
                End If

            Else
                If m_parent.nodeArrayList(m_parent.GetNodeIndex(node)).getNodeType = "P" Then
                    If Not pointsReverseList.Contains(node) Then
                        'If it's on the reverse list, don't add it to the set list
                        If Not pointsSetList.Contains(node) Then
                            'don't add it if it's already on the list
                            pointsSetList.Add(node)
                        End If
                    End If
                End If
                If m_parent.nodeArrayList(m_parent.GetNodeIndex(nodeID)).getNodeType = "P" Then
                    If Not pointsReverseList.Contains(nodeID) Then
                        'If it's on the reverse list, don't add it to the set list
                        If Not pointsSetList.Contains(nodeID) Then
                            'don't add it if it's already on the list
                            pointsSetList.Add(nodeID)
                        End If
                    End If
                End If
            End If
            pointsLockList.Add(node)
            '   Console.WriteLine(" Node from " & node & " to type " & _
            '                    m_parent.GetNodeDevTo(node) & " from type " & _
            '                    m_parent.GetNodeDevFrom(node) & " to type " & _
            '                    m_parent.GetNodeDevTo(nodeID) & " from id type " & _
            '                    m_parent.GetNodeDevFrom(nodeID) & " from id type " & _
            '                    " to " & nodeID & " route " & route & " requires " & requires)
            'routelist.Add(route)
        End While

        ArraySort()

    End Sub


    Private Sub GetSetReverse(ByVal nodeFrom As String, ByVal nodeTo As String, _
                              ByVal normalReverse As String)

        'Get Type
        If m_parent.GetNodeType(nodeFrom) = "P" Then
            'this is a point
        End If
    End Sub
    Private Sub ArraySort()

        For Each loopnodeid In pointsReverseList
            Dim countIndex As Integer
            For countIndex = 0 To (pointsSetList.Count - 1)
                If pointsSetList(countIndex) = loopnodeid Then
                    pointsSetList.RemoveAt(countIndex)
                    pointsSetList.Add(Nothing)
                End If
            Next
        Next
    End Sub

    Private Sub GetFollowingRoute(ByVal exitNode As String)

        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)
        Try
            routesdfDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route sdf dBconnection")

        End Try


        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [routedB] where node = '" & _
              exitNode & "'"

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "subroutedB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("subroutedB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader
        'sdfRouteReader.
        Dim route As String = Nothing
        Dim node As String
        Dim nodeID As String
        While sdfRouteReader.Read
            route = sdfRouteReader(0).ToString
            node = sdfRouteReader(1).ToString
            nodeID = sdfRouteReader(2).ToString
            routeID = route
            'accessTrack(node, nodeID, route)
            followingRouteList.Add(route)
        End While



    End Sub

    Private Sub GetPrecdedingRoute(ByVal entryNode As String, ByRef precedingRouteList As ArrayList)


        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)
        Try
            routesdfDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route sdf dBconnection")

        End Try
        exitNode = entryNode
        precedingRouteList.Clear()
        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [routedB] where nodeto = '" & _
              entryNode & "'"

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "subroutedB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("subroutedB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader
        'sdfRouteReader.
        Dim route As String = Nothing
        Dim node As String
        Dim nodeID As String
        While sdfRouteReader.Read
            route = sdfRouteReader(0).ToString
            node = sdfRouteReader(1).ToString
            nodeID = sdfRouteReader(2).ToString
            RouteId = route
            'accessTrack(node, nodeID, route)
            precedingRouteList.Add(route)
        End While



    End Sub




    Public Sub setRouteSetdBFlag(ByVal routeid As String, ByVal flag As Integer)
        'Setup connection to new sdf database
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)

        Try
            routesdfDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route sdf dBconnection")
        End Try

        Dim sqlRouteDataUpdate As String = "UPDATE routedB " & _
                                 "SET routedB.isSet = '" & _
                                 flag & _
                                 "' WHERE (routedB.route = '" & _
                                 routeid & _
                                 "')"
        Dim routeTabledaUpdateCommand As New SqlCeCommand(sqlRouteDataUpdate, routesdfDbConn)
        routeTabledaUpdateCommand.ExecuteNonQuery()
    End Sub

    Private Sub setSubsidSetdBFlag(ByVal routeid As String, ByVal flag As Integer)
        'Setup connection to new sdf database
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)

        Try
            routesdfDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route sdf dBconnection")
        End Try

        Dim sqlRouteDataUpdate As String = "UPDATE routedB " & _
                                 "SET routedB.subsidSet = '" & _
                                 flag & _
                                 "' WHERE (routedB.route = '" & _
                                 routeid & _
                                 "')"
        Dim routeTabledaUpdateCommand As New SqlCeCommand(sqlRouteDataUpdate, routesdfDbConn)
        routeTabledaUpdateCommand.ExecuteNonQuery()
    End Sub

    Public Sub CheckPrecedingRoute(ByRef precedingRouteList As ArrayList)
        Dim precedingRouteisClear As Boolean
        Dim precedingRouteIsSet As Boolean = False
        For Each selectedRoute As String In precedingRouteList
            precedingRouteisClear = True

            If CheckRouteSet(selectedRoute) Then
                UpdateRoute(selectedRoute)
                precedingRouteIsSet = True
            End If
            'CheckRouteElementsSet()
        Next

    End Sub

    Public Function CheckFollowingRoute()
        Dim followingRouteisClear As Boolean
        Dim followingRouteIsSet As Boolean = False


        If Form1.GetNodeIsFringe(exitNode) Then
            followingRouteIsSet = True
            followingRouteisClear = True

        Else
            GetFollowingRoute(exitNode)
            For Each selectedRoute As String In followingRouteList
                followingRouteisClear = True

                If CheckRouteSet(selectedRoute) Then
                    followingRouteIsSet = True
                End If
                'CheckRouteElementsSet()
            Next
        End If

        Return followingRouteIsSet
    End Function


    Public Function CheckRoute(ByVal fromNodeId As String, _
                                ByVal toNodeId As String) As Boolean

        If CheckRouteExists(fromNodeId, toNodeId) Then
            ' For Each selectedRoute As String In routelist
            routeIsClear = True
            'routeIsSet = CheckRouteSet(selectedRoute)
            CheckRouteElementsSet()
             If routeIsClear Then
            Else
                If setClear = False Then
                    MsgBox("Route is Not Clear")
                End If

                End If
                Return True
        Else
                Return False
        End If

    End Function


   


End Class
