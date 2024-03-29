﻿
Imports Xabe.FFmpeg
Imports System.Threading
Imports System.IO
Imports System.Drawing.Imaging
Imports Microsoft.VisualBasic.ApplicationServices

Public Class Form1

    Dim MediaList As New List(Of MediaFileStreamInfo)
    Private Shared MediaListlockObject As New Object()

    Dim MediaFileList As New List(Of String)

    Dim ScanCount As Integer = 0
    Private Shared ScanCountlockObject As New Object()
    Dim ThreadComplete As Integer = 0
    Dim threadcount As Integer = 4
    Dim ScanFunctionThread As New Thread(AddressOf ScanFunction)

    'Settings Registry.
    ReadOnly SettingsRegKey As String = "SOFTWARE\MRHSYSTEMS\AudioStreamCleanup"

    'Page properties.
    Dim ItemHeightCounter As Integer
    Dim RowCount As Integer = 20
    Dim PageCount As Integer
    Dim MaxPageCount As Integer

    'GUI Controls.
    Dim TextBoxList As New List(Of TextBox)
    Dim DefaultAudioStream As New List(Of TextBox)
    Dim ComboBoxList As New List(Of ComboBox)
    Dim CheckBoxList As New List(Of CheckBox)


    Private Sub btnScan_Click(sender As Object, e As EventArgs) Handles btnScan.Click
        ScanFolder()
    End Sub

    Sub ResetControlLists()
        'Reset properties.
        ItemHeightCounter = 15 'Set Item height start
        MaxPageCount = 0
        PageCount = 1
        MediaList.Clear()
        TextBoxList.Clear()
        DefaultAudioStream.Clear()
        ComboBoxList.Clear()
        CheckBoxList.Clear()

        gbxMedia.Controls.Clear()
        lblPage.Text = "Page: 0/0"

    End Sub

    Sub ScanFolder()
        btnScan.Enabled = False
        ResetControlLists()

        If Not txtScanLocation.Text.EndsWith("\") Then
            txtScanLocation.Text = txtScanLocation.Text & "\"
        End If

        ScanFunctionThread = New Thread(AddressOf ScanFunction)
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
        MediaFileList.Clear()
        ScanCount = 0
        ThreadComplete = 0

        Dim Location As String = txtScanLocation.Text

        InvokeControl(lblStatus, Sub(x) x.Text = "Status: Indexing files")
        Dim lastfile As String = ""
        Try

            For Each MediaFile In My.Computer.FileSystem.GetFiles(Location, FileIO.SearchOption.SearchAllSubDirectories, {"*.avi", "*.mp4", "*.mkv"})
                lastfile = MediaFile
                'Skip / Exclude QNAP Thumbs files.
                If MediaFile.Contains(".@__thumb") Then
                    Continue For
                End If
                MediaFileList.Add(MediaFile)
            Next

        Catch ex As Exception
            MsgBox("Failed to get file index on folder: " & Location & Environment.NewLine & "Last file checked:" & lastfile & Environment.NewLine & "Recommend checking for inaccessible directoriese / corrupt file system." & Environment.NewLine & "Exception: " & ex.ToString, MsgBoxStyle.Exclamation)
            Exit Sub
        End Try

        'Get Info from FFMPEG



        'Make list of files 


        Dim itemsPerThread As Integer = MediaFileList.Count \ threadcount
        Dim remainingItems As Integer = MediaFileList.Count Mod threadcount
        'Slit up file list by thread count, assign ranges to threads.
        For i As Integer = 0 To threadcount - 1
            Dim startIndex As Integer = i * itemsPerThread
            Dim endIndex As Integer = If(i < threadcount - 1, startIndex + itemsPerThread - 1, startIndex + itemsPerThread + remainingItems - 1)
            Dim t As New Thread(Sub() CheckFileInfoRange(startIndex, endIndex))
            t.Start()
        Next

        ' Wait for all threads to finish
        Do Until ThreadComplete = threadcount
            Threading.Thread.Sleep(100)
        Loop

        InvokeControl(lblStatus, Sub(x) x.Text = "Status: Loading GUI controls.")

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

            'Display default audio stream.
            Dim DATxtBox As New TextBox
            DATxtBox.Location = New Point(TxtBox.Location.X + TxtBox.Width + 10, ItemHeightCounter)
            DATxtBox.ReadOnly = True
            DATxtBox.Width = 250
            DATxtBox.Text = MFile.AudioStreams(0).SummaryName
            DefaultAudioStream.Add(DATxtBox)

            Dim ComBox As New ComboBox
            ComBox.Location = New Point(DATxtBox.Location.X + DATxtBox.Width + 10, ItemHeightCounter)
            ComBox.Width = 250
            'Add audio streams to combo box.
            For Each AStream In MFile.AudioStreams
                ComBox.Items.Add(AStream.SummaryName)
            Next

            ComBox.SelectedIndex = 0 'Set Default audio track if none auto-matched.


            'Match to first English language if present
            Dim count As Integer = -1
            Dim channels As Integer = 0
            Dim bitrate As Integer = 0


            For Each AStream In MFile.AudioStreams
                count += 1
                If Not String.IsNullOrEmpty(AStream.Language) Then
                    If Not AStream.Language.ToString().ToLower = "eng" Then
                        Continue For 'skip non english audio.
                    End If
                End If

                If AStream.Bitrate = 0 And Not AStream.Codec.ToLower.Contains("aac") And Not AStream.Codec.ToLower.Contains("ac3") Then
                    'Matched to bitstream protocol - DTS / TrueHD
                    ComBox.SelectedIndex = count
                    Exit For

                    'Preference by highest channel count
                ElseIf AStream.Channels > channels Then
                    channels = AStream.Channels
                    bitrate = AStream.Bitrate
                    ComBox.SelectedIndex = count
                    Continue For 'skip remaining checks, start comparison of audio stream.

                    'if channel count matches, check for highest bitrate.
                ElseIf AStream.Channels = channels Then
                    If AStream.Bitrate = 0 Then
                        'Lossy - highest priority if channels match.
                        ComBox.SelectedIndex = count
                    ElseIf bitrate = 0 Then
                        'existing track is lossy, skip compare.
                        Continue For
                    ElseIf AStream.Bitrate > bitrate Then
                        'bitrate is higher.
                        bitrate = AStream.Bitrate
                        ComBox.SelectedIndex = count
                    End If

                End If
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

    Async Sub CheckFileInfoRange(ByVal startIndex As Integer, ByVal endIndex As Integer)
        For i As Integer = startIndex To endIndex
            Try
                SyncLock ScanCountlockObject
                    ScanCount += 1 'threadsafe increment
                    InvokeControl(lblStatus, Sub(x) x.Text = "Status: Scanning (" & ScanCount & "/" & MediaFileList.Count & ")")
                End SyncLock

                If Not My.Computer.FileSystem.FileExists(MediaFileList(i)) Then
                    'File Deleted since initial scan, skip.
                    Continue For
                End If
                Dim MediaInfo As IMediaInfo = Await FFmpeg.GetMediaInfo(MediaFileList(i))

                If MediaInfo.AudioStreams.Count > 1 Then
                    Dim tmpMFSI As New MediaFileStreamInfo
                    tmpMFSI.filename = MediaFileList(i)
                    For Each AStream In MediaInfo.AudioStreams
                        Dim tmpASInfo As New MediaFileStreamInfo.AudioStreamInfo
                        tmpASInfo.Index = AStream.Index
                        tmpASInfo.Codec = AStream.Codec
                        tmpASInfo.Bitrate = AStream.Bitrate
                        tmpASInfo.Channels = AStream.Channels
                        tmpASInfo.Language = AStream.Language
                        tmpMFSI.AudioStreams.Add(tmpASInfo)
                    Next
                    tmpMFSI.index = MediaList.Count + 1
                    SyncLock MediaListlockObject
                        MediaList.Add(tmpMFSI)
                    End SyncLock
                End If
            Catch ex As Exception
                'FFMPEG Unable to probe file for information.
                'Exception has occured on older versions of ffmpeg.
                If MsgBox("Unable to check stream information from:" & MediaFileList(i) & Environment.NewLine & "Recommend updating FFMPEG" & " Retry?", MsgBoxStyle.Critical + MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                    Continue For
                End If
            End Try
        Next

        Interlocked.Increment(ThreadComplete) 'thread completed tracking.

    End Sub



    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not My.Computer.FileSystem.FileExists("C:\ffmpeg\bin\ffmpeg.exe") Then
            MsgBox("Copy FFMPEG for Windows to C:\ffmpeg", MsgBoxStyle.Exclamation)
        End If

        My.Computer.Registry.CurrentUser.CreateSubKey(SettingsRegKey)
        Dim LastPath As String = My.Computer.Registry.CurrentUser.OpenSubKey(SettingsRegKey, True).GetValue("ScanPath", "C:\Movies\")
        txtScanLocation.Text = LastPath

        FFmpeg.SetExecutablesPath("C:\ffmpeg\bin")
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
            AddControlFromThread()
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


    Private Async Sub btnProcess_Click(sender As Object, e As EventArgs) Handles btnProcess.Click
        btnProcess.Enabled = False
        Dim ProcessIndexList As New List(Of Integer)
        For Each Item In CheckBoxList
            If Item.Checked = True Then
                ProcessIndexList.Add(Integer.Parse(Item.Text) - 1)
            End If
        Next

        'Process List.
        Dim ShowFFMPEG As Boolean = False
        If MsgBox("Show FFMPEG Output?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            ShowFFMPEG = True
        End If

        Dim Counter As Integer = 0
        For Each IndexInt In ProcessIndexList
            Counter += 1
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

            'clear any previous tmpfile.
            If My.Computer.FileSystem.FileExists(tmpfilename) Then
                My.Computer.FileSystem.DeleteFile(tmpfilename)
            End If
            'test write access
            Do Until My.Computer.FileSystem.FileExists(tmpfilename)
                Try
                    Dim TestWriteFile As New StreamWriter(tmpfilename)
                    TestWriteFile.WriteLine("test")
                    TestWriteFile.Close()
                Catch ex As Exception
                    MsgBox("Write access test to directory failed - check write access and click OK to retry." & tmpfilename, MsgBoxStyle.OkOnly)
                End Try
            Loop
            My.Computer.FileSystem.DeleteFile(tmpfilename)



            lblStatus.Text = "Status: Processing (" & Counter & "/" & ProcessIndexList.Count & ") " & MediaList(IndexInt).filename

            'Generate new file
            'ffmpeg -i <videofile> -map 0:0 -c:v copy -map 0:4 -c:a copy -c:s copy <videofileoutput>
            Dim ConvertProcess As New Process
            ConvertProcess.StartInfo.FileName = "C:\ffmpeg\bin\ffmpeg.exe"
            ConvertProcess.StartInfo.Arguments = "-i " & Chr(34) & MediaList(IndexInt).filename & Chr(34) & " -map 0:0 -c:v copy -map 0:" & MediaList(IndexInt).AudioStreams(AudioStreamIndex).Index & " -c:a copy -c:s copy " & Chr(34) & tmpfilename & Chr(34)
            ConvertProcess.StartInfo.WorkingDirectory = "C:\ffmpeg\bin"

            If ShowFFMPEG = False Then
                ConvertProcess.StartInfo.UseShellExecute = False
                ConvertProcess.StartInfo.CreateNoWindow = True
            End If

            'ConvertProcess.StartInfo.RedirectStandardOutput = True
            'ConvertProcess.StartInfo.RedirectStandardError = True
            'AddHandler ConvertProcess.ErrorDataReceived, AddressOf ErrorInfo
            'AddHandler ConvertProcess.OutputDataReceived, AddressOf OutputInfo

            ErrorOutput = ""
            StandardOutput = ""

            ConvertProcess.Start()
            ConvertProcess.WaitForExit()

            'Validate file, check audio codec.
            If Not My.Computer.FileSystem.FileExists(tmpfilename) Then
                MsgBox("Conversion failure of:" & MediaList(IndexInt).filename & Environment.NewLine & "Executed: ffmpeg " & ConvertProcess.StartInfo.Arguments, MsgBoxStyle.Critical)
                Continue For
            End If

            Dim ValidateFileInfo As IMediaInfo = Await FFmpeg.GetMediaInfo(tmpfilename)

            'Basic validation (audio codec readable and matching).
            If ValidateFileInfo.AudioStreams(0).Codec = MediaList(IndexInt).AudioStreams(AudioStreamIndex).Codec Then
                'rename existing file to .old ext
                My.Computer.FileSystem.MoveFile(MediaList(IndexInt).filename, MediaList(IndexInt).filename & ".old")
                My.Computer.FileSystem.MoveFile(tmpfilename, MediaList(IndexInt).filename)
                Dim oldfile As String = MediaList(IndexInt).filename & ".old"
                While My.Computer.FileSystem.FileExists(oldfile)
                    Try
                        'clear read only flag
                        System.IO.File.SetAttributes(oldfile, IO.FileAttributes.Normal)
                    Catch ex As Exception

                    End Try

                    'delete file
                    Try
                        My.Computer.FileSystem.DeleteFile(oldfile)
                    Catch ex As Exception
                        MsgBox("Error deleting " & oldfile & Environment.NewLine & "Click ok to retry.", MsgBoxStyle.Critical)
                    End Try
                End While
            End If
        Next

        'Check Media INFO on file for validation.


        'Delete old file and rename new file.
        lblStatus.Text = "Status: Completed"

        'Reset gui controls.
        ResetControlLists()
        InvokeControl(gbxMedia, Sub(x) x.Controls.Clear())
        Me.Invoke(New MethodInvoker(AddressOf Me.UpdatePageLabel))

        btnProcess.Enabled = True
    End Sub

    Private Sub txtScanLocation_TextChanged(sender As Object, e As EventArgs) Handles txtScanLocation.TextChanged
        Dim SettingsKey As Microsoft.Win32.RegistryKey = My.Computer.Registry.CurrentUser.OpenSubKey(SettingsRegKey, True)
        If Not SettingsKey Is Nothing Then
            My.Computer.Registry.CurrentUser.OpenSubKey(SettingsRegKey, True).SetValue("ScanPath", txtScanLocation.Text)
        End If
    End Sub

    Private Sub gbxMedia_Enter(sender As Object, e As EventArgs) Handles gbxMedia.Enter

    End Sub

    Private Sub txtThreads_TextChanged(sender As Object, e As EventArgs) Handles txtThreads.TextChanged
        threadcount = Integer.Parse(txtThreads.Text)
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If ScanFunctionThread.IsAlive Then
            ScanFunctionThread.Abort()
        End If

    End Sub
End Class


Public Class MediaFileStreamInfo
    Property filename As String
    Property index As Integer
    Property AudioStreams As New List(Of AudioStreamInfo)
    Class AudioStreamInfo
        Property Index As Integer
        Property Codec As String
        Property Channels As Integer
        Property Bitrate As Long
        Property Language As String

        Function SummaryName() As String
            Dim DisplayName As String = Index & ": " & Language & " - " & Codec & " - Channels:" & Channels
            If Not Bitrate = 0 Then
                DisplayName = DisplayName & " - Bitrate:" & Bitrate
            End If
            Return DisplayName
        End Function
    End Class
End Class


