

Imports System
Imports System.Threading
Imports System.Data.SqlServerCe


Public Class clsClockTimer
    Private currentTime As Integer
    Public m_Parent As Form1
    ' Public m_TrainMon As trainMon
    Private running As Boolean
    Delegate Sub ChangeTextControlDelegate(ByVal aThreadName As String, ByVal aTextBox As ListBox, ByVal newText As String)

    Delegate Sub ChangeTimeLabelDelegate(ByVal aThreadName As String, ByVal aTextBox As Label, ByVal newText As Integer)
    Delegate Sub ChangeCurrentTimeDelegate(ByVal aThreadName As String, ByVal aTextBox As Label, ByVal currentTime As Integer)


    Sub New()
        currentTime = 0
        running = True
    End Sub

    Sub RunClock()
        While True

            m_Parent.ClockDisplayLabel.Invoke(New ChangeTimeLabelDelegate(AddressOf m_Parent.SetCurrentTime), New Object() {"clockref", m_Parent.ClockDisplayLabel, currentTime})
            currentTime = currentTime + 1
            Thread.Sleep(500)

        End While
        'forever
        ' ScheduleTrains()
    End Sub



    Private Sub ScheduleTrains()

        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder
        Dim nowTime As Integer = -1
        sConnection("Data Source") = "F:\MRCCC_Source_Release2\MRCCC\Database1.sdf"
        'sConnection("Persist Security Info") = False
        'Console.WriteLine(sConnection.ConnectionString)
        Dim objConn As New SqlCeConnection(sConnection.ConnectionString)

        Try

            objConn.Open()

        Catch ex As Exception

            'Console.WriteLine("can't open connection")

            MessageBox.Show("can't open connection")

        End Try

        Dim sqlTrainDataSelect As String
        Dim dr As SqlCeDataReader

        While True

            While nowTime = currentTime
                'if we've already been aroud the loop for this time,
                'then don't do it again
                Thread.Sleep(100)
            End While
            nowTime = currentTime
            sqlTrainDataSelect = "SELECT " & _
                                 "TrainENT, TrainENP, TrainServiceId " & _
                                 "FROM TrainTable " & _
                                  "WHERE (TrainTable.TrainENP <> '0') AND (TrainTable.TrainENT = '" & _
                                   currentTime & "')"

            Dim TrainTabledaSelectCommand As New SqlCeCommand(sqlTrainDataSelect, objConn)

            dr = TrainTabledaSelectCommand.ExecuteReader()
            'Get everything from the database that has an entry point as now
            While (dr.Read())

                Dim simENT As Integer = dr.GetInt32(0)
                Dim simENP As String = dr.GetString(1)
                Dim trainId As String = dr.GetString(2)
                TrainEntry(trainId, 100000)

            End While
            'Also need to get everything that is due to depart now, and is the
            'first in the list of timing points. As we are using this to launch
            'trains(threads) using the first in the list gives us the entry point
            'of all departing trains.
            'If we assume that all departures are the result of some arrival, then
            'we could probably use this too. This does not, however, take into account
            'the cases where we start running with a train already at a platform, ready
            'to depart. This scenario is probably a bit complicated for the moment.


            sqlTrainDataSelect = "SELECT " & _
                "TrainENT, TrainENP, TrainServiceId " & _
                "FROM TrainTable " & _
                "WHERE (TrainTable.TrainDEP = " & _
                currentTime & ")"


            Dim TrainTableDEPdaSelectCommand As New SqlCeCommand(sqlTrainDataSelect, objConn)
            dr = TrainTableDEPdaSelectCommand.ExecuteReader()
            'Get everything from the database that has an departure point as now
            While (dr.Read())

                Dim simENT As Integer = dr.GetInt32(0)
                Dim simENP As String = dr.GetString(1)
                Dim trainId As String = dr.GetString(2)

                TrainDepart(trainId, 100000)

            End While
            m_Parent.ClockDisplayLabel.Invoke(New ChangeTimeLabelDelegate(AddressOf m_Parent.SetCurrentTime), New Object() {"clockref", m_Parent.ClockDisplayLabel, currentTime})
            currentTime = currentTime + 1
            Thread.Sleep(500)

        End While
        'forever

    End Sub


    Sub TrainEntry(ByVal TrainServiceId As String, ByVal tickDelay As Integer)
        'A train entering from an external entry point

        ' Dim TrainObject As New TrainWithDetails(TrainServiceId, "entering", m_Parent, m_TrainMon)


    End Sub
    Sub TrainDepart(ByVal TrainServiceId As String, ByVal tickDelay As Integer)
        'A train departing from a point under our control. Typically when on cervice terminates
        'an turns into a new service

        'Dim TrainObject As New TrainWithDetails(TrainServiceId, "departing", m_Parent, m_TrainMon)

    End Sub

End Class

