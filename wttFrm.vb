Imports System.IO
Imports System.Data.SqlServerCe
Imports System.Drawing
Imports System.Data.OleDb
Imports System.Threading
Imports System.Math



Public Class wttFrm
    Dim trainobj As trainDataFrm
    Dim trainObjIsSet As Boolean = False
    Dim trainRunningObj As TrainInfoFrm
    Dim trainRunningIsSet As Boolean = False
    Dim m_Parent As Form1
    Dim FILENAME As String = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\trainselect\trainselect"
    Dim entryTime As String
    Dim exitTime As String
    Dim departTime As String
    Dim terminateTime As String
    Dim originateTime As String
    Dim entrytimeInteger As Integer
    'Public lineArray(10) As String



    Public Sub DeleteDB(ByVal dBName As String)

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            routeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route dBconnection")
        End Try

        Dim RouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlDeleteRouteTable As String = "DELETE FROM " & dBName
        RouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlDeleteRouteTable, routeDbConn)

        Dim RouteTableds As New DataSet
        RouteTableda.SelectCommand.ExecuteNonQuery()


    End Sub

    Public Sub UpdateRunning(ByVal headcode As String, ByVal setUnset As String)

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            routeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Route dBconnection")
        End Try

        Dim RouteTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlDeleteRouteTable As String = "UPDATE running_wtt SET running='" & _
                                            setUnset & _
                                            "'  WHERE headcode='" & headcode & "'"
        RouteTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlDeleteRouteTable, routeDbConn)

        Dim RouteTableds As New DataSet
        RouteTableda.SelectCommand.ExecuteNonQuery()


    End Sub


    Private Sub addDetailstoWTTDatabase(ByVal headcode As String, _
                                         ByVal trip As String, _
                                         ByVal lineIn As String, _
                                         ByVal lineOut As String, _
                                         ByVal notes As String, _
                                         ByVal exform As String, _
                                         ByVal forms As String, _
                                         ByVal trainIndex As String, _
                                         ByRef dt As DataTable)

        ' Add A Row
        Dim newRow As DataRow = dt.NewRow()
        newRow("headcode") = headcode
        If lineIn > 0 Then
            newRow("entryfrom") = m_Parent.lineArray(lineIn).description
        Else
            newRow("entryfrom") = "-"
        End If

        If lineOut > 0 Then
            newRow("entryto") = m_Parent.lineArray(lineOut).description
        Else
            newRow("entryto") = "-"
        End If

        newRow("entryTime") = TimeStringToInteger(entryTime)
        newRow("arrive") = terminateTime
        newRow("depart") = originateTime
        newRow("running") = "running"
        newRow("trainindex") = trainIndex
        newRow("notes") = notes
        newRow("exform") = exform
        newRow("forms") = forms
        newRow("trip") = trip
        newRow("lineIn") = lineIn

        dt.Rows.Add(newRow)

    End Sub

    Private Sub InsertintoTrainRunning(ByVal headcode As String, ByVal index As Integer)

        trainRunningObj.UpdateRunningdB(headcode, index)
        UpdateRunning(headcode, "~")
        RefreshDataView()

    End Sub


    Public Sub RemoveFromTrainRunning(ByVal headcode As String, ByVal index As Integer)

        trainRunningObj.DeleteRunningHeadcodeDB(headcode, index)
        trainRunningObj.RefreshDataView()
        UpdateRunning(headcode, " ")
        RefreshDataView()

    End Sub

    Public Sub RefreshDataView()

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
        Dim sqlSelectNodeTable As String = "SELECT * FROM running_wtt "
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

        ' nodeXpos = 100
        'nodeypos = 200
        ' nodeDbConn.Close()
        ' DataGridView1.Columns("NodeType").Visible = False
        Dim fontSize As Single = 7.0
        Dim style As FontStyle = FontStyle.Regular
        DataGridView1.Font = New Font("arial", fontSize, style)
        DataGridView1.Columns("notes").Visible = False
        DataGridView1.Columns("entryTime").Visible = False
        DataGridView1.Columns("trainindex").Visible = False
        DataGridView1.Columns("exform").Visible = False
        DataGridView1.Columns("forms").Visible = False
        DataGridView1.Columns("running").DisplayIndex = 0
        DataGridView1.Columns("headcode").DisplayIndex = 1
        DataGridView1.Columns("entryfrom").DisplayIndex = 2
        DataGridView1.Columns("arrive").DisplayIndex = 3
        DataGridView1.Columns("entryto").DisplayIndex = 4
        DataGridView1.Columns("depart").DisplayIndex = 4
        DataGridView1.Columns("forms").DisplayIndex = 8
        DataGridView1.Columns(0).Width = 35 ' headcode
        DataGridView1.Columns(1).Width = 45 ' arrive
        DataGridView1.Columns(2).Width = 45 ' depart
        DataGridView1.Columns(3).Width = 15 ' running
        DataGridView1.Columns(4).Width = 45 ' entrytime
        DataGridView1.Columns(5).Width = 65 'entryFrom
        DataGridView1.Columns(6).Width = 65 'entry to
        DataGridView1.Columns(7).Width = 25


    End Sub






    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        DeleteDB("running_Wtt")
        ImportWTTLineDBF()
        ImportWTTDBF()

        RefreshDataView()

        trainobj = New trainDataFrm

        trainobj.Show()
        trainObjIsSet = True
        trainRunningObj = New TrainInfoFrm(Me)


        trainRunningObj.Show()
        trainRunningIsSet = True
        Me.Location = New Point(10, 300)
        trainRunningObj.Location = New Point(Me.Location.X + Me.Width, Me.Location.Y)
        trainobj.Location = New Point(trainRunningObj.Location.X + trainRunningObj.Width, trainRunningObj.Location.Y)

    End Sub

    Private Sub DataGridView1_SelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DataGridView1.SelectionChanged
        ' Get the current cell location.
        Dim y As Integer = DataGridView1.CurrentCellAddress.Y
        UpdateRow(y)
    End Sub


    Public Sub ImportWTTDBF()
        'Setup connection to new sdf database
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim runningWTTsdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)

        Try
            runningWTTsdfDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open running WTT sdf dBconnection")
        End Try

        Dim runningWTTsdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectRunningWTTTable As String = "SELECT * FROM running_wtt ORDER BY entrytime "
        runningWTTsdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectRunningWTTTable, runningWTTsdfDbConn)

        Dim runningWTTsdfTableds As New DataSet
        runningWTTsdfTableda.Fill(runningWTTsdfTableds, "running_wtt")
        Dim runningWTTsdfTabledt As DataTable = runningWTTsdfTableds.Tables("running_wtt")

        Dim insertsdfRunningWTTTable As String = "INSERT INTO running_wtt " & _
                     "(headcode,entryfrom,entryto,arrive,depart,trainindex,entryTime,notes,exform,forms,trip,lineIn) " & _
                     "VALUES " & _
                     "(@headcode,@entryfrom,@entryto,@arrive,@depart,@trainindex,@entryTime,@notes,@exform,@forms,@trip,@lineIn)"


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

        Dim selectDBFAllRouteString As String = "SELECT * FROM wtt_train_ew01 "
        Dim DBFdBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectDBFAllRouteString, DBFdBaseConnection)
        'dBaseCommandSelectAll.CommandText = CommandType.Text


        Dim indexCount As Integer

        indexCount = 0
        'Set up a reader for the DB
        Dim DBFRouteReader As OleDbDataReader
        Try
            DBFRouteReader = DBFdBaseCommandSelectAll.ExecuteReader()
        Catch ex As Exception

            MessageBox.Show("can't execute")

        End Try
        ' Dim route As String
        'Dim node As String
        'Dim nodeto As String
        'Dim line As Integer
        Dim maxSpeed As Integer
        'Dim tc As Integer

        Try
            DBFRouteReader.Read()

        Catch ex As Exception

            MessageBox.Show("can't open connection")

        End Try
        Dim sdfRunningWTTTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectDBFRunningWTTTable As String = "SELECT * FROM running_wtt "
        sdfRunningWTTTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectDBFRunningWTTTable, runningWTTsdfDbConn)

        Dim sdfRunningWTTTableds As New DataSet
        sdfRunningWTTTableda.Fill(sdfRunningWTTTableds, "running_wtt")
        Dim sdfRouteTabledt As DataTable = sdfRunningWTTTableds.Tables("running_wtt")


        Dim headcode As String
        Dim trip As String
        Dim notes As String
        Dim lineIn As String
        Dim lineOut As String

        Dim exform As String
        Dim forms As String

        While DBFRouteReader.Read
            headcode = DBFRouteReader(0).ToString
            trip = DBFRouteReader(1).ToString
            lineIn = DBFRouteReader(3).ToString
            lineOut = DBFRouteReader(4).ToString
            maxSpeed = DBFRouteReader(3).ToString
            exform = DBFRouteReader(6).ToString
            forms = DBFRouteReader(7).ToString
            notes = DBFRouteReader(9).ToString
            ImportWTTLocnDBF(headcode, trip)
            addDetailstoWTTDatabase(headcode, trip, lineIn, lineOut, notes, exform:=exform, forms:=forms, trainIndex:=indexCount, dt:=sdfRouteTabledt)
            indexCount += 1
        End While
        DBFRouteReader.Close()




        Dim sdfRouteTableInsertCmd As New SqlCeCommand(insertsdfRunningWTTTable, runningWTTsdfDbConn)
        sdfRouteTableInsertCmd.Parameters.Add("@headcode", _
                                          SqlDbType.NVarChar, 20, "headcode")
        sdfRouteTableInsertCmd.Parameters.Add("@entryfrom", _
                                           SqlDbType.NVarChar, 20, "entryfrom")
        sdfRouteTableInsertCmd.Parameters.Add("@entryto",
                                          SqlDbType.NVarChar, 20, "entryto")
        sdfRouteTableInsertCmd.Parameters.Add("@arrive",
                                          SqlDbType.NVarChar, 20, "arrive")
        sdfRouteTableInsertCmd.Parameters.Add("@depart",
                                           SqlDbType.NVarChar, 20, "depart")
        'sdfRouteTableInsertCmd.Parameters.Add("@runnning",
        '                                  SqlDbType.NVarChar, 20, "running")
        sdfRouteTableInsertCmd.Parameters.Add("@trainindex",
                                          SqlDbType.NVarChar, 20, "trainindex")
        sdfRouteTableInsertCmd.Parameters.Add("@entryTime",
                                          SqlDbType.Int, 1, "entryTime")
        sdfRouteTableInsertCmd.Parameters.Add("@notes",
                                               SqlDbType.NVarChar, 200, "notes")
        sdfRouteTableInsertCmd.Parameters.Add("@exform",
                                                   SqlDbType.NVarChar, 20, "exform")
        sdfRouteTableInsertCmd.Parameters.Add("@forms",
                                                     SqlDbType.NVarChar, 20, "forms")
        sdfRouteTableInsertCmd.Parameters.Add("@trip", _
                                   SqlDbType.NVarChar, 20, "trip")

        sdfRouteTableInsertCmd.Parameters.Add("@lineIn", _
                                         SqlDbType.NVarChar, 20, "lineIn")

        sdfRunningWTTTableda.InsertCommand = sdfRouteTableInsertCmd
        Try
            sdfRunningWTTTableda.Update(sdfRunningWTTTableds, "running_wtt")
        Catch ex As Exception
        End Try

        'This populates the table with the node data
        runningWTTsdfDbConn.Close()


    End Sub

    Private Function TimeStringToInteger(ByVal timeStringIn As String) As Integer

        Dim timeStrArray() As String = timeStringIn.Split(":")
        Dim timeInteger As Integer
        timeInteger = timeStrArray(0) * 60 * 60 ' hours
        timeInteger += timeStrArray(1) * 60      ' minutes
        timeInteger += timeStrArray(2)           ' seconds
        Return timeInteger
    End Function

    Public Sub ImportWTTLineDBF()
        'Setup connection to new sdf database
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim runningWTTsdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)

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

        Dim selectDBFAllRouteString As String = "SELECT * FROM line"
        Dim DBFdBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectDBFAllRouteString, DBFdBaseConnection)
        'dBaseCommandSelectAll.CommandText = CommandType.Text


        Dim routeCount As Integer

        routeCount = 0
        'Set up a reader for the DB
        Dim DBFRouteReader As OleDbDataReader
        Try
            DBFRouteReader = DBFdBaseCommandSelectAll.ExecuteReader()
        Catch ex As Exception

            MessageBox.Show("can't execute")

        End Try
        'Dim route As String
        'Dim node As String
        Dim nodeto As String
        'Dim line As Integer
        'Dim maxSpeed As Integer
        'Dim tc As Integer



        Dim lineId As String
        Dim lineDesc As String
        Dim inNode As String
        Dim outNode As String
        Dim direction As String
        Dim firstDepart As Boolean = True

        'sdfRouteTabledt.Rows.Add(newDataRow)
        terminateTime = "-"
        originateTime = "-"

        While DBFRouteReader.Read
            lineId = DBFRouteReader(0).ToString
            direction = DBFRouteReader(1).ToString
            lineDesc = DBFRouteReader(7).ToString
            nodeto = DBFRouteReader(2).ToString
            inNode = DBFRouteReader(4).ToString
            outNode = DBFRouteReader(5).ToString
            ReDim Preserve m_Parent.lineArray(m_Parent.lineArray.Count + 1)
            m_Parent.lineArray(lineId).description = lineDesc
            m_Parent.lineArray(lineId).nodeIn = inNode
            m_Parent.lineArray(lineId).nodeOut = outNode
            m_Parent.lineArray(lineId).direction = direction
        End While
        DBFRouteReader.Close()


    End Sub


    Public Sub GetWTTEventFromTime(ByVal currentTime As Integer, ByRef nofEvents As Integer, ByRef headcodes As String, ByRef trainIndex As String)

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim wttDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            wttDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open wtt dBconnection")
        End Try

        Dim wttTableda As New SqlServerCe.SqlCeDataAdapter

        Dim RouteTableds As New DataSet
        ' RouteTableda.SelectCommand.ExecuteNonQuery()
        ' Dim RouteTabledt As DataTable = RouteTableds.Tables("subroutedB")


        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectwttTable As String = "SELECT headcode,linein,trainindex  FROM [running_wtt] " & _
            "WHERE (entryTime = '" & currentTime & "')"
        Dim SelectCommand As SqlCeCommand

        SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectwttTable, wttDbConn)

        Dim wttReader As SqlCeDataReader

        wttReader = SelectCommand.ExecuteReader
        Dim header As String
        Dim lineIn As Integer = 0
        nofEvents = 0
        While wttReader.Read
            header = wttReader(0).ToString
            lineIn = wttReader(1).ToString

            If lineIn > 0 Then
                Console.WriteLine("Service " & header & " at time " & currentTime)
                nofEvents += 1
                headcodes = header
                trainIndex = wttReader(2).ToString


            End If

        End While
        wttDbConn.Close()



    End Sub



    Public Sub ImportWTTLocnDBF(ByVal headcode As String, ByVal trip As String)
        'Setup connection to new sdf database
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim runningWTTsdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)

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

        Dim selectDBFAllRouteString As String = "SELECT * FROM wtrn_locn_ew01 WHERE (TRAIN = '" & headcode & "')" & _
                                           " AND (TRIP = " & trip & ")"
        Dim DBFdBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(selectDBFAllRouteString, DBFdBaseConnection)
        'dBaseCommandSelectAll.CommandText = CommandType.Text


        Dim routeCount As Integer

        routeCount = 0
        'Set up a reader for the DB
        Dim DBFRouteReader As OleDbDataReader
        Try
            DBFRouteReader = DBFdBaseCommandSelectAll.ExecuteReader()
        Catch ex As Exception

            MessageBox.Show("can't execute")

        End Try
        'Dim route As String
        Dim node As String
        Dim nodeto As String
        'Dim line As Integer
        'Dim maxSpeed As Integer
        'Dim tc As Integer



        Dim headcode1 As String
        Dim eventType As String
        Dim eventTime As String
        Dim firstDepart As Boolean = True

        'sdfRouteTabledt.Rows.Add(newDataRow)
        terminateTime = "-"
        originateTime = "-"

        While DBFRouteReader.Read
            headcode1 = DBFRouteReader(0).ToString
            node = DBFRouteReader(1).ToString
            nodeto = DBFRouteReader(2).ToString
            eventType = DBFRouteReader(3).ToString
            eventTime = DBFRouteReader(5).ToString
            ' addDetailstoRouteDatabase(route, node, nodeto, sdfRouteTabledt)
            ' Console.WriteLine(headcode & " entry at " & eventTime & " type " & eventType)


            If eventType = "N" Then
                entryTime = eventTime
            End If
            If eventType = "O" Then
                originateTime = eventTime.Remove(5, 3)
            End If
            If eventType = "X" Then
                exitTime = eventTime
            End If
            If eventType = "T" Then
                terminateTime = eventTime.Remove(5, 3)
            End If
            If eventType = "D" Then
                If firstDepart Then
                    departTime = eventTime
                    firstDepart = False
                End If
            End If


            routeCount = routeCount + 1
        End While
        DBFRouteReader.Close()
        entrytimeInteger = TimeStringToInteger(entryTime)
        Console.WriteLine(headcode & " entry at " & entryTime & " exit at " & exitTime)


    End Sub

    Private Function CheckNull(ByVal data As String) As String
        If data = Nothing Then
            data = ""
        End If
        Return data
    End Function

    Private Function CheckNull() As String
        Dim returnData As String = ""
        Return returnData
    End Function


    Private Sub UpdateRow(ByVal currentRow As Integer)

        Dim x As Integer = DataGridView1.CurrentCellAddress.X
        'Dim a As String
        Dim notes As String
        Dim arrivalTime As String
        Dim terminateTime As String
        Dim lineFrom As String
        Dim lineTo As String
        Dim exform As String
        Dim forms As String

        If trainObjIsSet Then
            trainobj.SetHeaderText(DataGridView1.Rows(currentRow).Cells(0).Value)
            arrivalTime = DataGridView1.Rows(currentRow).Cells(1).Value
            If arrivalTime = Nothing Then
                arrivalTime = "-"
            End If
            Try
                terminateTime = DataGridView1.Rows(currentRow).Cells(2).Value

            Catch ex As Exception
                terminateTime = "-"
            End Try



            notes = DataGridView1.Rows(currentRow).Cells(8).Value
            exform = DataGridView1.Rows(currentRow).Cells(9).Value
            If exform = Nothing Then
                exform = ""
            End If

            forms = DataGridView1.Rows(currentRow).Cells(10).Value
            If forms = Nothing Then
                forms = ""
            End If

            lineFrom = DataGridView1.Rows(currentRow).Cells(5).Value
            lineTo = DataGridView1.Rows(currentRow).Cells(6).Value

            trainobj.SetText(arrivalTime, terminateTime, _
                            notes, "3", _
                            forms, exform, lineFrom, lineTo)

        End If

        ' trainobj.Text = "Boobs "
        ' Write coordinates to console.
        'Console.WriteLine(" y is " & currentRow + " " + x.ToString)
    End Sub

    Public Sub HighlightRow(ByVal rowIndex As Integer)


        'DataGrid1.CurrentCell = New DataGridCell(hit.Row, hit.Column)
        DataGridView1.Rows(rowIndex).Selected = True
        DataGridView1.CurrentCell = DataGridView1(0, rowIndex)


        UpdateRow(rowIndex)

    End Sub

    Private Sub Form1_Load2(ByVal sender As System.Object, ByVal e As System.EventArgs)
        '
        ' Fill in the data grid on form load.
        '
        DataGridView1.DataSource = GetDataTable()
    End Sub

    Private Function GetDataTable() As DataTable
        '
        ' This Function needs to build the data table.
        '
        Return New DataTable()
    End Function
    Public Function GetParent() As Form1
        Return m_Parent
    End Function


    Private Sub DataGridView1_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim index As Integer
        Dim TrainIndex As Integer
        Dim headcode As String
        ' Dim noPreviousTrainTermDetails As Form1.termDetailsType
         index = DataGridView1.CurrentCellAddress.Y
        headcode = DataGridView1.Rows(index).Cells(0).Value
        TrainIndex = DataGridView1.Rows(index).Cells(7).Value
        Console.WriteLine("write this train " & DataGridView1.Rows(index).Cells(0).Value & " trainIndex " & TrainIndex)
        m_Parent.SetRoute("Z901", "S802")
        m_Parent.SetRoute("Z903", "S806")
        ' m_Parent.SetRoute("S802", "S804")
        ' m_Parent.SetRoute("S806", "S808")
        'm_Parent.SetRoute("S808", "S104")
        m_Parent.SetRoute("S104", "S144")
        m_Parent.SetRoute("S144", "S168")
        m_Parent.SetRoute("S168", "S176")
        m_Parent.SetRoute("S176", "S204")
        m_Parent.SetRoute("S204", "S212")
        m_Parent.SetRoute("S804", "S102")
        m_Parent.SetRoute("S102", "S140")
        m_Parent.SetRoute("S140", "S164")
        m_Parent.SetRoute("S164", "S172")
        'm_Parent.SetRoute("S172", "S200")
        m_Parent.SetRoute("S200", "S210")
        m_Parent.SetRoute("S210", "B015")
        m_Parent.SetRoute("S212", "S234")
        m_Parent.SetRoute("S234", "S258")

        m_Parent.SetRoute("S105", "S807")
        m_Parent.SetRoute("S147", "S105")
        m_Parent.SetRoute("S169", "S147")
        m_Parent.SetRoute("S173", "S169")
        m_Parent.SetRoute("S199", "S173")
        m_Parent.SetRoute("S207", "S199")
        m_Parent.StartClock()

        ' m_Parent.StartTrain(headcode, TrainIndex, noPreviousTrainTermDetails)
       
    End Sub

    Public Sub StartTrain(ByVal headcode As String, _
                          ByVal setIndex As String)
        InsertintoTrainRunning(headcode, setIndex)
        trainRunningObj.RefreshDataView()


    End Sub

    Public Sub CheckEvent(ByVal currentTime As Integer, _
                          ByRef currentEvent As String)
        'Dim nofEvents As Integer = 0
        'Dim headcodes As String = "none"

        '  GetWTTEventFromTime(currentTime, nofEvents, headcodes)
        'If nofEvents > 0 Then
        ' currentEvent = headcodes
        ' Else
        ' currentEvent = 0

        '        End If

    End Sub



    Public Sub New(ByVal set_m_parent As Form1)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_Parent = set_m_parent
    End Sub
End Class
