Public Class Form2old

    Private xloc As Integer
    Private yloc As Integer
    Private tcindex As Integer
    Public TrackCircuitArrayList As New ArrayList()



    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        tcindex = TrackCircuitArrayList.Count
        Dim checkboxn As New tcCheckBox(xloc, yloc, tcindex)
        yloc = yloc + 30
        Controls.Add(checkboxn.checkBoxn)
        TrackCircuitArrayList.Add(checkboxn)
    End Sub


    Private Sub Form2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        xloc = 20
        yloc = 40
    End Sub
End Class

Public Class tcCheckBox

    Public WithEvents checkBoxn As CheckBox
    Private tcIndex As Integer

    Public Sub New(ByVal xloc As Integer, ByVal yloc As Integer, ByVal setIndex As Integer)
        tcIndex = setIndex
        checkBoxn = New CheckBox()
        checkBoxn.Location = New Point(xloc, yloc)
        checkBoxn.Text = "Tc " & (tcIndex)
        checkBoxn.Size = New Size(95, 45)
        yloc = yloc + 30
        Form2old.Controls.Add(checkBoxn)
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, _
ByVal e As System.EventArgs) Handles checkBoxn.CheckStateChanged
        Form2old.TextBox1.Text = "CheckBox Checked"
        Console.WriteLine("Check " & tcIndex)

        Form1.setTrackCircuit(tcIndex, sender.checked, False)


        ' Dim trackArrayIndex As Integer
        'Dim trackStatus As Integer
        'For tcCount As Integer = 0 To (Form1.trackCircuitArray(tcIndex).Count - 1)
        'trackArrayIndex = Form1.trackCircuitArray(tcIndex)(tcCount)
        'trackStatus = Form1.TrackArrayList(trackArrayIndex).GetTrackStatus()
        ' subRouteList.Add(trackID)
        'If checkBoxn.Checked = True Then
        ' Form1.TrackArrayList(trackArrayIndex).TrackOccupied()
        'Else
        'Form1.TrackArrayList(trackArrayIndex).TrackClear()

        'End If
        'If trackStatus > 0 Then
        'subroute is occupied
        ' Form1.TrackArrayList(trackArrayIndex).ClearSetRoute()

        'Console.WriteLine("Track Index " & trackArrayIndex & " is " & trackStatus & "set")
        ' routeIsClear = False
        'End If
        'Next
    End Sub


End Class
