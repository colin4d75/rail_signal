Imports System.Threading
Imports System.Data.SqlServerCe


Public Class MoveLeverClass

    Private t As Thread
    Private pointBlankList As New ArrayList

    Private m_parent As Form1
    Private nodeindex As Integer
    Private leverID As String
    Private pointStatus As String
    Private setReverse As Boolean
    Private running As Boolean

    Delegate Sub UpdatePointStatusCallbackDelegate(ByVal aThreadName As MoveLeverClass, _
                                    ByVal setNodeID As String, ByVal setReverse As Boolean)

    Delegate Sub AddPointToBlankListCallbackDelegate(ByVal aThreadName As MoveLeverClass, _
                                    ByVal setPointID As String)

    Delegate Sub PointSetCompleteCallbackDelegate(ByVal aThreadName As String, _
                                        ByVal theColor As Color, _
                                        ByVal nodeIndex As String, _
                                        ByVal nodeid As String)
    Delegate Sub PointSetBlankCallbackDelegate(ByVal aThreadName As String, _
                                  ByVal theColor As Color, _
                                  ByVal nodeIndex As String, _
                                  ByVal nodeid As String)




    Sub New(ByVal set_mparent As Form1, ByVal lever As String, _
          ByVal setSetReverse As Boolean)
        t = New Thread(AddressOf ThrowPoint)
        leverID = lever
        m_parent = set_mparent
        setReverse = setSetReverse
        t.Start()
    End Sub

    Private Sub ThrowPoint()
        running = True
        SetBlanksFromLever(leverID)
        Thread.Sleep(5000)
        GetPointsFromLever(leverID)
    End Sub

    Private Function SetBlanksFromLever(ByVal leverID As String) As Point

        Dim sqlACTDetailsSelect As String = "SELECT  " & _
                     "nodeID FROM nodedB " & _
                       "WHERE (lever = '" & _
                     leverID & "')"
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
            Console.WriteLine("open another connection")
        Catch ex As Exception
            MessageBox.Show("can't open connection")
        End Try

        Dim dr As SqlCeDataReader

        Dim TrainTabledaDetailsSelectCommand As New SqlCeCommand(sqlACTDetailsSelect, objConn)
        dr = TrainTabledaDetailsSelectCommand.ExecuteReader()

        Dim pointID As String
        While (dr.Read())

            pointID = dr(0).ToString
            AddPointToBlank(pointID)
        End While
        ' Console.WriteLine("close it now")
        objConn.Close()
    End Function

    Private Function GetPointsFromLever(ByVal leverID As String) As Point

        Dim sqlACTDetailsSelect As String = "SELECT  " & _
                     "nodeID FROM nodedB " & _
                       "WHERE (lever = '" & _
                     leverID & "')"
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
            'Console.WriteLine("open another connection")
        Catch ex As Exception
            MessageBox.Show("can't open connection")
        End Try

        Dim dr As SqlCeDataReader

        Dim TrainTabledaDetailsSelectCommand As New SqlCeCommand(sqlACTDetailsSelect, objConn)
        dr = TrainTabledaDetailsSelectCommand.ExecuteReader()

        Dim pointID As String
        While (dr.Read())

            pointID = dr(0).ToString
            UpdatePointStatus(pointID, setReverse)
        End While
        'Console.WriteLine("close it now")
        objConn.Close()
    End Function

    Private Sub AddPointToBlank(ByVal pointID As String)
        'Console.WriteLine("Update Point " & pointID)
        m_parent.ClockDisplayLabel.Invoke(New AddPointToBlankListCallbackDelegate(AddressOf m_parent.AddPointToBlankListCallback), New Object() {Me, pointID})


    End Sub

    Private Sub UpdatePointStatus(ByVal nodeID As String, ByVal setReverse As Boolean)
        'Console.WriteLine("Update Point " & nodeID)
        m_parent.ClockDisplayLabel.Invoke(New UpdatePointStatusCallbackDelegate(AddressOf m_parent.UpdatePointStatusCallback), New Object() {Me, nodeID, setReverse})
        m_parent.ClockDisplayLabel.Invoke(New PointSetCompleteCallbackDelegate(AddressOf m_parent.PointSetCompleteCallback), New Object() {"clockref", Color.Black, nodeID, "1"})
        'Console.WriteLine("point set delay")
        'Thread.Sleep(1500)

        m_parent.ClockDisplayLabel.Invoke(New PointSetBlankCallbackDelegate(AddressOf m_parent.PointSetBlankCallback), New Object() {"clockref", Color.Black, nodeID, "1"})
    End Sub



End Class
