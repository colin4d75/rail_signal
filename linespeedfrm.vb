Imports System.Math
Imports System.Threading


Public Class linespeedfrm

    Const constDistBeforeSignalTC As Integer = 40
    Const constPreStoppingSpeed As Integer = 10

    Private mygraphics As Graphics
    Private line1XStart As Integer
    Private line2XStart As Integer
    Private line3XStart As Integer
    Private line4XStart As Integer
    Private line5XStart As Integer
    Private line5Xend As Integer
    Private routeDistance As Integer
    Private totalDistance As Integer
    Private displayWidth As Integer
    Private accInitSpeedArray(1) As Integer
    Private accDistanceArray(1) As Integer
    Private accTypeArray(1) As Integer
    Private lineSpeedArray As New ArrayList()
    Private distanceArray As New ArrayList()
    Private brakingrate As Integer = -0.75
    Private accelDataobj As clsTraverseRoute.accelDataType
    Private trainSpeed As Single
    Private trackEndSpeed As Single
    Private acceleration As Single = 1.0
    Private currentTrackDistance As Integer
    Private linespeed As Integer



    Dim objcalc As New clsCalc
    ' Dim traverserouteobj As New clsTraverseRoute(Me)

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        PictureBox1.Image = New Bitmap(570, 300)
        PictureBox2.Image = New Bitmap(570, 300)
        mygraphics = Graphics.FromImage(PictureBox1.Image)
        SetInitValues()

    End Sub

    Public Function CompareAccInitSpeedArray(ByVal accInitSpeedArrayIn() As Integer) As Boolean
        Dim isTheSame As Boolean = True

        If accInitSpeedArrayIn.Count = accInitSpeedArray.Count Then
            For index As Integer = 0 To accInitSpeedArrayIn.Count - 1
                If accInitSpeedArrayIn(index) <> accInitSpeedArray(index) Then
                    isTheSame = False
                End If
            Next
        Else
            isTheSame = False
        End If

        Return isTheSame
    End Function

    Public Function CompareAccDistanceArray(ByVal accDistanceArrayIn() As Integer) As Boolean
        Dim isTheSame As Boolean = True

        If accDistanceArray.Count = accInitSpeedArray.Count Then
            For index As Integer = 0 To accDistanceArrayIn.Count - 1
                If index < accDistanceArray.Count - 1 Then

                    If accDistanceArrayIn(index) <> accDistanceArray(index) Then
                        isTheSame = False
                    End If
                End If

            Next
        Else
            isTheSame = False
        End If

        Return isTheSame
    End Function

    Public Sub CalcTargetSpeeds()

        routeDistance = 0
        ReDim accDistanceArray(0)
        ReDim accTypeArray(0)
        ReDim accInitSpeedArray(0)
        Dim initSpeed As Single
        'If routelist.Count < 3 Then
        'then we're stopping, so set initspeed
        'initSpeed = 0
        'Else
        initSpeed = CType(endspeedLabel.Text, Integer)

        'End If
        For Each distanceCount In distanceArray
            routeDistance = routeDistance + distanceCount
        Next
        If stopPointLabel.Text > 0 Then
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

        endSpeedmps = objcalc.mph2Mps(endSpeed)
        lineSpeedmps = objcalc.mph2Mps(currentLinespeed)

        If endSpeed = 0 Then
            'we are stopping, so what we want to do is slow just before the last track
            'so that when the train hits it, it can slow to zero without slamming on the brakes

        End If
        If lineIndex > 0 Then
            previousLineSpeedmph = lineSpeedArray(lineIndex - 1)
        Else
            previousLineSpeedmph = currentLinespeed
        End If

        previousLineSpeedmps = objcalc.mph2Mps(previousLineSpeedmph)

        If currentLinespeed < endSpeed Then
            'we don't need to do anything as the speed is slower
            ' previously and we'll be accelerating
            '
            '          ---------
            '          |
            '          |\
            '----------| \
            '
            addToArray(routeDistance - currentTrackDistance, 0, currentLinespeed)
            initMaxSpeedmps = objcalc.mph2Mps(currentLinespeed)

        ElseIf currentLinespeed = endSpeed Then
            '
            '-------------------
            '          \
            '          |\
            '          | \
            '
            addToArray(routeDistance - currentTrackDistance, 0, currentLinespeed)
            initMaxSpeedmps = objcalc.mph2Mps(endSpeed)
        Else


            'we're slowing
            Dim sqrtVal As Single
            sqrtVal = ((endSpeedmps * endSpeedmps) - (2 * acceleration * currentTrackDistance))
            initMaxSpeedmps = Sqrt(sqrtVal)
            If initMaxSpeedmps > lineSpeedmps Then
                'we are going to exceed the line speed
                'So the initial speed has to be the line speed.
                'We must coast for a bit at line speed,
                'then decelerate to the target end speed
                '
                ' -------     line speed
                '        \
                '         \
                '          \

                'now work out where to start decelerating
                decelerationDistance = objcalc.CalcDistanceFromSpeeds(currentLinespeed, endSpeed, acceleration)
                'Add deceleration point
                addToArray(routeDistance - decelerationDistance, -1, currentLinespeed)

                If initMaxSpeedmps > previousLineSpeedmps Then
                    'line before has a lower linespeed than this, so we can't
                    'set the higher value  - the predicted max value as the
                    'target speed, so set this to the line speed of the line before
                    addToArray(routeDistance - currentTrackDistance, 1, objcalc.Mps2mph(previousLineSpeedmps))
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

                    addToArray(routeDistance - currentTrackDistance, -1, objcalc.Mps2mph(previousLineSpeedmps))
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
                    addToArray(routeDistance - currentTrackDistance, -1, objcalc.Mps2mph(initMaxSpeedmps))

                End If
            End If
        End If


        routeDistance = routeDistance - currentTrackDistance
        Return objcalc.Mps2mph(initMaxSpeedmps)
    End Function


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



    Private Sub linespeed1Ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles linespeed1Ctrl.Scroll
        linespeed1Label.Text = 135 - linespeed1Ctrl.Value
    End Sub



    Private Sub linespeed2Ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles linespeed2Ctrl.Scroll
        linespeed2Label.Text = 135 - linespeed2Ctrl.Value
    End Sub

    Private Sub linespeed3Ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles linespeed3Ctrl.Scroll
        linespeed3Label.Text = 135 - linespeed3Ctrl.Value

    End Sub

    Private Sub linespeed4Ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles linespeed4Ctrl.Scroll
        linespeed4Label.Text = 135 - linespeed4Ctrl.Value

    End Sub

    Private Sub linespeed5ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles linespeed5ctrl.Scroll
        linespeed5Label.Text = 135 - linespeed5ctrl.Value

    End Sub

    Private Sub endspeedctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles endspeedctrl.Scroll
        endspeedLabel.Text = 125 - endspeedctrl.Value

    End Sub

    Private Sub distance1ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles distance1ctrl.Scroll
        distance1label.Text = distance1ctrl.Value

    End Sub

    Private Sub distance2ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles distance2ctrl.Scroll
        distance2label.Text = distance2ctrl.Value

    End Sub

    Private Sub distance3ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles distance3ctrl.Scroll
        distance3label.Text = distance3ctrl.Value

    End Sub

    Private Sub distance4ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles distance4ctrl.Scroll
        distance4label.Text = distance4ctrl.Value

    End Sub

    Private Sub distance5ctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles distance5ctrl.Scroll
        distance5label.Text = distance5ctrl.Value

    End Sub

    Public Sub UpdateaccInitSpeedArray(ByVal accinitSpeedArrayIn() As Integer
                                 )
       
        accInitSpeedArray = accinitSpeedArrayIn

    End Sub

    Public Sub UpdateaccDistanceArray(ByVal accDistanceArrayIn() As Integer
                                 )
        accDistanceArray = accDistanceArrayIn

    End Sub


    Public Sub UpdateDistanceArray(ByVal DistanceArrayIn As ArrayList
                               )
        distanceArray = DistanceArrayIn

    End Sub
    Public Sub UpdateLineSpeedArray(ByVal LineSpeedArrayIn As ArrayList
                                  )
        lineSpeedArray = LineSpeedArrayIn

    End Sub
    Private Sub SetInitValues()
        linespeed1Label.Text = 75
        linespeed1Ctrl.Value = 135 - 75
        linespeed2Label.Text = 40
        linespeed2Ctrl.Value = 135 - 40
        linespeed3Label.Text = 100
        linespeed3Ctrl.Value = 135 - 100
        linespeed4Label.Text = 125
        linespeed4Ctrl.Value = 135 - 125
        linespeed5Label.Text = 95
        linespeed5ctrl.Value = 135 - 95

        startspeedctrl.Value = 125 - 50
        startspeedlabel.Text = 50
        endspeedctrl.Value = 125 - 50
        endspeedLabel.Text = 50

        distance1ctrl.Value = 225
        distance1label.Text = 225
        distance2ctrl.Value = 300
        distance2label.Text = 300
        distance3ctrl.Value = 30
        distance3label.Text = 30
        distance4ctrl.Value = 95
        distance4label.Text = 95
        distance5ctrl.Value = 40
        distance5label.Text = 40
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
       
        mygraphics.Clear(Color.Black)
        CalcLineDistances()
        Drawgrid()
        DrawLineSpeeds()
        GetAcceleration()
        UpdateDisplay()
        GetTotalDistance()
        If scanbutton.Checked = True Then

            For count As Integer = 50 To routeDistance Step 10
                stopPointLabel.Text = count
                stopPointCtrl.Value = count
                Console.WriteLine("Stop distance is " & count)
                mygraphics.Clear(Color.Black)
                CalcLineDistances()
                Drawgrid()
                DrawLineSpeeds()
                GetAcceleration()
                UpdateDisplay()
                Refresh()
                Thread.Sleep(1000)
                UpdateDisplay()
                Refresh()
            Next
        End If
    End Sub


    Public Sub GetAcceleration()
        ModifySpeedsForStopping()
        CalcTargetSpeeds()
        DrawMaxSpeeds()
        CalcTraverseSpeeds()
    End Sub

    Private Sub DrawActualSpeedLine(ByVal startDistance As Integer, ByVal endDistance As Integer, _
                                     ByVal startSpeed As Integer, ByVal endSpeed As Integer)
        Dim startxccord As Integer
        Dim endxcoord As Integer
        Dim startycoord As Integer
        Dim endycoord As Integer

        startxccord = ConvertToXCoord(startDistance, totalDistance, displayWidth) + 30
        endxcoord = ConvertToXCoord(endDistance, totalDistance, displayWidth) + 30
        startycoord = ConvertToYCoord(startSpeed)
        endycoord = ConvertToYCoord(endSpeed)
        Drawline(Color.Orange, startxccord, startycoord, endxcoord, endycoord)


    End Sub

    Public Sub CalcTraverseSpeeds()
        Dim accelEndSpeed As Integer
        Dim currentDistance As Integer
        Dim trackdistance As Integer
        Dim trackcount As Integer
        Dim trackEnd As Integer
        trainSpeed = CType(startspeedlabel.Text, Integer)
        If trainSpeed > accInitSpeedArray(0) Then
            'Console.WriteLine("DEBUG:: Init speed is too high")
            trainSpeed = accInitSpeedArray(0) - 1
        End If
        trackcount = 0
        currentDistance = 0
        trackdistance = 0
        trackEnd = 0

        For subtrackCount As Integer = 0 To accInitSpeedArray.Count - 2
            trackEndSpeed = accInitSpeedArray(subtrackCount + 1)
            currentTrackDistance = accDistanceArray(subtrackCount + 1) - accDistanceArray(subtrackCount)
            linespeed = lineSpeedArray(trackcount)
            accelDataobj = objcalc.CalcAccelUntilDistance(trainSpeed, trackEndSpeed, acceleration, brakingrate, currentTrackDistance, linespeed)
            accelEndSpeed = objcalc.CalcEndSpeed(trainSpeed, acceleration, accelDataobj.accelDistance)
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

            DrawActualSpeedLine(currentDistance, currentDistance + accelDataobj.accelDistance, trainSpeed, accelEndSpeed)
            trainSpeed = accelEndSpeed
            currentDistance = currentDistance + accelDataobj.accelDistance
            DrawActualSpeedLine(currentDistance, currentDistance + accelDataobj.coastDistance, trainSpeed, accelEndSpeed)
            currentDistance = currentDistance + accelDataobj.coastDistance
            accelEndSpeed = objcalc.CalcEndSpeed(trainSpeed, brakingrate, accelDataobj.decelDistance)
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

            DrawActualSpeedLine(currentDistance, currentDistance + accelDataobj.decelDistance, trainSpeed, accelEndSpeed)
            currentDistance = currentDistance + accelDataobj.decelDistance
            trainSpeed = accelEndSpeed
            If trainSpeed <> trackEndSpeed Then
                'Console.WriteLine("Track end speed is incorrect " & trainSpeed & " is not " & trackEndSpeed)
            End If
            If (currentDistance - trackEnd) = distanceArray(trackcount) Then


                Select Case trackcount
                    Case 0
                        initspeed1label.Text = startspeedlabel.Text
                        initspeed2label.Text = CType(trainSpeed, Integer)
                    Case 1
                        initspeed3label.Text = CType(trainSpeed, Integer)
                    Case 2
                        initspeed4label.Text = CType(trainSpeed, Integer)
                    Case 3
                        initspeed5label.Text = CType(trainSpeed, Integer)
                    Case 4
                        initspeed6label.Text = CType(trainSpeed, Integer)
                        ' Case 5
                        '    initspeed6label.Text = trainSpeed



                End Select



                trackcount = trackcount + 1
                trackEnd = currentDistance
            End If
        Next





    End Sub


    Private Sub CalcLineDistances()
        Dim totalDistanceScale As Single
        totalDistance = GetTotalDistance()
        totalDistanceScale = PictureBox1.Width - 100
        displayWidth = totalDistanceScale

        'Dim lineDistance As Single

        distanceArray.Clear()
        distanceArray.Add(CType(distance1label.Text, Integer))
        distanceArray.Add(CType(distance2label.Text, Integer))
        distanceArray.Add(CType(distance3label.Text, Integer))
        distanceArray.Add(CType(distance4label.Text, Integer))
        distanceArray.Add(CType(distance5label.Text, Integer))


        line1XStart = 30
        line2XStart = line1XStart + ConvertToXCoord(CType(distance1label.Text, Integer), totalDistance, totalDistanceScale)
        line3XStart = line2XStart + ConvertToXCoord(CType(distance2label.Text, Integer), totalDistance, totalDistanceScale)
        line4XStart = line3XStart + ConvertToXCoord(CType(distance3label.Text, Integer), totalDistance, totalDistanceScale)
        line5XStart = line4XStart + ConvertToXCoord(CType(distance4label.Text, Integer), totalDistance, totalDistanceScale)
        line5Xend = line5XStart + ConvertToXCoord(CType(distance5label.Text, Integer), totalDistance, totalDistanceScale)


    End Sub

    Public Sub DrawGridFromArray()
        Dim XCoord As Integer
        Dim YCoord As Integer
        Dim lineXstart As Integer = 30
        Dim lineXend As Integer = 30
        Dim totalDistanceScale As Single
        Dim distance As Integer
        Dim linespeed As Integer
        totalDistance = GetTotalDistance()
        totalDistanceScale = PictureBox1.Width - 100
        displayWidth = totalDistanceScale
        mygraphics.Clear(Color.Black)


        Drawline(Color.Gray, lineXstart, 100, lineXstart, 200)

        For index As Integer = 0 To (distanceArray.Count - 1)
            distance = distanceArray(index)
            linespeed = lineSpeedArray(index)
            XCoord = ConvertToXCoord(distance, totalDistance, totalDistanceScale)
            YCoord = ConvertToYCoord(linespeed)
            lineXend = lineXstart + XCoord

            Drawline(Color.Gray, lineXend, 100, lineXend, 200)
            Drawline(Color.Red, lineXstart, YCoord, lineXend, YCoord)
            lineXstart = lineXend

        Next
    End Sub
    Private Sub Drawgrid()
        'PictureBox1.Image. = New Bitmap(570, 300)

        Drawline(Color.Gray, line1XStart, 100, line1XStart, 200)
        Drawline(Color.Gray, line2XStart, 100, line2XStart, 200)
        Drawline(Color.Gray, line3XStart, 100, line3XStart, 200)
        Drawline(Color.Gray, line4XStart, 100, line4XStart, 200)
        Drawline(Color.Gray, line5XStart, 100, line5XStart, 200)
        Drawline(Color.Gray, line5Xend, 100, line5Xend, 200)


    End Sub

    Private Function ConvertToXCoord(ByVal distance As Integer, _
                                     ByVal routeDistance As Integer, _
                                     ByVal displayWidth As Integer) As Integer


        Dim xcoord As Single
        xcoord = distance / routeDistance
        xcoord = (xcoord * displayWidth)
        Return xcoord
    End Function

    Private Function ConvertToYCoord(ByVal Linespeed As Integer) As Integer
        Dim baseline As Integer = 180
        Dim ycoord As Integer
        ycoord = baseline - (Linespeed / 2)
        Return ycoord
    End Function

    Public Sub DrawMaxSpeeds()

        Dim startxccord As Integer
        Dim endxcoord As Integer
        Dim startycoord As Integer
        Dim endycoord As Integer

        For indexcount As Integer = 0 To accInitSpeedArray.Count - 2
            startxccord = ConvertToXCoord(accDistanceArray(indexcount), totalDistance, displayWidth) + 30
            endxcoord = ConvertToXCoord(accDistanceArray(indexcount + 1), totalDistance, displayWidth) + 30
            startycoord = ConvertToYCoord(accInitSpeedArray(indexcount))
            endycoord = ConvertToYCoord(accInitSpeedArray(indexcount + 1))
            Drawline(Color.Blue, startxccord, startycoord, endxcoord, endycoord)
        Next

    End Sub




    Private Sub DrawLineSpeeds()
        Dim line1speed As Integer
        Dim line2speed As Integer
        Dim line3speed As Integer
        Dim line4speed As Integer
        Dim line5speed As Integer
        Dim baseline As Integer = 180

        lineSpeedArray.Clear()

        lineSpeedArray.Add(CType(linespeed1Label.Text, Integer))
        lineSpeedArray.Add(CType(linespeed2Label.Text, Integer))
        lineSpeedArray.Add(CType(linespeed3Label.Text, Integer))
        lineSpeedArray.Add(CType(linespeed4Label.Text, Integer))
        lineSpeedArray.Add(CType(linespeed5Label.Text, Integer))

      
        line1speed = ConvertToYCoord(linespeed1Label.Text)
        line2speed = ConvertToYCoord(linespeed2Label.Text)
        line3speed = ConvertToYCoord(linespeed3Label.Text)
        line4speed = ConvertToYCoord(linespeed4Label.Text)
        line5speed = ConvertToYCoord(linespeed5Label.Text)

        Drawline(Color.Red, line1XStart, line1speed, line2XStart, line1speed)
        Drawline(Color.Red, line2XStart, line2speed, line3XStart, line2speed)
        Drawline(Color.Red, line3XStart, line3speed, line4XStart, line3speed)
        Drawline(Color.Red, line4XStart, line4speed, line5XStart, line4speed)
        Drawline(Color.Red, line5XStart, line5speed, line5Xend, line5speed)


    End Sub

    Public Sub DrawLineSpeedsFromArray()
        Dim line1speed As Integer
        Dim line2speed As Integer
        Dim line3speed As Integer
        Dim line4speed As Integer
        Dim line5speed As Integer
        Dim baseline As Integer = 180

        lineSpeedArray.Clear()

        lineSpeedArray.Add(CType(linespeed1Label.Text, Integer))
        lineSpeedArray.Add(CType(linespeed2Label.Text, Integer))
        lineSpeedArray.Add(CType(linespeed3Label.Text, Integer))
        lineSpeedArray.Add(CType(linespeed4Label.Text, Integer))
        lineSpeedArray.Add(CType(linespeed5Label.Text, Integer))


        line1speed = ConvertToYCoord(linespeed1Label.Text)
        line2speed = ConvertToYCoord(linespeed2Label.Text)
        line3speed = ConvertToYCoord(linespeed3Label.Text)
        line4speed = ConvertToYCoord(linespeed4Label.Text)
        line5speed = ConvertToYCoord(linespeed5Label.Text)

        Drawline(Color.Red, line1XStart, line1speed, line2XStart, line1speed)
        Drawline(Color.Red, line2XStart, line2speed, line3XStart, line2speed)
        Drawline(Color.Red, line3XStart, line3speed, line4XStart, line3speed)
        Drawline(Color.Red, line4XStart, line4speed, line5XStart, line4speed)
        Drawline(Color.Red, line5XStart, line5speed, line5Xend, line5speed)


    End Sub

    Private Sub ModifySpeedsForStopping()

        ' Dim lastTrackDistance As Integer
        'Dim signalTCDistance As Integer
        'Dim lastTrackLineSpeed As Integer
        'Dim signalTCLineSpeed As Integer
        Dim currentDistance As Integer
        Dim stopPoint As Integer
        'Dim currentTrackDistance As Integer

        stopPoint = stopPointLabel.Text

        If (stopPoint > (routeDistance - distanceArray(distanceArray.Count - 1) - constDistBeforeSignalTC)) And stopPoint > 0 Then
            'This is a signal stop
            SlowForSignalStop()

        ElseIf stopPoint > 0 Then
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
    End Sub

    Private Sub SlowForSignalStop()

        Dim lastTrackDistance As Integer
        Dim signalTCDistance As Integer
        Dim lastTrackLineSpeed As Integer
        Dim signalTCLineSpeed As Integer

        If stopPointLabel.Text > (routeDistance - distanceArray(distanceArray.Count - 1) - constDistBeforeSignalTC) Then

            'The assume we're stopping at the end of the route


            lastTrackDistance = distanceArray(distanceArray.Count - 2)
            signalTCDistance = distanceArray(distanceArray.Count - 1)
            lastTrackLineSpeed = lineSpeedArray(lineSpeedArray.Count - 2)
            signalTCLineSpeed = lineSpeedArray(lineSpeedArray.Count - 1)
            If lastTrackDistance < constDistBeforeSignalTC Then
                'slowing point extends into previous track i.e. count -3
                distanceArray(distanceArray.Count - 3) = distanceArray(distanceArray.Count - 3) + lastTrackDistance - constDistBeforeSignalTC
                distanceArray(distanceArray.Count - 2) = constDistBeforeSignalTC - lastTrackDistance
                distanceArray(distanceArray.Count - 1) = lastTrackDistance
                lineSpeedArray(lineSpeedArray.Count - 2) = constPreStoppingSpeed
                lineSpeedArray(lineSpeedArray.Count - 1) = constPreStoppingSpeed

            Else
                distanceArray(distanceArray.Count - 2) = lastTrackDistance - constDistBeforeSignalTC
                distanceArray(distanceArray.Count - 1) = constDistBeforeSignalTC
                'lineSpeedArray(lineSpeedArray.Count - 2) = constPreStoppingSpeed
                lineSpeedArray(lineSpeedArray.Count - 1) = constPreStoppingSpeed

            End If

            distanceArray.Add(signalTCDistance)
            lineSpeedArray.Add(signalTCLineSpeed)

        Else

        End If



    End Sub


    Private Function GetTotalDistance() As Integer
        Dim totalDistance As Integer = 0
        For Each trackDistance In distanceArray
            totalDistance = totalDistance + trackDistance

        Next
        ' totalDistance = CType(distance1label.Text, Integer) + _
        'CType(distance2label.Text, Integer)(+ _
        'CType(distance3label.Text, Integer) + _
        'CType(distance4label.Text, Integer) + _
        'CType(distance5label.Text, Integer))

        stopPointCtrl.Maximum = totalDistance
        routeDistance = totalDistance
        Return totalDistance
    End Function

    Public Sub UpdateDisplay()


        Dim displaywidth As Integer
        Dim displayheight As Integer
        displaywidth = PictureBox2.Width
        displayheight = PictureBox2.Height



        Dim fr_rect As New Rectangle(PictureBox1.Location.X, PictureBox1.Location.Y, PictureBox2.Height, PictureBox2.Width)
        Dim to_rect As New Rectangle(0, 0, displaywidth, displayheight)
        Dim to_bm As New Bitmap(displaywidth, displayheight)
        'Dim to_bm2 As New Bitmap(CInt(PictureBo'x1.Image.Width / 10), CInt(PictureBox1.Image.Height / 10))
        Dim gr As Graphics = Graphics.FromImage(to_bm)
        Dim gr2 As Graphics = Graphics.FromImage(to_bm)
        'PictureBox2.Width = displayWidth
        'PictureBox2.Height = displayHeight

        Dim mypen As Pen
        mypen = New Pen(Color.Beige)

        mypen.Color = Color.Red
        mypen.Width = 4

        'gr2.DrawLine(pen:=mypen, x1:=100, y1:=200, x2:=200, y2:=100)

        'gr.DrawImage(PictureBox1.Image, to_rect, fr_rect, GraphicsUnit.Pixel)
        gr2.DrawImage(Me.PictureBox1.Image, 0, 0, PictureBox2.Width, PictureBox2.Height)
        'gr2.DrawRectangle(Pens.White, fr_rect)
        PictureBox2.Image = to_bm

    End Sub
    Public Sub Drawline(ByVal penColor As Color, _
                        ByVal Xstart As Single, _
                        ByVal Ystart As Single, _
                        ByVal Xend As Single, _
                        ByVal Yend As Single)
        Dim mypen As Pen
        mypen = New Pen(penColor, 3)

        'draw the line on the form using the pen object
        mygraphics.DrawLine(pen:=mypen, x1:=Xstart, y1:=Ystart, x2:=Xend, y2:=Yend)
        UpdateDisplay()
        Refresh()
    End Sub

    Private Sub startspeedctrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles startspeedctrl.Scroll
        startspeedlabel.Text = 125 - startspeedctrl.Value
    End Sub

    Private Sub stopPointCtrl_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles stopPointCtrl.Scroll
        stopPointLabel.Text = stopPointCtrl.Value
    End Sub


    Private Sub Button1_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.MouseHover

    End Sub
End Class