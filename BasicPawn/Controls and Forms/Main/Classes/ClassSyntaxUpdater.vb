﻿'BasicPawn
'Copyright(C) 2018 TheTimocop

'This program Is free software: you can redistribute it And/Or modify
'it under the terms Of the GNU General Public License As published by
'the Free Software Foundation, either version 3 Of the License, Or
'(at your option) any later version.

'This program Is distributed In the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty Of
'MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License For more details.

'You should have received a copy Of the GNU General Public License
'along with this program. If Not, see < http: //www.gnu.org/licenses/>.


Imports System.Text.RegularExpressions

Public Class ClassSyntaxUpdater
    Private g_mFormMain As FormMain
    Private g_mSourceSyntaxUpdaterThread As Threading.Thread

    Public Sub New(f As FormMain)
        g_mFormMain = f
    End Sub

    ''' <summary>
    ''' Starts the updater thread
    ''' </summary>
    Public Sub StartThread()
        If (ClassThread.IsValid(g_mSourceSyntaxUpdaterThread)) Then
            Return
        End If

        g_mSourceSyntaxUpdaterThread = New Threading.Thread(AddressOf SourceSyntaxUpdater_Thread) With {
               .Priority = Threading.ThreadPriority.Lowest,
               .IsBackground = True
           }
        g_mSourceSyntaxUpdaterThread.Start()
    End Sub

    ''' <summary>
    ''' Stops the updater thread
    ''' </summary>
    Public Sub StopThread()
        ClassThread.Abort(g_mSourceSyntaxUpdaterThread)
    End Sub

    ''' <summary>
    ''' The main thread to update all kinds of stuff.
    ''' </summary>
    Private Sub SourceSyntaxUpdater_Thread()
        Static mFullSyntaxParseDelay As New TimeSpan(0, 0, 1, 0, 0)
        Static mVarSyntaxParseDelay As New TimeSpan(0, 0, 0, 10, 0)
        Static mFoldingUpdateDelay As New TimeSpan(0, 0, 5)
        Static mTextMinimapDelay As New TimeSpan(0, 0, 1)
        Static mTextMinimapRefreshDelay As New TimeSpan(0, 0, 10)
        Static mMarkCaretWordDelay As New TimeSpan(0, 0, 1)

        Dim dLastFullSyntaxParseDelay As Date = (Now + mFullSyntaxParseDelay)
        Dim dLastVarSyntaxParseDelay As Date = (Now + mVarSyntaxParseDelay)
        Dim dLastFoldingUpdateDelay As Date = (Now + mFoldingUpdateDelay)
        Dim dLastTextMinimapDelay As Date = (Now + mTextMinimapDelay)
        Dim dLastTextMinimapRefreshDelay As Date = (Now + mTextMinimapRefreshDelay)
        Dim dLastMarkCaretWordDelay As Date = (Now + mMarkCaretWordDelay)

        While True
            Threading.Thread.Sleep(ClassSettings.g_iSettingsThreadUpdateRate)

            Try
                Dim bIsFormMainFocused As Boolean = (Not ClassSettings.g_iSettingsOnlyUpdateSyntaxWhenFocused OrElse ClassThread.ExecEx(Of Boolean)(g_mFormMain, Function() Form.ActiveForm IsNot Nothing))

                'Update Autocomplete
                If (bIsFormMainFocused AndAlso g_mFormMain.g_ClassSyntaxParser.g_lFullSyntaxParseRequests.Count > 0) Then
                    Dim sRequestedTabIdentifier As String = g_mFormMain.g_ClassSyntaxParser.g_lFullSyntaxParseRequests(0).sTabIdentifier
                    Dim sActiveTabIdentifier As String = ClassThread.ExecEx(Of String)(g_mFormMain, Function() g_mFormMain.g_ClassTabControl.m_ActiveTab.m_Identifier)

                    'Active tabs have higher priority to update
                    If (g_mFormMain.g_ClassSyntaxParser.g_lFullSyntaxParseRequests.Exists(Function(a As ClassSyntaxParser.STRUC_SYNTAX_PARSE_TAB_REQUEST) a.sTabIdentifier = sActiveTabIdentifier)) Then
                        sRequestedTabIdentifier = sActiveTabIdentifier
                    End If

                    ClassThread.ExecAsync(g_mFormMain, Sub()
                                                           g_mFormMain.g_ClassSyntaxParser.StartUpdateSchedule(ClassSyntaxParser.ENUM_PARSE_TYPE_FLAGS.ALL, sRequestedTabIdentifier, ClassSyntaxParser.ENUM_PARSE_OPTIONS_FLAGS.NOONE)
                                                       End Sub)

                ElseIf (bIsFormMainFocused AndAlso dLastFullSyntaxParseDelay < Now) Then
                    dLastFullSyntaxParseDelay = (Now + mFullSyntaxParseDelay)

                    ClassThread.ExecAsync(g_mFormMain, Sub()
                                                           g_mFormMain.g_ClassSyntaxParser.StartUpdateSchedule(ClassSyntaxParser.ENUM_PARSE_TYPE_FLAGS.ALL)
                                                       End Sub)
                End If

                'Update Variable Autocomplete
                If (bIsFormMainFocused AndAlso dLastVarSyntaxParseDelay < Now) Then
                    dLastVarSyntaxParseDelay = (Now + mVarSyntaxParseDelay)

                    ClassThread.ExecAsync(g_mFormMain, Sub()
                                                           g_mFormMain.g_ClassSyntaxParser.StartUpdateSchedule(ClassSyntaxParser.ENUM_PARSE_TYPE_FLAGS.VAR_PARSE)
                                                       End Sub)
                End If

                'Update Foldings
                If (bIsFormMainFocused AndAlso dLastFoldingUpdateDelay < Now) Then
                    dLastFoldingUpdateDelay = (Now + mFoldingUpdateDelay)

                    ClassThread.ExecAsync(g_mFormMain, Sub()
                                                           g_mFormMain.g_ClassTabControl.m_ActiveTab.UpdateFoldings()
                                                       End Sub)
                End If

                'Update document minimap
                If (bIsFormMainFocused AndAlso dLastTextMinimapDelay < Now) Then
                    dLastTextMinimapDelay = (Now + mTextMinimapDelay)

                    ClassThread.ExecAsync(g_mFormMain, Sub()
                                                           g_mFormMain.g_mUCTextMinimap.UpdateText(False)
                                                           g_mFormMain.g_mUCTextMinimap.UpdatePosition(False, True, False)
                                                       End Sub)
                End If

                'Update document minimap refresh
                If (bIsFormMainFocused AndAlso dLastTextMinimapRefreshDelay < Now) Then
                    dLastTextMinimapRefreshDelay = (Now + mTextMinimapRefreshDelay)

                    ClassThread.ExecAsync(g_mFormMain, Sub()
                                                           g_mFormMain.g_mUCTextMinimap.UpdateText(True)
                                                           g_mFormMain.g_mUCTextMinimap.UpdatePosition(False, True, False)
                                                       End Sub)
                End If


                Dim iCaretOffset As Integer = ClassThread.ExecEx(Of Integer)(g_mFormMain, Function() g_mFormMain.g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset)
                Dim iCaretPos As Point = ClassThread.ExecEx(Of Point)(g_mFormMain, Function() g_mFormMain.g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea.Caret.ScreenPosition)

                'Update Autocomplete
                Static iLastAutoupdateCaretOffset As Integer = -1
                If (iLastAutoupdateCaretOffset <> iCaretOffset) Then
                    iLastAutoupdateCaretOffset = iCaretOffset

                    UpdateAutocomplete(iCaretOffset)
                End If

                'Update IntelliSense 
                Static iLastIntelliSenseCaretOffset As Integer = -1
                If (iLastIntelliSenseCaretOffset <> iCaretOffset) Then
                    iLastIntelliSenseCaretOffset = iCaretOffset

                    UpdateIntelliSense(iCaretOffset)
                End If

                'Hide Autocomplete & IntelliSense Tooltips when scrolling 
                Static iLastToolTipCaretPos As Point
                If (iLastToolTipCaretPos <> iCaretPos) Then
                    iLastToolTipCaretPos = iCaretPos

                    ClassThread.ExecAsync(g_mFormMain.g_mUCAutocomplete, Sub()
                                                                             g_mFormMain.g_mUCAutocomplete.g_ClassToolTip.UpdateToolTipFormLocation()
                                                                         End Sub)
                End If

                'Update caret word maker
                Static iLastAutoupdateCaretOffset3 As Integer = -1
                If (iLastAutoupdateCaretOffset3 <> iCaretOffset AndAlso dLastMarkCaretWordDelay < Now) Then
                    iLastAutoupdateCaretOffset3 = iCaretOffset
                    dLastMarkCaretWordDelay = (Now + mMarkCaretWordDelay)

                    If (ClassSettings.g_iSettingsAutoMark) Then
                        ClassThread.ExecAsync(g_mFormMain, Sub()
                                                               g_mFormMain.g_ClassTextEditorTools.MarkCaretWord()
                                                           End Sub)
                    End If
                End If

            Catch ex As Threading.ThreadAbortException
                Throw
            Catch ex As Exception
                ClassExceptionLog.WriteToLogMessageBox(ex)
                Threading.Thread.Sleep(5000)
            End Try
        End While
    End Sub

    Private Sub UpdateAutocomplete(iCaretOffset As Integer)
        Dim sTextContent As String = ClassThread.ExecEx(Of String)(g_mFormMain, Function() g_mFormMain.g_ClassTabControl.m_ActiveTab.m_TextEditor.Document.TextContent)
        Dim iLanguage As ClassSyntaxTools.ENUM_LANGUAGE_TYPE = ClassThread.ExecEx(Of ClassSyntaxTools.ENUM_LANGUAGE_TYPE)(g_mFormMain, Function() g_mFormMain.g_ClassTabControl.m_ActiveTab.m_Language)
        Dim mSourceAnalysis As New ClassSyntaxTools.ClassSyntaxSourceAnalysis(sTextContent, iLanguage)

        iCaretOffset = Math.Min(iCaretOffset, sTextContent.Length - 1)

        If (iCaretOffset < 0) Then
            ClassThread.ExecAsync(g_mFormMain, Sub()
                                                   g_mFormMain.g_mUCAutocomplete.UpdateAutocomplete("")
                                                   g_mFormMain.g_mUCAutocomplete.g_ClassToolTip.UpdateToolTip()
                                               End Sub)
            Return
        End If

        If (Not mSourceAnalysis.m_InRange(iCaretOffset) OrElse
                        mSourceAnalysis.m_InString(iCaretOffset) OrElse
                        mSourceAnalysis.m_InChar(iCaretOffset) OrElse
                        mSourceAnalysis.m_InMultiComment(iCaretOffset) OrElse
                        mSourceAnalysis.m_InSingleComment(iCaretOffset)) Then
            ClassThread.ExecAsync(g_mFormMain, Sub()
                                                   g_mFormMain.g_mUCAutocomplete.UpdateAutocomplete("")
                                                   g_mFormMain.g_mUCAutocomplete.g_ClassToolTip.UpdateToolTip()
                                               End Sub)
            Return
        End If

        Dim sFunctionName As String = ClassThread.ExecEx(Of String)(g_mFormMain, Function() g_mFormMain.g_ClassTextEditorTools.GetCaretWord(True, True, True))

        If (ClassThread.ExecEx(Of Integer)(g_mFormMain.g_mUCAutocomplete, Function() g_mFormMain.g_mUCAutocomplete.UpdateAutocomplete(sFunctionName)) < 1) Then
            sFunctionName = ClassThread.ExecEx(Of String)(g_mFormMain, Function() g_mFormMain.g_ClassTextEditorTools.GetCaretWord(False, False, False))

            ClassThread.ExecAsync(g_mFormMain.g_mUCAutocomplete, Sub()
                                                                     g_mFormMain.g_mUCAutocomplete.UpdateAutocomplete(sFunctionName)
                                                                 End Sub)
        End If

        ClassThread.ExecAsync(g_mFormMain.g_mUCAutocomplete, Sub()
                                                                 g_mFormMain.g_mUCAutocomplete.g_ClassToolTip.UpdateToolTip()
                                                             End Sub)
    End Sub

    Private Sub UpdateIntelliSense(iCaretOffset As Integer)
        Dim sTextContent As String = ClassThread.ExecEx(Of String)(g_mFormMain, Function() g_mFormMain.g_ClassTabControl.m_ActiveTab.m_TextEditor.Document.TextContent)
        Dim iLanguage As ClassSyntaxTools.ENUM_LANGUAGE_TYPE = ClassThread.ExecEx(Of ClassSyntaxTools.ENUM_LANGUAGE_TYPE)(g_mFormMain, Function() g_mFormMain.g_ClassTabControl.m_ActiveTab.m_Language)
        Dim mSourceAnalysis As New ClassSyntaxTools.ClassSyntaxSourceAnalysis(sTextContent, iLanguage)

        iCaretOffset = Math.Min(iCaretOffset, sTextContent.Length - 1)

        If (iCaretOffset < 0) Then
            ClassThread.ExecAsync(g_mFormMain, Sub()
                                                   g_mFormMain.g_mUCAutocomplete.g_ClassToolTip.UpdateToolTip("")
                                               End Sub)
            Return
        End If

        If (Not mSourceAnalysis.m_InRange(iCaretOffset) OrElse
                        mSourceAnalysis.m_InMultiComment(iCaretOffset) OrElse
                        mSourceAnalysis.m_InSingleComment(iCaretOffset)) Then
            ClassThread.ExecAsync(g_mFormMain, Sub()
                                                   g_mFormMain.g_mUCAutocomplete.g_ClassToolTip.UpdateToolTip("")
                                               End Sub)
            Return
        End If

        'Create a valid range to read the method name and for performance. 
        Dim mStringBuilder As New Text.StringBuilder
        Dim iLastParenthesisRange As ClassSyntaxTools.ClassSyntaxSourceAnalysis.ENUM_STATE_RANGE
        Dim iLastParenthesis As Integer = mSourceAnalysis.GetParenthesisLevel(iCaretOffset, iLastParenthesisRange)
        If (iLastParenthesisRange = ClassSyntaxTools.ClassSyntaxSourceAnalysis.ENUM_STATE_RANGE.START) Then
            iLastParenthesis -= 1
        End If

        Dim i As Integer
        For i = iCaretOffset - 1 To 0 Step -1
            If (mSourceAnalysis.GetBraceLevel(i, Nothing) < 1 OrElse
                        mSourceAnalysis.GetParenthesisLevel(i, Nothing) < iLastParenthesis - 1) Then
                Exit For
            End If

            If (mSourceAnalysis.m_InNonCode(i)) Then
                Continue For
            End If

            If (mSourceAnalysis.GetParenthesisLevel(i, Nothing) > iLastParenthesis - 1 OrElse
                        mSourceAnalysis.GetBracketLevel(i, Nothing) > 0) Then
                Continue For
            End If

            mStringBuilder.Append(sTextContent(i))
        Next

        Dim sTmp As String = StrReverse(mStringBuilder.ToString).Trim
        Dim sMethodStart As String = Regex.Match(sTmp, "((\b[a-zA-Z0-9_]+\b)(\.){0,1}(\b[a-zA-Z0-9_]+\b){0,1})$").Value

        ClassThread.ExecAsync(g_mFormMain, Sub()
                                               g_mFormMain.g_mUCAutocomplete.g_ClassToolTip.UpdateToolTip(sMethodStart)
                                           End Sub)
    End Sub

End Class
