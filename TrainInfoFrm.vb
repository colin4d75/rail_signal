Imports System.IO
Imports System.Data.SqlServerCe
Imports System.Drawing
Imports System.Data.OleDb
Imports System.Threading
Imports System.Math

Public Class TrainInfoFrm

    Dim trainobj As trainDataFrm
    Dim trainObjIsSet As Boolean = False
    Dim wttObj As wttFrm


    Private Sub addDetailstotrainRunningDatabase(ByVal headcode As String, _
                                         ByVal index As String, _
                                         ByVal speed As String, _
                                         ByRef dt As DataTable)

        ' Add A Row
        Dim newRow As DataRow = dt.NewRow()
        newRow("headcode") = headcode
        newRow("tableIndex") = index
        newRow("actionRequired") = ""
        newRow("speed") = speed
        ' newRow("nodeto") = nodeto
        'newRow("isSet") = "0"
        dt.Rows.Add(newRow)

    End Sub

    Public Sub UpdateTrainRunningdB(ByVal headcode As String, ByVal index As Integer, _
                                    ByVal speed As String, ByVal location As String, _
                                    ByVal trainDesc As String, ByVal trainAction As String)
        ' open connection to node database

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder
        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open running train dBconnection")
        End Try


        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [runningTrainsdb] "
        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, nodeDbConn)

        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "runningTrainsdb")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("runningTrainsdb")

        Dim insertsdfRouteTable As String = "UPDATE runningTrainsdb " & _
                          "SET  speed='" & _
                              speed & "', position=' " & location & _
                             "' , notes='" & trainDesc & _
                             "' , trainAction='" & trainAction & _
                             "' WHERE (headcode='" & headcode & "')"

        Dim sdfRouteTableda As New SqlServerCe.SqlCeDataAdapter

        Dim sdfRouteTableds As New DataSet
        sdfRouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(insertsdfRouteTable, nodeDbConn)

        sdfRouteTableda.SelectCommand.ExecuteNonQuery()



    End Sub


    Public Sub UpdateRunningdB(ByVal headcode As String, ByVal index As Integer)
        ' open connection to node database

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder
        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open running train dBconnection")
        End Try


        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [runningTrainsdb] "
        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, nodeDbConn)

        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "runningTrainsdb")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("runningTrainsdb")

        Dim insertsdfRouteTable As String = "INSERT INTO runningTrainsdb " & _
                          "(headcode, tableIndex,speed,actionRequired) " & _
                              "VALUES " & _
                              "(@headcode,@tableIndex,@speed,@actionRequired)"

        Dim sdfRouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectDBFRouteTable As String = "SELECT * FROM [runningTrainsdb] "
        sdfRouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectDBFRouteTable, nodeDbConn)

        Dim sdfRouteTableds As New DataSet
        sdfRouteTableda.Fill(sdfRouteTableds, "runningTrainsdb")
        Dim sdfRouteTabledt As DataTable = sdfRouteTableds.Tables("runningTrainsdb")

        sdfRouteTabledt.NewRow()
        Dim newDataRow As DataRow = sdfRouteTabledt.NewRow()
        newDataRow("headcode") = "1Z99"


        addDetailstotrainRunningDatabase(headcode, index, 99, sdfRouteTabledt)
        Dim TrackTableInsertCmd As New SqlCeCommand(insertsdfRouteTable, nodeDbConn)

        TrackTableInsertCmd.Parameters.Add("@headcode", _
                                         SqlDbType.NVarChar, 20, "headcode")
        TrackTableInsertCmd.Parameters.Add("@tableIndex",
                                           SqlDbType.NVarChar, 20, "tableIndex")
        TrackTableInsertCmd.Parameters.Add("@actionRequired",
                                                 SqlDbType.NVarChar, 20, "actionRequired")
        TrackTableInsertCmd.Parameters.Add("@speed",
                                                 SqlDbType.NVarChar, 20, "speed")


        sdfRouteTableda.InsertCommand = TrackTableInsertCmd
        sdfRouteTableda.Update(sdfRouteTableds, "runningTrainsdb")


    End Sub

    Public Sub DeleteRunningtrainsDB()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            routeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route dBconnection")
        End Try

        Dim RouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlDeleteRouteTable As String = "DELETE FROM runningTrainsdb "
        RouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlDeleteRouteTable, routeDbConn)

        Dim RouteTableds As New DataSet
        RouteTableda.SelectCommand.ExecuteNonQuery()
        Dim RouteTabledt As DataTable = RouteTableds.Tables("runningTrainsdb")

    End Sub


    Public Sub DeleteRunningWTTDB(ByVal headcode As String, ByVal index As String)

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            routeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route dBconnection")
        End Try

        Dim RouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlDeleteRouteTable As String = "DELETE FROM running_wtt WHERE headcode = '" _
                                            & headcode & "' AND tableIndex='" _
                                            & index & "'"
        RouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlDeleteRouteTable, routeDbConn)

        Dim RouteTableds As New DataSet
        RouteTableda.SelectCommand.ExecuteNonQuery()
        'Dim RouteTabledt As DataTable = RouteTableds.Tables("runningTrainsdb")

    End Sub

    Public Sub DeleteRunningHeadcodeDB(ByVal headcode As String, ByVal index As String)

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            routeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route dBconnection")
        End Try

        Dim RouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlDeleteRouteTable As String = "DELETE FROM runningTrainsdb WHERE headcode = '" _
                                            & headcode & "' AND tableIndex='" _
                                            & index & "'"
        RouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlDeleteRouteTable, routeDbConn)

        Dim RouteTableds As New DataSet
        RouteTableda.SelectCommand.ExecuteNonQuery()
        Dim RouteTabledt As DataTable = RouteTableds.Tables("runningTrainsdb")

    End Sub
    Private Sub TrainInfoForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        DeleteRunningtrainsDB()
        ' DeleteRunningWTTDB(headcode:=' index)
        RefreshDataView()

    End Sub

    Private Sub DataGridView1_SelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DataGridView1.SelectionChanged
        ' Get the current cell location.
        Dim y As Integer = DataGridView1.CurrentCellAddress.Y
        Dim x As Integer = DataGridView1.CurrentCellAddress.X
        Dim index As Integer
        Dim numberOfRows As Integer

        numberOfRows = DataGridView1.Rows.Count
        'Console.WriteLine(y.ToString + " " + x.ToString)
        If numberOfRows > 0 Then
            index = DataGridView1.Rows(y).Cells(6).Value
            wttObj.HighlightRow(index)
        End If

    End Sub


    Public Sub RefreshDataView()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodedB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open Node dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectNodeTable As String = "SELECT * FROM [runningTrainsdB] "
        Dim SelectCommand As SqlCeCommand
        Dim dataAdapter As SqlCeDataAdapter = New SqlCeDataAdapter(sqlSelectNodeTable, nodeDbConn)
        Dim commandBuilder As SqlCeCommandBuilder = New SqlCeCommandBuilder(dataAdapter)

        SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
        ' Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(sqlSelectNodeTable, nodeDbConn)

        Dim nodeReader As SqlCeDataReader

        ' Dim myConnection As SqlCeConnection
        Dim mycommand As SqlCeCommand

        mycommand = New SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
        nodeReader = mycommand.ExecuteReader



        ' Populate a new data table and bind it to the BindingSource.

        Dim table As DataTable = New DataTable()
        table.Locale = System.Globalization.CultureInfo.InvariantCulture
        dataAdapter.Fill(table)
        BindingSource1.DataSource = table



        'Resize the DataGridView columns to fit the newly loaded content.

        'dbGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

        'you can make it grid readonly.

        'dbGridView.ReadOnly = true; 

        '// finally bind the data to the grid

        DataGridView1.DataSource = BindingSource1


        
        ' DataGridView1.Columns("NodeType").Visible = False
        ' DataGridView1.Columns("NodeXPos").Visible = False
        ' DataGridView1.Columns("NodeYpos").DisplayIndex = 0
        ' DataGridView1.Columns("NodeID").DisplayIndex = 1
        DataGridView1.Columns(0).Width = 45 ' headcode
        DataGridView1.Columns(1).Width = 5  ' action required
        DataGridView1.Columns(2).Width = 20 ' speed 
        DataGridView1.Columns(3).Width = 5  ' direction
        DataGridView1.Columns(4).Width = 165 'notes

        DataGridView1.Columns("actionRequired").DisplayIndex = 0
        DataGridView1.Columns("headcode").DisplayIndex = 1
        DataGridView1.Columns("trainAction").DisplayIndex = 3
        ' DataGridView1.Columns("ShipCountry").DisplayIndex = 0
        ' DataGridView1.Columns("ShipName").DisplayIndex = 4
        ' trainobj = New Form2
        'trainobj.Show()
        'trainObjIsSet = True
        nodeDbConn.Close()

    End Sub




    Public Sub New(ByVal setwttObj As wttFrm)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        wttObj = setwttObj
        wttObj.GetParent.ReturnTrainInfoObj(Me)
    End Sub
End Class