
Imports System.Threading
Public Class Class1
    Private trd As Thread
    Private parent As Form1
    Public status As Integer
    Delegate Sub ChangeTrainMonTextDelegate(ByVal aThreadName As String, ByVal setTextVal As String)
    Delegate Sub getStatusDelegate(ByVal aThreadName As Class1, ByVal index As Integer)


    Sub updatetextbox(ByVal settextVal As String)
        parent.Label1.Invoke(New ChangeTrainMonTextDelegate(AddressOf parent.setoutputText), New Object() {"trainref", settextVal})

    End Sub
    Sub getIndexVal(ByVal indexVal As Integer)
        parent.Label1.Invoke(New getStatusDelegate(AddressOf parent.getoutputText), New Object() {Me, indexVal})

    End Sub

    Sub New(ByVal setParent As Form1)
        parent = setParent
        RunRouteThread()
    End Sub

    Public Sub RunRouteThread()
        trd = New Thread(AddressOf ThreadTask1)
        trd.IsBackground = True
        trd.Start()
    End Sub

    Private Sub threadtask1()
        Dim val As Integer = 333
        getIndexVal(1)
        val = val + status
        updatetextbox(val)
    End Sub
End Class
