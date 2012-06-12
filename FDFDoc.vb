Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Security
Imports System.Security.Permissions
Imports System.Net.Mail.MailMessage
Imports System.Diagnostics.Process
Imports System.Data.OleDb
Imports System.Data
Imports System.Collections
Imports iTextSharp.text.pdf.PdfCopyFields
Imports iTextSharp.text.pdf.PdfReader
Imports System.Drawing
Imports System.Drawing.Image
Imports System.Collections.Generic
Namespace FDFApp
    ' FDFDOC CLASS
	Public Class FDFDoc_Class
		Implements IDisposable
        'Private _DebugMode As Boolean = False
        'Protected Function DEBUGMODE_TESTSCRIPT() As Boolean
        '    Try
        '        If True = True Then
        '            Throw New Exception("Error TEST")
        '        End If
        '    Catch exExceptionError As Exception
        '        If _DebugMode Then
        '            Dim st As New StackTrace
        '            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & exExceptionError.Message, st.GetFrame(1).GetMethod.ReflectedType.Name & "." & st.GetFrame(1).GetMethod.Name, 1)
        '        Else
        '            Throw exExceptionError
        '        End If
        '    End Try
        'End Function
		Private _PDF As New PDFDoc
        Private _FDF As New List(Of FDFDoc_Class)
        Private _CurFDFDoc As Integer = 0
        Private _FDFObjects As New List(Of FDFObjects)
		Dim xField As FDFField, FoundField As Boolean
		Private _FDFErrors As New FDFErrors
		Private _FDFFieldCntr As Integer
		Private _defaultEncoding As Encoding = Encoding.UTF8
		''' <summary>
		''' FDFType enumerator
		''' </summary>
		''' <remarks></remarks>
		Public Enum FDFType
			FDF = 1
			xFDF = 2
			XML = 3
			PDF = 4
			XDP = 5
			XPDF = 6
		End Enum
		''' <summary>
		''' Encryption strength for PDF Forms
		''' </summary>
		''' <remarks></remarks>
		Public Enum EncryptionStrength
			STRENGTH40BITS = False
			STRENGTH128BITS = True
		End Enum
		''' <summary>
		''' PDF Permissions for PDF Form
		''' </summary>
		''' <remarks></remarks>
		Public Enum PDFPermissions
			AllowPrinting = iTextSharp.text.pdf.PdfWriter.ALLOW_PRINTING
			AllowModifyContents = iTextSharp.text.pdf.PdfWriter.ALLOW_MODIFY_CONTENTS
			AllowCopy = iTextSharp.text.pdf.PdfWriter.ALLOW_COPY
			AllowModifyAnnotations = iTextSharp.text.pdf.PdfWriter.ALLOW_MODIFY_ANNOTATIONS
			AllowFillIn = iTextSharp.text.pdf.PdfWriter.ALLOW_FILL_IN
			AllowScreenReaders = iTextSharp.text.pdf.PdfWriter.ALLOW_SCREENREADERS
			AllowAssembly = iTextSharp.text.pdf.PdfWriter.ALLOW_ASSEMBLY
			AllowDegradedPrinting = iTextSharp.text.pdf.PdfWriter.ALLOW_DEGRADED_PRINTING
		End Enum
		''' <summary>
		''' Action trigger types
		''' </summary>
		''' <remarks></remarks>
		Public Enum FDFActionTrigger
            FDFEnter = 0
            FDFExit = 1
            FDFDown = 2
            FDFUp = 3
            FDFFormat = 4
            FDFValidate = 5
            FDFKeystroke = 6
            FDFCalculate = 7
            FDFOnFocus = 8
            FDFOnBlur = 9
		End Enum
		''' <summary>
		''' XDP Action trigger types
		''' </summary>
		''' <remarks></remarks>
		Public Enum XDPActionTrigger
			Clicked = 0
			OnMouseEnter = 1
			OnMouseExit = 2
			MouseDown = 3
			MouseUp = 4
			OnFocus = 8
			OnBlur = 9
			OnHover = 10
		End Enum
		''' <summary>
		''' Action types
		''' </summary>
		''' <remarks></remarks>
		Public Enum ActionTypes
			JavaScript = 1
			Submit = 2
			URL = 3
			Reset = 4
			SubmitXDP = 5
		End Enum
		''' <summary>
		''' Field types
		''' </summary>
		''' <remarks></remarks>
		Public Enum FieldType
			FldTextual = 1
			FldMultiSelect = 3
			FldOption = 5
			FldButton = 10
			FldLiveCycleImage = 20
		End Enum
		''' <summary>
		''' Doc types
		''' </summary>
		''' <remarks></remarks>
		Public Enum FDFDocType
			FDFDoc = 1
			FDFTemplate = 2
			XDPForm = 3
		End Enum
		''' <summary>
		''' Field Structure
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFField
            Public FieldName As String
            Public FieldValue As New List(Of String)
            Public FieldNum As Integer
            Public FieldType As FieldType
            Public FieldEnabled As Boolean
            Public DefaultValue As New List(Of String)
            Public DisplayName As New List(Of String)
            Public DisplayValue As New List(Of String)
            Public ImageBase64 As String
        End Class
		''' <summary>
		''' FDFTemplates
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFTemplate
            Public FDFFields As New List(Of FDFField)
            Public FileName As String
            Public FDFJSActions As New List(Of FDFActions)
        End Class
		''' <summary>
		''' FDF Actions
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFActions
            Public FieldName As String
            Public FieldNum As Integer
            Public FieldType As FieldType
            Public JavaScript_URL As String
            Public ActionType As ActionTypes
            Public Trigger As FDFActionTrigger
            Public Exported As Boolean
            'Public  SubmitAction As String
        End Class
		''' <summary>
		''' XDP Actions
		''' </summary>
		''' <remarks></remarks>
        Public Class XDPActions
            Public FieldName As String
            Public FieldNum As Integer
            Public FieldType As FieldType
            Public JavaScript_URL As String
            Public ActionType As ActionTypes
            Public Trigger As XDPActionTrigger
            Public Exported As Boolean
            Public xdpContent As String
            Public EmbedPDF As Boolean
            Public Format As String
            Public Lock As Boolean
            Public use As String
            Public ID As String
            'Public  SubmitAction As String
        End Class
		''' <summary>
		''' Reset actions
		''' </summary>
		''' <remarks></remarks>
		Private Sub ResetActions()
            'Dim xAction As Integer
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                For Each _x As FDFActions In _FDF(_CurFDFDoc).struc_FDFActions
                    _x.Exported = False
                Next
            End If

            If Not _FDF(_CurFDFDoc).struc_HideActions.Count <= 0 Then
                For Each _x As FDFHideAction In _FDF(_CurFDFDoc).struc_HideActions
                    _x.Exported = False
                Next
            End If

            If Not _FDF(_CurFDFDoc).struc_ImportDataAction.Count <= 0 Then
                For Each _x As FDFImportDataAction In _FDF(_CurFDFDoc).struc_ImportDataAction
                    _x.Exported = False
                Next
            End If

            If Not _FDF(_CurFDFDoc).struc_NamedActions.Count <= 0 Then
                For Each _x As FDFNamedAction In _FDF(_CurFDFDoc).struc_NamedActions
                    _x.Exported = False
                Next
            End If

            If Not _FDF(_CurFDFDoc).struc_XDPActions.Count <= 0 Then
                For Each _x As XDPActions In _FDF(_CurFDFDoc).struc_XDPActions
                    _x.Exported = False
                Next
            End If

		End Sub
		''' <summary>
		''' PDF Document
		''' </summary>
		''' <remarks></remarks>
        Public Class PDFDoc
            Public FileName As String
        End Class
		''' <summary>
		''' FDFDoc class
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFDoc_Class
            Public FileName As String
            Public Status As String
            Public FDFData As String
            Public XDPData As String
            Public PDFData() As Byte
            Public HasChanges As Boolean
            Public Version As String
            Public Differences As String
            Public Annotations As String
            Public AppendSaves As String
            Public TargetFrame As String
            Public DocType As FDFDocType
            Public struc_FDFFields As New List(Of FDFField)
            Public struc_FDFActions As New List(Of FDFActions)
            Public struc_DocScript As New List(Of FDFScripts)
            Public struc_ImportScripts As New List(Of FDFImportScript)
            Public struc_HideActions As New List(Of FDFHideAction)
            Public struc_NamedActions As New List(Of FDFNamedAction)
            Public struc_ImportDataAction As New List(Of FDFImportDataAction)
            Public struc_XDPActions As New List(Of XDPActions)
            Public TmpNewPage As Boolean
            Public TmpTemplateName As String
            Public TmpRename As Boolean
            Public FormName As String
            Public FormLevel As String ' form1/subform1/subform4
            Public XDPSubForms() As FDFDoc_Class
            Public WrittenXDP As Boolean
        End Class
		''' <summary>
		''' FDF Objects
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFObjects
            Public objNum As String
            Public objInteger As Integer
            Public objData As String
            Public objAnnotations As String
            Public objDifferences As String
            Public objVersion As String
        End Class
		''' <summary>
		''' FDF Scripts
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFScripts
            Public ScriptName As String
            Public ScriptCode As String
        End Class
		''' <summary>
		''' FDF Import script
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFImportScript
            Public ScriptCode As String
            Public Before As Boolean
        End Class
		''' <summary>
		''' FDF Hide Action
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFHideAction
            Public FieldName As String
            Public Trigger As FDFActionTrigger
            Public Target As String
            Public Hide As Boolean
            Public Exported As Boolean
        End Class

		''' <summary>
		''' FDF Named Action
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFNamedAction
            Public FieldName As String
            Public Trigger As FDFActionTrigger
            Public Name As String
            Public Hide As Boolean
            Public Exported As Boolean
        End Class

		''' <summary>
		''' FDF Import Data Action
		''' </summary>
		''' <remarks></remarks>
        Public Class FDFImportDataAction
            Public FieldName As String
            Public Trigger As FDFActionTrigger
            Public FileName As String
            Public Exported As Boolean
        End Class

       

		''' <summary>
		''' Merges a dataset into PDF Files
		''' </summary>
		''' <param name="dsData">Dataset to merge</param>
		''' <param name="FileName_FieldNames">File Names</param>
		''' <param name="BlankPDFFormPath">Original Blank PDF File</param>
		''' <param name="DataTableName">Table Name of dataset</param>
		''' <param name="Flatten">Flatten PDF</param>
		''' <returns>Returns true</returns>
		''' <remarks></remarks>
		Public Function PDFBatchMergeDataset2PDFFiles(ByVal dsData As DataSet, ByVal FileName_FieldNames() As String, ByVal BlankPDFFormPath As String, Optional ByVal DataTableName As String = "", Optional ByVal Flatten As Boolean = False) As Boolean
			Dim formFile As String = BlankPDFFormPath
			Try
				If DataTableName = "" Then
					For Each dr As DataRow In dsData.Tables(0).Rows
						If formFile = "" Then
							If _FDF(0).FileName = "" Then
								Return Nothing
								Exit Function
							Else
								formFile = _FDF(0).FileName & ""
							End If
						End If
						Dim newFile As String = ""
						For Each s As String In FileName_FieldNames
							newFile &= "_" & CStr(dr(s))
						Next
						newFile = newFile.TrimStart("_")
						newFile = newFile.TrimEnd("_")
						newFile = FileNameCheck(newFile)
						newFile = newFile & ".pdf"

						Dim reader As New iTextSharp.text.pdf.PdfReader(formFile)
                        Dim MemStream As New MemoryStream
						Try

							Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
							Dim fields As iTextSharp.text.pdf.AcroFields
							fields = stamper.AcroFields
                            FDFSetValuesFromDataRow(dr)
                            Dim xFld As New iTextSharp.text.pdf.AcroFields.Item
                            For Each _fld As FDFField In FDFFields
                                xFld = New iTextSharp.text.pdf.AcroFields.Item
                                xFld = fields.GetFieldItem(_fld.FieldName)
                                If Not xFld Is Nothing Then
                                    If Not String_IsNullOrEmpty(_fld.FieldName) Then
                                        If Not _fld.FieldValue.Count <= 0 Then
                                            If Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                                If _fld.FieldValue.Count = _fld.DisplayValue.Count Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray), FDFCheckCharReverse2(_fld.DisplayName.ToArray))
                                                End If
                                            ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                                If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.DisplayValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))

                                                ElseIf Not _fld.FieldValue.Count <= 0 Then
                                                    If _fld.DisplayValue.Count = 1 Then
                                                        fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                                    ElseIf _fld.DisplayValue.Count > 0 Then
                                                        If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                            fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                        End If
                                                        fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                    End If
                                                End If
                                            ElseIf Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                                If _fld.DisplayValue.Count = _fld.DisplayValue.Count Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                End If
                                            ElseIf Not _fld.FieldValue.Count <= 0 Then
                                                If _fld.DisplayValue.Count = 1 Then
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                                ElseIf _fld.DisplayValue.Count > 0 Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                End If
                                            End If
                                        ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                            If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.DisplayValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            ElseIf Not _fld.FieldValue.Count <= 0 Then
                                                If _fld.DisplayValue.Count = 1 Then
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                                ElseIf _fld.DisplayValue.Count > 0 Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                            stamper.FormFlattening = Flatten
                            stamper.Writer.CloseStream = False
                            stamper.Close()
                            If Not MemStream Is Nothing Then
                                Dim xStream As New FileStream(newFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)
                                xStream.Write(GetUsedBytesOnly(MemStream), 0, GetUsedBytesOnly(MemStream).Length)
                                MemStream.Close()
                                MemStream.Dispose()
                                xStream.Close()
                            End If
                        Catch ex As Exception
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFBatchMergeDataset2PDFFiles", 1)
                        End Try
                    Next
                Else
                    For Each dr As DataRow In dsData.Tables(DataTableName).Rows
                        If formFile = "" Then
                            If _FDF(0).FileName = "" Then
                                Return Nothing
                                Exit Function
                            Else
                                formFile = _FDF(0).FileName & ""
                            End If
                        End If
                        Dim newFile As String = ""
                        For Each s As String In FileName_FieldNames
                            newFile &= "_" & CStr(dr(s))
                        Next
                        newFile = newFile.TrimStart("_")
                        newFile = newFile.TrimEnd("_")
                        newFile = FileNameCheck(newFile)

                        Dim reader As New iTextSharp.text.pdf.PdfReader(formFile)
                        Dim MemStream As New MemoryStream
                        Try

                            Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                            Dim fields As iTextSharp.text.pdf.AcroFields
                            fields = stamper.AcroFields

                            FDFSetValuesFromDataRow(dr)

                            Dim FDFFields() As FDFApp.FDFDoc_Class.FDFField = FDFGetFields()
                            'Dim FDFField As FDFApp.FDFDoc_Class.FDFField
                            Dim xFld As New iTextSharp.text.pdf.AcroFields.Item
                            For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                                xFld = New iTextSharp.text.pdf.AcroFields.Item
                                xFld = fields.GetFieldItem(_fld.FieldName)
                                If Not xFld Is Nothing Then
                                    If Not String_IsNullOrEmpty(_fld.FieldName) Then
                                        If Not _fld.FieldValue.Count <= 0 Then
                                            If Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                                If _fld.DisplayValue.Count = _fld.DisplayValue.Count Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                            ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                                If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.DisplayValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))

                                                ElseIf Not _fld.FieldValue.Count <= 0 Then
                                                    If _fld.DisplayValue.Count = 1 Then
                                                        fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                                    ElseIf _fld.DisplayValue.Count > 0 Then
                                                        If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                            fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                        End If
                                                        fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                    End If
                                                End If
                                            ElseIf Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                                If _fld.DisplayValue.Count = _fld.DisplayValue.Count Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                End If
                                            ElseIf Not _fld.FieldValue.Count <= 0 Then
                                                If _fld.DisplayValue.Count = 1 Then
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                                ElseIf _fld.DisplayValue.Count > 0 Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                End If
                                            End If
                                        ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                            If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.DisplayValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            ElseIf Not _fld.FieldValue.Count <= 0 Then
                                                If _fld.DisplayValue.Count = 1 Then
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                                ElseIf _fld.DisplayValue.Count > 0 Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.DisplayValue.Count = _fld.DisplayName.Count) And (_fld.DisplayValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                            stamper.FormFlattening = Flatten
                            stamper.Writer.CloseStream = False
                            stamper.Close()
                            If Not MemStream Is Nothing Then
                                Dim xStream As New FileStream(newFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)
                                xStream.Write(GetUsedBytesOnly(MemStream), 0, GetUsedBytesOnly(MemStream).Length)
                                MemStream.Close()
                                MemStream.Dispose()
                                xStream.Close()
                            End If
                        Catch ex As Exception
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFBatchMergeDataset2PDFFiles", 1)
                        End Try
                    Next
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFBatchMergeDataset2PDFFiles", 1)
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF with PDF, saves to a file
        ''' </summary>
        ''' <param name="newPDFFile">New PDF File name</param>
        ''' <param name="OriginalSourcePDFFormPath">Blank Original PDF Form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2File(ByVal newPDFFile As String, Optional ByVal OriginalSourcePDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Boolean
            Dim formFile As String = OriginalSourcePDFFormPath
            If formFile = "" Then
                If _FDF(0).FileName = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = _FDF(0).FileName & ""
                End If
            End If
            Dim newFile As String = newPDFFile
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)

                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream)
                End If
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                stamper = Nothing
                myFileStream.Close()
                myFileStream.Dispose()
                'If Not MemStream Is Nothing Then
                '    Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                '    Try
                '        If MemStream.CanSeek Then
                '            MemStream.Position = 0
                '        End If
                '        With myFileStream
                '            .Write(MemStream.GetBuffer, 0, MemStream.GetBuffer.Length)
                '        End With
                '        MemStream.Close()
                '        MemStream.Dispose()
                '    Catch ex As Exception
                '        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                '    Finally
                '        If Not myFileStream Is Nothing Then
                '            With myFileStream
                '                .Close()
                '                .Dispose()
                '            End With
                '        End If
                '    End Try
                '    Return True
                'Else
                '    Return False
                'End If
                Return True
                Return True
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2File", 1)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF and PDF to stream object
        ''' </summary>
        ''' <param name="PDFStream">Output Stream</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Stream with Merged PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Stream(ByVal PDFStream As Stream, Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Stream
            Dim formFile As String = ""
            Dim fdfa As New FDFApp.FDFApp_Class
            Dim reader As iTextSharp.text.pdf.PdfReader = Nothing
            If formFile = "" Then
                If Not PDFStream Is Nothing Then
                    If String_IsNullOrEmpty(ownerPassword) Then
                        reader = New iTextSharp.text.pdf.PdfReader(PDFStream)
                    Else
                        reader = New iTextSharp.text.pdf.PdfReader(PDFStream, DefaultEncoding.GetBytes(ownerPassword))
                    End If
                Else
                    If _FDF(0).FileName = "" Then
                        Return Nothing
                        Exit Function
                    Else
                        formFile = _FDF(0).FileName & ""
                        If String_IsNullOrEmpty(ownerPassword) Then
                            reader = New iTextSharp.text.pdf.PdfReader(formFile)
                        Else
                            reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
                        End If
                    End If
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    If MemStream.CanSeek Then MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Stream", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF and PDF to stream
        ''' </summary>
        ''' <param name="PDFFormPath">Original Blank PDF path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Stream with merged PDF and FDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Stream(Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Stream
            Dim formFile As String = PDFFormPath
            If formFile = "" Then
                If _FDF(0).FileName = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = _FDF(0).FileName & ""
                End If
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    If MemStream.CanSeek Then MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Stream", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF Form to byte array (buffer)
        ''' </summary>
        ''' <param name="PDFFormPath">Original Blank PDF Form Path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with merged PDF and data</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Byte()
            Dim formFile As String = PDFFormPath
            If formFile = "" Then
                If _FDF(0).FileName = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = _FDF(0).FileName & ""
                End If
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF to byte array
        ''' </summary>
        ''' <param name="PDFStream">PDF Stream of original blank pdf</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Returns byte array with merged Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal PDFStream As Stream, Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Byte()
            Dim formFile As String = ""
            Dim reader As iTextSharp.text.pdf.PdfReader = Nothing
            If Not PDFStream Is Nothing Then
                PDFStream.Position = 0
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFStream)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFStream, DefaultEncoding.GetBytes(ownerPassword))
                End If
            Else
                If formFile = "" Then
                    If _FDF(0).FileName = "" Then
                        Return Nothing
                        Exit Function
                    Else
                        formFile = _FDF(0).FileName & ""
                        If String_IsNullOrEmpty(ownerPassword) Then
                            reader = New iTextSharp.text.pdf.PdfReader(PDFStream)
                        Else
                            reader = New iTextSharp.text.pdf.PdfReader(PDFStream, DefaultEncoding.GetBytes(ownerPassword))
                        End If
                    End If
                End If
            End If

            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF Data with PDF Form and returns Byte array, allows encryption and permissions
        ''' </summary>
        ''' <param name="PDFStream">Original PDF Form Stream</param>
        ''' <param name="OpenPassword">Open PDF password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">Permissions</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array containing merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal PDFStream As Stream, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFStream.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                PDFStream.Position = 0
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFStream)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFStream, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF form to byte array
        ''' </summary>
        ''' <param name="FDFDocument">FDFDoc_Class document</param>
        ''' <param name="PDFForm">Original Blank PDF Form Bytes</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal FDFDocument As FDFApp.FDFDoc_Class, ByVal PDFForm As Byte(), Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Set_PDF_Fields_Merge(FDFDocument, reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF Data with PDF Form and returns Byte array, allows encryption and permissions
        ''' </summary>
        ''' <param name="FDFDocument">FDFDoc_Class with data</param>
        ''' <param name="PDFForm">Original PDF Form Bytes</param>
        ''' <param name="OpenPassword">Open PDF password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">Permissions</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array containing merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal FDFDocument As FDFApp.FDFDoc_Class, ByVal PDFForm As Byte(), ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Set_PDF_Fields_Merge(FDFDocument, reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function
        Private Sub Set_PDF_Fields_Merge(ByRef reader As iTextSharp.text.pdf.PdfReader, ByRef stamper As iTextSharp.text.pdf.PdfStamper)
            Try

                Dim fields As iTextSharp.text.pdf.AcroFields
                fields = stamper.AcroFields
                Dim FDFDoc As FDFDoc_Class = _FDF(0)
                Dim FDFApp As New FDFApp_Class
                Dim FDFFields() As FDFApp.FDFDoc_Class.FDFField = FDFGetFields()
                'Dim FDFField As FDFApp.FDFDoc_Class.FDFField
                Dim xFld As New iTextSharp.text.pdf.AcroFields.Item
                Dim retString As String = ""
                Try

                    If HasDocJavaScripts() Or HasDocOnImportJavaScripts() Then
                        retString = ""
                        If HasDocJavaScripts() Then
                            retString = retString & GetDocJavaScripts()
                            If HasDocOnImportJavaScripts() Then
                                retString = retString & Me.FDFGetImportJSActions(False)
                                retString = FDFCheckCharReverse(retString)
                                Dim writer As iTextSharp.text.pdf.PdfWriter
                                writer = stamper.Writer
                                Dim JSAction As iTextSharp.text.pdf.PdfAction = iTextSharp.text.pdf.PdfAction.JavaScript(retString, writer)
                                writer.AddJavaScript(JSAction)
                            End If
                        Else
                            If HasDocOnImportJavaScripts() Then
                                retString = Me.FDFGetImportJSActions(True, True)
                                retString = FDFCheckCharReverse(retString)
                                Dim writer As iTextSharp.text.pdf.PdfWriter
                                writer = stamper.Writer
                                Dim JSAction As iTextSharp.text.pdf.PdfAction = iTextSharp.text.pdf.PdfAction.JavaScript(retString, writer)
                                writer.AddJavaScript(JSAction)
                            End If
                        End If
                    End If
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.Set_PDF_Fields_Merge", 1)
                End Try

                Try

                    For Each _fld As FDFField In FDFFields
                        xFld = New iTextSharp.text.pdf.AcroFields.Item
                        If Not String_IsNullOrEmpty(_fld.FieldName) Then
                            xFld = fields.GetFieldItem(_fld.FieldName)
                            If Not xFld Is Nothing Then
                                If Not String_IsNullOrEmpty(_fld.FieldName) Then

                                    If Not _fld.FieldValue.Count <= 0 Then
                                        If Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                            If _fld.FieldValue.Count = _fld.DisplayValue.Count Then
                                                fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                            End If
                                        ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                            If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))

                                            ElseIf Not _fld.FieldValue.Count <= 0 Then
                                                If _fld.FieldValue.Count = 1 Then
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                                ElseIf _fld.FieldValue.Count > 0 Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                End If
                                            End If
                                        ElseIf Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                            If _fld.FieldValue.Count = _fld.DisplayValue.Count Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            End If
                                        ElseIf Not _fld.FieldValue.Count <= 0 Then
                                            If _fld.FieldValue.Count = 1 Then
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                            ElseIf _fld.FieldValue.Count > 0 Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            End If
                                        End If
                                    ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                        If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                            If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                            End If
                                            fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                        ElseIf Not _fld.FieldValue.Count <= 0 Then
                                            If _fld.FieldValue.Count = 1 Then
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                            ElseIf _fld.FieldValue.Count > 0 Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            End If
                                        End If

                                    End If
                                End If
                            End If
                        End If
                    Next
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.Set_PDF_Fields_Merge", 2)
                End Try
                Try
                    PDF_iTextSharp_SetSubmitButtonURLs(stamper, reader)
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.Set_PDF_Fields_Merge", 3)
                End Try
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.Set_PDF_Fields_Merge", 4)
            End Try

        End Sub
        Private Sub Set_PDF_Fields_Merge(ByVal FDFDocX As FDFApp.FDFDoc_Class, ByRef reader As iTextSharp.text.pdf.PdfReader, ByRef stamper As iTextSharp.text.pdf.PdfStamper)
            Try

                Dim fields As iTextSharp.text.pdf.AcroFields
                fields = stamper.AcroFields
                Dim FDFDoc As FDFApp.FDFDoc_Class = FDFDocX
                Dim FDFApp As New FDFApp_Class
                Dim FDFFields() As FDFApp.FDFDoc_Class.FDFField = FDFGetFields()
                'Dim FDFField As FDFApp.FDFDoc_Class.FDFField
                Dim xFld As New iTextSharp.text.pdf.AcroFields.Item
                Dim retString As String = ""
                Try

                    If FDFDoc.HasDocJavaScripts() Or FDFDoc.HasDocOnImportJavaScripts() Then
                        retString = ""
                        If FDFDoc.HasDocJavaScripts() Then
                            retString = retString & FDFDoc.GetDocJavaScripts()
                            If FDFDoc.HasDocOnImportJavaScripts() Then
                                retString = retString & FDFDoc.FDFGetImportJSActions(False)
                                retString = FDFDoc.FDFCheckCharReverse(retString)
                                Dim writer As iTextSharp.text.pdf.PdfWriter
                                writer = stamper.Writer
                                Dim JSAction As iTextSharp.text.pdf.PdfAction = iTextSharp.text.pdf.PdfAction.JavaScript(retString, writer)
                                writer.AddJavaScript(JSAction)
                            End If
                        Else
                            If HasDocOnImportJavaScripts() Then
                                retString = FDFDoc.FDFGetImportJSActions(True, True)
                                retString = FDFCheckCharReverse(retString)
                                Dim writer As iTextSharp.text.pdf.PdfWriter
                                writer = stamper.Writer
                                Dim JSAction As iTextSharp.text.pdf.PdfAction = iTextSharp.text.pdf.PdfAction.JavaScript(retString, writer)
                                'Dim JSAction As iTextSharp.text.pdf.PdfAction = iTextSharp.text.pdf.PdfAction.JavaScript("var clr = this.getField('FullName');clr.textColor = color.red;", writer)
                                'var clr = this.getField('FullName');clr.textColor = color.red;"
                                writer.AddJavaScript(JSAction)
                            End If
                        End If
                    End If
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.Set_PDF_Fields_Merge", 1)
                End Try

                Try

                    For Each _fld As FDFField In FDFDoc.FDFFields
                        xFld = New iTextSharp.text.pdf.AcroFields.Item
                        If Not String_IsNullOrEmpty(_fld.FieldName) Then
                            xFld = fields.GetFieldItem(_fld.FieldName)
                            If Not xFld Is Nothing Then
                                If Not String_IsNullOrEmpty(_fld.FieldName) Then

                                    If Not _fld.FieldValue.Count <= 0 Then
                                        If Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                            If _fld.FieldValue.Count = _fld.DisplayValue.Count Then
                                                fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                            End If
                                        ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                            If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.ToArray().Length) And (_fld.FieldValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))

                                            ElseIf Not _fld.FieldValue.Count <= 0 Then
                                                If _fld.FieldValue.Count = 1 Then
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                                ElseIf _fld.FieldValue.Count > 0 Then
                                                    If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.ToArray().Length) And (_fld.FieldValue.Count >= 1) Then
                                                        fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                    End If
                                                    fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                                End If
                                            End If
                                        ElseIf Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                            If _fld.FieldValue.Count = _fld.DisplayValue.Count Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.ToArray().Length) And (_fld.FieldValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            End If
                                        ElseIf Not _fld.FieldValue.Count <= 0 Then
                                            If _fld.FieldValue.Count = 1 Then
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                            ElseIf _fld.FieldValue.Count > 0 Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.ToArray().Length) And (_fld.FieldValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            End If
                                        End If
                                    ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                        If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                            If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.ToArray().Length) And (_fld.FieldValue.Count >= 1) Then
                                                fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                            End If
                                            fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                        ElseIf Not _fld.FieldValue.Count <= 0 Then
                                            If _fld.FieldValue.Count = 1 Then
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                            ElseIf _fld.FieldValue.Count > 0 Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.ToArray().Length) And (_fld.FieldValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray()))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            End If
                                        End If

                                    End If
                                End If
                            End If
                        End If
                    Next
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.Set_PDF_Fields_Merge", 2)
                End Try
                Try
                    PDF_iTextSharp_SetSubmitButtonURLs(stamper, reader)
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.Set_PDF_Fields_Merge", 3)
                End Try
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.Set_PDF_Fields_Merge", 4)
            End Try

        End Sub
        ''' <summary>
        ''' Merges FDF Data and PDF form to byte array
        ''' </summary>
        ''' <param name="PDFForm">Original Blank PDF Form Bytes</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal PDFForm As Byte(), Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function
#Region "PRESERVE EXTENDED RIGHTS"

        ' EDITED 2010-10-29 NK-INC @ 8:52PM
        ' <param name="preserveExtendedRights">Preserve Extended Reader Rights</param>
        ' <param name="removeExtendedRights">Remove Extended Reader Rights</param>
        'Public Function PDFMergeFDF2Buf(ByVal PDFForm As Byte(), ByVal Flatten As Boolean, ByVal ownerPassword As String, ByVal preserveExtendedRights As Boolean, ByVal removeExtendedRights As Boolean) As Byte()
        ''' <summary>
        ''' Merges FDF Data and PDF Form to byte array (buffer)
        ''' </summary>
        ''' <param name="PDFFormPath">Original Blank PDF Form Path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <param name="preserveExtendedRights">Preserve Extended Reader Rights</param>
        ''' <param name="removeExtendedRights">Remove Extended Reader Rights</param>
        ''' <returns>Byte array with merged PDF and data</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal PDFFormPath As String, ByVal Flatten As Boolean, ByVal ownerPassword As String, ByVal preserveExtendedRights As Boolean, ByVal removeExtendedRights As Boolean) As Byte()
            Dim formFile As String = PDFFormPath
            If formFile = "" Then
                If _FDF(0).FileName = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = _FDF(0).FileName & ""
                End If
            End If
            'Dim reader As New iTextSharp.text.pdf.PdfReader(formFile)
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If removeExtendedRights Or RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If preserveExtendedRights Or PreserveUsageRights Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                'Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'PDFData = GetUsedBytesOnly(MemStream)
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    Return GetUsedBytesOnly(MemStream, True)
                Else
                    'stamper.Close()
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function
#End Region
        ''' <summary>
        ''' Merges FDF Data with PDF Form and returns Byte array, allows encryption and permissions
        ''' </summary>
        ''' <param name="PDFForm">Original PDF Form Bytes</param>
        ''' <param name="OpenPassword">Open PDF password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">Permissions</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <returns>Byte array containing merged FDF Data and PDF</returns>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal PDFForm As Byte(), ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function






        ''' <summary>
        ''' Merges FDF Data and PDF form to byte array
        ''' </summary>
        ''' <param name="FDFDocument">FDFDoc_Class document</param>
        ''' <param name="PDFFormPath">Original Blank PDF Form Path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal FDFDocument As FDFApp.FDFDoc_Class, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Byte()
            Dim formFile As String = PDFFormPath & ""
            If formFile = "" Then
                If FDFDocument.FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFDocument.FDFGetFile & ""
                End If
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Set_PDF_Fields_Merge(FDFDocument, reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    'stamper.Close()
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function
        Public Function Change_SubmitButtonURL(ByVal formFile As String, ByVal ownerPassword As String, ByVal fieldname As String, ByVal submitURL As String, ByVal submitDataType As FDFType) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            Try
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(formFile)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
                End If
            Catch ex As Exception
                Return Nothing
            End Try
            Dim MemStream As New MemoryStream
            'Dim pdfStamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
            'Dim pdfStamper As iTextSharp.text.pdf.PdfStamper
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim form As iTextSharp.text.pdf.AcroFields
                form = stamper.AcroFields
                'form.SetFields(fdfReader)
                Dim fields As Hashtable
                fields = form.Fields
                Try
                    If Not form.Fields(fieldname) Is Nothing Then

                        ' NEW
                        Dim submitBtn As iTextSharp.text.pdf.PushbuttonField = form.GetNewPushbuttonFromField(fieldname) ' new PushbuttonField(writer,
                        Dim submitField As iTextSharp.text.pdf.PdfFormField = submitBtn.Field
                        Select Case submitDataType
                            Case FDFType.FDF
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_INCLUDE_NO_VALUE_FIELDS)
                            Case FDFType.PDF
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_PDF)
                            Case FDFType.xFDF
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_XFDF)
                                'Case FDFType.XML
                                'submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_HTML_FORMAT)
                            Case Else
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_INCLUDE_NO_VALUE_FIELDS)
                        End Select
                        stamper.AcroFields.ReplacePushbuttonField(fieldname, submitField)
                        stamper.Writer.CloseStream = False
                        stamper.Close()
                        ' END NEW

                    End If
                Catch ex As Exception

                End Try
                'If MemStream.CanSeek Then MemStream.Position = 0
                Return GetUsedBytesOnly(MemStream, True)
                'Return MemStream.GetBuffer
            Catch ex As Exception
                Return Nothing
            End Try
        End Function
        Public Function Change_SubmitButtonURL(ByVal formFile() As Byte, ByVal ownerPassword As String, ByVal fieldname As String, ByVal submitURL As String, ByVal submitDataType As FDFType) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            Try
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(formFile)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
                End If
            Catch ex As Exception
                Return Nothing
            End Try
            Dim MemStream As New MemoryStream
            'Dim pdfStamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
            'Dim pdfStamper As iTextSharp.text.pdf.PdfStamper
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim form As iTextSharp.text.pdf.AcroFields
                form = stamper.AcroFields
                'form.SetFields(fdfReader)
                Dim fields As Hashtable
                fields = form.Fields
                Try
                    If Not form.Fields(fieldname) Is Nothing Then

                        ' NEW
                        Dim submitBtn As iTextSharp.text.pdf.PushbuttonField = form.GetNewPushbuttonFromField(fieldname) ' new PushbuttonField(writer,
                        Dim submitField As iTextSharp.text.pdf.PdfFormField = submitBtn.Field
                        Select Case submitDataType
                            Case FDFType.FDF
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_INCLUDE_NO_VALUE_FIELDS)
                            Case FDFType.PDF
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_PDF)
                            Case FDFType.xFDF
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_XFDF)
                                'Case FDFType.XML
                                'submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_HTML_FORMAT)
                            Case Else
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_INCLUDE_NO_VALUE_FIELDS)
                        End Select
                        stamper.AcroFields.ReplacePushbuttonField(fieldname, submitField)
                        stamper.Writer.CloseStream = False
                        stamper.Close()
                        ' END NEW


                    End If
                Catch ex As Exception

                End Try
                'If MemStream.CanSeek Then MemStream.Position = 0
                Return GetUsedBytesOnly(MemStream, True)
                'Return MemStream.GetBuffer
            Catch ex As Exception
                Return Nothing
            End Try
        End Function
        Public Function Change_SubmitButtonURL(ByVal formFile As Stream, ByVal ownerPassword As String, ByVal fieldname As String, ByVal submitURL As String, ByVal submitDataType As FDFType) As Byte()
            If formFile.CanSeek Then
                formFile.Position = 0
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            Try
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(formFile)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
                End If
            Catch ex As Exception
                Return Nothing
            End Try
            Dim MemStream As New MemoryStream
            'Dim pdfStamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
            'Dim pdfStamper As iTextSharp.text.pdf.PdfStamper
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim form As iTextSharp.text.pdf.AcroFields
                form = stamper.AcroFields
                'form.SetFields(fdfReader)
                Dim fields As Hashtable
                fields = form.Fields
                Try
                    If Not form.Fields(fieldname) Is Nothing Then
                        ' NEW
                        Dim submitBtn As iTextSharp.text.pdf.PushbuttonField = form.GetNewPushbuttonFromField(fieldname) ' new PushbuttonField(writer,
                        Dim submitField As iTextSharp.text.pdf.PdfFormField = submitBtn.Field
                        Select Case submitDataType
                            Case FDFType.FDF
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_INCLUDE_NO_VALUE_FIELDS)
                            Case FDFType.PDF
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_PDF)
                            Case FDFType.xFDF
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_XFDF)
                                'Case FDFType.XML
                                'submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_HTML_FORMAT)
                            Case Else
                                submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(submitURL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_INCLUDE_NO_VALUE_FIELDS)
                        End Select
                        stamper.AcroFields.ReplacePushbuttonField(fieldname, submitField)
                        stamper.Writer.CloseStream = False
                        stamper.Close()
                        ' END NEW

                        stamper.Writer.CloseStream = False
                        stamper.Close()
                    End If
                Catch ex As Exception

                End Try
                'If MemStream.CanSeek Then MemStream.Position = 0
                Return GetUsedBytesOnly(MemStream, True)
                'Return MemStream.GetBuffer
            Catch ex As Exception
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF Data with PDF Form and returns Byte array, allows encryption and permissions
        ''' </summary>
        ''' <param name="FDFDocument">FDFDoc_Class with data</param>
        ''' <param name="OpenPassword">Open PDF password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">Permissions</param>
        ''' <param name="PDFFormPath">Original PDF Form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array containing merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal FDFDocument As FDFApp.FDFDoc_Class, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Byte()
            Dim formFile As String = PDFFormPath
            If formFile = "" Then
                If FDFDocument.FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFDocument.FDFGetFile & ""
                End If
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Set_PDF_Fields_Merge(FDFDocument, reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'PDFData = GetUsedBytesOnly(MemStream)
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    'stamper.Close()
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF Form with password protection and outputs to a file
        ''' </summary>
        ''' <param name="newPDFFile">New PDF file</param>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">Permissions</param>
        ''' <param name="PDFFormPath">Original PDF Form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption Strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Private Function PDFMergeFDF2File_PwProtected(ByVal newPDFFile As String, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal oldModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Boolean
            Dim formFile As String = PDFFormPath
            If formFile = "" Then
                If _FDF(0).FileName = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = _FDF(0).FileName & ""
                End If
            End If
            Dim newFile As String = newPDFFile
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If


            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)

                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream)
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                stamper = Nothing
                myFileStream.Close()
                myFileStream.Dispose()
                'If Not MemStream Is Nothing Then
                '    Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                '    Try
                '        With myFileStream
                '            .Write(GetUsedBytesOnly(MemStream), 0, GetUsedBytesOnly(MemStream).Length)
                '        End With
                '        MemStream.Close()
                '        MemStream.Dispose()
                '    Catch ex As Exception
                '        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                '    Finally
                '        If Not myFileStream Is Nothing Then
                '            With myFileStream
                '                .Close()
                '                .Dispose()
                '            End With
                '        End If
                '    End Try
                '    Return True
                'Else
                '    Return False
                'End If
                Return True
            Catch ex As Exception
                'IOPerm.RevertAssert()
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2File", 1)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF Form and outputs to a file
        ''' </summary>
        ''' <param name="newPDFFile">New PDF file</param>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">Permissions</param>
        ''' <param name="PDFFormPath">Original PDF Form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption Strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2File(ByVal newPDFFile As String, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Boolean
            Dim formFile As String = PDFFormPath
            If formFile = "" Then
                If _FDF(0).FileName = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = _FDF(0).FileName & ""
                End If
            End If
           Dim newFile As String = newPDFFile
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)

                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream)
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                stamper = Nothing
                myFileStream.Close()
                myFileStream.Dispose()
                'If Not MemStream Is Nothing Then
                '    Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                '    Try
                '        If MemStream.CanSeek Then
                '            MemStream.Position = 0
                '        End If
                '        With myFileStream
                '            .Write(GetUsedBytesOnly(MemStream), 0, GetUsedBytesOnly(MemStream).Length)
                '        End With
                '        MemStream.Close()
                '        MemStream.Dispose()
                '    Catch ex As Exception
                '        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                '    Finally
                '        If Not myFileStream Is Nothing Then
                '            With myFileStream
                '                .Close()
                '                .Dispose()
                '            End With
                '        End If
                '    End Try
                '    Return True
                'Else
                '    Return False
                'End If
                Return True
            Catch ex As Exception
                'IOPerm.RevertAssert()
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2File", 1)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Combines PDF Forms and then Merges with data and outputs to a file
        ''' </summary>
        ''' <param name="FileNames">PDF File names (String array)</param>
        ''' <param name="newPDFFile">New PDF Form path</param>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption Strength</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFConcatenateForms2File(ByVal FileNames() As String, ByVal newPDFFile As String, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS) As Boolean
            Dim newFile As String = newPDFFile
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim copy As iTextSharp.text.pdf.PdfCopyFields
            Dim copyStream As New MemoryStream
            If PreserveUsageRights And Flatten = False Then
                copy = New iTextSharp.text.pdf.PdfCopyFields(copyStream, "\0")
            Else
                copy = New iTextSharp.text.pdf.PdfCopyFields(copyStream)
            End If
            Try
                For Each FileNm As String In FileNames
                    If Not FileNm Is Nothing Then
                        If FileNm.Length > 0 Then
                            reader = New iTextSharp.text.pdf.PdfReader(FileNm)
                            If Not reader Is Nothing Then
                                If RemoveUsageRights Then
                                    reader.RemoveUsageRights()
                                End If
                            End If
                            If Not reader Is Nothing Then
                                copy.AddDocument(reader)
                            End If
                        End If
                    End If
                Next

                copy.Writer.CloseStream = False
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File(1)", 1)
            End Try

            Dim xStream As New MemoryStream
            If copyStream.CanSeek Then
                copyStream.Position = 0
            End If
            reader = New iTextSharp.text.pdf.PdfReader(copyStream.GetBuffer)
            Dim stamper As iTextSharp.text.pdf.PdfStamper
            stamper = New iTextSharp.text.pdf.PdfStamper(reader, xStream)
            Try
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                stamper.Writer.CloseStream = False
                stamper.FormFlattening = Flatten
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File(2)", 2)
                Return False
            End Try

            Try
                Dim FileStrem As New FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)
                If Not FileStrem Is Nothing Then
                    'If xStream.CanSeek Then xStream.Position = 0
                    With FileStrem
                        .Write(GetUsedBytesOnly(xStream), 0, GetUsedBytesOnly(xStream).Length)
                    End With
                    FileStrem.Close()
                    FileStrem.Dispose()
                    xStream.Close()
                    xStream.Dispose()
                    copyStream.Close()
                    copyStream.Dispose()
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File(3)", 3)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Combines PDF Forms and then Merges with data and outputs to a byte array
        ''' </summary>
        ''' <param name="FileNames">PDF File names (String array)</param>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption Strength</param>
        ''' <returns>return Combined and merged PDF Form</returns>
        ''' <remarks></remarks>
        Public Function PDFConcatenateForms2Buf(ByVal FileNames() As String, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim MemStream As New MemoryStream
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(MemStream)
            Try
                For Each FileNm As String In FileNames
                    If Not FileNm Is Nothing Then
                        If FileNm.Length > 0 Then
                            reader = New iTextSharp.text.pdf.PdfReader(FileNm)
                            If Not reader Is Nothing Then
                                If RemoveUsageRights = True Or Flatten = True Then
                                    reader.RemoveUsageRights()
                                End If
                            End If
                            If Not reader Is Nothing Then
                                copy.AddDocument(reader)
                            End If
                        End If
                    End If
                Next
                copy.Writer.CloseStream = False
            Catch ex As Exception
                ''File.Delete(newPDFFile & ".tmp")
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
                Return Nothing
            End Try

            Try
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
            End Try

            If MemStream.CanSeek Then
                MemStream.Position = 0
            End If
            reader = New iTextSharp.text.pdf.PdfReader(MemStream)
            Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
            Try

                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2Buf", 1)
                Return Nothing
            End Try

            Try
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2Buf", 2)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Combines PDF Forms and then outputs to a file
        ''' </summary>
        ''' <param name="FileNames">PDF File names (String array)</param>
        ''' <param name="newPDFFile">New PDF File path</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFConcatenateForms2File(ByVal FileNames() As String, ByVal newPDFFile As String) As Boolean

            Dim newFile As String = newPDFFile
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim copy As iTextSharp.text.pdf.PdfCopyFields
            Dim copyStream As New MemoryStream
            If PreserveUsageRights Then
                copy = New iTextSharp.text.pdf.PdfCopyFields(copyStream, "\0")
            Else

                copy = New iTextSharp.text.pdf.PdfCopyFields(copyStream)
            End If
            Try
                For Each FileNm As String In FileNames
                    If Not FileNm Is Nothing Then
                        If FileNm.Length > 0 Then
                            reader = New iTextSharp.text.pdf.PdfReader(FileNm)
                            If Not reader Is Nothing Then
                                If RemoveUsageRights Then
                                    reader.RemoveUsageRights()
                                End If
                            End If
                            If Not reader Is Nothing Then
                                copy.AddDocument(reader)
                            End If
                        End If
                    End If
                Next

                copy.Writer.CloseStream = False
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File(1)", 1)
            End Try
            If copyStream.CanSeek Then
                copyStream.Position = 0
            End If
            reader = New iTextSharp.text.pdf.PdfReader(copyStream.GetBuffer)
            Try
                Dim FileStrem As New FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)
                If Not FileStrem Is Nothing Then
                    'If copyStream.CanSeek Then
                    '    copyStream.Position = 0
                    'End If
                    FileStrem.Write(GetUsedBytesOnly(copyStream), 0, GetUsedBytesOnly(copyStream).Length)
                    FileStrem.Close()
                    FileStrem.Dispose()
                    copyStream.Close()
                    copyStream.Dispose()
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Combines PDF Forms and then outputs to a file
        ''' </summary>
        ''' <param name="PDFFiles">PDF Files (Array of Byte array)</param>
        ''' <param name="newPDFFile">New PDF File path</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFConcatenateForms2File(ByVal PDFFiles As Byte()(), ByVal newPDFFile As String) As Boolean

            Dim newFile As String = newPDFFile
            Dim reader As iTextSharp.text.pdf.PdfReader = Nothing
            Dim MemStream As New MemoryStream
            Try
                Dim copy As New iTextSharp.text.pdf.PdfCopyFields(MemStream)
                For Each FileNm As Byte() In PDFFiles
                    If Not FileNm Is Nothing Then
                        If FileNm.Length > 0 Then
                            reader = New iTextSharp.text.pdf.PdfReader(FileNm)
                            If Not reader Is Nothing Then
                                If RemoveUsageRights = True Then
                                    reader.RemoveUsageRights()
                                End If
                            End If
                            If Not reader Is Nothing Then
                                copy.AddDocument(reader)
                            End If
                            Try
                                reader.Close()
                            Catch ex As Exception

                            End Try
                        End If
                    End If
                Next

                Try
                    copy.Writer.CloseStream = False
                    copy.Close()
                    If MemStream.CanSeek Then
                        MemStream.Position = 0
                    End If
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2Buf", 1)
                    Return Nothing
                End Try
                Dim FileStrem As New FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)
               
                If Not FileStrem Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    Dim b() As Byte = GetUsedBytesOnly(MemStream, True)
                    FileStrem.Write(b, 0, b.Length)
                    MemStream.Close()
                    MemStream.Dispose()
                    FileStrem.Close()
                    FileStrem.Dispose()
                    MemStream = Nothing
                    FileStrem = Nothing
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
                Return False
            End Try


        End Function
        ''' <summary>
        ''' Combines PDF Forms and then outputs to a byte array
        ''' </summary>
        ''' <param name="FileNames">PDF File names (String array)</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFConcatenateForms2Buf(ByVal FileNames() As String) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim MemStream As New MemoryStream
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(MemStream)
            Try
                For Each FileNm As String In FileNames
                    If Not FileNm Is Nothing Then
                        If FileNm.Length > 0 Then
                            reader = New iTextSharp.text.pdf.PdfReader(FileNm)
                            If Not reader Is Nothing Then
                                If RemoveUsageRights = True Then
                                    reader.RemoveUsageRights()
                                End If
                            End If
                            If Not reader Is Nothing Then
                                copy.AddDocument(reader)
                            End If
                        End If
                    End If
                Next
                copy.Writer.CloseStream = False
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
                Return Nothing
            End Try

            Try
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
            End Try
            Try
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    Return GetUsedBytesOnly(MemStream, True)
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2Buf", 2)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Combines PDF Forms and then outputs to a byte array
        ''' </summary>
        ''' <param name="PDFFiles">PDF Files (Array of Bytes Array)</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFConcatenateForms2Buf(ByVal PDFFiles As Byte()()) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim MemStream As New MemoryStream
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(MemStream)
            Try
                For Each FileNm As Byte() In PDFFiles
                    If Not FileNm Is Nothing Then
                        If FileNm.Length > 0 Then
                            reader = New iTextSharp.text.pdf.PdfReader(FileNm)
                            If Not reader Is Nothing Then
                                If RemoveUsageRights = True Then
                                    reader.RemoveUsageRights()
                                End If
                            End If
                            If Not reader Is Nothing Then
                                copy.AddDocument(reader)
                            End If
                        End If
                    End If
                Next
                copy.Writer.CloseStream = False
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
                Return Nothing
            End Try

            Try
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
            End Try

            Try

                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    Return GetUsedBytesOnly(MemStream, True)
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2Buf", 2)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Gets used bytes
        ''' </summary>
        ''' <param name="m"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetUsedBytesOnly(ByRef m As MemoryStream, Optional ByVal closeStream As Boolean = False) As Byte()
            If m.CanSeek Then m.Position = 0
            Dim bytes As Byte() = m.GetBuffer()
            Dim i As Integer = 0
            For i = bytes.Length - 1 To 1 Step -1
                If bytes(i) <> 0 Then
                    Exit For
                End If
            Next
            Dim newBytes As Byte() = New Byte(i - 1) {}
            Array.Copy(bytes, newBytes, i)
            ReDim bytes(0)
            bytes = Nothing
            If closeStream Then
                m.Close()
                m.Dispose()
            End If
            Return newBytes
        End Function
        ''' <summary>
        ''' Combines PDF Forms and merges the data and outputs to a file
        ''' </summary>
        ''' <param name="FDFDocs">PDFDoc_Classes array - FDFSetFile must be set for each</param>
        ''' <param name="newPDFFile">New PDF File</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDFConcatenateForms2File(ByVal FDFDocs() As FDFApp.FDFDoc_Class, ByVal newPDFFile As String, Optional ByVal Flatten As Boolean = False) As Boolean
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim FileStream As New FileStream(newPDFFile, FileMode.Create, FileAccess.Write, FileShare.Write)
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(FileStream)
            Try
                Dim ctr As Integer = 0
                For Each FDFDoc As FDFApp.FDFDoc_Class In FDFDocs
                    If Not FDFDoc.FDFGetFile Is Nothing Then
                        If FDFDocs(ctr).FDFGetFile.Length > 0 Then
                            Dim pdfByte() As Byte
                            Dim MemStream As New MemoryStream
                            pdfByte = PDFMergeFDF2Buf(FDFDocs(ctr), FDFDocs(ctr).FDFGetFile, Flatten)
                            MemStream.Write(pdfByte, 0, pdfByte.Length - 1)
                            reader = New iTextSharp.text.pdf.PdfReader(MemStream.ToArray)
                            copy.AddDocument(reader)
                        End If
                    End If
                    ctr += 1
                Next

                copy.Writer.CloseStream = False

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 1)
                FileStream.Close()
                Return False
            End Try

            Try

                If Not FileStream Is Nothing Then
                    FileStream.Close()
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 2)
                FileStream.Close()
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Combines PDF Forms and merges the XDP data and outputs to a file
        ''' </summary>
        ''' <param name="FDFDocs">PDFDoc_Classes array - FDFSetFile must be set for each</param>
        ''' <param name="newPDFFile">New PDF File</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDPConcatenateForms2File(ByVal FDFDocs() As FDFApp.FDFDoc_Class, ByVal newPDFFile As String, Optional ByVal Flatten As Boolean = False) As Boolean
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim FileStream As New FileStream(newPDFFile, FileMode.Create, FileAccess.Write, FileShare.Write)
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(FileStream)
            Try
                Dim ctr As Integer = 0
                For Each FDFDoc As FDFApp.FDFDoc_Class In FDFDocs
                    If Not FDFDoc.FDFGetFile Is Nothing Then
                        If FDFDocs(ctr).FDFGetFile.Length > 0 Then
                            Dim pdfByte() As Byte
                            Dim MemStream As New MemoryStream
                            pdfByte = PDFMergeXDP2Buf(FDFDocs(ctr), FDFDocs(ctr).FDFGetFile, Flatten)
                            MemStream.Write(pdfByte, 0, pdfByte.Length - 1)
                            If MemStream.CanSeek Then
                                MemStream.Position = 0
                            End If
                            reader = New iTextSharp.text.pdf.PdfReader(MemStream.ToArray)
                            copy.AddDocument(reader)
                        End If
                    End If
                    ctr += 1
                Next
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 1)
                FileStream.Close()
                Return False
            End Try
            Try

                If Not FileStream Is Nothing Then
                    FileStream.Close()
                    FileStream.Dispose()
                    Return True
                Else
                    Return False
                End If
                copy.Writer.CloseStream = False
            Catch ex As Exception
                copy.Writer.CloseStream = False
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 2)
                FileStream.Close()
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Combines PDF Files and then merges with data and outputs to a file
        ''' </summary>
        ''' <param name="PDFFileNames">PDF File names array</param>
        ''' <param name="FDFDocs">FDFDoc_Class array</param>
        ''' <param name="newPDFFile">New PDF File</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDFConcatenateForms2File(ByVal PDFFileNames() As String, ByVal FDFDocs() As FDFApp.FDFDoc_Class, ByVal newPDFFile As String, Optional ByVal Flatten As Boolean = False) As Boolean
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim FileStream As New FileStream(newPDFFile, FileMode.Create, FileAccess.Write, FileShare.Write)
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(FileStream)
            Try
                Dim ctr As Integer = 0
                For Each FDFDoc As FDFApp.FDFDoc_Class In FDFDocs
                    If Not PDFFileNames(ctr) Is Nothing Then
                        If Not PDFFileNames(ctr) Is Nothing And PDFFileNames(ctr).Length > 0 Then
                            Dim pdfByte() As Byte
                            Dim MemStream As New MemoryStream
                            pdfByte = PDFMergeFDF2Buf(FDFDocs(ctr), PDFFileNames(ctr), Flatten)
                            MemStream.Write(pdfByte, 0, pdfByte.Length - 1)
                            If MemStream.CanSeek Then
                                MemStream.Position = 0
                            End If
                            reader = New iTextSharp.text.pdf.PdfReader(MemStream)
                            copy.AddDocument(reader)
                        ElseIf Not FDFDocs(ctr) Is Nothing And Not FDFDocs(ctr).FDFGetFile Is Nothing Then
                            If FDFDocs(ctr).FDFGetFile.Length > 0 Then
                                Dim pdfByte() As Byte
                                Dim MemStream As New MemoryStream
                                pdfByte = PDFMergeFDF2Buf(FDFDocs(ctr), FDFDocs(ctr).FDFGetFile, Flatten)
                                MemStream.Write(pdfByte, 0, pdfByte.Length - 1)
                                If MemStream.CanSeek Then
                                    MemStream.Position = 0
                                End If
                                reader = New iTextSharp.text.pdf.PdfReader(MemStream)
                                copy.AddDocument(reader)
                            End If
                        End If
                    End If
                    ctr += 1
                Next
                copy.Writer.CloseStream = False

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 1)
                FileStream.Close()
                Return False
            End Try

            Try
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
            End Try

            Try

                If Not FileStream Is Nothing Then
                    FileStream.Close()
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 2)
                FileStream.Close()
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Combines PDF Files and then merges with XDP data and outputs to a file
        ''' </summary>
        ''' <param name="PDFFileNames">PDF File names array</param>
        ''' <param name="FDFDocs">FDFDoc_Class array</param>
        ''' <param name="newPDFFile">New PDF File</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>returns true</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDPConcatenateForms2File(ByVal PDFFileNames() As String, ByVal FDFDocs() As FDFApp.FDFDoc_Class, ByVal newPDFFile As String, Optional ByVal Flatten As Boolean = False) As Boolean
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim FileStream As New FileStream(newPDFFile, FileMode.Create, FileAccess.Write, FileShare.Write)
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(FileStream)
            Try
                Dim ctr As Integer = 0
                For Each FDFDoc As FDFApp.FDFDoc_Class In FDFDocs
                    If Not PDFFileNames(ctr) Is Nothing Then
                        If Not PDFFileNames(ctr) Is Nothing And PDFFileNames(ctr).Length > 0 Then
                            Dim pdfByte() As Byte
                            Dim MemStream As New MemoryStream
                            pdfByte = PDFMergeXDP2Buf(FDFDocs(ctr), PDFFileNames(ctr), Flatten)
                            MemStream.Write(pdfByte, 0, pdfByte.Length - 1)
                            If MemStream.CanSeek Then
                                MemStream.Position = 0
                            End If
                            reader = New iTextSharp.text.pdf.PdfReader(MemStream)
                            copy.AddDocument(reader)
                        ElseIf Not FDFDocs(ctr) Is Nothing And Not FDFDocs(ctr).FDFGetFile Is Nothing Then
                            If FDFDocs(ctr).FDFGetFile.Length > 0 Then
                                Dim pdfByte() As Byte
                                Dim MemStream As New MemoryStream
                                pdfByte = PDFMergeXDP2Buf(FDFDocs(ctr), FDFDocs(ctr).FDFGetFile, Flatten)
                                MemStream.Write(pdfByte, 0, pdfByte.Length - 1)
                                If MemStream.CanSeek Then
                                    MemStream.Position = 0
                                End If
                                reader = New iTextSharp.text.pdf.PdfReader(MemStream.ToArray)
                                copy.AddDocument(reader)
                            End If
                        End If
                    End If
                    ctr += 1
                Next
                copy.Writer.CloseStream = False

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 1)
                FileStream.Close()
                Return False
            End Try

            Try
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
            End Try

            Try

                If Not FileStream Is Nothing Then
                    FileStream.Close()
                    FileStream.Dispose()
                    Return True
                Else
                    Return False
                End If
                'IOPerm.RevertAssert()
            Catch ex As Exception
                'IOPerm.RevertAssert()
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 2)
                FileStream.Close()
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Combines PDFs and merges with data and outputs to a byte array
        ''' </summary>
        ''' <param name="FDFDocs">FDFDoc_Class array - FDFSetFile must be set for each</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>returns byte array</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDFConcatenateForms2Buf(ByVal FDFDocs() As FDFApp.FDFDoc_Class, Optional ByVal Flatten As Boolean = False) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim MemStream As New MemoryStream
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(MemStream)
            Try
                Dim ctr As Integer = 0
                For Each FDFDoc As FDFApp.FDFDoc_Class In FDFDocs
                    If Not FDFDocs(ctr) Is Nothing And FDFDocs(ctr).FDFGetFile.Length > 0 Then
                        Dim pdfByte() As Byte = PDFMergeFDF2Buf(FDFDocs(ctr), FDFDocs(ctr).FDFGetFile, Flatten)
                        Dim MemStreamTmp As New MemoryStream
                        MemStreamTmp.Write(pdfByte, 0, pdfByte.Length)
                        If MemStreamTmp.CanSeek Then
                            MemStreamTmp.Position = 0
                        End If
                        reader = New iTextSharp.text.pdf.PdfReader(MemStreamTmp.ToArray)
                        copy.AddDocument(reader)
                        reader.Close()
                    End If
                    ctr += 1
                Next
                copy.Writer.CloseStream = False
            Catch ex As Exception
                'File.Delete(newPDFFile & ".tmp")

                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 1)
                Return Nothing
            End Try

            Try
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
            End Try

            Try
                If Not MemStream Is Nothing Then
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2Buf", 2)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Combines PDFs and merges with XDP data and outputs to a byte array
        ''' </summary>
        ''' <param name="FDFDocs">FDFDoc_Class array - FDFSetFile must be set for each</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>returns byte array</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDPConcatenateForms2Buf(ByVal FDFDocs() As FDFApp.FDFDoc_Class, Optional ByVal Flatten As Boolean = False) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim MemStream As New MemoryStream
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(MemStream)
            Try
                ' COPY EACH FORM TO NEW FILE IN ORDER OF FILE NAME
                Dim ctr As Integer = 0
                For Each FDFDoc As FDFApp.FDFDoc_Class In FDFDocs
                    If Not FDFDocs(ctr) Is Nothing And FDFDocs(ctr).FDFGetFile.Length > 0 Then
                        Dim pdfByte() As Byte
                        Dim MemStreamTmp As New MemoryStream
                        '
                        pdfByte = PDFMergeXDP2Buf(FDFDocs(ctr), FDFDocs(ctr).FDFGetFile, Flatten)
                        MemStreamTmp.Write(pdfByte, 0, pdfByte.Length - 1)
                        reader = New iTextSharp.text.pdf.PdfReader(MemStreamTmp.ToArray)
                        copy.AddDocument(reader)
                    End If
                    ctr += 1
                Next
                copy.Writer.CloseStream = False
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 1)
                Return Nothing
            End Try

            Try
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
            End Try

            Try
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2Buf", 2)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Combines PDFs and merges with data and outputs to a byte array
        ''' </summary>
        ''' <param name="PDFFileNames">PDF File names string array</param>
        ''' <param name="FDFDocs">FDFDoc_Class array</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>return byte array</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDFConcatenateForms2Buf(ByVal PDFFileNames() As String, ByVal FDFDocs() As FDFApp.FDFDoc_Class, Optional ByVal Flatten As Boolean = False) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim MemStream As New MemoryStream
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(MemStream)
            Try
                Dim ctr As Integer = 0
                For Each FileNm As String In PDFFileNames
                    If Not FileNm Is Nothing And FileNm.Length > 0 Then
                        Dim pdfByte() As Byte
                        Dim MemStreamTmp As New MemoryStream
                        pdfByte = PDFMergeFDF2Buf(FDFDocs(ctr), FileNm, Flatten)
                        MemStreamTmp.Write(pdfByte, 0, pdfByte.Length - 1)
                        reader = New iTextSharp.text.pdf.PdfReader(MemStreamTmp.ToArray)
                        copy.AddDocument(reader)
                    ElseIf Not FDFDocs(ctr) Is Nothing And FDFDocs(ctr).FDFGetFile.Length > 0 Then
                        Dim pdfByte() As Byte
                        Dim MemStreamTmp As New MemoryStream
                        pdfByte = PDFMergeFDF2Buf(FDFDocs(ctr), FDFDocs(ctr).FDFGetFile, Flatten)
                        MemStreamTmp.Write(pdfByte, 0, pdfByte.Length - 1)
                        reader = New iTextSharp.text.pdf.PdfReader(MemStreamTmp.ToArray)
                        copy.AddDocument(reader)
                    End If
                    ctr += 1
                Next
                copy.Writer.CloseStream = False
            Catch ex As Exception
                Return Nothing
            End Try

            Try
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
            End Try

            Try
                If Not MemStream Is Nothing Then
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2Buf", 2)
                Return Nothing
            End Try


        End Function
        ''' <summary>
        ''' Combines PDFs and merges with XDP data and outputs to a byte array
        ''' </summary>
        ''' <param name="PDFFileNames">PDF File names string array</param>
        ''' <param name="FDFDocs">FDFDoc_Class array</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <returns>return byte array</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDPConcatenateForms2Buf(ByVal PDFFileNames() As String, ByVal FDFDocs() As FDFApp.FDFDoc_Class, Optional ByVal Flatten As Boolean = False) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            Dim MemStream As New MemoryStream
            Dim copy As New iTextSharp.text.pdf.PdfCopyFields(MemStream)
            Try
                Dim ctr As Integer = 0
                For Each FileNm As String In PDFFileNames
                    If Not FileNm Is Nothing And FileNm.Length > 0 Then
                        Dim pdfByte() As Byte
                        Dim MemStreamTmp As New MemoryStream
                        pdfByte = PDFMergeXDP2Buf(FDFDocs(ctr), FileNm, Flatten)
                        MemStreamTmp.Write(pdfByte, 0, pdfByte.Length - 1)
                        reader = New iTextSharp.text.pdf.PdfReader(MemStreamTmp.ToArray)
                        copy.AddDocument(reader)
                    ElseIf Not FDFDocs(ctr) Is Nothing And FDFDocs(ctr).FDFGetFile.Length > 0 Then
                        Dim pdfByte() As Byte
                        Dim MemStreamTmp As New MemoryStream
                        pdfByte = PDFMergeXDP2Buf(FDFDocs(ctr), FDFDocs(ctr).FDFGetFile, Flatten)
                        MemStreamTmp.Write(pdfByte, 0, pdfByte.Length - 1)
                        reader = New iTextSharp.text.pdf.PdfReader(MemStreamTmp.ToArray)
                        copy.AddDocument(reader)
                    End If
                    ctr += 1
                Next
                copy.Writer.CloseStream = False

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDFConcatenateForms2File", 1)
                Return Nothing
            End Try

            Try
                copy.Close()
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2File", 1)
            End Try

            Try
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFConcatenateForms2Buf", 2)
                Return Nothing
            End Try


        End Function
        ''' <summary>
        ''' Merges PDF Document and FDF Data and outputs to a stream
        ''' </summary>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="PDFFormPath">Original blank PDF Form Path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption Strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>stream</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Stream(ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Stream
            Dim formFile As String = PDFFormPath
            If formFile = "" Then
                If _FDF(0).FileName = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = _FDF(0).FileName & ""
                End If
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Set_PDF_Fields_Merge(reader, stamper)
                Dim fields As iTextSharp.text.pdf.AcroFields

                fields = stamper.AcroFields
                Dim FDFDoc As FDFDoc_Class = _FDF(0)
                Dim FDFApp As New FDFApp_Class
                Dim FDFFields() As FDFApp.FDFDoc_Class.FDFField = FDFGetFields()
                'Dim FDFField As FDFApp.FDFDoc_Class.FDFField
                Dim xFld As New iTextSharp.text.pdf.AcroFields.Item
                Dim retString As String = ""
                If HasDocJavaScripts() Or HasDocOnImportJavaScripts() Then
                    retString = ""
                    If HasDocJavaScripts() Then
                        retString = retString & GetDocJavaScripts()
                        If HasDocOnImportJavaScripts() Then
                            retString = retString & Me.FDFGetImportJSActions(False)
                            retString = FDFCheckCharReverse(retString)
                            Dim writer As iTextSharp.text.pdf.PdfWriter
                            writer = stamper.Writer
                            Dim JSAction As iTextSharp.text.pdf.PdfAction = iTextSharp.text.pdf.PdfAction.JavaScript(retString, writer)
                            writer.AddJavaScript(JSAction)
                        End If
                    Else
                        If HasDocOnImportJavaScripts() Then
                            retString = Me.FDFGetImportJSActions(True, True)
                            retString = FDFCheckCharReverse(retString)
                            Dim writer As iTextSharp.text.pdf.PdfWriter
                            writer = stamper.Writer
                            Dim JSAction As iTextSharp.text.pdf.PdfAction = iTextSharp.text.pdf.PdfAction.JavaScript(retString, writer)
                            writer.AddJavaScript(JSAction)
                        End If
                    End If

                End If
                For Each _fld As FDFField In FDFFields
                    xFld = New iTextSharp.text.pdf.AcroFields.Item
                    If Not String_IsNullOrEmpty(_fld.FieldName) Then
                        xFld = fields.GetFieldItem(_fld.FieldName)
                        If Not xFld Is Nothing Then
                            If Not String_IsNullOrEmpty(_fld.FieldName) Then
                                If Not _fld.FieldValue.Count <= 0 Then
                                    If Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                        If _fld.FieldValue.Count = _fld.DisplayValue.Count Then
                                            fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray))
                                        End If
                                    ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                        If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                            If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.DisplayValue.ToArray), FDFCheckCharReverse2(_fld.DisplayName.ToArray))
                                            End If
                                            fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                        ElseIf Not _fld.FieldValue.Count <= 0 Then
                                            If _fld.FieldValue.Count = 1 Then
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                            ElseIf _fld.FieldValue.Count > 0 Then
                                                If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                    fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray))
                                                End If
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            End If
                                        End If
                                    ElseIf Not _fld.FieldValue.Count <= 0 And Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                        If _fld.FieldValue.Count = _fld.DisplayValue.Count Then
                                            If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray))
                                            Else
                                                fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                            End If
                                        End If
                                    ElseIf Not _fld.FieldValue.Count <= 0 Then
                                        If _fld.FieldValue.Count = 1 Then
                                            fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                        ElseIf _fld.FieldValue.Count > 0 Then
                                            If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray))
                                            End If
                                            fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                        End If
                                    End If

                                ElseIf Not _fld.DisplayValue.Count <= 0 And Not _fld.DisplayName.Count <= 0 Then
                                    If _fld.DisplayName.Count = _fld.DisplayValue.Count Then
                                        If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                            fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.DisplayValue.ToArray), FDFCheckCharReverse2(_fld.DisplayName.ToArray))
                                        End If
                                        fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                    ElseIf Not _fld.FieldValue.Count <= 0 Then
                                        If _fld.FieldValue.Count = 1 Then
                                            fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0) & ""))
                                        ElseIf _fld.FieldValue.Count > 0 Then
                                            If (fields.GetFieldType(_fld.FieldName) = 5 Or fields.GetFieldType(_fld.FieldName) = 6) And (_fld.FieldValue.Count = _fld.DisplayName.Count) And (_fld.FieldValue.Count >= 1) Then
                                                fields.SetListOption(_fld.FieldName & "", FDFCheckCharReverse2(_fld.FieldValue.ToArray()), FDFCheckCharReverse2(_fld.DisplayName.ToArray))
                                            End If
                                            fields.SetField(_fld.FieldName & "", FDFCheckCharReverse(_fld.FieldValue(0)))
                                        End If
                                    End If

                                End If
                            End If
                        End If
                    End If

                Next
                PDF_iTextSharp_SetSubmitButtonURLs(stamper, reader)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    If MemStream.CanSeek Then MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Stream", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges PDF Document with FDF Data and outputs to a byte array
        ''' </summary>
        ''' <param name="OpenPassword">PDF Open Password</param>
        ''' <param name="ModificationPassword">PDF Modify Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="PDFFormPath">Original Blank PDF Form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption Strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>byte array</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeFDF2Buf(ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Byte()
            Dim formFile As String = PDFFormPath
            If formFile = "" Then
                If _FDF(0).FileName = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = _FDF(0).FileName & ""
                End If
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Set_PDF_Fields_Merge(reader, stamper)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeFDF2Buf", 1)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Current FDF Document
        ''' </summary>
        ''' <value></value>
        ''' <returns>FDFDoc_Class object</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FDFDocument() As FDFDoc_Class
            Get
                Return _FDF(_CurFDFDoc)
            End Get
        End Property
        ''' <summary>
        ''' FDFErrors
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Property FDFErrors() As FDFErrors
            Get
                Return _FDFErrors
            End Get
            Set(ByVal Value As FDFErrors)
                _FDFErrors = Value
            End Set
        End Property
        ''' <summary>
        ''' FDF has errors
        ''' </summary>
        ''' <returns>true if it has errors</returns>
        ''' <remarks></remarks>
        Public Function FDFHasErrors() As Boolean
            Return _FDFErrors.FDFHasErrors
        End Function
        ''' <summary>
        ''' Resets errors
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub ResetErrors()
            _FDFErrors.ResetErrors()
        End Sub
        ''' <summary>
        ''' Returns FDFErrors array
        ''' </summary>
        ''' <value></value>
        ''' <returns>FDFErrors object</returns>
        ''' <remarks></remarks>
        Public Property FDFDocErrors() As FDFErrors
            Get
                Return _FDFErrors
            End Get
            Set(ByVal Value As FDFErrors)
                _FDFErrors = Value
            End Set
        End Property

        ''' <summary>
        ''' Returns FDF Errors in a string
        ''' </summary>
        ''' <param name="HTMLFormat">If true it will return the string in html format</param>
        ''' <returns>String of errors</returns>
        ''' <remarks></remarks>
        Public Function FDFDocErrorsStr(Optional ByVal HTMLFormat As Boolean = False) As String
            Dim FDFErrors As FDFErrors
            Dim FDFError As FDFErrors.FDFError
            FDFErrors = _FDFErrors
            Dim retString As String
            retString = IIf(HTMLFormat, "<br>", vbNewLine) & "FDFDoc Errors:"
            For Each FDFError In FDFErrors.FDFErrors
                retString = retString & IIf(HTMLFormat, "<br>", vbNewLine) & vbTab & "Error: " & FDFError.FDFError & IIf(HTMLFormat, "<br>", vbNewLine) & vbTab & "#: " & FDFError.FDFError_Number & IIf(HTMLFormat, "<br>", vbNewLine) & vbTab & "Module: " & FDFError.FDFError_Module & IIf(HTMLFormat, "<br>", vbNewLine) & vbTab & "Message: " & FDFError.FDFError_Msg & IIf(HTMLFormat, "<br>", vbNewLine)
            Next
            Return retString
        End Function
        ''' <summary>
        ''' Has changes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FDFHasChanges() As Boolean
            Get
                Return _FDF(_CurFDFDoc).HasChanges
            End Get
            Set(ByVal Value As Boolean)
                Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                _nFDF.HasChanges = Value
                _FDF(_CurFDFDoc) = _nFDF
            End Set
        End Property
        ''' <summary>
        ''' Append saves
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FDFAppendSaves() As String
            Get
                If _FDF(_CurFDFDoc).AppendSaves = "" Then
                    Return GetChanges() & ""
                End If
                Return _FDF(_CurFDFDoc).AppendSaves
            End Get
            Set(ByVal Value As String)
                Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                _nFDF.AppendSaves = Value
                _FDF(_CurFDFDoc) = _nFDF
            End Set
        End Property
        ''' <summary>
        ''' FDF Version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Version() As String
            Get
                Return _FDF(_CurFDFDoc).Version
            End Get
            Set(ByVal Value As String)
                Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                _nFDF.Version = Value
                _FDF(_CurFDFDoc) = _nFDF
            End Set
        End Property

        ''' <summary>
        ''' Differences
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Differences() As String
            Get
                Return _FDF(_CurFDFDoc).Differences
            End Get
            Set(ByVal Value As String)
                Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                _nFDF.Differences = Value
                _FDF(_CurFDFDoc) = _nFDF
            End Set
        End Property
        ''' <summary>
        ''' Annotations
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Annotations() As String
            Get
                Return _FDF(_CurFDFDoc).Annotations
            End Get
            Set(ByVal Value As String)
                Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                _nFDF.Annotations = Value
                _FDF(_CurFDFDoc) = _nFDF
            End Set
        End Property
        ''' <summary>
        ''' FDF Data
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FDFData() As String
            Get
                If _FDF(_CurFDFDoc).FDFData Is Nothing Or _FDF(_CurFDFDoc).FDFData = WriteHead(FDFType.FDF, False) Then
                    Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                    _nFDF.FDFData = FDFSavetoStr(FDFType.FDF, True)
                    _FDF(_CurFDFDoc) = _nFDF
                End If
                Return _FDF(_CurFDFDoc).FDFData
            End Get
            Set(ByVal Value As String)
                Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                _nFDF.FDFData = Value
                _FDF(_CurFDFDoc) = _nFDF
            End Set
        End Property
        Public Property XDPData() As String
            Get
                If _FDF(_CurFDFDoc).FDFData Is Nothing Or _FDF(_CurFDFDoc).FDFData = WriteHead(FDFType.FDF, False) Then
                    Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                    _nFDF.XDPData = FDFSavetoStr(FDFType.XDP, True)
                    _FDF(_CurFDFDoc) = _nFDF
                End If
                Return _FDF(_CurFDFDoc).XDPData
            End Get
            Set(ByVal Value As String)
                Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                _nFDF.XDPData = FDFSavetoStr(FDFType.XDP, True)
                _FDF(_CurFDFDoc) = _nFDF
            End Set
        End Property

        ''' <summary>
        ''' PDF Data
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PDFData() As Byte()
            Get
                Return _FDF(_CurFDFDoc).PDFData
            End Get
            Set(ByVal Value() As Byte)
                Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                _nFDF.PDFData = Value
                _FDF(_CurFDFDoc) = _nFDF
            End Set
        End Property


        ''' <summary>
        ''' PDF File name referenced in the FDF Document
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Property FDFFileName() As String
            Get
                Return _FDF(_CurFDFDoc).FileName
            End Get
            Set(ByVal Value As String)
                Dim _nFDF As FDFDoc_Class = _FDF(_CurFDFDoc)
                _nFDF.FileName = Value
                _FDF(_CurFDFDoc) = _nFDF
            End Set
        End Property

        ''' <summary>
        ''' Set status response for FDF Document
        ''' </summary>
        ''' <param name="Value">Response to send to client</param>
        ''' <param name="ReplaceStatus">Replaces current status</param>
        ''' <remarks></remarks>
        Public Sub FDFSetStatus(ByVal Value As String, Optional ByVal ReplaceStatus As Boolean = True)
            If ReplaceStatus Then
                _FDF(0).Status = Value
            Else
                Dim _nFDF As FDFDoc_Class = _FDF(0)
                _nFDF.Status = _FDF(0).Status & "\n" & Value
                _FDF(0) = _nFDF
            End If
        End Sub
        Private Sub FDFAddTemplate(ByVal bNewPage As Boolean, ByVal bstrFileName As String, ByVal bstrTemplateName As String, ByVal bRename As Boolean)
            Dim _FDFX As New FDFDoc_Class
            _FDFX.FileName = bstrFileName
            _FDFX.TmpTemplateName = bstrTemplateName
            _FDFX.TmpNewPage = bNewPage
            _FDFX.TmpRename = bRename
            _FDFX.DocType = FDFDocType.FDFTemplate
            _CurFDFDoc += 1
            _FDFX.struc_FDFFields.Clear()
            _FDFX.struc_FDFActions.Clear()
            _FDFX.struc_DocScript.Clear()
            _FDF.Add(_FDFX)
        End Sub

        ''' <summary>
        ''' Set FDF Version
        ''' </summary>
        ''' <param name="bstrVersion"></param>
        ''' <remarks></remarks>
        Public Sub FDFSetFDFVersion(ByVal bstrVersion As String)
            Dim _FDFX As FDFDoc_Class = _FDF(_CurFDFDoc)
            _FDFX.Version = bstrVersion
            _FDF(_CurFDFDoc) = _FDFX
        End Sub
        ''' <summary>
        ''' Get FDF status message
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FDFGetStatus() As String
            Get
                Return _FDF(0).Status
            End Get
        End Property
        ''' <summary>
        ''' Returns FDF Documents field count
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FDFFieldCount() As Integer
            If _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                Return 0
            Else
                Return _FDF(_CurFDFDoc).struc_FDFFields.Count
            End If
        End Function

        ''' <summary>
        ''' Returns XDP Documents field count
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function XDPFieldCount() As Integer
            If XDPGetFields() Is Nothing Then
                Return 0
            Else
                Dim cntr As Integer = 0
                For Each fld As FDFField In XDPGetFields()
                    If Not fld.FieldName Is Nothing Then
                        If Not String_IsNullOrEmpty(fld.FieldName & "") Then
                            cntr += 1
                        End If
                    End If
                Next
                Return cntr
            End If
        End Function

        ''' <summary>
        ''' Set or Gets FDFField Objects array
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FDFFields() As FDFField()
            Get
                Return _FDF(_CurFDFDoc).struc_FDFFields.ToArray
            End Get
            Set(ByVal Value As FDFField())
                _FDF(_CurFDFDoc).struc_FDFFields.Clear()
                _FDF(_CurFDFDoc).struc_FDFFields.AddRange(Value)
            End Set
        End Property
        ''' <summary>
        ''' Returns FDF Object count
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FDFObjectCount() As Integer
            If _FDFObjects(0).objNum Is Nothing Then
                Return 0
            Else
                Return _FDFObjects.Count
            End If
        End Function

        ''' <summary>
        ''' Return FDF Objects array
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FDFGetObjects() As FDFObjects()
            Get
                Return _FDFObjects.ToArray
            End Get
            Set(ByVal Value As FDFObjects())
                _FDFObjects.Clear()
                _FDFObjects.AddRange(Value)
            End Set
        End Property
        ''' <summary>
        ''' Returns FDFobject
        ''' </summary>
        ''' <param name="objNum"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FDFGetObject(ByVal objNum As String) As FDFObjects
            Dim xFDFObj As FDFObjects, intNum As Integer
            intNum = 0
            Try
                For Each xFDFObj In Me._FDFObjects
                    If xFDFObj.objNum.ToLower = objNum.ToLower Then
                        Return _FDFObjects(intNum)
                        Exit Function
                    End If
                    intNum = intNum + 1
                Next

                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFGetObject", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Private Property xFDFField(ByVal FieldName As String, Optional ByVal FieldNum As Integer = -1) As FDFField
            Get
                FoundField = False
                For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                    If xField.FieldName = FieldName Then
                        Return xField
                        FoundField = True
                    End If
                Next
                If Not FoundField Then
                    If FieldNum > -1 Then
                        Return _FDF(_CurFDFDoc).struc_FDFFields(FieldNum)
                    End If
                End If
                Return Nothing
            End Get

            Set(ByVal xFDFField As FDFField)
                FoundField = False
                For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                    If xField.FieldName = xFDFField.FieldName Then
                        xField.FieldValue = xFDFField.FieldValue
                        FoundField = True
                    End If
                Next
                If Not FoundField Then
                    FDFAddField(xFDFField.FieldName, xFDFField.FieldValue.ToArray)
                End If
            End Set
        End Property
        ''' <summary>
        ''' Sets the target frame for FDF Document, similar to HTML target object
        ''' </summary>
        ''' <value>_blank, _self, _top, _bottom, _(framename)</value>
        ''' <remarks></remarks>
        Public WriteOnly Property FDFSetTargetFrame() As String
            Set(ByVal Value As String)
                Dim _FDFX As FDFDoc_Class = _FDF(_CurFDFDoc)
                _FDFX.TargetFrame = Value & ""
                _FDF(_CurFDFDoc) = _FDFX
            End Set
        End Property
        ''' <summary>
        ''' Returns the current target frame
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FDFGetTargetFrame() As String
            Get
                Return _FDF(_CurFDFDoc).TargetFrame & ""
            End Get
        End Property
        ''' <summary>
        ''' Sets PDF File path for FDF Document
        ''' </summary>
        ''' <param name="bstrNewFile"></param>
        ''' <remarks></remarks>
        Public Sub FDFSetFile(ByVal bstrNewFile As String)
            _PDF.FileName = bstrNewFile & ""
        End Sub

        ''' <summary>
        ''' Gets PDF File path for FDF Document
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FDFGetFile() As String
            Get
                Return _PDF.FileName & ""
            End Get
        End Property



        Private Function FileNameCheck(ByVal strFileName As String) As String
            Dim tmpScript As String
            If strFileName.Length <= 0 Then
                Return ""
                Exit Function
            End If
            tmpScript = strFileName
            Dim str_bld As New StringBuilder(tmpScript)
            str_bld = str_bld.Replace("  ", " ")
            str_bld = str_bld.Replace("(", "")
            str_bld = str_bld.Replace(")", "")
            str_bld = str_bld.Replace("" & Chr(13), "")
            str_bld = str_bld.Replace("" & Chr(10), "")
            str_bld = str_bld.Replace(vbNewLine, "")
            str_bld = str_bld.Replace("'", "")
            str_bld = str_bld.Replace("", "")
            str_bld = str_bld.Replace("*", "")
            str_bld = str_bld.Replace("|", "")
            str_bld = str_bld.Replace("{", "")
            str_bld = str_bld.Replace("}", "")
            str_bld = str_bld.Replace("!", "")
            str_bld = str_bld.Replace("@", "")
            str_bld = str_bld.Replace("#", "")
            str_bld = str_bld.Replace("$", "")
            str_bld = str_bld.Replace("%", "")
            str_bld = str_bld.Replace("^", "")
            str_bld = str_bld.Replace("&", "")
            str_bld = str_bld.Replace("*", "")
            str_bld = str_bld.Replace("(", "")
            str_bld = str_bld.Replace("_", "_")
            str_bld = str_bld.Replace("-", "_")
            str_bld = str_bld.Replace("+", "")
            str_bld = str_bld.Replace("=", "")
            str_bld = str_bld.Replace("[", "")
            str_bld = str_bld.Replace("]", "")
            str_bld = str_bld.Replace(":", "")
            str_bld = str_bld.Replace(";", "")
            str_bld = str_bld.Replace("/", "\")
            str_bld = str_bld.Replace(",", "")
            str_bld = str_bld.Replace("?", "")
            str_bld = str_bld.Replace("<", "")
            str_bld = str_bld.Replace(">", "")
            str_bld = str_bld.Replace(".", "")
            str_bld = str_bld.Replace("  ", " ")
            Return str_bld.ToString & ""
        End Function
        Private Function FDFCheckChar(ByVal strINPUT As String) As String
            If strINPUT.Length <= 0 Then
                Return ""
                Exit Function
            End If
            strINPUT = strINPUT.Replace("\(", "(")
            strINPUT = strINPUT.Replace("\)", "\")
            strINPUT = strINPUT.Replace("\'", "'")
            strINPUT = strINPUT.Replace("\", "'")
            strINPUT = strINPUT.Replace("\\", "\")

            strINPUT = strINPUT.Replace("\", "\\")
            strINPUT = strINPUT.Replace("(", "\(")
            strINPUT = strINPUT.Replace(")", "\)")
            strINPUT = strINPUT.Replace(vbNewLine, "\r")
            strINPUT = strINPUT.Replace(Environment.NewLine, "\r")
            strINPUT = strINPUT.Replace(Chr(13), "\r")
            strINPUT = strINPUT.Replace(Chr(10), "\r")
            strINPUT = strINPUT.Replace("'", "\'")
            strINPUT = strINPUT.Replace("", "\'")
            Return strINPUT & ""

        End Function

        Private Function FDFCheckCharReverse(ByVal strINPUT As String) As String
            If strINPUT.Length <= 0 Then
                Return ""
                Exit Function
            End If
            Dim str_bld As String = strINPUT
            str_bld = str_bld.Replace("\\", "\")
            str_bld = str_bld.Replace("\(", "(")
            str_bld = str_bld.Replace("\)", ")")
            str_bld = str_bld.Replace("\r", vbNewLine)  ' \r\n
            str_bld = str_bld.Replace("\" & Environment.NewLine, vbNewLine)    ' \r\n
            str_bld = str_bld.Replace("\" & Chr(13), vbNewLine)      ' \r\n
            str_bld = str_bld.Replace("\" & Chr(10), vbNewLine)      ' \r\n
            str_bld = str_bld.Replace("\'", "'")      ' \r\n
            str_bld = str_bld.Replace("\", "'")      ' \r\n
            Return str_bld.ToString
        End Function
        Private Function FDFCheckCharReverse2(ByVal strINPUT As String()) As String()
            Dim tmpScript As String
            If strINPUT.Length <= 0 Then
                Return Nothing
                Exit Function
            End If
            Dim strOutput(strINPUT.Length - 1) As String, index As Integer
            For Each tmpScript In strINPUT
                tmpScript = tmpScript.Replace("\\", "\")
                tmpScript = tmpScript.Replace("\(", "(")
                tmpScript = tmpScript.Replace("\)", ")")
                tmpScript = tmpScript.Replace("\'", "'")
                tmpScript = tmpScript.Replace("\", "'")
                tmpScript = tmpScript.Replace("\" & Environment.NewLine, vbNewLine)      ' \r\n
                tmpScript = tmpScript.Replace("\" & Chr(13), vbNewLine)      ' \r\n
                tmpScript = tmpScript.Replace("\" & Chr(10), vbNewLine)      ' \r\n
                tmpScript = tmpScript.Replace("\r", vbNewLine)    ' \r\n
                strOutput(index) = tmpScript & ""
                index += 1
            Next
            Return strOutput
        End Function
        ''' <summary>
        ''' Set Javascript action
        ''' </summary>
        ''' <param name="FieldName">PDF Field name</param>
        ''' <param name="whichTrigger">Trigger</param>
        ''' <param name="theScript">Script to set</param>
        ''' <remarks></remarks>
        Public Sub FDFSetJavaScriptAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger, ByVal theScript As String)
            Dim tmpScript As String
            tmpScript = Me.FDFCheckCharReverse(theScript)
            tmpScript = Me.FDFCheckChar(tmpScript)
            theScript = tmpScript

            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each _fld As FDFActions In _FDF(_CurFDFDoc).struc_FDFActions
                        If _fld.FieldName = FieldName And (whichTrigger = FDFActionTrigger.FDFUp And _fld.Trigger = whichTrigger) Then       'And struc_FDFActions(fldCnt).ActionType = ActionTypes.JavaScript 
                            _fld.Trigger = whichTrigger
                            _fld.JavaScript_URL = theScript
                            _fld.ActionType = ActionTypes.JavaScript
                            FoundField = True
                            Exit Sub
                        End If
                    Next
                End If
                If Not FoundField Then
                    FDFAddJSAction(FieldName, whichTrigger, theScript)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub
        ''' <summary>
        ''' Sets Submit Form Action for a field
        ''' </summary>
        ''' <param name="FieldName">PDF Field Name</param>
        ''' <param name="whichTrigger">Trigger</param>
        ''' <param name="URL">Submit to url</param>
        ''' <remarks></remarks>
        Public Sub FDFSetSubmitFormAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger, ByVal URL As String)
            Dim FoundField As Boolean
            FoundField = False
            'Dim fldCnt As Integer
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each _fldA As FDFActions In _FDF(_CurFDFDoc).struc_FDFActions
                        If _fldA.FieldName = FieldName And (whichTrigger = FDFActionTrigger.FDFUp And _fldA.Trigger = FDFActionTrigger.FDFUp) Then       'And struc_FDFActions(fldCnt).ActionType = ActionTypes.Submit 
                            _fldA.Trigger = whichTrigger
                            _fldA.JavaScript_URL = URL
                            _fldA.ActionType = ActionTypes.Submit
                            FoundField = True
                            Exit Sub
                        End If
                    Next
                End If
                If Not FoundField Then
                    FDFAddSubmitAction(FieldName, whichTrigger, URL)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub

        ''' <summary>
        ''' Sets Submit Form Action for a XDP field
        ''' </summary>
        ''' <param name="FieldName">PDF Field Name</param>
        ''' <param name="whichTrigger">Trigger</param>
        ''' <param name="URL">Submit to url</param>
        ''' <remarks></remarks>
        Private Sub XDPSetSubmitFormAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger, ByVal URL As String)
            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not _FDF(_CurFDFDoc).struc_XDPActions Is Nothing Then
                    For Each _fldA As XDPActions In _FDF(_CurFDFDoc).struc_XDPActions
                        If _fldA.FieldName = FieldName And (whichTrigger = FDFActionTrigger.FDFUp And _fldA.Trigger = FDFActionTrigger.FDFUp) Then       'And struc_XDPActions(fldCnt).ActionType = ActionTypes.Submit 
                            _fldA.Trigger = whichTrigger
                            _fldA.JavaScript_URL = URL
                            _fldA.ActionType = ActionTypes.Submit
                            FoundField = True
                            Exit Sub
                        ElseIf _fldA.FieldName = FieldName And (Not whichTrigger = FDFActionTrigger.FDFUp And Not _fldA.Trigger = FDFActionTrigger.FDFUp) Then       'And struc_XDPActions(fldCnt).ActionType = ActionTypes.Submit 
                            _fldA.Trigger = whichTrigger
                            _fldA.JavaScript_URL = URL
                            _fldA.ActionType = ActionTypes.Submit
                            FoundField = True
                            Exit Sub
                        End If
                    Next
                End If
                If Not FoundField Then
                    FDFAddSubmitAction(FieldName, whichTrigger, URL)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub


        ''' <summary>
        ''' Set value of field
        ''' </summary>
        ''' <param name="FieldName">PDF Field name</param>
        ''' <param name="FieldValue">PDF Field value</param>
        ''' <param name="FDFEmpty">Leave empty or set to false</param>
        ''' <param name="FieldEnabled">Set to true to enable field</param>
        ''' <remarks></remarks>
        Public Sub FDFSetValue(ByVal FieldName As String, ByVal FieldValue As String, Optional ByVal FDFEmpty As Boolean = False, Optional ByVal FieldEnabled As Boolean = True)
            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields

                        If _fld.FieldName = FieldName Then
                            _fld.FieldValue.Clear()
                            _fld.FieldValue.Add(Me.FDFCheckChar(FieldValue))
                            _fld.FieldEnabled = FieldEnabled
                            FoundField = True
                            Exit Sub
                        End If
                    Next
                End If
                If Not FoundField Then
                    FDFAddField(FieldName, (FieldValue))
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub
        ''' <summary>
        ''' Set value of field
        ''' </summary>
        ''' <param name="FieldName">PDF Field name</param>
        ''' <param name="FieldValue">PDF Field value</param>
        ''' <remarks></remarks>
        Public Sub FDFSetValue(ByVal FieldName As String, ByVal FieldValue As Object)
            Dim FoundField As Boolean
            FoundField = False
            Try
                Dim FieldValueStr As String = CStr(FieldValue & "") & ""
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                        If _fld.FieldName = FieldName Then
                            _fld.FieldValue.Clear()
                            _fld.FieldValue.Add(Me.FDFCheckChar(FieldValueStr))
                            _fld.FieldEnabled = True
                            FoundField = True
                            Exit Sub
                        End If
                    Next
                End If
                If Not FoundField Then
                    FDFAddField(FieldName, (FieldValue))
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub
        ''' <summary>
        ''' Set value of field in a Live-Cycle Form
        ''' </summary>
        ''' <param name="FieldName">Live-Cycle PDF Form Field Name</param>
        ''' <param name="FieldValue">Live-Cycle PDF Form Field Value</param>
        ''' <param name="FormName">Live-Cycle PDF Form Name</param>
        ''' <param name="FDFEmpty">Leave blank or set to false</param>
        ''' <param name="FieldEnabled">Set to true to enable field</param>
        ''' <remarks></remarks>
        Public Sub XDPSetValue(ByVal FieldName As String, ByVal FieldValue As String, ByVal FormName As String, Optional ByVal FDFEmpty As Boolean = False, Optional ByVal FieldEnabled As Boolean = True)
            Dim FoundField As Boolean
            FoundField = False
            Try
                Dim xdpFrm As New FDFDoc_Class
                xdpFrm = XDPForm(FormName)
                If Not xdpFrm.FormName Is Nothing Then
                    If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                        If xdpFrm.struc_FDFFields.Count >= 1 Then
                            For Each _fld As FDFField In xdpFrm.struc_FDFFields
                                If Not String_IsNullOrEmpty(_fld.FieldName) Then
                                    If _fld.FieldName.ToLower = FieldName.ToLower Then
                                        _fld.FieldValue.Clear()
                                        _fld.FieldValue.Add((FieldValue))
                                        _fld.FieldEnabled = FieldEnabled
                                        FoundField = True
                                        Exit Sub
                                    End If
                                End If
                            Next
                        End If
                    End If
                    If Not FoundField Then
                        XDPAddField(FieldName, FieldValue, _FDF(_CurFDFDoc).FormName)
                    End If
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub
        ''' <summary>
        ''' Set value of field in a Live-Cycle Form
        ''' </summary>
        ''' <param name="FieldName">Live-Cycle PDF Form Field Name</param>
        ''' <param name="FieldValue">Live-Cycle PDF Form Field Value</param>
        ''' <param name="FDFEmpty">Leave blank or set to false</param>
        ''' <param name="FieldEnabled">Set to true to enable field</param>
        ''' <remarks></remarks>
        Public Sub XDPSetValue(ByVal FieldName As String, ByVal FieldValue As String, Optional ByVal FDFEmpty As Boolean = False, Optional ByVal FieldEnabled As Boolean = True)
            Dim FoundField As Boolean
            ''EDITED BY NK-INC 11/30/2011
            FoundField = False
            Try
                Dim xdpFrm As New FDFDoc_Class
                Dim TmpCurFDFDoc As Integer = 0
                If _FDF.Count > 0 Then
                    'If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName) Then
                    Dim intx As Integer = -1
                    For Each _fdfdoc As FDFDoc_Class In _FDF
                        intx += 1
                        If Not _fdfdoc.struc_FDFFields.Count <= 0 Then
                            If _fdfdoc.struc_FDFFields.Count >= 1 Then      '_FDF(XDPFDF).DocType = FDFDocType.XDPForm And 
                                For Each _fld As FDFField In _fdfdoc.struc_FDFFields
                                    If Not String_IsNullOrEmpty(_fld.FieldName) Then
                                        If _fld.FieldName.ToLower = FieldName.ToLower Then
                                            '_CurFDFDoc = intx
                                            _fld.FieldValue.Clear()
                                            _fld.FieldValue.Add((FieldValue))
                                            _fld.FieldEnabled = FieldEnabled
                                            FoundField = True
                                            'Exit Sub
                                        End If
                                    End If
                                Next
                            End If
                        End If
                        'End If
                    Next
                    If FoundField = False Then
                        GoTo continue_setting_value
                    Else
                        Exit Sub
                    End If
                Else
                    If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then
                        GoTo continue_setting_value
                    End If
                End If
                'End If
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field (" & FieldName & ") Not Found", "FDFDoc.FDFSetValue", 1)
                Exit Sub
continue_setting_value:
                If Not FoundField Then
                    XDPAddField(FieldName, FieldValue, _FDF(_CurFDFDoc).FormName)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub
        ''' <summary>
        ''' Set value of field in a Live-Cycle Form
        ''' </summary>
        ''' <param name="FieldName">Live-Cycle PDF Form Field Name</param>
        ''' <param name="FieldValue">Live-Cycle PDF Form Field Value</param>
        ''' <param name="FDFEmpty">Leave blank or set to false</param>
        ''' <param name="FieldEnabled">Set to true to enable field</param>
        ''' <remarks></remarks>
        Public Sub XDPSetValue(ByVal FieldName As String, ByVal FieldNumber As Integer, ByVal FieldValue As String, Optional ByVal FDFEmpty As Boolean = False, Optional ByVal FieldEnabled As Boolean = True)
            Dim FoundField As Boolean
            FoundField = False
            Try
                Dim xdpFrm As New FDFDoc_Class
                Dim TmpCurFDFDoc As Integer = 0
                If _FDF.Count > 0 Then
                    If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName) Then
                        If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then
                            GoTo continue_setting_value
                        End If
                    Else
                        Dim intX As Integer = -1
                        For Each _fdfdoc As FDFDoc_Class In _FDF
                            intX += 1
                            If Not _fdfdoc.struc_FDFFields.Count <= 0 Then
                                If _fdfdoc.struc_FDFFields.Count >= 1 Then
                                    For Each _fld As FDFField In _fdfdoc.struc_FDFFields
                                        If Not String_IsNullOrEmpty(_fld.FieldName) Then
                                            If _fld.FieldName.ToLower = FieldName.ToLower Then
                                                If _fld.FieldNum = FieldNumber Then
                                                    _fld.FieldValue.Clear()
                                                    _fld.FieldValue.Add((FieldValue))
                                                    _fld.FieldEnabled = FieldEnabled
                                                    FoundField = True
                                                    Exit Sub
                                                End If
                                                _CurFDFDoc = intX
                                                GoTo continue_setting_value
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        Next
                        GoTo continue_setting_value
                    End If
                End If
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field (" & FieldName & ") Not Found", "FDFDoc.FDFSetValue", 1)
                Exit Sub
continue_setting_value:
                If Not FoundField Then
                    XDPAddField(FieldName, FieldValue, _FDF(_CurFDFDoc).FormName)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub

        ''' <summary>
        ''' Set value of field in a Live-Cycle Form
        ''' </summary>
        ''' <param name="FieldName">Live-Cycle PDF Form Field Name</param>
        ''' <param name="FieldValue">Live-Cycle PDF Form Field Value</param>
        ''' <param name="FormNumber">Form number</param>
        ''' <param name="FDFEmpty">Leave blank or set to false</param>
        ''' <param name="FieldEnabled">Set to true to enable field</param>
        ''' <remarks></remarks>
        Public Sub XDPSetValue(ByVal FieldName As String, ByVal FieldValue As String, ByVal FormNumber As Integer, Optional ByVal FDFEmpty As Boolean = False, Optional ByVal FieldEnabled As Boolean = True)
            Dim FoundField As Boolean
            FoundField = False
            Try
                Dim xdpFrm As New FDFDoc_Class
                xdpFrm = XDPForm(FormNumber)
                If Not xdpFrm.FormName Is Nothing Then
                    If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                        If xdpFrm.struc_FDFFields.Count >= 1 Then
                            For Each _fld As FDFField In xdpFrm.struc_FDFFields
                                If Not String_IsNullOrEmpty(_fld.FieldName) Then
                                    If _fld.FieldName.ToLower = FieldName.ToLower Then
                                        _fld.FieldValue.Clear()
                                        _fld.FieldValue.Add((FieldValue))
                                        _fld.FieldEnabled = FieldEnabled
                                        FoundField = True
                                        Exit Sub
                                    End If
                                End If
                            Next
                        End If
                    End If
                    If Not FoundField Then
                        XDPAddField(FieldName, FieldValue, _FDF(_CurFDFDoc).FormName)
                    End If
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub
        Private Sub FDFSetValueByte(ByVal FieldName As String, ByVal FieldValue As Byte(), Optional ByVal FDFEmpty As Boolean = False, Optional ByVal FieldEnabled As Boolean = True)
            Dim FoundField As Boolean
            FoundField = False
            Try
                Dim SB As New StringBuilder, strTmp As String = ""
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                        If _fld.FieldName = FieldName Then
                            _fld.FieldValue.Clear()
                            For Each xByte As Byte In FieldValue
                                strTmp = Chr(xByte)
                                SB.Append(strTmp)
                            Next
                            _fld.FieldValue.Add(Me.FDFCheckChar(SB.ToString))
                            _fld.FieldEnabled = FieldEnabled
                            FoundField = True
                            Exit Sub
                        End If
                    Next
                End If
                If Not FoundField Then
                    For Each xByte As Byte In FieldValue
                        strTmp = xByte.ToString
                        SB.Append(strTmp)
                    Next
                    FDFAddField(FieldName, Me.FDFCheckChar(SB.ToString))
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try
        End Sub
        ''' <summary>
        ''' Sets values of list or multi-select form field
        ''' </summary>
        ''' <param name="FieldName">PDF Form Field name</param>
        ''' <param name="FieldValues">PDF Form field values array</param>
        ''' <param name="FieldEnabled">Set to true to enable form field</param>
        ''' <remarks></remarks>
        Public Sub FDFSetValues(ByVal FieldName As String, ByVal FieldValues() As String, Optional ByVal FieldEnabled As Boolean = True)

            Dim FoundField As Boolean
            FoundField = False
            Dim FieldValue As String
            Dim strValue As String = ""
            Try
                If _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 And Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then

                    For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                        If _fld.FieldName = FieldName Then
                            If FieldValues.Length > 0 Then
                                _fld.FieldValue.Clear()
                                _fld.FieldValue.AddRange(FieldValues)
                                _fld.FieldEnabled = FieldEnabled
                                _fld.FieldType = FieldType.FldMultiSelect
                                FoundField = True
                                Exit Sub
                            Else
                                FoundField = False
                            End If
                        End If
                    Next
                End If
                If Not FoundField And FieldValues.Length > 0 Then
                    FieldValue = strValue
                    FDFAddField(FieldName, FieldValues, FieldType.FldMultiSelect, FieldEnabled)
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub
        ''' <summary>
        ''' Sets list form field display and values
        ''' </summary>
        ''' <param name="FieldName">PDF Form field name</param>
        ''' <param name="FieldValue">PDF Form field value</param>
        ''' <param name="DisplayName">Display array for items</param>
        ''' <param name="FieldEnabled">Set to true to enable</param>
        ''' <remarks></remarks>
        Public Sub FDFSetOpt(ByVal FieldName As String, ByVal FieldValue As String, ByVal DisplayName As String, Optional ByVal FieldEnabled As Boolean = True)

            Dim FoundField As Boolean
            FoundField = False
            Try

                If FieldName.Length > 0 Then
                    FieldName = FieldName & ""
                Else
                    FieldName = FieldValue & ""
                End If

                If FieldValue.Length > 0 Then
                    FieldValue = FieldValue & ""
                Else
                    FieldValue = FieldName & ""
                End If

                If FieldValue = "" And FieldName = "" Then
                    Exit Sub
                End If

                If _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 And Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                        If _fld.FieldName = FieldName Then
                            If _fld.DisplayValue.Count > 0 Then
                                _fld.FieldValue.Add(FieldValue)
                                _fld.DisplayName.Add(DisplayName)
                                _fld.FieldEnabled = FieldEnabled
                                _fld.FieldType = FieldType.FldOption
                                FoundField = True
                                Exit Sub
                            Else
                                FoundField = False
                            End If
                        End If
                    Next
                End If
                If Not FoundField Then
                    FDFAddOptField(FieldName, FieldValue, DisplayName, FieldEnabled)
                    Exit Sub
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub
        ''' <summary>
        ''' Sets list form field display and values
        ''' </summary>
        ''' <param name="FieldName">PDF Form field name</param>
        ''' <param name="FieldValue">PDF FOrm field values array</param>
        ''' <param name="DisplayName">PDF Form field display item array</param>
        ''' <param name="FieldEnabled">Set to true to enable field</param>
        ''' <remarks></remarks>
        Public Sub FDFSetOpt(ByVal FieldName As String, Optional ByVal FieldValue() As String = Nothing, Optional ByVal DisplayName() As String = Nothing, Optional ByVal FieldEnabled As Boolean = True)

            Dim FoundField As Boolean
            FoundField = False
            Try

                If DisplayName Is Nothing Or FieldValue Is Nothing Then
                    Exit Sub
                End If

                If FieldName.Length > 0 Then
                    FieldName = FieldName & ""
                Else
                    FieldName = DisplayName(0) & ""
                End If


                If FieldValue Is Nothing And FieldName = "" Then
                    Exit Sub
                End If
                If _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 And Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                        If _fld.FieldName = FieldName Then
                            If _fld.DisplayValue.Count > 0 Then
                                If FieldValue.Length <> DisplayName.Length Then
                                    Exit Sub
                                Else
                                    _fld.DisplayValue.Clear()
                                    _fld.DisplayName.Clear()
                                End If
                                _fld.DisplayValue.AddRange(FieldValue)
                                _fld.DisplayName.AddRange(DisplayName)
                                _fld.FieldEnabled = FieldEnabled
                                _fld.FieldType = FieldType.FldOption
                                FoundField = True
                                Exit Sub
                            Else
                                FoundField = False
                            End If
                        End If
                    Next
                End If
                If Not FoundField Then
                    FDFAddOptField(FieldName, FieldValue, DisplayName, FieldEnabled)
                    Exit Sub
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFSetValue", 1)
                Exit Sub
            End Try

        End Sub
        ''' <summary>
        ''' Populates a Dataset form PDF Form values
        ''' </summary>
        ''' <param name="Ds">Dataset to set</param>
        ''' <param name="Row_Number">DataRow number</param>
        ''' <returns>Populated dataset</returns>
        ''' <remarks></remarks>
        Public Function FDFSetDataSetFromValues(ByVal Ds As DataSet, Optional ByVal Row_Number As Integer = 0) As DataSet
            Dim Fieldname As String
            Dim DsFld As DataColumn
            Try
                For Each DsFld In Ds.Tables(0).Columns
                    FoundField = False
                    Fieldname = DsFld.ColumnName & ""
                    If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 And DsFld.AutoIncrement = False Then
                        If Not DsFld.DataType Is Nothing And Row_Number < Ds.Tables(0).Rows.Count Then
                            Dim xfld As FDFApp.FDFDoc_Class.FDFField() = Me.FDFGetFields(Fieldname)
                            If Not xfld Is Nothing And xfld(0).FieldEnabled = True Then
                                Select Case LCase(DsFld.DataType.ToString & "")
                                    Case "system.dbnull"
                                        Ds.Tables(0).Rows(Row_Number)(Fieldname) = Me.XDPGetValue(Fieldname) & ""
                                    Case "system.sbyte"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CByte(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.empty"
                                        Ds.Tables(0).Rows(Row_Number)(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                    Case "system.object"
                                        Ds.Tables(0).Rows(Row_Number)(Fieldname) = Me.FDFGetValue(Fieldname)
                                    Case "system.string"
                                        Ds.Tables(0).Rows(Row_Number)(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                    Case "system.char"
                                        Ds.Tables(0).Rows(Row_Number)(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                    Case "system.character"
                                        Ds.Tables(0).Rows(Row_Number)(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                    Case "system.boolean"
                                        Ds.Tables(0).Rows(Row_Number)(Fieldname) = CBool(Me.FDFGetValue(Fieldname)) + 0
                                    Case "system.datetime"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CDate(IIf(IsDate(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0))
                                        End If

                                    Case "system.date"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CDate(IIf(IsDate(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0))
                                        End If
                                    Case "system.time"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CDate(IIf(IsDate(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0))
                                        End If
                                    Case "system.single"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CSng(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.byte"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CByte(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.double"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CDbl(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.int16"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.int32"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.int64"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.uint16"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.uint32"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.uint64"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                    Case "system.decimal"
                                        If Me.FDFGetValue(Fieldname) = "" Then
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = System.DBNull.Value
                                        Else
                                            Ds.Tables(0).Rows(Row_Number)(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                        End If
                                End Select
                            End If
                        End If
                    End If
                Next
                Return Ds
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetValuesFromDataSet", 1)
                Return Ds
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Sets a datarow with values from PDF Form values
        ''' </summary>
        ''' <param name="Dr">Datarow to populate</param>
        ''' <returns>Populated datarow</returns>
        ''' <remarks></remarks>
        Public Function FDFSetDataRowFromValues(ByRef Dr As DataRow) As DataRow
            Dim Fieldname As String

            Dim DsFld As DataColumn
            Try
                For Each DsFld In Dr.Table.Columns
                    FoundField = False
                    Fieldname = DsFld.ColumnName & ""
                    If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 And DsFld.AutoIncrement = False Then
                        If Not Me.FDFGetValue(Fieldname) Is Nothing Then
                            If Not DsFld.DataType Is Nothing Then
                                Dim xfld As FDFApp.FDFDoc_Class.FDFField() = Me.FDFGetFields(Fieldname)
                                If Not xfld Is Nothing And xfld(0).FieldEnabled = True Then
                                    Select Case LCase(DsFld.DataType.ToString & "")
                                        Case "system.dbnull"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.sbyte"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CByte(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.empty"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.object"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname)
                                        Case "system.string"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.char"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.character"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.boolean"
                                            Dr(Fieldname) = CBool(Me.FDFGetValue(Fieldname)) + 0
                                        Case "system.datetime"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CDate(IIf(IsDate(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0))
                                            End If

                                        Case "system.date"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CDate(IIf(IsDate(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0))
                                            End If
                                        Case "system.time"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CDate(IIf(IsDate(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0))
                                            End If
                                        Case "system.single"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CSng(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.byte"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CByte(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.double"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CDbl(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.int16"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.int32"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.int64"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.uint16"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.uint32"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.uint64"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.decimal"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                    End Select
                                End If
                            End If
                        End If
                    End If
                Next
                Return Dr
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetValuesFromDataRow", 1)
                Return Dr
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Sets datarow values with PDF Form field values, and excludes Auto Increment field
        ''' </summary>
        ''' <param name="Dr">Datarow to populate</param>
        ''' <param name="AutoIncrementField">Datarow column name to exclude</param>
        ''' <returns>Populated datarow</returns>
        ''' <remarks></remarks>
        Public Function FDFSetDataRowFromValues(ByVal Dr As DataRow, ByVal AutoIncrementField As String) As DataRow
            Dim Fieldname As String
            Dim DsFld As DataColumn
            Try
                For Each DsFld In Dr.Table.Columns
                    FoundField = False
                    Fieldname = DsFld.ColumnName & ""
                    If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 And Fieldname.ToLower <> AutoIncrementField.ToLower Then
                        If Not DsFld.DataType Is Nothing Then
                            If Not Me.FDFGetValue(Fieldname) Is Nothing Then
                                Dim xfld As FDFApp.FDFDoc_Class.FDFField() = Me.FDFGetFields(Fieldname)
                                If Not xfld Is Nothing And xfld(0).FieldEnabled = True Then
                                    Select Case LCase(DsFld.DataType.ToString & "")
                                        Case "system.dbnull"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.sbyte"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CByte(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.empty"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.object"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname)
                                        Case "system.string"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.char"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.character"
                                            Dr(Fieldname) = Me.FDFGetValue(Fieldname) & ""
                                        Case "system.boolean"
                                            Dr(Fieldname) = CBool(Me.FDFGetValue(Fieldname)) + 0
                                        Case "system.datetime"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CDate(IIf(IsDate(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0))
                                            End If

                                        Case "system.date"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CDate(IIf(IsDate(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0))
                                            End If
                                        Case "system.time"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CDate(IIf(IsDate(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0))
                                            End If
                                        Case "system.single"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CSng(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.byte"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CByte(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.double"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CDbl(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.int16"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.int32"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.int64"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.uint16"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.uint32"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.uint64"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                        Case "system.decimal"
                                            If Me.FDFGetValue(Fieldname) = "" Then
                                                Dr(Fieldname) = System.DBNull.Value
                                            Else
                                                Dr(Fieldname) = CInt(IIf(IsNumeric(Me.FDFGetValue(Fieldname)), Me.FDFGetValue(Fieldname), 0) + 0)
                                            End If
                                    End Select
                                End If
                            End If
                        End If
                    End If
                Next
                Return Dr
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetValuesFromDataRow", 1)
                Return Dr
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Sets FDF Data values from Dataset values
        ''' </summary>
        ''' <param name="Ds">Data set to get data from</param>
        ''' <param name="OptionNames">Sets FDF Form fields as options fields</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FDFSetValuesFromDataSet(ByVal Ds As DataSet, Optional ByVal OptionNames() As String = Nothing) As String
            Dim FieldValue As String
            Dim Fieldname As String
            Dim FieldTypes As String
            Dim FieldEnabled As Boolean
            Dim DsFld As DataColumn
            Dim Options As String, bFoundOption As Boolean
            Dim ReturnString As String = ""
            Try
                For Each DsFld In Ds.Tables(0).Columns
                    FoundField = False
                    Fieldname = DsFld.ColumnName & ""
                    If Not IsDBNull(Ds.Tables(0).Rows(0)(Fieldname)) Then
                        FieldValue = CStr(Ds.Tables(0).Rows(0)(Fieldname)) & ""
                    Else
                        FieldValue = ""
                    End If
                    If Not DsFld.DataType Is Nothing Then
                        FieldTypes = DsFld.DataType.ToString & ""
                        ReturnString = ReturnString & Fieldname & ";" & FieldValue & ";" & FieldTypes & vbNewLine
                    Else
                        FieldTypes = "UNKNOWN"
                        ReturnString = ReturnString & Fieldname & ";" & FieldValue & ";" & FieldTypes & vbNewLine
                    End If

                    FieldEnabled = True
                    If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                        For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                            If _fld.FieldName = Fieldname Then
                                _fld.FieldValue.Clear()
                                _fld.FieldValue.Add(FieldValue)
                                _fld.FieldEnabled = FieldEnabled
                                If Not OptionNames Is Nothing Then
                                    For Each Options In OptionNames
                                        If Options = Fieldname Then
                                            bFoundOption = True
                                        End If
                                    Next
                                End If
                                If bFoundOption Then
                                    _fld.FieldType = FieldType.FldOption
                                    bFoundOption = False
                                Else
                                    _fld.FieldType = FieldType.FldTextual
                                End If
                                FoundField = True
                                Return Nothing
                                Exit Function
                            Else
                                bFoundOption = False
                                FoundField = False
                            End If
                        Next
                    End If
                    If Not FoundField Then
                        If Not OptionNames Is Nothing Then
                            For Each Options In OptionNames
                                If Options = Fieldname Then
                                    bFoundOption = True
                                End If
                            Next
                        End If
                        If bFoundOption Then
                            FDFAddField(Fieldname, FieldValue, FieldType.FldOption, FieldEnabled)
                            bFoundOption = False
                        Else
                            FDFAddField(Fieldname, FieldValue, FieldType.FldTextual, FieldEnabled)
                        End If
                    End If
                Next

                FDFData = FDFSavetoStr(FDFType.FDF, True)
                Return FDFData
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetValuesFromDataSet", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Populates Data set from Excel spreadsheet document
        ''' </summary>
        ''' <param name="FileName">Excel document</param>
        ''' <param name="RangeName">Range Name of data</param>
        ''' <returns>Populated Dataset</returns>
        ''' <remarks></remarks>
        Public Function GetDataFromExcel(ByVal FileName As String, ByVal RangeName As String) As System.Data.DataSet
            Try
                Dim strConn As String = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & FileName & "; Extended Properties=Excel 8.0;"
                Dim objConn As New System.Data.OleDb.OleDbConnection(strConn)
                objConn.Open()
                Dim objCmd As New System.Data.OleDb.OleDbCommand("SELECT * FROM " & RangeName, objConn)
                Dim objDA As New System.Data.OleDb.OleDbDataAdapter
                objDA.SelectCommand = objCmd
                Dim objDS As New System.Data.DataSet
                objDA.Fill(objDS)
                objConn.Close()
                Return objDS
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.GetDataFromExcel", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Creates HTML form from FDF Data
        ''' </summary>
        ''' <param name="method">Post/Get</param>
        ''' <param name="action">Form action</param>
        ''' <param name="target">target frame of form</param>
        ''' <param name="runat">server</param>
        ''' <returns>HTML form populated with PDF Field values</returns>
        ''' <remarks></remarks>
        Public Function FDFCreateHTMLFormFromFDF(Optional ByVal method As String = "POST", Optional ByVal action As String = "", Optional ByVal target As String = "_self", Optional ByVal runat As String = "server") As String
            Dim xField As FDFField
            Dim FieldValue As String
            Dim Fieldname As String
            Dim ReturnString As String = ""
            Try
                ReturnString = "<form name=""fdfappnet"" id=""fdfappnet"" method=""" & method & """ action=""" & action & """ target=""" & target & """ runat=""" & runat & """>"
                ReturnString &= "<table>"
                For CurFDFDoc As Integer = 0 To _FDF.Count - 1
                    If Not _FDF(CurFDFDoc).struc_FDFFields.Count <= 0 Then
                        For Each xField In _FDF(CurFDFDoc).struc_FDFFields
                            FoundField = False
                            Fieldname = xField.FieldName & ""
                            If Not xField.DisplayName.Count <= 0 Or Not xField.DisplayValue.Count <= 0 And xField.FieldValue.Count <= 0 Then
                                If xField.DisplayName(0).Length > 0 Then
                                    ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td>"
                                    ReturnString &= Chr(10) & "<select id=""" & Fieldname & """ name=""" & Fieldname & """ runat=""" & runat & """>"
                                    If xField.DisplayName.Count > 1 Then
                                        If xField.DisplayName.Count = xField.DisplayValue.Count Then
                                            For intX As Integer = 0 To xField.DisplayName.Count - 1
                                                ReturnString &= Chr(10) & "<option value=""" & xField.DisplayValue(intX) & """>" & xField.DisplayName(intX) & "</option>"
                                            Next
                                        ElseIf xField.DisplayName.Count = xField.FieldValue.Count Then
                                            For intX As Integer = 0 To xField.DisplayName.Count - 1
                                                ReturnString &= Chr(10) & "<option value=""" & xField.FieldValue(intX) & """>" & xField.DisplayName(intX) & "</option>"
                                            Next
                                        ElseIf xField.DisplayName.Count > 1 And (xField.DisplayValue.Count <= 1 Or xField.FieldValue.Count <= 1) Then
                                            For intX As Integer = 0 To xField.DisplayName.Count - 1
                                                ReturnString &= Chr(10) & "<option value=""" & xField.DisplayName(intX) & """>" & xField.DisplayName(intX) & "</option>"
                                            Next
                                        End If
                                    End If
                                ElseIf xField.FieldValue.Count > 1 Then
                                    If xField.DisplayName.Count = xField.DisplayValue.Count Then
                                        For intX As Integer = 0 To xField.DisplayName.Count - 1
                                            ReturnString &= Chr(10) & "<option value=""" & xField.DisplayValue(intX) & """>" & xField.DisplayName(intX) & "</option>"
                                        Next
                                    ElseIf xField.DisplayName.Count = xField.FieldValue.Count Then
                                        For intX As Integer = 0 To xField.DisplayName.Count - 1
                                            ReturnString &= Chr(10) & "<option value=""" & xField.FieldValue(intX) & """>" & xField.DisplayName(intX) & "</option>"
                                        Next
                                    ElseIf xField.FieldValue.Count > 1 And (xField.DisplayName.Count <= 1 Or xField.DisplayValue.Count <= 1) Then
                                        For intX As Integer = 0 To xField.DisplayName.Count - 1
                                            ReturnString &= Chr(10) & "<option value=""" & xField.FieldValue(intX) & """>" & xField.FieldValue(intX) & "</option>"
                                        Next
                                    End If
                                Else
                                    If Not IsDBNull(xField.FieldValue(0)) Then
                                        FieldValue = CStr(xField.FieldValue(0)) & ""
                                    Else
                                        FieldValue = ""
                                    End If
                                    If FieldValue.Length > 255 Then
                                        ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><textarea id=""" & Fieldname & """ rows=""10"" cols=""50"" name=""" & Fieldname & """ runat=""" & runat & """>" & FieldValue & "</textarea></td></tr>"
                                    ElseIf InStr(FieldValue & "", Chr(10)) Or InStr(FieldValue & "", Chr(13)) Or InStr(FieldValue & "", vbCrLf) Or InStr(FieldValue & "", vbNewLine) Then
                                        ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><textarea id=""" & Fieldname & """ rows=""10"" cols=""50"" name=""" & Fieldname & """ runat=""" & runat & """>" & FieldValue & "</textarea></td></tr>"
                                    Else
                                        ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><input type=""text"" id=""" & Fieldname & """ name=""" & Fieldname & """ value=""" & FieldValue & "" & """ runat=""" & runat & """></td></tr>"
                                    End If
                                End If
                                ReturnString &= Chr(10) & "</select></td></tr>"
                            Else
                                If Not xField.FieldValue.Count <= 0 Then
                                    FieldValue = CStr(xField.FieldValue(0)) & ""
                                Else
                                    FieldValue = ""
                                End If
                                If Not String_IsNullOrEmpty(xField.FieldName) Then
                                    If FieldValue.Length > 255 Then
                                        ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><textarea id=""" & Fieldname & """ rows=""10"" cols=""50"" name=""" & Fieldname & """ runat=""" & runat & """>" & FieldValue & "</textarea></td></tr>"
                                    ElseIf InStr(FieldValue & "", Chr(10)) Or InStr(FieldValue & "", Chr(13)) Or InStr(FieldValue & "", vbCrLf) Or InStr(FieldValue & "", vbNewLine) Then
                                        ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><textarea id=""" & Fieldname & """ rows=""10"" cols=""50"" name=""" & Fieldname & """ runat=""" & runat & """>" & FieldValue & "</textarea></td></tr>"
                                    Else
                                        ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><input type=""text"" id=""" & Fieldname & """ name=""" & Fieldname & """ value=""" & FieldValue & "" & """ runat=""" & runat & """></td></tr>"
                                    End If
                                End If
                            End If
                        Next
                    End If
                Next
                ReturnString &= Chr(10) & "<tr colspan=2><td><input type=""submit"" id=""submit"" name=""submit"" value=""Submit"" runat=""" & runat & """>   <input type=""reset"" id=""reset"" name=""reset"" value=""Reset"" runat=""" & runat & """></td></tr>"
                ReturnString &= Chr(10) & "</table>"
                ReturnString &= Chr(10) & "</form>"
                Return ReturnString
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetValuesFromDataSet", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Creates HTML Form from Datarow
        ''' </summary>
        ''' <param name="Dr">Datarow with data</param>
        ''' <param name="method">Post/Get</param>
        ''' <param name="action">HTML form action</param>
        ''' <param name="target">HTML form target frame</param>
        ''' <param name="runat">server</param>
        ''' <returns>Populated HTML form with values from datarow</returns>
        ''' <remarks></remarks>
        Public Function FDFCreateHTMLFormFromDataRow(ByVal Dr As DataRow, Optional ByVal method As String = "POST", Optional ByVal action As String = "", Optional ByVal target As String = "_self", Optional ByVal runat As String = "server") As String
            Dim FieldValue As String
            Dim Fieldname As String
            Dim DsFld As DataColumn
            Dim ReturnString As String = ""
            Try
                ReturnString = "<form name=""fdfappnet"" id=""fdfappnet"" method=""" & method & """ action=""" & action & """ target=""" & target & """ runat=""" & runat & """>"
                ReturnString &= "<table>"
                For Each DsFld In Dr.Table.Columns
                    FoundField = False
                    Fieldname = DsFld.ColumnName & ""
                    If Not IsDBNull(Dr(Fieldname)) Then
                        FieldValue = CStr(Dr(Fieldname)) & ""
                    Else
                        FieldValue = ""
                    End If
                    Select Case DsFld.DataType.ToString.ToLower
                        Case "system.string"
                            If DsFld.MaxLength > 255 Then
                                ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><textarea id=""" & Fieldname & """ rows=""20"" cols=""70"" name=""" & Fieldname & """ runat=""" & runat & """>" & FieldValue & "</textarea></td></tr>"
                            Else
                                ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><input type=""text"" id=""" & Fieldname & """ name=""" & Fieldname & """ value=""" & FieldValue & "" & """ runat=""" & runat & """></td></tr>"
                            End If
                        Case "system.char"
                            ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><input type=""text"" id=""" & Fieldname & """ name=""" & Fieldname & """ value=""" & FieldValue & "" & """ runat=""" & runat & """></td></tr>"
                        Case "system.boolean"
                            ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><select id=""" & Fieldname & """ name=""" & Fieldname & """ runat=""" & runat & """>" & IIf(FieldValue.ToLower = "true", "<option SELECTED>True</option><option>False</option>", "<option>True</option><option SELECTED>False</option>") & "</select>" & """></td></tr>"
                        Case "system.integer", "system.int16", "system.int32", "system.int64", "system.long", "system.single", "system.byte", "system.bit", "system.short", "system.decimal" Or "system.double"
                            ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><input type=""text"" id=""" & Fieldname & """ name=""" & Fieldname & """ value=""" & FieldValue & "" & """ runat=""" & runat & """></td></tr>"
                        Case "system.binary"
                            ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><input type=""file"" id=""" & Fieldname & """ name=""" & Fieldname & """ runat=""" & runat & """></td></tr>"
                        Case "system.datetime", "system.date", "system.time"
                            ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><input type=""text"" id=""" & Fieldname & """ name=""" & Fieldname & """ value=""" & FieldValue & "" & """ runat=""" & runat & """></td></tr>"
                        Case Else
                            If DsFld.MaxLength > 255 Then
                                ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><textarea id=""" & Fieldname & """ rows=""20"" cols=""70"" name=""" & Fieldname & """ runat=""" & runat & """>" & FieldValue & "</textarea></td></tr>"
                            Else
                                ReturnString &= Chr(10) & "<tr><td>" & Fieldname & "</td><td><input type=""text"" id=""" & Fieldname & """ name=""" & Fieldname & """ value=""" & FieldValue & "" & """ runat=""" & runat & """></td></tr>"
                            End If
                    End Select
                Next
                ReturnString &= Chr(10) & "<tr colspan=2><td><input type=""submit"" id=""submit"" name=""submit"" value=""Submit"" runat=""" & runat & """>   <input type=""reset"" id=""reset"" name=""reset"" value=""Reset"" runat=""" & runat & """></td></tr>"
                ReturnString &= Chr(10) & "</table>"
                ReturnString &= Chr(10) & "</form>"
                Return ReturnString
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetValuesFromDataSet", 1)
                Return Nothing
                Exit Function
            End Try
        End Function

        ''' <summary>
        ''' Creates FDF Form from HTML form submission data
        ''' </summary>
        ''' <param name="Page">Me.Page</param>
        ''' <param name="OptionNames">Option Names</param>
        ''' <returns>True</returns>
        ''' <remarks></remarks>
        Public Function FDFCreateFDFFromHTMLForm(ByVal Page As System.Web.UI.Page, Optional ByVal OptionNames() As String = Nothing) As Boolean
            Dim FieldValue As String
            Dim Fieldname As String
            Dim FieldEnabled As Boolean
            Dim Options As String, bFoundOption As Boolean
            Try
                For Each Fieldname In Page.Request.Form
                    FieldEnabled = True
                    FieldValue = Page.Request.Form.Item(Fieldname) & ""
                    If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                        For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                            If _fld.FieldName = Fieldname Then
                                _fld.FieldValue.Clear()
                                _fld.FieldValue.Add(FieldValue)
                                _fld.FieldEnabled = FieldEnabled
                                If Not OptionNames Is Nothing Then
                                    For Each Options In OptionNames
                                        If Options = Fieldname Then
                                            bFoundOption = True
                                        End If
                                    Next
                                End If
                                If bFoundOption Then
                                    _fld.FieldType = FieldType.FldOption
                                    bFoundOption = False
                                Else
                                    _fld.FieldType = FieldType.FldTextual
                                End If
                                FoundField = True
                                Exit Function
                            Else
                                bFoundOption = False
                                FoundField = False
                            End If
                        Next
                    Else
                        FoundField = False
                    End If
                    If Not FoundField Then
                        If Not Fieldname & "" = "submit" Then
                            If Not OptionNames Is Nothing Then
                                For Each Options In OptionNames
                                    If Options = Fieldname Then
                                        bFoundOption = True
                                    End If
                                Next
                            End If

                            If bFoundOption Then
                                FDFAddField(Fieldname, FieldValue, FieldType.FldOption, FieldEnabled)
                                bFoundOption = False
                            Else
                                FDFAddField(Fieldname, FieldValue, FieldType.FldTextual, FieldEnabled)
                            End If
                        End If
                    End If
                Next
                Return True
                Exit Function
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetValuesFromDataSet", 1)
                Return False
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Creates XDP Form from HTML form submission data
        ''' </summary>
        ''' <param name="Page">Me.Page</param>
        ''' <param name="OptionNames">Option Names</param>
        ''' <returns>True</returns>
        ''' <remarks></remarks>
        Public Function FDFCreateXDPFromHTMLForm(ByVal Page As System.Web.UI.Page, Optional ByVal OptionNames() As String = Nothing, Optional ByVal FormName As String = "subform1") As Boolean
            Dim FieldValue As String
            Dim Fieldname As String
            Dim FieldEnabled As Boolean
            Dim Options As String, bFoundOption As Boolean
            Try
                Try
                    For intFrm As Integer = 0 To _FDF.Count - 1
                        If Not String_IsNullOrEmpty(_FDF(intFrm).FormName) Then
                            If _FDF(intFrm).FormName.ToLower & "" = FormName.ToLower & "" Then
                                _CurFDFDoc = intFrm
                                Exit For
                            End If
                        End If
                    Next
                Catch ex As Exception

                End Try
                If String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName) Then
                    XDPAddForm(FormName, "")
                End If
                For Each Fieldname In Page.Request.Form
                    FieldEnabled = True
                    FieldValue = Page.Request.Form.Item(Fieldname) & ""
                    If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                        For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                            If _fld.FieldName = Fieldname Then
                                _fld.FieldValue.Clear()
                                _fld.FieldValue.Add(FieldValue)
                                _fld.FieldEnabled = FieldEnabled
                                If Not OptionNames Is Nothing Then
                                    For Each Options In OptionNames
                                        If Options = Fieldname Then
                                            bFoundOption = True
                                        End If
                                    Next
                                End If
                                If bFoundOption Then
                                    _fld.FieldType = FieldType.FldOption
                                    bFoundOption = False
                                Else
                                    _fld.FieldType = FieldType.FldTextual
                                End If
                                FoundField = True
                                Exit Function
                            Else
                                bFoundOption = False
                                FoundField = False
                            End If
                        Next
                    Else
                        FoundField = False
                    End If
                    If Not FoundField Then
                        If Not Fieldname & "" = "submit" Then
                            If Not OptionNames Is Nothing Then
                                For Each Options In OptionNames
                                    If Options = Fieldname Then
                                        bFoundOption = True
                                    End If
                                Next
                            End If
                            If bFoundOption Then
                                XDPAddField(Fieldname, FieldValue, FormName, FieldType.FldOption, FieldEnabled)
                                bFoundOption = False
                            Else
                                XDPAddField(Fieldname, FieldValue, FormName, FieldType.FldTextual, FieldEnabled)
                            End If
                        End If
                    End If
                Next
                Return True
                Exit Function
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetValuesFromDataSet", 1)
                Return False
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Sets FDF Data from Datarow
        ''' </summary>
        ''' <param name="Dr">Datarow with data</param>
        ''' <param name="OptionNames">Option names</param>
        ''' <returns>FDF Data</returns>
        ''' <remarks></remarks>
        Public Function FDFSetValuesFromDataRow(ByVal Dr As DataRow, Optional ByVal OptionNames() As String = Nothing) As String
            Dim FieldValue As String = ""
            Dim Fieldname As String = ""
            Dim FieldTypes As String = ""
            Dim FieldEnabled As Boolean = True
            Dim DsFld As DataColumn
            Dim fldCnt As Integer = 0
            Dim Options As String = "", bFoundOption As Boolean = False
            Dim ReturnString As String = ""
            Try
                For Each DsFld In Dr.Table.Columns
                    FoundField = False
                    Fieldname = DsFld.ColumnName & ""
                    If Not IsDBNull(Dr(Fieldname)) Then
                        FieldValue = CStr(Dr(Fieldname)) & ""
                    Else
                        FieldValue = ""
                    End If
                    If Not DsFld.DataType Is Nothing Then
                        FieldTypes = DsFld.DataType.ToString & ""
                        ReturnString = ReturnString & Fieldname & ";" & FieldValue & ";" & FieldTypes & vbNewLine
                    Else
                        FieldTypes = "UNKNOWN"
                        ReturnString = ReturnString & Fieldname & ";" & FieldValue & ";" & FieldTypes & vbNewLine
                    End If

                    FieldEnabled = True
                    If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                        For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                            If _fld.FieldName = Fieldname Then
                                _fld.FieldValue.Clear()
                                _fld.FieldValue.Add(FieldValue)
                                _fld.FieldEnabled = FieldEnabled
                                If Not OptionNames Is Nothing Then
                                    For Each Options In OptionNames
                                        If Options = Fieldname Then
                                            bFoundOption = True
                                        End If
                                    Next
                                End If
                                If bFoundOption Then
                                    _fld.FieldType = FieldType.FldOption
                                    bFoundOption = False
                                Else
                                    _fld.FieldType = FieldType.FldTextual
                                End If
                                FoundField = True
                                Return Nothing
                                Exit Function
                            Else
                                bFoundOption = False
                                FoundField = False
                            End If
                        Next
                    End If
                    If Not FoundField Then
                        If Not OptionNames Is Nothing Then
                            For Each Options In OptionNames
                                If Options = Fieldname Then
                                    bFoundOption = True
                                End If
                            Next
                        End If
                        If bFoundOption Then
                            FDFAddField(Fieldname, FieldValue, FieldType.FldOption, FieldEnabled)
                            bFoundOption = False
                        Else
                            FDFAddField(Fieldname, FieldValue, FieldType.FldTextual, FieldEnabled)
                        End If
                    End If
                Next

                FDFData = FDFSavetoStr(FDFType.FDF, True)
                Return FDFData
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetValuesFromDataSet", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Add / remove fdf field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldEnabled">True / False</param>
        ''' <remarks></remarks>
        Public Sub FDFAddRemoveField(ByVal FieldName As String, Optional ByVal FieldEnabled As Boolean = False)
            Try
                Dim FoundField As Boolean
                FoundField = False
                If _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each _fld As FDFField In _FDF(_CurFDFDoc).struc_FDFFields
                        If LCase(_fld.FieldName) = LCase(FieldName) Then
                            _fld.FieldEnabled = FieldEnabled
                            FoundField = True
                            Exit Sub
                        End If
                    Next
                End If
                If Not FoundField Then
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found", "FDFAddRemoveField", 1)
                    Exit Sub
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFAddRemoveField", 1)
                Exit Sub
            End Try
        End Sub
        ''' <summary>
        ''' Iterates to the next field name
        ''' </summary>
        ''' <param name="bstrFieldName">Current Field name</param>
        ''' <param name="CaseSensitive">If True field name must match case</param>
        ''' <returns>next Field name</returns>
        ''' <remarks></remarks>
        Public Function FDFNextFieldName(ByVal bstrFieldName As String, Optional ByVal CaseSensitive As Boolean = False) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not bstrFieldName = "" Then
                    For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                        If FoundField = True Then
                            Return xField.FieldName & ""
                            Exit Function
                        End If
                        If CaseSensitive = True Then
                            If xField.FieldName = bstrFieldName Then
                                FoundField = True
                                Return xField.FieldName & ""
                                Exit Function
                            End If
                        Else
                            If LCase(xField.FieldName) = LCase(bstrFieldName) Then
                                FoundField = True
                                Return xField.FieldName & ""
                                Exit Function
                            End If
                        End If
                    Next
                Else
                    Return Nothing
                    Exit Function
                End If
                If FoundField = False Or Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    Return ""
                    Exit Function
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFNextFieldName", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue()", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets value of FDF Field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="CaseSensitive">If true must match case</param>
        ''' <returns>Field value</returns>
        ''' <remarks></remarks>
        Public Function FDFGetValue(ByVal FieldName As String, ByVal CaseSensitive As Boolean) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                        If CaseSensitive = True Then
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If xField.FieldName & "" = FieldName Then
                                    Return FDFCheckCharReverse(xField.FieldValue(0) & "")
                                    Exit Function
                                End If
                            End If
                        Else
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                    Return FDFCheckCharReverse(xField.FieldValue(0) & "")
                                    Exit Function
                                End If
                            End If
                        End If
                    Next
                End If
                Return Nothing
                Exit Function
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets value of FDF Field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <returns>Field value</returns>
        ''' <remarks></remarks>
        Public Function FDFGetValue(ByVal FieldName As String) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                        If Not String_IsNullOrEmpty(xField.FieldName) Then
                            If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                Return FDFCheckCharReverse(xField.FieldValue(0) & "")
                                Exit Function
                            End If
                        End If
                    Next
                End If
                Return Nothing
                Exit Function
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        Public Function FDFGetValueEnc(ByVal FieldName As String, ByVal outEncoding As Encoding, Optional ByVal CaseSensitive As Boolean = False) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                        If CaseSensitive = True Then
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If xField.FieldName & "" = FieldName Then
                                    Return FDFCheckCharReverse(xField.FieldValue(0) & "")
                                    Exit Function
                                End If
                            End If
                        Else
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                    Return FDFCheckCharReverse(xField.FieldValue(0) & "")
                                    Exit Function
                                End If
                            End If
                        End If
                    Next
                End If
                Return Nothing
                Exit Function
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets value of Live-Cycle Form Field 
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="xdpFormName">Live-Cycle Form Name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function XDPGetValue(ByVal FieldName As String, ByVal xdpFormName As String, Optional ByVal CaseSensitive As Boolean = False) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            xdpFrm = XDPForm(xdpFormName)

            Try
                If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                    If xdpFrm.struc_FDFFields.Count >= 1 Then
                        For Each xField In xdpFrm.struc_FDFFields
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If CaseSensitive = True Then
                                    If xField.FieldName & "" = FieldName Then
                                        Return Me.XDPCheckCharReverse(xField.FieldValue(0) & "")
                                        Exit Function
                                    End If
                                Else
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        Return Me.XDPCheckCharReverse(xField.FieldValue(0) & "")
                                        Exit Function
                                    End If
                                End If
                            End If
                        Next
                    Else
                        Return Nothing
                        Exit Function
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets value of Live-Cycle Form Field 
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' ''' <param name="FieldNumber">Field Number</param>
        ''' <param name="xdpFormName">Live-Cycle Form Name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function XDPGetValue(ByVal FieldName As String, ByVal FieldNumber As Integer, ByVal xdpFormName As String, Optional ByVal CaseSensitive As Boolean = False) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            xdpFrm = XDPForm(xdpFormName)

            Try
                If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                    If xdpFrm.struc_FDFFields.Count >= 1 Then
                        For Each xField In xdpFrm.struc_FDFFields
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If CaseSensitive = True Then
                                    If xField.FieldName & "" = FieldName Then
                                        If xField.FieldNum = FieldNumber Then
                                            Return Me.XDPCheckCharReverse(xField.FieldValue(0) & "")
                                            Exit Function
                                        End If

                                    End If
                                Else
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        If xField.FieldNum = FieldNumber Then
                                            Return Me.XDPCheckCharReverse(xField.FieldValue(0) & "")
                                            Exit Function
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Else
                        Return Nothing
                        Exit Function
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Get value of Live-Cycle form field
        ''' </summary>
        ''' <param name="FieldName">Live-Cycle form field name</param>
        ''' <param name="xdpFormNumber">Live-Cycle form name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field value</returns>
        ''' <remarks></remarks>
        Public Function XDPGetValue(ByVal FieldName As String, ByVal xdpFormNumber As Integer, Optional ByVal CaseSensitive As Boolean = False) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            xdpFrm = XDPForm(xdpFormNumber)

            Try
                If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                    If xdpFrm.struc_FDFFields.Count >= 1 Then
                        For Each xField In xdpFrm.struc_FDFFields
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If CaseSensitive = True Then
                                    If xField.FieldName & "" = FieldName Then
                                        Return Me.XDPCheckCharReverse(xField.FieldValue(0) & "")
                                        Exit Function
                                    End If
                                Else
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        Return Me.XDPCheckCharReverse(xField.FieldValue(0) & "")
                                        Exit Function
                                    End If
                                End If
                            End If
                        Next
                    Else
                        Return Nothing
                        Exit Function
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets value of Live-Cycle form field, in any Live-Cycle form
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field value</returns>
        ''' <remarks></remarks>
        Public Function XDPGetValue(ByVal FieldName As String, ByVal CaseSensitive As Boolean) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            Try
                For Each xdpFrm In _FDF
                    If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                        If xdpFrm.struc_FDFFields.Count >= 1 Then
                            For Each xField In xdpFrm.struc_FDFFields
                                If Not String_IsNullOrEmpty(xField.FieldName) Then
                                    If CaseSensitive = True Then
                                        If xField.FieldName & "" = FieldName Then
                                            Return Me.XDPCheckCharReverse(xField.FieldValue(0) & "")
                                            Exit Function
                                        End If
                                    Else
                                        If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                            Return Me.XDPCheckCharReverse(xField.FieldValue(0) & "")
                                            Exit Function
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets the value of an FDF Field in a character array
        ''' </summary>
        ''' <param name="FieldName">FDF Field name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field value character array</returns>
        ''' <remarks></remarks>
        Public Function FDFGetValueCharArray(ByVal FieldName As String, Optional ByVal CaseSensitive As Boolean = False) As Char()
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                        If CaseSensitive = True Then
                            If xField.FieldName & "" = FieldName Then
                                Return xField.FieldValue(0).ToCharArray
                                Exit Function
                            End If
                        Else
                            If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                Return xField.FieldValue(0).ToCharArray
                                Exit Function
                            End If
                        End If
                    Next
                Else
                    Return Nothing
                    Exit Function
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets the value of an FDF Field in a byte array
        ''' </summary>
        ''' <param name="FieldName">FDF Field name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field value byte array</returns>
        ''' <remarks></remarks>
        Public Function FDFGetValueByteArray(ByVal FieldName As String, Optional ByVal CaseSensitive As Boolean = False) As Byte()
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                        If CaseSensitive = True Then
                            If xField.FieldName & "" = FieldName Then
                                Return Me.StrToByteArray(xField.FieldValue(0).ToString)
                                Exit Function
                            End If
                        Else
                            If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                Return Me.StrToByteArray(xField.FieldValue(0).ToString)
                                Exit Function
                            End If
                        End If
                    Next
                Else
                    Return Nothing
                    Exit Function
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Get values array of FDF Form field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field values string array</returns>
        ''' <remarks></remarks>
        Public Function FDFGetValues(ByVal FieldName As String, Optional ByVal CaseSensitive As Boolean = False) As String()

            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim fldCnt As Integer
            Dim FieldValue As String
            Dim FieldValues(0) As String

            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then

                    For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                        If CaseSensitive = True Then
                            If xField.FieldName = FieldName Then
                                FieldValues = xField.FieldValue.ToArray
                                If FieldValues.Length = 1 And FieldValues(0) <> "" Then
                                    FieldValues(0) = FieldValues(0).TrimStart("(")
                                    FieldValues(0) = FieldValues(0).TrimEnd(")")
                                    Return FieldValues
                                    Exit Function
                                ElseIf FieldValues.Length > 1 Then
                                    ReDim Preserve FieldValues(FieldValues.Length - 2)
                                    For Each FieldValue In FieldValues
                                        FieldValues(fldCnt) = FieldValues(fldCnt).TrimStart("(")
                                        FieldValues(fldCnt) = FieldValues(fldCnt).TrimEnd(")")
                                        fldCnt += 1
                                    Next

                                    Return FieldValues
                                    Exit Function
                                Else
                                    Return Nothing
                                    Exit Function
                                End If
                            End If
                        Else
                            If LCase(xField.FieldName) = LCase(FieldName) And Not xField.FieldValue.Count <= 0 Then
                                FieldValues = xField.FieldValue.ToArray
                                If FieldValues.Length > 0 Then
                                    If FieldValues.Length = 1 And FieldValues(0) <> "" Then
                                        FieldValues(0) = FieldValues(0).TrimStart("(")
                                        FieldValues(0) = FieldValues(0).TrimEnd(")")
                                        Return FieldValues
                                        Exit Function
                                    ElseIf FieldValues.Length > 1 Then
                                        ReDim Preserve FieldValues(FieldValues.Length - 2)
                                        For Each FieldValue In FieldValues
                                            FieldValues(fldCnt) = FieldValues(fldCnt).TrimStart("(")
                                            FieldValues(fldCnt) = FieldValues(fldCnt).TrimEnd(")")
                                            fldCnt += 1
                                        Next
                                        Return FieldValues
                                        Exit Function
                                    Else
                                        Return Nothing
                                        Exit Function
                                    End If
                                End If
                            End If
                        End If
                    Next
                    Return FieldValues
                Else
                    Return Nothing
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found", "FDFDoc.FDFGetValues", 1)
                    Exit Function
                End If
            Catch ex As Exception
                Return FieldValues
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFGetValues", 1)
                Exit Function
            End Try

        End Function
        ''' <summary>
        ''' Get values array of FDF Form field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FormNumber">Form Number</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field values string array</returns>
        ''' <remarks></remarks>
        Public Function FDFGetValues(ByVal FieldName As String, ByVal FormNumber As Integer, Optional ByVal CaseSensitive As Boolean = False) As String()

            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim fldCnt As Integer
            Dim FieldValue As String
            Dim FieldValues(0) As String
            Try
                If Not XDPForm(FormNumber).struc_FDFFields.Count <= 0 Then

                    For Each xField In XDPForm(FormNumber).struc_FDFFields
                        If CaseSensitive = True Then
                            If xField.FieldName = FieldName Then
                                FieldValues = xField.FieldValue.ToArray
                                If FieldValues.Length = 1 And FieldValues(0) <> "" Then
                                    FieldValues(0) = FieldValues(0).TrimStart("(")
                                    FieldValues(0) = FieldValues(0).TrimEnd(")")
                                    Return FieldValues
                                    Exit Function
                                ElseIf FieldValues.Length > 1 Then
                                    ReDim Preserve FieldValues(FieldValues.Length - 2)
                                    For Each FieldValue In FieldValues
                                        FieldValues(fldCnt) = FieldValues(fldCnt).TrimStart("(")
                                        FieldValues(fldCnt) = FieldValues(fldCnt).TrimEnd(")")
                                        fldCnt += 1
                                    Next

                                    Return FieldValues
                                    Exit Function
                                Else
                                    Return Nothing
                                    Exit Function
                                End If
                            End If
                        Else
                            If LCase(xField.FieldName) = LCase(FieldName) And Not xField.FieldValue.Count <= 0 Then
                                FieldValues = xField.FieldValue.ToArray
                                If FieldValues.Length > 0 Then
                                    If FieldValues.Length = 1 And FieldValues(0) <> "" Then
                                        FieldValues(0) = FieldValues(0).TrimStart("(")
                                        FieldValues(0) = FieldValues(0).TrimEnd(")")
                                        Return FieldValues
                                        Exit Function
                                    ElseIf FieldValues.Length > 1 Then
                                        ReDim Preserve FieldValues(FieldValues.Length - 2)
                                        For Each FieldValue In FieldValues
                                            FieldValues(fldCnt) = FieldValues(fldCnt).TrimStart("(")
                                            FieldValues(fldCnt) = FieldValues(fldCnt).TrimEnd(")")
                                            fldCnt += 1
                                        Next
                                        Return FieldValues
                                        Exit Function
                                    Else
                                        Return Nothing
                                        Exit Function
                                    End If
                                End If
                            End If
                        End If
                    Next
                    Return FieldValues
                Else
                    Return Nothing
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found", "FDFDoc.FDFGetValues", 1)
                    Exit Function
                End If
            Catch ex As Exception
                Return FieldValues
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFGetValues", 1)
                Exit Function
            End Try

        End Function
        ''' <summary>
        ''' Get values array of FDF Form field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FormName">Form Name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field values string array</returns>
        ''' <remarks></remarks>
        Public Function FDFGetValues(ByVal FieldName As String, ByVal FormName As String, Optional ByVal CaseSensitive As Boolean = False) As String()

            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim fldCnt As Integer
            Dim FieldValue As String
            Dim FieldValues(0) As String
            Try
                If Not XDPForm(FormName).struc_FDFFields.Count <= 0 Then

                    For Each xField In XDPForm(FormName).struc_FDFFields
                        If CaseSensitive = True Then
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If xField.FieldName = FieldName Then
                                    FieldValues = xField.FieldValue.ToArray
                                    If FieldValues.Length = 1 And FieldValues(0) <> "" Then
                                        FieldValues(0) = FieldValues(0).TrimStart("(")
                                        FieldValues(0) = FieldValues(0).TrimEnd(")")
                                        Return FieldValues
                                        Exit Function
                                    ElseIf FieldValues.Length > 1 Then
                                        ReDim Preserve FieldValues(FieldValues.Length - 2)
                                        For Each FieldValue In FieldValues
                                            FieldValues(fldCnt) = FieldValues(fldCnt).TrimStart("(")
                                            FieldValues(fldCnt) = FieldValues(fldCnt).TrimEnd(")")
                                            fldCnt += 1
                                        Next

                                        Return FieldValues
                                        Exit Function
                                    Else
                                        Return Nothing
                                        Exit Function
                                    End If
                                End If
                            End If
                        Else
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If LCase(xField.FieldName) = LCase(FieldName) And Not xField.FieldValue.Count <= 0 Then
                                    FieldValues = xField.FieldValue.ToArray
                                    If FieldValues.Length > 0 Then
                                        If FieldValues.Length = 1 And FieldValues(0) <> "" Then
                                            FieldValues(0) = FieldValues(0).TrimStart("(")
                                            FieldValues(0) = FieldValues(0).TrimEnd(")")
                                            Return FieldValues
                                            Exit Function
                                        ElseIf FieldValues.Length > 1 Then
                                            ReDim Preserve FieldValues(FieldValues.Length - 2)
                                            For Each FieldValue In FieldValues
                                                FieldValues(fldCnt) = FieldValues(fldCnt).TrimStart("(")
                                                FieldValues(fldCnt) = FieldValues(fldCnt).TrimEnd(")")
                                                fldCnt += 1
                                            Next
                                            Return FieldValues
                                            Exit Function
                                        Else
                                            Return Nothing
                                            Exit Function
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                    Return FieldValues
                Else
                    Return Nothing
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found", "FDFDoc.FDFGetValues", 1)
                    Exit Function
                End If
            Catch ex As Exception
                Return FieldValues
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFGetValues", 1)
                Exit Function
            End Try

        End Function

        ''' <summary>
        ''' Get FDFField object array
        ''' </summary>
        ''' <param name="FieldNames">Optional Field Names</param>
        ''' <returns>FDFField object array</returns>
        ''' <remarks></remarks>
        Public Function FDFGetFields(Optional ByVal FieldNames As String = "") As FDFField()
            ' Inputs String and Splits it based on semicolin ";"
            Dim xField As FDFField
            Dim FoundField As Boolean
            Dim FieldCount As Integer
            FoundField = False
            Dim _ExportFields As New List(Of FDFField)
            Try
                If FieldNames = "" Then
                    Return _FDF(_CurFDFDoc).struc_FDFFields.ToArray()
                Else
                    Dim FldNames() As String = FieldNames.Split(";")
                    Dim FldName As String
                    FieldCount = 0
                    For Each FldName In FldNames
                        For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                            Try
                                If FldName.ToLower = xField.FieldName.ToLower Then
                                    _ExportFields.Add(xField)
                                End If
                            Catch ex As Exception

                            End Try

                        Next

                        FieldCount = FieldCount + 1
                    Next
                End If
                Return _ExportFields.ToArray
                Exit Function
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFGetFields", 1)
                Return _ExportFields.ToArray
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Sets On Import Javascript
        ''' </summary>
        ''' <param name="bstrScript">Script</param>
        ''' <param name="bBefore">Before page loads</param>
        ''' <remarks></remarks>
        Public Sub FDFSetOnImportJavaScript(ByVal bstrScript As String, ByVal bBefore As Boolean)
            Try
                Dim xAction As FDFImportScript
                Dim tmpScript As String
                tmpScript = Me.FDFCheckCharReverse(bstrScript)
                tmpScript = Me.FDFCheckChar(tmpScript)
                bstrScript = tmpScript
                'bstrScript = FDFCheckChar(bstrScript)
                Dim xCntr As Integer = 0
                Dim bFound As Boolean
                If Not _FDF(_CurFDFDoc).struc_ImportScripts Is Nothing Then
                    For Each xAction In _FDF(_CurFDFDoc).struc_ImportScripts
                        If xAction.Before = bBefore And xAction.ScriptCode <> "" Then
                            xAction.Before = bBefore
                            xAction.ScriptCode = bstrScript
                            bFound = True
                            Exit Sub
                        Else
                            bFound = False
                            Exit For
                        End If
                        xCntr += 1
                    Next
                End If
                'bstrScript = Me.FDFCheckChar(bstrScript)
                If bFound = False Then
                    If Not _FDF(_CurFDFDoc).struc_ImportScripts Is Nothing Then
                        If _FDF(_CurFDFDoc).struc_ImportScripts.Count < 2 Then
                            Dim _i As New FDFApp.FDFDoc_Class.FDFImportScript
                            _i.ScriptCode = bstrScript
                            _i.Before = bBefore
                            _FDF(_CurFDFDoc).struc_ImportScripts.Add(_i)
                        End If
                    ElseIf Not bstrScript = "" Then
                        Dim _i As New FDFApp.FDFDoc_Class.FDFImportScript
                        _i.ScriptCode = bstrScript
                        _i.Before = bBefore
                        _FDF(_CurFDFDoc).struc_ImportScripts.Add(_i)
                    End If
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetOnImportJavaScript", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Set Hide action
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="whichTrigger">Trigger</param>
        ''' <param name="bstrTheTarget">Target</param>
        ''' <param name="isHide">Hide (true/false)</param>
        ''' <remarks></remarks>
        Public Sub FDFSetHideAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger, ByVal bstrTheTarget As String, Optional ByVal isHide As Boolean = True)
            Try
                Dim xAction As FDFHideAction
                Dim xCntr As Integer, bFound As Boolean
                If Not _FDF(_CurFDFDoc).struc_HideActions Is Nothing Then
                    For Each xAction In _FDF(_CurFDFDoc).struc_HideActions
                        If xAction.FieldName = FieldName Then
                            If (whichTrigger = FDFActionTrigger.FDFUp And xAction.Trigger = FDFActionTrigger.FDFUp) Or (Not whichTrigger = FDFActionTrigger.FDFUp) Then     'And xAction.ActionType = ActionTypes.JavaScript 
                                xAction.FieldName = FieldName
                                xAction.Trigger = whichTrigger
                                xAction.Target = bstrTheTarget
                                xAction.Hide = isHide
                                Exit Sub
                                bFound = True
                                Exit For
                            Else
                                bFound = False
                                Exit For
                            End If
                        End If
                        xCntr += 1
                    Next
                End If
                If bFound = True Then

                Else
                    If Not _FDF(_CurFDFDoc).struc_HideActions Is Nothing Then
                        Dim _a As New FDFApp.FDFDoc_Class.FDFHideAction()
                        _a.FieldName = FieldName
                        _a.Trigger = whichTrigger
                        _a.Target = bstrTheTarget
                        _a.Hide = isHide
                        _FDF(_CurFDFDoc).struc_HideActions.Add(_a)
                    ElseIf Not FieldName = "" And Not bstrTheTarget = "" Then
                        Dim _a As New FDFApp.FDFDoc_Class.FDFHideAction()
                        _a.FieldName = FieldName
                        _a.Trigger = whichTrigger
                        _a.Target = bstrTheTarget
                        _a.Hide = isHide
                        _FDF(_CurFDFDoc).struc_HideActions.Add(_a)
                    End If
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetHideAction", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Sets import data action
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="whichTrigger">Trigger</param>
        ''' <param name="bstrTheFile">File to import</param>
        ''' <remarks></remarks>
        Public Sub FDFSetImportDataAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger, ByVal bstrTheFile As String)
            Try
                Dim xAction As FDFImportDataAction
                Dim xCntr As Integer, bFound As Boolean
                If Not _FDF(_CurFDFDoc).struc_ImportDataAction Is Nothing Then
                    For Each xAction In _FDF(_CurFDFDoc).struc_ImportDataAction
                        If xAction.FieldName = FieldName Then
                            If (whichTrigger = FDFActionTrigger.FDFUp And xAction.Trigger = FDFActionTrigger.FDFUp) Or (Not whichTrigger = FDFActionTrigger.FDFUp) Then     'And xAction.ActionType = ActionTypes.JavaScript 
                                xAction.FieldName = FieldName
                                xAction.Trigger = whichTrigger
                                xAction.FileName = bstrTheFile
                                Exit Sub
                                bFound = True
                                Exit For
                            Else
                                bFound = False
                                Exit For
                            End If
                        End If
                        xCntr += 1
                    Next
                End If
                If bFound = True Then

                Else
                    If Not _FDF(_CurFDFDoc).struc_ImportDataAction Is Nothing Then
                        Dim _i As New FDFApp.FDFDoc_Class.FDFImportDataAction
                        _i.FieldName = FieldName
                        _i.Trigger = whichTrigger
                        _i.FileName = bstrTheFile
                        _FDF(_CurFDFDoc).struc_ImportDataAction.Add(_i)

                    ElseIf Not FieldName = "" Then
                        Dim _i As New FDFApp.FDFDoc_Class.FDFImportDataAction
                        _i.FieldName = FieldName
                        _i.Trigger = whichTrigger
                        _i.FileName = bstrTheFile
                        _FDF(_CurFDFDoc).struc_ImportDataAction.Add(_i)
                    End If
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetImportDataAction", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Sets named action
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="whichTrigger">Trigger</param>
        ''' <param name="theName">PDF Action name</param>
        ''' <remarks></remarks>
        Public Sub FDFSetNamedAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger, ByVal theName As String)
            Try
                Dim xAction As FDFNamedAction
                Dim xCntr As Integer, bFound As Boolean
                If Not _FDF(_CurFDFDoc).struc_NamedActions Is Nothing Then
                    For Each xAction In _FDF(_CurFDFDoc).struc_NamedActions
                        If xAction.FieldName = FieldName Then
                            If (whichTrigger = FDFActionTrigger.FDFUp And xAction.Trigger = FDFActionTrigger.FDFUp) Or (Not whichTrigger = FDFActionTrigger.FDFUp) Then     'And xAction.ActionType = ActionTypes.JavaScript 
                                xAction.FieldName = FieldName
                                xAction.Trigger = whichTrigger
                                xAction.Name = theName
                                Exit Sub
                                bFound = True
                                Exit For
                            Else
                                bFound = False
                                Exit For
                            End If
                        End If
                        xCntr += 1
                    Next
                End If
                If bFound = True Then

                Else
                    If Not _FDF(_CurFDFDoc).struc_NamedActions Is Nothing Then
                        Dim _n As New FDFApp.FDFDoc_Class.FDFNamedAction
                        _n.FieldName = FieldName
                        _n.Trigger = whichTrigger
                        _n.Name = theName
                        _FDF(_CurFDFDoc).struc_NamedActions.Add(_n)
                    ElseIf Not FieldName = "" Then
                        Dim _n As New FDFApp.FDFDoc_Class.FDFNamedAction
                        _n.FieldName = FieldName
                        _n.Trigger = whichTrigger
                        _n.Name = theName
                        _FDF(_CurFDFDoc).struc_NamedActions.Add(_n)
                    End If
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetNamedAction", 1)
            End Try
        End Sub

        ''' <summary>
        ''' Adds JS action
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="whichTrigger">Trigger</param>
        ''' <param name="theScript">Script</param>
        ''' <remarks></remarks>
        Private Sub FDFAddJSAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger, ByVal theScript As String)
            Try
                Dim xAction As FDFActions
                'theScript = Me.FDFCheckCharReverse(theScript)
                'theScript = Me.FDFCheckChar(theScript)
                Dim xCntr As Integer, bFound As Boolean
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                        If xAction.FieldName = FieldName Then
                            If (whichTrigger = FDFActionTrigger.FDFUp And xAction.Trigger = whichTrigger) Then     'And xAction.ActionType = ActionTypes.JavaScript 
                                xAction.FieldName = FieldName
                                xAction.Trigger = whichTrigger
                                xAction.JavaScript_URL = theScript
                                xAction.ActionType = ActionTypes.JavaScript
                                Exit Sub
                                bFound = True
                                Exit For
                            Else
                                bFound = False
                                Exit For
                            End If
                        End If
                        xCntr += 1
                    Next
                End If
                If bFound = True Then

                Else
                    If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                        Dim _fa As New FDFApp.FDFDoc_Class.FDFActions
                        _fa.FieldName = FieldName
                        _fa.Trigger = whichTrigger
                        _fa.JavaScript_URL = theScript
                        _fa.ActionType = ActionTypes.JavaScript
                        _FDF(_CurFDFDoc).struc_FDFActions.Add(_fa)
                    ElseIf Not FieldName = "" Then
                        Dim _fa As New FDFApp.FDFDoc_Class.FDFActions
                        _fa.FieldName = FieldName
                        _fa.Trigger = whichTrigger
                        _fa.JavaScript_URL = theScript
                        _fa.ActionType = ActionTypes.JavaScript
                        _FDF(_CurFDFDoc).struc_FDFActions.Add(_fa)
                    End If
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddJSAction", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Sets reset form action
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="whichTrigger">Trigger</param>
        ''' <remarks></remarks>
        Public Sub FDFSetResetFormAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger)
            Try
                Dim xAction As FDFActions
                Dim xCntr As Integer, bFound As Boolean
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                        If xAction.FieldName = FieldName Then
                            If (whichTrigger = FDFActionTrigger.FDFUp And xAction.Trigger = FDFActionTrigger.FDFUp) Or (Not whichTrigger = FDFActionTrigger.FDFUp) Then     'And xAction.ActionType = ActionTypes.JavaScript 
                                xAction.FieldName = FieldName
                                xAction.Trigger = whichTrigger
                                xAction.ActionType = ActionTypes.Reset
                                Exit Sub
                                bFound = True
                                Exit For
                            Else
                                bFound = False
                                Exit For
                            End If
                        End If
                        xCntr += 1
                    Next
                End If
                If bFound = True Then

                Else
                    If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                        Dim _fa As New FDFActions
                        _fa.FieldName = FieldName
                        _fa.Trigger = whichTrigger
                        _fa.ActionType = ActionTypes.Reset
                        _FDF(_CurFDFDoc).struc_FDFActions.Add(_fa)
                    ElseIf Not FieldName = "" Then
                        Dim _fa As New FDFActions
                        _fa.FieldName = FieldName
                        _fa.Trigger = whichTrigger
                        _fa.ActionType = ActionTypes.Reset
                        _FDF(_CurFDFDoc).struc_FDFActions.Add(_fa)
                    End If
                End If


            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetResetFormAction", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Set URL action
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="whichTrigger">trigger</param>
        ''' <param name="URl">URL</param>
        ''' <remarks></remarks>
        Public Sub FDFSetURIAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger, ByVal URl As String)
            Try
                Dim xAction As FDFActions
                Dim xCntr As Integer, bFound As Boolean
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                        If xAction.FieldName = FieldName Then
                            If (whichTrigger = FDFActionTrigger.FDFUp And xAction.Trigger = FDFActionTrigger.FDFUp) Or (Not whichTrigger = FDFActionTrigger.FDFUp) Then     'And xAction.ActionType = ActionTypes.JavaScript 
                                xAction.FieldName = FieldName
                                xAction.Trigger = whichTrigger
                                xAction.JavaScript_URL = URl
                                xAction.ActionType = ActionTypes.URL
                                bFound = True
                                Exit Sub
                                Exit For
                            Else
                                bFound = False
                                Exit For
                            End If
                        End If
                        xCntr += 1
                    Next
                End If
                If bFound = True Then

                Else
                    If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                        Dim _fa As New FDFActions
                        _fa.FieldName = FieldName
                        _fa.Trigger = whichTrigger
                        _fa.JavaScript_URL = URl
                        _fa.ActionType = ActionTypes.URL
                        _FDF(_CurFDFDoc).struc_FDFActions.Add(_fa)
                    ElseIf Not FieldName = "" Then
                        Dim _fa As New FDFActions
                        _fa.FieldName = FieldName
                        _fa.Trigger = whichTrigger
                        _fa.JavaScript_URL = URl
                        _fa.ActionType = ActionTypes.URL
                        _FDF(_CurFDFDoc).struc_FDFActions.Add(_fa)
                    End If
                End If


            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSetURIAction", 1)
            End Try
        End Sub

        ''' <summary>
        ''' Adds Document Javascript
        ''' </summary>
        ''' <param name="bstrScriptName">Script name</param>
        ''' <param name="bstrScript">Document Script</param>
        ''' <remarks></remarks>
        Public Sub FDFAddDocJavaScript(ByVal bstrScriptName As String, ByVal bstrScript As String)
            Try
                bstrScript = Me.FDFCheckCharReverse(bstrScript)
                bstrScript = Me.FDFCheckChar(bstrScript)
                If Not _FDF(_CurFDFDoc).struc_DocScript Is Nothing Then
                    Dim _fa As New FDFApp.FDFDoc_Class.FDFScripts
                    _fa.ScriptName = bstrScriptName
                    _fa.ScriptCode = bstrScript
                    _FDF(_CurFDFDoc).struc_DocScript.Add(_fa)
                ElseIf Not bstrScriptName = "" Then
                    Dim _fa As New FDFApp.FDFDoc_Class.FDFScripts
                    _fa.ScriptName = bstrScriptName
                    _fa.ScriptCode = bstrScript
                    _FDF(_CurFDFDoc).struc_DocScript.Add(_fa)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddDocJavaScript", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Gets Document Javascripts
        ''' </summary>
        ''' <returns>Document javascripts</returns>
        ''' <remarks></remarks>
        Private Function GetDocJavaScripts(Optional ByVal iText As Boolean = True) As String
            Dim strScripts As String = ""
            Dim Script As FDFScripts
            Try
                If Not _FDF(_CurFDFDoc).struc_DocScript Is Nothing Then
                    For Each Script In _FDF(_CurFDFDoc).struc_DocScript
                        If iText = True Then
                            strScripts = strScripts & Environment.NewLine & _FDF(_CurFDFDoc).struc_DocScript(_FDF(_CurFDFDoc).struc_DocScript.Count - 1).ScriptCode
                        Else
                            If _FDF(_CurFDFDoc).struc_DocScript(_FDF(_CurFDFDoc).struc_DocScript.Count - 1).ScriptName <> "" Or Not _FDF(_CurFDFDoc).struc_DocScript(_FDF(_CurFDFDoc).struc_DocScript.Count - 1).ScriptName Is Nothing Then
                                strScripts = strScripts & "(" & _FDF(_CurFDFDoc).struc_DocScript(_FDF(_CurFDFDoc).struc_DocScript.Count - 1).ScriptName & ") " & "(" & _FDF(_CurFDFDoc).struc_DocScript(_FDF(_CurFDFDoc).struc_DocScript.Count - 1).ScriptCode & ")"
                            End If
                        End If


                    Next
                End If
                Return strScripts & ""
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.GetDocJavaScripts", 1)
                Return ""
            End Try
        End Function
        ''' <summary>
        ''' Has import javascripts
        ''' </summary>
        ''' <returns>True</returns>
        ''' <remarks></remarks>
        Private Function HasDocOnImportJavaScripts() As Boolean
            Try
                If _FDF(_CurFDFDoc).struc_ImportScripts.Count > 0 Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.HasDocOnImportJavaScripts", 1)
                Return False
            End Try
        End Function
        Private Function HasDocJavaScripts() As Boolean
            Try
                If _FDF(_CurFDFDoc).struc_DocScript.Count > 0 Then
                    If _FDF(_CurFDFDoc).struc_DocScript.Count = 1 And (Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).struc_DocScript(0).ScriptCode) And Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).struc_DocScript(0).ScriptCode)) Then
                        Return True
                    End If
                    Return False
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.HasDocJavaScripts", 1)
                Return False
            End Try
        End Function
        Private Function HasField_SubmitActions(ByVal field_Name As String) As Boolean
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each fld As FDFActions In _FDF(_CurFDFDoc).struc_FDFActions
                        If fld.FieldName.ToLower = field_Name.ToLower Then
                            If fld.ActionType = ActionTypes.Submit Then
                                Return True
                            End If
                        End If
                    Next
                End If
                Return False
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.HasDocJavaScripts", 1)
                Return False
            End Try
        End Function
        Private Function GetField_SubmitActionURL(ByVal field_Name As String) As String
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each fld As FDFActions In _FDF(_CurFDFDoc).struc_FDFActions
                        If fld.FieldName.ToLower = field_Name.ToLower Then
                            If fld.ActionType = ActionTypes.Submit Then
                                Return fld.JavaScript_URL
                            End If
                        End If
                    Next
                End If
                Return ""
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.HasDocJavaScripts", 1)
                Return False
            End Try
        End Function
        Private Function HasField_ResetActions(ByVal field_Name As String) As Boolean
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each fld As FDFActions In _FDF(_CurFDFDoc).struc_FDFActions
                        If fld.FieldName.ToLower = field_Name.ToLower Then
                            If fld.ActionType = ActionTypes.Reset Then
                                Return True
                            End If
                        End If
                    Next
                End If
                Return False
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.HasDocJavaScripts", 1)
                Return False
            End Try
        End Function
        Private Function HasField_URLActions(ByVal field_Name As String) As Boolean
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each fld As FDFActions In _FDF(_CurFDFDoc).struc_FDFActions
                        If fld.FieldName.ToLower = field_Name.ToLower Then
                            If fld.ActionType = ActionTypes.URL Then
                                Return True
                            End If
                        End If
                    Next
                End If
                Return False
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.HasDocJavaScripts", 1)
                Return False
            End Try
        End Function
        Private Function HasField_JavascriptActions(ByVal field_Name As String) As Boolean
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each fld As FDFActions In _FDF(_CurFDFDoc).struc_FDFActions
                        If fld.FieldName.ToLower = field_Name.ToLower Then
                            If fld.ActionType = ActionTypes.JavaScript Then
                                Return True
                            End If
                        End If
                    Next
                End If
                Return False
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.HasDocJavaScripts", 1)
                Return False
            End Try
        End Function
        Private Function ReturnTriggerString(ByVal whichTrigger As FDFActionTrigger) As String
            Select Case whichTrigger
                Case FDFActionTrigger.FDFEnter
                    Return "E"
                Case FDFActionTrigger.FDFExit
                    Return "X"
                Case FDFActionTrigger.FDFDown
                    Return "D"
                Case FDFActionTrigger.FDFUp
                    Return ""
                Case FDFActionTrigger.FDFOnFocus
                    Return "Fo"
                Case FDFActionTrigger.FDFOnBlur
                    Return "Bl"
                Case FDFActionTrigger.FDFCalculate
                    Return "C"
                Case FDFActionTrigger.FDFFormat
                    Return "F"
                Case FDFActionTrigger.FDFKeystroke
                    Return "K"
                Case FDFActionTrigger.FDFValidate
                    Return "V"
                Case Else
                    Return ""
            End Select
        End Function
        Private Function FDFGetJSAction(ByVal FieldName As String, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If xAction.FieldName = FieldName And xAction.ActionType = ActionTypes.JavaScript And xAction.Exported = False And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /JavaScript /JS (" & xAction.JavaScript_URL & ") >> "
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA") & " << /" & ReturnTriggerString(xAction.Trigger), "") & " << /S /JavaScript /JS (" & xAction.JavaScript_URL & ") >> " & IIf(IncludeTrigger, ">> ", "")
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetImportDataAction(ByVal FieldName As String, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_ImportDataAction Is Nothing Then
                Dim xAction As FDFImportDataAction
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_ImportDataAction
                    If xAction.FieldName = FieldName And xAction.Exported = False Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /ImportData /F (" & xAction.FileName & ") >> "
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /" & ReturnTriggerString(xAction.Trigger) & " << /S /ImportData /F (" & xAction.FileName & ") >> " & IIf(IncludeTrigger, ">>", "")
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetImportDataActions(Optional ByVal IncludeExported As Boolean = False, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_ImportDataAction Is Nothing Then
                Dim xAction As FDFImportDataAction
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_ImportDataAction
                    If (xAction.Exported And IncludeExported = True) Or (xAction.Exported = False) And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /ImportData /F (" & xAction.FileName & ") >> "
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA") & " << /" & ReturnTriggerString(xAction.Trigger), "") & " << /S /ImportData /F (" & xAction.FileName & ") >> " & IIf(IncludeTrigger, ">> ", "")
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function

        Private Function FDFGetImportJSAction(Optional ByVal Before As Boolean = Nothing, Optional ByVal iText As Boolean = False) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_ImportScripts Is Nothing Then
                Dim xAction As FDFImportScript
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_ImportScripts
                    If xAction.Before = Before And xAction.ScriptCode & "" <> "" Then
                        If iText = True Then
                            returnAction = returnAction & Environment.NewLine & xAction.ScriptCode
                        Else
                            returnAction = returnAction & IIf(xAction.Before = True, "/Before", "/After") & "(" & xAction.ScriptCode & ") "
                        End If
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetImportJSActions(Optional ByVal IncludeBrackets As Boolean = False, Optional ByVal iText As Boolean = False) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_ImportScripts Is Nothing Then
                Dim xAction As FDFImportScript
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_ImportScripts
                    If xAction.ScriptCode & "" <> "" Then
                        If iText = True Then
                            returnAction = returnAction & Environment.NewLine & xAction.ScriptCode
                        Else
                            returnAction = returnAction & " /" & IIf(xAction.Before = True, "Before", "After") & "(" & xAction.ScriptCode & ") "
                            xCntr += 1
                        End If
                    End If
                Next
                If xCntr > 0 Then
                    If IncludeBrackets Then
                        returnAction = "<< " & returnAction & " >> "
                    Else
                        returnAction = returnAction
                    End If
                End If
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetSubmitAction(ByVal FieldName As String, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If xAction.FieldName = FieldName And xAction.ActionType = ActionTypes.Submit And xAction.Exported = False And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /SubmitForm /F (" & xAction.JavaScript_URL & ") >> "
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA") & " << /" & ReturnTriggerString(xAction.Trigger), "") & " << /S /SubmitForm /F (" & xAction.JavaScript_URL & ") >> " & IIf(IncludeTrigger, ">> ", "")
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function

        Private Function FDFGetNamedAction(ByVal FieldName As String, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_NamedActions Is Nothing Then
                Dim xAction As FDFNamedAction
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_NamedActions
                    If xAction.FieldName = FieldName And xAction.Exported = False And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /Named /N /" & xAction.Name & ") >> "
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA") & " << /" & ReturnTriggerString(xAction.Trigger), "") & " << /S /Named /N /" & xAction.Name & ") >> " & IIf(IncludeTrigger, ">> ", "")
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetNamedActions(Optional ByVal IncludeExported As Boolean = False, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_NamedActions Is Nothing Then
                Dim xAction As FDFNamedAction
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_NamedActions
                    If (xAction.Exported And IncludeExported = True) Or (xAction.Exported = False) And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /Named /N /" & xAction.Name & ") >> "
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA") & " << /" & ReturnTriggerString(xAction.Trigger), "") & " << /S /Named /N /" & xAction.Name & ") >> " & IIf(IncludeTrigger, ">>", "")
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetHideAction(ByVal FieldName As String, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_HideActions Is Nothing Then
                Dim xAction As FDFHideAction
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_HideActions
                    If xAction.FieldName = FieldName And xAction.Exported = False And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /Hide /T (" & xAction.Target & ") " & IIf(xAction.Hide, "", "/H false") & " >> "
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA") & " << /" & ReturnTriggerString(xAction.Trigger), "") & " << /S /Hide /T (" & xAction.Target & ") " & IIf(xAction.Hide, "", "/H false") & " >> " & IIf(IncludeTrigger, ">> ", "")
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetHideActions(Optional ByVal IncludeExported As Boolean = False, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_HideActions Is Nothing Then
                Dim xAction As FDFHideAction
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_HideActions
                    If (xAction.Exported And IncludeExported = True) Or (xAction.Exported = False) And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /Hide /T (" & xAction.Target & ") " & IIf(xAction.Hide, "", "/H false") & " >> "
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA") & " << /" & ReturnTriggerString(xAction.Trigger), "") & " << /S /Hide /T (" & xAction.Target & ") " & IIf(xAction.Hide, "", "/H false") & " >> " & IIf(IncludeTrigger, ">> ", "")
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetResetAction(ByVal FieldName As String, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If xAction.FieldName = FieldName And xAction.ActionType = ActionTypes.Reset And xAction.Exported = False And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /ResetForm >> "       '/F (" & xAction.JavaScript_URL & ")
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA") & " << /" & ReturnTriggerString(xAction.Trigger), "") & " << /S /ResetForm >> " & IIf(IncludeTrigger, ">> ", "")          '/F (" & xAction.JavaScript_URL & ") 
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function

        Private Function FDFGetURlAction(ByVal FieldName As String, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeTrigger As Boolean = True) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If xAction.FieldName = FieldName And xAction.ActionType = ActionTypes.URL And xAction.Exported = False And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        Select Case xAction.Trigger
                            Case FDFActionTrigger.FDFUp
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA"), "") & " << /S /URI /URI (" & xAction.JavaScript_URL & ") >> "
                            Case Else
                                returnAction = returnAction & IIf(IncludeTrigger, " /" & IIf(xAction.Trigger = FDFActionTrigger.FDFUp, "A", "AA") & " << /" & ReturnTriggerString(xAction.Trigger), "") & " << /S /URI /URI (" & xAction.JavaScript_URL & ") >> " & IIf(IncludeTrigger, ">> ", "")
                        End Select
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetSubmitActions(Optional ByVal IncludeExported As Boolean = False, Optional ByVal Trigger As FDFActionTrigger = Nothing) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If (xAction.Exported And IncludeExported = True) Or (xAction.Exported = False) And xAction.ActionType = ActionTypes.Submit And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        returnAction = returnAction & "<< /T (" & xAction.FieldName & ") " & FDFGetSubmitAction(xAction.FieldName) & FDFGetSubmitAction(xAction.FieldName) & " >> "
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetResetActions(Optional ByVal IncludeExported As Boolean = False, Optional ByVal Trigger As FDFActionTrigger = Nothing) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If (xAction.Exported And IncludeExported = True) Or (xAction.Exported = False) And xAction.ActionType = ActionTypes.Reset And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        returnAction = returnAction & "<< /T (" & xAction.FieldName & ") " & FDFGetResetAction(xAction.FieldName) & FDFGetResetAction(xAction.FieldName) & " >> "
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetJSActions(Optional ByVal IncludeExported As Boolean = False, Optional ByVal Trigger As FDFActionTrigger = Nothing) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If (xAction.Exported And IncludeExported = True) Or (xAction.Exported = False) And xAction.ActionType = ActionTypes.JavaScript And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        returnAction = returnAction & " << /T (" & xAction.FieldName & ") " & FDFGetJSAction(xAction.FieldName) & " " & FDFGetJSAction(xAction.FieldName) & " >> "
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetURlActions(Optional ByVal IncludeExported As Boolean = False, Optional ByVal Trigger As FDFActionTrigger = Nothing) As String
            Dim returnAction As String = ""
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                Dim xCntr As Integer = 0
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If (xAction.Exported And IncludeExported = True) Or (xAction.Exported = False) And xAction.ActionType = ActionTypes.URL And (Trigger = xAction.Trigger Or Trigger = Nothing) Then
                        returnAction = returnAction & "<< /T (" & xAction.FieldName & ") " & FDFGetJSAction(xAction.FieldName) & FDFGetURlAction(xAction.FieldName) & " >> "
                        xAction.Exported = True
                    End If
                    xCntr += 1
                Next
            End If
            Return returnAction & ""
        End Function
        Private Function FDFGetAllActions(Optional ByVal FieldName As String = "", Optional ByVal Trigger As FDFActionTrigger = Nothing) As String
            Dim returnAction As String = ""
            Dim xTrigger As FDFActionTrigger, xTriggers(9) As FDFActionTrigger
            xTriggers(0) = FDFActionTrigger.FDFCalculate
            xTriggers(1) = FDFActionTrigger.FDFDown
            xTriggers(2) = FDFActionTrigger.FDFEnter
            xTriggers(3) = FDFActionTrigger.FDFExit
            xTriggers(4) = FDFActionTrigger.FDFFormat
            xTriggers(5) = FDFActionTrigger.FDFKeystroke
            xTriggers(6) = FDFActionTrigger.FDFOnBlur
            xTriggers(7) = FDFActionTrigger.FDFOnFocus
            xTriggers(8) = FDFActionTrigger.FDFUp
            xTriggers(9) = FDFActionTrigger.FDFValidate
            Dim xCntr As Integer = 0
            xCntr = 0
            Dim FLDS As FDFField
            For Each FLDS In _FDF(_CurFDFDoc).struc_FDFFields
                If (FieldName = FLDS.FieldName) Then
                    returnAction = returnAction & IIf(Not FieldName = "", "", " << /T (" & FLDS.FieldName & ") ")
                    For Each xTrigger In xTriggers
                        returnAction = returnAction & FDFGetJSAction(FLDS.FieldName, xTrigger) & FDFGetSubmitAction(FLDS.FieldName, xTrigger) & FDFGetSubmitAction(FLDS.FieldName, xTrigger) & FDFGetResetAction(FLDS.FieldName, xTrigger) & FDFGetResetAction(FLDS.FieldName, xTrigger) & FDFGetURlAction(FLDS.FieldName, xTrigger) & Me.FDFGetNamedAction(FLDS.FieldName, xTrigger) & Me.FDFGetHideAction(FLDS.FieldName, xTrigger)
                    Next
                    returnAction = returnAction & IIf(Not FieldName = "", "", " >> ")
                End If
                xCntr += 1
            Next
            Return returnAction & ""
        End Function
        Private Function FDFGetAllActionsForField(Optional ByVal FieldName As String = "", Optional ByVal IncludeTrigger As Boolean = True, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeFieldName As Boolean = False) As String
            Dim returnAction As String = ""
            Dim xAction1 As FDFActions, xTrigger As FDFActionTrigger, xTriggers(9) As FDFActionTrigger
            Dim xAction2 As FDFHideAction
            Dim xAction3 As FDFImportDataAction
            Dim xAction5 As FDFNamedAction

            'EDITED 2011-01-02
            xTriggers(0) = FDFActionTrigger.FDFUp
            xTriggers(1) = FDFActionTrigger.FDFEnter
            xTriggers(2) = FDFActionTrigger.FDFExit
            xTriggers(3) = FDFActionTrigger.FDFDown
            xTriggers(4) = FDFActionTrigger.FDFFormat
            xTriggers(5) = FDFActionTrigger.FDFValidate
            xTriggers(6) = FDFActionTrigger.FDFKeystroke
            xTriggers(7) = FDFActionTrigger.FDFCalculate
            xTriggers(8) = FDFActionTrigger.FDFOnFocus
            xTriggers(9) = FDFActionTrigger.FDFOnBlur

            'FDFEnter = 0
            'FDFExit = 1
            'FDFDown = 2
            'FDFUp = 3
            'FDFFormat = 4
            'FDFValidate = 5
            'FDFKeystroke = 6
            'FDFCalculate = 7
            'FDFOnFocus = 8
            'FDFOnBlur = 9

            Dim xCntr As Integer = 0
            xCntr = 0
            Dim ClosingBracket As Boolean
            Dim strActionString As String = ""
            Dim strActionStringClose As String = " >> "
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                'returnAction = returnAction & IIf(IncludeFieldName, "<< /T (" & xAction1.FieldName & ") ", "") & IIf(xAction1.Trigger = FDFActionTrigger.FDFUp, "/A", "/AA") & "<< "
                'strActionString = IIf(xAction1.Trigger = FDFActionTrigger.FDFUp, "/A", "/AA") & "<< "
            End If
            Dim keyUpTrigger As String = "", predicateTrigger As String = ""
            For Each xTrigger In xTriggers
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    'Dim xAction As FDFActions
                    For Each xAction1 In _FDF(_CurFDFDoc).struc_FDFActions
                        If (xAction1.Exported = False And FieldName = xAction1.FieldName) Then
                            If xAction1.Trigger = xTrigger Then
                                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                                    If String_IsNullOrEmpty(strActionString) Then
                                        strActionString = IIf(IncludeFieldName, "<< /T (" & xAction1.FieldName & ") ", "")
                                        Select Case xAction1.Trigger
                                            Case FDFActionTrigger.FDFUp
                                                predicateTrigger = "/A"
                                                returnAction = returnAction & predicateTrigger & " << "
                                                returnAction = returnAction & " /" & IIf(xAction1.Trigger = FDFActionTrigger.FDFUp, "S", ReturnTriggerString(xAction1.Trigger)) & " /JavaScript /JS (" & xAction1.JavaScript_URL & ") "
                                                returnAction = returnAction & " >> "
                                                
                                                If IncludeTrigger Then ClosingBracket = True
                                                xAction1.Exported = True
                                            Case Else
                                                predicateTrigger = "/AA "
                                                If returnAction.Contains(predicateTrigger) Then
                                                    returnAction = returnAction & " /" & ReturnTriggerString(xAction1.Trigger) & " << /S /JavaScript /JS (" & xAction1.JavaScript_URL & ") >> "
                                                Else
                                                    returnAction = returnAction & predicateTrigger & " << "
                                                    returnAction = returnAction & "/" & IIf(xAction1.Trigger = FDFActionTrigger.FDFUp, "S", ReturnTriggerString(xAction1.Trigger)) & " << /S /JavaScript /JS (" & xAction1.JavaScript_URL & ") >> "
                                                End If
                                                xAction1.Exported = True
                                                If IncludeTrigger Then ClosingBracket = True
                                        End Select

                                        If Not _FDF(_CurFDFDoc).struc_HideActions.Count <= 0 Then
                                            For Each xAction2 In _FDF(_CurFDFDoc).struc_HideActions
                                                If (xAction2.Exported = False And FieldName = xAction2.FieldName) Then
                                                    If xAction2.Trigger = xTrigger Then
                                                        returnAction = returnAction & IIf(FDFHasHideActions(xAction2.FieldName), Me.FDFGetHideAction(xAction2.FieldName, xTrigger), "") & " "
                                                        If Not _FDF(_CurFDFDoc).struc_ImportDataAction Is Nothing Then
                                                            For Each xAction3 In _FDF(_CurFDFDoc).struc_ImportDataAction
                                                                If (xAction3.Exported = False) And (FieldName = xAction3.FieldName) Then
                                                                    If xAction3.Trigger = xTrigger Then
                                                                        returnAction = returnAction & IIf(Me.FDFHasImportDataActions(xAction3.FieldName), Me.FDFGetImportDataAction(xAction3.FieldName, xTrigger), "") & " "
                                                                    End If
                                                                End If
                                                            Next
                                                        End If
                                                        If Not _FDF(_CurFDFDoc).struc_NamedActions Is Nothing Then
                                                            For Each xAction5 In _FDF(_CurFDFDoc).struc_NamedActions
                                                                If (xAction5.Exported = False) And (FieldName = xAction5.FieldName) Then
                                                                    If xAction5.Trigger = xTrigger Then
                                                                        returnAction = returnAction & IIf(Me.FDFHasNamedActions(xAction5.FieldName), Me.FDFGetNamedAction(xAction5.FieldName, xTrigger), "") & " "
                                                                    End If
                                                                End If
                                                            Next
                                                        End If
                                                    End If

                                                End If
                                            Next
                                        End If
                                    End If
                                End If
                            End If
                        End If
                        xCntr += 1
                    Next

                    xCntr = 0
                End If

                If Not _FDF(_CurFDFDoc).struc_HideActions.Count <= 0 Then
                    For Each xAction2 In _FDF(_CurFDFDoc).struc_HideActions
                        If (xAction2.Exported = False And FieldName = xAction2.FieldName) Then
                            If xAction2.Trigger = xTrigger Then
                                returnAction = returnAction & IIf(IncludeTrigger, "<< /T(" & xAction2.FieldName & ") ", "") & IIf(FDFHasHideActions(xAction2.FieldName), Me.FDFGetHideAction(xAction2.FieldName, xTrigger), "") & IIf(IncludeTrigger, " >>", "")
                                If IncludeTrigger Then ClosingBracket = True
                                If Not _FDF(_CurFDFDoc).struc_ImportDataAction Is Nothing Then
                                    For Each xAction3 In _FDF(_CurFDFDoc).struc_ImportDataAction
                                        If (xAction3.Exported = False) And (FieldName = xAction3.FieldName) Then
                                            If xAction3.Trigger = xTrigger Then
                                                returnAction = returnAction & IIf(Me.FDFHasImportDataActions(xAction3.FieldName), Me.FDFGetImportDataAction(xAction3.FieldName, xTrigger), "") & " "
                                            End If
                                        End If
                                    Next
                                End If
                                If Not _FDF(_CurFDFDoc).struc_NamedActions.Count <= 0 Then
                                    For Each xAction5 In _FDF(_CurFDFDoc).struc_NamedActions
                                        If (xAction5.Exported = False) And (FieldName = xAction5.FieldName) Then
                                            If xAction5.Trigger = xTrigger Then
                                                returnAction = returnAction & IIf(Me.FDFHasNamedActions(xAction5.FieldName), Me.FDFGetNamedAction(xAction5.FieldName, xTrigger), "") & " "
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    Next
                End If

                If Not _FDF(_CurFDFDoc).struc_ImportDataAction.Count <= 0 Then
                    For Each xAction3 In _FDF(_CurFDFDoc).struc_ImportDataAction
                        If (xAction3.Exported = False And FieldName = xAction3.FieldName) Then
                            If xAction3.Trigger = xTrigger Then
                                returnAction = returnAction & IIf(IncludeTrigger, "<< /T(" & xAction3.FieldName & ") ", "") & IIf(Me.FDFHasImportDataActions(xAction3.FieldName), Me.FDFGetImportDataAction(xAction3.FieldName, xTrigger), "") & IIf(Me.FDFHasNamedActions(xAction3.FieldName), Me.FDFGetNamedAction(xAction3.FieldName, xTrigger), "") & IIf(IncludeTrigger, " >>", "")
                                If IncludeTrigger Then ClosingBracket = True
                                If Not _FDF(_CurFDFDoc).struc_NamedActions Is Nothing Then
                                    For Each xAction5 In _FDF(_CurFDFDoc).struc_NamedActions
                                        If (xAction5.Exported = False) And (FieldName = xAction5.FieldName) Then
                                            If xAction5.Trigger = xTrigger Then
                                                returnAction = returnAction & IIf(Me.FDFHasNamedActions(xAction5.FieldName), Me.FDFGetNamedAction(xAction5.FieldName, xTrigger), "")
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    Next
                End If
                If Not _FDF(_CurFDFDoc).struc_NamedActions.Count <= 0 Then
                    For Each xAction5 In _FDF(_CurFDFDoc).struc_NamedActions
                        If (xAction5.Exported = False) And (FieldName = xAction5.FieldName) Then
                            If xAction5.Trigger = xTrigger Then
                                returnAction = returnAction & IIf(IncludeFieldName, "<< /T(" & xAction5.FieldName & ") ", "") & IIf(Me.FDFHasNamedActions(xAction5.FieldName), Me.FDFGetNamedAction(xAction5.FieldName, xTrigger), "") & IIf(IncludeTrigger, " >>", "")
                                If IncludeTrigger Then ClosingBracket = True
                            End If
                        End If
                    Next
                End If
            Next
            If String_IsNullOrEmpty(strActionString) And String_IsNullOrEmpty(returnAction) Then
                strActionStringClose = ""
            End If
            returnAction = strActionString & returnAction & strActionStringClose
            Return returnAction & ""
        End Function

        Private Function XDPGetAllActionsForField(Optional ByVal FieldName As String = "", Optional ByVal IncludeTrigger As Boolean = True, Optional ByVal Trigger As FDFActionTrigger = Nothing, Optional ByVal IncludeFieldName As Boolean = False) As String
            Dim returnAction As String = ""
            Dim xAction1 As XDPActions, xTrigger As XDPActionTrigger, xTriggers(7) As XDPActionTrigger

            xTriggers(0) = XDPActionTrigger.Clicked
            xTriggers(1) = XDPActionTrigger.MouseDown
            xTriggers(2) = XDPActionTrigger.MouseUp
            xTriggers(3) = XDPActionTrigger.OnBlur
            xTriggers(4) = XDPActionTrigger.OnFocus
            xTriggers(5) = XDPActionTrigger.OnHover
            xTriggers(6) = XDPActionTrigger.OnMouseEnter
            xTriggers(7) = XDPActionTrigger.OnMouseExit

            Dim xCntr As Integer = 0
            xCntr = 0
            Dim ClosingBracket As Boolean
            For Each xTrigger In xTriggers
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each xAction1 In _FDF(_CurFDFDoc).struc_XDPActions
                        If (xAction1.Exported = False And FieldName = xAction1.FieldName) Then
                            If xAction1.Trigger = xTrigger Then
                                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                                    Select Case xAction1.Trigger
                                        Case XDPActionTrigger.Clicked
                                            returnAction = returnAction & "<event activity=""click""><submit textEncoding=""" & DefaultEncoding.ToString & """ format=""" & xAction1.Format & """ xdpContent = """ & xAction1.xdpContent & """ target = """ & xAction1.JavaScript_URL & """ embedPDF=""" & xAction1.EmbedPDF + 0 & """" & IIf(xAction1.Lock = True, " lock=""1""", "") & IIf(xAction1.EmbedPDF = True, " embedPDF=""1""", "") & IIf(xAction1.ID.Length > 0, " id=""" & xAction1.ID & """", "") & IIf(xAction1.use.Length > 0, " use=""" & xAction1.use & """", "") & """/></event>"
                                            If IncludeTrigger Then ClosingBracket = True
                                        Case Else
                                            returnAction = returnAction & "<event activity=""click""><submit textEncoding=""" & DefaultEncoding.ToString & """ format=""" & xAction1.Format & """ xdpContent = """ & xAction1.xdpContent & """ target = """ & xAction1.JavaScript_URL & """ embedPDF=""" & xAction1.EmbedPDF + 0 & """" & IIf(xAction1.Lock = True, " lock=""1""", "") & IIf(xAction1.EmbedPDF = True, " embedPDF=""1""", "") & IIf(xAction1.ID.Length > 0, " id=""" & xAction1.ID & """", "") & IIf(xAction1.use.Length > 0, " use=""" & xAction1.use & """", "") & """/></event>"
                                            If IncludeTrigger Then ClosingBracket = True
                                    End Select
                                End If
                            End If
                        End If
                        xCntr += 1
                    Next
                    xCntr = 0
                End If
            Next
            Return returnAction & ""
        End Function

        Private Function FDFGetRemainingActions(Optional ByVal includeTrigger As Boolean = False) As String
            Dim returnAction As String = ""

            Dim xCntr As Integer = 0
            xCntr = 0
            returnAction = returnAction & FDFGetSubmitActions() & FDFGetResetActions() & FDFGetHideActions() & FDFGetJSActions() & FDFGetNamedActions() & FDFGetImportDataActions()
            returnAction = returnAction
            Return returnAction

        End Function

        Private Function FDFHasSubmitAction(ByVal FieldName As String) As Boolean
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If xAction.FieldName = FieldName And xAction.ActionType = ActionTypes.Submit And xAction.Exported = False Then
                        Return True
                        Exit Function
                    End If
                Next
                Return False
                Exit Function
            End If
            Return False
            Exit Function
        End Function
        Private Function FDFHasJSAction(ByVal FieldName As String) As Boolean
            If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                Dim xAction As FDFActions
                For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                    If xAction.FieldName = FieldName And xAction.ActionType = ActionTypes.JavaScript And xAction.Exported = False Then
                        Return True
                        Exit Function
                    End If
                Next
                Return False
                Exit Function
            End If
            Return False
            Exit Function
        End Function
        Private Function FDFHasHideActions(ByVal FieldName As String) As Boolean
            If Not _FDF(_CurFDFDoc).struc_HideActions Is Nothing Then
                Dim xAction As FDFHideAction
                For Each xAction In _FDF(_CurFDFDoc).struc_HideActions
                    If xAction.FieldName = FieldName And xAction.Exported = False Then
                        Return True
                        Exit Function
                    End If
                Next
                Return False
                Exit Function
            End If
            Return False
            Exit Function
        End Function

        Private Function FDFHasImportDataActions(ByVal FieldName As String) As Boolean
            If Not _FDF(_CurFDFDoc).struc_ImportDataAction Is Nothing Then
                Dim xAction As FDFImportDataAction
                For Each xAction In _FDF(_CurFDFDoc).struc_ImportDataAction
                    If xAction.FieldName = FieldName And xAction.Exported = False Then
                        Return True
                        Exit Function
                    End If
                Next
                Return False
                Exit Function
            End If
            Return False
            Exit Function
        End Function

        Private Function FDFHasNamedActions(ByVal FieldName As String) As Boolean
            If Not _FDF(_CurFDFDoc).struc_NamedActions Is Nothing Then
                Dim xAction As FDFNamedAction
                For Each xAction In _FDF(_CurFDFDoc).struc_NamedActions
                    If xAction.FieldName = FieldName And xAction.Exported = False Then
                        Return True
                        Exit Function
                    End If
                Next
                Return False
                Exit Function
            End If
            Return False
            Exit Function
        End Function

        'FDFAddSubmitAction
        Private Sub FDFAddSubmitAction(ByVal FieldName As String, ByVal whichTrigger As FDFActionTrigger, ByVal theURL As String)
            Try
                Dim xAction As FDFActions
                Dim xCntr As Integer, bFound As Boolean
                Select Case whichTrigger
                    Case FDFActionTrigger.FDFEnter, FDFActionTrigger.FDFExit, FDFActionTrigger.FDFOnBlur, FDFActionTrigger.FDFOnFocus, FDFActionTrigger.FDFUp, FDFActionTrigger.FDFDown
                        bFound = False
                    Case Else
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcBadParameter, "Error: SubmitAction must use FDFActionTrigger [FDFEnter, FDFExit, FDFOnBlur, FDFOnFocus, FDFUp, FDFDown] only", "FDFDoc.FDFAddSubmitAction", 1)
                End Select
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                        If xAction.FieldName = FieldName Then
                            If (whichTrigger = FDFActionTrigger.FDFUp And xAction.Trigger = FDFActionTrigger.FDFUp) Or (Not whichTrigger = FDFActionTrigger.FDFUp) Then
                                xAction.FieldName = FieldName
                                xAction.Trigger = whichTrigger
                                xAction.JavaScript_URL = theURL
                                xAction.ActionType = ActionTypes.Submit
                                Exit Sub
                                bFound = True
                                Exit For
                            Else
                                bFound = False
                                Exit For
                            End If
                        End If
                        xCntr += 1
                    Next
                End If
                If bFound = True Then

                Else
                    If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                        Dim _fa As New FDFActions
                        _fa.FieldName = FieldName
                        _fa.Trigger = whichTrigger
                        _fa.JavaScript_URL = theURL
                        _fa.ActionType = ActionTypes.Submit
                        _FDF(_CurFDFDoc).struc_FDFActions.Add(_fa)
                    ElseIf Not FieldName = "" Then
                        Dim _fa As New FDFActions
                        _fa.FieldName = FieldName
                        _fa.Trigger = whichTrigger
                        _fa.JavaScript_URL = theURL
                        _fa.ActionType = ActionTypes.Submit
                        _FDF(_CurFDFDoc).struc_FDFActions.Add(_fa)
                    End If
                End If
                Array.Sort(_FDF(_CurFDFDoc).struc_FDFActions.ToArray)
            Catch ex As Exception

                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddSubmitAction", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Adds option field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldValue">Field value</param>
        ''' <param name="DisplayName">Display value</param>
        ''' <param name="FieldEnabled">True</param>
        ''' <remarks></remarks>
        Public Sub FDFAddOptField(ByVal FieldName As String, ByVal FieldValue As String, Optional ByVal DisplayName As String = "", Optional ByVal FieldEnabled As Boolean = True)
            Try
                Dim FieldType As FieldType = FieldType.FldOption
                If FieldName = "" Then
                    FieldName = FieldValue
                End If
                If FieldValue = "" Then
                    FieldValue = FieldName
                End If
                If DisplayName = "" Then
                    DisplayName = FieldName
                End If
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then

                    If FieldValue = "" And FieldName = "" And DisplayName = "" Then Exit Sub
                    Dim _fld As New FDFField()
                    _fld.FieldName = FieldName
                    _fld.FieldType = FieldType
                    _fld.DisplayValue.Add(FieldValue)
                    _fld.DisplayName.Add(DisplayName)
                    _fld.FieldEnabled = FieldEnabled
                    _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                ElseIf Not FieldName = "" Then
                    Dim _fld As New FDFField()
                    _fld.FieldName = FieldName
                    _fld.FieldType = FieldType
                    _fld.DisplayValue.Add(FieldValue)
                    _fld.DisplayName.Add(DisplayName)
                    _fld.FieldEnabled = FieldEnabled
                    _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Adds Option field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldValue">Field value array</param>
        ''' <param name="DisplayName">Display value array</param>
        ''' <param name="FieldEnabled">True</param>
        ''' <remarks></remarks>
        Public Sub FDFAddOptField(ByVal FieldName As String, ByVal FieldValue() As String, ByVal DisplayName() As String, Optional ByVal FieldEnabled As Boolean = True)
            Try
                Dim FieldType As FieldType = FieldType.FldOption
                If DisplayName.Length <> FieldValue.Length Then
                    Exit Sub
                End If
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    If FieldValue Is Nothing And FieldName = "" And DisplayName Is Nothing Then Exit Sub
                    Dim _fld As New FDFField()
                    _fld.FieldName = FieldName
                    _fld.FieldType = FieldType
                    _fld.DisplayValue.AddRange(FieldValue)
                    _fld.DisplayName.AddRange(DisplayName)
                    _fld.FieldEnabled = FieldEnabled
                    _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                ElseIf Not FieldName = "" Then
                    Dim _fld As New FDFField()
                    _fld.FieldName = FieldName
                    _fld.FieldType = FieldType
                    _fld.DisplayValue.AddRange(FieldValue)
                    _fld.DisplayName.AddRange(DisplayName)
                    _fld.FieldEnabled = FieldEnabled
                    _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Adds a field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldValue">Field value</param>
        ''' <param name="FieldType">Field type</param>
        ''' <param name="FieldEnabled">True</param>
        ''' <param name="ReplaceField">if true, Replaces field</param>
        ''' <remarks></remarks>
        Public Sub FDFAddField(ByVal FieldName As String, ByVal FieldValue As String, Optional ByVal FieldType As FieldType = FieldType.FldTextual, Optional ByVal FieldEnabled As Boolean = True, Optional ByVal ReplaceField As Boolean = True)
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    Dim blnFound As Boolean = False
                    If ReplaceField = True Then
                        If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                            If _FDF(_CurFDFDoc).struc_FDFFields.Count >= 1 Then
                                For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                                    If Not String_IsNullOrEmpty(xField.FieldName) Then
                                        If FieldName.ToLower = xField.FieldName.ToLower Then
                                            xField.FieldValue.Clear()
                                            xField.FieldValue.Add(FieldValue)
                                            xField.FieldEnabled = FieldEnabled
                                            xField.FieldType = FieldType
                                            blnFound = True
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End If
                    If blnFound = True Then
                        Exit Sub
                    Else
                        If Not FDFGetValue(FieldName) Is Nothing And Not ReplaceField Then Exit Sub
                        Dim xField As New FDFField
                        xField.FieldName = FieldName
                        xField.FieldType = FieldType
                        Dim FldVals() As String, FldVal As String = "", MultiVal As String = ""
                        Select Case FieldType
                            Case FieldType.FldMultiSelect
                                If FieldValue.Split("|").Length > 0 Then
                                    FldVals = FieldValue.Split("|")
                                    For Each FldVal In FldVals
                                        MultiVal &= "(" & FldVal & ")"
                                    Next
                                    xField.FieldValue.Add(MultiVal)
                                    xField.FieldEnabled = FieldEnabled
                                Else
                                    xField.FieldValue.Add(FieldValue)
                                    xField.FieldEnabled = FieldEnabled
                                End If
                            Case FieldType.FldOption
                                xField.FieldValue.Add(FieldValue)
                                xField.FieldEnabled = FieldEnabled
                            Case FieldType.FldTextual
                                xField.FieldValue.Add(FieldValue)
                                xField.FieldEnabled = FieldEnabled
                        End Select
                        _FDF(_CurFDFDoc).struc_FDFFields.Add(xField)
                    End If
                ElseIf Not FieldName = "" Then
                    If Not FDFGetValue(FieldName) Is Nothing And ReplaceField = False Then Exit Sub
                    Dim xField As New FDFField
                    xField.FieldName = FieldName
                    xField.FieldType = FieldType
                    xField.FieldValue.Add(FieldValue)
                    xField.FieldEnabled = FieldEnabled
                    _FDF(_CurFDFDoc).struc_FDFFields.Add(xField)
                End If
                Dim x As Integer
                x = 0
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Adds a field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldValue">Field value array</param>
        ''' <param name="FieldType">field type</param>
        ''' <param name="FieldEnabled">True</param>
        ''' <param name="ReplaceField">If true, replaces field</param>
        ''' <remarks></remarks>
        Public Sub FDFAddField(ByVal FieldName As String, ByVal FieldValue() As String, Optional ByVal FieldType As FieldType = FieldType.FldTextual, Optional ByVal FieldEnabled As Boolean = True, Optional ByVal ReplaceField As Boolean = True)
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    Dim blnFound As Boolean = False
                    If ReplaceField = True Then
                        If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                            If _FDF(_CurFDFDoc).struc_FDFFields.Count >= 1 Then
                                For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                                    If Not String_IsNullOrEmpty(xField.FieldName) Then
                                        If FieldName.ToLower = xField.FieldName.ToLower Then
                                            xField.FieldValue.Clear()
                                            xField.FieldValue.AddRange(FieldValue)
                                            xField.FieldEnabled = FieldEnabled
                                            xField.FieldType = FieldType
                                            blnFound = True
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End If
                    If blnFound = True Then
                        Exit Sub
                    Else
                        If Not FDFGetValue(FieldName) Is Nothing And Not ReplaceField Then Exit Sub
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldType = FieldType
                        Select Case FieldType
                            Case FieldType.FldMultiSelect
                                If FieldValue.Length > 1 Then
                                    _fld.FieldValue.AddRange(FieldValue)
                                ElseIf FieldValue.Length = 1 Then
                                    _fld.FieldValue.AddRange(FieldValue)
                                End If
                            Case FieldType.FldOption
                                If FieldValue.Length = 1 Then
                                    _fld.FieldValue.AddRange(FieldValue)
                                End If
                            Case FieldType.FldTextual
                                If FieldValue.Length = 1 Then
                                    _fld.FieldValue.AddRange(FieldValue)
                                End If
                        End Select
                        _fld.FieldEnabled = FieldEnabled
                        _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                    End If
                ElseIf Not FieldName = "" Then
                    If Not FDFGetValue(FieldName) Is Nothing And Not ReplaceField Then Exit Sub
                    Dim _fld As New FDFField
                    _fld.FieldName = FieldName
                    _fld.FieldType = FieldType
                    _fld.FieldEnabled = FieldEnabled
                    If FieldValue.Length > 1 Then
                        _fld.FieldValue.AddRange(FieldValue)
                    ElseIf FieldValue.Length = 1 Then
                        _fld.FieldValue.AddRange(FieldValue)
                    End If
                    _fld.FieldEnabled = FieldEnabled
                    _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                End If

            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Adds Object
        ''' </summary>
        ''' <param name="objNum">Object number</param>
        ''' <param name="FDFObjectData">Object data</param>
        ''' <param name="Differences"></param>
        ''' <param name="Version"></param>
        ''' <param name="Annotations"></param>
        ''' <remarks></remarks>
        Public Sub FDFAddObject(ByVal objNum As String, Optional ByVal FDFObjectData As String = "", Optional ByVal Differences As String = "", Optional ByVal Version As String = "", Optional ByVal Annotations As String = "")
            Try

                If Not Me._FDFObjects Is Nothing Then
                    Dim _f As New FDFObjects()
                    _f.objNum = objNum
                    _f.objData = FDFObjectData
                    _FDFObjects.Add(_f)
                ElseIf Not objNum = "" Then
                    Dim _f As New FDFObjects()
                    _f.objNum = objNum
                    _f.objData = FDFObjectData
                    _f.objVersion = Version
                    _f.objDifferences = Differences
                    _f.objAnnotations = Annotations
                    _FDFObjects.Add(_f)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetObject", 1)
                Exit Sub
            End Try
        End Sub

        Private Function GetLengthIncrChanges(ByVal xStr As String) As Integer
            Return Len(xStr)
        End Function

        ''' <summary>
        ''' Gets changes
        ''' </summary>
        ''' <param name="intObjNum">Object number</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetChanges(Optional ByVal intObjNum As Integer = -1) As String
            Dim xOutput As String = "", xFDFObjects As FDFObjects, xObjNum As Integer = 0
            xObjNum = 0
            Try
                If intObjNum = -1 Then
                    If Me.FDFObjectCount > 0 Then
                        For Each xFDFObjects In Me._FDFObjects
                            If xFDFObjects.objNum.ToLower <> "1 0 obj" Then
                                xOutput = xOutput & vbNewLine & xFDFObjects.objNum
                                xOutput = xOutput & xFDFObjects.objData
                            End If
                            xObjNum = xObjNum + 1
                        Next
                    End If
                    FDFAppendSaves = xOutput
                    Return xOutput & ""
                Else
                    If Me.FDFObjectCount >= intObjNum Then
                        xFDFObjects = Me._FDFObjects(intObjNum - 1)
                        xOutput = xOutput & vbNewLine & xFDFObjects.objNum
                        xOutput = xOutput & xFDFObjects.objData
                    End If
                    Return xOutput & ""
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFGetChanges", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Public Property DefaultEncoding() As Encoding
            Get
                Return _defaultEncoding
            End Get
            Set(ByVal value As Encoding)
                _defaultEncoding = value
            End Set
        End Property
        Private Function Convert_Encoded_Data(ByVal str As String, ByVal codePage As Integer) As String
            Dim strCode As String = ""
            Dim targetEncoding As Encoding
            Dim encodedChars() As Byte
            targetEncoding = Encoding.GetEncoding(codePage)
            encodedChars = targetEncoding.GetBytes(str)
            strCode = strCode + String.Format("Byte representation of '{0}' in Code Page  '{1}':", str, codePage)
            For i As Integer = 0 To encodedChars.Length - 1 Step 1
                strCode = strCode + String.Format("Byte {0}: {1}", i, encodedChars(i))
            Next
            Return strCode
        End Function
        Private Function Convert_Encoded_Data(ByVal data As Byte(), ByVal codePage As Integer) As String
            Dim strCode As String = ""
            Dim targetEncoding As Encoding
            Dim encodedChars() As Byte
            targetEncoding = Encoding.GetEncoding(codePage)
            Dim str As String = targetEncoding.GetString(data)
            encodedChars = targetEncoding.GetBytes(str)
            strCode = strCode + String.Format("Byte representation of '{0}' in Code Page  '{1}':", str, codePage)
            For i As Integer = 0 To encodedChars.Length - 1 Step 1
                strCode = strCode + String.Format("Byte {0}: {1}", i, encodedChars(i))
            Next
            Return strCode
        End Function

        Private Function OpenFile(ByVal FullPath As String) As String
            Dim strContents As String
            Dim objReader As StreamReader
            Try
                If File.Exists(FullPath) Then
                    objReader = New StreamReader(FullPath)
                    strContents = objReader.ReadToEnd()
                    objReader.Close()
                    Return strContents
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: Path Not Found", "FDFDoc.OpenFile", 1)
                    Return ""
                    Exit Function
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.OpenFile", 1)
                Return Nothing
                Exit Function
            End Try
            Return ""
        End Function
        Private Function PDFExportString(ByVal PDFPath As String) As String
            Dim PDFFile As String
            If IsValidUrl(PDFPath) Then
                Dim wrq As WebRequest = WebRequest.Create(PDFPath)
                Dim wrp As WebResponse = wrq.GetResponse()
                Dim reader As StreamReader = New StreamReader(wrp.GetResponseStream())
                PDFFile = reader.ReadToEnd
                Return PDFFile
            ElseIf File.Exists(PDFPath) Then
                PDFFile = Me.OpenFile(PDFPath)
                Return PDFFile
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: PDF File Not Found", "FDFDoc.PDFExportString", 1)
                Return ""
            End If
        End Function
        Private Function PDFExportStream(ByVal PDFPath As String) As Stream
            If IsValidUrl(PDFPath) Then
                Dim URL As String = PDFPath
                Dim request As WebRequest = WebRequest.Create(URL)
                Dim response As WebResponse = request.GetResponse()
                Dim input As Stream = response.GetResponseStream()
                Return input
            ElseIf File.Exists(PDFPath) Then
                Dim rdStream As New FileStream(PDFPath, FileMode.Open, FileAccess.Read)
                Return rdStream
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: PDF File Not Found", "FDFDoc.PDFExportStream", 1)
                Return Nothing
            End If
        End Function
        Private Function PDFExportBuffer(ByVal PDFPath As String) As Char()
            Dim PDFFile As String
            If IsValidUrl(PDFPath) Then
                Dim wrq As WebRequest = WebRequest.Create(PDFPath)
                Dim wrp As WebResponse = wrq.GetResponse()
                Dim reader As StreamReader = New StreamReader(wrp.GetResponseStream())
                PDFFile = reader.ReadToEnd
                Return ExportBuffer(PDFFile)
            ElseIf File.Exists(PDFPath) Then
                PDFFile = Me.OpenFile(PDFPath)
                Return ExportBuffer(PDFFile)
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: PDF File Not Found", "FDFDoc.PDFExportBuffer", 1)
                Return Nothing
            End If
        End Function
        Private Function PDFExportByte(ByVal PDFPath As String) As Byte()
            Dim PDFFile As String
            If IsValidUrl(PDFPath) Then
                Dim wrq As WebRequest = WebRequest.Create(PDFPath)
                Dim wrp As WebResponse = wrq.GetResponse()
                Dim reader As StreamReader = New StreamReader(wrp.GetResponseStream())
                PDFFile = reader.ReadToEnd
                Return ExportByte(PDFFile)
            ElseIf File.Exists(PDFPath) Then
                PDFFile = Me.OpenFile(PDFPath)
                Return ExportByte(PDFFile)
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: PDF File Not Found", "FDFDoc.PDFExportBuffer", 1)
                Return Nothing
            End If
        End Function
        Private Function IsValidUrl(ByVal url As String) As Boolean
            Return System.Text.RegularExpressions.Regex.IsMatch(url, "^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$")
        End Function
        Private Function WriteAppendSaves(ByVal xType As FDFType) As String
            Dim retString As String = ""
            Select Case xType
                Case FDFType.FDF
                    retString = GetChanges()
                Case FDFType.xFDF
                    Return ""
                Case FDFType.XML
                    Return ""
            End Select
            Return retString
        End Function
        Private Sub Reset_CurFDFDoc()
            _CurFDFDoc = 0
        End Sub

        Private Function WriteHead(ByVal xType As FDFType, Optional ByVal AppendSaves As Boolean = False) As String
            Dim retString As String = ""
            Try
                Reset_CurFDFDoc()
                Select Case xType
                    Case FDFType.FDF
                        If AppendSaves And FDFHasChanges Then
                            '	"%FDF-1.2" & vbNewLine & 
                            retString = "%FDF-1.2" & vbNewLine & "%βγΟΣ" & vbNewLine & "1 0 obj << /Version/" & IIf(_FDF(_CurFDFDoc).Version <> "", _FDF(_CurFDFDoc).Version, "1.4") & " /FDF " & IIf(_FDF(_CurFDFDoc).Annotations <> "" And AppendSaves = True, "<< /Annots [" & _FDF(_CurFDFDoc).Annotations & "] >>", " << ")

                        Else
                            '"%FDF-1.2" & vbNewLine & 
                            retString = "%FDF-1.2" & vbNewLine & "%βγΟΣ" & vbNewLine & "1 0 obj << /Version/1.6 /FDF << "
                        End If
                    Case FDFType.xFDF
                        '<?xml version="1.0" encoding="UTF-8"?>
                        retString = "<?xml version=""1.0"" encoding=""UTF-8""?><xfdf xmlns=""http://ns.adobe.com/xfdf/"" xml:space=""preserve"">"
                    Case FDFType.XML
                        retString = "<?xml version=""1.0"" encoding=""UTF-8""?>"
                    Case FDFType.XDP
                        'retString = "<?xml version=""1.0"" encoding=""UTF-8""?><?xfa generator=""XFA2_4"" APIVersion=""2.6.7120.0""?><v xmlns:xdp=""http://ns.adobe.com/xdp/""><xfa:datasets xmlns:xfa=""http://www.xfa.org/schema/xfa-data/1.0/""><xfa:data>"
                        retString = "<?xml version=""1.0"" encoding=""UTF-8""?><?xfa generator=""XFA2_4"" APIVersion=""2.6.7120.0""?><xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""><xfa:datasets xmlns:xfa=""http://www.xfa.org/schema/xfa-data/1.0/""><xfa:data>"
                End Select
                Return retString
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteHead", 1)
                Return ""
            End Try
        End Function

        Private Function WriteTemplates(ByVal xType As FDFType) As String
            Dim retString As String = ""
            Try
                Select Case xType
                    Case FDFType.FDF
                        '	"%FDF-1.2" & vbNewLine & 
                        If Not _FDF.Count <= 0 Then
                            If _FDF.Count >= 1 Then
                                retString = " /Pages /Templates ["
                                For _CurFDFDoc = 1 To _FDF.Count - 1 Step 1
                                    retString = retString & "<< /TRef /F (" & _FDF(_CurFDFDoc).FileName & ") /Name (" & _FDF(_CurFDFDoc).TmpTemplateName & ") " & IIf(_FDF(_CurFDFDoc).TmpRename, " ", " /Rename false ") & IIf(FDFFields.Length > 0, Me.WriteFields(xType), "") & " >> "    '" /Fields " & 
                                    'retString = Me.WriteFields(xType) & ""
                                Next
                                retString = retString & "] "
                            End If
                        End If
                    Case Else
                        retString = ""
                End Select
                Return retString
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try
        End Function

        Private Function WriteFields(ByVal xType As FDFType) As String
            Dim retString As String = "", xFDFField As FDFField
            Dim FldValue As String = ""
            Try
                Select Case xType
                    Case FDFType.FDF
                        'If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 And Not FDFFields Is Nothing And Not _FDF(_CurFDFDoc).struc_NamedActions Is Nothing And Not _FDF(_CurFDFDoc).struc_ImportDataAction Is Nothing And Not _FDF(_CurFDFDoc).struc_HideActions Is Nothing Then
                        '	retString = " /Fields ["
                        'Else
                        '	retString = " /Fields ["
                        '                  End If
                        retString = " /Fields ["
                        Me.ResetActions()
                        If Not _FDF.Count <= 0 Then
                            Dim FormIndex As Integer = -1
                            For Each XDPDoc1 As FDFDoc_Class In _FDF
                                FormIndex += 1
                                If Not XDPDoc1.struc_FDFFields.Count <= 0 Then
                                    If XDPDoc1.struc_FDFFields.Count > 0 Then 'XDPDoc1.DocType = FDFDocType.XDPForm And
                                        If Not XDPDoc1.struc_FDFFields.Count <= 0 Then
                                            For Each xFDFField In XDPDoc1.struc_FDFFields
                                                If xFDFField.FieldEnabled Then
                                                    If xFDFField.FieldType = FieldType.FldOption And Not xFDFField.FieldValue.Count <= 0 Then
                                                        Dim valCntr As Integer
                                                        Dim Values, DisplayNames As String
                                                        FldValue = ""
                                                        If xFDFField.DisplayValue.Count <= 0 And xFDFField.DisplayName.Count <= 0 Then
                                                            FldValue = xFDFField.FieldValue(0) & ""
                                                            If Not FldValue.IndexOf("\(") >= 0 Then
                                                                FldValue = FldValue.Replace("(", "\(")
                                                            End If
                                                            If Not FldValue.IndexOf("\)") >= 0 Then
                                                                FldValue = FldValue.Replace(")", "\)")
                                                            End If
                                                            If Not FldValue.IndexOf("\'") >= 0 Then
                                                                FldValue = FldValue.Replace("'", "\'")
                                                            End If
                                                            If Not FldValue.IndexOf("\") >= 0 Then
                                                                FldValue = FldValue.Replace("", "\'")
                                                            End If
                                                            If Not FldValue.IndexOf("\" & Chr(13)) > 0 Then
                                                                FldValue = FldValue.Replace("\" & Chr(13), "")  ' \r\n
                                                            End If
                                                            If Not FldValue.IndexOf("\r") > 0 Then
                                                                FldValue = FldValue.Replace(vbNewLine, "\r")            ' \r\n
                                                            End If
                                                            retString = retString & "<< /T (" & xFDFField.FieldName & ") /V (" & FldValue & ") " & " " & FDFGetAllActionsForField(xFDFField.FieldName) & " >> "
                                                        Else
                                                            For valCntr = 0 To xFDFField.DisplayValue.Count - 1
                                                                Values = xFDFField.DisplayValue(valCntr) & ""
                                                                If Not Values.IndexOf("\(") >= 0 Then
                                                                    Values = Values.Replace("(", "\(")
                                                                End If
                                                                If Not Values.IndexOf("\)") >= 0 Then
                                                                    Values = Values.Replace(")", "\)")
                                                                End If
                                                                '
                                                                If Not Values.IndexOf("\'") >= 0 Then
                                                                    Values = Values.Replace("'", "\'")
                                                                End If
                                                                If Not Values.IndexOf("\") >= 0 Then
                                                                    Values = Values.Replace("", "\'")
                                                                End If
                                                                If Values.IndexOf(Chr(13)) > 0 Then
                                                                    Values = Values.Replace(Chr(13), "\r")    ' \r\n
                                                                End If
                                                                If Values.IndexOf(vbNewLine) > 0 Then
                                                                    Values = Values.Replace(vbNewLine, "\r")           ' \r\n
                                                                End If
                                                                If Values.IndexOf(Environment.NewLine) > 0 Then
                                                                    Values = Values.Replace(Environment.NewLine, "\r")             ' \r\n
                                                                End If

                                                                DisplayNames = xFDFField.DisplayName(valCntr) & ""
                                                                If Not DisplayNames.IndexOf("\(") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace("(", "\(")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\)") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace(")", "\)")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\'") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace("'", "\'")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace("", "\'")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\" & Chr(13)) > 0 Then
                                                                    DisplayNames = DisplayNames.Replace("\" & Chr(13), "")      ' \r\n
                                                                End If
                                                                If Not DisplayNames.IndexOf("\r") > 0 Then
                                                                    DisplayNames = DisplayNames.Replace(vbNewLine, "\r")  ' \r\n
                                                                End If

                                                                FldValue &= " [(" & Values & ")(" & DisplayNames & ")] "
                                                            Next
                                                            retString = retString & "<< /T (" & xFDFField.FieldName & ") /Opt [" & FldValue & "] " & IIf(xFDFField.FieldValue.Count > 0, " /V (" & xFDFField.FieldValue(0) & ") ", " ") & FDFGetAllActionsForField(xFDFField.FieldName) & " >> "
                                                        End If
                                                    ElseIf xFDFField.FieldType = FieldType.FldOption And xFDFField.FieldValue.Count <= 0 Then
                                                        Dim valCntr As Integer
                                                        Dim Values, DisplayNames As String
                                                        FldValue = ""
                                                        If Not xFDFField.DisplayValue.Count <= 0 And Not xFDFField.DisplayName.Count <= 0 Then
                                                            For valCntr = 0 To xFDFField.DisplayValue.Count - 1
                                                                Values = xFDFField.DisplayValue(valCntr) & ""
                                                                If Not Values.IndexOf("\(") >= 0 Then
                                                                    Values = Values.Replace("(", "\(")
                                                                End If
                                                                If Not Values.IndexOf("\)") >= 0 Then
                                                                    Values = Values.Replace(")", "\)")
                                                                End If
                                                                '
                                                                If Not Values.IndexOf("\'") >= 0 Then
                                                                    Values = Values.Replace("'", "\'")
                                                                End If
                                                                If Not Values.IndexOf("\") >= 0 Then
                                                                    Values = Values.Replace("", "\'")
                                                                End If
                                                                If Values.IndexOf(Chr(13)) > 0 Then
                                                                    Values = Values.Replace(Chr(13), "\r")    ' \r\n
                                                                End If
                                                                If Values.IndexOf(vbNewLine) > 0 Then
                                                                    Values = Values.Replace(vbNewLine, "\r")           ' \r\n
                                                                End If
                                                                If Values.IndexOf(Environment.NewLine) > 0 Then
                                                                    Values = Values.Replace(Environment.NewLine, "\r")             ' \r\n
                                                                End If

                                                                DisplayNames = xFDFField.DisplayName(valCntr) & ""
                                                                If Not DisplayNames.IndexOf("\(") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace("(", "\(")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\)") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace(")", "\)")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\'") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace("'", "\'")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace("", "\'")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\" & Chr(13)) > 0 Then
                                                                    DisplayNames = DisplayNames.Replace("\" & Chr(13), "")      ' \r\n
                                                                End If
                                                                If Not DisplayNames.IndexOf("\r") > 0 Then
                                                                    DisplayNames = DisplayNames.Replace(vbNewLine, "\r")  ' \r\n
                                                                End If
                                                                FldValue &= " [(" & Values & ")(" & DisplayNames & ")] "
                                                            Next
                                                            retString = retString & "<< /T (" & xFDFField.FieldName & ") /Opt [" & FldValue & "] " & FDFGetAllActionsForField(xFDFField.FieldName) & " >> "
                                                        End If
                                                    ElseIf xFDFField.FieldType = FieldType.FldMultiSelect And Not xFDFField.FieldValue.Count <= 0 Then
                                                        Dim valCntr As Integer
                                                        Dim Values, DisplayNames As String
                                                        FldValue = ""
                                                        If xFDFField.DisplayValue.Count <= 0 And xFDFField.DisplayName.Count <= 0 Then
                                                            FldValue = xFDFField.FieldValue(0) & ""
                                                            If Not FldValue.IndexOf("\(") >= 0 Then
                                                                FldValue = FldValue.Replace("(", "\(")
                                                            End If
                                                            If Not FldValue.IndexOf("\)") >= 0 Then
                                                                FldValue = FldValue.Replace(")", "\)")
                                                            End If
                                                            If Not FldValue.IndexOf("\'") >= 0 Then
                                                                FldValue = FldValue.Replace("'", "\'")
                                                            End If
                                                            If Not FldValue.IndexOf("\") >= 0 Then
                                                                FldValue = FldValue.Replace("", "\'")
                                                            End If
                                                            If FldValue.IndexOf(Chr(13)) > 0 Then
                                                                FldValue = FldValue.Replace(Chr(13), "\r")  ' \r\n
                                                            End If
                                                            If FldValue.IndexOf(vbNewLine) > 0 Then
                                                                FldValue = FldValue.Replace(vbNewLine, "\r")            ' \r\n
                                                            End If
                                                            If FldValue.IndexOf(Environment.NewLine) > 0 Then
                                                                FldValue = FldValue.Replace(Environment.NewLine, "\r")          ' \r\n
                                                            End If
                                                            retString = retString & "<< /T (" & xFDFField.FieldName & ") /V ["
                                                            If xFDFField.FieldValue.Count > 0 Then
                                                                For Each FldValue In xFDFField.FieldValue
                                                                    retString = retString & "(" & FldValue & ")"
                                                                Next
                                                            End If
                                                            retString = retString & "] " & " " & FDFGetAllActionsForField(xFDFField.FieldName) & " >> "
                                                        Else
                                                            For valCntr = 0 To xFDFField.DisplayValue.Count - 1
                                                                Values = xFDFField.DisplayValue(valCntr) & ""
                                                                If Not Values.IndexOf("\(") >= 0 Then
                                                                    Values = Values.Replace("(", "\(")
                                                                End If
                                                                If Not Values.IndexOf("\)") >= 0 Then
                                                                    Values = Values.Replace(")", "\)")
                                                                End If
                                                                '
                                                                If Not Values.IndexOf("\'") >= 0 Then
                                                                    Values = Values.Replace("'", "\'")
                                                                End If
                                                                If Not Values.IndexOf("\") >= 0 Then
                                                                    Values = Values.Replace("", "\'")
                                                                End If
                                                                If Values.IndexOf(Chr(13)) > 0 Then
                                                                    Values = Values.Replace(Chr(13), "\r")    ' \r\n
                                                                End If
                                                                If Values.IndexOf(vbNewLine) > 0 Then
                                                                    Values = Values.Replace(vbNewLine, "\r")           ' \r\n
                                                                End If
                                                                If Values.IndexOf(Environment.NewLine) > 0 Then
                                                                    Values = Values.Replace(Environment.NewLine, "\r")             ' \r\n
                                                                End If

                                                                DisplayNames = xFDFField.DisplayName(valCntr) & ""
                                                                If Not DisplayNames.IndexOf("\(") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace("(", "\(")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\)") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace(")", "\)")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\'") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace("'", "\'")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\") >= 0 Then
                                                                    DisplayNames = DisplayNames.Replace("", "\'")
                                                                End If
                                                                If Not DisplayNames.IndexOf("\" & Chr(13)) > 0 Then
                                                                    DisplayNames = DisplayNames.Replace("\" & Chr(13), "")      ' \r\n
                                                                End If
                                                                If Not DisplayNames.IndexOf("\r") > 0 Then
                                                                    DisplayNames = DisplayNames.Replace(vbNewLine, "\r")  ' \r\n
                                                                End If
                                                                FldValue &= " [(" & Values & ")(" & DisplayNames & ")] "
                                                            Next
                                                            retString = retString & "<< /T (" & xFDFField.FieldName & ") /Opt [" & FldValue & "] /V ["
                                                            If xFDFField.FieldValue.Count > 0 Then
                                                                For Each FldValue In xFDFField.FieldValue
                                                                    retString = retString & "(" & FldValue & ")"
                                                                Next
                                                            End If
                                                            retString = retString & "] " & " " & FDFGetAllActionsForField(xFDFField.FieldName) & " >> "
                                                        End If
                                                    ElseIf xFDFField.FieldType = FieldType.FldTextual And Not xFDFField.FieldValue.Count <= 0 Then
                                                        FldValue = xFDFField.FieldValue(0)
                                                        If Not FldValue.IndexOf("\(") >= 0 Then
                                                            FldValue = FldValue.Replace("(", "\(")
                                                        End If
                                                        If Not FldValue.IndexOf("\)") >= 0 Then
                                                            FldValue = FldValue.Replace(")", "\)")
                                                        End If
                                                        If Not FldValue.IndexOf("\'") >= 0 Then
                                                            FldValue = FldValue.Replace("'", "\'")
                                                        End If
                                                        If Not FldValue.IndexOf("\") >= 0 Then
                                                            FldValue = FldValue.Replace("", "\'")
                                                        End If
                                                        If FldValue.IndexOf(Chr(13)) > 0 Then
                                                            FldValue = FldValue.Replace(Chr(13), "\r")    ' \r\n
                                                        End If
                                                        If FldValue.IndexOf(vbNewLine) > 0 Then
                                                            FldValue = FldValue.Replace(vbNewLine, "\r")        ' \r\n
                                                        End If
                                                        If FldValue.IndexOf(Environment.NewLine) > 0 Then
                                                            FldValue = FldValue.Replace(Environment.NewLine, "\r")      ' \r\n
                                                        End If
                                                        retString = retString & "<< /T (" & xFDFField.FieldName & ") /V (" & FldValue & ") " & " " & FDFGetAllActionsForField(xFDFField.FieldName) & " >> "
                                                    ElseIf xFDFField.FieldType = FieldType.FldButton Then
                                                        retString = retString & "<< /T (" & xFDFField.FieldName & ") " & " " & FDFGetAllActionsForField(xFDFField.FieldName) & " >> "
                                                    End If
                                                End If
                                            Next
                                        End If

                                    End If
                                End If
                            Next
                            retString = retString & " " & Me.FDFGetRemainingActions(True)
                            If retString.Contains(" /Fields [") Then
                                retString = retString & "] "
                            Else
                                retString = ""
                            End If
                            If _CurFDFDoc = 0 Then
                                If HasDocJavaScripts() Or HasDocOnImportJavaScripts() Then
                                    retString = retString & " /JavaScript "
                                    If HasDocJavaScripts() Then
                                        retString = retString & " << " & " /Doc [ " & GetDocJavaScripts(False) & "] "
                                        If HasDocOnImportJavaScripts() Then
                                            retString = retString & Me.FDFGetImportJSActions(False)
                                        Else
                                            retString = retString
                                        End If
                                        retString = retString & " >>"
                                    Else
                                        If HasDocOnImportJavaScripts() Then
                                            retString = retString & Me.FDFGetImportJSActions(True)
                                        Else
                                            retString = retString
                                        End If
                                        retString = retString
                                    End If

                                End If
                            End If
                        End If
                    Case FDFType.xFDF
                        retString = "<fields>"
                        If Not _FDF.Count <= 0 Then
                            Dim FormIndex As Integer = -1
                            For Each XDPDoc1 As FDFDoc_Class In _FDF
                                FormIndex += 1
                                If Not XDPDoc1.struc_FDFFields.Count <= 0 Then
                                    For Each xFDFField In XDPDoc1.struc_FDFFields
                                        If xFDFField.FieldEnabled Then
                                            Select Case xFDFField.FieldType
                                                Case FieldType.FldOption And Not xFDFField.FieldValue.Count <= 0
                                                    retString = retString & "<field name=""" & xFDFField.FieldName & """><value>" & xFDFField.FieldValue(0) & "</value></field>"
                                                Case FieldType.FldMultiSelect And Not xFDFField.FieldValue.Count <= 0
                                                    Dim FldsVal() As String = xFDFField.FieldValue.ToArray
                                                    Dim FldVal As String, FldNum As Integer = 0
                                                    For Each FldVal In FldsVal
                                                        FldNum += 1
                                                        If FldNum = 1 Then
                                                            FldValue = FldValue & "<value>" & FldVal.TrimStart("(") & "</value>"
                                                        ElseIf FldNum = FldsVal.Length Then
                                                            FldValue = FldValue & "<value>" & FldVal.TrimEnd(")") & "</value>"       '
                                                        Else
                                                            FldValue = FldValue & "<value>" & FldVal & "</value>"
                                                        End If
                                                    Next

                                                    retString = retString & "<field name=""" & xFDFField.FieldName & """>" & FldValue & "</field>"
                                                Case FieldType.FldTextual
                                                    retString = retString & "<field name=""" & xFDFField.FieldName & """><value>" & xFDFField.FieldValue(0) & "</value></field>"
                                            End Select
                                        End If
                                    Next
                                End If
                            Next
                        End If
                        retString = retString & "</fields>"
                        Dim intX As Integer = InStrRev(retString, "</fields>", -1, CompareMethod.Text)
                        retString = retString.Substring(0, intX + 8)

                    Case FDFType.XML
                        retString = "<fields>" & WriteXMLFormFields() & "</fields>"
                        Dim intX As Integer = InStrRev(retString, "</fields>", -1, CompareMethod.Text)
                        retString = retString.Substring(0, intX + 8)

                    Case FDFType.XDP
                        retString = WriteXDPFormFields()
                End Select
                Me.ResetActions()
                Return retString
                'Return retString.Trim(" ".ToCharArray) & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteFields", 1)
                Return ""
            End Try
            Return ""
        End Function
        Private Function ReplaceXMLReservedCharacters(ByVal strInput As String) As String
            strInput = strInput.Replace("&", "_")
            strInput = strInput.Replace("<", "_")
            strInput = strInput.Replace(">", "_")
            strInput = strInput.Replace("""", "_")
            strInput = strInput.Replace("'", "_")
            strInput = strInput.Replace(" ", "_")
            Return strInput & ""
            'strInput = strInput.Replace("", "")
        End Function
        Private Function CheckXMLReservedWords(ByVal strInput As String) As String
            strInput = strInput.Replace("&amp;", "&")
            strInput = strInput.Replace("&", "&amp;")
            strInput = strInput.Replace("<", "&lt;")
            strInput = strInput.Replace(">", "&gt;")
            strInput = strInput.Replace("""", "&quot;")
            strInput = strInput.Replace("'", "&apos;")
            Return strInput & ""
        End Function

        Private Function WriteEnd(ByVal xType As FDFType, Optional ByVal AppendSaves As Boolean = False) As String
            Dim retString As String = ""
            Me._CurFDFDoc = 0
            Try
                Select Case xType
                    Case FDFType.FDF
                        '/Differences 5 0 R
                        If AppendSaves And FDFHasChanges Then
                            '/Target (_blank)
                            retString = IIf(FDFGetFile = "", "", " /F (" & FDFGetFile & ") ") & IIf(FDFGetTargetFrame = "", "", " /Target (" & FDFGetTargetFrame & ") ") & IIf(FDFGetStatus = "", "", " /Status (" & FDFGetStatus & ") ") & IIf(_FDF(_CurFDFDoc).Differences <> "" And AppendSaves = True, " /Differences " & _FDF(_CurFDFDoc).Differences, "") & ">> >> " & vbNewLine & "endobj" & vbNewLine & IIf(FDFHasChanges = True And AppendSaves = True, WriteAppendSaves(xType), "") & vbNewLine & "trailer" & vbNewLine & "<</Root 1 0 R>>" & vbNewLine & "%%EOF"
                        Else
                            retString = IIf(FDFGetFile = "", "", " /F (" & FDFGetFile & ") ") & IIf(FDFGetTargetFrame = "", "", " /Target (" & FDFGetTargetFrame & ") ") & IIf(FDFGetStatus = "", "", " /Status (" & FDFGetStatus & ")") & ">> >> " & vbNewLine & "endobj" & vbNewLine & "trailer" & vbNewLine & "<</Root 1 0 R>>" & vbNewLine & "%%EOF"
                        End If

                    Case FDFType.xFDF
                        retString = IIf(FDFGetFile = "", "", "<f href=""" & FDFGetFile & """/>") & "</xfdf>"
                    Case FDFType.XML
                        retString = ""
                    Case FDFType.XDP
                        retString = "</xfa:data></xfa:datasets><pdf href=""" & FDFGetFile & """ xmlns=""http://ns.adobe.com/xdp/pdf/""/></xdp:xdp>"
                End Select
                Return retString
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteEnd", 1)
                Return ""
            End Try
        End Function
        ' FDF EXPORT
        Private Function FDFExportString(Optional ByVal AppendSaves As Boolean = True) As String
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.FDF, AppendSaves)
            FDFExport = FDFExport & WriteFields(FDFType.FDF)
            FDFExport = FDFExport & WriteEnd(FDFType.FDF, AppendSaves)
            Return FDFExport
        End Function
        Private Function FDFExportBuffer(Optional ByVal AppendSaves As Boolean = True) As Char()
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.FDF, AppendSaves)
            FDFExport = FDFExport & WriteFields(FDFType.FDF)
            FDFExport = FDFExport & WriteEnd(FDFType.FDF, AppendSaves)
            Return ExportBuffer(FDFExport)
        End Function
        Private Function XMLExportString() As String
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XML)
            FDFExport = FDFExport & WriteFields(FDFType.XML)
            FDFExport = FDFExport & WriteEnd(FDFType.XML)
            Return FDFExport
        End Function

        Private Function XMLExportBuffer() As Char()
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XML)
            FDFExport = FDFExport & WriteFields(FDFType.XML)
            FDFExport = FDFExport & WriteEnd(FDFType.XML)
            Return ExportBuffer(FDFExport)
        End Function

        Private Function XMLExportStream() As Stream
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XML)
            FDFExport = FDFExport & WriteFields(FDFType.XML)
            FDFExport = FDFExport & WriteEnd(FDFType.XML)
            Return ExportStream(FDFExport)
        End Function

        Private Function XDPExportString() As String
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XDP)
            FDFExport = FDFExport & WriteFields(FDFType.XDP)
            FDFExport = FDFExport & WriteEnd(FDFType.XDP)
            Return FDFExport
        End Function
        Private Function XDPExportBuffer() As Char()
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XDP)
            FDFExport = FDFExport & WriteFields(FDFType.XDP)
            FDFExport = FDFExport & WriteEnd(FDFType.XDP)
            Return ExportBuffer(FDFExport)
        End Function
        Private Function XDPExportStream() As Stream
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XDP)
            FDFExport = FDFExport & WriteFields(FDFType.XDP)
            FDFExport = FDFExport & WriteEnd(FDFType.XDP)
            Return ExportStream(FDFExport)
        End Function


        Private Function XFDFExportString() As String
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.xFDF)
            FDFExport = FDFExport & WriteFields(FDFType.xFDF)
            FDFExport = FDFExport & WriteEnd(FDFType.xFDF)
            Return FDFExport & ""
        End Function
        Private Function XFDFExportBuffer() As Char()
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.xFDF)
            FDFExport = FDFExport & WriteFields(FDFType.xFDF)
            FDFExport = FDFExport & WriteEnd(FDFType.xFDF)
            Return ExportBuffer(FDFExport)
        End Function
        Private Function PDFExportString() As String
            Dim FDFExport() As Byte
            FDFExport = PDFData
            Return ByteArray2String(FDFExport)
        End Function
        Private Function PDFExportBuffer(Optional ByVal AppendSaves As Boolean = True) As Char()
            Dim FDFExport() As Byte
            FDFExport = PDFData
            Select Case Me.Determine_Type(Me.FDFGetFile())
                Case FDFType.PDF
                    PDFData = PDFMergeFDF2Buf(Me.FDFGetFile, False, "")
                    ByteArray2CharArray(PDFData)
                Case FDFType.XPDF
                    PDFData = PDFMergeXDP2Buf(Me.FDFGetFile, False, "")
                    ByteArray2CharArray(PDFData)
                Case Else
                    Return Nothing
            End Select
            Return Nothing
        End Function
        Private Function getEncodedString(ByVal str As String) As String

            Dim fileStream As New MemoryStream(StringCharToByteArray(str))
            Dim encodingString As String
            Dim _detectedEncoding As Encoding
            _detectedEncoding = Encoding.GetEncoding("iso-8859-1")
            Dim stream_reader As IO.StreamReader = Nothing
            Try
                stream_reader = New IO.StreamReader(fileStream, _detectedEncoding, True)
            Catch ex As Exception

            Finally
                encodingString = stream_reader.ReadToEnd
                stream_reader.Close()            ' clean up
            End Try
            Return encodingString
        End Function

        Private Function PDFExportStream() As Stream
            Dim FDFExport() As Byte
            FDFExport = PDFData
            Select Case Me.Determine_Type(Me.FDFGetFile())
                Case FDFType.PDF
                    PDFData = PDFMergeFDF2Buf(Me.FDFGetFile, False, "")
                    Return ByteArray2Stream(PDFData)
                Case FDFType.XPDF
                    PDFData = PDFMergeXDP2Buf(Me.FDFGetFile, False, "")
                    Return ByteArray2Stream(PDFData)
                Case Else
                    Return Nothing
            End Select
        End Function
        Private Function PDFExportByte() As Byte()
            Dim FDFExport() As Byte
            FDFExport = PDFData
            Select Case Me.Determine_Type(Me.FDFGetFile())
                Case FDFType.PDF
                    PDFData = PDFMergeFDF2Buf(Me.FDFGetFile, False, "")
                    Return PDFData
                Case FDFType.XPDF
                    PDFData = PDFMergeXDP2Buf(Me.FDFGetFile, False, "")
                    Return PDFData
                Case Else
                    Return Nothing
            End Select
            Return FDFExport
        End Function
        Private Function PDFExportByte(ByVal preserveRights As Boolean, ByVal removeRights As Boolean) As Byte()
            Dim FDFExport() As Byte
            Me.PreserveUsageRights = preserveRights
            Me.RemoveUsageRights = removeRights
            FDFExport = PDFData
            Select Case Me.Determine_Type(Me.FDFGetFile())
                Case FDFType.PDF
                    PDFData = PDFMergeFDF2Buf(Me.FDFGetFile, False, "")
                    Return PDFData
                Case FDFType.XPDF
                    PDFData = PDFMergeXDP2Buf(Me.FDFGetFile, False, "")
                    Return PDFData
                Case Else
                    Return Nothing
            End Select
            Return FDFExport
        End Function
        Private Function ReadStream(ByVal InStream As Stream, Optional ByVal omitReturn As Boolean = True) As String
            Dim iCounter As Long = 0, StreamLength As Long = 0, iRead As Integer
            Dim OutString As String = ""
            StreamLength = CInt(InStream.Length)
            Dim bytearray(StreamLength) As Byte
            Try
                iRead = InStream.Read(bytearray, 0, StreamLength)
                InStream.Close()
                For iCounter = 0 To StreamLength - 1
                    If bytearray(iCounter) = 10 And omitReturn Then
                        ' Leave Blank
                    Else
                        OutString &= Chr(bytearray(iCounter))
                    End If
                Next
                Return OutString
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.ReadStream", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Private Function ByteArray2String(ByVal bytearray() As Byte, Optional ByVal omitReturn As Boolean = True) As String
            Try
                Dim str As String
                str = _defaultEncoding.GetString(bytearray)
                Return str
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.ByteArray2String", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Private Function ByteArray2CharArray(ByVal bytearray() As Byte, Optional ByVal omitReturn As Boolean = True) As Char()
            Try
                Dim iCounter As Long = 0, StreamLength As Long = 0
                Dim OutString As String = ""
                StreamLength = CInt(bytearray.Length)
                For iCounter = 0 To StreamLength - 1
                    If bytearray(iCounter) = 10 And omitReturn Then
                        ' Leave Blank
                    Else
                        OutString &= Chr(bytearray(iCounter))
                    End If
                Next
                Return OutString.ToCharArray
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.ByteArray2CharArray", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Private Function ByteArray2Stream(ByVal Bytes() As Byte) As Stream
            Dim iCounter As Long = 0, StreamLength As Long = 0
            Dim InString As String = "", outStream As New MemoryStream

            StreamLength = CInt(Bytes.Length)
            Dim bytearray(StreamLength) As Byte
            Try
                outStream.Write(Bytes, 0, StreamLength)
                Return outStream
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.WriteBytes", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Private Function WriteStream(ByVal InStream As Stream) As Byte()

            Dim iCounter As Long = 0, StreamLength As Long = 0, iRead As Long = 0
            Dim InString As String = ""
            StreamLength = CInt(InStream.Length)
            Dim bytearray(StreamLength) As Byte
            Try
                iRead = InStream.Read(bytearray, 0, StreamLength)
                InStream.Close()
                Return bytearray
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.WriteBytes", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Private Function StringCharToByteArray(ByVal str As String) As Byte()
            'e.g. "abcdefg" to {a,b,c,d,e,f,g}
            Dim s As Char()
            s = str.ToCharArray
            Dim b(s.Length - 1) As Byte
            Dim i As Integer
            For i = 0 To s.Length - 1
                b(i) = Convert.ToByte(s(i))
            Next
            Return b
        End Function

        Private Function StringToByteArray(ByVal str As String) As Byte()
            ' e.g. "1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16" to 
            '{1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16}
            Dim s As String()
            s = str.Split(" ")
            Dim b(s.Length - 1) As Byte
            Dim i As Integer
            For i = 0 To s.Length - 1
                'b(i) = Convert.ToByte(s(i))
                b(i) = Convert.ToByte(s(i))
            Next
            Return b
        End Function

        Private Function ByteArrayToString(ByVal b() As Byte) As String
            Dim str As String
            str = _defaultEncoding.GetString(b)
            Return str
        End Function
        Private Function ByteArrayToASCII(ByVal b() As Byte) As String
            Dim i As Integer
            Dim s As New System.Text.StringBuilder
            For i = 0 To b.Length - 1
                If i <> b.Length - 1 Then
                    s.Append(Chr(b(i)) & " ")
                Else
                    If Not b(i) = 10 Then
                        s.Append(Chr(b(i)))
                    End If
                End If
            Next
            Return s.ToString
        End Function
        Private Function XMLExportByte() As Byte()
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XML)
            FDFExport = FDFExport & WriteFields(FDFType.XML)
            FDFExport = FDFExport & WriteEnd(FDFType.XML)
            Return ExportByte(FDFExport)
        End Function
        Private Function XFDFExportStream() As Stream
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.xFDF)
            FDFExport = FDFExport & WriteFields(FDFType.xFDF)
            FDFExport = FDFExport & WriteEnd(FDFType.xFDF)
            Return ExportStream(FDFExport)
        End Function
        Private Function XFDFExportByte() As Byte()
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.xFDF)
            FDFExport = FDFExport & WriteFields(FDFType.xFDF)
            FDFExport = FDFExport & WriteEnd(FDFType.xFDF)
            Return ExportByte(FDFExport)
        End Function
        ''' <summary>
        ''' Determines what format the data is in.
        ''' </summary>
        ''' <param name="PDFData">PDF Data (String) or PDF URL ir PDF Local File Path</param>
        ''' <returns>FDFType (XML/xFDF/XDP/PDF=Acrobat/XPDF=LiveCycle)</returns>
        ''' <remarks></remarks>
        Public Function Determine_Type(ByVal PDFData As String) As FDFType
            'FDFDox.DefaultEncoding = DefaultEncoding
            Dim PDFData2 As String = PDFData
            Dim PDFFileName As String = ""
            Dim bytes() As Byte = Nothing
            Try
                If IsValidUrl(PDFData) Then
                    Dim client As New WebClient
                    'Dim input As New StreamReader(client.OpenRead(PDFData))
                    PDFFileName = PDFData
                    'PDFData = input.ReadToEnd
                    Dim wClient As New Net.WebClient
                    Dim strPDF As New MemoryStream
                    bytes = wClient.DownloadData(PDFData)
                    PDFData = _defaultEncoding.GetString(bytes)
                    'bytes = DefaultEncoding.GetBytes(PDFData)
                ElseIf File.Exists(PDFData) Then
                    PDFFileName = PDFData
                    'PDFFile = Me.OpenFile(PDFData)
                    Dim FS As New FileStream(PDFData, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Dim reader As StreamReader = New StreamReader(FS)
                    ReDim bytes(FS.Length)
                    reader.BaseStream.Read(bytes, 0, reader.BaseStream.Length)
                    'PDFData = reader.ReadToEnd
                    'bytes = Encoding.Unicode.GetBytes(PDFData)
                    FS.Close()
                    PDFData = _defaultEncoding.GetString(bytes)
                End If
            Catch ex As Exception
                PDFData = PDFData2
            End Try
            If PDFData.StartsWith("%FDF") Then
                Return FDFType.FDF
            ElseIf PDFData.StartsWith("%PDF") Then
                'If InStr(PDFData, "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/"">") Then

                Try
                    Dim reader As iTextSharp.text.pdf.PdfReader
                    If Not String_IsNullOrEmpty(PDFFileName) Then
                        reader = New iTextSharp.text.pdf.PdfReader(PDFFileName)
                    Else
                        reader = New iTextSharp.text.pdf.PdfReader(bytes)
                    End If
                    Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
                    Dim isXFA As Boolean = False
                    isXFA = xfaFrm.XfaPresent
                    reader.Close()
                    reader = Nothing
                    xfaFrm = Nothing
                    If isXFA Then
                        Return FDFType.XPDF
                    Else
                        Return FDFType.PDF
                    End If
                Catch ex As Exception
                    Return FDFType.PDF
                End Try
                Return FDFType.PDF
                '     If InStr(PDFData, "xdp:xdp") Then
                ''If InStr(PDFData, "<xdp:xdp") Then
                'Return FDFType.XPDF
                '     Else
                'Return FDFType.PDF
                '     End If
            ElseIf InStr(PDFData, "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""") Then
                Return FDFType.XDP
            ElseIf PDFData.StartsWith("<?xml version=""1.0""") Then
                If InStrRev(PDFData, "<xfdf", -1, CompareMethod.Text) > 0 Then
                    Return FDFType.xFDF
                Else
                    Return FDFType.XML
                End If
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcBadFDF, "Error: Bad FDF/PDF or Unknown Data Type", "FDFApp.FDFType", 1)
            End If
            Return Nothing
            Exit Function
        End Function
        ''' <summary>
        ''' Determines what format the data is in.
        ''' </summary>
        ''' <param name="PDFData">PDF Data</param>
        ''' <returns>FDFType (XML/xFDF/XDP/PDF=Acrobat/XPDF=LiveCycle)</returns>
        ''' <remarks></remarks>
        Public Function Determine_Type(ByVal PDFData As Byte()) As FDFType
            'Dim Data As String = ReadBytes(PDFData, False)
            Dim data As String = DefaultEncoding.GetString(PDFData)
            'Dim data As New StringBuilder(DefaultEncoding.GetString(PDFData))
            If data.ToString.StartsWith("%FDF") Then
                Return FDFType.FDF
            ElseIf data.ToString.StartsWith("%PDF") Then
                Try
                    Dim reader As New iTextSharp.text.pdf.PdfReader(PDFData)
                    Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
                    Dim isXFA As Boolean = False
                    isXFA = xfaFrm.XfaPresent
                    reader.Close()
                    reader = Nothing
                    xfaFrm = Nothing
                    If isXFA Then
                        Return FDFType.XPDF
                    Else
                        Return FDFType.PDF
                    End If
                Catch ex As Exception
                    Return FDFType.PDF
                End Try
                Return FDFType.PDF
            ElseIf InStr(data.ToString, "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""") Then
                Return FDFType.XDP
            ElseIf data.ToString.StartsWith("<?xml version=""1.0""") Then
                If InStrRev(data.ToString, "<xfdf", -1, CompareMethod.Text) > 0 Then
                    Return FDFType.xFDF
                Else
                    Return FDFType.XML
                End If
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcBadFDF, "Error: Bad FDF/PDF or Unknown Data Type", "FDFDoc.FDFType", 1)
                Return FDFType.FDF
            End If
        End Function
        ''' <summary>
        ''' Determines what format the data is in.
        ''' </summary>
        ''' <param name="PDFData">PDF Data</param>
        ''' <param name="ownerPassword">OwnerPassword</param>
        ''' <returns>FDFType (XML/xFDF/XDP/PDF=Acrobat/XPDF=LiveCycle)</returns>
        ''' <remarks></remarks>
        Public Function Determine_Type(ByVal PDFData As Byte(), ByVal ownerPassword As String) As FDFType
            'Dim Data As String = ReadBytes(PDFData, False)
            Dim data As String = DefaultEncoding.GetString(PDFData)
            'Dim data As New StringBuilder(PDFData ))
            If data.ToString.StartsWith("%FDF") Then
                Return FDFType.FDF
            ElseIf data.ToString.StartsWith("%PDF") Then
                Try
                    Dim reader As iTextSharp.text.pdf.PdfReader
                    If Not String.IsNullOrEmpty(ownerPassword) Then
                        reader = New iTextSharp.text.pdf.PdfReader(PDFData, _defaultEncoding.GetBytes(ownerPassword))
                    Else
                        reader = New iTextSharp.text.pdf.PdfReader(PDFData)
                    End If
                    Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
                    Dim isXFA As Boolean = False
                    isXFA = xfaFrm.XfaPresent
                    reader.Close()
                    reader = Nothing
                    xfaFrm = Nothing
                    If isXFA Then
                        Return FDFType.XPDF
                    Else
                        Return FDFType.PDF
                    End If
                Catch ex As Exception
                    Return FDFType.PDF
                End Try
                Return FDFType.PDF
            ElseIf InStr(data.ToString, "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""") Then
                Return FDFType.XDP
            ElseIf data.ToString.StartsWith("<?xml version=""1.0""") Then
                If InStrRev(data.ToString, "<xfdf", -1, CompareMethod.Text) > 0 Then
                    Return FDFType.xFDF
                Else
                    Return FDFType.XML
                End If
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcBadFDF, "Error: Bad FDF/PDF or Unknown Data Type", "FDFApp.FDFType", 1)
                Return FDFType.FDF
            End If
        End Function

        ''' <summary>
        ''' Determines what format the data is in.
        ''' </summary>
        ''' <param name="PDFData">PDF Data</param>
        ''' <returns>FDFType (XML/xFDF/XDP/PDF=Acrobat/XPDF=LiveCycle)</returns>
        ''' <remarks></remarks>
        Public Function Determine_Type(ByVal PDFData As Stream) As FDFType
            Dim Data As String = ReadStream_New(PDFData, False)
            If Data.StartsWith("%FDF") Then
                Return FDFType.FDF
            ElseIf Data.StartsWith("%PDF") Then
                Try
                    Dim reader As New iTextSharp.text.pdf.PdfReader(PDFData)
                    Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
                    Dim isXFA As Boolean = False
                    isXFA = xfaFrm.XfaPresent
                    reader.Close()
                    reader = Nothing
                    xfaFrm = Nothing
                    If isXFA Then
                        Return FDFType.XPDF
                    Else
                        Return FDFType.PDF
                    End If
                Catch ex As Exception
                    Return FDFType.PDF
                End Try
                Return FDFType.PDF
            ElseIf InStr(Data, "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""") Then
                Return FDFType.XDP
            ElseIf Data.StartsWith("<?xml version=""1.0""") Then
                If InStrRev(Data, "<xfdf", -1, CompareMethod.Text) > 0 Then
                    Return FDFType.xFDF
                Else
                    Return FDFType.XML
                End If
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcBadFDF, "Error: Bad FDF/PDF or Unknown Data Type", "FDFDoc.FDFType", 1)
                Return FDFType.FDF
            End If
        End Function
        Private Function ReadStream_New(ByVal InStream As Stream, Optional ByVal omitReturn As Boolean = True) As String
            Dim iCounter As Long = 0, StreamLength As Long = 0
            Dim OutString As String
            Dim Stream16 As New StreamReader(InStream, True)
            OutString = Stream16.ReadToEnd
            Try
                Stream16.Close()
                InStream.Close()
                Stream16 = Nothing
                InStream = Nothing
                'GC.Collect()
                Return OutString
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.ReadStream", 1)
                Return OutString
                Exit Function
            End Try
        End Function
        Private Function ReadBytes(ByVal byteArray() As Byte, Optional ByVal omitReturn As Boolean = True) As String
            Dim iCounter As Long = 0, StreamLength As Long = 0
            Dim OutString As String
            Dim InStream As New MemoryStream(byteArray)
            Dim Stream16 As New StreamReader(InStream, True)
            OutString = Stream16.ReadToEnd
            Try
                Stream16.Close()
                InStream.Close()
                InStream = Nothing
                Stream16 = Nothing
                'GC.Collect()
                Return OutString
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.ReadBytes_New", 1)
                Return OutString
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Saves FDF to File
        ''' </summary>
        ''' <param name="FileName">File path</param>
        ''' <param name="eFDFType">File type</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FDFSave(ByVal FileName As String, Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Boolean
            Dim strFDFData As String = ""
            Try
                If Determine_Type(FDFData) = FDFType.PDF Then
                    strFDFData = PDFExportString()
                ElseIf Determine_Type(FDFData) = FDFType.XPDF Then
                    strFDFData = PDFExportString()
                Else
                    Select Case eFDFType
                        Case FDFType.FDF
                            ' Create FDF Document
                            strFDFData = FDFExportString(AppendSaves)
                        Case FDFType.xFDF
                            ' Create xFDF Document
                            strFDFData = XFDFExportString()
                        Case FDFType.XML
                            ' Create XML Document
                            strFDFData = XMLExportString()
                        Case FDFType.XDP
                            ' Create XML Document
                            strFDFData = XDPExportString()
                    End Select
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSave", 1)
                Exit Function
            End Try
            Dim bAns As Boolean = False
            Dim objReader As StreamWriter
            Try
                If strFDFData <> "" Then
                    Try
                        objReader = New StreamWriter(FileName)
                        objReader.Write(strFDFData)
                        objReader.Close()
                        bAns = True
                        Return True
                    Catch Ex As Exception
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex.Message, "FDFDoc.FDFSave", 1)
                        Return False
                    End Try
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSave", 1)
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Saves FDF Data to file
        ''' </summary>
        ''' <param name="FileName">File name</param>
        ''' <param name="eFDFType">File type</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns>true</returns>
        ''' <remarks></remarks>
        Public Function FDFSavetoFile(ByVal FileName As String, Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Boolean
            Dim strFDFData As String = ""
            Try
                Select Case eFDFType
                    Case FDFType.PDF
                        strFDFData = PDFExportString()
                    Case FDFType.XPDF
                        strFDFData = PDFExportString()
                    Case FDFType.FDF
                        strFDFData = FDFExportString(AppendSaves)
                    Case FDFType.xFDF
                        strFDFData = XFDFExportString()
                    Case FDFType.XML
                        strFDFData = XMLExportString()
                    Case FDFType.XDP
                        strFDFData = XDPExportString()
                    Case Else
                        Return False
                        Exit Function
                End Select
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSaveatoFile", 1)
                Exit Function
            End Try

            Dim bAns As Boolean = False
            Try
                If strFDFData <> "" Then
                    Try
                        Dim myFileStream As New System.IO.FileStream(FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                        Try
                            PDFData = _defaultEncoding.GetBytes(strFDFData)
                            With myFileStream
                                .Write(PDFData, 0, PDFData.Length)
                            End With
                        Catch ex As Exception
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                        Finally
                            If Not myFileStream Is Nothing Then
                                With myFileStream
                                    .Close()
                                    .Dispose()
                                End With
                            End If
                        End Try
                        Return True

                    Catch Ex As Exception
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoFile", 1)
                        Return False
                    End Try
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File Path Error", "FDFDoc.FDFSavetoFile", 1)
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSavetoFile", 1)
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Saves FDF Document to byte array
        ''' </summary>
        ''' <param name="eFDFType">File type</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function FDFSavetoBuf(Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Byte()
            Dim strFDFData As String = ""
            Try
                If Not FDFData Is Nothing Then
                    If Determine_Type(FDFData) = FDFType.PDF Then
                        strFDFData = PDFExportString()
                    ElseIf Determine_Type(FDFData) = FDFType.XPDF Then
                        strFDFData = PDFExportString()
                    Else
                        Select Case eFDFType
                            Case FDFType.FDF
                                ' Create FDF Document
                                strFDFData = FDFExportString(AppendSaves)
                            Case FDFType.xFDF
                                ' Create xFDF Document
                                strFDFData = XFDFExportString()
                            Case FDFType.XML
                                ' Create XML Document
                                strFDFData = XMLExportString()
                            Case FDFType.XDP
                                ' Create XML Document
                                strFDFData = XDPExportString()
                        End Select
                    End If
                Else
                    Select Case eFDFType
                        Case FDFType.FDF
                            ' Create FDF Document
                            strFDFData = FDFExportString(AppendSaves)
                        Case FDFType.xFDF
                            ' Create xFDF Document
                            strFDFData = XFDFExportString()
                        Case FDFType.XML
                            ' Create XML Document
                            strFDFData = XMLExportString()
                        Case FDFType.XDP
                            ' Create XML Document
                            strFDFData = XDPExportString()
                    End Select
                End If
                Return _defaultEncoding.GetBytes(strFDFData)
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoBuf", 1)
                Return Nothing
            End Try

        End Function
#Region "Edited 2010-09-28"
        ''' <summary>
        ''' Saves FDF Data to file
        ''' </summary>
        ''' <param name="FileName">File name</param>
        ''' <param name="eFDFType">File type</param>
        ''' <returns>true</returns>
        ''' <remarks></remarks>
        Public Function toFile(ByVal FileName As String, Optional ByVal eFDFType As FDFType = FDFType.FDF) As Boolean
            Dim strFDFData As String = ""
            Try
                Select Case eFDFType
                    Case FDFType.PDF
                        'strFDFData = PDFExportString()
                        Return PDFMergeFDF2File(FileName, Me.FDFGetFile, False, "")
                        '	PDFMergeFDF2File(FileName, Me.FDFGetFile, False, "")
                    Case FDFType.XPDF
                        strFDFData = PDFExportString()
                    Case FDFType.FDF
                        strFDFData = FDFExportString(True)
                    Case FDFType.xFDF
                        strFDFData = XFDFExportString()
                    Case FDFType.XML
                        strFDFData = XMLExportString()
                    Case FDFType.XDP
                        strFDFData = XDPExportString()
                        'Return PDFMergeXDP2File(FileName, Me.XDPGetFile, False, "")
                    Case Else
                        Return False
                        Exit Function
                End Select
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSaveatoFile", 1)
                Exit Function
            End Try

            Dim bAns As Boolean = False
            Try
                If strFDFData <> "" Then
                    Try
                        Dim myFileStream As New System.IO.FileStream(FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                        Try
                            PDFData = _defaultEncoding.GetBytes(strFDFData)
                            With myFileStream
                                .Write(PDFData, 0, PDFData.Length)
                            End With
                        Catch ex As Exception
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                        Finally
                            If Not myFileStream Is Nothing Then
                                With myFileStream
                                    .Close()
                                    .Dispose()
                                End With
                            End If
                        End Try
                        Return True

                    Catch Ex As Exception
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoFile", 1)
                        Return False
                    End Try
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File Path Error", "FDFDoc.FDFSavetoFile", 1)
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFSavetoFile", 1)
                Exit Function
            End Try
        End Function


        ''' <summary>
        ''' Saves FDF Document to byte array
        ''' </summary>
        ''' <param name="eFDFType">File type</param>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toBuffer(Optional ByVal eFDFType As FDFType = FDFType.FDF) As Byte()
            Dim strFDFData As String = ""
            Try
                If Not FDFData Is Nothing Then
                    If Determine_Type(FDFData) = FDFType.PDF Then
                        strFDFData = PDFExportString()
                    ElseIf Determine_Type(FDFData) = FDFType.XPDF Then
                        strFDFData = PDFExportString()
                    Else
                        Select Case eFDFType
                            Case FDFType.FDF
                                ' Create FDF Document
                                strFDFData = FDFExportString(True)
                            Case FDFType.xFDF
                                ' Create xFDF Document
                                strFDFData = XFDFExportString()
                            Case FDFType.XML
                                ' Create XML Document
                                strFDFData = XMLExportString()
                            Case FDFType.XDP
                                ' Create XML Document
                                strFDFData = XDPExportString()
                            Case FDFType.PDF
                                Return PDFExportByte()
                            Case FDFType.XPDF
                                Return PDFExportByte()
                        End Select
                    End If
                Else
                    Select Case eFDFType
                        Case FDFType.FDF
                            ' Create FDF Document
                            strFDFData = FDFExportString(True)
                        Case FDFType.xFDF
                            ' Create xFDF Document
                            strFDFData = XFDFExportString()
                        Case FDFType.XML
                            ' Create XML Document
                            strFDFData = XMLExportString()
                        Case FDFType.XDP
                            ' Create XML Document
                            strFDFData = XDPExportString()
                        Case FDFType.PDF
                            Return PDFExportByte()
                        Case FDFType.XPDF
                            Return PDFExportByte()
                    End Select
                End If
                'Dim memStream As New MemoryStream(_defaultEncoding.GetBytes(strFDFData))
                Return _defaultEncoding.GetBytes(strFDFData)
                'Return GetUsedBytesOnly(MemStream,true)
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoBuf", 1)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Saves data to stream
        ''' </summary>
        ''' <param name="eFDFType">File Type</param>
        ''' <returns>Stream with Data</returns>
        ''' <remarks></remarks>
        Public Function toStream(Optional ByVal eFDFType As FDFType = FDFType.FDF) As Stream
            Dim strFDFData As String = ""
            Try
                If Not FDFData Is Nothing Then
                    If Determine_Type(FDFData) = FDFType.PDF Then
                        Return New MemoryStream(PDFExportByte)
                        Exit Function
                    ElseIf Determine_Type(FDFData) = FDFType.XPDF Then
                        Return New MemoryStream(PDFExportByte)
                        Exit Function
                    Else
                        Select Case eFDFType
                            Case FDFType.FDF
                                ' Create FDF Document
                                strFDFData = FDFExportString(True)
                            Case FDFType.xFDF
                                ' Create xFDF Document
                                strFDFData = XFDFExportString()
                            Case FDFType.XML
                                ' Create XML Document
                                strFDFData = XMLExportString()
                            Case FDFType.XDP
                                ' Create XML Document
                                strFDFData = XDPExportString()
                            Case FDFType.PDF
                                Return PDFExportStream()
                            Case FDFType.XPDF
                                Return PDFExportStream()
                        End Select
                    End If
                Else
                    Select Case eFDFType
                        Case FDFType.FDF
                            ' Create FDF Document
                            strFDFData = FDFExportString(True)
                        Case FDFType.xFDF
                            ' Create xFDF Document
                            strFDFData = XFDFExportString()
                        Case FDFType.XML
                            ' Create XML Document
                            strFDFData = XMLExportString()
                        Case FDFType.XDP
                            ' Create XML Document
                            strFDFData = XDPExportString()
                        Case FDFType.PDF
                            Return PDFExportStream()
                        Case FDFType.XPDF
                            Return PDFExportStream()
                    End Select
                End If
                Dim pdfStream As New MemoryStream(_defaultEncoding.GetBytes(strFDFData))
                If pdfStream.CanSeek Then
                    pdfStream.Position = 0
                End If
                Return pdfStream
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoBuf", 1)
                Return Nothing
            End Try

        End Function
#End Region

#Region "Added 2010-09-29"
        ''' <summary>
        ''' Returns FDF Document to byte array
        ''' </summary>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toFDF() As Byte()
            Dim strFDFData As String = ""
            Try
                strFDFData = FDFExportString(True)
                'Dim memStream As New MemoryStream(_defaultEncoding.GetBytes(strFDFData))
                Return _defaultEncoding.GetBytes(strFDFData)
                'Return GetUsedBytesOnly(MemStream,true)
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.toFDF", 1)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Returns PDF Document to byte array
        ''' </summary>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toPDF() As Byte()
            Dim strFDFData As String = ""
            Try
                Return PDFExportByte()
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.toPDF", 1)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Returns PDF Document to byte array
        ''' </summary>
        ''' <param name="preserveExtendedRights">Preserves Extended Reader Rights</param>
        ''' <param name="removeExtendedRights">Removes Extended Reader Rights</param>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toPDF(ByVal preserveExtendedRights As Boolean, ByVal removeExtendedRights As Boolean) As Byte()
            Dim strFDFData As String = ""
            Try
                Return PDFExportByte(preserveExtendedRights, removeExtendedRights)
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.toPDF", 1)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Returns XDP Document to byte array
        ''' </summary>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toXDP() As Byte()
            Dim strFDFData As String = ""
            Try

                Return DefaultEncoding.GetBytes(XDPExportString())
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.toXDP", 1)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Returns XFDF Document to byte array
        ''' </summary>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toXFDF() As Byte()
            Dim strFDFData As String = ""
            Try

                Return DefaultEncoding.GetBytes(XFDFExportString())
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.toXFDF", 1)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Returns XML Document to byte array
        ''' </summary>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toXML() As Byte()
            Dim strFDFData As String = ""
            Try

                Return DefaultEncoding.GetBytes(XMLExportString())
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.ToXML", 1)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Returns XFA Document to byte array
        ''' </summary>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toXPDF() As Byte()
            Dim strFDFData As String = ""
            Try

                Return PDFExportByte()
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.toXPDF", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Returns XFA Document to byte array
        ''' </summary>
        ''' <param name="preserveExtendedRights">Preserves Extended Reader Rights</param>
        ''' <param name="removeExtendedRights">Removes Extended Reader Rights</param>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toXPDF(ByVal preserveExtendedRights As Boolean, ByVal removeExtendedRights As Boolean) As Byte()
            Dim strFDFData As String = ""
            Try

                Return PDFExportByte(preserveExtendedRights, removeExtendedRights)
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.toXPDF", 1)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Returns XFA Document to byte array
        ''' </summary>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toXFA() As Byte()
            Dim strFDFData As String = ""
            Try

                Return PDFExportByte()
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.toXFA", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Returns XFA Document to byte array
        ''' </summary>
        ''' <param name="preserveExtendedRights">Preserves Extended Reader Rights</param>
        ''' <param name="removeExtendedRights">Removes Extended Reader Rights</param>
        ''' <returns>Data Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function toXFA(ByVal preserveExtendedRights As Boolean, ByVal removeExtendedRights As Boolean) As Byte()
            Dim strFDFData As String = ""
            Try

                Return PDFExportByte(preserveExtendedRights, removeExtendedRights)
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.toXFA", 1)
                Return Nothing
            End Try
        End Function

#End Region

        ''' <summary>
        ''' Saves data to stream
        ''' </summary>
        ''' <param name="eFDFType">File Type</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns>Stream with Data</returns>
        ''' <remarks></remarks>
        Public Function FDFSavetoStream(Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Stream
            Dim strFDFData As String = ""
            Try
                If Not FDFData Is Nothing Then
                    If Determine_Type(FDFData) = FDFType.PDF Then
                        Return New MemoryStream(PDFExportByte)
                        Exit Function
                    ElseIf Determine_Type(FDFData) = FDFType.XPDF Then
                        Return New MemoryStream(PDFExportByte)
                        Exit Function
                    Else
                        Select Case eFDFType
                            Case FDFType.FDF
                                ' Create FDF Document
                                strFDFData = FDFExportString(AppendSaves)
                            Case FDFType.xFDF
                                ' Create xFDF Document
                                strFDFData = XFDFExportString()
                            Case FDFType.XML
                                ' Create XML Document
                                strFDFData = XMLExportString()
                            Case FDFType.XDP
                                ' Create XML Document
                                strFDFData = XDPExportString()
                        End Select
                    End If
                Else
                    Select Case eFDFType
                        Case FDFType.FDF
                            ' Create FDF Document
                            strFDFData = FDFExportString(AppendSaves)
                        Case FDFType.xFDF
                            ' Create xFDF Document
                            strFDFData = XFDFExportString()
                        Case FDFType.XML
                            ' Create XML Document
                            strFDFData = XMLExportString()
                        Case FDFType.XDP
                            ' Create XML Document
                            strFDFData = XDPExportString()
                    End Select
                End If
                Return New MemoryStream(_defaultEncoding.GetBytes(strFDFData))
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoBuf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Saves PDF to byte array
        ''' </summary>
        ''' <param name="eFDFType">File Type</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns>PDF Document in byte array</returns>
        ''' <remarks></remarks>
        Public Function PDFSavetoBuf(Optional ByVal eFDFType As FDFType = FDFType.PDF, Optional ByVal AppendSaves As Boolean = True) As Byte()
            Try
                If Determine_Type(FDFData) = FDFType.PDF Then
                    Return PDFExportByte()
                    Exit Function
                ElseIf Determine_Type(FDFData) = FDFType.XPDF Then
                    Return PDFExportByte()
                    Exit Function
                End If
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoBuf", 1)
                Return Nothing
            End Try
            Return Nothing
        End Function
        ''' <summary>
        ''' Saves PDF to stream
        ''' </summary>
        ''' <param name="eFDFType">File Type</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns>PDF Document in stream</returns>
        ''' <remarks></remarks>
        Public Function PDFSavetoStream(Optional ByVal eFDFType As FDFType = FDFType.PDF, Optional ByVal AppendSaves As Boolean = True) As Stream
            Try
                If Determine_Type(FDFData) = FDFType.PDF Then
                    Return New MemoryStream(PDFExportByte())
                    Exit Function
                ElseIf Determine_Type(FDFData) = FDFType.XPDF Then
                    Return New MemoryStream(PDFExportByte())
                    Exit Function
                End If
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoBuf", 1)
                Return Nothing
            End Try
            Return Nothing
        End Function
        ''' <summary>
        ''' Saves data to string
        ''' </summary>
        ''' <param name="eFDFType">File Type</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns>Data in string format</returns>
        ''' <remarks></remarks>
        Public Function FDFSavetoStr(Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As String
            Dim strFDFData As String = ""
            Try
                Select Case eFDFType
                    Case FDFType.FDF
                        ' Create FDF Document
                        strFDFData = FDFExportString(AppendSaves)
                    Case FDFType.xFDF
                        ' Create xFDF Document
                        strFDFData = XFDFExportString()
                    Case FDFType.XML
                        ' Create XML Document
                        strFDFData = XMLExportString()
                    Case FDFType.XDP
                        ' Create XML Document
                        strFDFData = XDPExportString()
                End Select
                Return ByteArrayToStr(_defaultEncoding.GetBytes(strFDFData)) & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoStr", 1)
                Return ""
            End Try

        End Function
        ''' <summary>
        ''' Saves data to character array
        ''' </summary>
        ''' <param name="eFDFType">File Type</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns>Data in character array format</returns>
        ''' <remarks></remarks>
        Public Function FDFSavetoArray(Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Char()
            Dim strFDFData As String = ""
            Try
                If Not FDFData Is Nothing Then
                    If Determine_Type(FDFData) = FDFType.PDF Then
                        strFDFData = PDFExportBuffer()
                    ElseIf Determine_Type(FDFData) = FDFType.XPDF Then
                        strFDFData = PDFExportBuffer()
                    Else
                        Select Case eFDFType
                            Case FDFType.FDF
                                ' Create FDF Document
                                strFDFData = FDFExportBuffer(AppendSaves)
                            Case FDFType.xFDF
                                ' Create xFDF Document
                                strFDFData = XFDFExportBuffer()

                            Case FDFType.XML
                                ' Create XML Document
                                strFDFData = XMLExportBuffer()
                            Case FDFType.XDP
                                ' Create XML Document
                                strFDFData = XDPExportBuffer()
                        End Select
                    End If
                Else
                    Select Case eFDFType
                        Case FDFType.FDF
                            ' Create FDF Document
                            strFDFData = FDFExportBuffer(AppendSaves)
                        Case FDFType.xFDF
                            ' Create xFDF Document
                            strFDFData = XFDFExportBuffer()

                        Case FDFType.XML
                            ' Create XML Document
                            strFDFData = XMLExportBuffer()
                        Case FDFType.XDP
                            ' Create XML Document
                            strFDFData = XDPExportBuffer()
                    End Select
                End If
                Return strFDFData & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.FDFSavetoBuf", 1)
                Return ""
            End Try
        End Function
        Private Function ExportStream(ByVal str As String) As Stream
            Dim xStream As New MemoryStream
            Dim b() As Byte
            b = _defaultEncoding.GetBytes(str)
            xStream.Read(b, 0, CInt(b.Length))
            Return xStream
        End Function
        Private Function StrToByteArray(ByVal str As String) As Byte()
            Return _defaultEncoding.GetBytes(str)
        End Function 'StrToByteArray
        Private Function ByteArrayToStr(ByVal dBytes() As Byte) As String
            Dim str As String
            str = _defaultEncoding.GetString(dBytes)
            Return str
        End Function 'StrToByteArray
        Private Function ExportDataset(ByRef OutResponse As System.Web.HttpResponse, ByVal dsImport As DataSet, ByVal enFormat As ExportFormat, ByVal strColDelim As String, ByVal strRowDelim As String, ByVal strFileName As String) As Boolean
            Dim dgExport As New System.Web.UI.WebControls.DataGrid
            dgExport.AllowPaging = False
            dgExport.DataSource = dsImport
            dsImport.DataSetName = "ExportDataset"
            dgExport.DataMember = dsImport.Tables(0).TableName
            dgExport.DataBind()
            OutResponse.Clear()
            OutResponse.Buffer = True
            OutResponse.ContentEncoding = Encoding.UTF8
            OutResponse.Charset = ""
            OutResponse.AddHeader("Content-Disposition", "attachment; filename=" & strFileName)

            Select Case enFormat
                Case ExportFormat.XLS
                    OutResponse.ContentType = "application/vnd.ms-excel"
                    Dim oStringWriter As System.IO.StringWriter = New System.IO.StringWriter()
                    Dim oHtmlTextWriter As System.Web.UI.HtmlTextWriter = New System.Web.UI.HtmlTextWriter(oStringWriter)
                    dgExport.RenderControl(oHtmlTextWriter)
                    OutResponse.Write(oStringWriter.ToString())
                Case ExportFormat.CUSTOM
                    Dim strText As String
                    OutResponse.ContentType = "text/txt"
                    Dim oStringWriter As System.IO.StringWriter = New System.IO.StringWriter()
                    Dim oHtmlTextWriter As System.Web.UI.HtmlTextWriter = New System.Web.UI.HtmlTextWriter(oStringWriter)
                    dgExport.RenderControl(oHtmlTextWriter)
                    strText = oStringWriter.ToString()
                    strText = ParseToDelim(strText, strRowDelim, strColDelim)
                    OutResponse.Write(strText)
                Case ExportFormat.CSV
                    Dim strText As String
                    strRowDelim = System.Environment.NewLine
                    strColDelim = ","
                    OutResponse.ContentType = "text/txt"
                    Dim oStringWriter As System.IO.StringWriter = New System.IO.StringWriter()
                    Dim oHtmlTextWriter As System.Web.UI.HtmlTextWriter = New System.Web.UI.HtmlTextWriter(oStringWriter)
                    dgExport.RenderControl(oHtmlTextWriter)
                    strText = oStringWriter.ToString()
                    strText = ParseToDelim(strText, strRowDelim, strColDelim)
                    OutResponse.Write(strText)
                Case ExportFormat.TSV
                    Dim strText As String
                    strRowDelim = System.Environment.NewLine
                    strColDelim = "\t"
                    OutResponse.ContentType = "text/txt"
                    Dim oStringWriter As System.IO.StringWriter = New System.IO.StringWriter()
                    Dim oHtmlTextWriter As System.Web.UI.HtmlTextWriter = New System.Web.UI.HtmlTextWriter(oStringWriter)
                    dgExport.RenderControl(oHtmlTextWriter)
                    strText = oStringWriter.ToString()
                    strText = ParseToDelim(strText, strRowDelim, strColDelim)
                    OutResponse.Write(strText)
                Case ExportFormat.XML
                    OutResponse.ContentType = "text/xml"
                    OutResponse.Write(dsImport.GetXml())
                Case ExportFormat.HTML
                    OutResponse.ContentType = "text/html"
                    Dim oStringWriter As System.IO.StringWriter = New System.IO.StringWriter()
                    Dim oHtmlTextWriter As System.Web.UI.HtmlTextWriter = New System.Web.UI.HtmlTextWriter(oStringWriter)
                    dgExport.RenderControl(oHtmlTextWriter)
                    OutResponse.Write(oStringWriter.ToString())
            End Select



        End Function
        Private Function ParseToDelim(ByVal strText As String, ByVal strRowDelim As String, ByVal strColDelim As String) As String

            Dim objReg As System.Text.RegularExpressions.Regex = New System.Text.RegularExpressions.Regex("(>\s+<)", RegularExpressions.RegexOptions.IgnoreCase)
            strText = objReg.Replace(strText, "><")
            strText = strText.Replace(System.Environment.NewLine, "")
            strText = strText.Replace("</td></tr><tr><td>", strRowDelim)
            strText = strText.Replace("</td><td>", strColDelim)
            objReg = New System.Text.RegularExpressions.Regex("<[^>]*>", RegularExpressions.RegexOptions.IgnoreCase)
            strText = objReg.Replace(strText, "")
            strText = System.Web.HttpUtility.HtmlDecode(strText)
            Return strText
        End Function
        Private Enum ExportFormat
            XML
            XLS
            HTML
            CSV
            CUSTOM
            TSV
        End Enum
        Private Function ExportByte(ByVal str As String) As Byte()
            Dim buffer() As Byte
            Dim encoder As New System.Text.UTF8Encoding
            ReDim buffer(str.Length - 1)
            _defaultEncoding.GetBytes(str, 0, str.Length, buffer, 0)
            Return buffer
        End Function
        Public Function ConvertString_ISO88591_UTF8(ByVal str As String) As String
            Dim fromEnc() As Byte = System.Text.Encoding.GetEncoding(1252).GetBytes(str)
            Dim toEnc() As Byte = System.Text.Encoding.Convert(System.Text.Encoding.GetEncoding(1252), System.Text.Encoding.UTF8, fromEnc)
            Return System.Text.Encoding.UTF8.GetString(toEnc)
        End Function
        Public Function ConvertString_UTF8_ISO88591(ByVal str As String) As String
            Dim fromEnc() As Byte = System.Text.Encoding.UTF8.GetBytes(str)
            Dim toEnc() As Byte = System.Text.Encoding.Convert(System.Text.Encoding.UTF8, System.Text.Encoding.GetEncoding(1252), fromEnc)
            Return System.Text.Encoding.UTF8.GetString(toEnc)
        End Function
        Private Function ExportBuffer(ByVal str As String) As Char()
            ' e.g. "1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16" to 
            '{1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16}
            Dim s As Char()
            s = str.ToCharArray
            Return s
        End Function


        'XPD FORM FUNCTIONS
        ''' <summary>
        ''' Document type
        ''' </summary>
        ''' <value>FDFDocType</value>
        ''' <returns>Document type of current FDFDoc_Class object</returns>
        ''' <remarks></remarks>
        Public Property DocType() As FDFDocType
            Get
                Return _FDF(_CurFDFDoc).DocType
            End Get
            Set(ByVal value As FDFDocType)
                Dim _FDFX As FDFDoc_Class = _FDF(_CurFDFDoc)
                _FDFX.DocType = value
                _FDF(_CurFDFDoc) = _FDFX
            End Set
        End Property
        ''' <summary>
        ''' Gets XDP Forms
        ''' </summary>
        ''' <returns>XDP Form array</returns>
        ''' <remarks></remarks>
        Public Function GetXDPForms() As FDFDoc_Class()
            Return _FDF.ToArray
        End Function
        ''' <summary>
        ''' Adds XDP form
        ''' </summary>
        ''' <param name="bstrFormName">Form name</param>
        ''' <param name="bstrFileName">Live-Cycle form path</param>
        ''' <remarks></remarks>
        Public Sub XDPAddForm(ByVal bstrFormName As String, ByVal bstrFileName As String)
            Dim ParentFormName As String = ""
            Dim _f As New FDFDoc_Class
            If _FDF.Count > 0 Then
                If Not _FDF(_FDF.Count - 1).DocType = Nothing Then
                    If _FDF(_FDF.Count - 1).DocType = FDFDocType.XDPForm Then
                        Try
                            _f.FileName = bstrFileName
                            _CurFDFDoc = _FDF.Count - 1
                            _PDF.FileName = bstrFileName
                            Try
                                ParentFormName = _FDF(_CurFDFDoc).FormLevel.TrimEnd("/") & "/" & bstrFormName
                            Catch ex As Exception
                                ParentFormName = bstrFormName
                            End Try
                            _f.FormLevel = ParentFormName
                            _f.FormName = bstrFormName
                            _f.DocType = FDFDocType.XDPForm
                        Catch ex As Exception

                        End Try
                        _FDF.Add(_f)
                        _CurFDFDoc = _FDF.Count - 1
                        Exit Sub
                    End If
                End If
            End If
            _CurFDFDoc = 0
            _f.FileName = bstrFileName
            _PDF.FileName = bstrFileName
            _f.FormName = bstrFormName
            _f.DocType = FDFDocType.XDPForm
            Try
                ParentFormName = _FDF(_CurFDFDoc).FormLevel.TrimEnd("/") & "/" & bstrFormName
            Catch ex As Exception
                ParentFormName = bstrFormName
            End Try
            _FDF.Add(_f)
            _CurFDFDoc = _FDF.Count - 1
            Exit Sub
            'Return _FDF(_CurFDFDoc)
        End Sub
        ''' <summary>
        ''' Adds XDP form
        ''' </summary>
        ''' <param name="ParentFormNames">Parent Form Names</param>
        ''' <param name="SubFormName">SubForm Name</param>
        ''' <param name="PDFFilePath">Live-Cycle form path</param>
        ''' <remarks></remarks>
        Public Function XDPAddSubForm(ByVal ParentFormNames() As String, ByVal SubFormName As String, ByVal PDFFilePath As String) As Integer
            Dim ParentFormName As String = String.Join("/", ParentFormNames) & "/" & SubFormName
            Dim _f As New FDFDoc_Class
            If Not String_IsNullOrEmpty(SubFormName) Then
                Try
                    _f.FileName = PDFFilePath
                    _PDF.FileName = PDFFilePath
                    _f.FormName = SubFormName
                    _f.DocType = FDFDocType.XDPForm
                    _f.FormLevel = ParentFormName.ToString
                    _f.FormLevel = _f.FormLevel.TrimStart("/")
                    _f.FormLevel = _f.FormLevel.TrimEnd("/")
                    _f.FormLevel = _f.FormLevel.Replace("//", "/")
                Catch ex As Exception

                End Try
                _FDF.Add(_f)
                _CurFDFDoc = _FDF.Count - 1
                Return _CurFDFDoc
            End If
            Return 0
        End Function
        ''' <summary>
        ''' Adds XDP form
        ''' </summary>
        ''' <param name="SubFormName">SubForm Name</param>
        ''' <param name="PDFFilePath">Live-Cycle form path</param>
        ''' <remarks></remarks>
        Public Function XDPAddSubForm(ByVal SubFormName As String, ByVal PDFFilePath As String) As Integer
            Dim _f As New FDFDoc_Class
            If _FDF.Count >= 1 Then
                If Not _FDF(_CurFDFDoc).DocType = Nothing Then
                    If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then
                        If Not String_IsNullOrEmpty(SubFormName) Then
                            Dim ParentFormName As String = ""
                            Try
                                If _FDF.Count <= 0 Then
                                    _CurFDFDoc = _FDF.Count - 1
                                    ParentFormName = SubFormName
                                Else
                                    _CurFDFDoc = _FDF.Count - 1
                                    Try
                                        ParentFormName = _FDF(IIf(_CurFDFDoc > 0, _CurFDFDoc - 1, _CurFDFDoc)).FormLevel.TrimEnd("/") & "/" & SubFormName
                                    Catch ex As Exception
                                        ParentFormName = SubFormName
                                    End Try

                                End If
                                _f.FileName = PDFFilePath
                                _PDF.FileName = PDFFilePath
                                _f.FormName = SubFormName
                                _f.DocType = FDFDocType.XDPForm
                                _f.FormLevel = ParentFormName.ToString
                                _f.FormLevel = _f.FormLevel.TrimStart("/")
                                _f.FormLevel = _f.FormLevel.TrimEnd("/")
                                _f.FormLevel = _f.FormLevel.Replace("//", "/")
                            Catch ex As Exception

                            End Try
                            _FDF.Add(_f)
                            _CurFDFDoc = _FDF.Count - 1
                            Return _CurFDFDoc
                        End If
                    End If
                End If
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' Adds XDP form
        ''' </summary>
        ''' <param name="SubFormNames">SubForm Names</param>
        ''' <param name="PDFFilePath">Live-Cycle form path</param>
        ''' <remarks></remarks>
        Public Function XDPAddSubForm(ByVal SubFormNames As String(), ByVal PDFFilePath As String) As Integer
            Dim _f As New FDFDoc_Class
            Dim subFormLevel As String = String.Join("/", SubFormNames)
            If Not String_IsNullOrEmpty(SubFormNames(SubFormNames.Length - 1)) Then
                Try
                    If _FDF.Count <= 0 Then
                        _CurFDFDoc = _FDF.Count - 1
                    Else
                        _CurFDFDoc = _FDF.Count - 1
                    End If
                    _f.FileName = PDFFilePath
                    _PDF.FileName = PDFFilePath
                    _f.FormName = SubFormNames(SubFormNames.Length - 1)
                    _f.DocType = FDFDocType.XDPForm
                    _f.FormLevel = String.Join("/", SubFormNames)
                    _f.FormLevel = _f.FormLevel.TrimStart("/")
                    _f.FormLevel = _f.FormLevel.TrimEnd("/")
                    _f.FormLevel = _f.FormLevel.Replace("//", "/")
                Catch ex As Exception

                End Try
                _FDF.Add(_f)
                _CurFDFDoc = _FDF.Count - 1
                Return _CurFDFDoc
            End If
            Return 0
        End Function
        Protected Friend Function GetSubformByName(ByVal SubFormName As String, ByVal FDFDoc As FDFDoc_Class) As FDFApp.FDFDoc_Class.FDFDoc_Class
            If _FDF(0).XDPSubForms Is Nothing Then
                Return Nothing
            End If
            For Each FDFDoc1 As FDFDoc_Class In _FDF
                If FDFDoc1.FormName = SubFormName Then
                    Return FDFDoc1
                End If
            Next
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets XDP form
        ''' </summary>
        ''' <param name="intFormNumber">Form number</param>
        ''' <value></value>
        ''' <returns>XDP Form</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property XDPForm(ByVal intFormNumber As Integer) As FDFDoc_Class
            Get
                ' ADD NEW DOC TO _FDF() ARRAY
                Dim TmpCurFDFDoc As Integer = 0
                If _FDF.Count > 0 Then
                    For XDPFDF As Integer = 0 To _FDF.Count - 1
                        If intFormNumber = XDPFDF Then
                            _CurFDFDoc = XDPFDF
                        End If
                        Return _FDF(XDPFDF)
                    Next
                Else
                    Return Nothing
                End If
                Return _FDF(0)
            End Get
        End Property

        ''' <summary>
        ''' Gets XDP Form
        ''' </summary>
        ''' <param name="FormName">Form name</param>
        ''' <value>XDP Form</value>
        ''' <returns>XDP Form</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property XDPForm(ByVal FormName As String) As FDFDoc_Class
            Get
                Dim TmpCurFDFDoc As Integer = 0
                If _FDF.Count > 0 Then
                    For XDPFDF As Integer = 0 To _FDF.Count - 1
                        If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                            If FormName.ToLower = _FDF(XDPFDF).FormName.ToLower Then
                                _CurFDFDoc = XDPFDF
                                Return _FDF(XDPFDF)
                            End If
                        End If
                    Next
                Else
                    Return Nothing
                End If
                Return _FDF(0)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets current XDP Form name
        ''' </summary>
        ''' <value>XDP Form name</value>
        ''' <returns>XDP Form name</returns>
        ''' <remarks></remarks>
        Public Property XDPFormName() As String
            Get
                If _FDF.Count > 0 Then
                    If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName) Then
                        Return _FDF(_CurFDFDoc).FormName & ""
                    End If
                End If
                Return ""
            End Get
            Set(ByVal value As String)
                Dim TmpCurFDFDoc As Integer = 0
                If _FDF.Count > 0 Then
                    If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName) Then
                        Dim _f As FDFDoc_Class = _FDF(_CurFDFDoc)
                        _f.FormName = value
                        _FDF(_CurFDFDoc) = _f
                    End If
                Else
                    Dim _f As FDFDoc_Class = _FDF(_CurFDFDoc)
                    _f.FormName = value
                    _FDF(_CurFDFDoc) = _f
                End If
            End Set
        End Property
        Public Property XDPFileName() As String
            Get
                If _FDF.Count > 0 Then
                    If Not _FDF(_CurFDFDoc).FileName Is Nothing Then
                        Return _FDF(_CurFDFDoc).FileName & ""
                    End If
                End If
                Return ""
            End Get
            Set(ByVal value As String)
                Dim TmpCurFDFDoc As Integer = 0
                If _FDF.Count > 0 Then
                    If Not _FDF(_CurFDFDoc).FileName Is Nothing Then
                        Dim _f As FDFDoc_Class = _FDF(_CurFDFDoc)
                        _f.FileName = value
                        _FDF(_CurFDFDoc) = _f
                    End If
                Else
                    Dim _f As FDFDoc_Class = _FDF(_CurFDFDoc)
                    _f.FileName = value
                    _FDF(_CurFDFDoc) = _f
                End If
            End Set
        End Property
        ''' <summary>
        ''' Return XDP Form Number
        ''' </summary>
        ''' <param name="FormName">XDP Form name</param>
        ''' <value></value>
        ''' <returns>XDP Form Number</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property XDPFormNumber(ByVal FormName As String) As Integer
            Get
                Dim TmpCurFDFDoc As Integer = 0
                If _FDF.Count > 0 Then
                    For Each XDPFDF As FDFDoc_Class In _FDF
                        TmpCurFDFDoc += 1
                        If XDPFDF.FileName.ToLower = FormName.ToLower Then
                            _CurFDFDoc = TmpCurFDFDoc
                            Return _CurFDFDoc
                        End If
                    Next
                End If
            End Get
        End Property
        ''' <summary>
        ''' Return XDP Form Number
        ''' </summary>
        ''' <param name="FormNames">XDP Form names</param>
        ''' <value></value>
        ''' <returns>XDP Form Number</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property XDPFormNumber(ByVal FormNames As String()) As Integer
            Get
                Dim TmpCurFDFDoc As Integer = 0
                If _FDF.Count > 0 Then
                    For Each XDPFDF As FDFDoc_Class In _FDF
                        Dim str As String = String.Join("/", FormNames)
                        str = str.TrimStart("/")
                        str = str.TrimEnd("/")
                        str = str.Replace("//", "/")
                        Try
                            If Not String_IsNullOrEmpty(XDPFDF.FormLevel & "") Then
                                If XDPFDF.FormLevel.ToLower = str.ToLower Then
                                    _CurFDFDoc = TmpCurFDFDoc
                                    Return _CurFDFDoc
                                End If
                            End If
                        Catch ex As Exception

                        End Try
                        TmpCurFDFDoc += 1
                    Next
                End If
            End Get
        End Property
        ''' <summary>
        ''' Adds XDP Form field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldValue">Field value</param>
        ''' <param name="FormName">Form name</param>
        ''' <param name="FieldType">Field type</param>
        ''' <param name="FieldEnabled">True</param>
        ''' <param name="ReplaceField">If True, replaces field</param>
        ''' <remarks></remarks>
        Public Sub XDPAddField(ByVal FieldName As String, ByVal FieldValue As String, ByVal FormName As String, Optional ByVal FieldType As FieldType = FieldType.FldTextual, Optional ByVal FieldEnabled As Boolean = True, Optional ByVal ReplaceField As Boolean = True)
            Dim fldNumber As Integer = 0
            Try
                Dim TmpCurFDFDoc As Integer = 0
                Dim XDPFDF As Integer = 0
                If _FDF.Count > 0 Then
                    For XDPFDF = 0 To _FDF.Count - 1
                        If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                            If FormName.ToLower = _FDF(XDPFDF).FormName.ToLower Then
                                _CurFDFDoc = XDPFDF
                                Exit For
                            End If
                        End If
                    Next
                    If XDPFDF = _FDF.Count Then
                        XDPFDF = _FDF.Count - 1
                    End If
                Else
                    Exit Sub
                End If

                Dim fldName As String = FieldName
                Try
                    If ReplaceField = False Then
                        If FieldName.LastIndexOf("[") > 0 Then
                            Dim int As Integer = FieldName.LastIndexOf("[") + 1
                            fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                        Else
                            For Each fld As FDFField In _FDF(XDPFDF).struc_FDFFields
                                If Not fld.FieldName Is Nothing Then
                                    If fld.FieldName = FieldName Then
                                        fldNumber += 1
                                    End If
                                End If
                            Next
                        End If
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                Try

                    If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                        If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                            Dim blnFound As Boolean = False
                            If ReplaceField = True Then
                                If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                                    If _FDF(XDPFDF).struc_FDFFields.Count > 0 Then
                                        For Each xField In _FDF(XDPFDF).struc_FDFFields
                                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                                If FieldName.ToLower = xField.FieldName.ToLower Then
                                                    xField.FieldValue.Clear()
                                                    xField.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                                                    xField.FieldEnabled = FieldEnabled
                                                    xField.FieldNum = fldNumber
                                                    xField.FieldType = FieldType
                                                    blnFound = True
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                            If blnFound = True Then
                                Exit Sub
                            Else
                                Dim _fld As New FDFField
                                _fld.FieldName = FieldName
                                _fld.FieldNum = fldNumber
                                _fld.FieldType = FieldType
                                Dim FldVals() As String, FldVal As String = "", MultiVal As String = ""
                                Select Case FieldType
                                    Case FieldType.FldMultiSelect
                                        If FieldValue.Split("|").Length > 0 Then
                                            FldVals = FieldValue.Split("|")
                                            For Each FldVal In FldVals
                                                MultiVal &= "(" & FldVal & ")"
                                            Next
                                            'ReDim _FDF(XDPFDF).struc_FDFFields(_FDF(XDPFDF).struc_FDFFields.Count - 1).FieldValue(0)
                                            _fld.FieldValue.Clear()
                                            _fld.FieldValue.Add(MultiVal)
                                            _fld.FieldEnabled = FieldEnabled
                                        Else
                                            _fld.FieldValue.Clear()
                                            _fld.FieldValue.Add(FieldValue)
                                            _fld.FieldEnabled = FieldEnabled
                                        End If
                                    Case FieldType.FldOption
                                        _fld.FieldValue.Clear()
                                        _fld.FieldValue.Add(FieldValue)
                                        _fld.FieldEnabled = FieldEnabled
                                    Case FieldType.FldTextual
                                        _fld.FieldValue.Clear()
                                        _fld.FieldValue.Add(FieldValue)
                                        _fld.FieldEnabled = FieldEnabled
                                End Select
                                _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                            End If
                        ElseIf Not FieldName = "" Then
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldValue.Clear()
                            _fld.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                            _fld.FieldType = FieldType
                            _fld.FieldEnabled = FieldEnabled
                            _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                        End If
                    ElseIf Not FieldName = "" Then
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldNum = fldNumber
                        _fld.FieldValue.Clear()
                        _fld.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                        _fld.FieldType = FieldType
                        _fld.FieldEnabled = FieldEnabled
                        _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                    End If
                Catch ex As Exception
                    Dim _fld As New FDFField
                    _fld.FieldName = FieldName
                    _fld.FieldNum = fldNumber
                    _fld.FieldValue.Clear()
                    _fld.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                    _fld.FieldType = FieldType
                    _fld.FieldEnabled = FieldEnabled
                    _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                End Try
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Adds XDP Form field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldValue">Field value</param>
        ''' <param name="FormName">Form name</param>
        ''' <param name="FieldType">Field type</param>
        ''' <param name="FieldEnabled">True</param>
        ''' <param name="ReplaceField">If True, replaces field</param>
        ''' <remarks></remarks>
        Public Sub XDPAddField(ByVal FieldName As String, ByVal FieldValue As String, ByVal FormName As String, ByVal ParentFormNames As String(), Optional ByVal FieldType As FieldType = FieldType.FldTextual, Optional ByVal FieldEnabled As Boolean = True, Optional ByVal ReplaceField As Boolean = True)
            Dim fldNumber As Integer = 0
            Try
                Dim TmpCurFDFDoc As Integer = 0
                Dim XDPFDF As Integer = 0
                If _FDF.Count > 0 Then
                    For XDPFDF = 0 To _FDF.Count - 1
                        If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                            If ParentFormNames.Length > 0 Then
                                If String_IsNullOrEmpty(ParentFormNames(0) & "") Then
                                    If FormName.ToLower = _FDF(XDPFDF).FormName.ToLower Then
                                        _CurFDFDoc = XDPFDF
                                        Exit For
                                    End If
                                Else
                                    If FormName.ToLower = _FDF(XDPFDF).FormName.ToLower Then
                                        If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormLevel) Then
                                            Dim strsubforms As String = String.Join("/", ParentFormNames).ToLower
                                            strsubforms = strsubforms.Replace("//", "/")
                                            strsubforms = strsubforms.TrimStart("/")
                                            strsubforms = strsubforms.TrimEnd("/")
                                            If _FDF(XDPFDF).FormLevel.ToLower = strsubforms.ToLower & "/" & FormName.ToLower Then
                                                _CurFDFDoc = XDPFDF
                                                Exit For
                                            End If
                                        End If
                                    End If
                                End If
                            Else
                                If FormName.ToLower = _FDF(XDPFDF).FormName.ToLower Then
                                    _CurFDFDoc = XDPFDF
                                    Exit For
                                End If
                            End If
                        End If
                    Next
                    If XDPFDF = _FDF.Count Then
                        XDPFDF = _FDF.Count - 1
                    End If
                Else
                    Exit Sub
                End If

                Dim fldName As String = FieldName
                Try
                    If ReplaceField = False Then
                        If FieldName.LastIndexOf("[") > 0 Then
                            Dim int As Integer = FieldName.LastIndexOf("[") + 1
                            fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                        Else
                            For Each fld As FDFField In _FDF(XDPFDF).struc_FDFFields
                                If Not fld.FieldName Is Nothing Then
                                    If fld.FieldName = FieldName Then
                                        fldNumber += 1
                                    End If
                                End If
                            Next
                        End If
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                Try

                    If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                        If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                            Dim blnFound As Boolean = False
                            If ReplaceField = True Then
                                If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                                    If _FDF(XDPFDF).struc_FDFFields.Count > 0 Then
                                        For Each xField In _FDF(XDPFDF).struc_FDFFields
                                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                                If FieldName.ToLower = xField.FieldName.ToLower Then
                                                    xField.FieldValue.Clear()
                                                    xField.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                                                    xField.FieldEnabled = FieldEnabled
                                                    xField.FieldNum = fldNumber
                                                    xField.FieldType = FieldType
                                                    blnFound = True
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                            If blnFound = True Then
                                Exit Sub
                            Else
                                Dim _fld As New FDFField
                                _fld.FieldName = FieldName
                                _fld.FieldNum = fldNumber
                                _fld.FieldType = FieldType
                                Dim FldVals() As String, FldVal As String = "", MultiVal As String = ""
                                Select Case FieldType
                                    Case FieldType.FldMultiSelect
                                        If FieldValue.Split("|").Length > 0 Then
                                            FldVals = FieldValue.Split("|")
                                            For Each FldVal In FldVals
                                                MultiVal &= "(" & FldVal & ")"
                                            Next
                                            'ReDim _FDF(XDPFDF).struc_FDFFields(_FDF(XDPFDF).struc_FDFFields.Count - 1).FieldValue(0)
                                            _fld.FieldValue.Add(MultiVal)
                                            _fld.FieldEnabled = FieldEnabled
                                        Else
                                            _fld.FieldValue.Add(FieldValue)
                                            _fld.FieldEnabled = FieldEnabled
                                        End If
                                    Case FieldType.FldOption
                                        _fld.FieldValue.Add(FieldValue)
                                        _fld.FieldEnabled = FieldEnabled
                                    Case FieldType.FldTextual
                                        _fld.FieldValue.Add(FieldValue)
                                        _fld.FieldEnabled = FieldEnabled
                                End Select
                                _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                            End If
                        ElseIf Not FieldName = "" Then
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType
                            _fld.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                            _fld.FieldType = FieldType
                            _fld.FieldEnabled = FieldEnabled
                            _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                        End If
                    ElseIf Not FieldName = "" Then
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldNum = fldNumber
                        _fld.FieldType = FieldType
                        _fld.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                        _fld.FieldType = FieldType
                        _fld.FieldEnabled = FieldEnabled
                        _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                    End If
                Catch ex As Exception
                    Dim _fld As New FDFField
                    _fld.FieldName = FieldName
                    _fld.FieldNum = fldNumber
                    _fld.FieldType = FieldType
                    _fld.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                    _fld.FieldType = FieldType
                    _fld.FieldEnabled = FieldEnabled
                    _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                End Try
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Adds XDP Form field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldValue">Field value</param>
        ''' <param name="FormNumber">Form number</param>
        ''' <param name="FieldType">Field type</param>
        ''' <param name="FieldEnabled">True</param>
        ''' <param name="ReplaceField">If True, replaces field</param>
        ''' <remarks></remarks>
        Public Sub XDPAddField(ByVal FieldName As String, ByVal FieldValue As String, ByVal FormNumber As Integer, Optional ByVal FieldType As FieldType = FieldType.FldTextual, Optional ByVal FieldEnabled As Boolean = True, Optional ByVal ReplaceField As Boolean = True)
            Dim fldNumber As Integer = 0
            Dim FormNum As Integer = 0
            Try
                Dim TmpCurFDFDoc As Integer = 0
                If _FDF.Count > 0 Then
                    If Not _FDF(FormNumber).FormName Is Nothing Then
                        FormNumber = FormNumber
                    Else
                        FormNumber = 0
                    End If
                Else
                    Exit Sub
                End If

                Dim fldName As String = FieldName
                Try
                    If ReplaceField = False Then
                        If FieldName.LastIndexOf("[") > 0 Then
                            Dim int As Integer = FieldName.LastIndexOf("[") + 1
                            fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                        Else
                            For Each fld As FDFField In _FDF(FormNumber).struc_FDFFields
                                If Not fld.FieldName Is Nothing Then
                                    If fld.FieldName = FieldName Then
                                        fldNumber += 1
                                    End If
                                End If
                            Next
                        End If
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                If Not _FDF(FormNumber).FormName Is Nothing Then
                    If Not _FDF(FormNumber).struc_FDFFields.Count <= 0 Then
                        Dim blnFound As Boolean = False
                        If ReplaceField = True Then
                            If Not _FDF(FormNumber).struc_FDFFields.Count <= 0 Then
                                If _FDF(FormNumber).struc_FDFFields.Count > 0 Then
                                    For Each xField In _FDF(FormNumber).struc_FDFFields
                                        If Not String_IsNullOrEmpty(xField.FieldName) Then
                                            If FieldName.ToLower = xField.FieldName.ToLower Then
                                                ' EDITED NK-INC @ 2012-04-07 NK
                                                xField.FieldValue.Clear()
                                                xField.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                                                xField.FieldEnabled = FieldEnabled
                                                xField.FieldType = FieldType
                                                blnFound = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        If blnFound = True Then
                            Exit Sub
                        Else
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldType = FieldType
                            _fld.FieldNum = fldNumber
                            Dim FldVals() As String, FldVal As String = "", MultiVal As String = ""
                            Select Case FieldType
                                Case FieldType.FldMultiSelect
                                    If FieldValue.Split("|").Length > 0 Then
                                        FldVals = FieldValue.Split("|")
                                        For Each FldVal In FldVals
                                            MultiVal &= "(" & FldVal & ")"
                                        Next
                                        _fld.FieldValue.Add(MultiVal)
                                        _fld.FieldEnabled = FieldEnabled
                                    Else
                                        _fld.FieldValue.Add(FieldValue)
                                        _fld.FieldEnabled = FieldEnabled
                                    End If
                                Case FieldType.FldOption
                                    _fld.FieldValue.Add(FieldValue)
                                    _fld.FieldEnabled = FieldEnabled
                                Case FieldType.FldTextual
                                    _fld.FieldValue.Add(FieldValue)
                                    _fld.FieldEnabled = FieldEnabled
                            End Select
                            _FDF(FormNumber).struc_FDFFields.Add(_fld)
                        End If
                    ElseIf Not FieldName = "" Then
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldValue.Add(XDPCheckChar(FieldValue))
                        _fld.FieldType = FieldType
                        _fld.FieldEnabled = FieldEnabled
                        _FDF(FormNumber).struc_FDFFields.Add(_fld)
                    End If
                End If
                Dim x As Integer
                x = 0
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Adds XDP Form field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldValue">Field value</param>
        ''' <param name="FieldType">Field type</param>
        ''' <param name="FieldEnabled">True</param>
        ''' <param name="ReplaceField">If True, replaces field</param>
        ''' <remarks></remarks>
        Public Sub XDPAddField(ByVal FieldName As String, ByVal FieldValue As String, Optional ByVal FieldType As FieldType = FieldType.FldTextual, Optional ByVal FieldEnabled As Boolean = True, Optional ByVal ReplaceField As Boolean = True)
            Dim fldNumber As Integer = 0
            Try
                Dim TmpCurFDFDoc As Integer = 0
                Dim XDPFDF As Integer = 0
                If _FDF.Count > 0 Then
                    If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName) Then
                        If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then
                            XDPFDF = _CurFDFDoc
                            GoTo contintue_here
                        Else
                            XDPFDF = _CurFDFDoc
                            GoTo contintue_here
                        End If
                    End If
                End If
                Exit Sub
contintue_here:

                Dim fldName As String = FieldName
                Try
                    If ReplaceField = False Then
                        If FieldName.LastIndexOf("[") > 0 Then
                            Dim int As Integer = FieldName.LastIndexOf("[") + 1
                            fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                        Else
                            For Each fld As FDFField In _FDF(XDPFDF).struc_FDFFields
                                If Not fld.FieldName Is Nothing Then
                                    If fld.FieldName = FieldName Then
                                        fldNumber += 1
                                    End If
                                End If
                            Next
                        End If
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try

                If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                    If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                        Dim blnFound As Boolean = False
                        If ReplaceField = True Then
                            If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                                If _FDF(XDPFDF).struc_FDFFields.Count > 0 Then
                                    For Each xField In _FDF(XDPFDF).struc_FDFFields
                                        If Not String_IsNullOrEmpty(xField.FieldName) Then
                                            If FieldName.ToLower = xField.FieldName.ToLower Then
                                                xField.FieldValue.Clear()
                                                xField.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                                                xField.FieldEnabled = FieldEnabled
                                                xField.FieldType = FieldType
                                                blnFound = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        If blnFound = True Then
                            Exit Sub
                        Else
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldType = FieldType
                            _fld.FieldNum = fldNumber
                            Dim FldVals() As String, FldVal As String = "", MultiVal As String = ""
                            Select Case FieldType
                                Case FieldType.FldMultiSelect
                                    If FieldValue.Split("|").Length > 0 Then
                                        FldVals = FieldValue.Split("|")
                                        For Each FldVal In FldVals
                                            MultiVal &= "(" & FldVal & ")"
                                        Next
                                        _fld.FieldValue.Add(MultiVal)
                                        _fld.FieldEnabled = FieldEnabled
                                    Else
                                        _fld.FieldValue.Add(FieldValue)
                                        _fld.FieldEnabled = FieldEnabled
                                    End If
                                Case FieldType.FldOption
                                    _fld.FieldValue.Add(FieldValue)
                                    _fld.FieldEnabled = FieldEnabled
                                Case FieldType.FldTextual
                                    _fld.FieldValue.Add(FieldValue)
                                    _fld.FieldEnabled = FieldEnabled
                            End Select
                            _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                        End If
                    ElseIf Not FieldName = "" Then
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldType = FieldType
                        _fld.FieldValue.Add(FieldValue)
                        _fld.FieldEnabled = FieldEnabled
                        _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                    End If
                End If
                Dim x As Integer
                x = 0
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Adds XDP Form field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="FieldValue">Field value</param>
        ''' <param name="FieldType">Field type</param>
        ''' <param name="FieldEnabled">True</param>
        ''' <param name="ReplaceField">If True, replaces field</param>
        ''' <remarks></remarks>
        Public Sub XDPAddField(ByVal FieldName As String, ByVal FieldNumber As Integer, ByVal FieldValue As String, Optional ByVal FieldType As FieldType = FieldType.FldTextual, Optional ByVal FieldEnabled As Boolean = True, Optional ByVal ReplaceField As Boolean = True)
            Dim fldNumber As Integer = FieldNumber
            Try
                Dim TmpCurFDFDoc As Integer = 0
                Dim XDPFDF As Integer = 0
                If _FDF.Count > 0 Then
                    If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName) Then
                        If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then
                            XDPFDF = _CurFDFDoc
                            GoTo contintue_here
                        Else
                            XDPFDF = _CurFDFDoc
                            GoTo contintue_here
                        End If
                    End If
                End If
                Exit Sub
contintue_here:

                Dim fldName As String = FieldName
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                    End If
                Catch ex As Exception
                    fldNumber = FieldNumber
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                    If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                        Dim blnFound As Boolean = False
                        If ReplaceField = True Then
                            If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                                If _FDF(XDPFDF).struc_FDFFields.Count > 0 Then
                                    For Each xField In _FDF(XDPFDF).struc_FDFFields
                                        If Not String_IsNullOrEmpty(xField.FieldName) Then
                                            If FieldName.ToLower = xField.FieldName.ToLower Then
                                                If FieldNumber = xField.FieldNum Then
                                                    ' EDITED NK-INC @ 2012-04-07 NK
                                                    xField.FieldValue.Clear()
                                                    xField.FieldValue.Add(Me.XDPCheckChar(FieldValue))
                                                    xField.FieldEnabled = FieldEnabled
                                                    xField.FieldType = FieldType
                                                    blnFound = True
                                                    Exit For
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        If blnFound = True Then
                            Exit Sub
                        Else
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType
                            Dim FldVals() As String, FldVal As String = "", MultiVal As String = ""
                            Select Case FieldType
                                Case FieldType.FldMultiSelect
                                    If FieldValue.Split("|").Length > 0 Then
                                        FldVals = FieldValue.Split("|")
                                        For Each FldVal In FldVals
                                            MultiVal &= "(" & FldVal & ")"
                                        Next
                                        _fld.FieldValue.Add(MultiVal)
                                        _fld.FieldEnabled = FieldEnabled
                                    Else
                                        _fld.FieldValue.Add(FieldValue)
                                        _fld.FieldEnabled = FieldEnabled
                                    End If
                                Case FieldType.FldOption
                                    _fld.FieldValue.Add(FieldValue)
                                    _fld.FieldEnabled = FieldEnabled
                                Case FieldType.FldTextual
                                    _fld.FieldValue.Add(FieldValue)
                                    _fld.FieldEnabled = FieldEnabled
                            End Select
                            _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                        End If
                    ElseIf Not FieldName = "" Then
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldValue.Add(FieldValue)
                        _fld.FieldType = FieldType
                        _fld.FieldEnabled = FieldEnabled
                        _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                    End If
                End If
                Dim x As Integer
                x = 0
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFAddField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Get last Field Number
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <remarks></remarks>
        Public Function XDPGetLastFieldNumber(ByVal FieldName As String) As Integer
            ' EDITED NK-INC 2012-04-07
            Dim fldNumber As Integer = -1
            Try
                Dim TmpCurFDFDoc As Integer = 0
                Dim XDPFDF As Integer = 0
                If _FDF.Count > 0 Then
                    If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName) Then
                        If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then
                            XDPFDF = _CurFDFDoc
                            GoTo contintue_here
                        Else
                            XDPFDF = _CurFDFDoc
                            GoTo contintue_here
                        End If
                    End If
                End If
                Exit Function
contintue_here:
                Dim fldName As String = FieldName
                Try
                    If False = False Then
                        If FieldName.LastIndexOf("[") > 0 Then
                            Dim int As Integer = FieldName.LastIndexOf("[") + 1
                            fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                        Else
                            For Each fld As FDFField In _FDF(XDPFDF).struc_FDFFields
                                If Not fld.FieldName Is Nothing Then
                                    If fld.FieldName = FieldName Then
                                        fldNumber += 1
                                    End If
                                End If
                            Next
                        End If
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Return fldNumber
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetLastFieldNumber", 1)
            End Try
        End Function
        ''' <summary>
        ''' Get Live-Cycle form path or url
        ''' </summary>
        ''' <value></value>
        ''' <returns>Live-Cycle form path or url</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property XDPGetFile() As String
            Get
                Return _PDF.FileName & ""
            End Get
        End Property
        ''' <summary>
        ''' Forces download of PDF file
        ''' </summary>
        ''' <param name="AspxPage">Me.Page</param>
        ''' <param name="FileBytes">Byte array of file</param>
        ''' <param name="FileName">PDF File name</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function PDFForceDownload(ByRef AspxPage As System.Web.UI.Page, ByVal FileBytes() As Byte, Optional ByVal FileName As String = "PDFDownloadForm.pdf") As Boolean
            Try
                If FileName = "" Then
                    Exit Function
                End If
                If FileBytes.Length > 0 Then
                    AspxPage.Response.Clear()
                    AspxPage.Response.ContentType = "application/pdf"
                    If Not FileName.ToString.ToLower.EndsWith(".pdf") Then
                        FileName &= ".pdf"
                    End If
                    If FileName = ".pdf" Then
                        FileName = "DownloadFile.pdf"
                    End If
                    'Response.ContentType = "application/octet-stream";
                    AspxPage.Response.AppendHeader("Content-Disposition", "attachment; filename=""" & FileName & """")
                    AspxPage.Response.BinaryWrite(FileBytes)
                    'AspxPage.Response.Write(outXML & "")
                    AspxPage.Response.Flush()
                    Return True
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.PDfForceDownload", 1)
                Return False
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Forces download of file
        ''' </summary>
        ''' <param name="AspxPage">Me.Page</param>
        ''' <param name="FileBytes">Byte array of file</param>
        ''' <param name="FileName">File name</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ForceDownload(ByRef AspxPage As System.Web.UI.Page, ByVal FileBytes() As Byte, Optional ByVal FileType As FDFType = FDFType.PDF, Optional ByVal FileName As String = "DownloadFile") As Boolean
            Try
                If FileName = "" Then
                    Exit Function
                End If
                Dim clsFDFApp As New FDFApp.FDFApp_Class
                If FileBytes.Length > 0 Then
                    AspxPage.Response.Clear()
                    Select Case FileType
                        Case FDFType.FDF
                            AspxPage.Response.ContentType = clsFDFApp.MimeFDF
                            If Not FileName.ToString.ToLower.EndsWith(".fdf") Then
                                FileName &= ".fdf"
                            End If
                        Case FDFType.PDF
                            AspxPage.Response.ContentType = clsFDFApp.MimePDF
                            If Not FileName.ToString.ToLower.EndsWith(".pdf") Then
                                FileName &= ".pdf"
                            End If
                        Case FDFType.XPDF
                            AspxPage.Response.ContentType = clsFDFApp.MimePDF
                            If Not FileName.ToString.ToLower.EndsWith(".pdf") Then
                                FileName &= ".pdf"
                            End If
                        Case FDFType.XDP
                            AspxPage.Response.ContentType = clsFDFApp.MimeXDP
                            If Not FileName.ToString.ToLower.EndsWith(".xdp") Then
                                FileName &= ".xdp"
                            End If
                        Case FDFType.xFDF
                            AspxPage.Response.ContentType = clsFDFApp.MimeXFDF
                            If Not FileName.ToString.ToLower.EndsWith(".xfdf") Then
                                FileName &= ".xfdf"
                            End If
                        Case FDFType.XML
                            AspxPage.Response.ContentType = clsFDFApp.MimeXML
                            If Not FileName.ToString.ToLower.EndsWith(".xml") Then
                                FileName &= ".xml"
                            End If
                    End Select
                    'Response.ContentType = "application/octet-stream";
                    AspxPage.Response.AppendHeader("Content-Disposition", "attachment; filename=""" & FileName & """")
                    AspxPage.Response.BinaryWrite(FileBytes)
                    AspxPage.Response.Flush()
                    Return True
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.PDfForceDownload", 1)
                Return False
                Exit Function
            End Try

        End Function
        ''' <summary>
        ''' Forces download of file
        ''' </summary>
        ''' <param name="AspxPage">Me.Page</param>
        ''' <param name="FileBytes">Byte array of file</param>
        ''' <param name="FileName">File name</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ForceDownload(ByRef AspxPage As System.Web.UI.Page, ByVal FileBytes() As Byte, ByVal FileName As String) As Boolean
            Try
                If FileName = "" Then
                    Exit Function
                End If
                Dim clsFDFApp As New FDFApp.FDFApp_Class
                If FileBytes.Length > 0 Then
                    AspxPage.Response.Clear()
                    If FileName.ToString.ToLower.EndsWith(".fdf") Then
                        AspxPage.Response.ContentType = clsFDFApp.MimeFDF
                    ElseIf FileName.ToString.ToLower.EndsWith(".pdf") Then
                        AspxPage.Response.ContentType = clsFDFApp.MimePDF
                    ElseIf FileName.ToString.ToLower.EndsWith(".xdp") Then
                        AspxPage.Response.ContentType = clsFDFApp.MimeXDP
                    ElseIf FileName.ToString.ToLower.EndsWith(".xfdf") Then
                        AspxPage.Response.ContentType = clsFDFApp.MimeXFDF
                    ElseIf Not FileName.ToString.ToLower.EndsWith(".xml") Then
                        AspxPage.Response.ContentType = clsFDFApp.MimeXML
                    Else
                        AspxPage.Response.ContentType = "application/octet-stream"
                    End If
                    AspxPage.Response.AppendHeader("Content-Disposition", "attachment; filename=""" & FileName & """")
                    AspxPage.Response.BinaryWrite(FileBytes)
                    AspxPage.Response.Flush()
                    Return True
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.PDfForceDownload", 1)
                Return False
                Exit Function
            End Try

        End Function
        Private Function XDPCheckChar(ByVal strINPUT As String) As String
            If strINPUT.Length <= 0 Then
                Return ""
                Exit Function
            End If
            strINPUT = strINPUT.Replace("&", "&amp;")
            strINPUT = strINPUT.Replace("<", "&lt;")
            strINPUT = strINPUT.Replace(">", "&gt;")
            strINPUT = strINPUT.Replace("""", "&quot;")
            strINPUT = strINPUT.Replace("'", "&apos;")
            strINPUT = strINPUT.Replace("'", "&apos;")
            strINPUT = strINPUT.Replace("`", "&apos;")
            strINPUT = strINPUT.Replace("""", "&apos;")
            strINPUT = strINPUT.Replace("", "&apos;")

            'strINPUT = strINPUT.Replace("&", "&&38;")
            'strINPUT = strINPUT.Replace("#", "&#35;")
            'strINPUT = strINPUT.Replace("&&38;", "&#38;")
            'strINPUT = strINPUT.Replace("<", "&#60;")
            'strINPUT = strINPUT.Replace(">", "&#62;")
            'strINPUT = strINPUT.Replace("(", "&#40;")
            'strINPUT = strINPUT.Replace(")", "&#41;")
            'strINPUT = strINPUT.Replace("'", "&#39;")
            'strINPUT = strINPUT.Replace("`", "&#39;")
            'strINPUT = strINPUT.Replace("""", "&#34;")
            'strINPUT = strINPUT.Replace("", "&#44;")
            'strINPUT = strINPUT.Replace("", "&#8217;")
            'strINPUT = strINPUT.Replace("$", "&#36;")
            'strINPUT = strINPUT.Replace("", "")
            'strINPUT = strINPUT.Replace("", "")

            Return strINPUT & ""

        End Function
        Private Function XDPCheckCharReverse(ByVal strINPUT As String) As String
            If strINPUT.Length <= 0 Then
                Return ""
                Exit Function
            End If
            Return strINPUT & ""
            Exit Function
            'strINPUT = strINPUT.Replace("&&38;", "&#38;")
            'strINPUT = strINPUT.Replace("&#60;", "<")
            'strINPUT = strINPUT.Replace("&#62;", ">")
            'strINPUT = strINPUT.Replace("&#40;", "(")
            'strINPUT = strINPUT.Replace("&#41;", ")")
            'strINPUT = strINPUT.Replace("&#39;", "'")
            'strINPUT = strINPUT.Replace("&#39;", "`")
            'strINPUT = strINPUT.Replace("&#34;", """")
            'strINPUT = strINPUT.Replace("&#44;", "")
            'strINPUT = strINPUT.Replace("&#39;", "'")
            'strINPUT = strINPUT.Replace("&#8217;", "")
            'strINPUT = strINPUT.Replace("&#36;", "$")
            'strINPUT = strINPUT.Replace("&#35;", "#")
            'strINPUT = strINPUT.Replace("&#38;", "&")

            strINPUT = strINPUT.Replace("&amp;", "&")
            strINPUT = strINPUT.Replace("&lt;", "<")
            strINPUT = strINPUT.Replace("&gt;", ">")
            strINPUT = strINPUT.Replace("&quot;", """")
            strINPUT = strINPUT.Replace("&apos;", "'")
            'strINPUT = strINPUT.Replace("&apos;", "'")
            'strINPUT = strINPUT.Replace("`", "&apos;")
            'strINPUT = strINPUT.Replace("""", "&apos;")
            'strINPUT = strINPUT.Replace("", "&apos;")
            Return strINPUT & ""

        End Function
        Private Function getXFADataElement(ByVal strXDP As String) As Xml.XmlElement
            Dim xmlDataDoc As New Xml.XmlDocument()
            xmlDataDoc.LoadXml(strXDP)
            Dim xmlNodeLst As Xml.XmlNodeList = xmlDataDoc.GetElementsByTagName("xfa:data")
            If Not xmlNodeLst Is Nothing And xmlNodeLst.Count > 0 Then
                Return xmlNodeLst(0)
            Else
                Return Nothing
            End If

        End Function
        Private Function WriteXDPFormFields_Original_Keep() As String
            Dim retString As String = ""
            Try
                '	"%FDF-1.2" & vbNewLine & 
                If Not _FDF.Count <= 0 Then
                    Dim FormIndex As Integer = 0
                    For Each XDPDoc1 As FDFDoc_Class In _FDF
                        FormIndex += 1
                        If Not XDPDoc1.struc_FDFFields.Count <= 0 Then
                            If XDPDoc1.struc_FDFFields.Count >= 1 Then  'XDPDoc1.DocType = FDFDocType.XDPForm And
                                If XDPDoc1.FormName & "" = "" Then
                                    XDPDoc1.FormName = "Page" & FormIndex
                                End If
                                retString &= "<" & XDPDoc1.FormName & ">"
                                For Each fld As FDFField In XDPDoc1.struc_FDFFields
                                    If Not fld.FieldName Is Nothing Then
                                        If fld.FieldType = FieldType.FldLiveCycleImage Then
                                            If Not fld.ImageBase64 Is Nothing Then
                                                'retString &= "<" & fld.FieldName & ">" & fld.FieldValue(0) & "</" & fld.FieldName & ">"
                                                retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                retString &= fld.ImageBase64
                                                retString &= "</" & fld.FieldName & ">"
                                            End If
                                        Else
                                            If fld.FieldValue.Count > 0 Then
                                                If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                    retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                Else
                                                    retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        End If
                                    ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                        If Not fld.ImageBase64 Is Nothing Then
                                            'retString &= "<" & fld.FieldName & ">" & fld.FieldValue(0) & "</" & fld.FieldName & ">"
                                            retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                            retString &= fld.ImageBase64
                                            retString &= "</" & fld.FieldName & ">"
                                        End If
                                    End If
                                Next
                                retString &= "</" & XDPDoc1.FormName & ">"
                            End If
                            'End If
                        Else
                            'retString = "<Form" & FormIndex & ">"
                        End If
                    Next
                End If
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try
        End Function
        Private Function WriteXDPFormFields_OLD() As String
            Dim retString As String = ""
            Try
                If Not _FDF.Count <= 0 Then
                    Dim FormIndex As Integer = 0
                    For Each XDPDoc1 As FDFDoc_Class In _FDF
                        FormIndex += 1
                        If Not XDPDoc1.struc_FDFFields.Count <= 0 Then
                            If XDPDoc1.struc_FDFFields.Count >= 1 Then  'XDPDoc1.DocType = FDFDocType.XDPForm And
                                If XDPDoc1.FormName & "" = "" Then
                                    XDPDoc1.FormName = "Page" & FormIndex
                                End If
                                retString &= "<" & XDPDoc1.FormName & ">"
                                For Each fld As FDFField In XDPDoc1.struc_FDFFields
                                    If Not fld.FieldName Is Nothing Then
                                        If fld.FieldType = FieldType.FldLiveCycleImage Then
                                            If Not fld.ImageBase64 Is Nothing Then
                                                retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                retString &= fld.ImageBase64
                                                retString &= "</" & fld.FieldName & ">"
                                            End If
                                        Else
                                            If fld.FieldValue.Count > 0 Then
                                                If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                    retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                Else
                                                    retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        End If
                                    ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                        If Not fld.ImageBase64 Is Nothing Then
                                            retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                            retString &= fld.ImageBase64
                                            retString &= "</" & fld.FieldName & ">"
                                        End If
                                    End If
                                Next
                                retString &= "</" & XDPDoc1.FormName & ">"
                            End If
                        Else
                        End If
                    Next
                End If
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try
        End Function
        Private Function WriteXDPFormFields_Original() As String
            Dim retString As String = ""
            Try
                If Not _FDF.Count <= 0 Then
                    Dim FormIndex As Integer = 0
                    For Each XDPDoc1 As FDFDoc_Class In _FDF
                        FormIndex += 1
                        If Not XDPDoc1.struc_FDFFields.Count <= 0 Then
                            If XDPDoc1.struc_FDFFields.Count >= 1 Then  'XDPDoc1.DocType = FDFDocType.XDPForm And
                                If XDPDoc1.FormName & "" = "" Then
                                    XDPDoc1.FormName = "Page" & FormIndex
                                End If
                                retString &= "<" & XDPDoc1.FormName & ">"
                                For Each fld As FDFField In XDPDoc1.struc_FDFFields
                                    If Not fld.FieldName Is Nothing Then
                                        If fld.FieldType = FieldType.FldLiveCycleImage Then
                                            If Not fld.ImageBase64 Is Nothing Then
                                                retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                retString &= fld.ImageBase64
                                                retString &= "</" & fld.FieldName & ">"
                                            End If
                                        Else
                                            If fld.FieldValue.Count > 0 Then
                                                If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                    retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                Else
                                                    retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        End If
                                    ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                        If Not fld.ImageBase64 Is Nothing Then
                                            retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                            retString &= fld.ImageBase64
                                            retString &= "</" & fld.FieldName & ">"
                                        End If
                                    End If
                                Next
                                retString &= "</" & XDPDoc1.FormName & ">"
                            End If
                        Else
                        End If
                    Next
                End If
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try
        End Function
        Private Function WriteXDPFormFields_Errors() As String
            Dim retString As String = ""
            For intForm As Integer = 0 To _FDF.Count - 1
                Dim _f As FDFDoc_Class = _FDF(intForm)
                _f.WrittenXDP = False
                _FDF(intForm) = _f
            Next
            Dim StartForm As String = ""
            Dim blnInLevel As Boolean = False, CurDepth As Integer = -1, PreviousDepth As Integer = 0
            Try
                If Not _FDF.Count <= 0 Then
                    Dim FormIndex As Integer = 0, SubformIndex As Integer = 0, PreviousDocumentLevels() As String = {}, CurrentDocumentLevels() As String = {}
                    For Each XDPDoc1 As FDFDoc_Class In _FDF
                        'If Not XDPDoc1.struc_FDFFields.Count <= 0 Then
                        If String_IsNullOrEmpty(XDPDoc1.FormName & "") Then
                            XDPDoc1.FormName = "form" & FormIndex + 1
                        End If
                        If String_IsNullOrEmpty(XDPDoc1.FormLevel & "") Then
                            XDPDoc1.FormLevel = XDPDoc1.FormName & ""
                        End If
                        If Not _FDF(FormIndex).WrittenXDP = True Then
                            If Not _FDF(FormIndex).struc_FDFFields.Count <= 0 Then
                                Try
                                    ' SETUP HISTORY
                                    PreviousDepth = CurDepth
                                    PreviousDocumentLevels = CurrentDocumentLevels

                                    CurrentDocumentLevels = XDPDoc1.FormLevel.Split("/")
                                    CurDepth = CurrentDocumentLevels.Length

                                    If CurDepth >= PreviousDepth Then
                                        retString &= "<" & XDPDoc1.FormName & ">"
                                        ReDim Preserve CurrentDocumentLevels(CurrentDocumentLevels.Length - 1)
                                        CurrentDocumentLevels(CurrentDocumentLevels.Length - 1) = XDPDoc1.FormName
                                    End If
                                    StartForm = XDPDoc1.FormName
                                    If XDPDoc1.struc_FDFFields.Count >= 1 Then  'XDPDoc1.DocType = FDFDocType.XDPForm And
                                        For Each fld As FDFField In XDPDoc1.struc_FDFFields
                                            If Not fld.FieldName Is Nothing Then
                                                If fld.FieldType = FieldType.FldLiveCycleImage Then
                                                    If Not fld.ImageBase64 Is Nothing Then
                                                        retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                        retString &= fld.ImageBase64
                                                        retString &= "</" & fld.FieldName & ">"
                                                    End If
                                                Else
                                                    If fld.FieldValue.Count > 0 Then
                                                        If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                            retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                        Else
                                                            retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                        End If
                                                    End If
                                                End If
                                            ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                                If Not fld.ImageBase64 Is Nothing Then
                                                    retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                    retString &= fld.ImageBase64
                                                    retString &= "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        Next
                                    End If
                                    retString &= WriteXDPSubforms(XDPDoc1.FormLevel)
                                Catch ex As Exception
                                    'Throw New Exception(ex.Message, ex)
                                Finally

                                End Try
                                Dim NextDepth As Integer = 0, NextDepthLevel As String() = {}
                                If _FDF.Count - 1 > FormIndex Then
                                    NextDepthLevel = _FDF(FormIndex + 1).FormLevel.Split("/")
                                    NextDepth = NextDepthLevel.Length
                                Else
                                    NextDepthLevel = _FDF(FormIndex).FormLevel.Split("/")
                                    NextDepth = NextDepthLevel.Length
                                End If
                                If CurDepth >= NextDepth Then
                                    For intDepthDifference As Integer = CurDepth To NextDepth Step -1
                                        retString &= "</" & CurrentDocumentLevels(intDepthDifference - 1) & ">"
                                    Next
                                    'retString &= "</" & XDPDoc1.FormName & ">"
                                End If
                            End If

                            'If StartForm = XDPDoc1.FormName Then
                            '    Exit For
                            'End If
                        End If
                        'End If
                        Dim _f2 As FDFDoc_Class = _FDF(FormIndex)
                        _f2.WrittenXDP = True
                        _FDF(FormIndex) = _f2
                        FormIndex += 1
                    Next
                End If
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try
        End Function
        Public Function WriteXMLFormFields() As String
            '' SUBFORMS
            Dim FormIndex As Integer = 0, SubformIndex As Integer = 0
            Dim retString As String = ""
            Try
                For intForm As Integer = 0 To _FDF.Count - 1
                    Dim _f As FDFDoc_Class = _FDF(intForm)
                    _f.WrittenXDP = False
                    _FDF(intForm) = _f
                Next
                Dim PrevFormLevel As String = ""
                Dim StartForm As String = ""
                Dim frmName As String = ""
                If Not _FDF.Count <= 0 Then

                    For Each XDPDoc1 As FDFDoc_Class In _FDF
                        If PrevFormLevel = "" Or PrevFormLevel <> XDPDoc1.FormLevel Or String_IsNullOrEmpty(XDPDoc1.FormLevel) Then
                            If Not XDPDoc1.struc_FDFFields.Count <= 0 And Not _FDF(FormIndex).WrittenXDP = True Then
                                Try
                                    frmName = XDPDoc1.FormName & ""
                                    If String_IsNullOrEmpty(XDPDoc1.FormName & "") Then
                                        frmName = "form" & FormIndex + 1
                                    End If
                                    If Not String_IsNullOrEmpty(XDPDoc1.FormName & "") Then
                                        frmName = XDPDoc1.FormName & ""
                                    End If
                                    If Not String_IsNullOrEmpty(XDPDoc1.FormLevel) Then
                                        If Not XDPDoc1.FormLevel.ToLower = PrevFormLevel.ToLower Then
                                            If Not XDPDoc1.FormName = "root" Then
                                                retString &= "<" & frmName & ">"
                                            End If
                                        End If
                                    Else
                                        If Not XDPDoc1.FormName = "root" Then
                                            retString &= "<" & frmName & ">"
                                        End If
                                    End If
                                    'retString &= "<" & XDPDoc1.FormName & ">"
                                    StartForm = XDPDoc1.FormName
                                    If XDPDoc1.struc_FDFFields.Count >= 1 Then  'XDPDoc1.DocType = FDFDocType.XDPForm And
                                        For Each fld As FDFField In XDPDoc1.struc_FDFFields
                                            If Not fld.FieldName Is Nothing Then

                                                If fld.FieldType = FieldType.FldLiveCycleImage Then
                                                    If Not fld.ImageBase64 Is Nothing Then
                                                        ' EDITED 2012-04-07 NK-INC NK - Removed XFA
                                                        retString &= "<" & fld.FieldName & " contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                        retString &= fld.ImageBase64
                                                        retString &= "</" & fld.FieldName & ">"
                                                    End If
                                                Else
                                                    If fld.FieldValue.Count > 0 Then
                                                        If String_IsNullOrEmpty(fld.FieldValue(0)) = True Then
                                                            retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                        Else
                                                            retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                        End If
                                                    End If
                                                End If
                                            ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                                If Not fld.ImageBase64 Is Nothing Then
                                                    ' EDITED 2012-04-07 NK-INC NK - Removed XFA
                                                    retString &= "<" & fld.FieldName & " contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                    retString &= fld.ImageBase64
                                                    retString &= "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        Next
                                    End If
                                    retString &= WriteXDPSubforms(XDPDoc1.FormLevel)
                                Catch ex As Exception
                                    Throw New Exception(ex.Message, ex)
                                Finally
                                    'If Subform1.struc_FDFFields.Count >= 1 Then	'XDPDoc1.DocType = FDFDocType.XDPForm And
                                    If Not String_IsNullOrEmpty(XDPDoc1.FormLevel) Then
                                        If Not XDPDoc1.FormLevel.ToLower = PrevFormLevel.ToLower Then
                                            If Not frmName = "root" Then
                                                retString &= "</" & frmName & ">"
                                            End If
                                        End If
                                        PrevFormLevel = XDPDoc1.FormLevel
                                    Else
                                        If Not frmName = "root" Then
                                            retString &= "</" & frmName & ">"
                                        End If
                                    End If
                                    'retString &= "</" & XDPDoc1.FormName & ">"
                                End Try
                            End If
                            FormIndex += 1
                        End If
                    Next
                End If
                Try
                    For FormIndex = 0 To _FDF.Count - 1
                        Dim _f3 As FDFDoc_Class = _FDF(FormIndex)
                        _f3.WrittenXDP = True
                        _FDF(FormIndex) = _f3
                    Next
                Catch ex3 As Exception
                    Err.Clear()
                End Try
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try
        End Function
        Private Function WriteXDPFormFields() As String
            '' SUBFORMS
            Dim FormIndex As Integer = 0, SubformIndex As Integer = 0
            Dim retString As String = ""
            Try
                For intForm As Integer = 0 To _FDF.Count - 1
                    Dim _f As FDFDoc_Class = _FDF(intForm)
                    _f.WrittenXDP = False
                    _FDF(intForm) = _f
                Next
                Dim PrevFormLevel As String = ""
                Dim StartForm As String = ""
                If Not _FDF.Count <= 0 Then

                    For Each XDPDoc1 As FDFDoc_Class In _FDF
                        If PrevFormLevel = "" Or PrevFormLevel <> XDPDoc1.FormLevel Or String_IsNullOrEmpty(XDPDoc1.FormLevel) Then
                            If XDPDoc1.struc_FDFFields.Count > 0 And Not _FDF(FormIndex).WrittenXDP = True Then

                                If String_IsNullOrEmpty(XDPDoc1.FormName & "") Then
                                    XDPDoc1.FormName = "form" & FormIndex + 1
                                End If
                                If String_IsNullOrEmpty(XDPDoc1.FormLevel & "") Then
                                    XDPDoc1.FormLevel = XDPDoc1.FormName & ""
                                End If
                                Try
                                    If Not XDPDoc1.FormLevel.ToLower = PrevFormLevel.ToLower Then
                                        If Not XDPDoc1.FormName = "root" Then
                                            retString &= "<" & XDPDoc1.FormName & ">"
                                        End If
                                    End If
                                    'retString &= "<" & XDPDoc1.FormName & ">"
                                    StartForm = XDPDoc1.FormName
                                    If XDPDoc1.struc_FDFFields.Count >= 1 Then  'XDPDoc1.DocType = FDFDocType.XDPForm And
                                        For Each fld As FDFField In XDPDoc1.struc_FDFFields
                                            If Not fld.FieldName Is Nothing Then
                                                If fld.FieldType = FieldType.FldLiveCycleImage Then
                                                    If Not fld.ImageBase64 Is Nothing Then
                                                        retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                        retString &= fld.ImageBase64
                                                        retString &= "</" & fld.FieldName & ">"
                                                    End If
                                                Else
                                                    If fld.FieldValue.Count > 0 Then
                                                        If String_IsNullOrEmpty(fld.FieldValue(0)) = True Then
                                                            retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                        Else
                                                            retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                        End If
                                                    End If
                                                End If
                                            ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                                If Not fld.ImageBase64 Is Nothing Then
                                                    retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                    retString &= fld.ImageBase64
                                                    retString &= "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        Next
                                    End If
                                    retString &= WriteXDPSubforms(XDPDoc1.FormLevel)
                                Catch ex As Exception
                                    Throw New Exception(ex.Message, ex)
                                Finally
                                    'If Subform1.struc_FDFFields.Count >= 1 Then	'XDPDoc1.DocType = FDFDocType.XDPForm And
                                    If Not XDPDoc1.FormLevel.ToLower = PrevFormLevel.ToLower Then
                                        If Not XDPDoc1.FormName = "root" Then
                                            retString &= "</" & XDPDoc1.FormName & ">"
                                        End If
                                    End If
                                    PrevFormLevel = XDPDoc1.FormLevel
                                    'retString &= "</" & XDPDoc1.FormName & ">"
                                End Try
                            End If
                            FormIndex += 1
                        End If
                    Next
                End If
                Try
                    For FormIndex = 0 To _FDF.Count - 1
                        Dim _f3 As FDFDoc_Class = _FDF(FormIndex)
                        _f3.WrittenXDP = True
                        _FDF(FormIndex) = _f3
                    Next
                Catch ex3 As Exception
                    Err.Clear()
                End Try
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try
        End Function
        Private Function WriteXDPFormFields_3D_Works_Good() As String
            '' SUBFORMS
            Dim FormIndex As Integer = 0, SubformIndex As Integer = 0
            Dim retString As String = ""
            Try
                For intForm As Integer = 0 To _FDF.Count - 1
                    Dim _f As FDFDoc_Class = _FDF(intForm)
                    _f.WrittenXDP = False
                    _FDF(intForm) = _f
                Next
                Dim PrevFormLevel As String = ""
                Dim StartForm As String = ""
                If Not _FDF.Count <= 0 Then

                    For Each XDPDoc1 As FDFDoc_Class In _FDF
                        If PrevFormLevel = "" Or PrevFormLevel = XDPDoc1.FormLevel Then
                            If Not XDPDoc1.struc_FDFFields.Count <= 0 And Not _FDF(FormIndex).WrittenXDP = True Then
                                If String_IsNullOrEmpty(XDPDoc1.FormName & "") Then
                                    XDPDoc1.FormName = "form" & FormIndex + 1
                                End If
                                If String_IsNullOrEmpty(XDPDoc1.FormLevel & "") Then
                                    XDPDoc1.FormLevel = XDPDoc1.FormName & ""
                                End If
                                Try
                                    If Not XDPDoc1.FormLevel.ToLower = PrevFormLevel.ToLower Then
                                        If Not XDPDoc1.FormName = "root" Then
                                            retString &= "<" & XDPDoc1.FormName & ">"
                                        End If
                                    End If
                                    'retString &= "<" & XDPDoc1.FormName & ">"
                                    StartForm = XDPDoc1.FormName
                                    If XDPDoc1.struc_FDFFields.Count >= 1 Then  'XDPDoc1.DocType = FDFDocType.XDPForm And
                                        For Each fld As FDFField In XDPDoc1.struc_FDFFields
                                            If Not fld.FieldName Is Nothing Then
                                                If fld.FieldType = FieldType.FldLiveCycleImage Then
                                                    If Not fld.ImageBase64 Is Nothing Then
                                                        retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                        retString &= fld.ImageBase64
                                                        retString &= "</" & fld.FieldName & ">"
                                                    End If
                                                Else
                                                    If fld.FieldValue.Count > 0 Then
                                                        If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                            retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                        Else
                                                            retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                        End If
                                                    End If
                                                End If
                                            ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                                If Not fld.ImageBase64 Is Nothing Then
                                                    retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                    retString &= fld.ImageBase64
                                                    retString &= "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        Next
                                    End If
                                    retString &= WriteXDPSubforms(XDPDoc1.FormLevel)
                                Catch ex As Exception
                                    Throw New Exception(ex.Message, ex)
                                Finally
                                    'If Subform1.struc_FDFFields.Count >= 1 Then	'XDPDoc1.DocType = FDFDocType.XDPForm And
                                    If Not XDPDoc1.FormLevel.ToLower = PrevFormLevel.ToLower Then
                                        If Not XDPDoc1.FormName = "root" Then
                                            retString &= "</" & XDPDoc1.FormName & ">"
                                        End If
                                    End If
                                    PrevFormLevel = XDPDoc1.FormLevel.ToLower
                                    'retString &= "</" & XDPDoc1.FormName & ">"
                                End Try
                            End If
                            FormIndex += 1
                        End If
                    Next
                End If
                Try
                    For FormIndex = 0 To _FDF.Count - 1
                        Dim _f3 As FDFDoc_Class = _FDF(FormIndex)
                        _f3.WrittenXDP = True
                        _FDF(FormIndex) = _f3
                    Next
                Catch ex3 As Exception
                    Err.Clear()
                End Try
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try
        End Function
        Private Function WriteXDPSubforms(ByVal formLevel As String) As String
            Dim FormIndex As Integer, SubformIndex As Integer = 0
            Dim retString As String = ""
            Try
                For Each Subform1 As FDFDoc_Class In _FDF
                    If String_IsNullOrEmpty(Subform1.FormName & "") Then
                        Subform1.FormName = "subform" & "1"
                    End If

                    If (formLevel = Subform1.FormLevel) And FormIndex = -100 Then
                        If Not _FDF(FormIndex).WrittenXDP = True Then
                            If Not Subform1.struc_FDFFields.Count <= 0 Then
                                If String_IsNullOrEmpty(Subform1.FormName & "") Then
                                    Subform1.FormName = "subform" & SubformIndex + 1
                                End If
                                If String_IsNullOrEmpty(Subform1.FormLevel & "") Then
                                    Subform1.FormLevel = Subform1.FormName & ""
                                End If
                                If Not _FDF(FormIndex).WrittenXDP = True Then
                                    If Not formLevel.ToLower = Subform1.FormLevel.ToLower Then
                                        retString &= "<" & Subform1.FormName & ">"
                                    End If

                                    Try
                                        For Each fld As FDFField In Subform1.struc_FDFFields
                                            If Not fld.FieldName Is Nothing Then
                                                If fld.FieldType = FieldType.FldLiveCycleImage Then
                                                    If Not fld.ImageBase64 Is Nothing Then
                                                        retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                        retString &= fld.ImageBase64
                                                        retString &= "</" & fld.FieldName & ">"
                                                    End If
                                                Else
                                                    If fld.FieldValue.Count > 0 Then
                                                        If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                            retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                        Else
                                                            retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                        End If
                                                    End If
                                                End If
                                            ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                                If Not fld.ImageBase64 Is Nothing Then
                                                    retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                    retString &= fld.ImageBase64
                                                    retString &= "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        Next
                                        retString &= WriteXDPSubforms(Subform1.FormLevel)
                                    Catch ex As Exception
                                        Throw New Exception(ex.Message, ex)
                                    Finally
                                        If Subform1.FormName = "Table4" Then
                                            Dim str As String = ""
                                        End If
                                        If Not formLevel.ToLower = Subform1.FormLevel.ToLower Then
                                            retString &= "</" & Subform1.FormName & ">"
                                        Else
                                            retString = retString
                                        End If
                                    End Try

                                    'End If
                                End If
                            End If
                            SubformIndex += 1
                        End If
                    ElseIf Not String_IsNullOrEmpty(Subform1.FormLevel & "") And Not Subform1.FormLevel = "" And Not Subform1.WrittenXDP = True Then
                        Dim frms() As String = Subform1.FormLevel.Split("/")
                        Dim curFrms() As String = formLevel.Split("/"), intFoundSubform As Integer = -1
                        Try
                            For intFrm As Integer = 0 To curFrms.Length
                                If frms(intFrm) = curFrms(intFrm) Then
                                    intFoundSubform += 1
                                Else
                                    Exit For
                                End If
                            Next
                        Catch ex As Exception

                        End Try

                        If intFoundSubform = curFrms.Length - 1 And frms.Length > curFrms.Length And Not Subform1.WrittenXDP = True Then
                            If Not Subform1.struc_FDFFields.Count <= 0 Then
                                If String_IsNullOrEmpty(Subform1.FormName & "") Then
                                    Subform1.FormName = "subform" & SubformIndex + 1
                                End If
                                If String_IsNullOrEmpty(Subform1.FormLevel & "") Then
                                    Subform1.FormLevel = Subform1.FormName & ""
                                End If
                                If Not _FDF(FormIndex).WrittenXDP = True Then
                                    If Not formLevel.ToLower = Subform1.FormLevel.ToLower Then
                                        retString &= "<" & Subform1.FormName & ">"
                                    End If

                                    Try
                                        For Each fld As FDFField In Subform1.struc_FDFFields
                                            If Not fld.FieldName Is Nothing Then
                                                If fld.FieldType = FieldType.FldLiveCycleImage Then
                                                    If Not fld.ImageBase64 Is Nothing Then
                                                        retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                        retString &= fld.ImageBase64
                                                        retString &= "</" & fld.FieldName & ">"
                                                    End If
                                                Else
                                                    If fld.FieldValue.Count > 0 Then
                                                        If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                            retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                        Else
                                                            retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                        End If
                                                    End If
                                                End If
                                            ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                                If Not fld.ImageBase64 Is Nothing Then
                                                    retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                    retString &= fld.ImageBase64
                                                    retString &= "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        Next
                                        retString &= WriteXDPSubforms(Subform1.FormLevel)
                                    Catch ex As Exception
                                        Throw New Exception(ex.Message, ex)
                                    Finally
                                        If Subform1.FormName = "Table4" Then
                                            Dim str As String = ""
                                        End If
                                        If Not formLevel.ToLower = Subform1.FormLevel.ToLower Then
                                            retString &= "</" & Subform1.FormName & ">"
                                        Else
                                            retString = retString
                                        End If
                                    End Try

                                    'End If
                                End If
                            End If
                            SubformIndex += 1
                        End If
                    End If
                    FormIndex += 1
                Next
                Try
                    
                Catch ex3 As Exception
                    Err.Clear()
                End Try
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try

        End Function
        Private Function WriteXDPSubforms_3D_Works_Good(ByVal formLevel As String) As String
            Dim FormIndex As Integer, SubformIndex As Integer = 0
            Dim retString As String = ""
            Try
                For Each Subform1 As FDFDoc_Class In _FDF
                    If String_IsNullOrEmpty(Subform1.FormName & "") Then
                        Subform1.FormName = "subform" & "1"
                    End If

                    If (formLevel = Subform1.FormLevel) And FormIndex = -100 Then
                        If Not _FDF(FormIndex).WrittenXDP = True Then
                            If Not Subform1.struc_FDFFields.Count <= 0 Then
                                If String_IsNullOrEmpty(Subform1.FormName & "") Then
                                    Subform1.FormName = "subform" & SubformIndex + 1
                                End If
                                If String_IsNullOrEmpty(Subform1.FormLevel & "") Then
                                    Subform1.FormLevel = Subform1.FormName & ""
                                End If
                                If Not _FDF(FormIndex).WrittenXDP = True Then
                                    '_f3.WrittenXDP = True
                                    'If Subform1.struc_FDFFields.Count >= 1 Then	'XDPDoc1.DocType = FDFDocType.XDPForm And
                                    If Not formLevel.ToLower = Subform1.FormLevel.ToLower Then
                                        retString &= "<" & Subform1.FormName & ">"
                                    End If

                                    Try
                                        For Each fld As FDFField In Subform1.struc_FDFFields
                                            If Not fld.FieldName Is Nothing Then
                                                If fld.FieldType = FieldType.FldLiveCycleImage Then
                                                    If Not fld.ImageBase64 Is Nothing Then
                                                        retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                        retString &= fld.ImageBase64
                                                        retString &= "</" & fld.FieldName & ">"
                                                    End If
                                                Else
                                                    If fld.FieldValue.Count > 0 Then
                                                        If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                            retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                        Else
                                                            retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                        End If
                                                    End If
                                                End If
                                            ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                                If Not fld.ImageBase64 Is Nothing Then
                                                    retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                    retString &= fld.ImageBase64
                                                    retString &= "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        Next
                                        retString &= WriteXDPSubforms(Subform1.FormLevel)
                                    Catch ex As Exception
                                        Throw New Exception(ex.Message, ex)
                                    Finally
                                        If Subform1.FormName = "Table4" Then
                                            Dim str As String = ""
                                        End If
                                        If Not formLevel.ToLower = Subform1.FormLevel.ToLower Then
                                            retString &= "</" & Subform1.FormName & ">"
                                        Else
                                            retString = retString
                                        End If
                                    End Try

                                    'End If
                                End If
                            End If
                            SubformIndex += 1
                        End If
                    ElseIf Not String_IsNullOrEmpty(Subform1.FormLevel & "") And Not Subform1.FormLevel = "" And Not Subform1.WrittenXDP = True Then
                        Dim SubForms() As String = formLevel.Split("/")
                        If (Subform1.FormLevel.IndexOf(formLevel) >= 0 And (Subform1.FormLevel.Split("/").Length >= formLevel.Split("/").Length)) Then
                            If Not Subform1.struc_FDFFields.Count <= 0 Then
                                If String_IsNullOrEmpty(Subform1.FormName & "") Then
                                    Subform1.FormName = "subform" & SubformIndex + 1
                                End If
                                If String_IsNullOrEmpty(Subform1.FormLevel & "") Then
                                    Subform1.FormLevel = Subform1.FormName & ""
                                End If
                                If Not _FDF(FormIndex).WrittenXDP = True Then
                                    If Not formLevel.ToLower = Subform1.FormLevel.ToLower Then
                                        retString &= "<" & Subform1.FormName & ">"
                                    End If

                                    Try
                                        For Each fld As FDFField In Subform1.struc_FDFFields
                                            If Not fld.FieldName Is Nothing Then
                                                If fld.FieldType = FieldType.FldLiveCycleImage Then
                                                    If Not fld.ImageBase64 Is Nothing Then
                                                        retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                        retString &= fld.ImageBase64
                                                        retString &= "</" & fld.FieldName & ">"
                                                    End If
                                                Else
                                                    If fld.FieldValue.Count > 0 Then
                                                        If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                            retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                        Else
                                                            retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                        End If
                                                    End If
                                                End If
                                            ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                                If Not fld.ImageBase64 Is Nothing Then
                                                    retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                    retString &= fld.ImageBase64
                                                    retString &= "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        Next
                                        retString &= WriteXDPSubforms(Subform1.FormLevel)
                                    Catch ex As Exception
                                        Throw New Exception(ex.Message, ex)
                                    Finally
                                        If Subform1.FormName = "Table4" Then
                                            Dim str As String = ""
                                        End If
                                        If Not formLevel.ToLower = Subform1.FormLevel.ToLower Then
                                            retString &= "</" & Subform1.FormName & ">"
                                        Else
                                            retString = retString
                                        End If
                                    End Try

                                    'End If
                                End If
                            End If
                            SubformIndex += 1
                        End If
                    End If
                    FormIndex += 1
                Next
                Try
                    For FormIndex = 0 To _FDF.Count - 1
                        Dim _f3 As FDFDoc_Class = _FDF(FormIndex)
                        _f3.WrittenXDP = True
                        _FDF(FormIndex) = _f3
                    Next
                Catch ex3 As Exception
                    Err.Clear()
                End Try
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try

        End Function
        Private Function WriteXDPSubforms_Good(ByVal FDFDocs As FDFDoc_Class(), ByVal formLevel As String) As String
            Dim FormIndex As Integer = 0, SubformIndex As Integer = 0
            Dim retString As String = ""
            Try
                For Each Subform1 As FDFDoc_Class In FDFDocs
                    If String_IsNullOrEmpty(Subform1.FormName & "") Then
                        Subform1.FormName = "subform" & "1"
                    End If
                    Dim SubForms() As String = formLevel.Split("/")
                    If Not String_IsNullOrEmpty(Subform1.FormLevel & "") And Not Subform1.FormLevel & "" = "root" And Not Subform1.WrittenXDP = True Then
                        If Subform1.FormLevel.IndexOf(formLevel) >= 0 And (Subform1.FormLevel.Split("/").Length = formLevel.Split("/").Length + 1) Then
                            If Not Subform1.struc_FDFFields.Count <= 0 Then
                                If String_IsNullOrEmpty(Subform1.FormName & "") Then
                                    Subform1.FormName = "subform" & SubformIndex + 1
                                End If
                                If String_IsNullOrEmpty(Subform1.FormLevel & "") Then
                                    Subform1.FormLevel = Subform1.FormName & ""
                                End If
                                If Not _FDF(FormIndex).WrittenXDP = True Then
                                    'If Subform1.struc_FDFFields.Count >= 1 Then	'XDPDoc1.DocType = FDFDocType.XDPForm And
                                    retString &= "<" & Subform1.FormName & ">"
                                    Try
                                        For Each fld As FDFField In Subform1.struc_FDFFields
                                            If Not fld.FieldName Is Nothing Then
                                                If fld.FieldType = FieldType.FldLiveCycleImage Then
                                                    If Not fld.ImageBase64 Is Nothing Then
                                                        retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                        retString &= fld.ImageBase64
                                                        retString &= "</" & fld.FieldName & ">"
                                                    End If
                                                Else
                                                    If fld.FieldValue.Count > 0 Then
                                                        If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                            retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                                        Else
                                                            retString &= "<" & fld.FieldName & ">" & XDPCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                                        End If
                                                    End If
                                                End If
                                            ElseIf fld.FieldType = FieldType.FldLiveCycleImage Then
                                                If Not fld.ImageBase64 Is Nothing Then
                                                    retString &= "<" & fld.FieldName & " xfa:contentType=""" & IIf(String_IsNullOrEmpty(fld.FieldValue(0)), "image/jpg", fld.FieldValue(0)) & """ href="""">"
                                                    retString &= fld.ImageBase64
                                                    retString &= "</" & fld.FieldName & ">"
                                                End If
                                            End If
                                        Next
                                        retString &= WriteXDPSubforms(Subform1.FormLevel)
                                    Catch ex As Exception
                                        Throw New Exception(ex.Message, ex)
                                    Finally
                                        If Subform1.FormName = "Table4" Then
                                            Dim str As String = ""
                                        End If
                                        retString &= "</" & Subform1.FormName & ">"
                                    End Try
                                    'End If
                                End If
                            End If
                            SubformIndex += 1
                        End If
                    End If
                    FormIndex += 1
                Next
                Try
                    For FormIndex = 0 To _FDF.Count - 1
                        Dim _f3 As FDFDoc_Class = _FDF(FormIndex)
                        _f3.WrittenXDP = True
                        _FDF(FormIndex) = _f3
                    Next
                Catch ex3 As Exception
                    Err.Clear()
                End Try
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFDoc.WriteTemplates", 1)
                Return ""
            End Try

        End Function
        ''' <summary>
        ''' Merges XDP Live-Cycle form with data to Byte array
        ''' </summary>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="PDFFormPath">Live-Cycle form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Merged Live-Cycle form with data in byte array</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Byte()
            Dim formFile As String = PDFFormPath & ""
            If formFile = "" Then
                If FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFGetFile & ""
                End If
            End If

            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'xStream.Close()
                    'Dim xStream As New FileStream(newFile, FileMode.Create)
                    'Dim byteRead() As Byte
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    'stamper.Close()
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Merges FDF Data and PDF form to byte array
        ''' </summary>
        ''' <param name="PDFForm">Original Blank PDF Form Bytes</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal PDFForm As Byte(), Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "", Optional ByVal RemoveUsageRights As Boolean = False) As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                'Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    'stamper.Close()
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF form to byte array
        ''' </summary>
        ''' <param name="PDFForm">Original Blank PDF Form URL or Local File Path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal PDFForm As String, Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Byte()
            Dim formFile As String = PDFForm
            If formFile = "" Then
                If FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFGetFile & ""
                End If
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                ',Optional ByVal RemoveUsageRights As Boolean = False
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF form to byte array
        ''' </summary>
        ''' <param name="PDFForm">Original Blank PDF Form Bytes</param>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal PDFForm As Byte(), ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, ByVal Flatten As Boolean, ByVal EncryptionStrength As EncryptionStrength, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                'Optional ByVal RemoveUsageRights As Boolean = False
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                'Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF form to byte array
        ''' </summary>
        ''' <param name="PDFForm">Original Blank PDF Form URL or Local File Path</param>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal PDFForm As String, ByVal OpenPassword As String, ByVal ModificationPassword As String, Optional ByVal Permissions As Integer = 0, Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = FDFApp.FDFDoc_Class.EncryptionStrength.STRENGTH40BITS, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                '
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF form to byte array
        ''' </summary>
        ''' <param name="PDFForm">Original Blank PDF Form Stream</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal PDFForm As Stream, Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Byte()
            If PDFForm.CanSeek Then
                PDFForm.Position = 0
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                '
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF form to stream
        ''' </summary>
        ''' <param name="PDFForm">Original Blank PDF Form Stream</param>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal PDFForm As Stream, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Byte()
            If PDFForm.CanSeek Then
                PDFForm.Position = 0
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                '
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges FDF Data and PDF form to byte array
        ''' </summary>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="PDFForm">Original Blank PDF Form Stream</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with merged FDF Data and PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, ByVal PDFForm As Stream, Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Byte()
            If PDFForm.CanSeek Then
                PDFForm.Position = 0
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                '
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges XDP Live-Cycle form with data to Byte array
        ''' </summary>
        ''' <param name="FDFDocument">FDFDoc_Class object</param>
        ''' <param name="PDFFormPath">Live-Cycle PDF form path or url</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Merged Live-Cycle form with data in byte array</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal FDFDocument As FDFApp.FDFDoc_Class, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Byte()
            Dim formFile As String = PDFFormPath & ""
            If formFile = "" Then
                If FDFDocument.FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFDocument.FDFGetFile & ""
                End If
            End If

            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try

        End Function
        Private Sub PDF_iTextSharp_SetSubmitButtonURLs(ByRef myPDFStamer As iTextSharp.text.pdf.PdfStamper, ByRef myPDFReader As iTextSharp.text.pdf.PdfReader)
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFActions.Count <= 0 Then
                    Dim xAction As FDFActions
                    Dim xCntr As Integer = 0
                    For Each xAction In _FDF(_CurFDFDoc).struc_FDFActions
                        If xAction.ActionType = ActionTypes.Submit Then
                            Try
                                Dim acroFields As iTextSharp.text.pdf.AcroFields = myPDFStamer.AcroFields
                                Dim hastTable As Hashtable = acroFields.Fields
                                Dim fieldItem As iTextSharp.text.pdf.AcroFields.Item = acroFields.GetFieldItem(xAction.FieldName & "")

                                Dim reference As iTextSharp.text.pdf.PRIndirectReference = CType(fieldItem.widget_refs(0), iTextSharp.text.pdf.PRIndirectReference)
                                Dim obj As iTextSharp.text.pdf.PdfDictionary = CType(myPDFReader.GetPdfObject(reference.Number), iTextSharp.text.pdf.PdfDictionary)

                                Dim action As iTextSharp.text.pdf.PdfDictionary = CType(obj.GetAsDict(iTextSharp.text.pdf.PdfName.A), iTextSharp.text.pdf.PdfDictionary)
                                If action Is CType(Nothing, iTextSharp.text.pdf.PdfDictionary) Then
                                    'action = obj.Put(iTextSharp.text.pdf.PdfName.A, iTextSharp.text.pdf.PdfAction.SUBMIT_INCLUDE_NO_VALUE_FIELDS)
                                    Dim newField As iTextSharp.text.pdf.PushbuttonField = acroFields.GetNewPushbuttonFromField(xAction.FieldName)
                                    action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(xAction.JavaScript_URL, Nothing, iTextSharp.text.pdf.PdfAction.SUBMIT_INCLUDE_NO_VALUE_FIELDS)
                                    Dim submitField As iTextSharp.text.pdf.PdfFormField
                                    submitField = newField.Field
                                    submitField.Action = action
                                    acroFields.ReplacePushbuttonField(xAction.FieldName & "", submitField)
                                Else
                                    Dim file As iTextSharp.text.pdf.PdfDictionary = CType(action.Get(iTextSharp.text.pdf.PdfName.F), iTextSharp.text.pdf.PdfDictionary)
                                    file.Put(iTextSharp.text.pdf.PdfName.F, New iTextSharp.text.pdf.PdfString(xAction.JavaScript_URL & ""))
                                End If
                            Catch ex As Exception
                                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDF_iTextSharp_SetSubmitURLs", 1)
                            End Try
                        End If
                        xCntr += 1
                    Next
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDF_iTextSharp_SetSubmitURLs", 2)
            End Try
        End Sub
        Private Sub PDF_iTextSharp_AddCloseDocActionToButton(ByRef myPDFStamer As iTextSharp.text.pdf.PdfStamper, ByRef myPDFReader As iTextSharp.text.pdf.PdfReader, ByVal buttonName As String)
            Dim action As iTextSharp.text.pdf.PdfAction = iTextSharp.text.pdf.PdfAction.JavaScript("this.closeDoc(true);\r", myPDFStamer.Writer)
            myPDFStamer.Writer.AddJavaScript(action)
            Dim acroFields As iTextSharp.text.pdf.AcroFields = myPDFStamer.AcroFields
            Dim hastTable As Hashtable = acroFields.Fields
            Dim fieldItem As iTextSharp.text.pdf.AcroFields.Item = acroFields.GetFieldItem(buttonName)
            fieldItem.widgets.Add(action)
        End Sub

        ''' <summary>
        ''' Merges XDP Live-Cycle form with data to Byte array
        ''' </summary>
        ''' <param name="XDPData">XDP Data to Merge</param>
        ''' <param name="FDFDocument">FDFDoc_Class object</param>
        ''' <param name="PDFFormPath">Live-Cycle PDF form path or url</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Merged Live-Cycle form with data in byte array</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Buf(ByVal XDPData As String, ByVal FDFDocument As FDFApp.FDFDoc_Class, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Byte()
            Dim formFile As String = PDFFormPath & ""
            If formFile = "" Then
                If FDFDocument.FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFDocument.FDFGetFile & ""
                End If
            End If

            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges XDP Live-Cycle form with data to stream
        ''' </summary>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="PDFFormPath">Live-Cycle form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Merged Live-Cycle form with data in stream</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Stream(ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Stream
            Dim formFile As String = PDFFormPath & ""
            If formFile = "" Then
                If FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFGetFile & ""
                End If
            End If

            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                'If (stamper.AcroFields.Fields.Count <> 0) Then
                '    Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                '    fields.IsGoingTobeFlattened(Flatten)
                '    fields.MergeXfaData(inputDataElement)
                'Else
                '    Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                '    xfaForm.Changed = True
                '    iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                'End If
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    If MemStream.CanSeek Then MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges XDP Live-Cycle form with data to stream
        ''' </summary>
        ''' <param name="XDPData">XDP Data to merge</param>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="PDFFormPath">Live-Cycle form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Merged Live-Cycle form with data in stream</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Stream(ByVal XDPData As String, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Stream
            Dim formFile As String = PDFFormPath & ""
            If formFile = "" Then
                If FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFGetFile & ""
                End If
            End If

            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    If MemStream.CanSeek Then MemStream.Position = 0
                    Return (MemStream)
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Merges XDP Live-Cycle form with data to stream
        ''' </summary>
        ''' <param name="FDFDocument">FDFDoc_Class object</param>
        ''' <param name="PDFFormPath">Live-Cycle PDF form path or url</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Merged Live-Cycle form with data in stream</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Stream(ByVal FDFDocument As FDFApp.FDFDoc_Class, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Stream
            Dim formFile As String = PDFFormPath & ""
            If formFile = "" Then
                If FDFDocument.FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFDocument.FDFGetFile & ""
                End If
            End If

            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    If MemStream.CanSeek Then MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Stream", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges XDP Live-Cycle form with data to stream
        ''' </summary>
        ''' <param name="FDFDocument">FDFDoc_Class object</param>
        ''' <param name="PDFFormPath">Live-Cycle PDF form path or url</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Merged Live-Cycle form with data in stream</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2Stream(ByVal XDPData As String, ByVal FDFDocument As FDFApp.FDFDoc_Class, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Stream
            Dim formFile As String = PDFFormPath & ""
            If formFile = "" Then
                If FDFDocument.FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFDocument.FDFGetFile & ""
                End If
            End If

            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    If MemStream.CanSeek Then MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2Stream", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Merges XDP Live-Cycle form with data and outputs to a file
        ''' </summary>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="PDFFormPath">Live-Cycle form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>true</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2File(ByVal newPDFFile As String, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Boolean
            Dim formFile As String = PDFFormPath
            If formFile = "" Then
                If FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFGetFile & ""
                End If
            End If
            Dim newFile As String = newPDFFile
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)

                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                stamper = Nothing
                myFileStream.Close()
                myFileStream.Dispose()
                'If Not MemStream Is Nothing Then

                '    Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                '    Try
                '        If MemStream.CanSeek Then
                '            MemStream.Position = 0
                '        End If
                '        'PDFData = MemStream.GetBuffer
                '        With myFileStream
                '            .Write(MemStream.GetBuffer, 0, MemStream.GetBuffer.Length)
                '        End With
                '        MemStream.Close()
                '        MemStream.Dispose()
                '    Catch ex As Exception
                '        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                '    Finally
                '        If Not myFileStream Is Nothing Then
                '            With myFileStream
                '                .Close()
                '                .Dispose()
                '            End With
                '        End If
                '    End Try
                '    Return True
                'Else
                '    Return False
                'End If
                Return True
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2File", 1)
                Return Nothing
            End Try
            Return True

        End Function
        ''' <summary>
        ''' Merges XDP Live-Cycle form with data and outputs to a file
        ''' </summary>
        ''' <param name="XDPData">XDP data to merge</param>
        ''' <param name="newPDFFile">New file path</param>
        ''' <param name="OpenPassword">Open PDF Password</param>
        ''' <param name="ModificationPassword">Modify PDF Password</param>
        ''' <param name="Permissions">PDF Permissions</param>
        ''' <param name="PDFFormPath">Live-Cycle form path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="EncryptionStrength">Encryption strength</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>true</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2File(ByVal XDPData As String, ByVal newPDFFile As String, ByVal OpenPassword As String, ByVal ModificationPassword As String, ByVal Permissions As Integer, Optional ByVal PDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal EncryptionStrength As EncryptionStrength = EncryptionStrength.STRENGTH128BITS, Optional ByVal ownerPassword As String = "") As Boolean
            Dim formFile As String = PDFFormPath
            'http://www.1t3xt.info/examples/browse/?page=example&id=348
            If formFile = "" Then
                If FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFGetFile & ""
                End If
            End If
            Dim newFile As String = newPDFFile
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)

                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                stamper = Nothing
                myFileStream.Close()
                myFileStream.Dispose()
                'If Not MemStream Is Nothing Then

                '    Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                '    Try
                '        If MemStream.CanSeek Then
                '            MemStream.Position = 0
                '        End If
                '        'PDFData = MemStream.GetBuffer
                '        With myFileStream
                '            .Write(MemStream.GetBuffer, 0, MemStream.GetBuffer.Length)
                '        End With
                '        MemStream.Close()
                '        MemStream.Dispose()
                '    Catch ex As Exception
                '        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                '    Finally
                '        If Not myFileStream Is Nothing Then
                '            With myFileStream
                '                .Close()
                '                .Dispose()
                '            End With
                '        End If
                '    End Try
                '    Return True
                'Else
                '    Return False
                'End If
                Return True
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2File", 1)
                Return Nothing
            End Try
            Return True

        End Function
        ''' <summary>
        ''' Merges XDP Live-Cycle form with data and outputs to a file
        ''' </summary>
        ''' <param name="newPDFFile">New PDF Form path</param>
        ''' <param name="OriginalSourcePDFFormPath">Live-Cycle PDF form path or url</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>true</returns>
        ''' <remarks></remarks>
        Public Function PDFMergeXDP2File(ByVal newPDFFile As String, Optional ByVal OriginalSourcePDFFormPath As String = "", Optional ByVal Flatten As Boolean = False, Optional ByVal ownerPassword As String = "") As Boolean
            Dim formFile As String = OriginalSourcePDFFormPath
            If formFile = "" Then
                If FDFGetFile = "" Then
                    Return Nothing
                    Exit Function
                Else
                    formFile = FDFGetFile & ""
                End If
            End If
            Dim newFile As String = newPDFFile
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(formFile)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(formFile, DefaultEncoding.GetBytes(ownerPassword))
            End If
            Dim MemStream As New MemoryStream
            Try
                Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)

                If Not reader Is Nothing Then
                    If RemoveUsageRights Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As iTextSharp.text.pdf.PdfStamper
                If PreserveUsageRights And Flatten = False Then
                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream, "\0", True)
                    Flatten = False
                Else

                    stamper = New iTextSharp.text.pdf.PdfStamper(reader, myFileStream)
                End If
                Dim inputDataElement As Xml.XmlElement = getXFADataElement(FDFSavetoStr(FDFType.XDP, False))
                If inputDataElement Is Nothing Then
                    Return Nothing
                End If
                'stamper.SetEncryption(EncryptionStrength, OpenPassword, ModificationPassword, Permissions)
                Try
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    Else
                        Dim xfaForm As iTextSharp.text.pdf.XfaForm = New iTextSharp.text.pdf.XfaForm(stamper.Reader)
                        If xfaForm.XfaPresent Then
                            If Not xfaForm Is Nothing Then
                                xfaForm.Changed = True
                                iTextSharp.text.pdf.XfaForm.SetXfa(iTextSharp.text.pdf.XfaForm.SerializeDoc(inputDataElement), stamper.Reader, stamper.Writer)
                            End If
                        End If
                    End If
                Catch exMerge As Exception
                    If (stamper.AcroFields.Fields.Count > 0) Then
                        Dim fields As iTextSharp.text.pdf.AcroFields = stamper.AcroFields
                        fields.IsGoingTobeFlattened(Flatten)
                        fields.MergeXfaData(inputDataElement)
                    End If
                End Try
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                stamper = Nothing
                myFileStream.Close()
                myFileStream.Dispose()
                'If Not MemStream Is Nothing Then

                '    Dim myFileStream As New System.IO.FileStream(newPDFFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                '    Try
                '        If MemStream.CanSeek Then
                '            MemStream.Position = 0
                '        End If
                '        'PDFData = MemStream.GetBuffer
                '        With myFileStream
                '            .Write(MemStream.GetBuffer, 0, MemStream.GetBuffer.Length)
                '        End With
                '        MemStream.Close()
                '        MemStream.Dispose()
                '    Catch ex As Exception
                '        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                '    Finally
                '        If Not myFileStream Is Nothing Then
                '            With myFileStream
                '                .Close()
                '                .Dispose()
                '            End With
                '        End If
                '    End Try
                '    Return True
                'Else
                '    Return False
                'End If

                Return True
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFMergeXDP2File", 1)
                Return Nothing
            End Try
            Return True

        End Function

        ''' <summary>
        ''' Flattens PDF form fields and returns the byte array
        ''' </summary>
        ''' <param name="PDFForm">PDF Form in byte array</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with Flattened PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2Buf(ByVal PDFForm As Byte(), Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Flattens PDF form fields and returns the byte array
        ''' </summary>
        ''' <param name="PDFForm">PDF Form URL or Absolute Path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with Flattened PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2Buf(ByVal PDFForm As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Flattens PDF form fields and outputs to a file
        ''' </summary>
        ''' <param name="PDFForm">PDF Form URL or Absolute Path</param>
        ''' <param name="NewPDFPath">Absolute Path for new Flattened PDF Form</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Boolean if Flattened PDF File</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2File(ByVal PDFForm As String, ByVal NewPDFPath As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Boolean
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Or NewPDFPath.Length = 0 Then
                Return False
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    Dim myFileStream As New System.IO.FileStream(NewPDFPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                    Try
                        If MemStream.CanSeek Then
                            MemStream.Position = 0
                        End If
                        'PDFData = MemStream.GetBuffer
                        With myFileStream
                            .Write(MemStream.GetBuffer, 0, MemStream.GetBuffer.Length)
                        End With
                        MemStream.Close()
                        MemStream.Dispose()
                    Catch ex As Exception
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                    Finally
                        If Not myFileStream Is Nothing Then
                            With myFileStream
                                .Close()
                                .Dispose()
                            End With
                        End If
                    End Try
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2File", 1)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Flattens PDF form fields and outputs to a file
        ''' </summary>
        ''' <param name="PDFForm">PDF Form byte array</param>
        ''' <param name="NewPDFPath">Absolute Path for new Flattened PDF Form</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Boolean if Flattened PDF File</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2File(ByVal PDFForm As Byte(), ByVal NewPDFPath As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Boolean
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Or NewPDFPath.Length = 0 Then
                Return False
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    Dim myFileStream As New System.IO.FileStream(NewPDFPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                    Try
                        If MemStream.CanSeek Then
                            MemStream.Position = 0
                        End If
                        With myFileStream
                            .Write(MemStream.GetBuffer, 0, MemStream.GetBuffer.Length)
                        End With
                        MemStream.Close()
                        MemStream.Dispose()
                    Catch ex As Exception
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                    Finally
                        If Not myFileStream Is Nothing Then
                            With myFileStream
                                .Close()
                                .Dispose()
                            End With
                        End If
                    End Try
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2File", 1)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Flattens PDF form fields and returns the stream
        ''' </summary>
        ''' <param name="PDFForm">PDF Form URL or Absolute Path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Stream with Flattened PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2Stream(ByVal PDFForm As Byte(), Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Stream
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'Dim byteRead() As Byte
                    'PDFData = GetUsedBytesOnly(MemStream)
                    'PDFData = byteRead
                    MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Flattens PDF form fields and returns the stream
        ''' </summary>
        ''' <param name="PDFForm">PDF Form URL or Absolute Path</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Stream with Flattened PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2Stream(ByVal PDFForm As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Stream
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2Stream", 1)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Flattens PDF form fields and returns the byte array
        ''' </summary>
        ''' <param name="PDFForm">PDF Form in byte array</param>
        ''' <param name="ExcludedFields">PDF Form Fields to exclude from flattening</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with Flattened PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2Buf(ByVal PDFForm As Byte(), ByVal ExcludedFields() As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                If Not ExcludedFields Is Nothing Then
                    For Each strField As String In ExcludedFields
                        If Not String_IsNullOrEmpty(strField) Then
                            stamper.PartialFormFlattening(strField)
                        End If
                    Next
                End If
                stamper.FormFlattening = Flatten

                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Flattens PDF form fields and returns the byte array
        ''' </summary>
        ''' <param name="PDFForm">PDF Form URL or Absolute Path</param>
        ''' <param name="ExcludedFields">PDF Form Fields to exclude from flattening</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Byte array with Flattened PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2Buf(ByVal PDFForm As String, ByVal ExcludedFields() As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Byte()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                If Not ExcludedFields Is Nothing Then
                    For Each strField As String In ExcludedFields
                        If Not String_IsNullOrEmpty(strField) Then
                            stamper.PartialFormFlattening(strField)
                        End If
                    Next
                End If
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'If MemStream.CanSeek Then MemStream.Position = 0
                    If Flatten Then
                        Return RemoveUsageRights_PDF(GetUsedBytesOnly(MemStream, True))
                    Else
                        Return GetUsedBytesOnly(MemStream, True)
                    End If
                    'MemStream.Close()
                    'MemStream.Dispose()
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2Buf", 1)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Flattens PDF form fields and outputs to a file
        ''' </summary>
        ''' <param name="PDFForm">PDF Form URL or Absolute Path</param>
        ''' <param name="NewPDFPath">Absolute Path for new Flattened PDF Form</param>
        ''' <param name="ExcludedFields">PDF Form Fields to exclude from flattening</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Boolean if Flattened PDF File</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2File(ByVal PDFForm As String, ByVal NewPDFPath As String, ByVal ExcludedFields() As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Boolean
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Or NewPDFPath.Length = 0 Then
                Return False
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                If Not ExcludedFields Is Nothing Then
                    For Each strField As String In ExcludedFields
                        If Not String_IsNullOrEmpty(strField) Then
                            stamper.PartialFormFlattening(strField)
                        End If
                    Next
                End If
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    Dim myFileStream As New System.IO.FileStream(NewPDFPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                    Try
                        If MemStream.CanSeek Then
                            MemStream.Position = 0
                        End If
                        'PDFData = MemStream.GetBuffer
                        With myFileStream
                            .Write(MemStream.GetBuffer, 0, MemStream.GetBuffer.Length)
                        End With
                        MemStream.Close()
                        MemStream.Dispose()
                    Catch ex As Exception
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                    Finally
                        If Not myFileStream Is Nothing Then
                            With myFileStream
                                .Close()
                                .Dispose()
                            End With
                        End If
                    End Try
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2File", 1)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Flattens PDF form fields and outputs to a file
        ''' </summary>
        ''' <param name="PDFForm">PDF Form byte array</param>
        ''' <param name="NewPDFPath">Absolute Path for new Flattened PDF Form</param>
        ''' <param name="ExcludedFields">PDF Form Fields to exclude from flattening</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Boolean if Flattened PDF File</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2File(ByVal PDFForm As Byte(), ByVal NewPDFPath As String, ByVal ExcludedFields() As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Boolean
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Or NewPDFPath.Length = 0 Then
                Return False
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                If Not ExcludedFields Is Nothing Then
                    For Each strField As String In ExcludedFields
                        If Not String_IsNullOrEmpty(strField) Then
                            stamper.PartialFormFlattening(strField)
                        End If
                    Next
                End If
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    Dim myFileStream As New System.IO.FileStream(NewPDFPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                    Try
                        If MemStream.CanSeek Then
                            MemStream.Position = 0
                        End If
                        'PDFData = MemStream.GetBuffer
                        With myFileStream
                            .Write(MemStream.GetBuffer, 0, MemStream.GetBuffer.Length)
                        End With
                        MemStream.Close()
                        MemStream.Dispose()
                    Catch ex As Exception
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FileWrite", 1)
                    Finally
                        If Not myFileStream Is Nothing Then
                            With myFileStream
                                .Close()
                                .Dispose()
                            End With
                        End If
                    End Try
                    Return True
                Else
                    'IOPerm.RevertAssert()
                    'stamper.Close()
                    Return False
                End If
            Catch ex As Exception
                'IOPerm.RevertAssert()
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2File", 1)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Flattens PDF form fields and returns the stream
        ''' </summary>
        ''' <param name="PDFForm">PDF Form URL or Absolute Path</param>
        ''' <param name="ExcludedFields">PDF Form Fields to exclude from flattening</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Stream with Flattened PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2Stream(ByVal PDFForm As Byte(), ByVal ExcludedFields() As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Stream
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                If Not ExcludedFields Is Nothing Then
                    For Each strField As String In ExcludedFields
                        If Not String_IsNullOrEmpty(strField) Then
                            stamper.PartialFormFlattening(strField)
                        End If
                    Next
                End If
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'Dim byteRead() As Byte
                    'PDFData = GetUsedBytesOnly(MemStream)
                    'PDFData = byteRead
                    MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2Buf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Flattens PDF form fields and returns the stream
        ''' </summary>
        ''' <param name="PDFForm">PDF Form URL or Absolute Path</param>
        ''' <param name="ExcludedFields">PDF Form Fields to exclude from flattening</param>
        ''' <param name="Flatten">Flatten</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>Stream with Flattened PDF</returns>
        ''' <remarks></remarks>
        Public Function PDFFlatten2Stream(ByVal PDFForm As String, ByVal ExcludedFields() As String, Optional ByVal Flatten As Boolean = True, Optional ByVal ownerPassword As String = "") As Stream
            Dim reader As iTextSharp.text.pdf.PdfReader
            If PDFForm.Length = 0 Then
                Return Nothing
                Exit Function
            Else
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(PDFForm, DefaultEncoding.GetBytes(ownerPassword))
                End If
            End If
            Dim MemStream As New MemoryStream
            Try
                If Not reader Is Nothing Then
                    If RemoveUsageRights = True Or Flatten = True Then
                        reader.RemoveUsageRights()
                    End If
                End If
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, MemStream)
                If Not ExcludedFields Is Nothing Then
                    For Each strField As String In ExcludedFields
                        If Not String_IsNullOrEmpty(strField) Then
                            stamper.PartialFormFlattening(strField)
                        End If
                    Next
                End If
                stamper.FormFlattening = Flatten
                stamper.Writer.CloseStream = False
                stamper.Close()
                If Not MemStream Is Nothing Then
                    'Dim byteRead() As Byte
                    'PDFData = GetUsedBytesOnly(MemStream)
                    'PDFData = byteRead
                    MemStream.Position = 0
                    Return MemStream
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFDoc.PDFFlatten2Stream", 1)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Get XDPField object array
        ''' </summary>
        ''' <param name="FormName">Optional Form Name</param>
        ''' <param name="FieldNames">Optional Field Name</param>
        ''' <returns>FDFField object array</returns>
        ''' <remarks></remarks>
        Public Function XDPGetFields(Optional ByVal FormName As String = "", Optional ByVal FieldNames As String = "") As FDFField()
            ' Inputs String and Splits it based on semicolin ";"
            Dim xField As FDFField
            Dim FoundField As Boolean
            Dim FieldCount As Integer
            FoundField = False
            Dim _ExportFields(0) As FDFField
            Try
                If String_IsNullOrEmpty(FieldNames & "") Then
                    If Not String_IsNullOrEmpty(FormName) Then
                        For curDoc As Integer = 0 To _FDF.Count - 1
                            If Not _FDF(curDoc).struc_FDFFields.Count <= 0 Then
                                For Each xField In _FDF(curDoc).struc_FDFFields
                                    If Not String_IsNullOrEmpty(xField.FieldName) Then
                                        If _FDF(curDoc).FormName.ToLower = FormName.ToLower Then
                                            ReDim Preserve _ExportFields(FieldCount)
                                            _ExportFields(FieldCount) = xField
                                            FieldCount = FieldCount + 1
                                        End If
                                    End If
                                Next
                            End If
                        Next
                        Return _ExportFields
                        Exit Function
                    Else
                        For curDoc As Integer = 0 To _FDF.Count - 1
                            If Not _FDF(curDoc).struc_FDFFields.Count <= 0 Then
                                For Each xField In _FDF(curDoc).struc_FDFFields
                                    If Not String_IsNullOrEmpty(xField.FieldName) Then
                                        ReDim Preserve _ExportFields(FieldCount)
                                        _ExportFields(FieldCount) = xField
                                        FieldCount = FieldCount + 1
                                    End If
                                Next
                            End If
                        Next
                        Return _ExportFields
                        Exit Function
                    End If
                Else
                    Dim FldNames() As String = FieldNames.Split(";")
                    Dim FldName As String

                    FieldCount = 0
                    For Each FldName In FldNames
                        For curDoc As Integer = 1 To _FDF.Count
                            If Not _FDF(curDoc).struc_FDFFields.Count <= 0 Then
                                For Each xField In _FDF(curDoc).struc_FDFFields
                                    If Not String_IsNullOrEmpty(xField.FieldName) Then
                                        If FldName.ToLower = xField.FieldName.ToLower And FormName = _FDF(curDoc).FormName Then
                                            ReDim Preserve _ExportFields(FieldCount)
                                            _ExportFields(FieldCount) = xField
                                            FieldCount = FieldCount + 1
                                        End If
                                    End If
                                Next
                            End If
                        Next
                    Next
                    Return _ExportFields
                    Exit Function
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.FDFGetFields", 1)
                Return _ExportFields
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Get FDFDoc_Class object array
        ''' </summary>
        ''' <param name="FormNames">Optional Form Names (=Nothing)</param>
        ''' <returns>FDFDoc_Class object array</returns>
        ''' <remarks></remarks>
        Public Function XDPGetForms(Optional ByVal FormNames() As String = Nothing) As FDFApp.FDFDoc_Class.FDFDoc_Class()
            ' Inputs String and Splits it based on semicolin ";"
            Dim FoundField As Boolean
            Dim FormCount As Integer
            FoundField = False
            Dim _ExportForms As New System.Collections.Generic.List(Of FDFApp.FDFDoc_Class.FDFDoc_Class)
            Try
                If Not FormNames Is Nothing Then
                    For curDoc As Integer = 0 To _FDF.Count - 1
                        For Each formname As String In FormNames
                            If Not _FDF(curDoc).struc_FDFFields.Count <= 0 Then
                                If _FDF(curDoc).FormName.ToLower = formname.ToLower Then
                                    'ReDim Preserve _ExportForms(FormCount)
                                    '_ExportForms(FormCount) = _FDF(curDoc)
                                    _ExportForms.Add(_FDF(curDoc))
                                    FormCount += 1
                                End If
                            End If
                        Next
                    Next
                    Return _ExportForms.ToArray
                    Exit Function
                Else
                    For curDoc As Integer = 0 To _FDF.Count - 1
                        If Not _FDF(curDoc).struc_FDFFields.Count <= 0 Then
                            'ReDim Preserve _ExportForms(FormCount)
                            '_ExportForms(FormCount) = _FDF(curDoc)
                            _ExportForms.Add(_FDF(curDoc))
                            FormCount += 1
                        End If
                    Next
                    Return _ExportForms.ToArray
                    Exit Function
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.XDPGetForms", 1)
                Return _ExportForms.ToArray
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Get FDFDoc_Class object
        ''' </summary>
        ''' <param name="FormXMLPath">Optional Form Path (=Nothing)</param>
        ''' <param name="Include_Subforms">Include Subform paths</param>
        ''' <returns>FDFDoc_Class object array</returns>
        ''' <remarks></remarks>
        Public Function XDPGetForm(ByVal FormXMLPath As String, Optional ByVal Include_Subforms As Boolean = False) As FDFApp.FDFDoc_Class.FDFDoc_Class()
            ' Inputs String and Splits it based on semicolin ";"
            Dim FoundField As Boolean
            Dim FormCount As Integer
            FoundField = False
            Dim _ExportForms As New System.Collections.Generic.List(Of FDFApp.FDFDoc_Class.FDFDoc_Class)
            Try
                If Not String_IsNullOrEmpty(FormXMLPath) Then
                    For Each frm As FDFDoc_Class In _FDF
                        'If Not frm.struc_FDFFields.Count <= 0 Then
                        Try
                            If frm.FormLevel.ToLower.Contains(FormXMLPath.ToLower) Then
                                If frm.FormLevel.ToLower = FormXMLPath.ToLower Then
                                    'ReDim Preserve _ExportForms(FormCount)
                                    '_ExportForms(FormCount) = frm
                                    _ExportForms.Add(frm)
                                    FormCount += 1
                                Else
                                    If Include_Subforms = True Then
                                        'ReDim Preserve _ExportForms(FormCount)
                                        '_ExportForms(FormCount) = frm
                                        _ExportForms.Add(frm)
                                        FormCount += 1
                                    End If
                                End If
                            ElseIf frm.FormName.ToLower = FormXMLPath.ToLower And _ExportForms.Count = 0 Then
                                'ReDim Preserve _ExportForms(FormCount)
                                '_ExportForms(FormCount) = frm
                                _ExportForms.Add(frm)
                                FormCount += 1
                                Exit For
                            End If
                        Catch ex As Exception

                            Try
                                If frm.FormName.ToLower = FormXMLPath.ToLower Then
                                    'ReDim Preserve _ExportForms(FormCount)
                                    '_ExportForms(FormCount) = frm
                                    _ExportForms.Add(frm)
                                    FormCount += 1
                                    Err.Clear()
                                    Exit For
                                End If
                            Catch ex2 As Exception
                                Err.Clear()
                            End Try
                            Err.Clear()
                        End Try
                        'End If
                    Next
                    Return _ExportForms.ToArray()
                    Exit Function
                Else
                    For curDoc As Integer = 0 To _FDF.Count - 1
                        If Not _FDF(curDoc).struc_FDFFields.Count <= 0 Then
                            'ReDim Preserve _ExportForms(FormCount)
                            '_ExportForms(FormCount) = _FDF(curDoc)
                            _ExportForms.Add(_FDF(curDoc))
                            FormCount += 1
                        End If
                    Next
                    Return _ExportForms.ToArray
                    Exit Function
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: " & ex.Message, "FDFDoc.XDPGetForms", 1)
                Return _ExportForms.ToArray
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Downloads Restricted File into a Stream
        ''' </summary>
        ''' <param name="PDF_URL">Name of PDF or File to download</param>
        ''' <returns>Stream containing restricted file</returns>
        ''' <remarks></remarks>
        Public Function Download_RestrictedFile(ByVal PDF_URL As String) As Stream
            Dim myCache As New CredentialCache
            Dim myWebClient As System.Net.WebClient
            Dim fs As New MemoryStream
            Try
                myWebClient = New System.Net.WebClient
                Dim bytes() As Byte
                myWebClient.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials
                myWebClient.UseDefaultCredentials = True
                bytes = myWebClient.DownloadData(PDF_URL)
                fs.Write(bytes, 0, bytes.Length)
                fs.Position = 0
                Return fs
            Catch ex As Exception
                Throw New Exception(ex.Message, ex)
            End Try
        End Function
        ''' <summary>
        ''' Downloads Restricted File into a Stream
        ''' </summary>
        ''' <param name="PDF_URL">Name of PDF or File to download</param>
        ''' <param name="Username">Credential Username</param>
        ''' <param name="Password">Credential Password</param>
        ''' <returns>Stream containing restricted file</returns>
        ''' <remarks></remarks>
        Public Function Download_RestrictedFile(ByVal PDF_URL As String, ByVal Username As String, ByVal Password As String) As Stream
            Dim myCache As New CredentialCache
            Dim myWebClient As System.Net.WebClient
            Dim fs As New MemoryStream
            Try
                myWebClient = New System.Net.WebClient
                Dim bytes() As Byte
                Dim creds As New System.Net.NetworkCredential(Username, Password)
                'myWebClient.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials
                myWebClient.Credentials = creds
                myWebClient.UseDefaultCredentials = False
                bytes = myWebClient.DownloadData(PDF_URL)
                fs.Write(bytes, 0, bytes.Length)
                fs.Position = 0
                Return fs
            Catch ex As Exception
                Throw New Exception(ex.Message, ex)
            End Try
        End Function
        ''' <summary>
        ''' Downloads Restricted File into a Stream
        ''' </summary>
        ''' <param name="PDF_URL">Name of PDF or File to download</param>
        ''' <param name="Username">Credential Username</param>
        ''' <param name="Password">Credential Password</param>
        ''' <param name="Domain">Credential Domain</param>
        ''' <returns>Stream containing restricted file</returns>
        ''' <remarks></remarks>
        Public Function Download_RestrictedFile(ByVal PDF_URL As String, ByVal Username As String, ByVal Password As String, ByVal Domain As String) As Stream
            Dim myCache As New CredentialCache
            Dim myWebClient As System.Net.WebClient
            Dim fs As New MemoryStream
            Try
                myWebClient = New System.Net.WebClient
                Dim bytes() As Byte
                Dim creds As New System.Net.NetworkCredential(Username, Password, Domain)
                'myWebClient.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials
                myWebClient.UseDefaultCredentials = False
                myWebClient.Credentials = creds
                bytes = myWebClient.DownloadData(PDF_URL)
                fs.Write(bytes, 0, bytes.Length)
                fs.Position = 0
                Return fs
            Catch ex As Exception
                Throw New Exception(ex.Message, ex)
            End Try
        End Function
        ' ADDED 2008-09-03
        ''' <summary>
        ''' Decrypt Data to String
        ''' </summary>
        ''' <param name="EncryptedFilePath">File Path of encrypted file</param>
        ''' <param name="EncryptionKey">Encryption key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Returns Encryped String</returns>
        ''' <remarks></remarks>
        Public Function FDFDecryptData2String(ByVal EncryptedFilePath As String, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As String
            '-- decrypt this binary file
            Dim decryptedData As Encryption.Data
            Dim sym2 As New Encryption.Symmetric(EncryptionProvider)
            Dim sr As IO.StreamReader = Nothing
            sym2.Key = New Encryption.Data(EncryptionKey)
            Try
                sr = New IO.StreamReader(EncryptedFilePath)
                decryptedData = sym2.Decrypt(sr.BaseStream)
            Finally
                If Not sr Is Nothing Then sr.Close()
            End Try
            Return decryptedData.ToString
        End Function
        ''' <summary>
        ''' Decrypt Data to String
        ''' </summary>
        ''' <param name="EncryptedFile">Byte array of encrypted file</param>
        ''' <param name="EncryptionKey">Encryption key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Returns Encryped String</returns>
        ''' <remarks></remarks>
        Public Function FDFDecryptData2String(ByVal EncryptedFile() As Byte, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As String
            '-- decrypt this binary file
            Dim decryptedData As Encryption.Data
            Dim sym2 As New Encryption.Symmetric(EncryptionProvider)
            Dim sr As IO.MemoryStream = Nothing
            sym2.Key = New Encryption.Data(EncryptionKey)
            Try
                sr = New IO.MemoryStream(EncryptedFile)
                decryptedData = sym2.Decrypt(sr)
            Finally
                If Not sr Is Nothing Then sr.Close()
            End Try
            Return decryptedData.ToString
        End Function
        ''' <summary>
        ''' Decrypt Data to String
        ''' </summary>
        ''' <param name="EncryptedFile">Stream of encrypted file</param>
        ''' <param name="EncryptionKey">Encryption key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Returns Encryped String</returns>
        ''' <remarks></remarks>
        Public Function FDFDecryptData2String(ByVal EncryptedFile As Stream, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As String
            '-- decrypt this binary file
            Dim decryptedData As Encryption.Data
            Dim sym2 As New Encryption.Symmetric(EncryptionProvider)
            Dim sr As IO.StreamReader = Nothing
            sym2.Key = New Encryption.Data(EncryptionKey)
            Try
                sr = New IO.StreamReader(EncryptedFile)
                decryptedData = sym2.Decrypt(sr.BaseStream)
            Finally
                If Not sr Is Nothing Then sr.Close()
            End Try
            Return decryptedData.ToString
        End Function
        ''' <summary>
        ''' Decrypt Data to Bytes
        ''' </summary>
        ''' <param name="EncryptedFilePath">File path of encrypted file</param>
        ''' <param name="EncryptionKey">Encryption key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Returns Encryped Bytes</returns>
        ''' <remarks></remarks>
        Public Function FDFDecryptData2Bytes(ByVal EncryptedFilePath As String, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As Byte()
            '-- decrypt this binary file
            Dim decryptedData As Encryption.Data
            Dim sym2 As New Encryption.Symmetric(EncryptionProvider)
            Dim sr As IO.StreamReader = Nothing
            sym2.Key = New Encryption.Data(EncryptionKey)
            Try
                sr = New IO.StreamReader(EncryptedFilePath)
                decryptedData = sym2.Decrypt(sr.BaseStream)
            Finally
                If Not sr Is Nothing Then sr.Close()
            End Try
            Return decryptedData.Bytes
        End Function
        ''' <summary>
        ''' Decrypt Data to Bytes
        ''' </summary>
        ''' <param name="EncryptedFile">File byte array of encrypted file</param>
        ''' <param name="EncryptionKey">Encryption key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Returns Encryped Bytes</returns>
        ''' <remarks></remarks>
        Public Function FDFDecryptData2Bytes(ByVal EncryptedFile() As Byte, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As Byte()
            '-- decrypt this binary file
            Dim decryptedData As Encryption.Data
            Dim sym2 As New Encryption.Symmetric(EncryptionProvider)
            Dim sr As IO.MemoryStream = Nothing
            sym2.Key = New Encryption.Data(EncryptionKey)
            Try
                sr = New IO.MemoryStream(EncryptedFile)
                decryptedData = sym2.Decrypt(sr)
            Finally
                If Not sr Is Nothing Then sr.Close()
            End Try
            Return decryptedData.Bytes
        End Function
        ''' <summary>
        ''' Decrypt Data to new File
        ''' </summary>
        ''' <param name="FilePath">File path of new decrypted file</param>
        ''' <param name="EncryptedFile">Stream of encrypted file</param>
        ''' <param name="EncryptionKey">Encryption key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Returns True or False</returns>
        ''' <remarks></remarks>
        Public Function FDFDecryptData2File(ByVal FilePath As String, ByVal EncryptedFile As Stream, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As Boolean
            '-- decrypt this binary file
            Dim decryptedData As Encryption.Data
            Dim sym2 As New Encryption.Symmetric(EncryptionProvider)
            Dim sr As IO.StreamReader = Nothing
            sym2.Key = New Encryption.Data(EncryptionKey)
            Try
                sr = New IO.StreamReader(EncryptedFile)
                decryptedData = sym2.Decrypt(sr.BaseStream)
            Finally
                If Not sr Is Nothing Then sr.Close()
            End Try
            Dim fs As New FileStream(FilePath, FileMode.Create)
            Try
                fs.Write(decryptedData.Bytes, 0, decryptedData.Bytes.Length)
            Catch ex As Exception
                fs.Close()
                Return False
            Finally
                fs.Close()
            End Try
            Return True
        End Function
        ''' <summary>
        ''' Decrypt Data to byte array
        ''' </summary>
        ''' <param name="FilePath">File path of new decrypted file</param>
        ''' <param name="EncryptedFilePath">File path of encrypted file</param>
        ''' <param name="EncryptionKey">Encryption key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Returns True or False</returns>
        ''' <remarks></remarks>
        Public Function FDFDecryptData2File(ByVal FilePath As String, ByVal EncryptedFilePath As String, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As Boolean
            '-- decrypt this binary file
            Dim decryptedData As Encryption.Data
            Dim sym2 As New Encryption.Symmetric(EncryptionProvider)
            Dim sr As IO.StreamReader = Nothing
            sym2.Key = New Encryption.Data(EncryptionKey)
            Try
                sr = New IO.StreamReader(EncryptedFilePath)
                decryptedData = sym2.Decrypt(sr.BaseStream)
            Finally
                If Not sr Is Nothing Then sr.Close()
            End Try
            Dim fs As New FileStream(FilePath, FileMode.Create)
            Try
                fs.Write(decryptedData.Bytes, 0, decryptedData.Bytes.Length)
            Catch ex As Exception
                fs.Close()
                Return False
            Finally
                fs.Close()
            End Try
            Return True
        End Function
        ''' <summary>
        ''' Decrypt Data to byte array
        ''' </summary>
        ''' <param name="FilePath">File path of new decrypted file</param>
        ''' <param name="EncryptedFile">Byte array of encrypted file</param>
        ''' <param name="EncryptionKey">Encryption key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Returns True or False</returns>
        ''' <remarks></remarks>
        Public Function FDFDecryptData2File(ByVal FilePath As String, ByVal EncryptedFile() As Byte, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As Boolean
            '-- decrypt this binary file
            Dim decryptedData As Encryption.Data
            Dim sym2 As New Encryption.Symmetric(EncryptionProvider)
            Dim sr As IO.MemoryStream = Nothing
            sym2.Key = New Encryption.Data(EncryptionKey)
            Try
                sr = New IO.MemoryStream(EncryptedFile)
                decryptedData = sym2.Decrypt(sr)
            Finally
                If Not sr Is Nothing Then sr.Close()
            End Try
            Dim fs As New FileStream(FilePath, FileMode.Create)
            Try
                fs.Write(decryptedData.Bytes, 0, decryptedData.Bytes.Length)
            Catch ex As Exception
                fs.Close()
                Return False
            Finally
                fs.Close()
            End Try
            Return True
        End Function
        ''' <summary>
        ''' Decrypt Data to byte array
        ''' </summary>
        ''' <param name="EncryptedFile">Byte array of encrypted file</param>
        ''' <param name="EncryptionKey">Encryption key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Returns True or False</returns>
        ''' <remarks></remarks>
        Public Function FDFDecryptData2Bytes(ByVal EncryptedFile As Stream, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As Byte()
            '-- decrypt this binary file
            Dim decryptedData As Encryption.Data
            Dim sym2 As New Encryption.Symmetric(EncryptionProvider)
            Dim sr As IO.StreamReader = Nothing
            sym2.Key = New Encryption.Data(EncryptionKey)
            Try
                sr = New IO.StreamReader(EncryptedFile)
                decryptedData = sym2.Decrypt(sr.BaseStream)
            Finally
                If Not sr Is Nothing Then sr.Close()
            End Try
            Return decryptedData.Bytes
        End Function
        ''' <summary>
        ''' Encrypt Data to String
        ''' </summary>
        ''' <param name="FileType">FDF/XML/XDP/xFDF</param>
        ''' <param name="EncryptionKey">Encryption Key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>String containing encrypted data</returns>
        ''' <remarks></remarks>
        Public Function FDFEncryptData2String(ByVal FileType As FDFType, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As String
            '-- Encrypt this binary file
            Dim clsEncrypt As New Encryption.Asymmetric()
            Dim sym As New Encryption.Symmetric(EncryptionProvider)
            sym.Key = New Encryption.Data(EncryptionKey)
            Dim encryptedData As New Encryption.Data
            encryptedData = sym.Encrypt(FDFSavetoStream(FileType, True))
            Return encryptedData.ToString
        End Function
        ''' <summary>
        ''' Encrypt Data to String
        ''' </summary>
        ''' <param name="FileType">FDF/XML/XDP/xFDF</param>
        ''' <param name="EncryptionKey">Encryption Key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>Byte array containing encrypted data</returns>
        ''' <remarks></remarks>
        Public Function FDFEncryptData2Byte(ByVal FileType As FDFType, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As Byte()
            '-- Encrypt this binary file
            Dim clsEncrypt As New Encryption.Asymmetric()
            Dim sym As New Encryption.Symmetric(EncryptionProvider)
            sym.Key = New Encryption.Data(EncryptionKey)
            Dim encryptedData As New Encryption.Data
            encryptedData = sym.Encrypt(FDFSavetoStream(FileType, True))
            Return encryptedData.Bytes
        End Function
        ''' <summary>
        ''' Encrypt Data to String
        ''' </summary>
        ''' <param name="FileType">FDF/XML/XDP/xFDF</param>
        ''' <param name="EncryptionKey">Encryption Key - String</param>
        ''' <param name="EncryptionProvider">Provider - Encryption Method</param>
        ''' <returns>True or False</returns>
        ''' <remarks></remarks>
        Public Function FDFEncryptData2File(ByVal FilePath As String, ByVal FileType As FDFType, ByVal EncryptionKey As String, Optional ByVal EncryptionProvider As Encryption.Symmetric.Provider = Encryption.Symmetric.Provider.Rijndael) As Boolean
            '-- Encrypt this binary file
            Dim clsEncrypt As New Encryption.Asymmetric()
            Dim sym As New Encryption.Symmetric(EncryptionProvider)
            sym.Key = New Encryption.Data(EncryptionKey)
            Dim encryptedData As New Encryption.Data
            encryptedData = sym.Encrypt(FDFSavetoStream(FileType, True))
            Dim fs As New FileStream(FilePath, FileMode.Create)
            Try
                fs.Write(encryptedData.Bytes, 0, encryptedData.Bytes.Length)
            Catch ex As Exception
                fs.Close()
                Return False
            Finally
                fs.Close()
            End Try
        End Function
        ''' <summary>
        ''' Save byte array to file
        ''' </summary>
        ''' <param name="FilePath">New File Path</param>
        ''' <param name="FileBytes">File Bytes to write to the file</param>
        ''' <remarks></remarks>
        Public Sub SaveBytesToFile(ByVal FilePath As String, ByVal FileBytes() As Byte)
            Dim fs As New FileStream(FilePath, FileMode.Create)
            Try
                fs.Write(FileBytes, 0, FileBytes.Length)
            Catch ex As Exception
                fs.Close()
            Finally
                fs.Close()
            End Try
        End Sub
        ''' <summary>
        ''' Read bytes from a file
        ''' </summary>
        ''' <param name="FilePath">Path of file to read</param>
        ''' <returns>Byte array containing file contents</returns>
        ''' <remarks></remarks>
        Public Function ReadFileToBytes(ByVal FilePath As String) As Byte()
            Dim fs As New FileStream(FilePath, FileMode.Open)
            Dim bytes(fs.Length) As Byte
            Try
                fs.Read(bytes, 0, fs.Length)
            Catch ex As Exception
                fs.Close()
                Return bytes
            Finally
                fs.Close()
            End Try
            Return bytes
        End Function
        ''' <summary>
        ''' Read bytes from a stream
        ''' </summary>
        ''' <param name="fs">Stream</param>
        ''' <returns>Byte array containing stream contents</returns>
        ''' <remarks></remarks>
        Public Function ReadStreamToBytes(ByVal fs As Stream) As Byte()
            Dim bytes(fs.Length) As Byte
            Try
                fs.Read(bytes, 0, fs.Length)
            Catch ex As Exception
                fs.Close()
                Return bytes
            Finally
                fs.Close()
            End Try
            Return bytes
        End Function
        ' END ADDED 2008-09-03
#Region "ADDED 2009-09-14 - USAGE RIGHTS"
        Private _removeUsageRights As Boolean = False
        Public Property RemoveUsageRights() As Boolean
            Get
                Return _removeUsageRights
            End Get
            Set(ByVal value As Boolean)
                _removeUsageRights = value
            End Set
        End Property
        ' ADDED NK-INC 2010-10-29 @ 9:07PM
        Private _preserveUsageRights As Boolean = False
        Public Property PreserveUsageRights() As Boolean
            Get
                Return _preserveUsageRights
            End Get
            Set(ByVal value As Boolean)
                _preserveUsageRights = value
            End Set
        End Property
        ''' <summary>
        ''' Removes Usage Rights from an iTextSharp PDFReader Object
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function RemoveUsageRights_Reader(ByVal reader As iTextSharp.text.pdf.PdfReader) As iTextSharp.text.pdf.PdfReader
            Try
                If Not reader Is Nothing Then
                    reader.RemoveUsageRights()
                End If
            Catch ex As Exception

            End Try
            Return reader
        End Function
        ''' <summary>
        ''' Removes Usage Rights from a PDF
        ''' </summary>
        ''' <param name="PDFFile">Absolute File Path to PDF</param>
        ''' <returns>Returns the PDF without usage rights enabled</returns>
        ''' <remarks></remarks>
        Public Function RemoveUsageRights_PDF(ByVal PDFFile As String) As Byte()
            Try
                Dim reader As New iTextSharp.text.pdf.PdfReader(PDFFile)
                If Not reader Is Nothing Then
                    reader.RemoveUsageRights()
                End If
                Dim strOut As New MemoryStream
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, strOut)
                stamper.Writer.CloseStream = False
                stamper.Close()
                Return strOut.GetBuffer
            Catch ex As Exception
                Try

                    Dim reader As New iTextSharp.text.pdf.PdfReader(PDFFile)
                    Dim strOut As New MemoryStream
                    Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, strOut)
                    stamper.Writer.CloseStream = False
                    stamper.Close()
                    Return strOut.GetBuffer
                Catch ex2 As Exception

                End Try
            End Try
            Return Nothing
        End Function
        ''' <summary>
        ''' Removes Usage Rights from a PDF
        ''' </summary>
        ''' <param name="PDFFile">PDF File Byte Array</param>
        ''' <returns>Returns the PDF without usage rights enabled</returns>
        ''' <remarks></remarks>
        Public Function RemoveUsageRights_PDF(ByVal PDFFile As Byte()) As Byte()
            Dim _tempBytes() As Byte = PDFFile
            Try
                Dim reader As New iTextSharp.text.pdf.PdfReader(PDFFile)
                If Not reader Is Nothing Then
                    reader.RemoveUsageRights()
                End If
                Dim strOut As New MemoryStream
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, strOut)
                stamper.Writer.CloseStream = False
                stamper.Close()
                Return strOut.GetBuffer
            Catch ex As Exception
                Return _tempBytes
            End Try
            Return _tempBytes
        End Function
        ''' <summary>
        ''' Removes Usage Rights from a PDF
        ''' </summary>
        ''' <param name="PDFFile">PDF File Stream Object</param>
        ''' <returns>Returns the PDF without usage rights enabled</returns>
        ''' <remarks></remarks>
        Public Function RemoveUsageRights_PDF(ByVal PDFFile As Stream) As Byte()
            Dim _tempBytes(PDFFile.Length) As Byte
            Try
                If PDFFile.CanSeek Then
                    PDFFile.Position = 0
                End If
                PDFFile.Read(_tempBytes, 0, PDFFile.Length)
                Dim reader As New iTextSharp.text.pdf.PdfReader(_tempBytes)
                If Not reader Is Nothing Then
                    reader.RemoveUsageRights()
                End If
                Dim strOut As New MemoryStream
                Dim stamper As New iTextSharp.text.pdf.PdfStamper(reader, strOut)
                stamper.Writer.CloseStream = False
                stamper.Close()
                Return strOut.GetBuffer
            Catch ex As Exception
                Return _tempBytes
            End Try
            Return _tempBytes
        End Function
        ''' <summary>
        ''' Checks Usage Rights from a PDF
        ''' </summary>
        ''' <param name="PDFFile">Absolute File Name</param>
        ''' <returns>Returns true if PDF has usage rights enabled</returns>
        ''' <remarks></remarks>
        Public Function HasUsageRights_PDF(ByVal PDFFile As String) As Boolean
            Dim reader As New iTextSharp.text.pdf.PdfReader(PDFFile)
            Return reader.HasUsageRights()
        End Function
        ''' <summary>
        ''' Checks Usage Rights from a PDF
        ''' </summary>
        ''' <param name="PDFFile">PDF File Stream from a File Stream or Memory Stream</param>
        ''' <returns>Returns true if PDF has usage rights enabled</returns>
        ''' <remarks></remarks>
        Public Function HasUsageRights_PDF(ByVal PDFFile As Stream) As Boolean
            Dim reader As New iTextSharp.text.pdf.PdfReader(PDFFile)
            Return reader.HasUsageRights()
        End Function
        ''' <summary>
        ''' Checks Usage Rights from a PDF
        ''' </summary>
        ''' <param name="PDFFile">PDF Byte Array</param>
        ''' <returns>Returns true if PDF has usage rights enabled</returns>
        ''' <remarks></remarks>
        Public Function HasUsageRights_PDF(ByVal PDFFile As Byte()) As Boolean
            Dim reader As New iTextSharp.text.pdf.PdfReader(PDFFile)
            Return reader.HasUsageRights()
        End Function
#End Region

#Region "Added 2009-01-30"
        ''' <summary>
        ''' IMAGE TYPE
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum ImageFieldMime
            JPG
            PNG
            GIF
            BMP
            EMF
        End Enum
        Private Function XDP_IMAGE_MIME_TYPES(ByVal ImageMime As ImageFieldMime) As String
            Select Case ImageMime
                Case ImageFieldMime.JPG
                    Return "image/jpg"
                Case ImageFieldMime.GIF
                    Return "image/gif"
                Case ImageFieldMime.PNG
                    Return "image/png"
                Case ImageFieldMime.BMP
                    Return "image/bmp"
                Case ImageFieldMime.EMF
                    Return "image/x-emf"
                Case Else
                    Return ""
            End Select
        End Function
        Private Function XDP_IMAGE_MIME_TYPES(ByVal ImageMime As String) As ImageFieldMime
            Select Case ImageMime.ToLower
                Case "image/jpeg"
                    'Return ImageFieldMime.JPG
                    Return ImageFieldMime.JPG
                Case "image/jpg"
                    'Return ImageFieldMime.JPG
                    Return ImageFieldMime.JPG
                Case "image/png"
                    'Return ImageFieldMime.PNG
                    Return ImageFieldMime.PNG
                Case "image/gif"
                    'Return ImageFieldMime.GIF
                    Return ImageFieldMime.GIF
                Case "image/bmp"
                    'Return ImageFieldMime.BMP
                    Return ImageFieldMime.BMP
                Case "image/x-emf"
                    'Return ImageFieldMime.EMF
                    Return ImageFieldMime.EMF
                Case Else
                    Return ImageFieldMime.JPG
            End Select
        End Function
        Private Function XDP_FILE_IMAGE_MIME_TYPES(ByVal ImageName As String) As ImageFieldMime
            If ImageName.EndsWith("jpg") Then
                Return ImageFieldMime.JPG
            ElseIf ImageName.EndsWith("jpeg") Then
                Return ImageFieldMime.JPG
            ElseIf ImageName.EndsWith("png") Then
                Return ImageFieldMime.PNG
            ElseIf ImageName.EndsWith("gif") Then
                Return ImageFieldMime.GIF
            ElseIf ImageName.EndsWith("bmp") Then
                Return ImageFieldMime.BMP
            ElseIf ImageName.EndsWith("emf") Then
                Return ImageFieldMime.EMF
            Else
                Return ImageFieldMime.JPG
            End If
        End Function
        ''' <summary>
        ''' ADD IMAGE TO XDP DATA
        ''' </summary>
        ''' <param name="FieldName">Image Field Name</param>
        ''' <param name="FormName">Form Name (Ex: subform1)</param>
        ''' <param name="ImageMIME">Image Type</param>
        ''' <param name="ImageFieldBytes">Image File</param>
        ''' <param name="ReplaceField">Replace field</param>
        ''' <remarks></remarks>
        Public Sub XDP_Add_ImageField(ByVal FieldName As String, ByVal FormName As String, ByVal ImageMIME As ImageFieldMime, ByVal ImageFieldBytes() As Byte, Optional ByVal ReplaceField As Boolean = False)
            Dim fldNumber As Integer = 0
            Try
                Dim TmpCurFDFDoc As Integer = 0
                Dim XDPFDF As Integer = 0
                If _FDF.Count > 0 Then
                    For XDPFDF = 0 To _FDF.Count - 1
                        If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                            If FormName.ToLower = _FDF(XDPFDF).FormName.ToLower Then
                                _CurFDFDoc = XDPFDF
                                Exit For
                            End If
                        End If
                    Next
                Else
                    Exit Sub
                End If
                Dim fldName As String = FieldName
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                'FieldValue = Me.XDPCheckChar(FieldValue)
                If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                    If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                        Dim blnFound As Boolean = False
                        If ReplaceField = True Then
                            If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                                If _FDF(XDPFDF).struc_FDFFields.Count > 0 Then
                                    For Each xField In _FDF(XDPFDF).struc_FDFFields
                                        If Not String_IsNullOrEmpty(xField.FieldName) Then
                                            If FieldName.ToLower = xField.FieldName.ToLower Then
                                                'xField.FieldValue = New String() {Me.XDPCheckChar(FieldValue)}
                                                xField.ImageBase64 = ConvertToBase64String(ImageFieldBytes)
                                                xField.FieldEnabled = True
                                                xField.FieldNum = fldNumber
                                                xField.FieldType = FieldType.FldLiveCycleImage
                                                xField.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                                blnFound = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        If blnFound = True Then
                            Exit Sub
                        Else
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType.FldLiveCycleImage
                            _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                            _fld.FieldEnabled = True
                            _fld.ImageBase64 = ConvertToBase64String(ImageFieldBytes)
                            _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                        End If
                    ElseIf Not FieldName = "" Then
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldNum = fldNumber
                        _fld.FieldType = FieldType.FldLiveCycleImage
                        _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                        _fld.FieldEnabled = True
                        _fld.ImageBase64 = ConvertToBase64String(ImageFieldBytes)
                        _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                    End If
                End If
                Dim x As Integer
                x = 0
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.XDP_Add_ImageField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' ADD IMAGE TO XDP DATA
        ''' </summary>
        ''' <param name="FieldName">Image Field Name</param>
        ''' <param name="FormName">Form Name (Ex: subform1)</param>
        ''' <param name="ImageMIME">Image Type</param>
        ''' <param name="ImageFieldStringBase64">Image File</param>
        ''' <param name="ReplaceField">Replace field</param>
        ''' <remarks></remarks>
        Public Sub XDP_Add_ImageField(ByVal FieldName As String, ByVal FormName As String, ByVal ImageMIME As ImageFieldMime, ByVal ImageFieldStringBase64 As String, Optional ByVal ReplaceField As Boolean = False)
            Dim fldNumber As Integer = 0
            Try
                Dim TmpCurFDFDoc As Integer = 0
                Dim XDPFDF As Integer = 0
                If _FDF.Count > 0 Then
                    For XDPFDF = 0 To _FDF.Count - 1
                        If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                            If FormName.ToLower = _FDF(XDPFDF).FormName.ToLower Then
                                _CurFDFDoc = XDPFDF
                                Exit For
                            End If
                        End If
                    Next
                Else
                    Exit Sub
                End If
                Dim fldName As String = FieldName
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                    If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                        Dim blnFound As Boolean = False
                        If ReplaceField = True Then
                            If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                                If _FDF(XDPFDF).struc_FDFFields.Count > 0 Then
                                    For Each xField In _FDF(XDPFDF).struc_FDFFields
                                        If Not String_IsNullOrEmpty(xField.FieldName) Then
                                            If FieldName.ToLower = xField.FieldName.ToLower Then
                                                'xField.FieldValue = New String() {Me.XDPCheckChar(FieldValue)}
                                                xField.ImageBase64 = ImageFieldStringBase64
                                                xField.FieldEnabled = True
                                                xField.FieldNum = fldNumber
                                                xField.FieldType = FieldType.FldLiveCycleImage
                                                xField.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                                blnFound = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        If blnFound = True Then
                            Exit Sub
                        Else
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType.FldLiveCycleImage
                            _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                            _fld.FieldEnabled = True
                            _fld.ImageBase64 = ImageFieldStringBase64
                            _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                        End If
                    ElseIf Not FieldName = "" Then
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldNum = fldNumber
                        _fld.FieldType = FieldType.FldLiveCycleImage
                        _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                        _fld.FieldEnabled = True
                        _fld.ImageBase64 = ImageFieldStringBase64
                        _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                    End If
                End If
                Dim x As Integer
                x = 0
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.XDP_Add_ImageField", 1)
            End Try
        End Sub

        ''' <summary>
        ''' ADD IMAGE TO XDP DATA
        ''' </summary>
        ''' <param name="FieldName">Image Field Name</param>
        ''' <param name="FormName">Form Name (Ex: subform1)</param>
        ''' <param name="ImageMIME">Image Type</param>
        ''' <param name="ImageFieldBitmap">System.Drawing.BitMap Image File</param>
        ''' <param name="ReplaceField">Replace field</param>
        ''' <remarks></remarks>
        Public Sub XDP_Add_ImageField(ByVal FieldName As String, ByVal FormName As String, ByVal ImageMIME As ImageFieldMime, ByVal ImageFieldBitmap As System.Drawing.Image, Optional ByVal ReplaceField As Boolean = False)
            Dim fldNumber As Integer = 0
            Try
                Dim ImageFieldStringBase64 As String = ""
                Select Case ImageMIME
                    'Case "image/jpg"
                    Case ImageFieldMime.JPG
                        ImageMIME = ImageFieldMime.JPG
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Jpeg)
                    Case ImageFieldMime.PNG
                        'Case "image/png"
                        ImageMIME = ImageFieldMime.PNG
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Png)
                    Case ImageFieldMime.GIF
                        'Case "image/gif"
                        ImageMIME = ImageFieldMime.GIF
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Gif)
                    Case ImageFieldMime.BMP
                        'Case "image/bmp"
                        ImageMIME = ImageFieldMime.BMP
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Bmp)
                    Case ImageFieldMime.EMF
                        'Case "image/x-emf"
                        ImageMIME = ImageFieldMime.EMF
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Emf)
                End Select
                Dim TmpCurFDFDoc As Integer = 0
                Dim XDPFDF As Integer = 0
                If _FDF.Count > 0 Then
                    For XDPFDF = 0 To _FDF.Count - 1
                        If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                            If FormName.ToLower = _FDF(XDPFDF).FormName.ToLower Then
                                _CurFDFDoc = XDPFDF
                                Exit For
                            End If
                        End If
                    Next
                Else
                    Exit Sub
                End If
                Dim fldName As String = FieldName
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                'FieldValue = Me.XDPCheckChar(FieldValue)
                If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                    If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                        Dim blnFound As Boolean = False
                        If ReplaceField = True Then
                            If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                                If _FDF(XDPFDF).struc_FDFFields.Count > 0 Then
                                    For Each xField In _FDF(XDPFDF).struc_FDFFields
                                        If Not String_IsNullOrEmpty(xField.FieldName) Then
                                            If FieldName.ToLower = xField.FieldName.ToLower Then
                                                'xField.FieldValue = New String() {Me.XDPCheckChar(FieldValue)}
                                                xField.FieldName = FieldName
                                                xField.ImageBase64 = ImageFieldStringBase64
                                                xField.FieldEnabled = True
                                                xField.FieldNum = fldNumber
                                                xField.FieldType = FieldType.FldLiveCycleImage
                                                xField.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                                blnFound = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        If blnFound = True Then
                            Exit Sub
                        Else
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType.FldLiveCycleImage
                            _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                            _fld.FieldEnabled = True
                            _fld.ImageBase64 = ImageFieldStringBase64
                            _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                        End If
                    ElseIf Not FieldName = "" Then
                        ' If Not FDFGetValue(FieldName) Is Nothing And ReplaceField = False Then Exit Sub
                        'ReDim _FDF(XDPFDF).struc_FDFFields(0)
                        'ReDim _FDF(XDPFDF).struc_FDFFields(0).FieldValue(0)
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldNum = fldNumber
                        _fld.FieldType = FieldType.FldLiveCycleImage
                        _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                        _fld.FieldEnabled = True
                        _fld.ImageBase64 = ImageFieldStringBase64
                        _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                    End If
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.XDP_Add_ImageField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' ADD IMAGE TO XDP DATA
        ''' </summary>
        ''' <param name="FieldName">Image Field Name</param>
        ''' <param name="FormName">Form Name (Ex: subform1)</param>
        ''' <param name="ImageUrlOrAbsolutePath">Image File URL or Absolute Path</param>
        ''' <param name="ReplaceField">Replace field</param>
        ''' <remarks></remarks>
        Public Sub XDP_Add_ImageField(ByVal FieldName As String, ByVal FormName As String, ByVal ImageUrlOrAbsolutePath As String, Optional ByVal ReplaceField As Boolean = False)
            Dim fldNumber As Integer = 0
            Try
                Dim ImageFieldStringBase64 As String = "", ImageMIME As ImageFieldMime
                'Select Case ImageUrlOrAbsolutePath.Substring(ImageUrlOrAbsolutePath.LastIndexOf(".") + 1, ImageUrlOrAbsolutePath.Length - ImageUrlOrAbsolutePath.LastIndexOf(".") + 1).ToLower
                Select Case XDP_FILE_IMAGE_MIME_TYPES(ImageUrlOrAbsolutePath)
                    'Case "image/jpg"
                    Case 0
                        ImageMIME = ImageFieldMime.JPG
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case 1
                        'Case "image/png"
                        ImageMIME = ImageFieldMime.PNG
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case 2
                        'Case "image/gif"
                        ImageMIME = ImageFieldMime.GIF
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case 3
                        'Case "image/bmp"
                        ImageMIME = ImageFieldMime.BMP
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case 4
                        'Case "image/x-emf"
                        ImageMIME = ImageFieldMime.EMF
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case Else
                        Exit Sub
                End Select
                Dim TmpCurFDFDoc As Integer = 0
                Dim XDPFDF As Integer = 0
                If _FDF.Count > 0 Then
                    For XDPFDF = 0 To _FDF.Count - 1
                        If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                            If FormName.ToLower = _FDF(XDPFDF).FormName.ToLower Then
                                _CurFDFDoc = XDPFDF
                                Exit For
                            End If
                        End If
                    Next
                Else
                    Exit Sub
                End If
                Dim fldName As String = FieldName
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                'FieldValue = Me.XDPCheckChar(FieldValue)
                If Not String_IsNullOrEmpty(_FDF(XDPFDF).FormName) Then
                    If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                        Dim blnFound As Boolean = False
                        If ReplaceField = True Then
                            If Not _FDF(XDPFDF).struc_FDFFields.Count <= 0 Then
                                If _FDF(XDPFDF).struc_FDFFields.Count > 0 Then
                                    For Each xField In _FDF(XDPFDF).struc_FDFFields
                                        If Not String_IsNullOrEmpty(xField.FieldName) Then
                                            If FieldName.ToLower = xField.FieldName.ToLower Then
                                                'xField.FieldValue = New String() {Me.XDPCheckChar(FieldValue)}
                                                xField.FieldName = FieldName
                                                xField.ImageBase64 = ImageFieldStringBase64
                                                xField.FieldEnabled = True
                                                xField.FieldNum = fldNumber
                                                xField.FieldType = FieldType.FldLiveCycleImage
                                                xField.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                                blnFound = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        If blnFound = True Then
                            Exit Sub
                        Else
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType.FldLiveCycleImage
                            _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                            _fld.FieldEnabled = True
                            _fld.ImageBase64 = ImageFieldStringBase64
                            _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                        End If
                    ElseIf Not FieldName = "" Then
                        Dim _fld As New FDFField
                        _fld.FieldName = FieldName
                        _fld.FieldNum = fldNumber
                        _fld.FieldType = FieldType.FldLiveCycleImage
                        _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                        _fld.FieldEnabled = True
                        _fld.ImageBase64 = ImageFieldStringBase64
                        _FDF(XDPFDF).struc_FDFFields.Add(_fld)
                    End If
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.XDP_Add_ImageField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' Open Image to buffer (byte array)
        ''' </summary>
        ''' <param name="ImagePathOrUrl">Image File URL or Absolute Path</param>
        ''' <param name="EncodeToBase64">Option to encode it to Base64 (compatible with XDP data)</param>
        ''' <returns>File in Byte Array</returns>
        ''' <remarks></remarks>
        Public Function XDP_OpenImageToBuf(ByVal ImagePathOrUrl As String, Optional ByVal EncodeToBase64 As Boolean = True) As Byte()
            Try
                If IsValidUrl(ImagePathOrUrl) Then
                    Dim client As New WebClient
                    'Dim input As New BinaryReader(client.OpenRead(ImagePathOrUrl), _defaultEncoding)
                    'ImageString = input.ReadToEnd
                    Dim ImageData() As Byte = client.DownloadData(ImagePathOrUrl)
                    'FDFData = _defaultEncoding.GetString(bytes)
                    'Dim ImageData(Input.BaseStream.Length) As Byte
                    'input.Read(ImageData, 0, ImageData.Length)
                    'input.Close()
                    If EncodeToBase64 = True Then
                        Return _defaultEncoding.GetBytes(Convert.ToBase64String(ImageData))
                    Else
                        Return ImageData
                    End If
                ElseIf File.Exists(ImagePathOrUrl) Then
                    Dim input As New FileStream(ImagePathOrUrl, FileMode.Open, FileAccess.Read)
                    Dim ImageData(input.Length) As Byte
                    input.Read(ImageData, 0, ImageData.Length)
                    input.Close()
                    If EncodeToBase64 = True Then
                        Return _defaultEncoding.GetBytes(Convert.ToBase64String(ImageData))
                    Else
                        Return ImageData
                    End If
                Else
                    Return Nothing
                    Exit Function
                End If
            Catch ex As Exception
                Return Nothing
            End Try
            Return Nothing
        End Function
        ''' <summary>
        ''' Open Image to Base64 (string)
        ''' </summary>
        ''' <param name="ImagePathOrUrl">Image File URL or Absolute Path</param>
        ''' <returns>File in Base64 String format</returns>
        ''' <remarks></remarks>
        Public Function XDP_OpenImageToBase64String(ByVal ImagePathOrUrl As String) As String
            Try
                If IsValidUrl(ImagePathOrUrl) Then
                    Dim client As New WebClient
                    Dim ImageData() As Byte = client.DownloadData(ImagePathOrUrl)
                    'Dim input As New BinaryReader(client.OpenRead(ImagePathOrUrl), _defaultEncoding)
                    'ImageString = input.ReadToEnd
                    'Dim ImageData(input.BaseStream.Length) As Byte
                    'input.Read(ImageData, 0, ImageData.Length)
                    'input.Close()
                    Return Convert.ToBase64String(ImageData)
                ElseIf File.Exists(ImagePathOrUrl) Then
                    Dim input As New FileStream(ImagePathOrUrl, FileMode.Open, FileAccess.Read)
                    Dim ImageData(input.Length) As Byte
                    input.Read(ImageData, 0, ImageData.Length)
                    input.Close()
                    Return Convert.ToBase64String(ImageData)
                Else
                    Return Nothing
                    Exit Function
                End If
            Catch ex As Exception
                Return Nothing
            End Try
            Return Nothing
        End Function
        ''' <summary>
        ''' Converts a byte array to base64 string
        ''' </summary>
        ''' <param name="Filebytes">File bytes array</param>
        ''' <returns>Base64 String</returns>
        ''' <remarks></remarks>
        Public Function ConvertToBase64String(ByVal Filebytes As Byte()) As String
            Dim strModified As String = ""
            strModified = Convert.ToBase64String(Filebytes)
            Return strModified
        End Function
        ''' <summary>
        ''' Converts an System.Drawing.Image to Base64 string
        ''' </summary>
        ''' <param name="image">System.Drawing.Image</param>
        ''' <param name="newFormat">System.Drawing.Imaging.ImageFormat (JPG/GIF/PNG/BMP/EMF only)</param>
        ''' <returns>Base64 string of Image</returns>
        ''' <remarks></remarks>
        Public Function ConvertToBase64String(ByVal image As System.Drawing.Image, ByVal newFormat As System.Drawing.Imaging.ImageFormat) As String
            Dim strModified As String = ""
            Dim imgStream As New MemoryStream
            image.Save(imgStream, newFormat)
            If imgStream.CanSeek Then
                imgStream.Position = 0
            End If
            Dim imgBytes(imgStream.Length) As Byte
            imgStream.Read(imgBytes, 0, imgStream.Length)
            strModified = Convert.ToBase64String(imgBytes)
            Return strModified
        End Function
        ''' <summary>
        ''' Converts an System.Drawing.Image to Base64 string
        ''' </summary>
        ''' <param name="image">System.Drawing.Image</param>
        ''' <returns>Base64 string of Image</returns>
        ''' <remarks></remarks>
        Public Function ConvertToBase64String(ByVal image As System.Drawing.Image) As String
            Dim strModified As String = ""
            Dim imgStream As New MemoryStream
            image.Save(imgStream, image.RawFormat)
            If imgStream.CanSeek Then
                imgStream.Position = 0
            End If
            Dim imgBytes(imgStream.Length) As Byte
            imgStream.Read(imgBytes, 0, imgStream.Length)
            strModified = Convert.ToBase64String(imgBytes)
            Return strModified
        End Function
        ''' <summary>
        ''' Converts an Byte Array to Base64 string
        ''' </summary>
        ''' <param name="FileBytes">System.Drawing.Image</param>
        ''' <returns>Base64 string</returns>
        ''' <remarks></remarks>
        Public Function ConvertToBase64Byte(ByVal FileBytes As Byte()) As Byte()
            Dim bytModified() As Byte, strModified As String
            strModified = Convert.ToBase64String(FileBytes)
            bytModified = _defaultEncoding.GetBytes(strModified)
            Return bytModified
        End Function
        ''' <summary>
        ''' Converts a Base64 encoded string to a default enocoded string
        ''' </summary>
        ''' <param name="strEncodedBase64">Base64 encoded string to convert</param>
        ''' <param name="ToEncoding">To specific encoding (UTF-8)</param>
        ''' <returns>Encoded String</returns>
        ''' <remarks></remarks>
        Public Function ConvertFromBase64ToString(ByVal strEncodedBase64 As String, ByVal ToEncoding As System.Text.Encoding) As String
            Dim b As Byte() = Convert.FromBase64String(strEncodedBase64)
            Dim strEncoded As String = ToEncoding.GetString(b)
            Return strEncoded
        End Function
        ''' <summary>
        ''' Converts a Base64 encoded string to a default enocoded byte array
        ''' </summary>
        ''' <param name="strEncodedBase64">Base64 encoded string to convert</param>
        ''' <param name="ToEncoding">To specific encoding (UTF-8)</param>
        ''' <returns>Encoded Byte Array</returns>
        ''' <remarks></remarks>
        Public Function ConvertFromBase64ToByte(ByVal strEncodedBase64 As String, ByVal ToEncoding As System.Text.Encoding) As Byte()
            Dim b As Byte() = Convert.FromBase64String(strEncodedBase64)
            Dim bytEncoded As Byte() = System.Text.Encoding.Convert(System.Text.UTF8Encoding.UTF8, ToEncoding, b)
            Return bytEncoded
        End Function
        ' END ADDED 2009-01-29

#End Region
#Region "ADDED 2009-01-31"
        ''' <summary>
        ''' Gets Image Base64 value of Live-Cycle form image field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="xdpFormName">Live-Cycle Form Name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function XDP_GetImageBase64(ByVal FieldName As String, ByVal xdpFormName As String, Optional ByVal CaseSensitive As Boolean = False) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            xdpFrm = XDPForm(xdpFormName)

            Try
                If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                    If xdpFrm.struc_FDFFields.Count >= 1 Then
                        For Each xField In xdpFrm.struc_FDFFields
                            If Not String_IsNullOrEmpty(xField.FieldName) And xField.FieldType = FieldType.FldLiveCycleImage Then
                                If CaseSensitive = True Then
                                    If xField.FieldName & "" = FieldName Then
                                        Return (xField.ImageBase64 & "")
                                        Exit Function
                                    End If
                                Else
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        Return (xField.ImageBase64 & "")
                                        Exit Function
                                    End If
                                End If
                            End If
                        Next
                    Else
                        Return Nothing
                        Exit Function
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetImageBase64", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetImageBase64", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets Image Base64 value of Live-Cycle form image field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' ''' <param name="FieldNumber">Field Number</param>
        ''' <param name="xdpFormName">Live-Cycle Form Name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function XDP_GetImageBase64(ByVal FieldName As String, ByVal FieldNumber As Integer, ByVal xdpFormName As String, Optional ByVal CaseSensitive As Boolean = False) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            xdpFrm = XDPForm(xdpFormName)

            Try
                If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                    If xdpFrm.struc_FDFFields.Count >= 1 Then
                        For Each xField In xdpFrm.struc_FDFFields
                            If Not String_IsNullOrEmpty(xField.FieldName) And xField.FieldType = FieldType.FldLiveCycleImage Then
                                If CaseSensitive = True Then
                                    If xField.FieldName & "" = FieldName Then
                                        If xField.FieldNum = FieldNumber Then
                                            Return (xField.ImageBase64 & "")
                                            Exit Function
                                        End If

                                    End If
                                Else
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        If xField.FieldNum = FieldNumber Then
                                            Return (xField.ImageBase64 & "")
                                            Exit Function
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Else
                        Return Nothing
                        Exit Function
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetImageBase64", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetImageBase64", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets Image Base64 value of Live-Cycle form image field
        ''' </summary>
        ''' <param name="FieldName">Live-Cycle form field name</param>
        ''' <param name="xdpFormNumber">Live-Cycle form name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field value</returns>
        ''' <remarks></remarks>
        Public Function XDP_GetImageBase64(ByVal FieldName As String, ByVal xdpFormNumber As Integer, Optional ByVal CaseSensitive As Boolean = False) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            xdpFrm = XDPForm(xdpFormNumber)

            Try
                If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                    If xdpFrm.struc_FDFFields.Count >= 1 Then
                        For Each xField In xdpFrm.struc_FDFFields
                            If Not String_IsNullOrEmpty(xField.FieldName) And xField.FieldType = FieldType.FldLiveCycleImage Then
                                If CaseSensitive = True Then
                                    If xField.FieldName & "" = FieldName Then
                                        Return (xField.ImageBase64 & "")
                                        Exit Function
                                    End If
                                Else
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        Return (xField.ImageBase64 & "")
                                        Exit Function
                                    End If
                                End If
                            End If
                        Next
                    Else
                        Return Nothing
                        Exit Function
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetImageBase64", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetImageBase64", 1)
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets Image Base64 value of Live-Cycle form image field, in any Live-Cycle form
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field value</returns>
        ''' <remarks></remarks>
        Public Function XDP_GetImageBase64(ByVal FieldName As String, Optional ByVal CaseSensitive As Boolean = False) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            Try
                For Each xdpFrm In _FDF
                    If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                        If xdpFrm.struc_FDFFields.Count >= 1 Then
                            For Each xField In xdpFrm.struc_FDFFields
                                If Not String_IsNullOrEmpty(xField.FieldName) And xField.FieldType = FieldType.FldLiveCycleImage Then
                                    If CaseSensitive = True Then
                                        If xField.FieldName & "" = FieldName Then
                                            Return (xField.ImageBase64 & "")
                                            Exit Function
                                        End If
                                    Else
                                        If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                            Return (xField.ImageBase64 & "")
                                            Exit Function
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetImageBase64", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetImageBase64", 1)
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets MIME value of FDF Field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="CaseSensitive">If true must match case</param>
        ''' <returns>Field value</returns>
        ''' <remarks></remarks>
        Public Function XDP_GetImageMime(ByVal FieldName As String, Optional ByVal CaseSensitive As Boolean = False) As ImageFieldMime
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Try
                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                    For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                        If CaseSensitive = True Then
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If xField.FieldName & "" = FieldName Then
                                    Return XDP_IMAGE_MIME_TYPES((xField.FieldValue(0) & ""))
                                    Exit Function
                                End If
                            End If
                        Else
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                    Return XDP_IMAGE_MIME_TYPES((xField.FieldValue(0) & ""))
                                    Exit Function
                                End If
                            End If
                        End If
                    Next
                End If
                Return Nothing
                Exit Function
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets MIME value of FDF Field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <param name="xdpFormName">Live-Cycle Form Name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function XDP_GetImageMime(ByVal FieldName As String, ByVal xdpFormName As String, Optional ByVal CaseSensitive As Boolean = False) As ImageFieldMime
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            xdpFrm = XDPForm(xdpFormName)

            Try
                If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                    If xdpFrm.struc_FDFFields.Count >= 1 Then
                        For Each xField In xdpFrm.struc_FDFFields
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If CaseSensitive = True Then
                                    If xField.FieldName & "" = FieldName Then
                                        Return XDP_IMAGE_MIME_TYPES((xField.FieldValue(0) & ""))
                                        Exit Function
                                    End If
                                Else
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        Return XDP_IMAGE_MIME_TYPES((xField.FieldValue(0) & ""))
                                        Exit Function
                                    End If
                                End If
                            End If
                        Next
                    Else
                        Return Nothing
                        Exit Function
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets MIME value of FDF Field
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' ''' <param name="FieldNumber">Field Number</param>
        ''' <param name="xdpFormName">Live-Cycle Form Name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function XDP_GetImageMime(ByVal FieldName As String, ByVal FieldNumber As Integer, ByVal xdpFormName As String, Optional ByVal CaseSensitive As Boolean = False) As ImageFieldMime
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            xdpFrm = XDPForm(xdpFormName)

            Try
                If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                    If xdpFrm.struc_FDFFields.Count >= 1 Then
                        For Each xField In xdpFrm.struc_FDFFields
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If CaseSensitive = True Then
                                    If xField.FieldName & "" = FieldName Then
                                        If xField.FieldNum = FieldNumber Then
                                            Return XDP_IMAGE_MIME_TYPES((xField.FieldValue(0) & ""))
                                            Exit Function
                                        End If

                                    End If
                                Else
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        If xField.FieldNum = FieldNumber Then
                                            Return XDP_IMAGE_MIME_TYPES((xField.FieldValue(0) & ""))
                                            Exit Function
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Else
                        Return Nothing
                        Exit Function
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets MIME value of FDF Field
        ''' </summary>
        ''' <param name="FieldName">Live-Cycle form field name</param>
        ''' <param name="xdpFormNumber">Live-Cycle form name</param>
        ''' <param name="CaseSensitive">If true, must match case</param>
        ''' <returns>Field value</returns>
        ''' <remarks></remarks>
        Public Function XDP_GetImageMime(ByVal FieldName As String, ByVal xdpFormNumber As Integer, Optional ByVal CaseSensitive As Boolean = False) As ImageFieldMime
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            xdpFrm = XDPForm(xdpFormNumber)

            Try
                If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                    If xdpFrm.struc_FDFFields.Count >= 1 Then
                        For Each xField In xdpFrm.struc_FDFFields
                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                If CaseSensitive = True Then
                                    If xField.FieldName & "" = FieldName Then
                                        Return XDP_IMAGE_MIME_TYPES((xField.FieldValue(0) & ""))
                                        Exit Function
                                    End If
                                Else
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        Return XDP_IMAGE_MIME_TYPES((xField.FieldValue(0) & ""))
                                        Exit Function
                                    End If
                                End If
                            End If
                        Next
                    Else
                        Return Nothing
                        Exit Function
                    End If
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function

#End Region
#Region "ADDED 2009-02-01"
        ''' <summary>
        ''' ADD IMAGE TO XDP DATA
        ''' </summary>
        ''' <param name="FieldName">Image Field Name</param>
        ''' <param name="ImageMIME">Image Type</param>
        ''' <param name="ImageFieldBytes">Image File</param>
        ''' <param name="ReplaceField">Replace field</param>
        ''' <remarks></remarks>
        Public Sub XDP_Add_ImageField(ByVal FieldName As String, ByVal ImageMIME As ImageFieldMime, ByVal ImageFieldBytes() As Byte, Optional ByVal ReplaceField As Boolean = False)
            Dim fldNumber As Integer = 0
            Try
                Dim TmpCurFDFDoc As Integer = 0
                'Dim _CurFDFDoc As Integer = 0
                Dim fldName As String = FieldName
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName)  Then
                    If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then

                        If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                            Dim blnFound As Boolean = False
                            If ReplaceField = True Then
                                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                                    If _FDF(_CurFDFDoc).struc_FDFFields.Count > 0 Then
                                        For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                                If FieldName.ToLower = xField.FieldName.ToLower Then
                                                    'xField.FieldValue = New String() {Me.XDPCheckChar(FieldValue)}
                                                    xField.ImageBase64 = ConvertToBase64String(ImageFieldBytes)
                                                    xField.FieldEnabled = True
                                                    xField.FieldNum = fldNumber
                                                    xField.FieldType = FieldType.FldLiveCycleImage
                                                    xField.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                                    blnFound = True
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                            If blnFound = True Then
                                Exit Sub
                            Else
                                Dim _fld As New FDFField
                                _fld.FieldName = FieldName
                                _fld.FieldNum = fldNumber
                                _fld.FieldType = FieldType.FldLiveCycleImage
                                _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                _fld.FieldEnabled = True
                                _fld.ImageBase64 = ConvertToBase64String(ImageFieldBytes)
                                _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                            End If
                        ElseIf Not FieldName = "" Then
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType.FldLiveCycleImage
                            _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                            _fld.FieldEnabled = True
                            _fld.ImageBase64 = ConvertToBase64String(ImageFieldBytes)
                            _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                        End If
                    Else
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: XDP Form Needed", "FDFDoc.XDP_Add_ImageField", 1)
                    End If
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: XDP Form Needed", "FDFDoc.XDP_Add_ImageField", 1)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.XDP_Add_ImageField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' ADD IMAGE TO XDP DATA
        ''' </summary>
        ''' <param name="FieldName">Image Field Name</param>
        ''' <param name="ImageMIME">Image Type</param>
        ''' <param name="ImageFieldStringBase64">Image File</param>
        ''' <param name="ReplaceField">Replace field</param>
        ''' <remarks></remarks>
        Public Sub XDP_Add_ImageField(ByVal FieldName As String, ByVal ImageMIME As ImageFieldMime, ByVal ImageFieldStringBase64 As String, Optional ByVal ReplaceField As Boolean = False)
            Dim fldNumber As Integer = 0
            Try
                Dim TmpCurFDFDoc As Integer = 0
                Dim fldName As String = FieldName
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                'FieldValue = Me.XDPCheckChar(FieldValue)
                If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName)  Then
                    If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then
                        If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                            Dim blnFound As Boolean = False
                            If ReplaceField = True Then
                                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                                    If _FDF(_CurFDFDoc).struc_FDFFields.Count > 0 Then
                                        For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                                If FieldName.ToLower = xField.FieldName.ToLower Then
                                                    'xField.FieldValue = New String() {Me.XDPCheckChar(FieldValue)}
                                                    xField.ImageBase64 = ImageFieldStringBase64
                                                    xField.FieldEnabled = True
                                                    xField.FieldNum = fldNumber
                                                    xField.FieldType = FieldType.FldLiveCycleImage
                                                    xField.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                                    blnFound = True
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                            If blnFound = True Then
                                Exit Sub
                            Else
                                Dim _fld As New FDFField
                                _fld.FieldName = FieldName
                                _fld.FieldNum = fldNumber
                                _fld.FieldType = FieldType.FldLiveCycleImage
                                _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                _fld.FieldEnabled = True
                                _fld.ImageBase64 = ImageFieldStringBase64
                                _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)

                            End If
                        ElseIf Not FieldName = "" Then
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType.FldLiveCycleImage
                            _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                            _fld.FieldEnabled = True
                            _fld.ImageBase64 = ImageFieldStringBase64
                            _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                        End If
                    Else
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: XDP Form Needed", "FDFDoc.XDP_Add_ImageField", 1)
                    End If
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: XDP Form Needed", "FDFDoc.XDP_Add_ImageField", 1)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.XDP_Add_ImageField", 1)
            End Try
        End Sub

        ''' <summary>
        ''' ADD IMAGE TO XDP DATA
        ''' </summary>
        ''' <param name="FieldName">Image Field Name</param>
        ''' <param name="ImageMIME">Image Type</param>
        ''' <param name="ImageFieldBitmap">System.Drawing.BitMap Image File</param>
        ''' <param name="ReplaceField">Replace field</param>
        ''' <remarks></remarks>
        Public Sub XDP_Add_ImageField(ByVal FieldName As String, ByVal ImageMIME As ImageFieldMime, ByVal ImageFieldBitmap As System.Drawing.Image, Optional ByVal ReplaceField As Boolean = False)
            Dim fldNumber As Integer = 0
            Try
                Dim ImageFieldStringBase64 As String = ""
                Select Case ImageMIME
                    'Case "image/jpg"
                    Case ImageFieldMime.JPG
                        ImageMIME = ImageFieldMime.JPG
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Jpeg)
                    Case ImageFieldMime.PNG
                        'Case "image/png"
                        ImageMIME = ImageFieldMime.PNG
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Png)
                    Case ImageFieldMime.GIF
                        'Case "image/gif"
                        ImageMIME = ImageFieldMime.GIF
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Gif)
                    Case ImageFieldMime.BMP
                        'Case "image/bmp"
                        ImageMIME = ImageFieldMime.BMP
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Bmp)
                    Case ImageFieldMime.EMF
                        'Case "image/x-emf"
                        ImageMIME = ImageFieldMime.EMF
                        ImageFieldStringBase64 = ConvertToBase64String(ImageFieldBitmap, System.Drawing.Imaging.ImageFormat.Emf)
                End Select
                Dim TmpCurFDFDoc As Integer = 0
                Dim fldName As String = FieldName
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                'FieldValue = Me.XDPCheckChar(FieldValue)
                If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName)  Then
                    If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then
                        If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                            Dim blnFound As Boolean = False
                            If ReplaceField = True Then
                                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                                    If _FDF(_CurFDFDoc).struc_FDFFields.Count > 0 Then
                                        For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                                If FieldName.ToLower = xField.FieldName.ToLower Then
                                                    'xField.FieldValue = New String() {Me.XDPCheckChar(FieldValue)}
                                                    xField.FieldName = FieldName
                                                    xField.ImageBase64 = ImageFieldStringBase64
                                                    xField.FieldEnabled = True
                                                    xField.FieldNum = fldNumber
                                                    xField.FieldType = FieldType.FldLiveCycleImage
                                                    xField.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                                    blnFound = True
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                            If blnFound = True Then
                                Exit Sub
                            Else
                                Dim _fld As New FDFField
                                _fld.FieldName = FieldName
                                _fld.FieldNum = fldNumber
                                _fld.FieldType = FieldType.FldLiveCycleImage
                                _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                _fld.FieldEnabled = True
                                _fld.ImageBase64 = ImageFieldStringBase64
                                _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                            End If
                        ElseIf Not FieldName = "" Then
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType.FldLiveCycleImage
                            _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                            _fld.FieldEnabled = True
                            _fld.ImageBase64 = ImageFieldStringBase64
                            _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                        End If
                    Else
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: XDP Form Needed", "FDFDoc.XDP_Add_ImageField", 1)
                    End If
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: XDP Form Needed", "FDFDoc.XDP_Add_ImageField", 1)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.XDP_Add_ImageField", 1)
            End Try
        End Sub
        ''' <summary>
        ''' ADD IMAGE TO XDP DATA
        ''' </summary>
        ''' <param name="FieldName">Image Field Name</param>
        ''' <param name="ImageUrlOrAbsolutePath">Image File URL or Absolute Path</param>
        ''' <param name="ReplaceField">Replace field</param>
        ''' <remarks></remarks>
        Public Sub XDP_Add_ImageField(ByVal FieldName As String, ByVal ImageUrlOrAbsolutePath As String, Optional ByVal ReplaceField As Boolean = False)
            Dim fldNumber As Integer = 0
            Try
                Dim ImageFieldStringBase64 As String = "", ImageMIME As ImageFieldMime
                'Select Case ImageUrlOrAbsolutePath.Substring(ImageUrlOrAbsolutePath.LastIndexOf(".") + 1, ImageUrlOrAbsolutePath.Length - ImageUrlOrAbsolutePath.LastIndexOf(".") + 1).ToLower
                Select Case XDP_FILE_IMAGE_MIME_TYPES(ImageUrlOrAbsolutePath)
                    'Case "image/jpg"
                    Case 0
                        ImageMIME = ImageFieldMime.JPG
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case 1
                        'Case "image/png"
                        ImageMIME = ImageFieldMime.PNG
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case 2
                        'Case "image/gif"
                        ImageMIME = ImageFieldMime.GIF
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case 3
                        'Case "image/bmp"
                        ImageMIME = ImageFieldMime.BMP
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case 4
                        'Case "image/x-emf"
                        ImageMIME = ImageFieldMime.EMF
                        ImageFieldStringBase64 = XDP_OpenImageToBase64String(ImageUrlOrAbsolutePath)
                    Case Else
                        Exit Sub
                End Select
                Dim TmpCurFDFDoc As Integer = 0
                Dim fldName As String = FieldName
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        fldNumber = FieldName.Substring(int, FieldName.LastIndexOf("]") - int)
                    End If
                Catch ex As Exception
                    fldNumber = 0
                End Try
                Try
                    If FieldName.LastIndexOf("[") > 0 Then
                        Dim int As Integer = FieldName.LastIndexOf("[") + 1
                        FieldName = FieldName.Substring(0, FieldName.LastIndexOf("["))
                    End If
                Catch ex As Exception
                    FieldName = fldName
                End Try
                'FieldValue = Me.XDPCheckChar(FieldValue)
                If Not String_IsNullOrEmpty(_FDF(_CurFDFDoc).FormName)  Then
                    If _FDF(_CurFDFDoc).DocType = FDFDocType.XDPForm Then
                        If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                            Dim blnFound As Boolean = False
                            If ReplaceField = True Then
                                If Not _FDF(_CurFDFDoc).struc_FDFFields.Count <= 0 Then
                                    If _FDF(_CurFDFDoc).struc_FDFFields.Count > 0 Then
                                        For Each xField In _FDF(_CurFDFDoc).struc_FDFFields
                                            If Not String_IsNullOrEmpty(xField.FieldName) Then
                                                If FieldName.ToLower = xField.FieldName.ToLower Then
                                                    'xField.FieldValue = New String() {Me.XDPCheckChar(FieldValue)}
                                                    xField.FieldName = FieldName
                                                    xField.ImageBase64 = ImageFieldStringBase64
                                                    xField.FieldEnabled = True
                                                    xField.FieldNum = fldNumber
                                                    xField.FieldType = FieldType.FldLiveCycleImage
                                                    xField.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                                    blnFound = True
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                            If blnFound = True Then
                                Exit Sub
                            Else
                                Dim _fld As New FDFField
                                _fld.FieldName = FieldName
                                _fld.FieldNum = fldNumber
                                _fld.FieldType = FieldType.FldLiveCycleImage
                                _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                                _fld.FieldEnabled = True
                                _fld.ImageBase64 = ImageFieldStringBase64
                                _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                            End If
                        ElseIf Not FieldName = "" Then
                            Dim _fld As New FDFField
                            _fld.FieldName = FieldName
                            _fld.FieldNum = fldNumber
                            _fld.FieldType = FieldType.FldLiveCycleImage
                            _fld.FieldValue.Add(XDP_IMAGE_MIME_TYPES(ImageMIME))
                            _fld.FieldEnabled = True
                            _fld.ImageBase64 = ImageFieldStringBase64
                            _FDF(_CurFDFDoc).struc_FDFFields.Add(_fld)
                        End If
                    Else
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: XDP Form Needed", "FDFDoc.XDP_Add_ImageField", 1)
                    End If
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: XDP Form Needed", "FDFDoc.XDP_Add_ImageField", 1)
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.XDP_Add_ImageField", 1)
            End Try
        End Sub
#End Region
#Region "ADDED 2010-10-16"
        ''' <summary>
        ''' Gets value of Live-Cycle form field, in any Live-Cycle form
        ''' </summary>
        ''' <param name="FieldName">Field name</param>
        ''' <returns>Field value</returns>
        ''' <remarks></remarks>
        Public Function XDPGetValue(ByVal FieldName As String) As String
            Dim xField As FDFField
            Dim FoundField As Boolean
            FoundField = False
            Dim xdpFrm As New FDFDoc_Class
            Try
                For Each xdpFrm In _FDF
                    If Not xdpFrm.struc_FDFFields.Count <= 0 Then
                        If xdpFrm.struc_FDFFields.Count >= 1 Then
                            For Each xField In xdpFrm.struc_FDFFields
                                If Not String_IsNullOrEmpty(xField.FieldName) Then
                                    If LCase(xField.FieldName) & "" = LCase(FieldName) Then
                                        Return Me.XDPCheckCharReverse(xField.FieldValue(0) & "")
                                        Exit Function
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFDoc.FDFGetValue", 1)
                Return Nothing
                Exit Function
            End Try
            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFieldNotFound, "Error: Field Not Found.", "FDFDoc.FDFGetValue", 1)
            Return Nothing
        End Function
#End Region
#Region "STRING FUNCTIONS"
        ' ADDED 2009/01/30
        ' Return 0 if object is null, else decimal value
        Protected Function Decimal_IsNullOrEmpty(ByVal o As Object) As Decimal
            If IsDBNull(o) Then
                Return True
            Else
                Return False
                'Return CType(o, Decimal)
            End If
        End Function

        ' Return 0 if null, else integer value of object.
        Protected Function Integer_IsNullOrEmpty(ByVal i As Object) As Integer
            If IsDBNull(i) Then
                Return True
            Else
                Return False
                'Return CType(i, Integer)
            End If
        End Function

        ' Return String if object is not null, else return empty.string
        Protected Function String_IsNullOrEmpty(ByVal s As Object) As Boolean
            If IsDBNull(s) Then
                Return True
            Else
                If String.Empty = s & "" Then
                    Return True
                Else
                    Return False
                End If
            End If
        End Function
        Protected Function SNE(ByVal s As Object) As String
            If IsDBNull(s) Then
                Return String.Empty
            Else
                If String.Empty = s & "" Then
                    Return String.Empty
                Else
                    Return CType(s, String)
                End If
            End If
        End Function
#End Region
#Region " IDisposable Support "
        Private disposedValue As Boolean = False                  ' To detect redundant calls
        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Try
                        _CurFDFDoc = Nothing
                        _FDFObjects = Nothing
                        _FDF.Clear()
                        _FDF = Nothing
                        _PDF = Nothing
                        _FDFErrors.Dispose()
                        _FDFErrors = Nothing
                    Catch ex As Exception

                    End Try

                End If
            End If
            Me.disposedValue = True
        End Sub
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        ''' <summary>
        ''' Disposes of managed objects
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        ''' <summary>
        ''' Disposes of managed objects (Calls Dispose())
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub FDFClose()
            Dispose(True)
            Exit Sub
            'GC.Collect(GC.GetGeneration(_FDF))
        End Sub
#End Region

#Region "FULL VERSION - COMMENT FOR DEMO ONLY"
        Public Sub New()
            Initialize()
        End Sub
        Public Sub Initialize()
            Try
                _CurFDFDoc = 0
                _FDF = New List(Of FDFDoc_Class)
                _PDF = New PDFDoc
                _FDF.Add(New FDFDoc_Class())
                PreserveUsageRights = True
                ResetErrors()
            Catch ex As Exception

            End Try

		End Sub

#End Region

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class

End Namespace