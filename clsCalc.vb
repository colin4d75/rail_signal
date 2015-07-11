Imports System.Math

Public Class clsCalc
    Public Function mph2Mps(ByVal mph As Single) As Single
        Dim mps As Single
        mps = mph * 0.447
        Return mps
    End Function

    Public Function Mps2mph(ByVal mps As Single) As Single
        Dim mph As Single
        mph = mps * 2.237
        Return mph
    End Function

    Public Function CalcEndSpeed(ByVal initSpeedmph As Single, _
                                 ByVal acceleration As Single, _
                                 ByVal distance As Single) As Single
        Dim endSpeedmps As Single
        Dim initSpeedmps As Single
        initSpeedmps = mph2Mps(initSpeedmph)
        Dim sqrtVal As Single
        sqrtVal = ((initSpeedmps * initSpeedmps) + (2 * acceleration * distance))
        If sqrtVal < 0.001 Then
            Return 0
        Else

            endSpeedmps = Sqrt(sqrtVal)
            Return Mps2mph(endSpeedmps)
        End If

    End Function

    Public Function CalcStartSpeed(ByVal endSpeedmph As Single, _
                                 ByVal acceleration As Single, _
                                 ByVal distance As Single) As Single
        Dim endSpeedmps As Single
        Dim initSpeedmps As Single
        endSpeedmps = mph2Mps(endSpeedmph)
        Dim sqrtVal As Single
        sqrtVal = ((endSpeedmps * endSpeedmps) - (2 * acceleration * distance))
        If sqrtVal < 0.001 Then
            Return 0
            ' if the speed is to less than zero, just return
            'zero. What this really means is that we'll stop before
            'the distance givem, but returning zero means the
            'function doesn't throw an error
        Else

            initSpeedmps = Sqrt(sqrtVal)
            Return Mps2mph(initSpeedmps)
        End If

    End Function

    Public Function CalcDistanceFromSpeeds(ByVal initSpeedmph As Single, _
                                         ByVal endSpeedmph As Single, _
                                         ByVal acceleration As Single) As Single

        Dim endSpeedmps As Single
        Dim initSpeedmps As Single
        initSpeedmps = mph2Mps(initSpeedmph)
        endSpeedmps = mph2Mps(endSpeedmph)
        Dim numerator As Single
        Dim distance As Single
        If initSpeedmph = endSpeedmph Then
            'we're already at the line speed
            distance = 0
        Else
            numerator = ((endSpeedmps * endSpeedmps) - (initSpeedmps * initSpeedmps))
            distance = numerator / (2 * acceleration)
        End If
        Return distance
    End Function

    'Function CalcAcceluntildistance
    'This function takes the initSpeed, endSpeed, distance,
    'acceleration and deceleration rates.
    'Form this, the function returns the distance for which the train
    'is accelerating
    Public Function CalcAccelUntilDistance(ByVal initSpeedmph As Single, _
                                        ByVal endSpeedmph As Single, _
                                        ByVal acceleration As Single, _
                                        ByVal deceleration As Single, _
                                        ByVal trackDistance As Single) As Integer

        Dim endSpeedmps As Single
        Dim initSpeedmps As Single
        initSpeedmps = mph2Mps(initSpeedmph)
        endSpeedmps = mph2Mps(endSpeedmph)
        Dim halfSpeedDelta As Single
        Dim numerator As Single
        Dim denominator As Single
        Dim distance As Integer
        halfSpeedDelta = ((endSpeedmps * endSpeedmps) - (initSpeedmps * initSpeedmps)) / 2
        numerator = halfSpeedDelta - (deceleration * trackDistance)
        denominator = acceleration - deceleration

        distance = numerator / denominator
        ' which should really be less than the track distance
        Return distance
    End Function


    'Function CalcAcceluntildistance
    'This function takes the initSpeed, endSpeed, distance,
    'acceleration and deceleration rates.
    'Form this, the function returns the distance for which the train
    'is accelerating
    'Overloaded function also uses the linespeed to see
    'if the max speed is over the linespeed
    Public Function CalcAccelUntilDistance(ByVal initSpeedmph As Single, _
                                       ByVal endSpeedmph As Single, _
                                       ByVal acceleration As Single, _
                                       ByVal deceleration As Single, _
                                       ByVal trackDistance As Single, _
                                       ByVal linespeed As Single) As clsTraverseRoute.accelDataType

        Dim endSpeedmps As Single
        Dim initSpeedmps As Single
        Dim lineSpeedmps As Single
        Dim maxSpeedmph As Single
        Dim accelDataobj As clsTraverseRoute.accelDataType

        initSpeedmps = mph2Mps(initSpeedmph)
        endSpeedmps = mph2Mps(endSpeedmph)
        lineSpeedmps = mph2Mps(endSpeedmph)
        Dim halfSpeedDelta As Single
        Dim numerator As Single
        Dim denominator As Single
        Dim accelDistance As Integer
        Dim decelDistance As Integer
        Dim coastDistance As Integer
        Dim tetaccelDistance As Integer
        Dim tetdecelDistance As Integer

        halfSpeedDelta = ((endSpeedmps * endSpeedmps) - (initSpeedmps * initSpeedmps)) / 2
        numerator = halfSpeedDelta - (deceleration * trackDistance)
        denominator = acceleration - deceleration

        accelDistance = numerator / denominator
        ' which should really be less than the track distance
        If accelDistance > trackDistance Then
            'we can't reach the desired max speed
            accelDistance = trackDistance

        End If

        If accelDistance < 0 Then
            'Console.WriteLine("DEBUG:: acceleration distance is less than zero " & accelDistance)
            accelDistance = 0
        End If

        maxSpeedmph = CalcEndSpeed(initSpeedmph, acceleration, accelDistance)

        'FOR MOINITORING/DEBUG
        tetaccelDistance = CalcDistanceFromSpeeds(initSpeedmph, maxSpeedmph, acceleration)
        tetdecelDistance = CalcDistanceFromSpeeds(maxSpeedmph, endSpeedmph, deceleration)

        If maxSpeedmph > linespeed Then
            'The max speed is greater than the line speed
            'instead use the distance to reach the linespeed
            accelDistance = CalcDistanceFromSpeeds(initSpeedmph, linespeed, acceleration)
            If initSpeedmph > linespeed Then
                ' Console.WriteLine("Track Speed error")
                initSpeedmph = linespeed
                accelDistance = 0
                decelDistance = CalcDistanceFromSpeeds(initSpeedmph, linespeed, deceleration)

            Else
                If endSpeedmph < linespeed Then
                    'we need to decelerat back down at the end of the line
                    decelDistance = CalcDistanceFromSpeeds(linespeed, endSpeedmph, deceleration)
                Else
                    'we can just stay at the line speed
                    decelDistance = 0
                End If
            End If

            coastDistance = trackDistance - accelDistance - decelDistance
        Else
            decelDistance = trackDistance - accelDistance
            coastDistance = 0
        End If

        accelDataobj.accelDistance = accelDistance
        accelDataobj.decelDistance = decelDistance
        accelDataobj.coastDistance = coastDistance
        Return accelDataobj
    End Function




    Public Function CalcEndSpeedfromTime(ByVal initSpeed As Single, _
                                         ByVal acceleration As Single, _
                                         ByVal traverseTime As Single) As Single

        Dim endSpeedmps As Single
        Dim initSpeedmps As Single

        initSpeedmps = mph2Mps(initSpeed)
        endSpeedmps = initSpeedmps + (acceleration * traverseTime)
        Return endSpeedmps
    End Function

    Public Function CalcAcceleration(ByVal initSpeedmph As Single, _
                                     ByVal endSpeedmph As Single, _
                                     ByVal distance As Single) As Single

        Dim endSpeedmps As Single
        Dim initSpeedmps As Single
        endSpeedmps = mph2Mps(endSpeedmph)
        initSpeedmps = mph2Mps(initSpeedmph)

        Dim acceleration As Single
        Dim numerator As Single
        numerator = ((endSpeedmps * endSpeedmps) - (initSpeedmps * initSpeedmps))
        acceleration = numerator / (2 * distance)
        Return acceleration
    End Function

    Public Function CalcTraverseTimeFromSpeeds(ByVal initSpeedmph As Single, _
                                     ByVal endSpeedmph As Single, _
                                     ByVal acceleration As Single) As Single
        Dim traverseTime As Single
        Dim initSpeedmps As Single
        initSpeedmps = mph2Mps(initSpeedmph)
        Dim endSpeedmps As Single
        endSpeedmps = mph2Mps(endSpeedmph)

        traverseTime = (initSpeedmps - endSpeedmps) / acceleration


        Return traverseTime
    End Function

    Public Function CalcTraverseTime(ByVal initSpeedmph As Single, _
                                     ByVal acceleration As Single, _
                                     ByVal distance As Single) As Single

        Dim traverseTime As Single
        Dim squareRootVal As Single
        Dim denominator As Single
        Dim numerator As Single
        Dim initSpeedmps As Single
        initSpeedmps = mph2Mps(initSpeedmph)
        Dim timeval1 As Single
        Dim timeval2 As Single
        Dim endspeed1 As Single
        Dim endspeed2 As Single

        If acceleration = 0 Then
            traverseTime = distance / initSpeedmps
        Else
            squareRootVal = Sqrt((initSpeedmps * initSpeedmps) + (2 * acceleration * distance))
            denominator = acceleration
            numerator = (0 - initSpeedmps - squareRootVal)
            timeval1 = numerator / denominator
            endspeed1 = CalcEndSpeedfromTime(initSpeedmph, acceleration, timeval1)

            If endspeed1 > -0.001 Then
                'distanceval1 = CalcDistance(initSpeedmps, acceleration, timeval1)
                traverseTime = timeval1
            Else
                numerator = (0 - initSpeedmps + squareRootVal)
                timeval2 = numerator / denominator
                endspeed2 = CalcEndSpeedfromTime(initSpeedmph, acceleration, timeval2)
                'distanceval2 = CalcDistance(initSpeedmps, acceleration, timeval2)
                traverseTime = timeval2
            End If
        End If
        Return traverseTime
    End Function

    Public Function CalcDistance(ByVal initSpeedmps As Single, _
                                 ByVal acceleration As Single, _
                                 ByVal traverseTime As Single) As Single

        Dim utVal As Single
        Dim halfatSquared As Single
        Dim trackdistance As Single

        utVal = initSpeedmps * traverseTime
        halfatSquared = 0.5 * acceleration * traverseTime * traverseTime
        trackdistance = utVal + halfatSquared
        Return trackdistance
    End Function

    Public Function CalcDistanceMph(ByVal initSpeedmph As Single, _
                                    ByVal acceleration As Single, _
                                    ByVal traverseTime As Single) As Single

        Dim initSpeedmps As Single
        initSpeedmps = mph2Mps(initSpeedmph)
        Dim utVal As Single
        Dim halfatSquared As Single
        Dim trackdistance As Single

        utVal = initSpeedmps * traverseTime
        halfatSquared = 0.5 * acceleration * traverseTime * traverseTime
        trackdistance = utVal + halfatSquared
        Return trackdistance
    End Function


End Class
