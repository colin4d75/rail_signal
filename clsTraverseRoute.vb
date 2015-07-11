Imports System
Imports System.Threading
Imports System.Data.SqlServerCe
Imports System.Data.OleDb
Imports System.Math
Imports System.Windows.forms.label



Public Class clsTraverseRoute
    ' Dim FILENAME As String = "C:\Users\Colin\Downloads\DataBuilder\Test 1\modeldata"
    Dim FILENAME As String = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\trainselect\trainselect"

    Const constDistBeforeSignalTC As Integer = 40
    Const constStandardSignalTCDistance As Integer = 20
    Const constPreStoppingSpeed As Integer = 10
    Private t As Thread
    Private trainReadyToDepart As Boolean
    Public flashTRTSIndex As String
    Private lastTrackCircuit As Integer

    Private headcode As String
    Private direction As String
    Private TRTSNodeIndex As String
    Private trip As String
    Private isStopping As Boolean
    Private entrypoint As String
    Private nextPlatformlength As String
    Private trainTermDetails As Form1.termDetailsType
    Private nextTimingLoc As String
    Private nextTimingTime As String
    Private nextTimingType As String
    Private trainIndex As String
    Private stationStopAtSignal As Boolean
    Public m_parent As Form1
    Private trainAction As String
    Private trainDistance As Integer = 0
    Private lastDistance As Integer = 0
    Private lastSpeed As Integer = 0
    Private trackEndSpeed As Single
    Private stoptrackId As Integer
    Private serviceForms As String ' What this service turns into
    Private serviceExForms As String ' What this service was before
    Private totalDistance As Integer
    Private currentTrackDistance As Integer
    Private stoppingPoint As Integer
    Private stopPlatform As String
    Private stopLocation As String
    Private stationStop As Boolean
    Private exitSignal As String
    Private routelist As New ArrayList()
    Private traversedRoute As New ArrayList()
    Private traversedDistance As New ArrayList()
    Private trackCircuitArray As New ArrayList()
    Private trackArray As New ArrayList()
    Private lineSpeedArray As New ArrayList()
    Private trainLength As Integer = 225
    Private objCalc As clsCalc
    Private removeTimingAtStop As Boolean
    Private accInitSpeedArray(1) As Integer
    Private accDistanceArray(1) As Integer
    Private accTypeArray(1) As Integer
    Private routeStillSet As Boolean
    Private trainTerminated As Boolean
    Private trainExited As Boolean
    Private routeExitsignal As String
    Private continueOnRoute As Boolean
    Private trainSpeed As Integer
    Private linespeed As Integer
    Private acceleration As Single
    Private brakingDistance As Single
    Private acceleratingDistance As Single
    Private distanceArray As New ArrayList
    Private brakingrate As Single = -1
    Private accelRate As Single = 0.75
    Private subRouteTracks As Integer
    Private subRouteDistance As Integer
    Private absoluteDistance As Integer
    Private signalDistance As Integer
    Private startingDistance As Integer
    Private stopAtDistance As Integer
    Private stationDistance As Integer
    Private routeDistance As Integer
    Private objCab As clstrainCab
    Private currentTrack As String = " 0"
    Private braking As Boolean = False
    Private accelerating As Boolean = False
    Private brakingToSpeed As Integer
    Private maxAccelToSpeed As Integer ' Maximum speed to accelerate to when starting
    Private signalStatus As Integer
    Private statusReady As Boolean
    Private heldAtSignal = False
    Private accelDataobj As accelDataType
    Private timesArrayList = New ArrayList
    Private routeToFringe As Boolean
    Private offDisplayExit As Boolean

    Structure timeEventType
        Dim eventTimeInt As Integer
        Dim eventTime As String
        Dim eventType As String
        Dim eventLocation As String
    End Structure

    Structure termDetailsType
        Dim location As Integer
        Dim platform As String
        Dim distance As String
        Dim direction As String
        Dim occupiedTCs As ArrayList
    End Structure

    Structure newtype
        Dim a As String
    End Structure
    Structure accelDataType

        Dim accelDistance As Integer
        Dim coastDistance As Integer
        Dim decelDistance As Integer
    End Structure


    'Private subRoutelist As New ArrayList()

    'Delegates
    Delegate Sub SetTcCallbackDelegate(ByVal aThreadName As String, _
                                       ByVal tc As Integer, _
                                       ByVal setclear As Boolean)

    Delegate Sub SetTrackTcCallbackDelegate(ByVal aThreadName As String, _
                                        ByVal track As Integer, _
                                        ByVal setclear As Boolean)

    Delegate Sub createCabCallbackDelegate(ByVal aThreadName As String)
    Delegate Sub createLinespeedCallbackDelegate(ByVal aThreadName As String)

    Delegate Sub GetSignalStatusCallbackDelegate(ByVal aThreadName As clsTraverseRoute,
                                                 ByVal nodeID As String)

    Delegate Sub UpdateCabCallbackDelegate(ByVal aThreadName As String, _
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

    Delegate Sub UpdateLinespeedCallbackDelegate(ByVal aThreadName As String, _
                             ByVal distanceArray As ArrayList, _
                             ByVal LineSpeedArray As ArrayList, _
                             ByVal accdistancearray() As Integer, _
                             ByVal accinitSpeedArray() As Integer)

    Delegate Sub AddLineToLinespeedCallbackDelegate(ByVal aThreadName As String, _
                                                    ByVal penColor As Color, _
                                   ByVal xStart As Integer, ByVal yStart As Integer, _
                                   ByVal xEnd As Integer, ByVal yEnd As Integer
                                   )

    Delegate Sub RemovetrainDetailsCallbackDelegate(ByVal aThreadName As String, _
                                                    ByVal headcode As String, _
                                                    ByVal index As String)

    Delegate Sub StartNewtrainCallbackDelegate(ByVal aThreadName As String, _
                                                   ByVal headcode As String, _
                                                   ByVal index As String, _
                                                   ByVal trainTermDetails As Form1.termDetailsType)

    Sub New(ByVal set_m_parent As Form1, ByVal setHeadcode As String, ByVal setIndex As String, _
            ByVal settrainTermDetails As Form1.termDetailsType)
        m_parent = set_m_parent
        headcode = setHeadcode
        trainTermDetails = settrainTermDetails
        signalDistance = trainTermDetails.distance
        '  serviceForms = setForms
        trainIndex = setIndex
        t = New Thread(AddressOf RunTrain)
        t.IsBackground = True
        t.Start()
        t.Name = setHeadcode
    End Sub

    Private Sub ScaledSleep(ByVal SleepTime As Integer)

        Dim timeStep As Single
        timeStep = 0.01
        Thread.Sleep(SleepTime * timeStep)

    End Sub

    Private Sub scheduleTrackClear(ByVal trainspeed As Integer, _
                                   ByVal acceleration As Integer, _
                                   ByVal trackIndex As Integer)


        Dim timeToClear As Single
        Dim scheduleObj As clsScheduletcClear

        timeToClear = objCalc.CalcTraverseTime(trainspeed, acceleration, trainLength)
        'we could decelerate to zero and stop before the train exits the track
        If timeToClear > 0 Then
            scheduleObj = New clsScheduletcClear(m_parent, trackIndex, timeToClear)
        End If



    End Sub

    Private Sub setAcceleratonArray(ByVal distancePoint As Integer, _
                                     ByVal accelerationType As Integer)
        accDistanceArray(0) = distancePoint
        accTypeArray(0) = accelerationType
    End Sub



    Private Sub removeFromAcceleratonArray(ByVal distancePoint As Integer, _
                                       ByVal accelerationType As Integer)
        Dim copyTrue As Boolean = False
        For arrayCounter As Integer = 0 To accDistanceArray.Count - 1

            If accDistanceArray(arrayCounter) = distancePoint Then
                copyTrue = True
            End If
            If copyTrue Then
                If arrayCounter < accDistanceArray.Count - 1 Then
                    accDistanceArray(arrayCounter) = accDistanceArray(arrayCounter + 1)
                    accTypeArray(arrayCounter) = accTypeArray(arrayCounter + 1)
                End If
            End If
        Next
        If copyTrue Then
            ReDim Preserve accDistanceArray(accDistanceArray.Count - 2)
            ReDim Preserve accTypeArray(accTypeArray.Count - 2)

        End If
        Array.Sort(accDistanceArray, accTypeArray)
    End Sub

    Private Function GetTotalDistance() As Integer
        Dim totalDistance As Integer = 0
        For Each trackDistance In distanceArray
            totalDistance = totalDistance + trackDistance

        Next
        routeDistance = totalDistance
        Return totalDistance
    End Function

    Private Function GetTraversedDistance() As Integer
        Dim trailingDistance As Integer = 0
        For Each distance In traversedDistance
            trailingDistance = trailingDistance + distance
        Next
        Return trailingDistance
    End Function

    Private Sub SlowSubroute()
        Dim firstSubroute As Boolean = True
        For Each route As String In routelist
            GetSubRoute(route)
            If firstSubroute Then
                subRouteTracks = trackArray.Count
                firstSubroute = False
            End If
        Next
        CalcStop()
    End Sub

    Private Sub AccelerateSubroute()
        Dim firstSubroute As Boolean = True
        For Each route As String In routelist
            GetSubRoute(route)
            If firstSubroute Then
                subRouteTracks = trackArray.Count
                firstSubroute = False
            End If

        Next
        CalcStart()
    End Sub

    Private Sub GetTrackSpeeds()
        'Get Subroutes
        Dim firstSubroute As Boolean = True
        For Each route As String In routelist
            GetSubRoute(route)
            If firstSubroute Then
                subRouteTracks = trackArray.Count
                firstSubroute = False
            End If
        Next
        '  For trackCount As Integer = 0 To subRouteTracks - 1

        'First work out if we need to slow down or stop
        Dim distance As Integer = 0
        ' Dim trackEndSpeed As Integer
        Dim trackDistance As Integer
        continueOnRoute = True
        For Each trackId In trackArray
            If continueOnRoute Then
                trackDistance = GetTrackLength(trackId)
                distance = distance + trackDistance
                distanceArray.Add(trackDistance)
                If trackDistance < 1 Then
                    Console.WriteLine("This can't happen so must be an error")
                End If
            Else
                Exit For
            End If
        Next
        routeDistance = distance
        totalDistance = distance
        totalDistance = GetTotalDistance()
        ' Next

    End Sub
    Private Sub GetDirection()
        'calculate the direction of the train
        'First of all try the entry point to see if its
        'direction is fixed
        For Each timingEntry In timesArrayList
            If (timingEntry.eventType = "N") Or (timingEntry.eventType = "X") Then
                'for other types of event Its a location rather than a line
                Dim eventDirection As String
                eventDirection = m_parent.lineArray(timingEntry.eventlocation).direction
                If eventDirection = -1 Or eventDirection = 1 Then

                    direction = m_parent.lineArray(timingEntry.eventlocation).direction
                    Exit For
                End If
            End If

        Next
        If direction = Nothing Then
            Console.Write("This is an error")
        End If
    End Sub


    Public Sub GetAcceleration()
        acceleration = accelRate
        ModifySpeedsForStopping()
        CalcTargetSpeeds()
        DrawMaxSpeeds()
        CalcTraverseSpeeds()
    End Sub

    Private Function ConvertToXCoord(ByVal distance As Integer, _
                                    ByVal routeDistance As Integer, _
                                    ByVal displayWidth As Integer) As Integer


        Dim xcoord As Single
        If distance = 0 Then
            xcoord = 0
        Else
            xcoord = distance / routeDistance
            xcoord = (xcoord * displayWidth)
        End If
        Return xcoord

    End Function

    Private Function ConvertToYCoord(ByVal Linespeed As Integer) As Integer
        Dim baseline As Integer = 180
        Dim ycoord As Integer
        ycoord = baseline - (Linespeed / 2)
        Return ycoord
    End Function

    Private Sub DrawActualSpeedLine(ByVal startDistance As Integer, ByVal endDistance As Integer, _
                                    ByVal startSpeed As Integer, ByVal endSpeed As Integer)
        Dim startxcoord As Integer
        Dim endxcoord As Integer
        Dim startycoord As Integer
        Dim endycoord As Integer
        Dim displaywidth As Integer = 489

        startxcoord = ConvertToXCoord(startDistance, totalDistance, displaywidth) + 30
        endxcoord = ConvertToXCoord(endDistance, totalDistance, displaywidth) + 30
        startycoord = ConvertToYCoord(startSpeed)
        endycoord = ConvertToYCoord(endSpeed)
        DrawLinespeed(Color.White, startxcoord, startycoord, endxcoord, endycoord)


    End Sub

    Private Sub DrawCurrentPositon()
        Dim startxcoord As Integer
        Dim endxcoord As Integer
        Dim startycoord As Integer
        Dim endycoord As Integer
        Dim displaywidth As Integer = 489

        startxcoord = ConvertToXCoord(lastDistance, totalDistance, displaywidth) + 30
        endxcoord = ConvertToXCoord(absoluteDistance, totalDistance, displaywidth) + 30
        startycoord = ConvertToYCoord(lastSpeed)
        endycoord = ConvertToYCoord(trainSpeed)
        DrawLinespeed(Color.Purple, startxcoord, startycoord, endxcoord, endycoord)

        lastDistance = absoluteDistance
        lastSpeed = trainSpeed

    End Sub

    Private Function TimeStringToInteger(ByVal timeStringIn As String) As Integer

        Dim timeStrArray() As String = timeStringIn.Split(":")
        Dim timeInteger As Integer
        timeInteger = timeStrArray(0) * 60 * 60 ' hours
        timeInteger += timeStrArray(1) * 60      ' minutes
        timeInteger += timeStrArray(2)           ' seconds
        Return timeInteger
    End Function


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
        'Dim nodeto As String
        'Dim line As Integer
        'Dim maxSpeed As Integer
        'Dim tc As Integer



        Dim headcode1 As String
        Dim eventType As String
        Dim eventTime As String
        Dim platform As String
        Dim location As String
        Dim firstDepart As Boolean = True
        Dim timeEvent As New timeEventType

        'sdfRouteTabledt.Rows.Add(newDataRow)
        'terminateTime = "-"
        'originateTime = "-"

        While DBFRouteReader.Read
            headcode1 = DBFRouteReader(0).ToString ' headcode
            node = DBFRouteReader(1).ToString 'trip
            location = DBFRouteReader(2).ToString 'location
            eventType = DBFRouteReader(3).ToString 'type
            'allowsec
            eventTime = DBFRouteReader(5).ToString 'time
            platform = DBFRouteReader(8).ToString 'platform
            timeEvent.eventLocation = location
            timeEvent.eventType = eventType
            timeEvent.eventTimeInt = TimeStringToInteger(eventTime)
            timeEvent.eventTime = eventTime.Remove(5, 3)
            timesArrayList.Add(timeEvent)
        End While
        DBFRouteReader.Close()
    End Sub



    Private Sub GetTrainTimes()
        ImportWTTLocnDBF(headcode, 0)


    End Sub

    Public Sub DrawMaxSpeeds()

        'Dim startxccord As Integer
        'Dim endxcoord As Integer
        'Dim startycoord As Integer
        'Dim endycoord As Integer
        Dim displaywidth As Integer = 489


        For indexcount As Integer = 0 To accInitSpeedArray.Count - 2
            'startxccord = ConvertToXCoord(accDistanceArray(indexcount), totalDistance, displayWidth) + 30
            'endxcoord = ConvertToXCoord(accDistanceArray(indexcount + 1), totalDistance, displayWidth) + 30
            'startycoord = ConvertToYCoord(accInitSpeedArray(indexcount))
            'endycoord = ConvertToYCoord(accInitSpeedArray(indexcount + 1))
            'DrawLinespeed(Color.Blue, startxccord, startycoord, endxcoord, endycoord)
        Next

    End Sub
    Private Function RoundTrackSpeed(ByVal accelEndSpeed As Integer)
        If accelEndSpeed = trackEndSpeed + 1 Then
            accelEndSpeed = trackEndSpeed
        End If
        If accelEndSpeed = trackEndSpeed - 1 Then
            accelEndSpeed = trackEndSpeed
        End If
        If accelEndSpeed = trackEndSpeed + 2 Then
            accelEndSpeed = trackEndSpeed
        End If
        If accelEndSpeed = trackEndSpeed - 2 Then
            accelEndSpeed = trackEndSpeed
        End If
        Return accelEndSpeed
    End Function

    Private Sub CheckTrainArriveTimingPoint()
        If nextTimingType = "T" Then
            'then train is going to terminate here     
            trainTerminated = True
            Console.WriteLine("Train " & headcode & " has terminated")

        End If
        RemoveCurrentTimingPoint()

    End Sub

    Private Sub CheckTrainExitedTimingPoint()
        If nextTimingType = "X" Then
            'then train is going to exit here  
            'check location here as well
            trainExited = True
            Console.WriteLine("Train " & headcode & " has exited")

        End If
        'RemoveCurrentTimingPoint()

    End Sub

    Private Sub RemoveCurrentTimingPoint()
        timesArrayList.removeat(0)
        GetTimingPoint()
    End Sub



    Private Sub CheckIfAtNextTimingLoc(ByVal track As String)
        Dim currentLocation As String
        currentLocation = m_parent.TrackArrayList(track).location
        ' Console.WriteLine("At track " & track & " on line " & m_parent.TrackArrayList(track).line)

        'Console.WriteLine("Aha this is a timing point " & m_parent.TrackArrayList(track).location & " of type " & nextTimingType & " from " & m_parent.TrackArrayList(track).nodefrom & " to " & m_parent.TrackArrayList(track).nodeto & " platform is " & m_parent.TrackArrayList(track).platform)

        If m_parent.TrackArrayList(track).platform <> " " Then
            Console.WriteLine("Aha this is a timing point " & m_parent.TrackArrayList(track).location & " of type " & nextTimingType & " from " & m_parent.TrackArrayList(track).nodefrom & " to " & m_parent.TrackArrayList(track).nodeto & " platform is " & m_parent.TrackArrayList(track).platform)
            If currentLocation = nextTimingLoc Then
                'we're at the next timing point
                If nextTimingType = "A" Or nextTimingType = "T" Then
                    'train is stopping, so wait for train to stop before advancing
                    removeTimingAtStop = True
                    stopPlatform = m_parent.TrackArrayList(track).platform
                    stopLocation = m_parent.TrackArrayList(track).location
                    Console.WriteLine("Don't remove it here though")
                Else
                    'Record time and advance to next timing point
                    RemoveCurrentTimingPoint()
                    'removeTimingAtStop = False
                End If
            Else
                'removeTimingAtStop = False
            End If
        End If


    End Sub

    Private Sub AdvancetimingPointAtStation()
        If nextTimingType = "D" Then
            'log timing point for a depart
            Console.WriteLine(headcode & " departed " & nextTimingLoc & " at ")
            RemoveCurrentTimingPoint()

        ElseIf nextTimingType = "T" Then
            'log timing point for a Terminate
            trainTerminated = True
            Console.WriteLine(headcode & " Terminated " & nextTimingLoc & " at ")
            RemoveCurrentTimingPoint()
        Else
            'Problem
            Console.WriteLine("Error:: timing point not for a depart")
        End If

    End Sub

    Private Sub AccelCoastDecel(ByRef linespeed As Integer, ByRef trackcount As Integer, _
                                ByRef accelEndSpeed As Integer, ByRef currentDistance As Integer)

        linespeed = lineSpeedArray(trackcount)
        accelDataobj = objCalc.CalcAccelUntilDistance(trainSpeed, trackEndSpeed, acceleration, brakingrate, currentTrackDistance, linespeed)
        Console.WriteLine("train distance " & absoluteDistance & " " & trainDistance & " accel distance " & accelDataobj.accelDistance & " coast " & accelDataobj.coastDistance & " decel " & accelDataobj.decelDistance & " trainSpeed " & trainSpeed)
        accelEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, accelDataobj.accelDistance)
        accelEndSpeed = RoundTrackSpeed(accelEndSpeed)
        Console.WriteLine("currentDistance " & currentDistance & " accel distance " & accelDataobj.accelDistance)
        DrawActualSpeedLine(currentDistance, currentDistance + accelDataobj.accelDistance, trainSpeed, accelEndSpeed)
        Console.WriteLine("normal acceleration " & accelDataobj.accelDistance & "  " & trainDistance)

        acceltrack(accelDataobj.accelDistance)

        trainSpeed = accelEndSpeed
        currentDistance = currentDistance + accelDataobj.accelDistance
        Console.WriteLine("currentDistance " & currentDistance & " coast distance " & accelDataobj.coastDistance)

        DrawActualSpeedLine(currentDistance, currentDistance + accelDataobj.coastDistance, trainSpeed, accelEndSpeed)
        Coasttrack(accelDataobj.coastDistance)

        currentDistance = currentDistance + accelDataobj.coastDistance
        accelEndSpeed = objCalc.CalcEndSpeed(trainSpeed, brakingrate, accelDataobj.decelDistance)
        accelEndSpeed = RoundTrackSpeed(accelEndSpeed)
        Console.WriteLine("currentDistance " & currentDistance & " decel distance " & accelDataobj.decelDistance)

        DrawActualSpeedLine(currentDistance, currentDistance + accelDataobj.decelDistance, trainSpeed, accelEndSpeed)
        Console.WriteLine("From here 5 distance is " & accelDataobj.decelDistance)

        DecelTrack(accelDataobj.decelDistance)



        currentDistance = currentDistance + accelDataobj.decelDistance
        trainSpeed = accelEndSpeed

        Console.WriteLine("From here Currentdistance " & currentDistance)


    End Sub



    Public Sub CalcTraverseSpeeds()
        Dim accelEndSpeed As Integer
        Dim currentDistance As Integer
        Dim trackdistance As Integer
        Dim trackcount As Integer
        Dim trackEnd As Integer
        'Dim atThistrack As Integer
        'trainSpeed = CType(startspeedlabel.Text, Integer)
        If trainSpeed > accInitSpeedArray(0) Then
            Console.WriteLine("DEBUG:: Init speed is too high " & trainSpeed & " > " & accInitSpeedArray(0))
            trainSpeed = accInitSpeedArray(0) - 1
        End If

        trackcount = 0
        If stationStop Then
            'reset the current distance if we're not starting again after stopping
            currentDistance = stopAtDistance
            absoluteDistance = stopAtDistance

        End If
        trackdistance = 0
        trackEnd = 0
        lastDistance = 0
        lastSpeed = trainSpeed
        If (distanceArray.Count = 1) And routeToFringe Then
            'Console.WriteLine("Fringe  Train distance " & routeToFringe & " " & trainDistance & " current distance " & currentDistance & " traversedroute " & traversedRoute.Count & " " & traversedRoute(0) & "  " & traversedDistance(0) & " track end " & trackEnd)

            While (traversedDistance.Count > 1)
                Dim setExitWhile As Boolean = False
                Dim coastDistance As Integer
                coastDistance = traversedDistance(1) - trainDistance
                If coastDistance > distanceArray(0) Then
                    coastDistance = distanceArray(0)
                    distanceArray(0) = trainLength
                    Console.WriteLine("start train in this track " & trainDistance & " subtrack " & " tc " & trackcount & " " & trackArray(trackcount) & " speed " & trainSpeed & " absolute distance " & absoluteDistance)

                    SetTrackTc(trackArray(trackcount))
                    traversedRoute.Add(trackArray(trackcount))
                    traversedRoute.Add(trackArray(trackcount))
                    lastTrackCircuit = trackArray(trackcount)
                    traversedDistance.Add(trainDistance + trainLength)
                    traversedDistance.Add(trainDistance + trainLength + coastDistance)
                    currentTrack = trackArray(trackcount)

                    setExitWhile = True
                End If
                Coasttrack(coastDistance)
                If setExitWhile Then
                    'Exit While
                End If
                If traversedDistance.Count = 1 Then
                    'Console.WriteLine("at end of Fringe " & trackcount & " " & trainDistance & " " & currentDistance)
                    ClearTailEndTrack()
                    CheckTrainExitedTimingPoint()
                End If
            End While
         Else
            For subtrackCount As Integer = 0 To accInitSpeedArray.Count - 2
                If stationStop Then
                    'We need to check to see if the route is set so we can 
                    'continue after the station stop

                    'We need to check to see if the platform we've stopped at 
                    'is at the end of the route. If it is, then we then need to check
                    'to see if there are any other routes set.
                    If routelist.Count = 1 Then
                        'this is the only route, but are we at the end?
                        If stopAtDistance > subRouteDistance - 50 Then
                            'we're pretty close to the end of the track, and should be
                            ' able to see the signal, so we should just wait until signal clears
                            ' set TRTS if there is one
                            stationStop = False
                            stationStopAtSignal = True

                            TRTSNodeIndex = m_parent.GetNodeIndex(exitSignal)
                            m_parent.FlashTRTS(TRTSNodeIndex, Me)
                            Exit Sub
                        End If

                    End If

                    'If the platform is not at the end of a route, then we should move off,
                    'but remembering what the state of the last signal was if we can't see
                    'the next (which we should assume is the case as we're not at the end
                    'of a route) and be prepared to stop at the next signal if necessary


                    Console.WriteLine("starting after a station stop " & trainDistance & " count " & subtrackCount)
                    Console.WriteLine("track " & trackArray(trackcount) & " track id " & trackcount & " speed " & trainSpeed)
                    isStopping = False

                     For accArrayindex As Integer = 0 To accDistanceArray.Count - 1
                        If accDistanceArray(accArrayindex) <= currentDistance Then
                            accDistanceArray(accArrayindex) = currentDistance
                             trackcount = accArrayindex
                        End If

                    Next
                    trackEnd = currentDistance
                    If accDistanceArray(subtrackCount) <= currentDistance Then
                        'accDistanceArray(subtrackCount) = currentDistance
                        ' lastTrackCircuit = trackArray(trackcount)
                        'trackcount = subtrackCount
                    End If
                    If accDistanceArray(subtrackCount + 1) >= currentDistance Then
                        If trainSpeed > 0 Then
                            'Train has moved off now
                            stationStop = False
                            Console.WriteLine("end of station Stop " & trainDistance)

                        End If
                    End If

                    If subtrackCount = trackcount Then
                        AdvancetimingPointAtStation()
                        Console.WriteLine("end of station Stop " & trainDistance)

                    End If
                Else
                    'not a station stop

                    If lastTrackCircuit <> trackArray(trackcount) Then
                        Console.WriteLine("start train in this track " & trainDistance & " subtrack " & subtrackCount & " tc " & trackcount & " " & trackArray(trackcount) & " speed " & trainSpeed & " absolute distance " & absoluteDistance)
                        'check timing event

                        CheckIfAtNextTimingLoc(trackArray(trackcount))
                        SetTrackTc(trackArray(trackcount))
                        traversedRoute.Add(trackArray(trackcount))
                        lastTrackCircuit = trackArray(trackcount)
                        traversedDistance.Add(trainDistance + trainLength)
                        currentTrack = trackArray(trackcount)
                    End If
                End If

                If trackcount = (trackArray.Count - 1) And (routeToFringe = False) Then
                    'it's the last track. 
                    'We'll need to stop in this track, but if it's a long track,
                    'then it could be we've got to travel a bit before stopping
                    Dim stopDistance As Integer
                    Console.WriteLine("stop now from " & trainSpeed & " sub trackcount " & subtrackCount & " absolute " & absoluteDistance & " trackebdspeed " & trackEndSpeed)



                    trackEndSpeed = accInitSpeedArray(subtrackCount + 1)


                    'linespeed = lineSpeedArray(trackcount)
                    'accelDataobj = objCalc.CalcAccelUntilDistance(trainSpeed, trackEndSpeed, acceleration, brakingrate, currentTrackDistance, linespeed)
                    '
                    currentTrackDistance = accDistanceArray(subtrackCount + 1) - accDistanceArray(subtrackCount)
                    'stopDistance = objCalc.CalcDistanceFromSpeeds(trainSpeed, 0, brakingrate)
                    Console.WriteLine("From here 4 " & stoppingPoint)

                    If accInitSpeedArray(subtrackCount) <> 0 Then
                        'don't need to stop yet
                        AccelCoastDecel(linespeed, trackcount, accelEndSpeed, currentDistance)

                         'DrawActualSpeedLine(currentDistance, currentDistance + currentTrackDistance - stopDistance, trainSpeed, accelEndSpeed)
                        'currentDistance = currentDistance + currentTrackDistance
                        'Console.WriteLine("coasting " & currentTrackDistance & " to " & currentDistance)

                    Else
                        Console.WriteLine("from here 8")
                        DecelTrack(stopDistance)
                        stopAtDistance = currentDistance
                        signalDistance = subRouteDistance - currentDistance
                        startingDistance = currentTrackDistance - stopDistance

                    End If
                    '
                    ''DrawActualSpeedLine(currentDistance, currentDistance + stopDistance, trainSpeed, 0)

          
                Else
                    'check if route is set?
                    trackEndSpeed = accInitSpeedArray(subtrackCount + 1)
                    currentTrackDistance = accDistanceArray(subtrackCount + 1) - accDistanceArray(subtrackCount)
                    If currentTrackDistance < 0 Then
                        'this is an error
                        Console.WriteLine("Current distance " & currentTrackDistance & " trackcount " & trackcount)
                    End If

                    If currentTrackDistance = 0 Then
                        'don't bother doing the rest of this as the distance is zero
                        Console.WriteLine("Not removing " & currentTrackDistance & " trackcount " & trackcount)

                    Else

                        linespeed = lineSpeedArray(trackcount)
                        accelDataobj = objCalc.CalcAccelUntilDistance(trainSpeed, trackEndSpeed, acceleration, brakingrate, currentTrackDistance, linespeed)
                        Console.WriteLine("train distance " & absoluteDistance & " " & trainDistance & " accel distance " & accelDataobj.accelDistance & " coast " & accelDataobj.coastDistance & " decel " & accelDataobj.decelDistance & " trainSpeed " & trainSpeed)
                        accelEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, accelDataobj.accelDistance)
                        accelEndSpeed = RoundTrackSpeed(accelEndSpeed)
                        'Console.WriteLine("currentDistance " & currentDistance & " accel distance " & accelDataobj.accelDistance)
                        DrawActualSpeedLine(currentDistance, currentDistance + accelDataobj.accelDistance, trainSpeed, accelEndSpeed)
                        'Console.WriteLine("normal acceleration " & accelDataobj.accelDistance & "  " & trainDistance)

                        acceltrack(accelDataobj.accelDistance)

                        trainSpeed = accelEndSpeed
                        currentDistance = currentDistance + accelDataobj.accelDistance
                        'Console.WriteLine("currentDistance " & currentDistance & " coast distance " & accelDataobj.coastDistance)

                        DrawActualSpeedLine(currentDistance, currentDistance + accelDataobj.coastDistance, trainSpeed, accelEndSpeed)
                        Coasttrack(accelDataobj.coastDistance)

                        currentDistance = currentDistance + accelDataobj.coastDistance
                        accelEndSpeed = objCalc.CalcEndSpeed(trainSpeed, brakingrate, accelDataobj.decelDistance)
                        accelEndSpeed = RoundTrackSpeed(accelEndSpeed)
                        'Console.WriteLine("currentDistance " & currentDistance & " decel distance " & accelDataobj.decelDistance)

                        DrawActualSpeedLine(currentDistance, currentDistance + accelDataobj.decelDistance, trainSpeed, accelEndSpeed)
                        Console.WriteLine("From here 5 distance is " & accelDataobj.decelDistance)
                        
                        DecelTrack(accelDataobj.decelDistance)



                        currentDistance = currentDistance + accelDataobj.decelDistance
                        trainSpeed = accelEndSpeed
                        If trainSpeed <> trackEndSpeed Then
                            ' Console.WriteLine("Track end speed is incorrect " & trainSpeed & " is not " & trackEndSpeed)
                        End If
                        If (currentDistance - trackEnd) = distanceArray(trackcount) Then
                            ' Console.WriteLine("at end of track " & trackcount & " " & trainDistance & " " & currentDistance)
                            trackcount = trackcount + 1
                            trackEnd = currentDistance
                            If trackcount = subRouteTracks Then
                                Exit For
                            End If
                        End If
                    End If

                End If
                If trainTerminated Then
                    Exit For
                End If

            Next


        End If



    End Sub



    Private Sub SlowForSignalStop()

        Dim lastTrackDistance As Integer
        Dim signalTCDistance As Integer
        Dim lastTrackLineSpeed As Integer
        Dim signalTCLineSpeed As Integer

        If stoppingPoint > (routeDistance - distanceArray(distanceArray.Count - 1) - constDistBeforeSignalTC) Then

            'The assume we're stopping at the end of the route
            'if we're stopping at a platform at a red light
            'then we've already modified the speeds in order
            'to stop at the signal, so the modifications to stop at 
            'the platform have to be a bit different
            If lineSpeedArray(lineSpeedArray.Count - 1) > 0 Then

            End If
            Console.WriteLine("modify for stopping point")
            isStopping = True
            Console.WriteLine("distanceArray(count-3) was " & distanceArray(distanceArray.Count - 3))
            Console.WriteLine("distanceArray(count-2) was " & distanceArray(distanceArray.Count - 2))
            Console.WriteLine("distanceArray(count-1) was " & distanceArray(distanceArray.Count - 1))

            Console.WriteLine("LineSpeedArray(count-3) was " & lineSpeedArray(distanceArray.Count - 3))
            Console.WriteLine("LineSpeedArray(count-2) was " & lineSpeedArray(distanceArray.Count - 2))
            Console.WriteLine("LineSpeedArray(count-1) was " & lineSpeedArray(distanceArray.Count - 1))

            lastTrackDistance = distanceArray(distanceArray.Count - 2)
            signalTCDistance = distanceArray(distanceArray.Count - 1)
            lastTrackLineSpeed = lineSpeedArray(lineSpeedArray.Count - 2)
            signalTCLineSpeed = lineSpeedArray(lineSpeedArray.Count - 1)
            If signalTCDistance > constStandardSignalTCDistance Then
                'The signal track is pretty big, so it's probably
                'not a short track in front of a signal that we would
                'expect in a model railway, so modify things accordingly
                ' distanceArray(distanceArray.Count - 2) = lastTrackDistance - constDistBeforeSignalTC
                distanceArray(distanceArray.Count - 1) = signalTCDistance - constDistBeforeSignalTC - constStandardSignalTCDistance
                distanceArray.Add(constDistBeforeSignalTC)
                distanceArray.Add(constStandardSignalTCDistance)

                'lineSpeedArray(lineSpeedArray.Count - 2) = constPreStoppingSpeed
                lineSpeedArray.Add(constPreStoppingSpeed)
                lineSpeedArray.Add(0)
                stoppingPoint -= constStandardSignalTCDistance

            Else
                'signal track length is what we'd expect for a short track
                'before a signal
                If lastTrackDistance <= constDistBeforeSignalTC Then
                    'slowing point extends into previous track i.e. count -3
                    Console.WriteLine("last track distance is less or equal to the pre signal distance")

                    distanceArray(distanceArray.Count - 3) = distanceArray(distanceArray.Count - 3) + lastTrackDistance - constDistBeforeSignalTC
                    distanceArray(distanceArray.Count - 2) = constDistBeforeSignalTC - lastTrackDistance
                    distanceArray(distanceArray.Count - 1) = lastTrackDistance
                    lineSpeedArray(lineSpeedArray.Count - 2) = constPreStoppingSpeed
                    lineSpeedArray(lineSpeedArray.Count - 1) = constPreStoppingSpeed
                Else
                    Console.WriteLine("last track distance isn't less than pre signal distance")
                    distanceArray(distanceArray.Count - 2) = lastTrackDistance - constDistBeforeSignalTC
                    distanceArray(distanceArray.Count - 1) = constDistBeforeSignalTC
                    'lineSpeedArray(lineSpeedArray.Count - 2) = constPreStoppingSpeed
                    lineSpeedArray(lineSpeedArray.Count - 1) = constPreStoppingSpeed

                End If ' lastTrackDistance <= constDistBeforeSignalTC
                distanceArray.Add(signalTCDistance)

                If signalTCDistance < 1 Then
                    Console.WriteLine("This can't happen so must be an error")
                End If

                lineSpeedArray.Add(0)
                Console.WriteLine("distanceArray(count-3)  is  " & distanceArray(distanceArray.Count - 4))
                Console.WriteLine("distanceArray(count-2)  is  " & distanceArray(distanceArray.Count - 3))
                Console.WriteLine("distanceArray(count-1)  is  " & distanceArray(distanceArray.Count - 2))
                Console.WriteLine("distanceArray(count) (NEW)  is  " & distanceArray(distanceArray.Count - 1))

                Console.WriteLine("LineSpeedArray(count-3) is  " & lineSpeedArray(distanceArray.Count - 4))
                Console.WriteLine("LineSpeedArray(count-2) is  " & lineSpeedArray(distanceArray.Count - 3))
                Console.WriteLine("LineSpeedArray(count-1) is  " & lineSpeedArray(distanceArray.Count - 2))
                Console.WriteLine("LineSpeedArray(count) NEW is  " & lineSpeedArray(distanceArray.Count - 1))

            End If
        Else

        End If
    End Sub


    Private Sub ModifySpeedsForStopping()

        'Dim lastTrackDistance As Integer
        'Dim signalTCDistance As Integer
        'Dim lastTrackLineSpeed As Integer
        'Dim signalTCLineSpeed As Integer
        Dim currentDistance As Integer
        Dim stopPoint As Integer
        'Dim currentTrackDistance As Integer
        If routeToFringe = False Then
            'Only stop if we're not going to a fringe

            stopPoint = stoppingPoint

            If stopPoint > (routeDistance - distanceArray(distanceArray.Count - 1) - constDistBeforeSignalTC) Then
                'This is a signal stop
                SlowForSignalStop()

            ElseIf stopPoint > 0 Then
                Console.WriteLine("stop point " & stopPoint)
                currentDistance = 0
                For trackcount As Integer = 0 To (distanceArray.Count - 1)
                    currentDistance = currentDistance + distanceArray(trackcount)

                    If currentDistance >= stopPoint Then
                        'We need to stop in this array
                        If trackcount = 0 Then
                            'and this is the first track   
                            distanceArray.RemoveRange(1, distanceArray.Count - 1)
                            lineSpeedArray.RemoveRange(1, lineSpeedArray.Count - 1)
                            distanceArray.Add(constDistBeforeSignalTC)
                            distanceArray.Add(distanceArray(0) - stopPoint)
                            distanceArray(0) = stopPoint - constDistBeforeSignalTC
                            lineSpeedArray.Add(constPreStoppingSpeed)
                            lineSpeedArray.Add(0)


                        ElseIf (stopPoint - constDistBeforeSignalTC) < (currentDistance - distanceArray(trackcount)) Then
                            'then we need to do something in the track before

                            distanceArray(trackcount + 1) = stopPoint + distanceArray(trackcount) - currentDistance
                            If (trackcount + 2) <= (distanceArray.Count - 1) Then

                                lineSpeedArray(trackcount + 2) = 0
                                distanceArray(trackcount + 2) = distanceArray(trackcount) - distanceArray(trackcount + 1)

                            Else
                                lineSpeedArray.Add(0)
                                distanceArray.Add(distanceArray(trackcount) - distanceArray(trackcount + 1))
                            End If


                            lineSpeedArray(trackcount + 1) = constPreStoppingSpeed
                            ' distanceArray(trackcount) = distanceArray(trackcount - 1) - stopPoint + constDistBeforeSignalTC
                            lineSpeedArray(trackcount) = constPreStoppingSpeed
                            If trackcount < 2 Then
                                distanceArray(trackcount - 1) = stopPoint - constDistBeforeSignalTC
                                distanceArray(trackcount) = constDistBeforeSignalTC - distanceArray(trackcount + 1)

                            Else
                                If distanceArray(trackcount - 1) < (constDistBeforeSignalTC - distanceArray(trackcount + 1)) Then
                                    distanceArray(trackcount) = distanceArray(trackcount - 1)
                                    distanceArray(trackcount - 1) = constDistBeforeSignalTC - distanceArray(trackcount + 1) - distanceArray(trackcount - 1)
                                    distanceArray(trackcount - 2) = distanceArray(trackcount - 2) - distanceArray(trackcount - 1)
                                Else
                                    distanceArray(trackcount) = constDistBeforeSignalTC - distanceArray(trackcount + 1)
                                    distanceArray(trackcount - 1) = distanceArray(trackcount - 1) - distanceArray(trackcount)

                                End If



                            End If
                            ' lineSpeedArray(trackcount - 1) = constPreStoppingSpeed
                            distanceArray.RemoveRange(trackcount + 3, distanceArray.Count - 3 - trackcount)
                            lineSpeedArray.RemoveRange(trackcount + 3, lineSpeedArray.Count - 3 - trackcount)

                        Else
                            'Everything is contained in this track
                            If (trackcount + 2) < (distanceArray.Count - 1) Then
                                ' set the stop point to be zero speed at the stopping point 
                                lineSpeedArray(trackcount + 2) = 0
                                distanceArray(trackcount + 2) = (distanceArray(trackcount) - (stopPoint + distanceArray(trackcount) - currentDistance))

                            Else
                                lineSpeedArray.Add(0)
                                distanceArray.Add(distanceArray(trackcount) - (stopPoint + distanceArray(trackcount) - currentDistance))

                            End If
                            distanceArray(trackcount + 1) = constDistBeforeSignalTC
                            distanceArray(trackcount) = stopPoint + distanceArray(trackcount) - currentDistance - constDistBeforeSignalTC

                            ' lineSpeedArray(trackcount + 2) = 0
                            lineSpeedArray(trackcount + 1) = constPreStoppingSpeed
                            distanceArray.RemoveRange(trackcount + 3, distanceArray.Count - 3 - trackcount)
                            lineSpeedArray.RemoveRange(trackcount + 3, lineSpeedArray.Count - 3 - trackcount)

                        End If
                        Exit For

                    End If
                Next
            End If
        End If 'routeToFringe = False

    End Sub

    Private Sub CheckIfStop()
        Dim distanceInt As Integer
        Dim stopInThisRoute As Boolean = False
        distanceInt = 0
        Dim arrayIndex As Integer
        For arrayIndex = 0 To (trackArray.Count - 1)
            If m_parent.TrackArrayList(trackArray(arrayIndex)).location = nextTimingLoc Then
                Console.WriteLine("From " & m_parent.TrackArrayList(trackArray(arrayIndex)).nodefrom & " to " & m_parent.TrackArrayList(trackArray(arrayIndex)).nodeto & " timing point " & m_parent.TrackArrayList(trackArray(arrayIndex)).location & " plat " & m_parent.TrackArrayList(trackArray(arrayIndex)).platform & " length  " & m_parent.TrackArrayList(trackArray(arrayIndex)).tracklength)
                'this is a timing point
                stopInThisRoute = True
                Exit For
            End If

            ' If trackArray(index) = stoptrackId Then
            'm_parent.TrackArrayList(trackArray(index)).location()
            ' Exit For
            'Else
            distanceInt += GetTrackLengthNoLinespeed(trackArray(arrayIndex))
            'End If
        Next
        If stopInThisRoute Then
            nextPlatformlength = GetTrackLengthNoLinespeed(trackArray(arrayIndex))
            stoppingPoint = distanceInt + nextPlatformlength

        Else
            stoppingPoint = 0
        End If
    End Sub


    Private Sub TraverseSubroute()
        'Dim traverseTime As Single
        Dim firstSubroute As Boolean = True
        'Dim inTrack As Boolean
        For Each route As String In routelist
            GetSubRoute(route)
            If firstSubroute Then
                subRouteTracks = trackArray.Count
                firstSubroute = False
            End If

        Next
        Dim trackDistance As Integer
        Dim trackStartDistance As Integer = 0
        'stationStop = False
        If stationStop = False Then
            CheckIfStop()

        End If
        CalcSpeed()
        subRouteDistance = 0
        absoluteDistance = 0
        'Console.WriteLine("Absolute = 0")
        For trackCount As Integer = 0 To subRouteTracks - 1
            subRouteDistance = subRouteDistance + distanceArray(trackCount)
        Next
        If brakingDistance < subRouteDistance Then
            'We need to start braking in this subroute
            'Console.WriteLine("start braking at " & brakingDistance)
        End If

        routeDistance = subRouteDistance
        CalcTargetSpeeds()
        totalDistance = GetTotalDistance()
        ' Dim currentTrackDistance As Integer
        ' Dim subTrackDistance As Integer
        Dim accIndex As Integer
        'Dim endOfTrackDistance As Integer
        accIndex = 0
        'absoluteDistance = 0
        ' Console.WriteLine("calculating on linespeed form")

        UpdateLinespeedFrm()
        'Console.WriteLine("calculating on this form")
        GetAcceleration()



        If accInitSpeedArray(0) < trainSpeed Then
            Console.WriteLine("Initial speed is too high " & trainSpeed)
        End If


        Dim trailingDistance As Integer
        trailingDistance = GetTraversedDistance()
        If traversedRoute.Count > 2 Then
            '  scheduleTrackClear(linespeed, acceleration, traversedRoute(0))
            ' traversedRoute.RemoveAt(0)
        End If

        'Console.WriteLine("Set tc " & trackArray(trackCount))
        absoluteDistance = absoluteDistance + trackDistance
        If trainSpeed > linespeed Then
            Console.WriteLine("Track speed error")
        End If


        If stationStop Then
            ' stationStop = False
        Else
            If routelist.Count > 0 Then
                routelist.RemoveAt(0)
            End If
        End If

    End Sub

    Private Sub CalcrouteAccelerationDeceleration(ByVal subRouteInitSpeedmph As Integer, _
                                                  ByVal subRouteEndSpeedmph As Integer, _
                                                  ByVal subRouteDistance As Integer)

        ' Dim subBrakingDistance As Integer
        Dim subAccelerationDistance As Integer
        Dim subTrackEndSpeed As Single
        Dim currentDistance As Integer
        Dim predictedRouteEndSpeed As Single
        predictedRouteEndSpeed = objCalc.CalcEndSpeed(subRouteInitSpeedmph, accelRate, subRouteDistance)

        'Work out the point that we need to start decelerating, ignoring line speeds thus far
        subAccelerationDistance = objCalc.CalcAccelUntilDistance(subRouteInitSpeedmph, subRouteEndSpeedmph, accelRate, brakingrate, subRouteDistance)

        ' Start Checking line speeds
        currentDistance = 0
        For distanceArrayCount As Integer = 0 To distanceArray.Count - 1
            currentDistance = currentDistance + distanceArray(distanceArrayCount)
            If currentDistance < subAccelerationDistance Then
                'we're not at the acceleration end point
                subTrackEndSpeed = objCalc.CalcEndSpeed(subRouteInitSpeedmph, accelRate, currentDistance)
                'check the predicted speed at the end of the track against the linespeed
                If subTrackEndSpeed > lineSpeedArray(distanceArrayCount + 1) Then
                    'oh no! we're going to end up going too fast!
                    'So for this acceleration work out when we're going to hit the linespeed
                    objCalc.CalcDistanceFromSpeeds(subRouteInitSpeedmph, lineSpeedArray(distanceArrayCount + 1), accelRate)
                    'also now we neet to recalculate the point at which we start decelerating
                    objCalc.CalcDistanceFromSpeeds(lineSpeedArray(distanceArrayCount + 1), subRouteInitSpeedmph, brakingrate)

                End If
            End If


        Next

    End Sub


    Private Sub addToArray(ByVal distance As Integer, ByVal accelDecel As Integer, _
                               ByVal speedmph As Integer)
        Dim sizeOfArrays As Integer
        sizeOfArrays = accDistanceArray.Count
        Dim copyArray(sizeOfArrays) As Integer

        ReDim Preserve accDistanceArray(sizeOfArrays)
        ReDim Preserve accTypeArray(sizeOfArrays)
        ReDim Preserve accInitSpeedArray(sizeOfArrays)
        accDistanceArray(sizeOfArrays) = distance
        accTypeArray(sizeOfArrays) = accelDecel
        accInitSpeedArray(sizeOfArrays) = speedmph

        accDistanceArray.CopyTo(copyArray, 0)
        Array.Sort(accDistanceArray, accTypeArray)
        Array.Sort(copyArray, accInitSpeedArray)


    End Sub

    Private Function CheckTrackOccupied(ByVal trackArray As ArrayList) As Boolean
        Dim isSet As Boolean = False
        For trackId As Integer = 0 To trackArray.Count - 1
            If m_parent.TrackArrayList(trackArray(trackId)).trackisoccupied Then
                isSet = True
                Exit For
            End If
        Next
        Return isSet
    End Function

    Private Function CheckRouteHasExitNode(ByVal trackArray As ArrayList, ByVal PrevExitNode As String) As Boolean
        Dim isSet As Boolean = False
        For trackId As Integer = 0 To trackArray.Count - 1
            If m_parent.TrackArrayList(trackArray(trackId)).nodeFrom = PrevExitNode Then
                isSet = True
                Exit For
            End If
        Next
        If m_parent.TrackArrayList(trackArray(trackArray.Count - 1)).nodeTo = PrevExitNode Then
            isSet = True
        End If
        Return isSet
    End Function

    Public Function CalcInitSpeed(ByVal endSpeed As Single, _
                                              ByVal lineIndex As Integer, _
                                              ByVal acceleration As Single) As Integer
        Dim currentLinespeed As Single = lineSpeedArray(lineIndex)
        Dim currentTrackDistance As Single = distanceArray(lineIndex)
        Dim endSpeedmps As Single
        Dim lineSpeedmps As Single
        Dim initMaxSpeedmps As Single
        Dim decelerationDistance As Single
        Dim previousLineSpeedmph As Single
        Dim previousLineSpeedmps As Single

        endSpeedmps = objCalc.mph2Mps(endSpeed)
        lineSpeedmps = objCalc.mph2Mps(currentLinespeed)

        ' If endSpeed = 0 Then
        'we are stopping, so what we want to do is slow just before the last track
        'so that when the train hits it, it can slow to zero without slamming on the brakes
        'Console.WriteLine("End speed is zero")
        'End If

        ' Else

        If lineIndex > 0 Then
            previousLineSpeedmph = lineSpeedArray(lineIndex - 1)
        Else
            previousLineSpeedmph = currentLinespeed
        End If

        previousLineSpeedmps = objCalc.mph2Mps(previousLineSpeedmph)

        If currentLinespeed < endSpeed Then
            If endSpeed = 0 Then
                Console.WriteLine("speed is zero")

            End If
            'we don't need to do anything as the speed is slower
            ' previously and we'll be accelerating
            '
            '          ---------
            '          |
            '          |\
            '----------| \
            '
            addToArray(routeDistance - currentTrackDistance, 0, currentLinespeed)
            initMaxSpeedmps = objCalc.mph2Mps(currentLinespeed)

        ElseIf currentLinespeed = endSpeed Then
            '
            '-------------------
            '          \
            '          |\
            '          | \
            '
            addToArray(routeDistance - currentTrackDistance, 0, currentLinespeed)
            initMaxSpeedmps = objCalc.mph2Mps(endSpeed)
        Else

            'currentLineSpeed > endSpeed
            If endSpeed = 0 Then
                Console.WriteLine("speed is zero")
                addToArray(routeDistance - currentTrackDistance, 0, currentLinespeed)

                initMaxSpeedmps = lineSpeedmps
            Else
                'we're slowing
                Dim sqrtVal As Single
                'work out the inital speed given the end speed , distance and acceleration
                ' v2 =u2 -2as
                sqrtVal = ((endSpeedmps * endSpeedmps) - (2 * acceleration * currentTrackDistance))
                initMaxSpeedmps = Sqrt(sqrtVal)
                If initMaxSpeedmps > lineSpeedmps Then
                    'Based on this, the initial speed is greater than the line
                    'speed, but as we can't go faster than the line
                    'So the initial speed has to be the line speed.
                    'We must coast for a bit at line speed,
                    'then decelerate to the target end speed
                    '
                    ' -------     line speed
                    '        \
                    '         \
                    '          \

                    'now work out where to start decelerating
                    decelerationDistance = objCalc.CalcDistanceFromSpeeds(currentLinespeed, endSpeed, acceleration)
                    'Add deceleration point
                    addToArray(routeDistance - decelerationDistance, -1, currentLinespeed)

                    If initMaxSpeedmps > previousLineSpeedmps Then
                        'line before has a lower linespeed than this, so we can't
                        'set the higher value  - the predicted max value as the
                        'target speed, so set this to the line speed of the line before
                        addToArray(routeDistance - currentTrackDistance, 1, objCalc.Mps2mph(previousLineSpeedmps))
                        initMaxSpeedmps = previousLineSpeedmps
                    Else
                        'line before had a higher line speed, and we don't need to start
                        'braking earlier
                        addToArray(routeDistance - currentTrackDistance, 0, currentLinespeed)
                        initMaxSpeedmps = lineSpeedmps

                    End If
                Else

                    'This is ok as we're not going to go over the line speed
                    If initMaxSpeedmps > previousLineSpeedmps Then
                        'line before has a lower linespeed than this, so we can't
                        'set the higher value  - the predicted max value as the
                        'target speed, so set this to the line speed of the line before
                        '
                        '
                        '       |
                        '       |-------     line speed
                        '       |      
                        '       |\
                        '       | \
                        ' ------|  \
                        '
                        ' So we set the target speed to be the max speed allowable
                        ' for braking as calculated

                        addToArray(routeDistance - currentTrackDistance, -1, objCalc.Mps2mph(previousLineSpeedmps))
                        initMaxSpeedmps = previousLineSpeedmps

                    Else
                        'line before has a higher linespeed than this, so we set
                        'target speed, to be the max speed at the track join
                        '
                        ' -------
                        '     \ |
                        '      \|-------     line speed
                        '       |      
                        '       |\
                        '       | \
                        '       |  \
                        '
                        ' So we set the target speed to be the max speed allowable
                        ' for braking as calculated
                        initMaxSpeedmps = initMaxSpeedmps
                        addToArray(routeDistance - currentTrackDistance, -1, objCalc.Mps2mph(initMaxSpeedmps))
                    End If
                End If
            End If
        End If
        ' End If


        routeDistance = routeDistance - currentTrackDistance
        Return objCalc.Mps2mph(initMaxSpeedmps)
    End Function

    Private Sub SignalSlowSubroute()
        If braking Then
            brakingToSpeed = constPreStoppingSpeed
            For trackCount As Integer = 0 To subRouteTracks - 1
                SetTrackTc(trackArray(trackCount))
                traversedRoute.Add(trackArray(trackCount))
                traversedDistance.Add(distanceArray(trackCount))
                Dim trailingDistance As Integer
                trailingDistance = GetTraversedDistance()
                If traversedRoute.Count > 2 Then
                    scheduleTrackClear(linespeed, acceleration, traversedRoute(0))
                    traversedRoute.RemoveAt(0)
                End If

                'Console.WriteLine("Set tc " & trackArray(trackCount))
                'trackDistance = distanceArray(trackCount)
                linespeed = lineSpeedArray(trackCount)
                'absoluteDistance = absoluteDistance + trackDistance
                If trainSpeed > linespeed Then
                    Console.WriteLine("Track speed error for track " & trackArray(trackCount))

                End If
                ' DecrementSpeed(trackCount)
            Next
        End If

    End Sub

    Private Sub SignalAccelerateSubroute()
        Dim accelerationDistance As Integer
        Dim trackEndSpeed As Integer
        brakingToSpeed = constPreStoppingSpeed
        accelerationDistance = objCalc.CalcDistanceFromSpeeds(trainSpeed, maxAccelToSpeed, acceleration)
        accelerating = True
        For trackCount As Integer = 0 To subRouteTracks - 1
            SetTrackTc(trackArray(trackCount))
            traversedRoute.Add(trackArray(trackCount))
            traversedDistance.Add(distanceArray(trackCount))
            Dim trailingDistance As Integer
            trailingDistance = GetTraversedDistance()
            If traversedRoute.Count > 2 Then
                scheduleTrackClear(linespeed, acceleration, traversedRoute(0))
                traversedRoute.RemoveAt(0)
            End If

            Console.WriteLine("Set tc " & trackArray(trackCount))
            'trackDistance = distanceArray(trackCount)
            linespeed = lineSpeedArray(trackCount)
            'absoluteDistance = absoluteDistance + trackDistance
            If trainSpeed > linespeed Then
                Console.WriteLine("Track speed error")

            End If
            'work out if we're going to hit the max speed in this track
            If accelerationDistance > (distanceArray(trackCount)) Then
                'it's not in this track
                trackEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, distanceArray(trackCount))
                accelerationDistance = accelerationDistance - distanceArray(trackCount)
                If trackEndSpeed > maxAccelToSpeed Then
                    'then we're going to end up faster 

                End If

            Else
                trackEndSpeed = maxAccelToSpeed
            End If
            If accelerating Then
                '' IncrementSpeed(trackCount, trackEndSpeed)
            Else
                Console.Write("coasting")
                'IncrementSpeed(trackCount, trackEndSpeed)

            End If

        Next

    End Sub



    Private Sub Coasttrack(ByVal coastingDistance As Integer)
        If trainSpeed = 0 Then
            'if trainspeed is zero, then we'll coast forever, unless distance is zero
            If coastingDistance > 0 Then
                'This is an error
            End If
            'Otherwise do nothing, and just finish the function


        Else
            Dim traverseTime As Single
            Dim coastDistance1 As Integer
            While CheckTrainEnd(trainDistance, coastingDistance)
                coastDistance1 = traversedDistance(1) - trainDistance
                traverseTime = objCalc.CalcTraverseTime(trainSpeed, 0, coastDistance1)
                coastingDistance = coastingDistance - coastDistance1
                absoluteDistance = absoluteDistance + coastDistance1
                trainDistance = trainDistance + coastDistance1
                trainAction = " "
                UpdateCab()

                ScaledSleep(traverseTime * 1000)
                Console.WriteLine("end of train in this track coast " & trainDistance & " tc " & traversedRoute(0) & " " & traversedDistance(1) & " speed " & trainSpeed)
                ClearTailEndTrack()
            End While

            traverseTime = objCalc.CalcTraverseTime(trainSpeed, 0, coastingDistance)
            absoluteDistance = absoluteDistance + coastingDistance
            trainDistance = trainDistance + coastingDistance

            'Console.WriteLine("Coasting : absolute " & absoluteDistance & " trACK " & currentTrack & " trainSpeed " & trainSpeed & " distance " & trainDistance _
            '& " Coasting distance " & coastingDistance & " traverse time " & traverseTime * 1000 & " acceleration " & acceleration)
            trainAction = " "
            UpdateCab()
            ScaledSleep(traverseTime * 1000)
        End If

    End Sub


    Private Function CheckTrainEnd(ByVal currentDistance As Integer, ByVal DistanceStep As Integer) As Boolean
        Dim inRange As Boolean = False
        If traversedDistance.Count > 1 Then
            Console.WriteLine("Check of distance " & trainDistance & " current " & currentDistance & "  " & traversedDistance(1) & " " & DistanceStep & " track " & trainSpeed)
            If traversedDistance(1) <= (currentDistance + DistanceStep) Then
                'The end of the train is in this track
                inRange = True
            End If
        End If
        Return inRange
    End Function

    Private Sub acceltrack(ByVal accelerationDistance As Integer)
        Dim traversetime As Single
        Dim trackDistanceCovered As Single = 0
        'what's the speed going to be at the end of this track?
        Dim accelEndSpeed As Single
        Dim exitFor As Boolean = False

        If accelerationDistance > 0 Then
            'only do this if distance is non-zero
            accelEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, accelerationDistance)

            Dim speedStepDistance As Integer
            speedStepDistance = objCalc.CalcDistanceFromSpeeds(trainSpeed, accelEndSpeed, acceleration)

            For speedStep As Integer = trainSpeed To accelEndSpeed
                speedStepDistance = objCalc.CalcDistanceFromSpeeds(speedStep, speedStep + 1, acceleration)
                If (trackDistanceCovered + speedStepDistance) >= accelerationDistance Then
                    'Due to rounding problems the calculated track is longer than the actual track 
                    'so adjust to the actual distance
                    speedStepDistance = accelerationDistance - trackDistanceCovered
                    exitFor = True
                End If
                traversetime = objCalc.CalcTraverseTime(speedStep, acceleration, speedStepDistance)

                If CheckTrainEnd(trainDistance, speedStepDistance) Then
                    'Console.WriteLine("end of train in this track accel " & trainDistance & " tc " & traversedRoute(0) & " " & traversedDistance(1))
                    ClearTailEndTrack()
                End If
                trackDistanceCovered += speedStepDistance
                absoluteDistance = absoluteDistance + speedStepDistance
                trainDistance = trainDistance + speedStepDistance
                'Console.WriteLine("accel " & trainDistance & " " & accelerationDistance & " speed " & speedStep & " " & accelEndSpeed & " " & trackDistanceCovered & " " & speedStepDistance)

                trainSpeed = speedStep
                trainAction = "Accelerating" & traversetime
                UpdateCab()
                DrawCurrentPositon()
                '              Console.WriteLine("Accelerating " & currentTrack & " trainSpeed " & trainSpeed & " distance " & absoluteDistance _
                '                                       & " traverse time " & traversetime * 1000 & " Maxspeed " & accelEndSpeed & " acceleration " & acceleration _
                '                                      & " speed step " & speedStepDistance)
                ScaledSleep(traversetime * 1000)
                If exitFor Then
                    Exit For
                End If

            Next
        End If
    End Sub

    Private Sub ClearTailEndTrack()
        ClearTrackTc(traversedRoute(0))
        traversedDistance.RemoveAt(0)
        traversedRoute.RemoveAt(0)

    End Sub

    Private Sub CalcStoppingPosition()
        'ok. So we have now stopped. Work out the distance to the signal,
        'and the end of the route
        Dim frontOfTrain As String
        Dim endOfTrain As String
        Dim endofTrainToEndOfPlatform As String

        frontOfTrain = subRouteDistance - currentTrackDistance
        endOfTrain = frontOfTrain - trainLength
        endofTrainToEndOfPlatform = endOfTrain



    End Sub

    Private Sub DecelTrack(ByVal decelerationDistance As Integer)
        Dim traversetime As Single
        Dim trackDistanceCovered As Integer = 0
        'what's the speed going to be at the end of this track?
        Dim decelEndSpeed As Integer
        Dim exitFor As Boolean = False

        decelEndSpeed = objCalc.CalcEndSpeed(trainSpeed, brakingrate, decelerationDistance)

        Dim speedStepDistance As Integer
        For speedStep As Integer = trainSpeed To decelEndSpeed Step -1
            speedStepDistance = objCalc.CalcDistanceFromSpeeds(speedStep, speedStep - 1, brakingrate)
            If (trackDistanceCovered + speedStepDistance) >= decelerationDistance Then
                'Due to rounding problems the calculated track is longer than the actual track 
                'so adjust to the actual distance
                speedStepDistance = decelerationDistance - trackDistanceCovered
                exitFor = True
            End If
            If speedStep = decelEndSpeed Then
                'at required speed, check rounding of track
                If speedStep = 0 Then
                    'there may be a dividing problem if speed is zero
                    speedStepDistance = decelerationDistance - trackDistanceCovered
                    trackDistanceCovered += speedStepDistance
                    absoluteDistance = absoluteDistance + speedStepDistance
                    trainDistance = trainDistance + speedStepDistance
                    speedStepDistance = 0
                Else
                    speedStepDistance = decelerationDistance - trackDistanceCovered

                End If
            End If

            If CheckTrainEnd(trainDistance, speedStepDistance) Then
                Console.WriteLine("end of train in this track decel" & trainDistance & " tc " & traversedRoute(0) & " " & traversedDistance(1))
                ClearTailEndTrack()
            End If

            ' If 

            'Then this track is a fringe
            'Else


            traversetime = objCalc.CalcTraverseTime(speedStep, brakingrate, speedStepDistance)
            trackDistanceCovered += speedStepDistance
            absoluteDistance = absoluteDistance + speedStepDistance
            trainDistance = trainDistance + speedStepDistance
            ' Console.WriteLine("decel " & trainDistance)
            trainSpeed = speedStep
            If decelerationDistance <> 0 Then
                trainAction = "Decelerating"
            Else
                trainAction = " "
            End If
            UpdateCab()
            DrawCurrentPositon()
            ' Console.WriteLine("Decelerating :" & currentTrack & " trainSpeed " & trainSpeed & " distance " & absoluteDistance _
            '                         & " traverse time " & traversetime * 1000 & " Maxspeed " & decelEndSpeed & " deceleration " & acceleration _
            '                     & " speed step " & speedStepDistance & " stopping point " & stoppingPoint)
            ScaledSleep(traversetime * 1000)
            If absoluteDistance >= stoppingPoint And stoppingPoint > 0 And trainSpeed = 0 Then
                Console.WriteLine("Stop for a bit at " & trainDistance & " absolute " & absoluteDistance & " trainspeed " & trainSpeed)
                stationStop = True
                stopAtDistance = stoppingPoint
                stoppingPoint = 0
                trainAction = "Station Stop"
                UpdateCab()
                If removeTimingAtStop Then
                    'Log arrival time
                    CheckTrainArriveTimingPoint()
                    Console.WriteLine("Removed the station stop here")
                    CalcStoppingPosition()
                    removeTimingAtStop = False
                    'stationStopAtSignal = True
                    isStopping = False
                Else
                    'this is a stop, but no timing info is there
                    Console.WriteLine("not removed at station stop")
                End If
                ScaledSleep(5500)

            End If
            'End If

            'Console.WriteLine("decel " & trainDistance & " " & decelerationDistance & " speed " & speedStep & " " & decelEndSpeed & " " & trackDistanceCovered & " " & speedStepDistance)
            If exitFor Then
                Exit For
            End If

        Next
    End Sub



    Private Sub TravelTrack(ByVal trackCount As Integer, ByVal trackEndSpeed As Integer, _
                            ByVal lineSpeed As Integer, ByVal currentTrackDistance As Integer)

        ' Dim traversetime As Single
        Dim trackDistanceCovered As Integer = 0
        Dim trackTraverseDistance As Integer
        currentTrack = trackArray(trackCount)
        stationDistance = distanceArray(trackCount)
        trackTraverseDistance = absoluteDistance
        'determine what we need to do in this track

        If trackEndSpeed = trainSpeed Then
            'we end up at the same speed as we are now
            If trainSpeed = lineSpeed Then
                'we're also at the line speed, so just coast
                Coasttrack(currentTrackDistance)
            Else
                'line speed MUST be higher than this, so we can accelerate for a bit, as
                'long as we decelerate to get back down to the speed at the end of the track
                accelDataobj = objCalc.CalcAccelUntilDistance(trainSpeed, trackEndSpeed, acceleration, brakingrate, currentTrackDistance, lineSpeed)
                acceltrack(accelDataobj.accelDistance)
                Coasttrack(accelDataobj.coastDistance)
                Console.WriteLine("From here 3")

                DecelTrack(accelDataobj.decelDistance)

            End If
        End If


        If trackEndSpeed < trainSpeed Then
            'this is a deceleration 
            acceleration = accelRate
            accelDataobj = objCalc.CalcAccelUntilDistance(trainSpeed, trackEndSpeed, acceleration, brakingrate, currentTrackDistance, lineSpeed)
            acceltrack(accelDataobj.accelDistance)
            Coasttrack(accelDataobj.coastDistance)
            Console.WriteLine("From here 1")

            DecelTrack(accelDataobj.decelDistance)
        End If

        If trackEndSpeed > trainSpeed Then
            'this is a acceleration 
            acceleration = accelRate
            accelDataobj = objCalc.CalcAccelUntilDistance(trainSpeed, trackEndSpeed, acceleration, brakingrate, currentTrackDistance, lineSpeed)
            acceltrack(accelDataobj.accelDistance)
            Coasttrack(accelDataobj.coastDistance)
            Console.WriteLine("From here 2")
            DecelTrack(accelDataobj.decelDistance)
        End If

    End Sub



    Private Sub DecrementSpeedPartTrack(ByVal remainingTrackDistance As Integer)
        Dim traversetime As Single
        Dim trackDistanceCovered As Integer = 0
        'what's the speed going to be at the end of this track?
        Dim trackEndSpeed As Integer
        trackEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, remainingTrackDistance)
        'Console.WriteLine("track end speed should be " & trackEndSpeed)
        ' currentTrack = trackArray(trackCount)
        'stationDistance = distanceArray(trackCount)

        Dim speedStepDistance As Integer
        For speedStep As Integer = (trainSpeed - 1) To trackEndSpeed Step -1
            If trainSpeed < brakingToSpeed Then
                braking = False
                acceleration = 0
                'Stop braking now as we're at the right speed
                speedStepDistance = remainingTrackDistance - trackDistanceCovered
                traversetime = objCalc.CalcTraverseTime(trainSpeed, acceleration, speedStepDistance)
                absoluteDistance = absoluteDistance + speedStepDistance
                UpdateCab()
                Console.WriteLine("stop partial braking step track " & currentTrack & " trainSpeed " & trainSpeed & " distance " & absoluteDistance _
                                  & " traverse time " & traversetime * 1000 & " linespeed " & remainingTrackDistance & " acceleration " & acceleration _
                                  & " speedstep " & speedStepDistance)
                ScaledSleep(traversetime * 1000)
                Exit For
            Else

                'decrement by 1mph
                speedStepDistance = objCalc.CalcDistanceFromSpeeds(speedStep, speedStep - 1, acceleration)
                traversetime = objCalc.CalcTraverseTime(speedStep, acceleration, speedStepDistance)
                trainSpeed = speedStep
                absoluteDistance = absoluteDistance + speedStepDistance
                signalDistance = signalDistance + speedStepDistance
                trackDistanceCovered = trackDistanceCovered + speedStepDistance
                UpdateCab()
                Console.WriteLine("braking step 33 track " & currentTrack & " trainSpeed " & trainSpeed & " distance " & absoluteDistance _
                                  & " traverse time " & traversetime * 1000 & " linespeed " & remainingTrackDistance & " acceleration " & acceleration & trackDistanceCovered)
                ScaledSleep(traversetime * 1000)
            End If
        Next
    End Sub


    ' Private Sub IncrementSpeed(ByVal trackCount As Integer, ByVal trackMaxAccelToSpeed As Integer)
    ' Dim traversetime As Single
    ' Dim trackDistanceCovered As Integer = 0
    ' 'what's the speed going to be at the end of this track?
    ' Dim trackEndSpeed As Integer
    '     trackEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, distanceArray(trackCount))
    ' 'Console.WriteLine("track end speed should be " & trackEndSpeed)
    '     currentTrack = trackArray(trackCount)
    '     stationDistance = distanceArray(trackCount)
    '
    '    Dim speedStepDistance As Integer
    '        For speedStep As Integer = trainSpeed To trackEndSpeed
    '            If trainSpeed = trackMaxAccelToSpeed Then
    '                acceleration = 0
    '                accelerating = False
    '    'Stop accelerating now as we're at the right speed
    '    'check to see if we're at the point where we need to start braking
    '    'We've worked out before that we can accelerate for a bit, then we have 
    '    'to start braking in order to stop at the signal correctly, but we may 
    '    'not be at it exactly yet
    '                If trackDistanceCovered >= brakingDistance Then
    '    'we need to start braking
    '                    braking = True
    '
    '                Else
    '    'otherwise make up the distance 
    '                    speedStepDistance = brakingDistance - trackDistanceCovered
    '                    traversetime = objCalc.CalcTraverseTime(speedStep, acceleration, speedStepDistance)
    '                    absoluteDistance = absoluteDistance + speedStepDistance
    '                    trainAction = "accelerating"
    '                    UpdateCab()
    '                    Console.WriteLine("stop accelerating step track " & currentTrack & " trainSpeed " & trainSpeed & " distance " & absoluteDistance _
    '                                      & " traverse time " & traversetime * 1000 & " linespeed " & lineSpeedArray(trackCount) & " acceleration " & acceleration _
    '                                      & " speed step " & speedStepDistance)
    '                    ScaledSleep(traversetime * 1000)
    '                    Exit For
    '
    '                End If
    '                speedStepDistance = distanceArray(trackCount) - trackDistanceCovered
    '                traversetime = objCalc.CalcTraverseTime(speedStep, acceleration, distanceArray(trackCount) - trackDistanceCovered)
    '                absoluteDistance = absoluteDistance + speedStepDistance
    '                heldAtSignal = False
    '                trainAction = " "
    '                UpdateCab()
    '                Console.WriteLine("stop braking 11 step track " & currentTrack & " trainSpeed " & trainSpeed & " distance " & absoluteDistance _
    '                                  & " traverse time " & traversetime * 1000 & " linespeed " & lineSpeedArray(trackCount) & " acceleration " & acceleration)
    '                ScaledSleep(traversetime * 1000)
    '                Exit For
    '            Else
    '
    '    'decrement by 1mph
    '                speedStepDistance = objCalc.CalcDistanceFromSpeeds(speedStep, speedStep + 1, acceleration)
    '                traversetime = objCalc.CalcTraverseTime(speedStep, acceleration, speedStepDistance)
    '                trainSpeed = speedStep
    '                absoluteDistance = absoluteDistance + speedStepDistance
    '                signalDistance = signalDistance + speedStepDistance
    '                trackDistanceCovered = trackDistanceCovered + speedStepDistance
    '                trainAction = "decelerating"
    '                UpdateCab()
    '                Console.WriteLine("accelerate step track " & currentTrack & " trainSpeed " & trainSpeed & " distance " & absoluteDistance _
    '                                  & " traverse time " & traversetime * 1000 & " linespeed " & lineSpeedArray(trackCount) & " acceleration " & acceleration & trackDistanceCovered)
    '                ScaledSleep(traversetime * 1000)
    '            End If
    '        Next
    '    'we've accelerated to the right speed, but we're probably not at the end of a track
    '        If trackDistanceCovered < distanceArray(trackCount) Then
    '    'were not at the end of the track.
    '    'Check to see if we now need to decelarate
    '            If braking Then
    '    'Set up parameters and start to brake
    '                acceleration = brakingrate
    '    Dim trackDistanceLeft As Integer
    '                trackDistanceLeft = distanceArray(trackCount) - trackDistanceCovered
    '                DecrementSpeedPartTrack(trackDistanceLeft)
    '            End If
    '
    '        End If
    '    End Sub

    Private Sub GetEntryPoint()
        If timesArrayList(0).eventtype = "N" Then
            entrypoint = timesArrayList(0).eventlocation
            routeExitsignal = m_parent.lineArray(entrypoint).nodeIn
            timesArrayList.removeat(0)
        End If

    End Sub

    Private Sub GetTimingPoint()
        nextTimingLoc = timesArrayList(0).eventlocation
        nextTimingTime = timesArrayList(0).eventTime
        nextTimingType = timesArrayList(0).eventType
    End Sub

    Private Function CheckIfTrainExists() As Boolean
        Dim trainExists As Boolean = False
        'see if the train is already in the simulation after being
        'formed from another servoce
        If serviceExForms.Trim().Length > 0 Then
            ' Then the train was something before, so should be
            ' on the simulation already
            trainExists = True
        End If
        Return trainExists
    End Function



    Private Function GetRoutesDirection(ByVal routeID As String, ByRef exitSignal As String) As String

        Dim subRoutes As New ArrayList
        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Node dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectNodeTable As String = "SELECT direction,nodeTo FROM [routedB] " & _
            "WHERE (route = '" & routeID & "') "

        Dim SelectCommand As SqlCeCommand

        SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
        ' Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(sqlSelectNodeTable, nodeDbConn)

        Dim nodeReader As SqlCeDataReader

        ' Dim myConnection As SqlCeConnection
        Dim mycommand As SqlCeCommand

        mycommand = New SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
        nodeReader = mycommand.ExecuteReader

        Dim direction As String = ""
        While nodeReader.Read
            direction = nodeReader(0).ToString
            exitSignal = nodeReader(1).ToString
        End While

        nodeDbConn.Close()
        Return direction
    End Function
    Private Function GetRoutesFromNode(ByVal nodeFrom As String, ByVal nodeTo As String) As ArrayList

        Dim subRoutes As New ArrayList
        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Node dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectNodeTable As String = "SELECT * FROM [subroutedB] " & _
            "WHERE ((node = '" & nodeFrom & "') AND (nodeto = '" & nodeTo & "')) OR " & _
                  "((nodeto = '" & nodeFrom & "') AND (node = '" & nodeTo & "')) "

        Dim SelectCommand As SqlCeCommand

        SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
        ' Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(sqlSelectNodeTable, nodeDbConn)

        Dim nodeReader As SqlCeDataReader

        ' Dim myConnection As SqlCeConnection
        Dim mycommand As SqlCeCommand

        mycommand = New SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
        nodeReader = mycommand.ExecuteReader

        Dim RouteId As String
        While nodeReader.Read
            RouteId = nodeReader(0).ToString
            subRoutes.Add(RouteId)
        End While

        nodeDbConn.Close()
        Return subRoutes
    End Function

    Private Function GetRoutesFromEitherNode(ByVal nodeFrom As String, ByVal nodeTo As String) As ArrayList

        Dim subRoutes As New ArrayList
        Dim sConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim nodeDbConn As New SqlServerCe.SqlCeConnection(sConnection.ConnectionString)

        Try
            nodeDbConn.Open()
        Catch ex As Exception

            Console.WriteLine("can't open Node dBconnection")
        End Try

        Dim NodeTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectNodeTable As String = "SELECT * FROM [subroutedB] " & _
            "WHERE ((node = '" & nodeFrom & "') OR (nodeto = '" & nodeTo & "')) OR " & _
                  "((nodeto = '" & nodeFrom & "') OR (node = '" & nodeTo & "')) "

        Dim SelectCommand As SqlCeCommand

        SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
        ' Dim dBaseCommandSelectAll As New System.Data.OleDb.OleDbCommand(sqlSelectNodeTable, nodeDbConn)

        Dim nodeReader As SqlCeDataReader

        ' Dim myConnection As SqlCeConnection
        Dim mycommand As SqlCeCommand

        mycommand = New SqlCeCommand(sqlSelectNodeTable, nodeDbConn)
        nodeReader = mycommand.ExecuteReader

        Dim RouteId As String
        While nodeReader.Read
            RouteId = nodeReader(0).ToString
            subRoutes.Add(RouteId)
        End While

        nodeDbConn.Close()
        Return subRoutes
    End Function


    Private Sub GetPlatformNodeDetails(ByVal nodeFrom As String, ByVal nodeto As String, ByVal prevExitSignal As String)
        Dim routes As ArrayList
        Dim routeDirection As String
        Dim selectedRoute As String
        Dim exitSignal As String = "999"
        routes = GetRoutesFromNode(nodeFrom, nodeto)

        If routes.Count = 0 Then
            'nothing? well it could be that the platform is quite
            'complex, having things like signals and/or points halfway
            'up the platform. In this case the Q nodes at either end
            'of the platform won't have a direct route between the two
            'nodes as some other nodes are in the way.
            routes = GetRoutesFromEitherNode(nodeFrom, nodeto)

        End If

        For Each route In routes
            routeDirection = GetRoutesDirection(route, exitSignal)
            If routeDirection = direction Then
                'the direction is correct for where we are going
                ' if it's a platform there may be multiple entrances
                ' but there should be only one exit signal in each direction
                selectedRoute = route
                routeExitsignal = exitSignal
                trackArray.Clear()

                GetSubRoute(route)
                distanceArray.Clear()
                routeDistance = PopulateSubrouteArrays()
                totalDistance = GetTotalDistance()
                Dim isSet As Boolean
                Dim hasPrevExitNode As Boolean
                isSet = CheckTrackOccupied(trackArray)
                hasPrevExitNode = CheckRouteHasExitNode(trackArray, prevExitSignal)
                If isSet And hasPrevExitNode Then
                    'We have to be already occupying this route, and the 
                    'exit signal for the previous route has to be part of this one too
                    Exit For
                End If
            End If
        Next
    End Sub

    Private Sub GetPlatformStartDetails(ByVal TermDetails As Form1.termDetailsType)
        Dim nodeFrom As String = ""
        Dim nodeto As String = ""
        Dim prevExitSignal As String

        For Each platform In m_parent.platformArrayList
            If platform.location = TermDetails.location Then
                If Trim(platform.platform) = TermDetails.platform Then
                    nodeFrom = platform.nodefrom
                    nodeto = platform.nodeto
                    Exit For
                End If
            End If
        Next
        prevExitSignal = trainTermDetails.exitNode
        signalDistance = trainTermDetails.distance
        GetPlatformNodeDetails(nodeFrom, nodeto, prevExitSignal)
        lineSpeedArray.Clear()
        trackCircuitArray.Clear()
        routeDistance = GetStartingRouteDistance(prevExitSignal, TermDetails.direction)
        startingDistance = routeDistance - signalDistance - trainLength
        trainDistance = signalDistance + trainLength
        traversedDistance.Add(0)
        distanceArray.Clear()

        For index As Integer = trainTermDetails.occupiedTCs.Count - 1 To 0 Step -1
            Dim tempDistance As Integer
            tempDistance = trainTermDetails.occupiedDistances(index) + trainLength + signalDistance

            traversedDistance.Add(tempDistance)
            traversedRoute.Add(trainTermDetails.occupiedTCs(index))
            lastTrackCircuit = trainTermDetails.occupiedTCs(index)
        Next
        traversedDistance.RemoveAt(traversedDistance.Count - 1)
    End Sub


    Private Sub CheckIfStationDepart()
        If stationStopAtSignal Then
            'we were stopped at the station
            Console.WriteLine("Departing ")
            stationStopAtSignal = False
            trainReadyToDepart = True
            'log depart time
            RemoveCurrentTimingPoint()
            m_parent.KillFlashingTRTS(flashTRTSIndex)

        End If

    End Sub



    Private Sub RunTrain()
        routeDistance = 895


        routeStillSet = True
        absoluteDistance = 0
        stoptrackId = 1334
        removeTimingAtStop = False
        offDisplayExit = False
        trainTerminated = False
        trainExited = False
        isStopping = False
        routeToFringe = False
        stationStopAtSignal = False

        GetTrainDetails()
        objCalc = New clsCalc
        createCab()
        CreateLinespeed()
        'Dim timedelay As Integer
        'Dim traverseTime As Single


        stationStop = False
        trainSpeed = 35
        trainAction = " "
        trainReadyToDepart = False
        acceleration = 0.0
        UpdateCab()
        GetTrainTimes()
        GetDirection()
        GetEntryPoint()
        lastTrackCircuit = -1

        If CheckIfTrainExists() Then
            'We need to handle this train a bit differently as
            'it is already present in the simulation
            Console.WriteLine("Train already exists")
            stationStopAtSignal = True
            acceleration = accelRate
            trainSpeed = 0
            GetPlatformStartDetails(trainTermDetails)
            GetTimingPoint()
            'wait for train to be originate
            RemoveCurrentTimingPoint()
            Dim atDepartTime As Boolean
            atDepartTime = False
            While atDepartTime = False
                'checkDepartTime
                'wait for departure time
                atDepartTime = True

            End While
            'RemoveCurrentTimingPoint()
            trainReadyToDepart = True

        End If


        Dim trainrunning As Boolean = True
        ' Dim signalStatus As Integer
        'this is the point where you set the entry point
        'routeExitsignal = "Z901"
        GetTimingPoint()
        While trainrunning
            statusReady = False
            signalStatus = 99
            RequestSignalStatus(routeExitsignal)
            routeStillSet = True

            If stationStop Then
                Console.WriteLine("check stationstop")
                distanceArray.Clear()
                lineSpeedArray.Clear()
                trackCircuitArray.Clear()
                TraverseSubroute()
                stationStop = False
                trackArray.Clear()
                trackCircuitArray.Clear()
                routelist.Clear()
                lineSpeedArray.Clear()
                distanceArray.Clear()
            Else
                ' Console.WriteLine("check signal")


                Select Case signalStatus

                    Case 1 To 2


                        If heldAtSignal Then
                            'we've previously stopped and are now starting again
                            Console.WriteLine("Starting again")
                            CheckIfStationDepart()

                            ' If stationStopAtSignal Then

                            '                            Console.WriteLine("Departing ")
                            '                            stationStopAtSignal = False
                            '                           'log depart time
                            '                          RemoveCurrentTimingPoint()
                            '
                            'End If
                            Dim accelEndSpeed As Single
                            Console.WriteLine(" we need to travel " & startingDistance & " before we can start again")
                            accelEndSpeed = objCalc.CalcEndSpeed(0, acceleration, startingDistance)
                            DrawActualSpeedLine(stopAtDistance, stopAtDistance + startingDistance, trainSpeed, accelEndSpeed)
                            Console.WriteLine("starting distance " & startingDistance & "  " & trainDistance)
                            acceltrack(startingDistance)

                            heldAtSignal = False

                        Else
                            'go and get the distance to the stop signal
                            routeStillSet = True
                            GetRouteFromEntrySignal(routeExitsignal)
                            routeExitsignal = exitSignal
                            While routeStillSet
                                GetRouteFromEntrySignal(exitSignal)
                            End While


                            'SlowSubroute()
                            'SignalSlowSubroute()  
                            TraverseSubroute()

                            trackArray.Clear()
                            If stationStop = False Then

                                trackCircuitArray.Clear()
                                routelist.Clear()
                                lineSpeedArray.Clear()
                                distanceArray.Clear()
                            End If

                        End If

                    Case 3
                        If heldAtSignal Then
                            'we've previously stopped and are now starting again
                            Console.WriteLine("Starting again")
                            CheckIfStationDepart()

                            Dim accelEndSpeed As Single
                            accelEndSpeed = objCalc.CalcEndSpeed(0, acceleration, startingDistance)
                            DrawActualSpeedLine(stopAtDistance, stopAtDistance + startingDistance, trainSpeed, accelEndSpeed)
                            acceltrack(startingDistance)

                            heldAtSignal = False
                        Else

                            'keep running, route is clear
                            ' signalStatus = Form1.GetSignalStatus(routeExitSignal)
                            ' Console.WriteLine("Entry signal is " & routeExitsignal & " status " & signalStatus & statusReady & t.Name)
                            GetRouteFromEntrySignal(routeExitsignal)
                            routeExitsignal = exitSignal
                            For loopcount As Integer = 1 To 3
                                If routeStillSet Then
                                    GetRouteFromEntrySignal(exitSignal)
                                Else
                                    Exit For
                                End If
                            Next


                            TraverseSubroute()

                            trackArray.Clear()
                            If stationStop = False Then

                                trackCircuitArray.Clear()
                                routelist.Clear()
                                lineSpeedArray.Clear()
                                distanceArray.Clear()
                            End If
                        End If

                    Case 0

                        If heldAtSignal = False Then
                            Console.WriteLine("held at signal " & routeExitsignal)
                            heldAtSignal = True
                            If trainReadyToDepart Then
                                TRTSNodeIndex = m_parent.GetNodeIndex(routeExitsignal)
                                m_parent.FlashTRTS(TRTSNodeIndex, Me)
                            End If
                        End If

                        trainAction = "held at signal " & routeExitsignal
                        UpdateCab()
                        ScaledSleep(1500)
                        '              End If

                End Select
            End If
            trainrunning = (Not trainTerminated) And (Not trainExited)
        End While


        'Train has terminated here
        If nextTimingType = "X" Then
            'Train is going to exit the simulation
            If serviceForms.Trim().Length > 0 Then
                'create new service from this,
                'shouldn't really as train has exited rather than terminated
                serviceForms = serviceForms

                'save important parameters for passing to mew service
                Dim trainTermDetails As Form1.termDetailsType
                SetTerminatedServiceDetails(trainTermDetails)
                StartNewService(trainTermDetails)
            End If
        Else
            'this is an error

        End If

        RemovetrainDetails()
        'terminate this thread
        t.Abort()

    End Sub

    Private Sub StartNewService(ByVal trainTermDetails As Form1.termDetailsType)
        StartNewTrain(trainTermDetails)


    End Sub


    Private Function GetStartingRouteDistance(ByVal prevExitNode As String, ByVal routeDirection As String) As Integer
        Dim startDistance As Integer = 0
        If routeDirection = "1" Then
            For trackId As Integer = 0 To trackArray.Count - 1

                startDistance = startDistance + distanceArray(trackId)
                If m_parent.TrackArrayList(trackArray(trackId)).nodeto = prevExitNode Then
                    Exit For
                End If
            Next
        Else
            'should be -1
            For trackId As Integer = trackArray.Count - 1 To 0 Step -1

                startDistance = startDistance + distanceArray(trackId)
                If m_parent.TrackArrayList(trackArray(trackId)).nodeto = prevExitNode Then
                    Exit For
                End If
            Next
        End If
       
        Return startDistance
    End Function
    Public Sub CalcTargetSpeeds()

        routeDistance = 0
        ReDim accDistanceArray(0)
        ReDim accTypeArray(0)
        ReDim accInitSpeedArray(0)
        Dim initSpeed As Single
        Dim stopdistance As Integer = 0

        initSpeed = trainSpeed

        For Each distanceCount In distanceArray
            routeDistance = routeDistance + distanceCount
        Next
        If stopdistance > 0 Then
            'we're stopping in this route
            accDistanceArray(0) = routeDistance
            accInitSpeedArray(0) = 0
            initSpeed = 0

        Else
            accDistanceArray(0) = routeDistance
            accInitSpeedArray(0) = initSpeed

        End If
        For trackCount As Integer = lineSpeedArray.Count - 1 To 0 Step -1
            initSpeed = CalcInitSpeed(initSpeed, trackCount, brakingrate)
        Next

    End Sub



    Private Sub SetTerminatedServiceDetails(ByRef trainTermDetails As Form1.termDetailsType)
        Dim tempDistance As Integer
        trainTermDetails.occupiedDistances = New ArrayList
        trainTermDetails.occupiedTCs = New ArrayList

        For traversedCount As Integer = 0 To (traversedRoute.Count - 1)
            tempDistance = trainDistance - traversedDistance(traversedCount) + trainLength
            trainTermDetails.occupiedDistances.Add(tempDistance)
            trainTermDetails.occupiedTCs.Add(traversedRoute(traversedCount))
        Next
        trainTermDetails.location = stopLocation
        trainTermDetails.platform = stopPlatform
        trainTermDetails.exitNode = routeExitsignal
        'transfer the distance to the signal, the end of the route
        trainTermDetails.distance = signalDistance
    End Sub
    Private Sub CalcTargetSpeedsold()

        routeDistance = 0
        ReDim accDistanceArray(0)
        ReDim accTypeArray(0)
        ReDim accInitSpeedArray(0)
        Dim initSpeed As Single
        'If routelist.Count < 3 Then
        'then we're stopping, so set initspeed
        'initSpeed = 0
        'Else
        initSpeed = lineSpeedArray(lineSpeedArray.Count - 1)

        'End If
        For Each distanceCount In distanceArray
            routeDistance = routeDistance + distanceCount
        Next
        accDistanceArray(0) = routeDistance
        accInitSpeedArray(0) = initSpeed
        For trackCount As Integer = lineSpeedArray.Count - 1 To 0 Step -1
            initSpeed = CalcInitSpeed(initSpeed, trackCount, brakingrate)
        Next

    End Sub


    Private Sub UpdateCab()

        m_parent.ClockDisplayLabel.Invoke(New UpdateCabCallbackDelegate(AddressOf m_parent.UpdateCabCallback), New Object() {"clockref", 0, _
                                                headcode, trainSpeed, linespeed, stationDistance, signalDistance, trainDistance, currentTrack, headcode, nextTimingLoc, nextTimingTime, trainAction})

    End Sub


    Private Sub SetTc(ByVal tc As Integer)
        m_parent.ClockDisplayLabel.Invoke(New SetTcCallbackDelegate(AddressOf m_parent.SetTcPostCallback), New Object() {"clockref", tc, True})
    End Sub

    Private Sub RemovetrainDetails()
        m_parent.ClockDisplayLabel.Invoke(New RemovetrainDetailsCallbackDelegate(AddressOf m_parent.RemovetrainDetailsCallback), New Object() {"clockref", headcode, trainIndex})
    End Sub

    Private Sub createCab()
        m_parent.ClockDisplayLabel.Invoke(New createCabCallbackDelegate(AddressOf m_parent.createCabCallback), New Object() {"clockref"})
    End Sub

    Private Sub CreateLinespeed()
        m_parent.ClockDisplayLabel.Invoke(New createLinespeedCallbackDelegate(AddressOf m_parent.createLinespeedCallback), New Object() {"clockref"})
    End Sub

    Private Sub ClearTc(ByVal tc As Integer)
        m_parent.ClockDisplayLabel.Invoke(New SetTcCallbackDelegate(AddressOf m_parent.SetTcPostCallback), New Object() {"clockref", tc, False})
    End Sub

    Private Sub SetTrackTc(ByVal tc As Integer)
        m_parent.ClockDisplayLabel.Invoke(New SetTrackTcCallbackDelegate(AddressOf m_parent.SetTrackTcCallback), New Object() {"clockref", tc, True})
    End Sub
    Private Sub ClearTrackTc(ByVal tc As Integer)
        m_parent.ClockDisplayLabel.Invoke(New SetTrackTcCallbackDelegate(AddressOf m_parent.SetTrackTcCallback), New Object() {"clockref", tc, False})
    End Sub

    Private Sub RequestSignalStatus(ByVal nodeID As String)
        m_parent.ClockDisplayLabel.Invoke(New GetSignalStatusCallbackDelegate(AddressOf m_parent.GetSignalStatusCallback), New Object() {Me, nodeID})
    End Sub
    Private Sub DrawLinespeed(ByVal penColor As Color, ByVal xStart As Integer, ByVal yStart As Integer, _
                                   ByVal xEnd As Integer, ByVal yEnd As Integer)
        m_parent.ClockDisplayLabel.Invoke(New AddLineToLinespeedCallbackDelegate(AddressOf m_parent.AddLineToLinespeedCallback), New Object() {"123", penColor, xStart, yStart, xEnd, yEnd})
    End Sub




    Private Function CheckIfRouteIsFringe(ByVal routeID As String) As Boolean
        Dim routeIsFringe As Boolean = False
        Dim exitNodeId As String = ""
        'get exit node for this route

        GetRoutesDirection(routeID, exitNodeId)
        routeIsFringe = m_parent.GetnodeIsFringe(exitNodeId)
        Return routeIsFringe
    End Function

    Private Sub DoStopArrayModification()
        'Do the actual modifying of the arrays here
        Dim lastTrackDistance As Integer
        Dim signalTCDistance As Integer
        Dim lastTrackLineSpeed As Integer
        Dim signalTCLineSpeed As Integer


        lastTrackDistance = distanceArray(distanceArray.Count - 2)
        signalTCDistance = distanceArray(distanceArray.Count - 1)
        lastTrackLineSpeed = lineSpeedArray(lineSpeedArray.Count - 2)
        signalTCLineSpeed = lineSpeedArray(lineSpeedArray.Count - 1)

        If signalTCDistance < constStandardSignalTCDistance Then
            ' we assume that the signal track in a route is quite short as
            ' the track should be just before a signal and therefore 
            ' should be short. 

            distanceArray(distanceArray.Count - 2) = lastTrackDistance - constDistBeforeSignalTC
            distanceArray(distanceArray.Count - 1) = constDistBeforeSignalTC
            lineSpeedArray(lineSpeedArray.Count - 1) = constPreStoppingSpeed

            distanceArray.Add(signalTCDistance)
            lineSpeedArray.Add(0)

            If routelist.Count = 1 Then
                'We need to add in an extra track in the number 
                'of subroute tracks, but only if it's the last subroute
                subRouteTracks = subRouteTracks + 1
                trackArray.Add(trackArray(trackArray.Count - 1))
                If trackArray.Count = 2 Then
                    trackArray(1) = trackArray(0)
                Else
                    trackArray(trackArray.Count - 2) = trackArray(trackArray.Count - 3)
                End If 'trackArray.Count = 2
            End If 'routelist.Count = 1


        Else
            'otherwise the last track is longer than we'd expect.
            'here we can modify the last track rather than the one previous to it

            distanceArray(distanceArray.Count - 1) = signalTCDistance - constDistBeforeSignalTC - constStandardSignalTCDistance
            lineSpeedArray(lineSpeedArray.Count - 1) = constPreStoppingSpeed

            distanceArray.Add(constDistBeforeSignalTC)
            lineSpeedArray.Add(0)

            distanceArray.Add(constStandardSignalTCDistance)
            lineSpeedArray.Add(0)
            'lineSpeedArray.Add(signalTCLineSpeed)

            If routelist.Count = 1 Then
                'We need to add in two extra tracks in the number 
                'of subroute tracks, but only if it's the last subroute
                subRouteTracks = subRouteTracks + 2
                trackArray.Add(trackArray(trackArray.Count - 1))
                trackArray.Add(trackArray(trackArray.Count - 1))
                If trackArray.Count = 2 Then
                    trackArray(1) = trackArray(0)
                Else
                    trackArray(trackArray.Count - 2) = trackArray(trackArray.Count - 3)
                End If 'trackArray.Count = 2
            End If 'routelist.Count = 1



        End If 'lastTrackDistance < constDistBeforeSignalTC





    End Sub




    Private Sub ModifyArraysForRouteStop()
        'routeDistance is not set more than the stopping distance
        ' so we must prepare to stop

        'check if route is going to a fringe
        If routeToFringe = False Then
            'if we've already set this there's no point doing it again

            Console.WriteLine("modify as less than 3 routes set")
            Console.WriteLine("distanceArray(count-3) was " & distanceArray(distanceArray.Count - 3))
            Console.WriteLine("distanceArray(count-2) was " & distanceArray(distanceArray.Count - 2))
            Console.WriteLine("distanceArray(count-1) was " & distanceArray(distanceArray.Count - 1))

            Console.WriteLine("LineSpeedArray(count-3) was " & lineSpeedArray(distanceArray.Count - 3))
            Console.WriteLine("LineSpeedArray(count-2) was " & lineSpeedArray(distanceArray.Count - 2))
            Console.WriteLine("LineSpeedArray(count-1) was " & lineSpeedArray(distanceArray.Count - 1))



            If CheckIfRouteIsFringe(routelist(routelist.Count - 1)) Then
                'this is a fringe so we're going to leave the simulation
                'Console.WriteLine("going to fringe " & (routelist(routelist.Count - 1)))
                routeToFringe = True
                stoppingPoint = -5000
            Else
                ' it's not a fringe, so we'll have to stop

                DoStopArrayModification()


                Console.WriteLine("distanceArray(count-3)  is  " & distanceArray(distanceArray.Count - 4))
                Console.WriteLine("distanceArray(count-2)  is  " & distanceArray(distanceArray.Count - 3))
                Console.WriteLine("distanceArray(count-1)  is  " & distanceArray(distanceArray.Count - 2))
                Console.WriteLine("distanceArray(count) (NEW)  is  " & distanceArray(distanceArray.Count - 1))

                Console.WriteLine("LineSpeedArray(count-3) is  " & lineSpeedArray(distanceArray.Count - 4))
                Console.WriteLine("LineSpeedArray(count-2) is  " & lineSpeedArray(distanceArray.Count - 3))
                Console.WriteLine("LineSpeedArray(count-1) is  " & lineSpeedArray(distanceArray.Count - 2))
                Console.WriteLine("LineSpeedArray(count) NEW is  " & lineSpeedArray(distanceArray.Count - 1))



            End If 'CheckIfRouteIsFringe(routelist(routelist.Count - 1))
            'acceleration = objCalc.CalcAcceleration(trainSpeed, brakingToSpeed, distance)
            brakingDistance = objCalc.CalcDistanceFromSpeeds(trainSpeed, brakingToSpeed, brakingrate)
            'this is the distance from the end, so convert this to distance from the start
            brakingDistance = routeDistance - brakingDistance
            trackEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, distanceArray(0))



        Else 'routeToFringe = False
            'Here we are going to the fringe
            brakingDistance = 0
            trackEndSpeed = trainSpeed

        End If 'routeToFringe = False

    End Sub

    Private Function PopulateSubrouteArrays() As Integer
        Dim distance As Integer = 0
        Dim trackDistance As Integer

        continueOnRoute = True
        For Each trackId In trackArray
            If continueOnRoute Then
                trackDistance = GetTrackLength(trackId)
                distance = distance + trackDistance
                distanceArray.Add(trackDistance)
                If trackDistance < 1 Then
                    Console.WriteLine("This can't happen so must be an error")
                End If
            Else
                Exit For
            End If
        Next
        Return distance
    End Function

    Private Sub CalcSpeed()

        'First work out if we need to slow down or stop


        Dim distance As Integer = 0
        'Dim trackEndSpeed As Integer
        'Dim trackDistance As Integer
        'continueOnRoute = True
        'For Each trackId In trackArray
        'If continueOnRoute Then
        'trackDistance = GetTrackLength(trackId)
        'distance = distance + trackDistance
        'distanceArray.Add(trackDistance)
        'If trackDistance < 1 Then
        ' Console.WriteLine("This can't happen so must be an error")
        ' End If
        ' Else
        ' Exit For
        'End If
        'Next

        routeDistance = PopulateSubrouteArrays()

        If (routelist.Count < 3) And (isStopping = False) Then
            'routeDistance is not set more than the stopping distance
            ' so we must prepare to stop
            ModifyArraysForRouteStop()

            ' Dim lastTrackDistance As Integer
            ' Dim signalTCDistance As Integer
            ' Dim lastTrackLineSpeed As Integer
            ' Dim signalTCLineSpeed As Integer
            Console.WriteLine("modify as less than 3 routes set")
            ' Console.WriteLine("distanceArray(count-3) was " & distanceArray(distanceArray.Count - 3))
            ' Console.WriteLine("distanceArray(count-2) was " & distanceArray(distanceArray.Count - 2))
            ' Console.WriteLine("distanceArray(count-1) was " & distanceArray(distanceArray.Count - 1))
            '
            '            Console.WriteLine("LineSpeedArray(count-3) was " & lineSpeedArray(distanceArray.Count - 3))
            '            Console.WriteLine("LineSpeedArray(count-2) was " & lineSpeedArray(distanceArray.Count - 2))
            '            Console.WriteLine("LineSpeedArray(count-1) was " & lineSpeedArray(distanceArray.Count - 1))
            '
            '            'check if route is going to a fringe
            '            If routeToFringe = False Then
            ' 'if we've already set this there's no point doing it again
            ' If CheckIfRouteIsFringe(routelist(routelist.Count - 1)) Then
            ' 'this is a fringe so we're going to leave the simulation
            ' 'Console.WriteLine("going to fringe " & (routelist(routelist.Count - 1)))
            ' routeToFringe = True
            ' stoppingPoint = -5000
            'Else
            ' ' it's not a fringe, so we'll have to stop

            '            lastTrackDistance = distanceArray(distanceArray.Count - 2)
            '            signalTCDistance = distanceArray(distanceArray.Count - 1)
            '            lastTrackLineSpeed = lineSpeedArray(lineSpeedArray.Count - 2)
            '            signalTCLineSpeed = lineSpeedArray(lineSpeedArray.Count - 1)
            '            distanceArray(distanceArray.Count - 2) = lastTrackDistance - constDistBeforeSignalTC
            '            distanceArray(distanceArray.Count - 1) = constDistBeforeSignalTC
            '            'lineSpeedArray(lineSpeedArray.Count - 2) = constPreStoppingSpeed
            '            lineSpeedArray(lineSpeedArray.Count - 1) = constPreStoppingSpeed
            '
            '            distanceArray.Add(signalTCDistance)
            '            lineSpeedArray.Add(signalTCLineSpeed)
            '            If routelist.Count = 1 Then
            ' 'We need to add in an extra track in the number 
            'of subroute tracks, but only if it's the last subroute
            '            subRouteTracks = subRouteTracks + 1
            '            trackArray.Add(trackArray(trackArray.Count - 1))
            '            If trackArray.Count = 2 Then
            ' trackArray(1) = trackArray(0)
            '   Else
            '            trackArray(trackArray.Count - 2) = trackArray(trackArray.Count - 3)
            '        End If 'trackArray.Count = 2
            '        End If 'routelist.Count = 1
            '            Console.WriteLine("distanceArray(count-3)  is  " & distanceArray(distanceArray.Count - 4))
            '            Console.WriteLine("distanceArray(count-2)  is  " & distanceArray(distanceArray.Count - 3))
            '            Console.WriteLine("distanceArray(count-1)  is  " & distanceArray(distanceArray.Count - 2))
            '            Console.WriteLine("distanceArray(count) (NEW)  is  " & distanceArray(distanceArray.Count - 1))
            '
            '            Console.WriteLine("LineSpeedArray(count-3) is  " & lineSpeedArray(distanceArray.Count - 4))
            '            Console.WriteLine("LineSpeedArray(count-2) is  " & lineSpeedArray(distanceArray.Count - 3))
            '            Console.WriteLine("LineSpeedArray(count-1) is  " & lineSpeedArray(distanceArray.Count - 2))
            '           Console.WriteLine("LineSpeedArray(count) NEW is  " & lineSpeedArray(distanceArray.Count - 1))



            '        End If 'CheckIfRouteIsFringe(routelist(routelist.Count - 1))
            ' 'acceleration = objCalc.CalcAcceleration(trainSpeed, brakingToSpeed, distance)
            ' brakingDistance = objCalc.CalcDistanceFromSpeeds(trainSpeed, brakingToSpeed, brakingrate)
            'this is the distance from the end, so convert this to distance from the start
            '            brakingDistance = routeDistance - brakingDistance
            '            trackEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, distanceArray(0))



            '        Else 'routeToFringe = False
            'Here we are going to the fringe
            '            brakingDistance = 0 
            '           trackEndSpeed = trainSpeed

            '            If distanceArray.Count = 0 Then
            'offDisplayExit = True
            'distanceArray.Add(trainLength)
            '            End If
            '        End If 'routeToFringe = False
        End If 'routelist.Count < 3 

    End Sub

    Private Sub CalcStop()

        'First work out if we need to slow down or stop
        Dim distance As Integer = 0
        Dim trackEndSpeed As Integer
        Dim trackDistance As Integer
        braking = True
        continueOnRoute = True
        For Each trackId In trackArray
            '  If continueOnRoute Then
            trackDistance = GetTrackLength(trackId)
            distance = distance + trackDistance
            distanceArray.Add(trackDistance)
            If trackDistance < 1 Then
                Console.WriteLine("This can't happen so must be an error")
            End If
            ' Else
            'Exit For
            'End If
        Next
        'In model railway land the track circuit just before a signal
        'will be used to stop the train at an appropriate distance from
        'the signal, so we'll slow to a slow speed, then when we hit the
        'tc just before the signal then we'll stop.
        'So here we're working out the distance to the end of the tc just
        'before the signal tc, minus an arbitary distance to allow the train to be 
        'at a slow speed for a period before the signal tc, the entering of which
        'will stop the train
        routeDistance = distance - trackDistance - constDistBeforeSignalTC
        acceleration = objCalc.CalcAcceleration(trainSpeed, constPreStoppingSpeed, routeDistance)
        brakingDistance = objCalc.CalcDistanceFromSpeeds(trainSpeed, constPreStoppingSpeed, acceleration)
        'this is the distance from the end, so convert this to distance from the start
        brakingDistance = routeDistance - brakingDistance
        trackEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, distanceArray(0))
    End Sub

    Private Sub CalcStart()

        'First work out if we need to slow down or stop
        Dim distance As Integer = 0
        ' Dim trackEndSpeed As Integer
        Dim trackDistance As Integer
        braking = True
        continueOnRoute = True
        For Each trackId In trackArray
            '  If continueOnRoute Then
            trackDistance = GetTrackLength(trackId)
            distance = distance + trackDistance
            distanceArray.Add(trackDistance)
            If trackDistance < 1 Then
                Console.WriteLine("This can't happen so must be an error")
            End If
            ' Else
            'Exit For
            'End If
        Next
        'here were starting off, so check if we can accelerate up to line speed
        'or do we plod along knowing we are going to stop soon
        If signalStatus < 3 Then
            'we're going to have to stop soon so work ot the distance to the stop
            'signal, work out where we need to slow down, the half this. The
            'first half we'll accelerate, then decelerate for the second half 

        End If
        routeDistance = distance - trackDistance - (constDistBeforeSignalTC / 2)
        ' we know we're not going to reach full speed so we can go a bit closer to the
        ' signal before slowing to stopping speed
        '  brakingDistance = routeDistance / 2
        ' Console.WriteLine("total distance " & distance & " Route Distance " & routeDistance & " braking " & brakingDistance)

        'As we're going to stop, get the maximum speed we can get up to and still be able
        'to stop in the required distance
        'Work this out form the distance, deceleration and end speed
        'maxAccelToSpeed = objCalc.CalcStartSpeed(constPreStoppingSpeed, brakingrate, brakingDistance)

        'Now work out if we can accelerate to this speed
        'acceleration = objCalc.CalcAcceleration(0, maxAccelToSpeed, brakingDistance)

        'If acceleration > accelRate Then
        'then we can't accelerate this quickly
        'Console.WriteLine("can't accelerate as this rate")
        'work out the max speed with the max acceleration rate
        'acceleration = accelRate
        'maxAccelToSpeed = objCalc.CalcEndSpeed(0, acceleration, brakingDistance)
        'End If
        'braking distance is set at half the distance of the route, minus the last track
        'this is the distance from the end, so convert this to distance from the start
        'brakingDistance = routeDistance - brakingDistance
        'acceleratingDistance = routeDistance - brakingDistance
        'now check to see if this acceleration rate is ok given the line speeds
        'Dim maxCheckDistance As Integer
        'Dim maxCheckSpeed As Integer
        'maxCheckDistance = 0

        'For checkTrackIndex As Integer = 0 To trackArray.Count - 1
        'maxCheckDistance = maxCheckDistance + distanceArray(checkTrackIndex)
        'If maxCheckDistance < acceleratingDistance Then
        'we only care about the maximum speed when we're accelerating

        'maxCheckSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, maxCheckDistance)
        'If checkTrackIndex < trackArray.Count - 2 Then
        'don't check the speed for the last track in the route
        'If maxCheckSpeed > lineSpeedArray(checkTrackIndex + 1) Then
        'Console.WriteLine("track speed error " & maxCheckSpeed & " is gthan " & lineSpeedArray(checkTrackIndex + 1))
        'maxAccelToSpeed = lineSpeedArray(checkTrackIndex + 1)
        'End If
        'End If
        'End If


        'Next

        'trackEndSpeed = objCalc.CalcEndSpeed(trainSpeed, acceleration, distanceArray(0))
        'Console.WriteLine("acceleration " & acceleration & " endspeed " & trackEndSpeed & " maxaccelto " & maxAccelToSpeed & " braking " & brakingDistance)

    End Sub





    Private Sub GetTrainDetails()
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
              headcode & "' AND trainindex = '" & trainIndex & "' )"

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "trackDB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("TrackDB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader

        Dim forms As String = ""
        Dim exforms As String = ""

        While sdfRouteReader.Read
            exforms = sdfRouteReader(9).ToString
            forms = sdfRouteReader(10).ToString
        End While

        serviceForms = forms
        serviceExForms = exforms
    End Sub




    Private Sub GetTrack(ByVal nodeFrom As String, ByVal nodeTo As String)
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
        Dim trackID As Integer
        While sdfRouteReader.Read
            route = sdfRouteReader(0).ToString
            node = sdfRouteReader(1).ToString
            nodeID = sdfRouteReader(2).ToString
            tc = sdfRouteReader(4).ToString
            trackID = sdfRouteReader(9).ToString
            'SetTc(tc)
            CheckTCArray(tc, trackID)
            'Console.WriteLine("set tc " & tc)
        End While
    End Sub

    Private Sub CheckTCArray(ByVal newTC As String, ByVal newTrackId As Integer)
        Dim isNotInArray As Boolean = True
        For Each tcIndex As String In trackCircuitArray
            If tcIndex = newTC Then
                ' isNotInArray = False
            End If
        Next
        If isNotInArray Then
            'SetTc(tc)
            trackCircuitArray.Add(newTC)
            trackArray.Add(newTrackId)
            'Console.WriteLine("adding " & newTC & " at " & newTrackId)

        Else
            ' Console.WriteLine(" Not adding " & newTC & " at " & newTrackId)

        End If

    End Sub

    Private Function GetTrackLength(ByVal trackID As Integer) As Integer
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)
        Try
            routesdfDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open Route sdf dBconnection")
        End Try
        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [trackdB] where (trackID = '" & _
               trackID & "' ) "

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "trackDB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("TrackDB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader

        ' routeIsClear = True
        Dim route As String = Nothing
        ' Dim node As String
        'Dim nodeID As String
        'Dim tc As String
        'Dim trackArrayIndex As Integer
        'Dim trackStatus As Integer
        Dim trackLength As Integer
        While sdfRouteReader.Read
            ' route = sdfRouteReader(0).ToString
            'node = sdfRouteReader(1).ToString
            'nodeID = sdfRouteReader(2).ToString
            linespeed = sdfRouteReader(3).ToString
            trackLength = sdfRouteReader(10).ToString
        End While

        lineSpeedArray.Add(linespeed)
        Return trackLength
    End Function

    Private Function GetTrackLengthNoLinespeed(ByVal trackID As Integer) As Integer
        Dim sdfConnection As New System.Data.SqlClient.SqlConnectionStringBuilder

        sdfConnection("Data Source") = "C:\Users\Colin\Documents\Visual Studio 2010\Projects\DBFReader\DBFReader\nodeDB.sdf"
        Dim routesdfDbConn As New SqlServerCe.SqlCeConnection(sdfConnection.ConnectionString)
        Try
            routesdfDbConn.Open()
        Catch ex As Exception
            Console.WriteLine("can't open Route sdf dBconnection")
        End Try
        Dim routesdfTableda As New SqlServerCe.SqlCeDataAdapter
        Dim sqlSelectrouteTable As String = "SELECT * FROM [trackdB] where (trackID = '" & _
               trackID & "' ) "

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "trackDB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("TrackDB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader

        ' routeIsClear = True
        Dim route As String = Nothing
        ' Dim node As String
        'Dim nodeID As String
        'Dim tc As String
        'Dim trackArrayIndex As Integer
        'Dim trackStatus As Integer
        Dim trackLength As Integer
        While sdfRouteReader.Read
            ' route = sdfRouteReader(0).ToString
            'node = sdfRouteReader(1).ToString
            'nodeID = sdfRouteReader(2).ToString
            linespeed = sdfRouteReader(3).ToString
            trackLength = sdfRouteReader(10).ToString
        End While
        Return trackLength
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

        routesdfTableda.SelectCommand = New SqlServerCe.SqlCeCommand(sqlSelectrouteTable, routesdfDbConn)
        Dim routesdfTableds As New DataSet
        routesdfTableda.Fill(routesdfTableds, "subroutedB")
        Dim routesdfTabledt As DataTable = routesdfTableds.Tables("subroutedB")

        Dim sdfRouteReader As SqlServerCe.SqlCeDataReader
        sdfRouteReader = routesdfTableda.SelectCommand.ExecuteReader
        'sdfRouteReader.
        Dim route As String = Nothing
        Dim node As String
        Dim nodeTo As String
        Dim requires As String
        While sdfRouteReader.Read
            route = sdfRouteReader(0).ToString
            node = sdfRouteReader(1).ToString
            nodeTo = sdfRouteReader(2).ToString
            requires = sdfRouteReader(3).ToString
            routeID = route
            GetTrack(node, nodeTo)
            'routelist.Add(route)
        End While
    End Sub

    Private Sub GetRouteFromEntrySignal(ByVal EntrySignalId As String)


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
            If isSet = "1" Then
                'Only add this route if it's set
                routelist.Add(route)
                exitSignal = nodeto
                numberOfRoutes += 1

            End If
        End While
        If numberOfRoutes = 0 Then
            'No routes are set from this signal
            routeStillSet = False
        End If



    End Sub
    Public Sub invoke()

    End Sub


    Public Sub UpdateLinespeedFrm()
        m_parent.ClockDisplayLabel.Invoke(New UpdateLinespeedCallbackDelegate(AddressOf m_parent.UpdateLinespeedCallback), New Object() {"clockref", _
                                            distanceArray, lineSpeedArray, accInitSpeedArray, accDistanceArray})

    End Sub
    Public Sub StartNewTrain(ByVal trainTermDetails As Form1.termDetailsType)
        m_parent.ClockDisplayLabel.Invoke(New StartNewtrainCallbackDelegate(AddressOf m_parent.StartNewTrainCallback), New Object() {"clockref", _
                                            serviceForms, trainIndex, trainTermDetails})

    End Sub
    Public Sub SetSignalStatus(ByVal status As Integer)
        Me.signalStatus = status
    End Sub
End Class
