﻿Imports System
Imports System.IO

''' <summary>
''' Controlador de errores y logs del sistema
''' </summary>
''' <remarks></remarks>
Public Class Ermac

    ''' <summary>
    ''' Path donde se va encuentra o se va a crear el archivo de log
    ''' </summary>
    ''' <value>Cadena con la ruta y nombre de archivo log</value>
    ''' <returns>La ruta completa con nombre de archivo log</returns>
    ''' <remarks></remarks>
    Public Property LogFilePath As String = My.Computer.FileSystem.SpecialDirectories.Temp & "\" & "syslog.log"


    ''' <summary>
    ''' Propiedad de visiblidad del archivo en el sistema operativo
    ''' </summary>
    ''' <value>Booleano indicando el estado. True: el archivo esta oculto, False: el archivo esta visible</value>
    ''' <returns>La </returns>
    ''' <remarks></remarks>
    Public Property isHidden As Boolean = True

    ''' <summary>
    ''' Nivel de detalle de logueo
    ''' </summary>
    ''' <value>El nivel de logueo</value>
    ''' <returns>Nivel de logueo actual</returns>
    ''' <remarks>0: Disable, 1: estandar, 2:debug</remarks>
    Public Property LogLevel As Byte = 1

    ''' <summary>
    ''' Timestamp de cuando ocurrio el problema
    ''' </summary>
    ''' <value>El tiempo y la hora del error</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Timestamp As Date = Now

    ''' <summary>
    ''' Nombre del subsistema donde ocurrio el error
    ''' </summary>
    ''' <value>Cadena con el nombre del subsistema donde ocurrio el error</value>
    ''' <returns>El nombre del subsistema</returns>
    ''' <remarks></remarks>
    Public Property SubSystem As String = ""

    ''' <summary>
    ''' Nombre del modulo donde ocurrio el error
    ''' </summary>
    ''' <value>Cadena con el nombre del modulo donde fallo</value>
    ''' <returns>El nombre del modulo donde ocurrio el fallo</returns>
    ''' <remarks></remarks>
    Public Property ModuleName As String = ""

    ''' <summary>
    ''' Descripcion del error
    ''' </summary>
    ''' <value>Cadena con la descripcion del error</value>
    ''' <returns>La descripcion del error</returns>
    ''' <remarks></remarks>
    Public Property Description As String = ""

    ''' <summary>
    ''' Codigos de error
    ''' </summary>
    ''' <value>Entero con codigo del error</value>
    ''' <returns>El codigo de error</returns>
    ''' <remarks></remarks>
    Public Property Code As Integer = 0

    ''' <summary>
    ''' Nivel de error
    ''' </summary>
    ''' <value>El nivel de error</value>
    ''' <returns>0: bajo, 1: medio, 2: grave, 3: catastrofico</returns>
    ''' <remarks></remarks>
    Public Property ErrorLevel As Byte = 0

    Public Sub New()
        MyBase.New()
        Try
            _LogLevel = 2
        Catch ex As Exception
            _LogLevel = 1
            Console.WriteLine(ex.Message)
        End Try
    End Sub


    Public Sub SetError(ByVal ex As Exception, ByVal subSystem As String, ByVal functionName As String, Optional ByVal debugMessage As String = "")
        ' TODO: Corregir niveles de rror
        _Timestamp = Now
        _SubSystem = subSystem
        _ModuleName = functionName
        If ex.Message.Length = 0 Then
            _Description = debugMessage
        Else
            _Description = ex.Message
        End If
        _Code = 1
        _ErrorLevel = 0
        Save()
    End Sub

    ''' <summary>
    ''' Guarda una entrada de error en el log
    ''' </summary>
    ''' <returns>True si se pudo crear el archivo y/o guardar la entrada, False si fallo</returns>
    ''' <remarks></remarks>
    Private Function Save() As Boolean
        If My.Computer.FileSystem.FileExists(_LogFilePath) Then
            Dim lineString As String

            lineString = _Timestamp.ToString & vbTab
            lineString &= "Level:" & _ErrorLevel & vbTab & vbTab
            lineString &= "Code:" & _Code & vbTab & vbTab
            lineString &= _SubSystem & vbTab & vbTab

            If _LogLevel = 2 Then
                lineString &= "FUNC: " & _ModuleName & vbTab & vbTab
                lineString &= _Description
            End If

            If _LogLevel <> 0 Then
                Using sw As StreamWriter = File.AppendText(_LogFilePath)
                    sw.WriteLine(lineString)
                End Using
            End If
            Return True
        Else
            CreateLogFile()
            Save()
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Crea un nuevo archivo de log
    ''' </summary>
    ''' <returns>True si se pudo crear, False si fallo</returns>
    ''' <remarks></remarks>
    Private Function CreateLogFile() As Boolean
        If Not My.Computer.FileSystem.FileExists(_LogFilePath) Then
            Try
                Using sw As StreamWriter = File.AppendText(_LogFilePath)
                    sw.WriteLine("LOG CREATED: " & Now)
                    If _isHidden = True Then
                        File.SetAttributes(_LogFilePath, FileAttributes.Hidden)
                    End If
                    Return True
                End Using
            Catch ex As Exception
                Console.WriteLine(ex)
                Return False
            End Try
        End If
        Return False
    End Function


End Class
