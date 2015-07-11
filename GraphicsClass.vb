
Imports System.Threading.Thread

Public Class GraphicsClass

    Dim myGraphics As Graphics
    Dim myPen As Pen
    Dim trackArrayList As New ArrayList()

    Private startx As Integer = 0
    Private starty As Integer = 0
    Private endx As Integer = 10
    Private endy As Integer = 100



    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs)
        ' Handles Form1.Form1.Load



        Form1.DisplayWindow.Image = New Bitmap(5000, 700)

        'return the current form as a drawing surface   

        myGraphics = Graphics.FromImage(Form1.DisplayWindow.Image)

        'instantiate a new pen object using the color structure

        myPen = New Pen(Color:=Color.Blue, Width:=4)

        'Form1.HScrollBar1.Maximum = Form1.PictureBox1.Width





    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        ' Handles Button1.Click

        'local scope

        Dim newTrack As TrackClass

        Dim rnd As New Random()
        Dim endx As Integer = rnd.Next(20, Form1.Width - 40)
        Dim endy As Integer = rnd.Next(20, Form1.Height - 40)


        ' newTrack = New TrackClass(startx, starty, endx, endy, 100.22)

        trackArrayList.Add(newTrack)


        startx = endx

        starty = endy



        'DrawTrack(myPen, 1, 1, 25, 50)



        'draw the line on the form using the pen object

        'myGraphics.DrawLine(pen:=myPen, x1:=1, y1:=1, x2:=25, y2:=50)

        'Dim rnd As New Random()

        'Dim i As Integer

        'For i = 0 To 210

        ' Dim px As Integer = Rnd.Next(20, Me.Width - 40)

        ' Dim py As Integer = Rnd.Next(20, Me.Height - 40)

        ' myGraphics.DrawEllipse(New Pen(Color.FromArgb(Rnd.Next(0, 255), Rnd.Next(0, 255), _

        '     Rnd.Next(0, 255)), 1), px, py, px + Rnd.Next(0, Me.Width - px - 20), _

        '     py + Rnd.Next(0, Me.Height - py - 20))

        ' Next i

        'Refresh()

    End Sub



    Public Sub DrawTrack(ByVal penColor As Pen, _
                         ByVal Xstart As Single, _
                         ByVal Ystart As Single, _
                         ByVal Xend As Single, _
                         ByVal Yend As Single)

        '  myGraphics = Graphics.FromHwnd(hwnd:=Form1)





        'draw the line on the form using the pen object

        myGraphics.DrawLine(pen:=penColor, x1:=Xstart, y1:=Ystart, x2:=Xend, y2:=Yend)

        'Refresh()

    End Sub





    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        For count1 As Integer = 0 To (trackArrayList.Count - 1)

            trackArrayList(count1).routeSet()

            Sleep(2000)

        Next

    End Sub





    Private Sub HScrollBar1_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs)
        'Handles HScrollBar1.Scroll

        '     PictureBox1.Left = -HScrollBar1.Value

        'TextBox1.Text = HScrollBar1.Value

    End Sub





    Private Sub PictureBox1_move(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        ' Handles PictureBox1.MouseMove



        'TextBox2.Text = "X." & e.X & "  Y." & e.Y

    End Sub

End Class


