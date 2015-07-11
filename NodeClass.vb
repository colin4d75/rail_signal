Imports System.Data.SqlServerCe


Public Class NodeClass
    Public mXcentre As Integer
    Public mYcentre As Integer
    Public labelXpos As Integer
    Public labelYpos As Integer
    Public p2NodeXpos As Integer
    Public p2NodeYpos As Integer
    Public lever As String
    Public devFrom As Byte
    Public devTo As Byte
    Private pointLocked As Boolean
    Private auto As String
    Public nodeIndex As Integer
    Private myRectangle As Rectangle
    Private labelRectangle As Rectangle
    Public nodeId As String
    Public nodeType As String
    Public nodeIsFringe As Boolean
    Public runDirection As Integer
    Private myGraphics As Graphics
    Private myPen As New Pen(Color.Blue)
    Private myRedPen As New Pen(Color.Red)
    Private myRedSignalPen As New Pen(Color.Red)
    Private myWhitePen As New Pen(Color.Red)
    Private myGreenPen As New Pen(Color.Green)
    Private myweePen As New Pen(Color.Green)
    Private myBlackPen As New Pen(Color.Black)
    Private pointType As String
    Private neighboursArrayList As New ArrayList
    Public signalState As Integer
    Public subsidSignalSet As Boolean
    Dim brush As SolidBrush = New SolidBrush(Color.Blue)
    Public pointSetReverse As Boolean = True
    Public isSet As Boolean = False


    Public Function GetPointSetReverse() As Boolean
        Return pointSetReverse
    End Function



    Public Function GetNodeType() As String
        Return nodeType
    End Function

    Public Function GetNodeLever() As String
        Return lever
    End Function

    Private Function GetNodeCoords(ByVal nodeIDSet As String) As Point

        Dim sqlACTDetailsSelect As String = "SELECT  " & _
                     "nodeXpos,nodeYpos FROM nodedB " & _
                       "WHERE (nodeID = '" & _
                     nodeIdSet & "')"
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

        Dim nodePoint As Point
        While (dr.Read())
            nodePoint.Y = dr(0).ToString
            nodePoint.X = dr(1).ToString
        End While
        Console.WriteLine("close it now")

        objConn.Close()
        Return nodePoint

    End Function

    Private Sub GetPointNeighbours()

        Dim sqlACTDetailsSelect As String = "SELECT  " & _
                     "nodeto FROM trackdB " & _
                       "WHERE (node = '" & _
                     nodeId & "')"
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
            neighboursArrayList.Add(dr(0).ToString)
        End While
        Console.WriteLine("close it now")
        objConn.Close()
    End Sub

    Private Sub GetPointToNeighbours()

        Dim sqlACTDetailsSelect As String = "SELECT  " & _
                     "node FROM trackdB " & _
                       "WHERE (nodeto = '" & _
                     nodeId & "')"
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

        Dim entrySignal As String = 0
        While (dr.Read())
            neighboursArrayList.Add(dr(0).ToString)
        End While
        objConn.Close()
        Console.WriteLine("close it now")

    End Sub

    Public Sub New(ByVal xCentre As Integer, ByVal yCentre As Integer, ByVal setNodeType As String, _
                   ByVal runDirectionSet As Integer, ByVal nodeIndexSet As Integer, ByVal setNodeId As String, _
                   ByVal setLabelXpos As Integer, ByVal setLabelYpos As Integer, _
                   ByVal setp2NodeXpos As Integer, ByVal setp2NodeYpos As Integer, _
                   ByVal setLever As String, ByVal setAuto As String,
                   ByVal setDevTo As Byte, ByVal setDevFrom As Byte)
        mXcentre = xCentre - 0
        mYcentre = yCentre - 0
        pointLocked = True
        labelXpos = setLabelXpos
        labelYpos = setLabelYpos
        p2NodeXpos = setp2NodeXpos
        p2NodeYpos = setp2NodeYpos
        lever = setLever
        auto = setAuto
        nodeId = setNodeId
        nodeType = setNodeType
        nodeIndex = nodeIndexSet
        runDirection = runDirectionSet
        devTo = setDevTo
        devFrom = setDevFrom

        Dim myPen As Pen
        'instantiate a new pen object using the color structure
        myPen = New Pen(Color:=Color.Blue, Width:=4)
        myRedPen = New Pen(Color:=Color.Red, Width:=4)
        myRedSignalPen = New Pen(Color:=Color.Red, Width:=4)
        myWhitePen = New Pen(Color:=Color.White, Width:=4)
        myGreenPen = New Pen(Color:=Color.Green, Width:=3)
        myweePen = New Pen(Color:=Color.Gray, Width:=1)
        neighboursArrayList = New ArrayList
        ' Console.WriteLine("adding node " & nodeId & " index " & nodeIndex)


    End Sub

    Public Sub DisplayNode()



        Select Case nodeType


            Case "B"
                If labelXpos > 0 Then
                    Form1.DrawBufferLabel(labelXpos, labelYpos, Color.Red, lever)
                End If
                labelRectangle = New Rectangle(labelXpos, labelYpos, 20, 10)
            Case "F"
                ' Form1.DrawBufferLabel(xCentre, yCentre, Color.Red, nodeId)

                'labelRectangle = New Rectangle(xCentre, yCentre, 20, 10)

            Case "Y"
                Form1.DrawBufferLabel(labelXpos, labelYpos, Color.Red, lever)
                labelRectangle = New Rectangle(labelXpos, labelYpos, 20, 10)

            Case "P"
                If labelXpos > 0 Then
                    Form1.DrawLabel(labelXpos, labelYpos, Color.Red, lever)
                End If
                'Form1.DrawNode(myPen, mXcentre, mYcentre)
                ' SetPointType()
            Case "S"
                'it's a signal

                If labelXpos > 0 Then
                    Form1.DrawSignalLabel(labelXpos, labelYpos, Color.Red, lever)
                End If
                Dim sigXCentre As Integer
                Dim sigYCentre As Integer
                sigXCentre = mXcentre + (runDirection * 11)
                sigYCentre = mYcentre - 2 - (runDirection * 8)
                If nodeId = "S008" Then
                    signalState = 0

                End If
                'Form1.DrawNode(myGreenPen, mXcentre, mYcentre)
                Form1.DrawSignal(myRedPen, nodeIndex)
                signalState = 0
                ' Form1.myGraphics.DrawLine,xCentrepen:=myweePen, x1:=xCentre, y1:=yCentre, x2:=xCentre, y2:=yCentre - (runDirection * 8))
                'Form1.myGraphics.DrawLine(pen:=myweePen, x1:=xCentre, y1:=yCentre - (runDirection * 8), x2:=xCentre + (runDirection * 11), y2:=yCentre - (runDirection * 8))
                myRectangle = New Rectangle(sigXCentre, sigYCentre, 10, 5)
                labelRectangle = New Rectangle(labelXpos, labelYpos, 20, 10)
                If p2NodeXpos > 0 Then
                    'we need to do something else like a TRTS or Auto indicator
                    If auto = "  " Then
                        'It's an auto button
                    Else
                        'it's a TRTS indicator
                        Form1.DrawTRTS(nodeIndex, False)

                    End If
                   

                End If

        End Select
    End Sub


    Public Overloads Function ArrayListToString(ByVal ar As System.Collections.ArrayList) As String
        Return ArrayListToString(ar, "")
    End Function

    Public Overloads Function ArrayListToString(ByVal ar As System.Collections.ArrayList, ByVal delim As Char) As String
        Return ArrayListToString(ar, delim.ToString)
    End Function

    Public Overloads Function ArrayListToString(ByVal ar As System.Collections.ArrayList, ByVal delim As String) As String
        Return String.Join(delim, CType(ar.ToArray(GetType(String)), String()))
    End Function

    Public Sub LockPoint()
        Console.WriteLine("Point lock " & nodeId)
        pointLocked = True
    End Sub
    Public Sub UnLockPoint()
        Console.WriteLine("Point unlock " & nodeId)

        pointLocked = False
    End Sub




    Public Sub SetPointType()
        Dim PointTypeArraylist As ArrayList
        PointTypeArraylist = New ArrayList

        GetPointNeighbours()
        GetPointToNeighbours()
        For Each neighbour In neighboursArrayList
            Dim thisPoint As Point
            thisPoint = GetNodeCoords(neighbour)
            If mXcentre < thisPoint.X Then
                'going from left to right
                If mYcentre > thisPoint.Y Then
                    PointTypeArraylist.Add("2")
                End If
                If mYcentre = thisPoint.Y Then
                    PointTypeArraylist.Add("3")
                End If
                If mYcentre < thisPoint.Y Then
                    PointTypeArraylist.Add("4")
                End If
            ElseIf mXcentre = thisPoint.X Then
                'going vertical
                If mYcentre > thisPoint.Y Then
                    PointTypeArraylist.Add("C")
                End If
                If mYcentre = thisPoint.Y Then
                    'This shouldn't happen as the point coords are the same
                End If
                If mYcentre < thisPoint.Y Then
                    PointTypeArraylist.Add("6")
                End If
            ElseIf mXcentre > thisPoint.X Then
                'going right to left
                If mYcentre > thisPoint.Y Then
                    PointTypeArraylist.Add("A")
                End If
                If mYcentre = thisPoint.Y Then
                    PointTypeArraylist.Add("9")
                End If
                If mYcentre < thisPoint.Y Then
                    PointTypeArraylist.Add("8")
                End If
            End If
        Next
        PointTypeArraylist.Sort()
        pointType = ArrayListToString(PointTypeArraylist)
    End Sub

    Public Sub BlankPoint(ByVal thePen As Pen)
        'If thePen.Color = Color.Black Then
        'Console.WriteLine("blank nodeId is " & nodeId & " index is " & nodeIndex & " Black")
        'Else
        'Console.WriteLine("blank nodeId is " & nodeId & " index is " & nodeIndex & " White")
        'End If
        If pointSetReverse Then
            If pointLocked Then
                BlankPointSet(thePen)
            Else
                BlankPointReverse(thePen)
            End If
            'point is set
        Else
            If pointLocked Then
                BlankPointReverse(thePen)
            Else
                BlankPointSet(thePen)
            End If

        End If
    End Sub

    Public Sub BlankPointSet(ByVal thePen As Pen)
        'Console.WriteLine("nodeId is " & nodeId & " index is " & nodeIndex)

        Select Case pointType
            'For Points
            Case "239"
                Form1.DrawPointBlank(mXcentre - 1, mYcentre - 4, mXcentre + 9, mYcentre - 4, thePen)
                If pointLocked = False Then
                    Form1.DrawPointClear(mXcentre - 1, mYcentre - 0, mXcentre + 10, mYcentre)
                End If
            Case "349"
                Form1.DrawPointBlank(mXcentre - 10, mYcentre + 3, mXcentre + 10, mYcentre + 3, thePen)
                If pointLocked = False Then
                    Form1.DrawPointClear(mXcentre - 1, mYcentre - 0, mXcentre + 10, mYcentre - 0)
                End If
            Case "389"
                Form1.DrawPointBlank(mXcentre - 10, mYcentre + 3, mXcentre + 10, mYcentre + 3, thePen)
                If pointLocked = False Then
                    Form1.DrawPointClear(mXcentre - 10, mYcentre, mXcentre + 10, mYcentre)
                End If

            Case "39A"
                Form1.DrawPointBlank(mXcentre - 10, mYcentre - 4, mXcentre + 10, mYcentre - 4, thePen)
                If pointLocked = False Then
                    Form1.DrawPointClear(mXcentre - 10, mYcentre - 0, mXcentre + 10, mYcentre - 0)
                End If
                'For Crossovers

            Case "2389"
                Form1.DrawPointBlank(mXcentre - 1, mYcentre - 3, mXcentre + 9, mYcentre - 3, thePen)
                Form1.DrawPointBlank(mXcentre - 10, mYcentre + 3, mXcentre + 10, mYcentre + 3, thePen)

        End Select
    End Sub


    Private Sub BlankPointReverse(ByVal thePen As Pen)
        'Console.WriteLine("nodeId is " & nodeId & " index is " & nodeIndex)

        Select Case pointType
            'For points
            Case "239"
                Form1.DrawPointBlank(mXcentre + 2, mYcentre + 3, mXcentre + 13, mYcentre - 8, thePen)
            Case "349"
                Form1.DrawPointBlank(mXcentre, mYcentre - 5, mXcentre + 11, mYcentre + 6, thePen)
            Case "389"
                Form1.DrawPointBlank(mXcentre - 10, mYcentre + 5, mXcentre - 1, mYcentre - 3, thePen)
            Case "39A"
                Form1.DrawPointBlank(mXcentre - 10, mYcentre - 5, mXcentre - 2, mYcentre + 2, thePen)
                'for crossovers
            Case "2389"
                Form1.DrawPointBlank(mXcentre + 2, mYcentre + 3, mXcentre + 13, mYcentre - 8, thePen)
                Form1.DrawPointBlank(mXcentre - 10, mYcentre + 5, mXcentre - 1, mYcentre - 1, thePen)



        End Select
    End Sub


    Public Sub ReversePoint()
        If nodeType = "P" Then
            pointSetReverse = False
            'Console.WriteLine("Reverse point " & nodeId & " type " & pointType)
            Form1.LeverList(lever) = 1
            'Form1.SetLever(lever, nodeId, False)

            BlankPointReverse(myBlackPen)
        End If
        If nodeType = "X" Then
            pointSetReverse = False
            Console.WriteLine("Reverse Crossover " & nodeId & " type " & pointType)
            Form1.LeverList(lever) = 1
            'Form1.SetLever(lever, nodeId, False)

            BlankPointReverse(myBlackPen)
        End If
    End Sub

    Public Sub SetPoint()
        If nodeType = "P" Then
            pointSetReverse = True
            'Console.WriteLine("Set point " & nodeId & " type " & pointType)
            Form1.LeverList(lever) = 0
            'Form1.SetLever(lever, nodeId, True)
            BlankPointSet(myBlackPen)


        End If
        If nodeType = "X" Then
            pointSetReverse = True
            Console.WriteLine("Set crossover " & nodeId & " type " & pointType)
            Form1.LeverList(lever) = 0
            'Form1.SetLever(lever, nodeId, True)

            BlankPointSet(myBlackPen)
        End If
    End Sub

    Public Sub UpdateState(ByVal setMode As Integer, ByVal setState As Integer)
        'Console.WriteLine("set  " & nodeId & " setstate " & setState & " signalstate " & signalState)

        If setMode = 1 Then
            'immediate set value
            Select Case setState
                Case 0
                    myPen.Color = Color.Red
                    signalState = 0
                Case 1
                    If subsidSignalSet Then
                        myPen.Color = Color.White
                        signalState = 0
                    Else
                        myPen.Color = Color.Yellow
                        signalState = 1
                    End If


                Case 2
                    myPen.Color = Color.Orange
                    signalState = 2
                Case 3 To 5
                    myPen.Color = Color.LawnGreen
                    signalState = 3

            End Select
        Else
            Select Case setState

                Case 0
                    'set to red
                    myPen.Color = Color.Red
                    signalState = 0
                Case 1
                    Select Case signalState
                        Case 1
                            myPen.Color = Color.Red
                            signalState = 0

                        Case 2
                            myPen.Color = Color.Yellow
                            signalState = 1
                        Case 3
                            myPen.Color = Color.Orange
                            signalState = 2


                    End Select
                Case 2
                    Select Case signalState
                        Case 0
                            myPen.Color = Color.Yellow
                            signalState = 1

                        Case 1
                            myPen.Color = Color.Orange
                            signalState = 2
                        Case 2
                            myPen.Color = Color.LawnGreen
                            signalState = 3


                    End Select
            End Select
        End If

        Form1.DrawSignal(myPen, nodeId)

    End Sub


    Public Function IsCollision(ByVal pPoint As Point) As Boolean

        Dim lrectCursorrect As Rectangle

        lrectCursorrect = New Rectangle(pPoint, New Size(2, 2))

        If (myRectangle.IntersectsWith(lrectCursorrect)) Then
            Form1.TextBox3.Text = nodeId
            Return True
        ElseIf (labelRectangle.IntersectsWith(lrectCursorrect)) Then
            Form1.TextBox3.Text = nodeId
            Return True
        Else
            'Form1.TextBox3.Text = ""

            Return False
        End If


    End Function

    Private Sub DrawCircle(ByVal cp As Point, ByVal radius As Integer)
        Dim gr As Graphics
        ' gr = Panel1.CreateGraphics
        Dim rect As Rectangle = New Rectangle(cp.X - radius, cp.Y - radius, 2 * radius, 2 * radius)
        gr.DrawEllipse(Pens.Black, rect)

    End Sub


End Class
