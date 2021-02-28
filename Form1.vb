
Imports Xabe.FFmpeg
Imports System.Threading

Public Class Form1
    Dim FileName As String
    Dim ItemHeightCounter As Integer = 40

    Private Sub btnScan_Click(sender As Object, e As EventArgs) Handles btnScan.Click
        If Not txtScanLocation.Text.EndsWith("\") Then
            txtScanLocation.Text = txtScanLocation.Text & "\"
        End If
        Dim ScanFunctionThread As New Thread(AddressOf ScanFunction)
        ScanFunctionThread.Start()


    End Sub


    Sub AddControlFromThread()

        For Each TXBX In TextBoxList
            Me.Controls.Add(TXBX)
        Next

        For Each TXBX In DefaultAudioStream
            Me.Controls.Add(TXBX)
        Next

        For Each CMBX In ComboBoxList
            Me.Controls.Add(CMBX)
        Next
    End Sub


    Dim TextBoxList As New List(Of TextBox)
    Dim DefaultAudioStream As New List(Of TextBox)
    Dim ComboBoxList As New List(Of ComboBox)
    Private Sub ScanFunction()



        Dim MediaFileList As New List(Of String)
        Dim Location As String = "G:\Videos\Movies\A\"

        InvokeControl(lblStatus, Sub(x) x.Text = "Status: Scanning Files")
        For Each MediaFile In My.Computer.FileSystem.GetFiles(Location, FileIO.SearchOption.SearchAllSubDirectories, "*.mkv")
            MediaFileList.Add(MediaFile)
        Next

        For Each MediaFile In My.Computer.FileSystem.GetFiles(Location, FileIO.SearchOption.SearchAllSubDirectories, "*.mp4")
            MediaFileList.Add(MediaFile)
        Next


        For Each MediaFile In My.Computer.FileSystem.GetFiles(Location, FileIO.SearchOption.SearchAllSubDirectories, "*.avi")
            MediaFileList.Add(MediaFile)
        Next


        InvokeControl(lblStatus, Sub(x) x.Text = "Status: Scanning Media Info")


        FFmpeg.SetExecutablesPath("C:\ffmpeg\bin")


        For Each MediaFile In MediaFileList

            FileName = MediaFile

            WaitForTaskComplete = False
            Dim GetFileInfoTask As New Task(AddressOf GetFileInfo)
            GetFileInfoTask.Start()

            InvokeControl(lblStatus, Sub(x) x.Text = "Status: Scanning " & FileName)
            Do Until WaitForTaskComplete = True
                Threading.Thread.Sleep(100)
            Loop


        Next

        'Generate Combobox for each media file for user to select primary audio track.
        For Each MFile In MediaList
            Dim TxtBox As New TextBox
            TxtBox.Location = New Point(10, ItemHeightCounter)
            TxtBox.ReadOnly = True
            TxtBox.Width = 600
            TxtBox.Text = MFile.filename.Substring(Location.Length)
            TextBoxList.Add(TxtBox)

            Dim DATxtBox As New TextBox
            DATxtBox.Location = New Point(TxtBox.Location.X + TxtBox.Width + 10, ItemHeightCounter)
            DATxtBox.ReadOnly = True
            DATxtBox.Width = 250
            DATxtBox.Text = GetAudioStreamDisplayname(MFile.AudioStreams(0))
            DefaultAudioStream.Add(DATxtBox)

            Dim ComBox As New ComboBox
            ComBox.Location = New Point(DATxtBox.Location.X + DATxtBox.Width + 10, ItemHeightCounter)
            ComBox.Width = 250

            For Each AStream In MFile.AudioStreams
                ComBox.Items.Add(GetAudioStreamDisplayname(AStream))
            Next

            ComBox.SelectedIndex = 0
            'Match to first English language if present
            Dim count As Integer = 0
            For Each AStream In MFile.AudioStreams
                If AStream.Language = "eng" Then
                    ComBox.SelectedIndex = count
                    Exit For
                End If
                count += 1
            Next

            ComboBoxList.Add(ComBox)

            ItemHeightCounter += 25
        Next

        Me.Invoke(New MethodInvoker(AddressOf Me.AddControlFromThread))

    End Sub

    Function GetAudioStreamDisplayname(ByVal AStream As AudioStream)
        Dim DisplayName As String = AStream.Index & ": " & AStream.Language & " - " & AStream.Codec & " - Channels:" & AStream.Channels
        If Not AStream.Bitrate = 0 Then
            DisplayName = DisplayName & " - Bitrate:" & AStream.Bitrate
        End If


        Return DisplayName
    End Function

    Dim WaitForTaskComplete As Boolean
    Dim MediaList As New List(Of MediaFileStreamInfo)


    Async Sub GetFileInfo()

        'Make list of files 

        Dim MediaInfo As IMediaInfo = Await FFmpeg.GetMediaInfo(FileName)
            If MediaInfo.AudioStreams.Count > 1 Then
                Dim tmpMFSI As New MediaFileStreamInfo
                tmpMFSI.filename = FileName
                For Each AStream In MediaInfo.AudioStreams
                    tmpMFSI.AudioStreams.Add(AStream)
                Next
                MediaList.Add(tmpMFSI)
            End If


            WaitForTaskComplete = True

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load


    End Sub

    Public Sub InvokeControl(Of T As Control)(ByVal Control As T, ByVal Action As Action(Of T))
        If Control.InvokeRequired Then
            Try
                Control.Invoke(New Action(Of T, Action(Of T))(AddressOf InvokeControl), New Object() {Control, Action})
            Catch ex As Exception
            End Try
        Else
            Action(Control)
        End If
    End Sub


End Class


Public Class MediaFileStreamInfo
    Public filename As String
    Public AudioStreams As New List(Of IAudioStream)
End Class


