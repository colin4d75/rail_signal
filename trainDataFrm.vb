Public Class trainDataFrm

    Private Sub Form2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Console.WriteLine("loaded")
    End Sub

    Public Sub SetHeaderText(ByVal newText As String)
        Me.Text = newText
    End Sub
    Public Sub SetText(ByVal arrivalTime As String, _
                       ByVal departureTime As String, _
                       ByVal notes As String, _
                       ByVal platform As String, _
                       ByVal forms As String, _
                       ByVal exform As String, _
                       ByVal lineFrom As String, _
                       ByVal lineTo As String)

        Me.TextBox1.Text = ""
        If arrivalTime <> "-" Then
            Me.TextBox1.Text = Me.TextBox1.Text & "Due " & arrivalTime & Environment.NewLine
        End If
        If departureTime <> "-" Then
            Me.TextBox1.Text = Me.TextBox1.Text & "Depart " & departureTime & Environment.NewLine
        End If
        If forms.Trim().Length > 0 Then
            Me.TextBox1.Text = Me.TextBox1.Text & "Forms " & forms & Environment.NewLine
        End If
        If exform.Trim().Length > 0 Then
            Me.TextBox1.Text = Me.TextBox1.Text & "Formed from " & exform & Environment.NewLine
        End If
        If lineFrom <> "-" Then
            Me.TextBox1.Text = Me.TextBox1.Text & "From " & lineFrom & Environment.NewLine
        End If
        If lineTo <> "-" Then
            Me.TextBox1.Text = Me.TextBox1.Text & "To " & lineTo & Environment.NewLine
        End If

        Me.TextBox1.Text = Me.TextBox1.Text & notes & Environment.NewLine
    End Sub
End Class