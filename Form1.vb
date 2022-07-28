
Imports Xabe.FFmpeg
Imports System.Threading

Public Class Form1
    Dim FileName As String
    Dim ItemHeightCounter As Integer
    Dim RowCount As Integer = 20
    Dim PageCount As Integer
    Dim MaxPageCount As Integer
    Dim WaitForTaskComplete As Boolean
    Dim MediaList As List(Of MediaFileStreamInfo)

    Dim TextBoxList As List(Of TextBox)
    Dim DefaultAudioStream As List(Of TextBox)
    Dim ComboBoxList As List(Of ComboBox)
    Dim CheckBoxList As List(Of CheckBox)


    Private Sub btnScan_Click(sender As Object, e As EventArgs) Handles btnScan.Click
        btnScan.Enabled = False
        ItemHeightCounter = 15 'Set Item height start


        MaxPageCount = 0
        PageCount = 1
        MediaList = New List(Of MediaFileStreamInfo)

        TextBoxList = New List(Of TextBox)
        DefaultAudioStream = New List(Of TextBox)
        ComboBoxList = New List(Of ComboBox)
        CheckBoxList = New List(Of CheckBox)


        If Not txtScanLocation.Text.EndsWith("\") Then
            txtScanLocation.Text = txtScanLocation.Text & "\"
        End If

        Dim ScanFunctionThread As New Thread(AddressOf ScanFunction)
        ScanFunctionThread.Start()


    End Sub



    Sub AddControlFromThread()

        'Calculate Range
        Dim ControlCounter As Integer = (PageCount * RowCount) - RowCount 'Start Range.

        Dim CounterLimit As Integer = ControlCounter + RowCount
        If CounterLimit > MediaList.Count Then
            CounterLimit = MediaList.Count 'max limit.
        End If

        gbxMedia.Controls.Clear()

        Do Until ControlCounter = CounterLimit
            gbxMedia.Controls.Add(TextBoxList(ControlCounter))
            gbxMedia.Controls.Add(DefaultAudioStream(ControlCounter))
            gbxMedia.Controls.Add(ComboBoxList(ControlCounter))
            gbxMedia.Controls.Add(CheckBoxList(ControlCounter))
            ControlCounter += 1
        Loop

    End Sub




    Private Sub ScanFunction()



        Dim MediaFileList As New List(Of String)
        Dim Location As String = txtScanLocation.Text

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

        InvokeControl(lblStatus, Sub(x) x.Text = "Status: Loading controls.")

        'Generate Combobox for each media file for user to select primary audio track.

        Dim InitialHeight As Integer = ItemHeightCounter
        Dim Counter As Integer = 0
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

            ComBox.SelectedIndex = 0 'Set Default audio track if none auto-matched.


            'Match to first English language if present
            Dim count As Integer = 0

            Dim channels As Integer = 0
            Dim bitrate As Integer = 0


            For Each AStream In MFile.AudioStreams
                If AStream.Language = "eng" Then
                    If AStream.Bitrate = 0 Then
                        'Matched to bitstream protocol - DTS / TrueHD
                        ComBox.SelectedIndex = count
                        Exit For
                    End If

                    'if channel count already matches, use highest bitrate.
                    If AStream.Channels = channels Then
                        If AStream.Bitrate > bitrate Then
                            bitrate = AStream.Bitrate
                            ComBox.SelectedIndex = count
                        End If
                    End If


                    'Order by highest channel count
                    If AStream.Channels > channels Then
                        channels = AStream.Channels
                        bitrate = AStream.Bitrate
                        ComBox.SelectedIndex = count
                    End If

                End If


                count += 1
            Next

            ComboBoxList.Add(ComBox)

            Dim CheckBox As New CheckBox
            CheckBox.Name = "chkbox" & MFile.index
            CheckBox.Text = MFile.index
            CheckBox.Location = New Point(ComBox.Location.X + ComBox.Width + 10, ItemHeightCounter)
            CheckBoxList.Add(CheckBox)

            'Increase Line Height
            ItemHeightCounter += 25


            'Reset when row count reached.
            Counter += 1
            If Counter = RowCount Then
                ItemHeightCounter = InitialHeight
                Counter = 0
            End If


        Next

        'Calculate how many pages.
        Dim Remain As Integer
        Dim Dividevalue As Integer = Math.DivRem(MediaList.Count, RowCount, Remain)
        If Not Remain = 0 Then
            Dividevalue += 1
        End If

        MaxPageCount = Dividevalue
        Me.Invoke(New MethodInvoker(AddressOf Me.UpdatePageLabel))

        Me.Invoke(New MethodInvoker(AddressOf Me.AddControlFromThread))

        InvokeControl(lblStatus, Sub(x) x.Text = "Status: Scanning Complete")
        InvokeControl(btnScan, Sub(x) x.Enabled = True)
    End Sub






    Function GetAudioStreamDisplayname(ByVal AStream As AudioStream)
        Dim DisplayName As String = AStream.Index & ": " & AStream.Language & " - " & AStream.Codec & " - Channels:" & AStream.Channels
        If Not AStream.Bitrate = 0 Then
            DisplayName = DisplayName & " - Bitrate:" & AStream.Bitrate
        End If


        Return DisplayName
    End Function




    Async Sub GetFileInfo()

        'Make list of files 

        Dim MediaInfo As IMediaInfo = Await FFmpeg.GetMediaInfo(FileName)
        If MediaInfo.AudioStreams.Count > 1 Then
            Dim tmpMFSI As New MediaFileStreamInfo
            tmpMFSI.filename = FileName
            For Each AStream In MediaInfo.AudioStreams
                tmpMFSI.AudioStreams.Add(AStream)
            Next
            tmpMFSI.index = MediaList.Count + 1
            MediaList.Add(tmpMFSI)
        End If


        WaitForTaskComplete = True

    End Sub

    Dim ValidateFileInfo As IMediaInfo
    Dim ValidateTaskComplete As Boolean = False
    Async Sub ValidateFile()
        'Make list of files 
        ValidateFileInfo = Await FFmpeg.GetMediaInfo(FileName)
        ValidateTaskComplete = True

    End Sub




    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not My.Computer.FileSystem.FileExists("C:\ffmpeg\bin\ffmpeg.exe") Then
            MsgBox("Copy FFMPEG for Windows to C:\ffmpeg", MsgBoxStyle.Exclamation)
        End If
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

    Private Sub btnCheckNone_Click(sender As Object, e As EventArgs) Handles btnCheckNone.Click
        For Each chkbox In CheckBoxList
            chkbox.Checked = False
        Next
    End Sub

    Private Sub btnCheckAll_Click(sender As Object, e As EventArgs) Handles btnCheckAll.Click
        For Each chkbox In CheckBoxList
            chkbox.Checked = True
        Next
    End Sub

    Private Sub btnNextPage_Click(sender As Object, e As EventArgs) Handles btnNextPage.Click
        If PageCount < MaxPageCount Then
            PageCount += 1
            UpdatePageLabel()
            AddControlFromThread
        End If

    End Sub

    Private Sub btnPreviousPage_Click(sender As Object, e As EventArgs) Handles btnPreviousPage.Click
        If PageCount > 1 Then
            PageCount -= 1
            UpdatePageLabel()
            AddControlFromThread()
        End If
    End Sub

    Private Sub UpdatePageLabel()
        lblPage.Text = "Page: " & PageCount & "/" & MaxPageCount
    End Sub

    Dim ErrorOutput As String
    Dim StandardOutput As String

    Private Sub ErrorInfo(ByVal sender As Object, ByVal e As DataReceivedEventArgs)
        ErrorOutput = e.Data & Environment.NewLine & ErrorOutput
        MsgBox(e.Data)
    End Sub

    Private Sub OutputInfo(ByVal sender As Object, ByVal e As DataReceivedEventArgs)
        StandardOutput = e.Data & Environment.NewLine & StandardOutput
        MsgBox(e.Data)
    End Sub


    Private Sub btnProcess_Click(sender As Object, e As EventArgs) Handles btnProcess.Click
        btnProcess.Enabled = False
        Dim ProcessIndexList As New List(Of Integer)
        For Each Item In CheckBoxList
            If Item.Checked = True Then
                ProcessIndexList.Add(Integer.Parse(Item.Text) - 1)
            End If
        Next

        'Process List.


        For Each IndexInt In ProcessIndexList

            Dim AudioStreamIndex As Integer = ComboBoxList(IndexInt).SelectedIndex

            Dim tmpfilename As String = ""
            Dim splitext() As String = MediaList(IndexInt).filename.Split(".")
            Dim count As Integer = 0
            For Each splt In splitext
                count += 1
                If count = splitext.Length Then
                    tmpfilename = tmpfilename & "tmp." & splt
                    Exit For
                End If
                tmpfilename = tmpfilename & splt & "."

            Next


            lblStatus.Text = "Status: Processing " & MediaList(IndexInt).filename

            'Generate new file
            'ffmpeg -i <videofile> -map 0:0 -c:v copy -map 0:4 -c:a copy -c:s copy <videofileoutput>
            Dim ConvertProcess As New Process
            ConvertProcess.StartInfo.FileName = "C:\ffmpeg\bin\ffmpeg.exe"
            ConvertProcess.StartInfo.Arguments = "-i " & Chr(34) & MediaList(IndexInt).filename & Chr(34) & " -map 0:0 -c:v copy -map 0:" & MediaList(IndexInt).AudioStreams(AudioStreamIndex).Index & " -c:a copy -c:s copy " & Chr(34) & tmpfilename & Chr(34)
            ConvertProcess.StartInfo.WorkingDirectory = "C:\ffmpeg\bin"

            'ConvertProcess.StartInfo.CreateNoWindow = True
            'ConvertProcess.StartInfo.UseShellExecute = False
            'ConvertProcess.StartInfo.RedirectStandardOutput = True
            'ConvertProcess.StartInfo.RedirectStandardError = True
            'AddHandler ConvertProcess.ErrorDataReceived, AddressOf ErrorInfo
            'AddHandler ConvertProcess.OutputDataReceived, AddressOf OutputInfo

            ErrorOutput = ""
            StandardOutput = ""

            ConvertProcess.Start()

            ConvertProcess.WaitForExit()

            'Validate file, check audio codec.
            FileName = tmpfilename

            ValidateTaskComplete = False
            Dim GetFileInfoTask As New Task(AddressOf ValidateFile)
            GetFileInfoTask.Start()

            Do Until ValidateTaskComplete = True
                Threading.Thread.Sleep(100) 'Wait for check to complete
            Loop

            If ValidateFileInfo.AudioStreams(0).Codec = MediaList(IndexInt).AudioStreams(AudioStreamIndex).Codec Then

                'Basic validation (audio codec readable and matching).
                'rename existing file to .old ext

                My.Computer.FileSystem.MoveFile(MediaList(IndexInt).filename, MediaList(IndexInt).filename & ".old")

                My.Computer.FileSystem.MoveFile(tmpfilename, MediaList(IndexInt).filename)

                Try
                    'clear read only flag
                    System.IO.File.SetAttributes(MediaList(IndexInt).filename & ".old", IO.FileAttributes.Normal)
                    'delete file
                    My.Computer.FileSystem.DeleteFile(MediaList(IndexInt).filename & ".old")
                Catch ex As Exception
                    MsgBox("error delete file:" & MediaList(IndexInt).filename & ".old " & ex.ToString)
                End Try

            End If





        Next

        'Check Media INFO on file for validation.


        'Delete old file and rename new file.
        lblStatus.Text = "Status: Complete"

        btnProcess.Enabled = True
    End Sub


End Class


Public Class MediaFileStreamInfo
    Public filename As String
    Public index As Integer
    Public AudioStreams As New List(Of IAudioStream)
End Class


