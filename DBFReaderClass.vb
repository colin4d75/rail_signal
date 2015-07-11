Imports System.IO
Imports System.Data.SqlServerCe
Imports System.Drawing
Imports System.Data.OleDb
Imports System.Threading
Imports System.Math



Public Class DBFReaderClass
    Private t As Thread
    Public m_parent As Form1
    Private nOfTrackCircuits As Integer
    Private TrackID As Integer
    Private auto As Boolean
    Private autoSignalList As New ArrayList

     Dim FILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Test 1\modeldata"

    Public routeCount As Integer

    



    'Stick the delegates here

    Delegate Sub UpdatePointTypesCallbackDelegate(ByVal aThreadName As String)


    Delegate Sub SetAutoRoutesCallbackDelegate(ByVal aThreadName As String, _
                                               ByVal autoSignalList As ArrayList)

    Delegate Sub SetPointBlanksCallbackDelegate(ByVal aThreadName As String)

    Delegate Sub AddTrackClassDelegate(ByVal aThreadName As String, _
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
                                       ByVal platType As String, _
                                       ByVal fringe As String, _
                                       ByVal Auto As Boolean, _
                                       ByVal AutoID As String, _
                                       ByVal setLever As String, _
                                       ByVal setLeverSetReverse As String)


    Delegate Sub UpdateDisplayCallbackDelegate(ByVal aThreadname As String)
    Delegate Sub ResizeTCArrayCallbackDelegate(ByVal aThreadname As String, _
                                               ByVal nOfTrackCircuits As Integer)
    Delegate Sub DrawNodeDelegate(ByVal aThreadname As String, _
                                   ByVal XPos As String, _
                                   ByVal YPos As String)
    Delegate Sub AddNodeClassDelegate(ByVal aThreadname As String, _
                                      ByVal XPos As String, _
                                      ByVal YPos As String, _
                                      ByVal nodeId As String, _
                                      ByVal nodeType As String, _
                                      ByVal runDirection As String, _
                                      ByVal labelXpos As String, _
                                      ByVal labelYpos As String, _
                                      ByVal p2NodeXpos As String, _
                                      ByVal p2NodeYpos As String, _
                                      ByVal lever As String, _
                                      ByVal devTo As Byte, _
                                      ByVal devFrom As Byte, _
                                      ByVal auto As String)

    Sub New()
        t = New Thread(AddressOf Me.ReadNodeDb)
        t.Start()
    End Sub


    Private Sub ReadNodeDb()
        'Display Splash
        nOfTrackCircuits = 0
        TrackID = 0
        m_parent.platformArrayList = New ArrayList
        DeleteDBNodes()
        'ReadDBFNodes()
        ReadConvertedDBFNodes()
        DeleteDBTracks()
        ImportPlatformDBF()
        'ImportTrackDBF()
        ImportCompiledTrackDBF()
        RedimTcArray()
        DeleteDBRoutes()
        ImportRouteDBF()
        DeleteSubDBRoutes()
        ImportSubRouteDBF()
        SetAutoRoutes()
        SetPointBlanks()
        m_parent.ClockDisplayLabel.Invoke(New UpdateDisplayCallbackDelegate(AddressOf m_parent.UpdateDisplayCallback), New Object() {"clockref"})
        'Remove splash
        t.Abort()
    End Sub

    Private Sub RedimTcArray()

        m_parent.ClockDisplayLabel.Invoke(New ResizeTCArrayCallbackDelegate(AddressOf m_parent.RedimTCArrayCallback), New Object() {"clockref", nOfTrackCircuits})

    End Sub
    Private Sub SetPointBlanks()
        m_parent.ClockDisplayLabel.Invoke(New SetPointBlanksCallbackDelegate(AddressOf m_parent.SetPointBlanksCallback), New Object() {"clockref"})

    End Sub

    Private Sub SetAutoRoutes()
        m_parent.ClockDisplayLabel.Invoke(New SetAutoRoutesCallbackDelegate(AddressOf m_parent.SetAutoRoutesCallback), New Object() {"clockref", autoSignalList})

    End Sub


    Private Sub SetPointTypes()
        m_parent.ClockDisplayLabel.Invoke(New UpdatePointTypesCallbackDelegate(AddressOf m_parent.UpdatePointTypesCallback), New Object() {"clockref"})

    End Sub
    'CalcLength
    ' This function takes the node coordinates an works out the distance between them using trigonometry
    ' remeber that each coord has been multiplied by three, so we need to divide the answer down by three at
    ' the end

    Private Function CalcLength(ByVal xFrom As Integer, ByVal yFrom As Integer, _
                                 ByVal xTo As Integer, ByVal yTo As Integer) As Integer

        Dim xDelta As Integer
        Dim yDelta As Integer
        Dim totalLengthSingle As Single
        Dim totalLengthInteger As Integer
        xDelta = xFrom - xTo
        yDelta = yFrom - yTo
        totalLengthSingle = Sqrt((xDelta * xDelta) + (yDelta * yDelta))
        totalLengthSingle = totalLengthSingle * 2
        'Now divide back by three
        totalLengthSingle = totalLengthSingle / 3
        totalLengthInteger = totalLengthSingle
        Return totalLengthInteger


    End Function

    Private Function ChainsToMetres(ByVal inChains As Single) As Single
        Dim outMetres As Single
        outMetres = 20.1168 * inChains
        Return outMetres
    End Function

    Private Function DevTypeToByte(ByVal inDevType As String) As Byte
        Dim outByte As Byte
        outByte = 0
        Select Case inDevType
            Case "- "
                outByte = 1
            Case "\-"
                outByte = 2
            Case "-/"
                outByte = 3
            Case "/-"
                outByte = 4
            Case "-\"
                outByte = 5

        End Select
        Return outByte
    End Function

    Private Sub addDetailstoTrackDatabase(ByVal node As String, _
                                 ByVal nodeto As String, _
                                 ByVal line As Integer, _
                                 ByVal maxspeed As Integer, _
                                 ByVal tc As Integer, _
                                 ByVal fringe As String, _
                                 ByVal scale As Single, _
                                 ByVal location As String, _
                                 ByVal platform As String, _
                                 ByVal platType As String, _
                                 ByVal setLever As String, _
                                 ByVal setLeverSetReverse As String, _
                                 ByRef dt As DataTable)



        ' Add A Row
        Dim newRow As DataRow = dt.NewRow()
        Dim nodeXpos As Integer = 10
        Dim nodeypos As Integer = 3
        Dim fromXpos As Integer
        Dim fromYpos As Integer
        Dim toXpos As Integer
        Dim toYpos As Integer
        'Dim trackLengthInChains As Single
        Dim trackLength As Integer
        newRow("node") = node
        ' newRow("noderef") = noderef
        newRow("nodeto") = nodeto
        'newRow("nodeRow") = nodeRows
        'newRow("nodeCol") = nodeCol
        newRow("tc") = tc
        If tc > nOfTrackCircuits Then
            nOfTrackCircuits = tc
        End If
        getNodeCoords(node, nodeXpos, nodeypos)
        newRow("fromYpos") = nodeypos
        newRow("fromXpos") = nodeXpos
        fromXpos = nodeXpos
        fromYpos = nodeypos
        getNodeCoords(nodeto, nodeXpos, nodeypos)
        newRow("toYpos") = nodeypos
        newRow("toXpos") = nodeXpos
        newRow("maxspeed") = maxspeed
        toXpos = nodeXpos
        toYpos = nodeypos
        newRow("trackID") = TrackID
        trackLength = CalcLength(fromXpos, fromYpos, toXpos, toYpos)
        'Dim outlength As Integer

        ''take this out just now to make things run faster for development
        ''trackLengthInChains = trackLength / scale
        ''trackLength = ChainsToMetres(trackLengthInChains)
        '' put this in instead
        trackLength = trackLength * 3

        'debug info
        'Console.WriteLine("Node " & node & " Nodeto " & nodeto & " fromX " & fromXpos & " to X " & toXpos & ", from Y " & fromYpos & " toY " & toYpos)
        'Console.WriteLine("Track " & TrackID & " length in " & trackLengthInChains & " scale " & scale & " outlength " & trackLength)
        newRow("trackLength") = trackLength
        newRow("location") = 99
        'newRow("newRowColSet") = 0
        dt.Rows.Add(newRow)
        m_parent.ClockDisplayLabel.Invoke(New AddTrackClassDelegate(AddressOf m_parent.AddTrackClass), _
                                          New Object() {"clockref", node, nodeto, fromXpos, fromYpos, toXpos, toYpos, tc, maxspeed, _
                                                        trackLength, line, location, platform, platType, fringe, auto, "autoID", _
                                                        setLever, setLeverSetReverse})
        TrackID = TrackID + 1


    End Sub
    '====================================================================
    '
    ' addDetailstoNodeDatabase
    '
    ' This sub adds the required details into the Node Datebase
    '
    '====================================================================

    Private Sub addDetailstoNodeDatabase(ByVal nodeid As Integer, _
                                  ByVal noderef As String, _
                                  ByVal nodeType As String, _
                                  ByVal nodeRow As String, _
                                  ByVal nodeCol As String, _
                                  ByVal labelXpos As String, _
                                  ByVal labelYpos As String, _
                                  ByVal lever As String, _
                                  ByVal p2nodeRow As String, _
                                  ByVal p2nodeCol As String, _
                                  ByVal devTo As String, _
                                  ByVal devFrom As String, _
                                  ByRef dt As DataTable)



        ' Add A Row
        Dim newRow As DataRow = dt.NewRow()
        newRow("nodeid") = noderef
        ' newRow("noderef") = noderef
        newRow("nodeType") = nodeType
        newRow("nodeYpos") = nodeRow
        newRow("nodeXpos") = nodeCol
        newRow("labelXpos") = labelXpos
        newRow("labelYpos") = labelYpos
        newRow("p2NodeRow") = p2nodeCol
        newRow("p2NodeCol") = p2nodeRow
        newRow("lever") = lever

        newRow("devFrom") = devFrom
        newRow("devTo") = devTo
        'newRow("nodeCoordSet") = 0
        'newRow("newRow") = 0
        'newRow("newCol") = 0
        'newRow("newRowColSet") = 0
        dt.Rows.Add(newRow)

    End Sub


    Private Sub addDetailstoRouteDatabase(ByVal route As String, _
                                          ByVal node As String, _
                                          ByVal nodeto As String, _
                                          ByVal direction As String, _
                                          ByRef dt As DataTable)

        ' Add A Row
        Dim newRow As DataRow = dt.NewRow()
        newRow("route") = route
        newRow("node") = node
        newRow("nodeto") = nodeto
        newRow("isSet") = "0"
        newRow("direction") = direction
        dt.Rows.Add(newRow)

    End Sub

    Private Sub addDetailstoSubRouteDatabase(ByVal route As String, _
                                          ByVal node As String, _
                                          ByVal nodeto As String, _
                                          ByVal requires As String, _
                                          ByRef dt As DataTable)

        ' Add A Row
        Dim newRow As DataRow = dt.NewRow()
        newRow("route") = route
        newRow("node") = node
        newRow("nodeto") = nodeto
        newRow("requires") = requires
        dt.Rows.Add(newRow)

    End Sub


    '====================================================================
    '
    ' Private Sub
    ' getNodeCoords
    '
    ' This sub is passed the node id. The node database is then 
    ' to retrieve the x and y coords for the node. These are then
    ' returned via nodeXpos and nodeYpos
    '
    '====================================================================

    Private Sub getNodeCoords(ByVal nodeId As String, _
                                 ByRef nodeXpos As Integer, _
                                 ByRef nodeypos As Integer)

        ' open connection to node database


        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open Node dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectNodeTable As String = "SELECT nodeXpos, nodeYpos  FROM [nodedB] " & _
            "WHERE (nodeId = '" & nodeId & "')"
        Dim SelectCommand As SqlCeCommand

        SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, nodeDbConn)

        Dim nodeReader As SqlCeDataReader
        Dim mycommand As SqlCeCommand

        mycommand = New SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
        nodeReader = mycommand.ExecuteReader


        While nodeReader.Read
            nodeypos = nodeReader(0).ToString
            nodeXpos = nodeReader(1).ToString
        End While
        nodeDbConn.Close()

    End Sub


    '====================================================================
    '
    ' Private Sub
    ' DeleteDBRoutes
    '
    ' This sub deletes all the entries in the Route database so it 
    ' can be guaranteed to be empty.
    '
    '====================================================================

    Public Sub DeleteDBRoutes()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            routeDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open Route dBconnection")
        End Try

        Dim RouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlDeleteRouteTable As String = "DELETE FROM routedb "
        RouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlDeleteRouteTable, routeDbConn)

        Dim RouteTableds As New DataSet
        RouteTableda.SelectCommand.ExecuteNonQuery()
        Dim RouteTabledt As DataTable = RouteTableds.Tables("routedB")
    End Sub

    '====================================================================
    '
    ' Private Sub
    ' DeleteDBTracks
    '
    ' This sub deletes all the entries in the Tracks database so it 
    ' can be guaranteed to be empty.
    '
    '====================================================================

    Public Sub DeleteDBTracks()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            routeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route dBconnection")
        End Try

        Dim RouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlDeleteRouteTable As String = "DELETE FROM trackdb "
        RouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlDeleteRouteTable, routeDbConn)

        Dim RouteTableds As New DataSet
        RouteTableda.SelectCommand.ExecuteNonQuery()
        Dim RouteTabledt As DataTable = RouteTableds.Tables("trackdB")
    End Sub


    '====================================================================
    '
    ' Private Sub
    ' DeleteSubDBRoutes
    '
    ' This sub deletes all the entries in the SubRoutes database so it 
    ' can be guaranteed to be empty.
    '
    '====================================================================


    Public Sub DeleteSubDBRoutes()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            routeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route dBconnection")
        End Try

        Dim RouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlDeleteRouteTable As String = "DELETE FROM subroutedb "
        RouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlDeleteRouteTable, routeDbConn)

        Dim RouteTableds As New DataSet
        RouteTableda.SelectCommand.ExecuteNonQuery()
        Dim RouteTabledt As DataTable = RouteTableds.Tables("subroutedB")



    End Sub

    '====================================================================
    '
    ' Private Sub
    ' DeleteDBNodes
    '
    ' This sub deletes all the entries in the Nodes database so it 
    ' can be guaranteed to be empty.
    '
    '====================================================================

    Public Sub DeleteDBNodes()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Node dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlDeleteNodeTable As String = "DELETE FROM nodedb "
        NodeTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlDeleteNodeTable, nodeDbConn)

        Dim NodeTableds As New DataSet
        NodeTableda.SelectCommand.ExecuteNonQuery()
        Dim NodeTabledt As DataTable = NodeTableds.Tables("nodedB")
    End Sub

    '====================================================================
    '
    ' Private Sub
    ' ImportRouteDBF
    '
    ' This sub deletes imports the data from the foxpro style database
    ' into a exchange sdf database so it can be accessed and queried a
    ' bit easier.
    ' This sub should not need to be run at each startup, but only during
    ' an import of the database.
    '
    '====================================================================


    Public Sub ImportRouteDBF()
        'Setup connection to new sdf database
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder
        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)

        Try
            routesdfDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open Route sdf dBconnection")
        End Try

        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [routedB] "
        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)

        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "routedB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("routedB")

        Dim insertsdfRouteTable As String = "INSERT INTO routedB " & _
                     "(route,node, nodeto,isSet,direction) " & _
                     "VALUES " & _
                     "(@route,@node,@nodeto,@isSet,@direction)"


        'We want to open the FoxPro .dbf file, then copy the data into a database that we can 
        'use a bit more easily, and will probably be supported for a bit longer
        Dim DBFFILENAME As String = FILENAME
        Dim DBFconnectionString As String = "Provider = VFPOLEDB;" & _
                  "Data Source=" & DBFFILENAME & ";Collating Sequence=general;"
        'Create Connection
        Dim DBFdBaseConnection As New System.Data.OleDb.OleDbConnection(DBFconnectionString)
        'Open connection to database

        Try
            DBFdBaseConnection.Open()
        Catch ex As Exception
            MessageBox.Show("can't open DBF connection")
            Console.WriteLine("can't open DBF dBconnection")
        End Try

        Dim selectDBFAllRouteString As String = "SELECT * FROM route "
        Dim DBFdBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectDBFAllRouteString, DBFdBaseConnection)
        Dim routeCount As Integer

        routeCount = 0
        'Set up a reader for the DB


        Dim DBFRouteReader As OleDbDataReader = DBFdBaseCommandSelectAll.ExecuteReader()
        Dim route As String
        Dim node As String
        Dim nodeto As String
        Dim direction As Integer
        'Dim maxSpeed As Integer

        'Try
        'DBFRouteReader.Read()

        'Catch ex As Exception

        'MessageBox.Show("can't open connection")

        'End Try
        Dim sdfRouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectDBFRouteTable As String = "SELECT * FROM [routedb] "
        sdfRouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectDBFRouteTable, routesdfDbConn)

        Dim sdfRouteTableds As New DataSet
        sdfRouteTableda.Fill(sdfRouteTableds, "routedB")
        Dim sdfRouteTabledt As DataTable = sdfRouteTableds.Tables("routedB")

        ' Dim insertsdfRouteTable As String = "INSERT INTO routedB " & _
        '             "(node,nodeto,tc,fromXpos,fromYpos,toXpos,toYpos) " & _
        '            "VALUES " & _
        '           "(@node,@nodeto,@tc,@fromXpos,@fromYpos,@toXpos,@toYpos)"
        sdfRouteTabledt.NewRow()
        While DBFRouteReader.Read
            ' For each entry in the foxpro database read the line and insert
            ' into the sdf database
            route = DBFRouteReader(0).ToString
            node = DBFRouteReader(1).ToString
            nodeto = DBFRouteReader(2).ToString
            direction = DBFRouteReader(3).ToString
            'tc = DBFRouteReader(4).ToString
            addDetailstoRouteDatabase(route, node, nodeto, direction, sdfRouteTabledt)
            routeCount = routeCount + 1
            ' m_parent.ClockDisplayLabel.Invoke(New AddTrackClassDelegate(AddressOf m_parent.AddTrackClass), New Object() {"clockref", inNodeRow, inNodeCol})
            'drawbox(inNodeRow, inNodeCol)
        End While
        DBFRouteReader.Close()

        Dim sdfRouteTableInsertCmd As New SqlCeCommand(insertsdfRouteTable, routesdfDbConn)
        sdfRouteTableInsertCmd.Parameters.Add("@route", _
                                          SqlDbType.NVarChar, 20, "route")
        sdfRouteTableInsertCmd.Parameters.Add("@node", _
                                           SqlDbType.NVarChar, 20, "node")
        sdfRouteTableInsertCmd.Parameters.Add("@nodeTo",
                                          SqlDbType.NVarChar, 20, "nodeTo")
        sdfRouteTableInsertCmd.Parameters.Add("@isSet",
                                                SqlDbType.NVarChar, 20, "isSet")
        sdfRouteTableInsertCmd.Parameters.Add("@direction",
                                                  SqlDbType.NVarChar, 20, "direction")


        sdfRouteTableda.InsertCommand = sdfRouteTableInsertCmd
        sdfRouteTableda.Update(sdfRouteTableds, "routedB")
        'This populates the table with the node data



    End Sub

    Public Sub ImportPlatformDBF()
        'Setup connection to new sdf database

        'We want to open the FoxPro .dbf file, then copy the data into a database that we can 
        'use a bit more easily, and will probably be supported for a bit longer
        'Dim DBFFILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Leith\modeldata"
        Dim DBFFILENAME As String = FILENAME


        Dim DBFconnectionString As String = "Provider = VFPOLEDB;" & _
                  "Data Source=" & DBFFILENAME & ";Collating Sequence=general;"
        'Create Connection
        Dim DBFdBaseConnection As New System.Data.OleDb.OleDbConnection(DBFconnectionString)
        'Open connection to database

        Try
            DBFdBaseConnection.Open()
        Catch ex As Exception
            MessageBox.Show("can't open DBF connection")
            Console.WriteLine("can't open DBF dBconnection")
        End Try

        Dim selectDBFAllRouteString As String = "SELECT * FROM platform "
        Dim DBFdBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectDBFAllRouteString, DBFdBaseConnection)
        'dBaseCommandSelectAll.CommandText = CommandType.Text

        Dim DBFRouteReader As OleDbDataReader = DBFdBaseCommandSelectAll.ExecuteReader()

        Dim location As String
        Dim platType As String
        Dim platform As String
        Dim node As String
        Dim nodeto As String
        Dim platXstart As String
        Dim platYstart As String
        Dim platXwidth As String
        Dim platYheight As String

        'Dim line As Integer
        'Dim maxSpeed As Integer
        Dim platformDetails As Form1.platformType

        While DBFRouteReader.Read
            location = DBFRouteReader(0).ToString
            platform = DBFRouteReader(1).ToString
            platType = DBFRouteReader(2).ToString
            node = DBFRouteReader(3).ToString
            nodeto = DBFRouteReader(4).ToString
            'platXstart = DBFRouteReader(9).ToString
            'platYstart = DBFRouteReader(10).ToString
            'platYheight = DBFRouteReader(6).ToString
            'platXwidth = DBFRouteReader(7).ToString
            platXstart = DBFRouteReader(6).ToString
            platYstart = DBFRouteReader(7).ToString
            platYheight = DBFRouteReader(8).ToString
            platXwidth = DBFRouteReader(9).ToString

            'tc = DBFRouteReader(4).ToString
            routeCount = routeCount + 1
            ' m_parent.ClockDisplayLabel.Invoke(New AddTrackClassDelegate(AddressOf m_parent.AddTrackClass), New Object() {"clockref", inNodeRow, inNodeCol})

            'drawbox(inNodeRow, inNodeCol)

            platformDetails.nodeFrom = node
            platformDetails.nodeto = nodeto
            platformDetails.platform = platform
            platformDetails.platType = platType
            platformDetails.location = location
            platformDetails.platXstart = platXstart
            platformDetails.platYstart = platYstart
            platformDetails.platXwidth = platXwidth
            platformDetails.platYheight = platYheight
            m_parent.platformArrayList.Add(platformDetails)
        End While
        DBFRouteReader.Close()





    End Sub
    Public Sub ImportSubRouteDBF()
        'The format of the subroute dB is pretty similar to tha route dB,
        ' so this function is pretty much the same as the route dB reader
        'Setup connection to new sdf database
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)

        Try
            routesdfDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Sub Route sdf dBconnection")
        End Try

        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [subroutedB] "
        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)

        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "subroutedB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("subroutedB")

        Dim insertsdfRouteTable As String = "INSERT INTO subroutedB " & _
                     "(route,node, nodeto,requires) " & _
                     "VALUES " & _
                     "(@route,@node,@nodeto,@requires)"


        'We want to open the FoxPro .dbf file, then copy the data into a database that we can 
        'use a bit more easily, and will probably be supported for a bit longer
        ' Dim DBFFILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Leith\modeldata"
        Dim DBFFILENAME As String = FILENAME


        Dim DBFconnectionString As String = "Provider = VFPOLEDB;" & _
                  "Data Source=" & DBFFILENAME & ";Collating Sequence=general;"
        'Create Connection
        Dim DBFdBaseConnection As New System.Data.OleDb.OleDbConnection(DBFconnectionString)
        'Open connection to database

        Try
            DBFdBaseConnection.Open()
        Catch ex As Exception
            MessageBox.Show("can't open DBF connection")
            Console.WriteLine("can't open DBF dBconnection")
        End Try

        Dim selectDBFAllRouteString As String = "SELECT * FROM subroute "
        Dim DBFdBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectDBFAllRouteString, DBFdBaseConnection)
        'dBaseCommandSelectAll.CommandText = CommandType.Text


        Dim routeCount As Integer

        routeCount = 0
        'Set up a reader for the DB


        Dim DBFRouteReader As OleDbDataReader = DBFdBaseCommandSelectAll.ExecuteReader()

        Dim route As String
        Dim node As String
        Dim nodeto As String
        Dim requires As String
        ' Dim line As Integer
        'Dim maxSpeed As Integer
        'Dim tc As Integer

        Try
            'DBFRouteReader.Read()

        Catch ex As Exception

            MessageBox.Show("can't open connection")

        End Try
        Dim sdfRouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectDBFRouteTable As String = "SELECT * FROM [subroutedb] "
        sdfRouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectDBFRouteTable, routesdfDbConn)

        Dim sdfRouteTableds As New DataSet
        sdfRouteTableda.Fill(sdfRouteTableds, "subroutedB")
        Dim sdfRouteTabledt As DataTable = sdfRouteTableds.Tables("subroutedB")

        ' Dim insertsdfRouteTable As String = "INSERT INTO routedB " & _
        '             "(node,nodeto,tc,fromXpos,fromYpos,toXpos,toYpos,trackLength,maxSpeed,location) " & _
        '            "VALUES " & _
        '           "(@node,@nodeto,@tc,@fromXpos,@fromYpos,@toXpos,@toYpos,@trackLength,@maxSpeed,@location)"
        'sdfRouteTabledt.NewRow()
        Dim newDataRow As DataRow = sdfRouteTabledt.NewRow()
        'newDataRow("node") = "tableNodeid"



        'sdfRouteTabledt.Rows.Add(newDataRow)

        While DBFRouteReader.Read
            route = DBFRouteReader(0).ToString
            node = DBFRouteReader(1).ToString
            nodeto = DBFRouteReader(2).ToString
            requires = DBFRouteReader(3).ToString
            'tc = DBFRouteReader(4).ToString
            addDetailstoSubRouteDatabase(route, node, nodeto, requires, sdfRouteTabledt)
            routeCount = routeCount + 1
            ' m_parent.ClockDisplayLabel.Invoke(New AddTrackClassDelegate(AddressOf m_parent.AddTrackClass), New Object() {"clockref", inNodeRow, inNodeCol})

            'drawbox(inNodeRow, inNodeCol)
        End While
        DBFRouteReader.Close()




        Dim sdfRouteTableInsertCmd As New SqlCeCommand(insertsdfRouteTable, routesdfDbConn)
        sdfRouteTableInsertCmd.Parameters.Add("@route", _
                                          SqlDbType.NVarChar, 20, "route")
        sdfRouteTableInsertCmd.Parameters.Add("@node", _
                                           SqlDbType.NVarChar, 20, "node")
        sdfRouteTableInsertCmd.Parameters.Add("@nodeTo",
                                          SqlDbType.NVarChar, 20, "nodeTo")
        sdfRouteTableInsertCmd.Parameters.Add("@requires",
                                                  SqlDbType.NVarChar, 20, "requires")


        sdfRouteTableda.InsertCommand = sdfRouteTableInsertCmd
        sdfRouteTableda.Update(sdfRouteTableds, "subroutedB")
        'This populates the table with the node data



    End Sub

    Private Function CheckIFPlatform(ByVal nodeFrom As String, ByVal nodeTo As String) As Form1.platformType
        Dim platform As Form1.platformType
        platform.platform = " "
        platform.location = " "
        For Each platformEntry In m_parent.platformArrayList
            If platformEntry.nodefrom = nodeFrom And platformEntry.nodeto = nodeTo Then
                platform.platform = platformEntry.platform
                platform.location = platformEntry.location
                platform.full = True
            ElseIf platformEntry.nodefrom = nodeFrom Then
                platform.platform = platformEntry.platform
                platform.location = platformEntry.location
                platform.full = False
            ElseIf platformEntry.nodeto = nodeTo Then
                platform.platform = platformEntry.platform
                platform.location = platformEntry.location
                platform.full = False
            End If
        Next
        Return platform
    End Function
    Public Sub ImportTrackDBF()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim trackDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)
        'nod  'create a rectangle based on x,y coordinates, width, & height

        'nodeDbConn.Open()

        Try
            trackDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Track dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectNodeTable As String = "SELECT * FROM [trackdB] "
        NodeTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, trackDbConn)

        Dim NodeTableds As New DataSet
        NodeTableda.Fill(NodeTableds, "nodedB")
        Dim NodeTabledt As DataTable = NodeTableds.Tables("nodedB")

        Dim insertNodeTable As String = "INSERT INTO nodedB " & _
                     "(nodeID, nodeType) " & _
                     "VALUES " & _
                     "(@nodeId,@nodeType)"




        'We want to open the FoxPro .dbf file, then copy the data into a database that we can 
        'use a bit more easily, and will probably be supported for a bit longer
        ' Dim FILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Leith\modeldata"


        Dim connectionString As String = "Provider = VFPOLEDB;" & _
                  "Data Source=" & FILENAME & ";Collating Sequence=general;"
        'Create Connection
        Dim dBaseConnection As New System.Data.OleDb.OleDbConnection(connectionString)
        'Open connection to database

        Try
            dBaseConnection.Open()
        Catch ex As Exception
            MessageBox.Show("can't open connection")
            Console.WriteLine("can't open dBconnection")
        End Try

        Dim selectAllNodeString As String = "SELECT * FROM track "
        Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectAllNodeString, dBaseConnection)
        'dBaseCommandSelectAll.CommandText = CommandType.Text


        Dim NodeCount As Integer

        NodeCount = 0
        'Set up a reader for the DB


        Dim DBFTrackReader As OleDbDataReader = dBaseCommandSelectAll.ExecuteReader()


        Dim node As String
        Dim nodeto As String
        Dim line As Integer
        Dim maxSpeed As Integer
        Dim tc As Integer

        'Try
        'DBFTrackReader.Read()

        'Catch ex As Exception

        'MessageBox.Show("can't open connection")

        'End Try
        Dim TrackTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelecttrackTable As String = "SELECT * FROM [trackdb] "
        TrackTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelecttrackTable, trackDbConn)

        Dim TrackTableds As New DataSet
        TrackTableda.Fill(TrackTableds, "trackdB")
        Dim TrackTabledt As DataTable = TrackTableds.Tables("trackdB")

        Dim insertTrackTable As String = "INSERT INTO trackdB " & _
                     "(node,nodeto,tc,fromXpos,fromYpos,toXpos,toYpos,trackID,trackLength,maxSpeed,location) " & _
                     "VALUES " & _
                     "(@node,@nodeto,@tc,@fromXpos,@fromYpos,@toXpos,@toYpos,@trackID,@trackLength,@maxSpeed,@location)"
        TrackTabledt.NewRow()



        Dim newDataRow As DataRow = TrackTabledt.NewRow()
        newDataRow("node") = "tableNodeid"
        Dim platform As Form1.platformType
        Dim fringe As String
        Dim scale As Single
        Dim lever As String
        Dim leverSetReversed As String



        While DBFTrackReader.Read
            node = DBFTrackReader(0).ToString
            nodeto = DBFTrackReader(1).ToString
            line = DBFTrackReader(2).ToString
            maxSpeed = DBFTrackReader(3).ToString
            tc = DBFTrackReader(4).ToString
            fringe = DBFTrackReader(5).ToString
            platform = CheckIFPlatform(node, nodeto)
            scale = DBFTrackReader(6).ToString
            addDetailstoTrackDatabase(node, nodeto, line, maxSpeed, tc, fringe, _
                                      scale, platform.location, platform.platform, _
                                      platform.platType, lever, leverSetReversed, TrackTabledt)
            NodeCount = NodeCount + 1
            ' m_parent.ClockDisplayLabel.Invoke(New AddTrackClassDelegate(AddressOf m_parent.AddTrackClass), New Object() {"clockref", inNodeRow, inNodeCol})

            'drawbox(inNodeRow, inNodeCol)
        End While
        DBFTrackReader.Close()




        Dim TrackTableInsertCmd As New SqlCeCommand(insertTrackTable, trackDbConn)
        TrackTableInsertCmd.Parameters.Add("@node", _
                                          SqlDbType.NVarChar, 20, "node")
        TrackTableInsertCmd.Parameters.Add("@nodeTo",
                                          SqlDbType.NVarChar, 20, "nodeTo")
        TrackTableInsertCmd.Parameters.Add("@tc", _
                                                 SqlDbType.NVarChar, 20, "tc")
        TrackTableInsertCmd.Parameters.Add("@fromXpos", _
                                                         SqlDbType.NVarChar, 20, "fromXpos")
        TrackTableInsertCmd.Parameters.Add("@fromYpos", _
                                                         SqlDbType.NVarChar, 20, "fromYpos")
        TrackTableInsertCmd.Parameters.Add("@toXpos", _
                                                         SqlDbType.NVarChar, 20, "toXpos")
        TrackTableInsertCmd.Parameters.Add("@toYpos", _
                                                         SqlDbType.NVarChar, 20, "toYpos")
        TrackTableInsertCmd.Parameters.Add("@trackID", _
                                                          SqlDbType.NVarChar, 20, "trackID")
        TrackTableInsertCmd.Parameters.Add("@trackLength", _
                                                              SqlDbType.NVarChar, 20, "trackLength")
        TrackTableInsertCmd.Parameters.Add("@maxSpeed", _
                                                      SqlDbType.NVarChar, 20, "maxSpeed")
        TrackTableInsertCmd.Parameters.Add("@location", _
                                                      SqlDbType.NVarChar, 20, "location")



        TrackTableda.InsertCommand = TrackTableInsertCmd
        TrackTableda.Update(TrackTableds, "trackdB")
        'This populates the table with the node data



    End Sub

    Public Sub ImportCompiledTrackDBF()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim trackDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)
        'nod  'create a rectangle based on x,y coordinates, width, & height

        'nodeDbConn.Open()

        Try
            trackDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Track dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectNodeTable As String = "SELECT * FROM [trackdB] "
        NodeTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, trackDbConn)

        Dim NodeTableds As New DataSet
        NodeTableda.Fill(NodeTableds, "nodedB")
        Dim NodeTabledt As DataTable = NodeTableds.Tables("nodedB")

        Dim insertNodeTable As String = "INSERT INTO nodedB " & _
                     "(nodeID, nodeType) " & _
                     "VALUES " & _
                     "(@nodeId,@nodeType)"




        'We want to open the FoxPro .dbf file, then copy the data into a database that we can 
        'use a bit more easily, and will probably be supported for a bit longer
        ' Dim FILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Leith\modeldata"


        Dim connectionString As String = "Provider = VFPOLEDB;" & _
                  "Data Source=" & FILENAME & ";Collating Sequence=general;"
        'Create Connection
        Dim dBaseConnection As New System.Data.OleDb.OleDbConnection(connectionString)
        'Open connection to database

        Try
            dBaseConnection.Open()
        Catch ex As Exception
            MessageBox.Show("can't open connection")
            Console.WriteLine("can't open dBconnection")
        End Try

        Dim selectAllNodeString As String = "SELECT " & _
                                            "NODE,NODETO,LINE,MAXSPEED,TC," & _
                                            "FRINGE,SCALE,LEVER,DIREC" & _
                                            " FROM trackew "
        Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectAllNodeString, dBaseConnection)
        'dBaseCommandSelectAll.CommandText = CommandType.Text


        Dim NodeCount As Integer

        NodeCount = 0
        'Set up a reader for the DB


        Dim DBFTrackReader As OleDbDataReader = dBaseCommandSelectAll.ExecuteReader()


        Dim node As String
        Dim nodeto As String
        Dim line As Integer
        Dim maxSpeed As Integer
        Dim tc As Integer

        'Try
        'DBFTrackReader.Read()

        'Catch ex As Exception

        'MessageBox.Show("can't open connection")

        'End Try
        Dim TrackTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelecttrackTable As String = "SELECT * FROM [trackdb] "
        TrackTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelecttrackTable, trackDbConn)

        Dim TrackTableds As New DataSet
        TrackTableda.Fill(TrackTableds, "trackdB")
        Dim TrackTabledt As DataTable = TrackTableds.Tables("trackdB")

        Dim insertTrackTable As String = "INSERT INTO trackdB " & _
                     "(node,nodeto,tc,fromXpos,fromYpos,toXpos,toYpos,trackID,trackLength,maxSpeed,location,lever,leversetreverse) " & _
                     "VALUES " & _
                     "(@node,@nodeto,@tc,@fromXpos,@fromYpos,@toXpos,@toYpos,@trackID,@trackLength,@maxSpeed,@location,@lever,@leversetreverse)"
        TrackTabledt.NewRow()



        Dim newDataRow As DataRow = TrackTabledt.NewRow()
        Dim platform As Form1.platformType
        Dim fringe As String
        Dim scale As Single
        Dim lever As String
        Dim leverSetReversed As String




        While DBFTrackReader.Read
            node = DBFTrackReader(0).ToString
            nodeto = DBFTrackReader(1).ToString
            line = DBFTrackReader(2).ToString
            maxSpeed = DBFTrackReader(3).ToString
            tc = DBFTrackReader(4).ToString
            fringe = DBFTrackReader(5).ToString
            platform = CheckIFPlatform(node, nodeto)
            'fringe = "X"
            scale = DBFTrackReader(6).ToString
            lever = DBFTrackReader(7).ToString
            leverSetReversed = DBFTrackReader(8).ToString
            addDetailstoTrackDatabase(node, nodeto, line, maxSpeed, tc, _
                                      fringe, scale, platform.location, _
                                      platform.platform, platform.platType, _
                                      lever, leverSetReversed, TrackTabledt)
            NodeCount = NodeCount + 1
        End While
        DBFTrackReader.Close()




        Dim TrackTableInsertCmd As New SqlCeCommand(insertTrackTable, trackDbConn)
        TrackTableInsertCmd.Parameters.Add("@node", _
                                          SqlDbType.NVarChar, 20, "node")
        TrackTableInsertCmd.Parameters.Add("@nodeTo",
                                          SqlDbType.NVarChar, 20, "nodeTo")
        TrackTableInsertCmd.Parameters.Add("@tc", _
                                                 SqlDbType.NVarChar, 20, "tc")
        TrackTableInsertCmd.Parameters.Add("@fromXpos", _
                                                         SqlDbType.NVarChar, 20, "fromXpos")
        TrackTableInsertCmd.Parameters.Add("@fromYpos", _
                                                         SqlDbType.NVarChar, 20, "fromYpos")
        TrackTableInsertCmd.Parameters.Add("@toXpos", _
                                                         SqlDbType.NVarChar, 20, "toXpos")
        TrackTableInsertCmd.Parameters.Add("@toYpos", _
                                                         SqlDbType.NVarChar, 20, "toYpos")
        TrackTableInsertCmd.Parameters.Add("@trackID", _
                                                          SqlDbType.NVarChar, 20, "trackID")
        TrackTableInsertCmd.Parameters.Add("@trackLength", _
                                                              SqlDbType.NVarChar, 20, "trackLength")
        TrackTableInsertCmd.Parameters.Add("@maxSpeed", _
                                                      SqlDbType.NVarChar, 20, "maxSpeed")
        TrackTableInsertCmd.Parameters.Add("@location", _
                                                      SqlDbType.NVarChar, 20, "location")
        TrackTableInsertCmd.Parameters.Add("@lever", _
                                                    SqlDbType.NVarChar, 20, "lever")
        TrackTableInsertCmd.Parameters.Add("@leversetreverse", _
                                                      SqlDbType.NVarChar, 20, "leversetreverse")




        TrackTableda.InsertCommand = TrackTableInsertCmd
        TrackTableda.Update(TrackTableds, "trackdB")
        'This populates the table with the node data



    End Sub


    Public Sub ReadDBFNodes()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Node dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectNodeTable As String = "SELECT * FROM [nodedb] "
        NodeTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, nodeDbConn)

        Dim NodeTableds As New DataSet
        NodeTableda.Fill(NodeTableds, "nodedB")
        Dim NodeTabledt As DataTable = NodeTableds.Tables("nodedB")

        Dim insertNodeTable As String = "INSERT INTO nodedB " & _
                     "(nodeID, nodeType,nodeXpos, nodeYpos, labelXpos, labelYpos, lever) " & _
                     "VALUES " & _
                     "(@nodeId,@nodeType,@nodeXpos,@nodeYpos, @labelXpos, @labelYpos, @lever)"




        'We want to open the FoxPro .dbf file, then copy the data into a database that we can 
        'use a bit more easily, and will probably be supported for a bit longer
        'Dim FILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Leith\modeldata"


        Dim connectionString As String = "Provider = VFPOLEDB;" & _
             "Data Source=" & FILENAME & ";Collating Sequence=general;"
        'Create Connection
        Dim dBaseConnection As New System.Data.OleDb.OleDbConnection(connectionString)
        'Open connection to database

        Try
            dBaseConnection.Open()
        Catch ex As Exception
            MessageBox.Show("can't open connection")
            Console.WriteLine("can't open dBconnection")
        End Try

        Dim selectAllNodeString As String = "SELECT * FROM node "
        Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectAllNodeString, dBaseConnection)
        'dBaseCommandSelectAll.CommandText = CommandType.Text
        Dim NodeCount As Integer

        NodeCount = 0
        'Set up a reader for the DB


        Dim myNodeNodeIdReader As OleDbDataReader = dBaseCommandSelectAll.ExecuteReader()


        Dim nodeId As String
        Dim nodeType As String
        Dim inNodeRow As String
        Dim inNodeCol As String
        Dim runDirection As String
        Dim labelXpos As String
        Dim labelYpos As String
        Dim lever As String
        Dim auto As String
        Dim p2NodeCol As String
        Dim p2NodeRow As String
        Dim devTo As String
        Dim devFrom As String




        While myNodeNodeIdReader.Read
            nodeId = myNodeNodeIdReader(0).ToString
            nodeType = myNodeNodeIdReader(1).ToString
            inNodeCol = (myNodeNodeIdReader(2).ToString) * 3
            inNodeRow = (myNodeNodeIdReader(3).ToString - 1000) * 3
            labelXpos = myNodeNodeIdReader(7).ToString
            labelYpos = myNodeNodeIdReader(6).ToString
            runDirection = myNodeNodeIdReader(11).ToString
            p2NodeCol = myNodeNodeIdReader(12).ToString
            p2NodeRow = myNodeNodeIdReader(13).ToString

            lever = myNodeNodeIdReader(14).ToString
            auto = myNodeNodeIdReader(19).ToString
            devTo = myNodeNodeIdReader(21).ToString
            devFrom = myNodeNodeIdReader(22).ToString

            addDetailstoNodeDatabase(NodeCount, nodeId, nodeType, inNodeRow, inNodeCol, labelXpos, labelYpos, lever, p2NodeRow, p2NodeCol, devTo, devFrom, NodeTabledt)
            NodeCount = NodeCount + 1
            'm_parent.ClockDisplayLabel.Invoke(New DrawNodeDelegate(AddressOf m_parent.DrawNodeCallback), New Object() {"clockref", inNodeRow, inNodeCol})
            m_parent.ClockDisplayLabel.Invoke(New AddNodeClassDelegate(AddressOf m_parent.AddNodeClassCallback), New Object() {"clockref", _
                                inNodeRow, inNodeCol, nodeId, nodeType, runDirection, labelXpos, labelYpos, lever, auto})
            'drawbox(inNodeRow, inNodeCol)
            If nodeType = "S" Then
                If auto = "F" Then
                    'add to autolist
                End If

            End If
        End While
        myNodeNodeIdReader.Close()




        Dim NodeTableInsertCmd As New SqlCeCommand(insertNodeTable, nodeDbConn)
        NodeTableInsertCmd.Parameters.Add("@nodeId", _
                                          SqlDbType.NVarChar, 20, "nodeId")
        '        NodeTableInsertCmd.Parameters.Add("@nodeId", _
        '                                          SqlDbType.NVarChar, 20, "nodeId")
        NodeTableInsertCmd.Parameters.Add("@nodeType", _
                                                 SqlDbType.NVarChar, 20, "nodeType")
        NodeTableInsertCmd.Parameters.Add("@nodeXpos", _
                                                         SqlDbType.NVarChar, 20, "nodeXpos")
        NodeTableInsertCmd.Parameters.Add("@nodeYpos", _
                                                         SqlDbType.NVarChar, 20, "nodeYpos")

        NodeTableInsertCmd.Parameters.Add("@labelXpos", _
                                                             SqlDbType.NVarChar, 20, "labelXpos")
        NodeTableInsertCmd.Parameters.Add("@labelYpos", _
                                                         SqlDbType.NVarChar, 20, "labelYpos")

        NodeTableInsertCmd.Parameters.Add("@lever", _
                                                     SqlDbType.NVarChar, 20, "lever")


        NodeTableda.InsertCommand = NodeTableInsertCmd
        NodeTableda.Update(NodeTableds, "nodedB")
        'This populates the table with the node data
        ' ImportTrackDBF()
        SetPointTypes()
    End Sub


    Public Sub ReadConvertedDBFNodes()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Node dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectNodeTable As String = "SELECT * FROM [nodedb] "
        NodeTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, nodeDbConn)

        Dim NodeTableds As New DataSet
        NodeTableda.Fill(NodeTableds, "nodedB")
        Dim NodeTabledt As DataTable = NodeTableds.Tables("nodedB")

        Dim insertNodeTable As String = "INSERT INTO nodedB " & _
                     "(nodeID, nodeType,nodeXpos, nodeYpos, labelXpos, labelYpos, lever, p2nodeRow, p2nodeCol , devTo, devFrom) " & _
                     "VALUES " & _
                     "(@nodeId,@nodeType,@nodeXpos,@nodeYpos, @labelXpos, @labelYpos, @lever, @p2nodeRow, @p2nodeCol, @devTo, @devFrom)"




        'We want to open the FoxPro .dbf file, then copy the data into a database that we can 
        'use a bit more easily, and will probably be supported for a bit longer
        'Dim FILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Leith\modeldata"


        Dim connectionString As String = "Provider = VFPOLEDB;" & _
             "Data Source=" & FILENAME & ";Collating Sequence=general;"
        'Create Connection
        Dim dBaseConnection As New System.Data.OleDb.OleDbConnection(connectionString)
        'Open connection to database

        Try
            dBaseConnection.Open()
        Catch ex As Exception
            MessageBox.Show("can't open connection")
            Console.WriteLine("can't open dBconnection")
        End Try

        Dim selectAllNodeString As String = "SELECT " & _
                                            "NODE,TYPE,ROW,COL,PROW,PCOL,PLABLROW,PLABLCOL, " & _
                                            "SUBTYPE,SUBSID,AUTO,RUNDIREC,PROW2,PCOL2,LEVER, " & _
                                            "DEV_TO,DEV_FROM FROM nodeew "
        Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectAllNodeString, dBaseConnection)
        'dBaseCommandSelectAll.CommandText = CommandType.Text
        Dim NodeCount As Integer

        NodeCount = 0
        'Set up a reader for the DB


        Dim myNodeNodeIdReader As OleDbDataReader = dBaseCommandSelectAll.ExecuteReader()


        Dim nodeId As String
        Dim nodeType As String
        Dim inNodeRow As String
        Dim inNodeCol As String
        Dim p2NodeRow As String
        Dim p2NodeCol As String
        Dim runDirection As String
        Dim labelXpos As String
        Dim labelYpos As String
        Dim lever As String
        Dim auto As String
        Dim devTo As String
        Dim devFrom As String
        Dim devToByte As Byte
        Dim devFromByte As Byte


        While myNodeNodeIdReader.Read
            nodeId = myNodeNodeIdReader(0).ToString
            nodeType = myNodeNodeIdReader(1).ToString
            inNodeCol = (myNodeNodeIdReader(2).ToString) * 3
            inNodeRow = (myNodeNodeIdReader(3).ToString - 1000) * 3
            labelXpos = myNodeNodeIdReader(7).ToString
            labelYpos = myNodeNodeIdReader(6).ToString
            auto = myNodeNodeIdReader(10).ToString
            runDirection = myNodeNodeIdReader(11).ToString
            p2NodeCol = myNodeNodeIdReader(12).ToString
            p2NodeRow = myNodeNodeIdReader(13).ToString
            lever = myNodeNodeIdReader(14).ToString
            devTo = myNodeNodeIdReader(15).ToString
            devFrom = myNodeNodeIdReader(16).ToString

            devFromByte = DevTypeToByte(devFrom)
            devToByte = DevTypeToByte(devTo)

            addDetailstoNodeDatabase(NodeCount, nodeId, nodeType, inNodeRow, inNodeCol, labelXpos, labelYpos, lever, p2NodeRow, p2NodeCol, devToByte, devFromByte, NodeTabledt)
            NodeCount = NodeCount + 1
            'm_parent.ClockDisplayLabel.Invoke(New DrawNodeDelegate(AddressOf m_parent.DrawNodeCallback), New Object() {"clockref", inNodeRow, inNodeCol})
            m_parent.ClockDisplayLabel.Invoke(New AddNodeClassDelegate(AddressOf m_parent.AddNodeClassCallback), New Object() {"clockref", _
                                inNodeRow, inNodeCol, nodeId, nodeType, runDirection, labelXpos, labelYpos, p2NodeRow, p2NodeCol, lever, devToByte, devFromByte, auto})
            'drawbox(inNodeRow, inNodeCol)
            If nodeType = "S" Then
                If auto = "F" Then
                    'add to autolist
                    autoSignalList.Add(nodeId)
                    Console.WriteLine("Node " & nodeId & " is full auto")
                End If

            End If
        End While
        myNodeNodeIdReader.Close()




        Dim NodeTableInsertCmd As New SqlCeCommand(insertNodeTable, nodeDbConn)
        NodeTableInsertCmd.Parameters.Add("@nodeId", _
                                          SqlDbType.NVarChar, 20, "nodeId")
        '        NodeTableInsertCmd.Parameters.Add("@nodeId", _
        '                                          SqlDbType.NVarChar, 20, "nodeId")
        NodeTableInsertCmd.Parameters.Add("@nodeType", _
                                                 SqlDbType.NVarChar, 20, "nodeType")
        NodeTableInsertCmd.Parameters.Add("@nodeXpos", _
                                                         SqlDbType.NVarChar, 20, "nodeXpos")
        NodeTableInsertCmd.Parameters.Add("@nodeYpos", _
                                                         SqlDbType.NVarChar, 20, "nodeYpos")

        NodeTableInsertCmd.Parameters.Add("@labelXpos", _
                                                             SqlDbType.NVarChar, 20, "labelXpos")
        NodeTableInsertCmd.Parameters.Add("@labelYpos", _
                                                         SqlDbType.NVarChar, 20, "labelYpos")

        NodeTableInsertCmd.Parameters.Add("@lever", _
                                                     SqlDbType.NVarChar, 20, "lever")
        NodeTableInsertCmd.Parameters.Add("@p2NodeRow", _
                                                           SqlDbType.NVarChar, 20, "p2NodeRow")
        NodeTableInsertCmd.Parameters.Add("@p2NodeCol", _
                                                             SqlDbType.NVarChar, 20, "p2NodeCol")
        NodeTableInsertCmd.Parameters.Add("@devTo", _
                                                         SqlDbType.NVarChar, 20, "devTo")
        NodeTableInsertCmd.Parameters.Add("@devFrom", _
                                                             SqlDbType.NVarChar, 20, "devFrom")


        NodeTableda.InsertCommand = NodeTableInsertCmd
        NodeTableda.Update(NodeTableds, "nodedB")
        'This populates the table with the node data
        ' ImportTrackDBF()
        SetPointTypes()
    End Sub


End Class
