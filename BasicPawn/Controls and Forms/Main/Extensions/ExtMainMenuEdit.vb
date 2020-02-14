﻿'BasicPawn
'Copyright(C) 2020 TheTimocop

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


Partial Public Class FormMain
    Private Sub ToolStripMenuItem_EditUndo_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditUndo.Click
        g_ClassTabControl.m_ActiveTab.m_TextEditor.Undo()
    End Sub

    Private Sub ToolStripMenuItem_EditRedo_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditRedo.Click
        g_ClassTabControl.m_ActiveTab.m_TextEditor.Redo()
    End Sub

    Private Sub ToolStripMenuItem_EditCut_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditCut.Click
        g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea.ClipboardHandler.Cut(sender, e)
    End Sub

    Private Sub ToolStripMenuItem_EditCopy_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditCopy.Click
        g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea.ClipboardHandler.Copy(sender, e)
    End Sub

    Private Sub ToolStripMenuItem_EditPaste_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditPaste.Click
        g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea.ClipboardHandler.Paste(sender, e)
    End Sub

    Private Sub ToolStripMenuItem_EditDelete_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditDelete.Click
        g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea.ClipboardHandler.Delete(sender, e)
    End Sub

    Private Sub ToolStripMenuItem_EditSelectAll_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditSelectAll.Click
        g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea.ClipboardHandler.SelectAll(sender, e)
    End Sub

    Private Sub ToolStripMenuItem_EditDupLine_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditDupLine.Click
        If (g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.SelectionManager.HasSomethingSelected) Then
            Dim sText As String = g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText
            Dim iCaretOffset As Integer = g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset

            g_ClassTabControl.m_ActiveTab.m_TextEditor.Document.Insert(iCaretOffset, sText)
        Else
            Dim iCaretOffset As Integer = g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset
            Dim iLineOffset As Integer = g_ClassTabControl.m_ActiveTab.m_TextEditor.Document.GetLineSegmentForOffset(iCaretOffset).Offset
            Dim iLineLen As Integer = g_ClassTabControl.m_ActiveTab.m_TextEditor.Document.GetLineSegmentForOffset(iCaretOffset).Length

            g_ClassTabControl.m_ActiveTab.m_TextEditor.Document.Insert(iLineOffset, g_ClassTabControl.m_ActiveTab.m_TextEditor.Document.GetText(iLineOffset, iLineLen) & Environment.NewLine)
        End If

        g_ClassTabControl.m_ActiveTab.m_TextEditor.InvalidateTextArea()
    End Sub

    Private Sub ToolStripMenuItem_EditLineUp_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditLineUp.Click
        With New TextEditorControlEx.MoveSelectedLine()
            .m_Direction = TextEditorControlEx.MoveSelectedLine.ENUM_DIRECTION.UP
            .Execute(g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea)
        End With
    End Sub

    Private Sub ToolStripMenuItem_EditLineDown_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_EditLineDown.Click
        With New TextEditorControlEx.MoveSelectedLine()
            .m_Direction = TextEditorControlEx.MoveSelectedLine.ENUM_DIRECTION.DOWN
            .Execute(g_ClassTabControl.m_ActiveTab.m_TextEditor.ActiveTextAreaControl.TextArea)
        End With
    End Sub
End Class
