Imports System.Data.OleDb
Imports System.IO
Imports System.Data.SqlServerCe
Imports System.Drawing
Imports System.Threading
Imports System.Math
Imports DBFReader.WinUsbDemo





Public Class Form1
    Private nodeArray(10) As NodeClass
    Private startRoute As Boolean = False
    Private routeExists As Boolean = False
    Private startNodeId As String
    Private endNodeId As String
    Private cabArrayList As New ArrayList
    Private pointBlankList As New ArrayList
    Private pointBlankPen As New Pen(Color.Black)

    Public trackCircuitArray(10) As ArrayList
    Public trackCircuitCountArray(10) As Integer
    Public trackIDArray(10) As Integer
    Private startNodeIndex As Integer
    Private currentTime As Integer
    Private linespeedObj As linespeedfrm
    Private wttObj As wttFrm
    Private TrainInfoObj As TrainInfoFrm
    Private flashObj As New FlashPointBlankClass(Me)

    Private TrackCircuitboxArrayList As New ArrayList
    Public nodeArrayList As New ArrayList()
    Public TrackArrayList As New ArrayList()
    Public LeverList(10) As Integer
    Public FlashingNodeArrayList As New ArrayList()
    Public FlashingTRTSArrayList As New ArrayList()
    Dim xpos As Integer = 100
    Dim ypos As Integer = 100
    Public myGraphics As Graphics
    Public myGraphics_pointblank_layer As Graphics
    Public myGraphics_layer1 As Graphics
    Public myGraphicsSignalLayer As Graphics
    Public myGraphicsLabelLayer As Graphics

    Dim myPen As New Pen(Color.Blue)
    Dim myredPen As New Pen(Color.Red)
    Private startx As Integer = 0
    Private starty As Integer = 0
    Private endx As Integer = 10
    Private endy As Integer = 100
    Private displayWidth As Integer = 1000
    Private displayHeight As Integer = 100
    Private displayDrawn As Boolean = False
    Public lineArray(10) As linetype
    Public platformArrayList As ArrayList

    Structure termDetailsType
        Dim location As Integer
        Dim platform As String
        Dim distance As String
        Dim direction As String
        Dim exitNode As String
        Dim occupiedTCs As ArrayList
        Dim occupiedDistances As ArrayList
    End Structure

    Structure platformType
        Dim nodeFrom As String
        Dim nodeto As String
        Dim platform As String
        Dim platType As String
        Dim full As Boolean
        Dim location As String
        Dim platXstart As String
        Dim platYstart As String
        Dim platYheight As String
        Dim platXwidth As String

    End Structure


    Public Structure linetype
        Dim nodeIn As String
        Dim nodeOut As String
        Dim location As String
        Dim description As String
        Dim direction As String

    End Structure

    Public Event GotTrackLength(ByVal trackLength As Integer)


    'Delegates here
    Delegate Sub AcceptSignalStatusCallbackDelegate(ByVal aThreadName As String,
                                             ByVal signalStatus As String)



    Public Sub SetCurrentTime(ByVal aThreadName As String, ByVal aTextBox As Label, ByVal newText As Integer)

        Dim currentEvent As String
        Dim trainIndex As String
        Dim notermDetails As termDetailsType
        Dim nofEvents As Integer = 0

        Dim timeSecs As Integer
        Dim timeMins As Integer
        Dim timeHours As Integer


        currentEvent = 0
        Me.currentTime = newText
        timeSecs = newText
        timeHours = DivRem(timeSecs, 3600, timeSecs)
        timeMins = DivRem(timeSecs, 60, timeSecs)


        Me.ClockDisplayLabel.Text = timeHours & ":" & timeMins & ":" & timeSecs
        wttObj.GetWTTEventFromTime(currentTime, nofEvents, currentEvent, trainIndex)
        If currentEvent <> "0" Then

            Console.WriteLine("event is " & currentEvent & " at " & currentTime)
            StartTrain(currentEvent, trainIndex, notermDetails)

        End If

    End Sub

    Public Sub GetTrackLength(ByVal trackId As Integer)
        Dim trackLength As Integer
        trackLength = TrackArrayList(trackId).trackLength
        RaiseEvent GotTrackLength(trackLength)
    End Sub

    Private Sub PopulateTCArray()
        Dim thisTrackCircuit As Integer
        Dim thisTrackID As Integer
        For trackID As Integer = 0 To (TrackArrayList.Count - 1)
            'get the track circuit for the track we're interested in
            thisTrackCircuit = TrackArrayList(trackID).tc
            'add this track to the list of tracks controlled by this track circuit
            trackCircuitArray(thisTrackCircuit).Add(trackID)
            'get the track ID of this track, for cross referencing
            thisTrackID = TrackArrayList(trackID).trackID
            'set the entry in trackIDArray to be this trackID
            trackIDArray(thisTrackID) = trackID

        Next
    End Sub




    Private Sub DrawTrack(ByVal fromXpos As Integer, _
                            ByVal fromYpos As Integer, _
                            ByVal toXpos As Integer, _
                            ByVal toYpos As Integer)
        'local scope
        Dim myPen As Pen
        'check to see if we're drawing a diagonal
        If (toXpos > fromXpos) Then
            If (toYpos > fromYpos) Then
                myPen = New Pen(Color:=Color.Blue, Width:=4)


            End If
        End If
        'instantiate a new pen object using the color structure
        myPen = New Pen(Color:=Color.Blue, Width:=4)

        'draw the line on the form using the pen object
        myGraphics.DrawLine(pen:=myPen, x1:=fromXpos, y1:=fromYpos, x2:=toXpos, y2:=toYpos)

        'UpdateDisplay()
        'Refresh()


    End Sub

    Public Sub DrawSignalPost(ByVal thePen As Pen, ByVal xCentre As Integer, _
                          ByVal yCentre As Integer, ByVal runDirection As Integer)
        Dim myweePen As Pen
        myweePen = New Pen(Color:=thePen.Color, Width:=1)
        Dim xStart As Integer
        Dim ystart As Integer
        Dim xPoint1 As Integer
        Dim xPoint2 As Integer
        Dim yPoint1 As Integer
        Dim yPoint2 As Integer

        If runDirection < 0 Then
            xStart = xCentre
            ystart = yCentre - 3
            xPoint1 = xCentre
            yPoint1 = yCentre + 7
            xPoint2 = xCentre - 7
            yPoint2 = yCentre + 7

        Else
            xStart = xCentre
            ystart = yCentre - 3
            xPoint1 = xCentre
            yPoint1 = yCentre - 8
            xPoint2 = xCentre + 8
            yPoint2 = yCentre - 8

        End If
        myGraphicsSignalLayer.DrawLine(pen:=myweePen, x1:=xStart, y1:=ystart, x2:=xPoint1, y2:=yPoint1)
        myGraphicsSignalLayer.DrawLine(pen:=myweePen, x1:=xPoint1, y1:=yPoint1, x2:=xPoint2, y2:=yPoint2)


    End Sub

    Public Sub setSubsidSignalState(ByVal NodeIndex As Integer, ByVal state As Boolean)

        nodeArrayList(NodeIndex).subsidSignalSet = state


    End Sub


    Public Sub DrawTRTS(ByVal nodeIndex As Integer, ByVal isFlash As Boolean)

        Dim xCentre As Integer
        Dim ycentre As Integer
        Dim runDirection As Integer

        xCentre = nodeArrayList(nodeIndex).p2NodeXpos
        ycentre = nodeArrayList(nodeIndex).p2NodeYpos
        runDirection = nodeArrayList(nodeIndex).rundirection

        Dim myGraySignalPen As Pen
        Dim myYellowSignalPen As Pen
        Dim myBlackSignalPen As Pen
        myGraySignalPen = New Pen(Color:=Color.Gray, Width:=1)
        myBlackSignalPen = New Pen(Color:=Color.Black, Width:=3)
        myYellowSignalPen = New Pen(Color:=Color.Yellow, Width:=3)
        If isFlash Then
            DrawCircle(myYellowSignalPen, xCentre, ycentre)
        Else
            DrawCircle(myBlackSignalPen, xCentre, ycentre)
            DrawCircle(myGraySignalPen, xCentre, ycentre)

        End If

        UpdateDisplay()
    End Sub

    Public Sub FlashTRTS(ByVal nodeIndex As Integer, ByVal clsTraverserouteobj As clsTraverseRoute)
        Dim FlashTRTSObj As FlashTRTSClass
        FlashTRTSObj = New FlashTRTSClass
        FlashTRTSObj.m_parent = Me
        FlashTRTSObj.nodeindex = nodeIndex
        FlashingTRTSArrayList.Add(FlashTRTSObj)
        clsTraverserouteobj.flashTRTSIndex = FlashingTRTSArrayList.Count - 1
    End Sub

    Public Sub DrawSignal(ByVal thePen As Pen, ByVal nodeIndex As Integer)

        Dim xCentre As Integer
        Dim ycentre As Integer
        Dim runDirection As Integer

        xCentre = nodeArrayList(nodeIndex).mxcentre
        ycentre = nodeArrayList(nodeIndex).mycentre
        runDirection = nodeArrayList(nodeIndex).rundirection

        Dim myGraySignalPen As Pen
        myGraySignalPen = New Pen(Color:=Color.Gray, Width:=1)
        thePen.Width = 4
        DrawSignalPost(myGraySignalPen, xCentre, ycentre, runDirection)
        Dim signalIs As String
        Dim sigXCentre As Integer
        Dim sigYCentre As Integer
        signalIs = nodeArrayList(nodeIndex).nodeId
        'Console.WriteLine("Drawing signal " & signalIs)
        sigXCentre = xCentre + (runDirection * 11)
        sigYCentre = ycentre - 2 - (runDirection * 8)
        DrawCircle(thePen, sigXCentre, sigYCentre)


    End Sub

    Public Sub DrawSignal(ByVal thePen As Pen, ByVal nodeId As String)

        Dim xCentre As Integer
        Dim ycentre As Integer
        Dim runDirection As Integer
        Dim nodeIndex As Integer
        nodeIndex = GetNodeIndex(nodeId)

        xCentre = nodeArrayList(nodeIndex).mxcentre
        ycentre = nodeArrayList(nodeIndex).mycentre
        runDirection = nodeArrayList(nodeIndex).rundirection

        Dim myGraySignalPen As Pen
        myGraySignalPen = New Pen(Color:=Color.Gray, Width:=1)
        thePen.Width = 4

        DrawSignalPost(myGraySignalPen, xCentre, ycentre, runDirection)
        If (thePen.Color = Color.Red) Then
            ' Console.WriteLine("set signal " & nodeIndex & " to be red")
        Else
            ' Console.WriteLine("set signal " & nodeIndex & " ")

        End If
        Dim sigXCentre As Integer
        Dim sigYCentre As Integer
        sigXCentre = xCentre + (runDirection * 11)
        sigYCentre = ycentre - 2 - (runDirection * 8)
        DrawCircle(thePen, sigXCentre, sigYCentre)


    End Sub

    Public Sub DrawNode(ByVal thePen As Pen, ByVal xCentre As Integer, _
                           ByVal yCentre As Integer)
        'local scope
        'Dim mXcentre = xCentre - 3
        'Dim mYcentre = yCentre - 3

        'draw the line on the form using the pen object
        Dim myrectangle As Rectangle
        myrectangle = New Rectangle()
        myGraphics.DrawRectangle(pen:=thePen, x:=xCentre, y:=yCentre, width:=4, height:=4)


    End Sub
    'Private Sub getNodeCoords(ByVal nodeId As String, _
    '                           ByRef nodeXpos As Integer, _
    '                           ByRef nodeypos As Integer)

    '    ' open connection to node database


    '    Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

    '    sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
    '    Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

    '    Try
    '        nodeDbConn.Open()
    '    Catch ex As Exception

    '        Console.WriteLine("can't open Node dBconnection")
    '    End Try

    '    Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
    '    Dim sqlSelectNodeTable As String = "SELECT nodeXpos, nodeYpos  FROM [nodedB] " & _
    '        "WHERE (nodeId = '" & nodeId & "')"
    '    Dim SelectCommand As SqlCeCommand

    '    SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
    '    ' Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(sqlSelectNodeTable, nodeDbConn)

    '    Dim nodeReader As SqlCeDataReader

    '    Dim myConnection As SqlCeConnection
    '    Dim mycommand As SqlCeCommand

    '    mycommand = New SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
    '    nodeReader = mycommand.ExecuteReader


    '    While nodeReader.Read
    '        nodeypos = nodeReader(0).ToString
    '        nodeXpos = nodeReader(1).ToString

    '    End While




    '    ' nodeXpos = 100
    '    'nodeypos = 200
    '    nodeDbConn.Close()

    'End Sub



    Private Sub Form1_Mousedown(ByVal sender As System.Object, ByVal e As  _
System.Windows.Forms.MouseEventArgs) Handles DisplayWindow.MouseMove
        TextBox1.Text = "Mouse down at" + CStr(e.X + HScrollBar1.Value) + " :" + CStr(e.Y + VScrollBar1.Value)

    End Sub

    Sub DrawCircle(ByVal thePen As Pen, ByVal Xpos As Integer, ByVal Ypos As Integer)
        Dim radius As Integer = 2
        Dim rect As Rectangle = New Rectangle(Xpos - 1, Ypos - 1, 2 * radius, 2 * radius)
        myGraphicsSignalLayer.DrawEllipse(thePen, rect)

    End Sub

    Private Sub KillFlashingNode(ByVal flashingNodeIndex As Integer)

        FlashingNodeArrayList(flashingNodeIndex).flashing = False
        FlashingNodeArrayList.RemoveAt(flashingNodeIndex)
    End Sub

    Public Sub KillFlashingTRTS(ByVal flashingTRTSIndex As Integer)

        FlashingTRTSArrayList(flashingTRTSIndex).flashing = False
        FlashingTRTSArrayList.RemoveAt(flashingTRTSIndex)
    End Sub

    Private Sub FlashNode(ByVal nodeIndex As Integer)
        Dim FlashNodeObj As FlashnodeClass
        FlashNodeObj = New FlashnodeClass
        FlashNodeObj.m_parent = Me
        FlashNodeObj.nodeindex = nodeIndex
        FlashingNodeArrayList.Add(FlashNodeObj)



    End Sub

    Public Sub SetNodePointBlank(ByVal nodeID As String)
        Console.WriteLine("set point blank " & nodeID)
        nodeArrayList(GetNodeIndex(nodeID)).BlankPoint(pointBlankPen)
    End Sub

    Public Function GetSignalStatus(ByVal nodeID As String) As Integer
        If nodeArrayList(GetNodeIndex(nodeID)).NodeType = "F" Then
            Return 3
        Else
            Return nodeArrayList(GetNodeIndex(nodeID)).signalState
        End If

    End Function

    Public Function GetnodeIsFringe(ByVal nodeID As String) As Boolean
        If nodeArrayList(GetNodeIndex(nodeID)).NodeType = "F" Then
            Return True
        Else
            Return False
        End If

    End Function


    Public Function GetPointSetReverse(ByVal NodeId As String) As String
        Dim nodeIndex As Integer
        nodeIndex = GetNodeIndex(NodeId)
        Return nodeArrayList(nodeIndex).pointSetReverse
    End Function

    Public Function GetNodeType(ByVal NodeId As String) As String
        Dim nodeIndex As Integer
        nodeIndex = GetNodeIndex(NodeId)
        Return nodeArrayList(nodeIndex).nodeType
    End Function

    Public Function GetNodeDevTo(ByVal NodeId As String) As Byte
        Dim nodeIndex As Integer
        nodeIndex = GetNodeIndex(NodeId)
        Return nodeArrayList(nodeIndex).devTo
    End Function

    Public Function GetNodeDevFrom(ByVal NodeId As String) As Byte
        Dim nodeIndex As Integer
        nodeIndex = GetNodeIndex(NodeId)
        Return nodeArrayList(nodeIndex).devfrom
    End Function


    Public Function GetNodeIndex(ByVal NodeID As String) As Integer
        For Each nodeIndex As NodeClass In nodeArrayList
            If nodeIndex.nodeId = NodeID Then
                Return nodeIndex.nodeIndex
            End If
        Next


    End Function

    Public Sub LockPoint(ByVal nodeIndex As Integer)
        nodeArrayList(nodeIndex).LockPoint()
    End Sub
    Public Sub LockPoint(ByVal nodeId As String)
        nodeArrayList(GetNodeIndex(nodeId)).LockPoint()
    End Sub

    Public Sub UnLockPoint(ByVal nodeIndex As Integer)
        nodeArrayList(nodeIndex).UnLockPoint()
    End Sub
    Public Sub UnLockPoint(ByVal nodeId As String)
        nodeArrayList(GetNodeIndex(nodeId)).UnlockPoint()
    End Sub


    Public Sub SetPoint(ByVal nodeIndex As Integer)
        nodeArrayList(nodeIndex).setPoint()
        'Console.WriteLine("Set Point " & nodeIndex)
        'Thread.Sleep(500)
    End Sub
    Public Sub SetPoint(ByVal nodeId As String)
        nodeArrayList(GetNodeIndex(nodeId)).setPoint()
        'Console.WriteLine("Set Point " & GetNodeIndex(nodeId))
        'Thread.Sleep(500)
    End Sub

    Public Sub ReversePoint(ByVal nodeIndex As Integer)
        nodeArrayList(nodeIndex).ReversePoint()
    End Sub

    Public Sub ReversePoint(ByVal nodeId As String)
        nodeArrayList(GetNodeIndex(nodeId)).ReversePoint()
    End Sub

    Public Sub ReturnRouteSet(ByVal localSubRouteList As ArrayList, _
                             ByVal localRouteId As String, _
                             ByVal localEntryNode As String, _
                             ByVal localExitNode As String, _
                             ByVal localEntryNodeID As String, _
                             ByVal localAuto As Boolean, _
                             ByVal localRouteObj As SetRouteClass)

        For Each trackArrayIndex As Integer In localSubRouteList
            TrackArrayList(trackArrayIndex).RouteSet(localRouteId, localEntryNode, localExitNode, _
                                                           localEntryNodeID, localAuto)
        Next
        localRouteObj.setRouteSetdBFlag(localRouteId, "1")
        setSubsidSignalState(localEntryNodeID, False)
        'Set Route
        If localRouteObj.routeIsClear Then
            If localRouteObj.subSidSet Then
            Else
                localRouteObj.SetRoute()

            End If
        End If

        'localRouteObj.multipleRouteSet = False
        localRouteObj.routeIsClear = True
        UpdateDisplay()
    End Sub

    Public Sub UpdateSignal(ByVal nodeIndex As Integer, ByVal setMode As Integer, ByVal setState As Integer)

        nodeArrayList(nodeIndex).UpdateState(setMode, setState)
    End Sub



    Private Sub Form1_MouseMove(ByVal sender As System.Object, ByVal e As  _
System.Windows.Forms.MouseEventArgs) Handles DisplayWindow.MouseDown
        Dim lpntMouseLocation As Point
        Dim hitBox As Boolean
        ' If e.Button = MouseButtons.Left Then
        ' TextBox1.Text = "Mouse down at" + CStr(e.X) + " :" + CStr(e.Y)

        'If e.X = 100 Then
        'TextBox2.Text = "122"
        lpntMouseLocation = New Point(e.X + HScrollBar1.Value, e.Y + VScrollBar1.Value)
        If nodeArrayList.Count > 0 Then
            For Each nodeIndex As NodeClass In nodeArrayList
                If Not IsNothing(nodeIndex) Then
                    hitBox = nodeIndex.IsCollision(lpntMouseLocation)
                    If hitBox Then
                        'try the start of a route
                        If startRoute = False Then
                            startNodeId = nodeIndex.nodeId
                            startNodeIndex = nodeIndex.nodeIndex
                            If nodeArrayList(startNodeIndex).nodeType = "S" Then
                                FlashNode(startNodeIndex)
                                startRoute = True
                            Else
                                MsgBox("No route from " & startNodeId)
                            End If
                        Else
                            Dim auto As Boolean
                            If startNodeId = "S808" Then
                                auto = True
                            End If
                            endNodeId = nodeIndex.nodeId
                            Dim tempRoute As SetRouteClass
                            tempRoute = New SetRouteClass(Me, auto)
                            tempRoute.entryNodeId = startNodeIndex
                            'CheckRoute

                            If tempRoute.CheckRoute(startNodeId, endNodeId) Then
                                'Set Route
                                'If tempRoute.routeIsClear Then
                                'If tempRoute.subSidSet Then
                                'Else
                                'tempRoute.SetRoute()

                                'End If
                                'End If
                            Else
                            'No Route Exists, so pop massage box
                            MsgBox("No route exists from " & _
                                   startNodeId & " to " & _
                                   endNodeId)

                        End If
                        startRoute = False
                        KillFlashingNode(0)

                        End If

                    End If
                End If
            Next
        End If
    End Sub


    'Private Sub ImportTrackDBF()




    '    Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

    '    sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
    '    Dim trackDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)
    '    'nod  'create a rectangle based on x,y coordinates, width, & height

    '    'nodeDbConn.Open()

    '    Try
    '        trackDbConn.Open()
    '    Catch ex As Exception

    '        Console.WriteLine("can't open Track dBconnection")
    '    End Try

    '    Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
    '    Dim sqlSelectNodeTable As String = "SELECT * FROM [trackdB] "
    '    NodeTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, trackDbConn)

    '    Dim NodeTableds As New DataSet
    '    NodeTableda.Fill(NodeTableds, "nodedB")
    '    Dim NodeTabledt As DataTable = NodeTableds.Tables("nodedB")

    '    Dim insertNodeTable As String = "INSERT INTO nodedB " & _
    '                 "(nodeID, nodeType) " & _
    '                 "VALUES " & _
    '                 "(@nodeId,@nodeType)"




    '    'We want to open the FoxPro .dbf file, then copy the data into a database that we can 
    '    'use a bit more easily, and will probably be supported for a bit longer
    '    Dim FILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Leith\modeldata"


    '    Dim connectionString As String = "Provider = VFPOLEDB;" & _
    '              "Data Source=" & FILENAME & ";Collating Sequence=general;"
    '    'Create Connection
    '    Dim dBaseConnection As New System.Data.OleDb.OleDbConnection(connectionString)
    '    'Open connection to database

    '    Try
    '        dBaseConnection.Open()
    '    Catch ex As Exception
    '        MessageBox.Show("can't open connection")
    '        Console.WriteLine("can't open dBconnection")
    '    End Try

    '    Dim selectAllNodeString As String = "SELECT * FROM track "
    '    Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectAllNodeString, dBaseConnection)
    '    'dBaseCommandSelectAll.CommandText = CommandType.Text


    '    Dim NodeCount As Integer

    '    NodeCount = 0
    '    'Set up a reader for the DB


    '    Dim DBFTrackReader As OleDbDataReader = dBaseCommandSelectAll.ExecuteReader()


    '    Dim node As String
    '    Dim nodeto As String
    '    Dim line As Integer
    '    Dim maxSpeed As Integer
    '    Dim tc As Integer

    '    Try
    '        DBFTrackReader.Read()

    '    Catch ex As Exception

    '        MessageBox.Show("can't open connection")

    '    End Try
    '    Dim TrackTableda As New SqlServerCe.SqlCeDataAdapter
    '    Dim sqlSelecttrackTable As String = "SELECT * FROM [trackdb] "
    '    TrackTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelecttrackTable, trackDbConn)

    '    Dim TrackTableds As New DataSet
    '    TrackTableda.Fill(TrackTableds, "trackdB")
    '    Dim TrackTabledt As DataTable = TrackTableds.Tables("trackdB")

    '    Dim insertTrackTable As String = "INSERT INTO trackdB " & _
    '                 "(node,nodeto,tc,fromXpos,fromYpos,toXpos,toYpos) " & _
    '                 "VALUES " & _
    '                 "(@node,@nodeto,@tc,@fromXpos,@fromYpos,@toXpos,@toYpos)"





    '    TrackTabledt.NewRow()



    '    Dim newDataRow As DataRow = TrackTabledt.NewRow()
    '    newDataRow("node") = "tableNodeid"



    '    TrackTabledt.Rows.Add(newDataRow)

    '    While DBFTrackReader.Read
    '        node = DBFTrackReader(0).ToString
    '        nodeto = DBFTrackReader(1).ToString
    '        line = DBFTrackReader(2).ToString
    '        maxSpeed = DBFTrackReader(3).ToString
    '        tc = DBFTrackReader(4).ToString
    '        addDetailstoTrackDatabase(node, nodeto, line, maxSpeed, tc, TrackTabledt)
    '        NodeCount = NodeCount + 1
    '        'drawbox(inNodeRow, inNodeCol)
    '    End While
    '    DBFTrackReader.Close()




    '    Dim TrackTableInsertCmd As New SqlCeCommand(insertTrackTable, trackDbConn)
    '    TrackTableInsertCmd.Parameters.Add("@node", _
    '                                      SqlDbType.NVarChar, 20, "node")
    '    TrackTableInsertCmd.Parameters.Add("@nodeTo",
    '                                      SqlDbType.NVarChar, 20, "nodeTo")
    '    TrackTableInsertCmd.Parameters.Add("@tc", _
    '                                             SqlDbType.NVarChar, 20, "tc")
    '    TrackTableInsertCmd.Parameters.Add("@fromXpos", _
    '                                                     SqlDbType.NVarChar, 20, "fromXpos")
    '    TrackTableInsertCmd.Parameters.Add("@fromYpos", _
    '                                                     SqlDbType.NVarChar, 20, "fromYpos")
    '    TrackTableInsertCmd.Parameters.Add("@toXpos", _
    '                                                             SqlDbType.NVarChar, 20, "toXpos")
    '    TrackTableInsertCmd.Parameters.Add("@toYpos", _
    '                                                     SqlDbType.NVarChar, 20, "toYpos")


    '    TrackTableda.InsertCommand = TrackTableInsertCmd
    '    TrackTableda.Update(TrackTableds, "trackdB")
    '    'This populates the table with the node data



    'End Sub


    'Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    'Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

    '    Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

    '    sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
    '    Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)
    '    'nod  'create a rectangle based on x,y coordinates, width, & height

    '    'nodeDbConn.Open()

    '    Try
    '        nodeDbConn.Open()
    '    Catch ex As Exception

    '        Console.WriteLine("can't open Node dBconnection")
    '    End Try

    '    Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
    '    Dim sqlSelectNodeTable As String = "SELECT * FROM [nodedb] "
    '    NodeTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, nodeDbConn)

    '    Dim NodeTableds As New DataSet
    '    NodeTableda.Fill(NodeTableds, "nodedB")
    '    Dim NodeTabledt As DataTable = NodeTableds.Tables("nodedB")

    '    Dim insertNodeTable As String = "INSERT INTO nodedB " & _
    '                 "(nodeID, nodeType,nodeXpos, nodeYpos) " & _
    '                 "VALUES " & _
    '                 "(@nodeId,@nodeType,@nodeXpos,@nodeYpos)"




    '    'We want to open the FoxPro .dbf file, then copy the data into a database that we can 
    '    'use a bit more easily, and will probably be supported for a bit longer
    '    Dim FILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Leith\modeldata"


    '    Dim connectionString As String = "Provider = VFPOLEDB;" & _
    '         "Data Source=" & FILENAME & ";Collating Sequence=general;"
    '    'Create Connection
    '    Dim dBaseConnection As New System.Data.OleDb.OleDbConnection(connectionString)
    '    'Open connection to database

    '    Try
    '        dBaseConnection.Open()
    '    Catch ex As Exception
    '        MessageBox.Show("can't open connection")
    '        Console.WriteLine("can't open dBconnection")
    '    End Try

    '    Dim selectAllNodeString As String = "SELECT * FROM node "
    '    Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectAllNodeString, dBaseConnection)
    '    'dBaseCommandSelectAll.CommandText = CommandType.Text
    '    Dim NodeCount As Integer

    '    NodeCount = 0
    '    'Set up a reader for the DB


    '    Dim myNodeNodeIdReader As OleDbDataReader = dBaseCommandSelectAll.ExecuteReader()


    '    Dim nodeId As String
    '    Dim nodeType As String
    '    Dim inNodeRow As String
    '    Dim inNodeCol As String

    '    Try
    '        myNodeNodeIdReader.Read()

    '    Catch ex As Exception

    '        MessageBox.Show("can't open connection")

    '    End Try
    '    NodeTabledt.NewRow()



    '    Dim newDataRow As DataRow = NodeTabledt.NewRow()
    '    newDataRow("nodeid") = "tableNodeid"



    '    NodeTabledt.Rows.Add(newDataRow)

    '    While myNodeNodeIdReader.Read
    '        nodeId = myNodeNodeIdReader(0).ToString
    '        nodeType = myNodeNodeIdReader(1).ToString
    '        inNodeCol = (myNodeNodeIdReader(2).ToString) * 3
    '        inNodeRow = (myNodeNodeIdReader(3).ToString - 1000) * 3
    '        'addDetailstoNodeDatabase(NodeCount, nodeId, nodeType, inNodeRow, inNodeCol, NodeTabledt)
    '        NodeCount = NodeCount + 1
    '        drawbox(inNodeRow, inNodeCol)
    '    End While
    '    myNodeNodeIdReader.Close()




    '    Dim NodeTableInsertCmd As New SqlCeCommand(insertNodeTable, nodeDbConn)
    '    NodeTableInsertCmd.Parameters.Add("@nodeId", _
    '                                      SqlDbType.NVarChar, 20, "nodeId")
    '    '        NodeTableInsertCmd.Parameters.Add("@nodeId", _
    '    '                                          SqlDbType.NVarChar, 20, "nodeId")
    '    NodeTableInsertCmd.Parameters.Add("@nodeType", _
    '                                             SqlDbType.NVarChar, 20, "nodeType")
    '    NodeTableInsertCmd.Parameters.Add("@nodeXpos", _
    '                                                     SqlDbType.NVarChar, 20, "nodeXpos")
    '    NodeTableInsertCmd.Parameters.Add("@nodeYpos", _
    '                                                     SqlDbType.NVarChar, 20, "nodeYpos")


    '    NodeTableda.InsertCommand = NodeTableInsertCmd
    '    NodeTableda.Update(NodeTableds, "nodedB")
    '    'This populates the table with the node data
    '    ImportTrackDBF()

    'End Sub

    'Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox2.Click

    '    For count1 As Integer = 1 To 500
    '        drawbox(1089, 100)
    '    Next


    'End Sub

    Private Sub drawbox(ByVal xpos As Integer, ByVal ypos As Integer)

        ' Dim rectangel As New NodeClass(xpos, ypos, "P")
        'nodeArray(0) = rectangel
        'nodeArrayList.Add(rectangel)

        ' xpos = xpos + 10

        'If xpos > 500 Then
        'xpos = 50
        'ypos = ypos + 10

        'End If




    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' Dim myPen As New Pen(Color.Blue)
        displayWidth = Me.Width - VScrollBar1.Width - 50
        DisplayWindow.Image = New Bitmap(300, 300)
        SourceImage.Image = New Bitmap(3880, 500)
        pointBlankImage.Image = New Bitmap(3880, 500)
        layer1Image.Image = New Bitmap(3880, 500)
        signalLayerImage.Image = New Bitmap(3880, 500)
        labelLayerImage.Image = New Bitmap(3880, 500)
        myGraphics = Graphics.FromImage(SourceImage.Image)
        myGraphics_pointblank_layer = Graphics.FromImage(pointBlankImage.Image)
        myGraphics_layer1 = Graphics.FromImage(layer1Image.Image)
        myGraphicsSignalLayer = Graphics.FromImage(signalLayerImage.Image)
        myGraphicsLabelLayer = Graphics.FromImage(labelLayerImage.Image)
        ' myGraphics.DrawLine(myPen, 10, 10, 100, 100)

        HScrollBar1.Maximum = DisplayWindow.Width
        VScrollBar1.Maximum = DisplayWindow.Height
        displayDrawn = True
        ResizeDisplay()
        wttObj = New wttFrm(Me)
        wttObj.Show()

    End Sub

    Public Sub StartTrain(ByVal headcode As String, _
                          ByVal trainIndex As String, _
                          ByVal trainTermDetails As termDetailsType)
        wttObj.StartTrain(headcode, trainIndex)

        Dim newTrain As clsTraverseRoute
        newTrain = New clsTraverseRoute(Me, headcode, trainIndex, trainTermDetails)

    End Sub

    Public Sub StartClock()
        Dim objClockTimer As New clsClockTimer
        Dim TClockThread As Object = New Thread(AddressOf objClockTimer.RunClock)
        objClockTimer.m_Parent = Me
        ' objClockTimer.m_TrainMon = Me.ref2trainMon
        TClockThread.Start()
    End Sub

    Public Sub SetRoute(ByVal aThreadName As String, ByVal aTextBox As Label, ByVal newText As Integer)
        'Me.currentTime = newText
        TrackArrayList(newText).routeset()
        UpdateDisplay()
    End Sub

    Private Sub formMaximize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.MaximumSizeChanged

        DisplayWindow.Width = 1000
        'PictureBox1.Height = 500
        UpdateDisplay()


    End Sub
    Private Sub Form1_resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.SizeChanged
        If displayDrawn Then
            ResizeDisplay()
        End If
    End Sub

    Private Sub ResizeDisplay()
        Dim vScrollLoc As Point
        Dim hScrollLoc As Point

        vScrollLoc.Y = DisplayWindow.Location.Y
        vScrollLoc.X = Me.Width - VScrollBar1.Width - 15


        hScrollLoc.Y = Me.Height - HScrollBar1.Height - Panel1.Height - 100
        hScrollLoc.X = DisplayWindow.Location.X
        displayWidth = Me.Width - VScrollBar1.Width - 40
        displayHeight = Me.Height - HScrollBar1.Height - Panel1.Height - 140
        If displayHeight > 50 Then
            'it is probably minimised, so don't bother doing anything else

            VScrollBar1.Location = vScrollLoc
            VScrollBar1.Height = displayHeight
            HScrollBar1.Location = hScrollLoc
            HScrollBar1.Width = displayWidth
            HScrollBar1.Maximum = SourceImage.Image.Width
            UpdateDisplay()
        End If
    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        'local scope
        StartClock()

    End Sub

    Public Sub DrawLabel(ByVal xPos As Integer, ByVal Ypos As Integer, ByVal boxcolor As Color, ByVal name As String)


        Dim Brush As New SolidBrush(boxcolor)
        Dim whiteBrush As New SolidBrush(Color.White)
        Dim labelwidth = (name.Length - 1) * 6
        Dim rect As New Rectangle(xPos, Ypos, labelwidth, Font.Height)
        Dim myWhitePen As New Pen(Color.White, 1)
        Dim writefont As New Font("arial", 16)


        myGraphicsLabelLayer.DrawString(name, Font, Brush, xPos, Ypos)



    End Sub
    Public Sub DrawSignalLabel(ByVal xPos As Integer, ByVal Ypos As Integer, ByVal boxcolor As Color, ByVal name As String)


        Dim Brush As New SolidBrush(boxcolor)
        Dim whiteBrush As New SolidBrush(Color.White)
        Dim blueBrush As New SolidBrush(Color.Blue)
        Dim labelwidth = ((name.Length - 1) * 5) + 1
        Dim rect As New Rectangle(xPos, Ypos, labelwidth, Font.Height - 1)
        Dim myWhitePen As New Pen(Color.White, 1)
        Dim writefont As New Font("arial", 8)

        myGraphicsLabelLayer.FillRectangle(whiteBrush, rect)
        myGraphicsLabelLayer.DrawString(name, writefont, Brush, xPos, Ypos)

        'myGraphics_pointblank_layer.FillRectangle(blueBrush, rect)
        'myGraphics_pointblank_layer.DrawString(name, writefont, Brush, xPos, Ypos + 20)



    End Sub

    Public Sub DrawBufferLabel(ByVal xPos As Integer, ByVal Ypos As Integer, ByVal boxcolor As Color, ByVal name As String)

        name = name.TrimStart("0")
        Dim Brush As New SolidBrush(boxcolor)
        Dim yellowBrush As New SolidBrush(Color.Yellow)
        Dim labelwidth = ((name.Length - 1) * 5) + 1
        Dim rect As New Rectangle(xPos, Ypos, labelwidth, Font.Height)
        Dim myyellowPen As New Pen(Color.Yellow, 1)
        Dim writefont As New Font("arial", 8)

        myGraphicsLabelLayer.FillRectangle(Brush, rect)
        myGraphicsLabelLayer.DrawString(name, writefont, yellowBrush, xPos, Ypos)
    End Sub

    Public Sub DrawPointClear(ByVal xStart As Integer, ByVal yStart As Integer, _
                              ByVal xEnd As Integer, ByVal yEnd As Integer)

        Dim thePen As New Pen(Color.Gray)
        thePen.Width = 4
        myGraphics.DrawLine(pen:=thePen, x1:=xStart, y1:=yStart, x2:=xEnd, y2:=yEnd)

    End Sub
    Public Sub DrawPointBlank(ByVal xStart As Integer, ByVal yStart As Integer, _
                                  ByVal xEnd As Integer, ByVal yEnd As Integer, _
                                  ByVal thePen As Pen)

         thePen.Width = 3
          myGraphics.DrawLine(pen:=thePen, x1:=xStart, y1:=yStart, x2:=xEnd, y2:=yEnd)
    End Sub
    Public Sub BlankPoint(ByVal nodeId As Integer, ByVal thePen As Pen)

        thePen.Width = 4
        nodeArrayList(nodeId).BlankPoint(thePen)
 
    End Sub

    Public Sub DrawTrack(ByVal penColor As Pen, _
                         ByVal Xstart As Single, _
                         ByVal Ystart As Single, _
                         ByVal Xend As Single, _
                         ByVal Yend As Single, _
                         ByVal fringe As String)


        'check to see if we're drawing a diagonal
        If (Xend > Xstart) Then
            If (Yend > Ystart) Then
                Xstart = Xstart - 2
                Ystart = Ystart - 2
            ElseIf Yend < Ystart Then
                Xend = Xend + 2
                Yend = Yend - 2
            Else
                'penColor.Color = Color.Green
                Xend = Xend + 1
            End If
        End If
        'If fringe = " " Then
        myGraphics.DrawLine(pen:=penColor, x1:=Xstart, y1:=Ystart, x2:=Xend, y2:=Yend)

        'End If
        ' UpdateDisplay()
        'Refresh()

    End Sub
    Private Sub UpdatePointTypes()

        For nodecount As Integer = 0 To nodeArrayList.Count - 1
            If nodeArrayList(nodecount).nodeType = "P" Then
                nodeArrayList(nodecount).SetPointType()
            End If
            If nodeArrayList(nodecount).nodeType = "X" Then
                nodeArrayList(nodecount).SetPointType()
            End If

        Next
    End Sub

    Private Sub SetAutoRoutes(ByVal autoSignalList As ArrayList)
        If autoSignalList.Count > 0 Then

            For Each signalID In autoSignalList
                'get route from this signal
                'there should only be one for 
                'full auto operation
                Dim routelist As New ArrayList
                Dim exitSignal As String
                GetRouteFromEntrySignal(signalID, routelist, exitSignal)
                '  Console.WriteLine("set route " & routelist(0) & " from " & signalID & " to " & exitSignal)
                SetRoute(signalID, exitSignal, True) ' the true here sets it as an automatic route
            Next
        End If

    End Sub

    Private Sub GetRouteFromEntrySignal(ByVal EntrySignalId As String, ByRef routelist As ArrayList, ByRef exitSignal As String)


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
              EntrySignalId & "'"

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "subroutedB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("subroutedB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader
        'sdfRouteReader.
        Dim route As String = Nothing
        Dim node As String
        Dim nodeto As String
        Dim isSet As String
        Dim numberOfRoutes As Integer = 0
        While sdfRouteReader.Read
            route = sdfRouteReader(0).ToString
            node = sdfRouteReader(1).ToString
            nodeto = sdfRouteReader(2).ToString
            isSet = sdfRouteReader(3).ToString
            routelist.Add(route)
            exitSignal = nodeto
        End While
    End Sub

    Public Sub UpdateDisplay()

        Dim fr_rect As New Rectangle(HScrollBar1.Value, VScrollBar1.Value, displayWidth, displayHeight)
        Dim to_rect As New Rectangle(0, 0, displayWidth, displayHeight)
        Dim source_rect As New Rectangle(0, 0, SourceImage.Image.Width, SourceImage.Image.Height)
        Dim overview_rect As New Rectangle(0, 0, OverviewDisplayBox.Width, OverviewDisplayBox.Height)
        Dim displaySectionXPos As Integer
        Dim displaySectionYPos As Integer
        Dim displaySectionWidth As Integer
        Dim displaySectionHeight As Integer
        'displaySectionXPos = CType((HScrollBar1.Value / HScrollBar1.Maximum) * OverviewDisplayBox.Width, Integer)
        displaySectionWidth = CType((OverviewDisplayBox.Width / SourceImage.Image.Width) * displayWidth, Integer)
        displaySectionHeight = CType((OverviewDisplayBox.Height / SourceImage.Image.Height) * displayHeight, Integer)
        displaySectionXPos = CType((HScrollBar1.Value / SourceImage.Image.Width) * (OverviewDisplayBox.Width), Integer)
        displaySectionYPos = CType((VScrollBar1.Value / SourceImage.Image.Height) * (OverviewDisplayBox.Height), Integer)

        ' displaySectionXPos = OverviewDisplayBox.Width - displaySectionwidth
        Dim display_rect As New Rectangle(displaySectionXPos, displaySectionYPos, displaySectionWidth, displaySectionHeight)
        Dim to_bm As New Bitmap(displayWidth, displayHeight)
        Dim to_bm_layer1 As New Bitmap(displayWidth, displayHeight)
        Dim overview_bm As New Bitmap(OverviewDisplayBox.Width, OverviewDisplayBox.Height)
        Dim gr As Graphics = Graphics.FromImage(to_bm)
        Dim gr_layer1 As Graphics = Graphics.FromImage(to_bm_layer1)
        Dim overviewGr As Graphics = Graphics.FromImage(overview_bm)
        DisplayWindow.Width = displayWidth
        DisplayWindow.Height = displayHeight


        gr.DrawImage(layer1Image.Image, to_rect, fr_rect, GraphicsUnit.Pixel)
        gr.DrawImage(SourceImage.Image, to_rect, fr_rect, GraphicsUnit.Pixel)
        gr.DrawImage(signalLayerImage.Image, to_rect, fr_rect, GraphicsUnit.Pixel)
        gr.DrawImage(pointBlankImage.Image, to_rect, fr_rect, GraphicsUnit.Pixel)
        gr.DrawImage(labelLayerImage.Image, to_rect, fr_rect, GraphicsUnit.Pixel)

        overviewGr.DrawImage(layer1Image.Image, overview_rect, source_rect, GraphicsUnit.Pixel)
        overviewGr.DrawImage(SourceImage.Image, overview_rect, source_rect, GraphicsUnit.Pixel)
        overviewGr.DrawImage(labelLayerImage.Image, overview_rect, source_rect, GraphicsUnit.Pixel)
        overviewGr.DrawRectangle(Pens.Red, display_rect)


        Dim mypen As New Pen(Color.Red)
        Dim semiTransPen = New Pen(Color.FromArgb(128, 0, 0, 255), 15)

         gr.DrawRectangle(semiTransPen, display_rect)
       


        DisplayWindow.Image = to_bm
        OverviewDisplayBox.Image = overview_bm

        'Console.WriteLine("update disply")

    End Sub
    Private Function GetTrainIndex(ByVal headcode As String, ByVal trip As String) As String
        Dim trainIndex As String
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)
        Try
            routesdfDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open Route sdf dBconnection")
        End Try
        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [running_wtt] where (headcode = '" & _
              headcode & "' AND trip = '" & trip & "' )"

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "trackDB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("TrackDB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader


        While sdfRouteReader.Read
            trainIndex = sdfRouteReader(7).ToString
        End While


        Return trainIndex
    End Function


    Sub StartNewTrainCallback(ByVal aThreadName As String, ByVal newService As String, _
                              ByVal serviceIndex As String, ByVal traintermDetails As termDetailsType)
        Dim strArr() As String
        Dim headcode As String
        Dim trip As String
        Dim trainIndex As String
        strArr = newService.Split(" ")
        headcode = strArr(0)
        trip = strArr(1)
        trainIndex = GetTrainIndex(headcode, trip)
        StartTrain(headcode, trainIndex, traintermDetails)
    End Sub


    Sub DrawNodeCallback(ByVal aThreadName As String, ByVal theColor As Color, ByVal NodeIndex As Integer, _
                         ByVal nodeId As String)
        Dim thePen As New Pen(theColor, 4)


        'DrawNode(thePen, nodeArrayList(NodeIndex).mxcentre, nodeArrayList(NodeIndex).mycentre)
        UpdateDisplay()
    End Sub

    Sub BlankPointCallback(ByVal aThreadName As String, _
                                         ByVal theColor As Color, _
                                         ByVal nodeIndex As Integer, _
                                         ByVal nodeid As String)
        Dim thePen As New Pen(theColor, 4)

        '  BlankPoint(nodeIndex, thePen)
        FlashBlank()

        'UpdateDisplay()
    End Sub

    Sub PointSetCompleteCallback(ByVal aThreadName As String, _
                                         ByVal theColor As Color, _
                                         ByVal nodeIndex As Integer, _
                                         ByVal nodeid As String)
        Dim thePen As New Pen(theColor, 4)
        RemovePointfromBlankList(nodeIndex)
    End Sub

    Sub PointSetCompleteCallback(ByVal aThreadName As String, _
                                         ByVal theColor As Color, _
                                         ByVal nodeId As String, _
                                         ByVal notAthing As String)
        Dim thePen As New Pen(theColor, 4)
        RemovePointfromBlankList(GetNodeIndex(nodeId))
    End Sub
    Sub PointSetBlankCallback(ByVal aThreadName As String, _
                                      ByVal theColor As Color, _
                                      ByVal nodeIndex As Integer, _
                                      ByVal nodeid As String)
        Dim thePen As New Pen(Color.Black, 4)
        BlankPoint(nodeIndex, thePen)
        UpdateDisplay()
    End Sub
    Sub PointSetBlankCallback(ByVal aThreadName As String, _
                                        ByVal theColor As Color, _
                                        ByVal nodeId As String, _
                                        ByVal nodteid As String)
        Dim thePen As New Pen(Color.Black, 4)
        BlankPoint(GetNodeIndex(nodeId), thePen)
        UpdateDisplay()
    End Sub


    Sub DrawTRTSCallback(ByVal aThreadName As String, ByVal isFlash As Boolean, ByVal NodeIndex As Integer)
        DrawTRTS(NodeIndex, isFlash)
        UpdateDisplay()
    End Sub


    Sub DrawSignalCallback(ByVal aThreadName As String, ByVal theColor As Color, ByVal NodeIndex As Integer, _
                        ByVal nodeId As String)
        Dim thePen As New Pen(theColor, 4)


        DrawSignal(thePen, NodeIndex)
        UpdateDisplay()
    End Sub

    Sub DrawSignalPostCallback(ByVal aThreadName As String, ByVal theColor As Color, ByVal NodeIndex As Integer, _
                         ByVal nodeId As String)
        Dim thePen As New Pen(theColor, 4)


        DrawSignalPost(thePen, nodeArrayList(NodeIndex).mxcentre, nodeArrayList(NodeIndex).mycentre, nodeArrayList(NodeIndex).rundirection)
        UpdateDisplay()
    End Sub



    Sub DrawCircleCallback(ByVal aThreadName As String, ByVal Xpos As Integer, ByVal Ypos As Integer)
        'DrawCircle(thepen, Xpos, Ypos)

    End Sub
    Sub UpdateDisplayCallback(ByVal aThreadName As String)
        UpdateDisplay()
    End Sub


    Sub AddTrackClass(ByVal aThreadName As String, _
                      ByVal node As String, _
                      ByVal nodeTo As String, _
                                       ByVal fromXPos As Integer, _
                                       ByVal fromYPos As Integer, _
                                       ByVal toXPos As Integer, _
                                       ByVal toYPos As Integer, _
                                       ByVal trackCircuit As Integer, _
                                       ByVal lineSpeed As Integer, _
                                       ByVal trackLength As Integer, _
                                       ByVal line As Integer, _
                                       ByVal location As String, _
                                       ByVal platform As String, _
                                       ByVal setPlatType As String, _
                                       ByVal setFringe As String, _
                                       ByVal setAuto As String, _
                                       ByVal setAutoID As String, _
                                       ByVal setLever As String, _
                                       ByVal setLeverSetReverse As String)

        Dim newTrack As TrackClass
        Dim rnd As New Random()
        Dim TrackId As Integer
        TrackId = TrackArrayList.Count
        newTrack = New TrackClass(Me, node, nodeTo, fromXPos, fromYPos, toXPos, _
                                  toYPos, trackCircuit, TrackId, lineSpeed, _
                                  trackLength, line, location, platform, _
                                  setPlatType, setFringe, setAuto, setAutoID, _
                                  setLever, setLeverSetReverse)

        TrackArrayList.Add(newTrack)
        If setLever <> "     " Then
            'Console.WriteLine("Add lever " & setLever)
            If LeverList.Count < (setLever + 1) Then
                ReDim Preserve LeverList(setLever + 1)
            End If
            LeverList(setLever) = 0

        End If

    End Sub

    Sub ClearTrackTcCallback(ByVal aThreadName As String, _
                                      ByVal track As Integer)
        setTrackTC(track, False)
    End Sub
    Sub SetTrackTcCallback(ByVal aThreadName As String, _
                                       ByVal track As Integer, _
                                       ByVal setClear As Boolean)
        setTrackTC(track, setClear)
    End Sub
    Sub SetTcPostCallback(ByVal aThreadName As String, _
                                        ByVal tc As Integer, _
                                        ByVal setClear As Boolean)
        setTrackCircuit(tc, setClear, False)
    End Sub


    Sub AddNodeClassCallback(ByVal aThreadName As String, _
                                      ByVal XPos As String, _
                                      ByVal YPos As String, _
                                      ByVal nodeId As String, _
                                      ByVal Nodetype As String, _
                                      ByVal runDirection As Integer, _
                                      ByVal labelXpos As String, _
                                      ByVal labelYpos As String, _
                                      ByVal p2NodeXpos As String, _
                                      ByVal p2NodeYpos As String, _
                                      ByVal lever As String, _
                                      ByVal devTo As Byte, _
                                      ByVal devFrom As Byte, _
                                      ByVal auto As String)

        Dim newNode As NodeClass
        Dim nodeIndex As Integer
        nodeIndex = nodeArrayList.Count
        newNode = New NodeClass(XPos, YPos, Nodetype, runDirection, nodeIndex, nodeId, _
                                labelXpos, labelYpos, p2NodeXpos, p2NodeYpos, lever, auto, _
                                devTo, devFrom)
        'newNode.nodeId = nodeId
        ' newNode.nodeIndex = nodeArrayList.Count
        nodeArrayList.Add(newNode)
        nodeArrayList(nodeIndex).DisplayNode()
      
    End Sub
    Sub UpdatePointTypesCallback(ByVal aThreadName As String)
        UpdatePointTypes()

    End Sub

    Sub createCabCallback(ByVal aThreadName As String)
        CreateTrainCab()

    End Sub

    Sub createLinespeedCallback(ByVal aThreadName As String)
        CreateLinespeedFrm()

    End Sub

    Sub RemovetrainDetailsCallback(ByVal aThreadName As String, _
                                   ByVal headcode As String, _
                                   ByVal index As String)
        wttObj.RemoveFromTrainRunning(headcode, index)

    End Sub

    Sub AddLineToLinespeedCallback(ByVal aThreadName As String, ByVal penColor As Color, _
                                   ByVal xStart As Integer, ByVal yStart As Integer,
                                   ByVal xEnd As Integer, ByVal yEnd As Integer
                                   )

        AddLineToLinespeed(penColor, xStart, yStart, xEnd, yEnd)

    End Sub


    Sub GetSignalStatusCallback(ByVal aThreadName As clsTraverseRoute, _
                                        ByVal setNodeID As String)
        Dim signalStatus As Integer
        signalStatus = GetSignalStatus(setNodeID)
        aThreadName.SetSignalStatus(signalStatus)
    End Sub

    Sub GetPointStatusCallback(ByVal aThreadName As SetRoutePointsClass, _
                                       ByVal setNodeID As String, ByVal setReverse As Boolean)
        Dim pointStatus As String
        'Console.WriteLine("form1 gets " & setNodeID)

        pointStatus = GetPointSetReverse(setNodeID)
        aThreadName.SetPointStatus(setNodeID, pointStatus, setReverse)
    End Sub

    Sub GetPointStatusCallback(ByVal aThreadName As SetPointClass, _
                                      ByVal setNodeID As String, ByVal setReverse As Boolean)
        Dim pointStatus As String
        'Console.WriteLine("form1 gets " & setNodeID & " setreverse " & setReverse)

        pointStatus = GetPointSetReverse(setNodeID)
        aThreadName.SetPointStatus(setNodeID, pointStatus, setReverse)
    End Sub

    Sub UpdatePointStatusCallback(ByVal aThreadName As SetPointClass, _
                                     ByVal setNodeID As String, ByVal setReverse As Boolean)
        If setReverse Then
            SetPoint(setNodeID)
        Else
            ReversePoint(setNodeID)

        End If
        LockPoint(setNodeID)

      End Sub
    Sub UpdatePointStatusCallback(ByVal aThreadName As MoveLeverClass, _
                                   ByVal setNodeID As String, ByVal setReverse As Boolean)
        If setReverse Then
            SetPoint(setNodeID)
        Else
            ReversePoint(setNodeID)

        End If
        LockPoint(setNodeID)
    End Sub


    Sub ReturnRouteSetCallback(ByVal aThreadName As SetRoutePointsClass, _
                               ByVal localSubRouteList As ArrayList, _
                               ByVal localRouteId As String, _
                               ByVal localEntryNode As String, _
                               ByVal localExitNode As String, _
                               ByVal localEntryNodeID As String, _
                               ByVal localAuto As Boolean, _
                               ByVal localRouteObj As SetRouteClass)


        ReturnRouteSet(localSubRouteList, localRouteId, localEntryNode, _
                       localExitNode, localEntryNodeID, _
                       localAuto, localRouteObj)
    End Sub


    Private Sub RequestSignalStatus(ByVal aThreadName As clsTraverseRoute, _
                                    ByVal setstatus As String)
        '      New AcceptSignalStatusCallbackDelegate(AddressOf aThreadName.AcceptSignalStatusCallback), New Object() {"Me", setstatus})
    End Sub
    Sub UpdateCabCallback(ByVal aThreadName As String, _
                          ByVal index As Integer, _
                               ByVal service As String, _
                               ByVal trainSpeed As Integer, _
                               ByVal lineSpeed As Integer, _
                               ByVal signalDistance As Integer, _
                               ByVal stationDistance As Integer, _
                               ByVal distance As Integer, _
                               ByVal location As String, _
                               ByVal headcode As String, _
                               ByVal nextTimingloc As String, _
                               ByVal nextTimingTime As String, _
                               ByVal trainAction As String)

        UpdateTrainCab(index, service, trainSpeed, lineSpeed, _
                       signalDistance, stationDistance, distance, location, headcode, nextTimingloc, nextTimingTime, trainAction)


    End Sub
    Sub UpdateLinespeedCallback(ByVal aThreadName As String, _
                        ByVal distanceArray As ArrayList, _
                        ByVal lineSpeedArray As ArrayList, _
                        ByVal accInitSpeedArray() As Integer, _
                        ByVal accDistanceArray() As Integer)

        UpdateLinespeedFrm(distanceArray, lineSpeedArray, accInitSpeedArray, accDistanceArray)


    End Sub

    Public Sub SetAutoRoutesCallback(ByVal aThreadName As String, _
                                               ByVal autoSignalList As ArrayList)

        Console.WriteLine("set auto routes")
        SetAutoRoutes(autoSignalList)

    End Sub


    Sub RedimTCArrayCallback(ByVal aThreadName As String, _
                                      ByVal nOfTrackCircuits As Integer)
        Dim xloc As Integer
        Dim yloc As Integer
        xloc = 20
        yloc = 30
        ReDim trackCircuitArray(nOfTrackCircuits)
        ReDim trackCircuitCountArray(nOfTrackCircuits)
        ' ReDim trackIDArray(nOfTrackCircuits)
        ReDim trackIDArray(TrackArrayList.Count - 1)


        Dim tcwindow As Form2old
        tcwindow = New Form2old
        tcwindow.Show()
        For tcIDCount As Integer = 0 To (nOfTrackCircuits)

            trackCircuitArray(tcIDCount) = New ArrayList
            trackCircuitCountArray(tcIDCount) = 0
        Next
        PopulateTCArray()
        For tcIDCount As Integer = 0 To (nOfTrackCircuits)
            If trackCircuitArray(tcIDCount).Count > 0 Then

                Dim checkboxn As New tcCheckBox(xloc, yloc, tcIDCount)
                yloc = yloc + 30
                If yloc > ((tcwindow.Height * 3) - 100) Then
                    yloc = 30
                    xloc = xloc + 150
                End If
                tcwindow.Controls.Add(checkboxn.checkBoxn)
                TrackCircuitboxArrayList.Add(checkboxn)
            End If
        Next
        DrawPlatforms()
    End Sub

    Private Sub DrawPlatform(ByVal platform As platformType)
        ' Console.WriteLine("draw platform " & platform.platform)
        Dim platformPen As Pen
        platformPen = New Pen(Color:=Color.Red, Width:=1)

        Dim myrectangle As Rectangle
        myrectangle = New Rectangle()
        ' myGraphics_layer1.DrawRectangle(pen:=platformPen, x:=CType(platform.platXstart, Integer), y:=CType(platform.platYstart, Integer), width:=CType(platform.platXwidth, Integer), height:=CType(platform.platYheight, Integer))

        Dim Brush As New SolidBrush(Color.CornflowerBlue)
        Dim rect As New Rectangle(y:=CType(platform.platXstart, Integer), x:=CType(platform.platYstart, Integer), Width:=CType(platform.platXwidth, Integer), Height:=CType(platform.platYheight, Integer))
        ' Dim rect As New Rectangle(x:=CType(platform.platXstart, Integer), y:=100, Width:=CType(platform.platXwidth, Integer), Height:=CType(platform.platYheight, Integer))
        Console.WriteLine("draw platform " & platform.platform & " " & CType(platform.platXstart, Integer) & " " & CType(platform.platYstart, Integer) & " " & CType(platform.platXwidth, Integer) & " " & CType(platform.platYheight, Integer))

        myGraphics_layer1.FillRectangle(Brush, rect)




    End Sub

    Private Sub DrawPlatforms()
        For Each platform As platformType In platformArrayList

            DrawPlatform(platform)
        Next

    End Sub


    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim DBFReaderObj As DBFReaderClass
        Button2.Enabled = False


        DBFReaderObj = New DBFReaderClass
        DBFReaderObj.m_parent = Me

        'UpdatePointTypes()
        'Console.WriteLine("Update Auto routes")
        UpdateDisplay()

        'DrawTrack(0, 0, 100, 100)

    End Sub

    Private Sub HScrollBar1_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles HScrollBar1.Scroll
        UpdateDisplay()



    End Sub

    Private Sub VScrollBar1_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles VScrollBar1.Scroll
        'DisplayWindow.Top = -VScrollBar1.Value

        UpdateDisplay()
        'Refresh()

    End Sub
    Public Sub SetRoute(ByVal startSignal As String, ByVal endSignal As String)

        SetRoute(startSignal, endSignal, False)

    End Sub



    Public Sub SetRoute(ByVal startSignal As String, ByVal endSignal As String, ByVal auto As Boolean)
        'SetRoute()
        Console.WriteLine("set route")
        Dim tempRoute As SetRouteClass
        tempRoute = New SetRouteClass(Me, auto)
        tempRoute.entryNodeId = GetNodeIndex(startSignal)

        'CheckRoute

        If tempRoute.CheckRoute(startSignal, endSignal) Then
            'Set Route
            If tempRoute.routeIsClear Then
                If tempRoute.subSidSet Then
                Else
                    tempRoute.SetRoute()

                End If
            End If
        End If
        Console.WriteLine("route set")

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        SetRoute("S001", "S003")
        SetRoute("S003", "S005")
        SetRoute("S005", "S007")
        SetRoute("S007", "S009")
        SetRoute("S009", "S015")


        '  Dim tcwindow As clsTraverseRoute
        ' tcwindow = New clsTraverseRoute(Me)
        'Dim a As Integer
        'a = GetSignalStatus("S001")
    End Sub




    Public Sub setTrackTC(ByVal trackId As Integer, ByVal setClear As Boolean)
        Dim tc As Integer
        tc = TrackArrayList(trackId).tc
        setTrackCircuit(tc, setClear, False)

    End Sub

    Public Sub CheckTracksinTCAreClear(ByVal tcIndex As Integer)

        For tcCount As Integer = 0 To (trackCircuitArray(tcIndex).Count - 1)
            'check to see if each track in the array is set or clear

        Next


    End Sub
    Public Sub setTrackCircuit(ByVal tcIndex As Integer, ByVal setClear As Boolean,
                               ByVal realNotVirtual As Boolean)
        Dim trackArrayIndex As Integer
        Dim trackStatus As Integer
        If setClear Then
            trackCircuitCountArray(tcIndex) = 1
        Else
            trackCircuitCountArray(tcIndex) = 0


        End If

        For tcCount As Integer = 0 To (trackCircuitArray(tcIndex).Count - 1)
            trackArrayIndex = trackCircuitArray(tcIndex)(tcCount)
            trackStatus = TrackArrayList(trackArrayIndex).GetTrackStatus()

            If setClear Then
                TrackArrayList(trackArrayIndex).TrackOccupied(realNotVirtual)
            Else
                If trackCircuitCountArray(tcIndex) = 0 Then
                    'only clear if all tracks in tc are clear
                    TrackArrayList(trackArrayIndex).TrackClear(realNotVirtual)
                    Console.WriteLine("Clear tc " & trackArrayIndex & " now")
                    '
                Else
                    '
                    Console.WriteLine("Don't clear tc " & trackArrayIndex & " yet count is " & trackCircuitCountArray(tcIndex))
                End If
            End If

            If trackStatus > 0 Then
                'subroute is occupied
                TrackArrayList(trackArrayIndex).ClearSetRoute()

                Console.WriteLine("Track Index " & trackArrayIndex & " is " & trackStatus & "set")
                ' routeIsClear = False
            End If
        Next

    End Sub

    Private Sub OverviewDisplayBox_Click(ByVal sender As System.Object, ByVal e As  _
System.Windows.Forms.MouseEventArgs) Handles OverviewDisplayBox.MouseDown

        Dim xPos As Integer
        Dim yPos As Integer

        xPos = CType(e.X, Integer)
        xPos = ((xPos / OverviewDisplayBox.Image.Width) * SourceImage.Image.Width) - (displayWidth / 2)
        If xPos < 0 Then
            xPos = 0
        End If

        yPos = CType(e.Y, Integer)
        yPos = ((yPos / OverviewDisplayBox.Image.Height) * SourceImage.Image.Height) - (displayHeight / 2)
        If yPos < 0 Then
            yPos = 0
        End If
        If yPos > VScrollBar1.Maximum Then
            yPos = VScrollBar1.Maximum
        End If


        VScrollBar1.Value = yPos
        HScrollBar1.Value = xPos

        TextBox1.Text = "Overview down at" + CStr(e.X) + " :" + CStr(e.Y) & " " & xPos
        UpdateDisplay()


    End Sub

    Private Sub CreateTrainCab()

        Dim cab As New clstrainCab
        cab.Show()
        cabArrayList.Add(cab)

    End Sub

    Private Sub CreateLinespeedFrm()

        linespeedObj = New linespeedfrm
        linespeedObj.Show()

    End Sub
    Private Sub UpdateLinespeedFrm(ByVal distanceArray As ArrayList, _
                                   ByVal lineSpeedArray As ArrayList, _
                                   ByVal accInitSpeedArray() As Integer, _
                                   ByVal accDistanceArray() As Integer)
        Dim isTheSame As Boolean
        linespeedObj.UpdateDistanceArray(distanceArray)
        linespeedObj.UpdateLineSpeedArray(lineSpeedArray)
        ' linespeedObj.UpdateaccInitSpeedArray(accInitSpeedArray)
        'linespeedObj.UpdateaccDistanceArray(accDistanceArray)
        linespeedObj.CalcTargetSpeeds()
        isTheSame = linespeedObj.CompareAccInitSpeedArray(accInitSpeedArray)
        isTheSame = linespeedObj.CompareAccDistanceArray(accDistanceArray)

        linespeedObj.DrawGridFromArray()
        linespeedObj.GetAcceleration()
        linespeedObj.UpdateDisplay()
    End Sub

    Private Sub AddLineToLinespeed(ByVal penColor As Color, ByVal xStart As Integer, ByVal yStart As Integer, _
                                   ByVal xEnd As Integer, ByVal yEnd As Integer
                                   )
        DrawLineOnLinespeed(penColor, xStart, yStart, xEnd, yEnd)

    End Sub


    Private Sub DrawLineOnLinespeed(ByVal penColor As Color, _
                                   ByVal xStart As Integer, _
                                    ByVal yStart As Integer, _
                                    ByVal xEnd As Integer, _
                                    ByVal yEnd As Integer
                                    )

        linespeedObj.Drawline(penColor, xStart, yStart, xEnd, yEnd)
        'linespeedObj.UpdateDisplay()


    End Sub
    Private Sub UpdateTrainRunningForm(ByVal headcode As String, ByVal speed As String, _
                                       ByVal location As String, ByVal nextTimingloc As String, _
                                       ByVal nextTimingTime As String, ByVal trainAction As String)
        Dim lineDesc As String
        Dim timingDesc As String
        timingDesc = "Next stop " & nextTimingloc & " at " & nextTimingTime
        lineDesc = lineArray(location).description
        TrainInfoObj.UpdateTrainRunningdB(headcode, 99, speed, lineDesc, timingDesc, trainAction)
        TrainInfoObj.RefreshDataView()

    End Sub

    Public Sub ReturnTrainInfoObj(ByVal setTrainInfoObj As TrainInfoFrm)
        TrainInfoObj = setTrainInfoObj

    End Sub
    Private Sub UpdateTrainCab(ByVal index As Integer, _
                               ByVal service As String, _
                               ByVal trainSpeed As String, _
                               ByVal lineSpeed As String, _
                               ByVal signalDistance As String, _
                               ByVal stationDistance As String, _
                               ByVal distance As String, _
                               ByVal location As String, _
                               ByVal headcode As String, _
                               ByVal nextTimingloc As String, _
                               ByVal nextTimingTime As String, _
                               ByVal trainAction As String)

        Dim lineIndex As String
        lineIndex = TrackArrayList(location).line
        cabArrayList(index).serviceText.Text = service
        cabArrayList(index).speedText.Text = trainSpeed
        cabArrayList(index).lineSpeedText.Text = lineSpeed
        cabArrayList(index).signalDistanceText.Text = signalDistance
        cabArrayList(index).stationDistanceText.Text = stationDistance
        cabArrayList(index).stationDistanceText.Text = stationDistance
        cabArrayList(index).distanceText.Text = distance
        cabArrayList(index).locationText.Text = location

        UpdateTrainRunningForm(headcode, trainSpeed, lineIndex, nextTimingloc, nextTimingTime, trainAction)
    End Sub

    Public Sub SetLever(ByVal lever As Integer, ByVal nodeId As String, ByVal setReverse As Boolean)

        If setReverse Then
            'Points should be normal

            If LeverList(lever) = 1 Then
                'Then we need to return the lever to set the points
                Console.WriteLine("Return lever " & lever)
                If GetPointSetReverse(nodeId) = "False" Then

                    'Console.WriteLine("nodeId is " & GetNodeIndex(nodeId) & " index is " & nodeId)
                    ' Dim newPointSetBlank As New FlashPointBlankClass(Me, GetNodeIndex(nodeId))
                    AddPointToBlankList(GetNodeIndex(nodeId))
                    Dim SetPointDelayobj As New SetPointClass(Me, GetNodeIndex(nodeId), setReverse)
                End If

            End If
        Else
            If LeverList(lever) = 0 Then
                Console.WriteLine("Pull lever " & lever)
                If GetPointSetReverse(nodeId) Then
                    AddPointToBlankList(GetNodeIndex(nodeId))

                    '     If realPoint Then
                    Dim SetPointDelayobj As New SetPointClass(Me, GetNodeIndex(nodeId), False)
                    'End If

                End If

                'wait for point to be set

            End If
        End If
    End Sub


    Private Sub FlashBlank()

        ' Console.WriteLine("flash blank")
        If pointBlankList.Count > 0 Then
            For Each nodeIndex As Integer In pointBlankList

                If nodeIndex > 0 Then
                    BlankPoint(nodeIndex, pointBlankPen)
                End If
            Next
            UpdateDisplay()
        End If
        If pointBlankPen.Color = Color.Black Then
            pointBlankPen.Color = Color.White
        Else
            pointBlankPen.Color = Color.Black
        End If

    End Sub


    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim objcalc2 As New clsCalc
        Dim distance As Integer
        Dim trackEndSpeed As Integer

        Dim linespeedfrmobj As linespeedfrm

        linespeedfrmobj = New linespeedfrm
        linespeedfrmobj.Show()

        Dim letters() As Integer = {1, 5, 6, 3, 4, 2}

        Dim letterSizes() As Integer = {110, 300, 110, 125, 180, 70}
        Dim letterthings() As Integer = {75, 90, 100, 30, 75, 50}

        Dim copyArray(letters.Count - 1)
        letters.CopyTo(copyArray, 0)
        'Console.WriteLine()
        For i As Integer = 0 To letters.Length - 1
            Console.WriteLine("{0}: up to {1} meters long. {2}", letters(i), letterSizes(i), letterthings(i))
        Next

        Console.WriteLine("Sort(letters, letterSizes)")
        Array.Sort(letters, letterSizes)
        Array.Sort(copyArray, letterthings)

        For i As Integer = 0 To letters.Length - 1
            Console.WriteLine("{0}: up to {1} meters long. {2}", letters(i), letterSizes(i), letterthings(i))
        Next

        'Dim x As Integer
        Dim keys(3) As Integer
        keys(0) = 0
        keys(1) = 100
        keys(2) = 200
        keys(3) = 300

        'keys.add(22)
        Dim names(3) As Integer
        names(0) = 1
        names(1) = 0
        names(2) = -1
        names(3) = 0

        ReDim Preserve keys(keys.Count)
        ReDim Preserve names(names.Count)
        keys(keys.Count - 1) = 110
        names(keys.Count - 1) = -1
        Array.Sort(keys, names)
        Dim copyTrue As Boolean = False
        For arrayCounter As Integer = 0 To keys.Count - 1

            If keys(arrayCounter) = 110 Then
                copyTrue = True
            End If
            If copyTrue Then
                If arrayCounter < keys.Count - 1 Then
                    names(arrayCounter) = names(arrayCounter + 1)
                    keys(arrayCounter) = keys(arrayCounter + 1)
                End If
            End If
        Next
        If copyTrue Then
            ReDim Preserve keys(keys.Count - 2)
            ReDim Preserve names(names.Count - 2)

        End If
        trackEndSpeed = objcalc2.mph2Mps(objcalc2.CalcEndSpeed(objcalc2.Mps2mph(9), 0.75, 250))

        distance = objcalc2.CalcAccelUntilDistance(objcalc2.Mps2mph(9), objcalc2.Mps2mph(20), 0.75, -1, 200)
        trackEndSpeed = objcalc2.CalcEndSpeed(objcalc2.Mps2mph(9), 0.75, distance)
        trackEndSpeed = objcalc2.CalcEndSpeed(trackEndSpeed, -1, 200 - distance)

        ' formobj.Show()
    End Sub

    Public Sub getoutputText(ByVal athread As Class1,
                            ByVal index As Integer)
        Dim myval As Integer
        myval = nodeArrayList(index).signalstate
        athread.status = myval
    End Sub
    Public Sub AddPointToBlankListCallback(ByVal aThreadName As MoveLeverClass, ByVal pointId As String)
        Dim nodeID As Integer
        nodeID = GetNodeIndex(pointId)
        UnLockPoint(nodeID)
        AddPointToBlankList(nodeID)
    End Sub


    Public Sub AddPointToBlankList(ByVal pointId As Integer)
        Console.WriteLine("Add " & pointId)

        If pointBlankList.Contains(pointId) Then

        Else
            pointBlankList.Add(pointId)
        End If
    End Sub
    Public Sub RemovePointfromBlankList(ByVal pointId As Integer)
        'Console.WriteLine("Remove " & pointId)

        If pointBlankList.Contains(pointId) Then
            pointBlankList.Remove(pointId)
        Else
        End If
    End Sub

    Public Sub setoutputText(ByVal athread As String,
                             ByVal textval As String)

        Label1.Text = textval
    End Sub

    Public Sub setTrackCircuitCallback(ByVal athread As String,
                               ByVal tcIndex As String,
                               ByVal setClear As Boolean,
                               ByVal realNotVirtual As Boolean)

        setTrackCircuit(tcIndex, setClear, realNotVirtual)
    End Sub



    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        Dim myDeviceManagement As New DeviceManagement()
        Dim USBHandlerObj0 As New USBHandlerClass(Me, 0)
        Dim USBHandlerObj1 As New USBHandlerClass(Me, 1)
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        flashObj.StartFlash()


    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub


    Private Sub pointBlankImage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pointBlankImage.Click

    End Sub



    Public Sub SetPointBlanksCallback(ByVal aThreadName As String)
        For Each nodeselect In nodeArrayList
            nodeselect.BlankPointSet(pointBlankPen)
            'Console.WriteLine("update pointblank  " & nodeselect.nodeid)
        Next

    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        ' myGraphics_pointblank_layer = Graphics.FromImage(pointBlankImage.Image)
        myGraphics_pointblank_layer.Clear(Color.FromArgb(0, 0, 0, 255))
        For Each nodeselect In nodeArrayList
            nodeselect.BlankPointSet(pointBlankPen)
            ' Console.WriteLine("update pointblank  " & nodeselect.nodeid)
        Next
        UpdateDisplay()
    End Sub
End Class
