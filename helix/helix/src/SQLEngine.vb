﻿Option Explicit On
Option Strict On

Imports System
Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.OleDb


Public Class SQLEngine

    '<--! INICIO CONSTANTES -->
    ''' <summary>
    ''' Cadena constante para el proveedor de db MS Access
    ''' </summary>
    Private Const _AccessProvider As String = "Provider=Microsoft.Jet.OLEDB.4.0;"

    ''' <summary>
    ''' Constante con el final de la cadena de conexion a MS Access
    ''' </summary>
    Private Const _AccessConnTrailer As String = "Persist Security Info=False;"

    ''' <summary>
    ''' Cadena constante para el proveedor de db SQL Server
    ''' </summary>
    Private Const _SQLServerProvider As String = "Provider=SQLOLEDB;"

    ''' <summary>
    ''' Flag de la cadena de conexion que indica que se van a pasar nombre de usuario y contraseña como credenciales
    ''' </summary>
    Private Const _SQLServerAuthentication As String = "Integrated Security=False;"

    ''' <summary>
    ''' Constante con el final de la cadena de conexion a SQL Server
    ''' </summary>
    Private Const _SQLServerConnTrailer As String = "Connect Timeout=30;Encrypt=False;TrustServerCertificate=False"

    '<--! FIN CONSTANTES -->



    '<--! INICIO VARIABLES -->

    ''' <summary>
    ''' Clase encargada de la eliminacion de registros
    ''' </summary>
    Public Delete As New SQLEngineDelete

    ''' <summary>
    ''' Clase encargada de la creacion de registros nuevos
    ''' </summary>
    Public Insert As New SQLEngineInsert

    ''' <summary>
    ''' Clase encargada de las actualizaciones de registros
    ''' </summary>
    Public Update As New SQLEngineUpdate

    ''' <summary>
    ''' Clase encargada de extraer registros de la base de datos
    ''' </summary>
    Public Query As New SQLEngineQuery

    ''' <summary>
    ''' Clase encargada del manejo de la base de datos (creacion, mantenimiento y borrado de tablas)
    ''' </summary>
    ''' <remarks></remarks>
    Public Db As New SQLEngineDB

    ''' <summary>
    ''' Clase intermedia creadora de base de datos, tablas y usuarios
    ''' </summary>
    ''' <remarks></remarks>
    Public Builder As New SQLEngineBuilder

    ''' <summary>
    ''' Ruta completa y nombre de archivo donde se van a guardar los logs
    ''' </summary>
    ''' <value>Cadena con la ruta completa y el nombre de archivo del log</value>
    ''' <returns>La ruta y el nombre del archivo log</returns>
    ''' <remarks></remarks>
    Public Property LogFileFullName As String = My.Computer.FileSystem.SpecialDirectories.Temp & "\" & "syslog.log"


    ''' <summary>
    ''' Cadena de conexion a la base de datos de Access
    ''' </summary>
    ''' <returns>La cadena de conexon a la base de datos de MS Access</returns>
    Public Property AccessConnectionString As String = ""




    Public Enum dataBaseType As Byte
        MS_ACCESS = 0
        SQL_SERVER = 1
        MYSQL = 2
        FOXPRO = 3
    End Enum


    ''' <summary>
    ''' Cadena de conexion a la base de datos
    ''' </summary>
    Private _connectionString As String = ""

    ''' <summary>
    ''' Tipo de base de datos. MsAccess = 0 / SQL Server = 1 / MySql = 2
    ''' </summary>
    ''' <remarks></remarks>
    Private _dbType As dataBaseType = dataBaseType.MS_ACCESS

    ''' <summary>
    ''' Nombre de usuario para acceso a la base de datos
    ''' </summary>
    Private _dbUsername As String = ""

    ''' <summary>
    ''' Contraseña para acceso a la base de datos
    ''' </summary>
    Private _dbPassword As String = ""

    ''' <summary>
    ''' Flag indicando si requiere credenciales (user/password) para ingresar a la base de datos
    ''' </summary>
    Private _requiereCredentials As Boolean = False

    ''' <summary>
    ''' Ruta de la base de datos. Puede ser un numero de IP, nombre de dominio o ruta de un sistema de archivos
    ''' </summary>
    Private _dbPath As String = ""

    ''' <summary>
    ''' Nombre de la base de datos a conectar. Solo para uso con bases SQL Server
    ''' </summary>
    Private _dbName As String = ""

    ''' <summary>
    ''' El puerto de escucha del servidor
    ''' </summary>
    Private _dbPort As Integer = 0

    '<----! FIN VARIABLES ---->





    ''' <summary>
    ''' Guarda o retorna el tipo de base de datos con la que se va a trabajar
    ''' </summary>
    ''' <value>Entero mayor que 0 indicando el tipo de base de datos. MS Access = 0 / SQL Server = 1 / MySQL = 2</value>
    ''' <returns>El tipo de base de datos a trabajar</returns>
    Public Property dbType As dataBaseType
        Get
            Return _dbType
        End Get
        Set(value As dataBaseType)
            _dbType = value
        End Set
    End Property


    ''' <summary>
    ''' Guarda o retorna un flag indicando si la conexion requiere credenciales de seguridad
    ''' </summary>
    ''' <value>Valor booleano indicando si requiere credenciales. TRUE = Requiere credendiales / FALSE = No requiere credenciales</value>
    ''' <returns>Si requiere credenciales</returns>
    Public Property RequireCredentials As Boolean
        Get
            Return _requiereCredentials
        End Get
        Set(value As Boolean)
            _requiereCredentials = value
        End Set
    End Property


    ''' <summary>
    ''' Guarda o retorna una cadena con la ruta a la base de datos. Puede ser un numero de IP, nombre de dominio o ruta dentro del sistema de archivos
    ''' </summary>
    ''' <value>Cadena con la ruta a la base de datos</value>
    ''' <returns>La ruta de la base de datos</returns>
    Public Property Path As String
        Get
            Return _dbPath
        End Get
        Set(value As String)
            _dbPath = value
        End Set
    End Property


    ''' <summary>
    ''' Guarda o retorna el nombre de usuario para ingresar a la base de datos
    ''' </summary>
    ''' <value>Cadena de caracteres con el nombre de usuario</value>
    ''' <returns>El nombre de usuario</returns>
    Public Property Username As String
        Get
            Return _dbUsername
        End Get
        Set(value As String)
            _dbUsername = value
        End Set
    End Property


    ''' <summary>
    ''' Guarda o retorna la contraseña para ingresar a la base de datos
    ''' </summary>
    ''' <value>Cadena conteniendo la contraseña de la base de datos</value>
    ''' <returns>La contraseña de la base de datos</returns>
    Public Property Password As String
        Get
            Return _dbPassword
        End Get
        Set(value As String)
            _dbPassword = value
        End Set
    End Property


    ''' <summary>
    ''' Retorna la cadena de conexion a la base de datos segun los parametros ingresados
    ''' </summary>
    ''' <returns>La cadena de conexion a la base de datos</returns>
    Public ReadOnly Property ConnectionString As String
        Get
            Return GenerateConnectionString()
        End Get
    End Property


    ''' <summary>
    ''' Guarda o retorna el nombre de la base de datos SQL Server
    ''' </summary>
    ''' <value>Nombre de la base de datos</value>
    ''' <returns>El nombre de la base de datos</returns>
    Public Property DatabaseName As String
        Get
            Return _dbName
        End Get
        Set(value As String)
            _dbName = value
        End Set
    End Property

    ''' <summary>
    ''' Guarda o retorna el puerto de escucha del servidor
    ''' </summary>
    ''' <returns></returns>
    Public Property Port As Integer
        Get
            Return _dbPort
        End Get
        Set(value As Integer)
            _dbPort = value
        End Set
    End Property


    ''' <summary>
    ''' Indica si el motor esta encendido o apagado
    ''' </summary>
    ''' <value>Valor booleano indicando el estado del motor</value>
    ''' <returns>El estado del motor, TRUE si esta encendido, FALSE si no</returns>
    ''' <remarks></remarks>
    Public Property IsStarted As Boolean = False



    ''' <summary>
    ''' Genera la cadena de conexion a la base de datos
    ''' </summary>
    ''' <returns>La cadena de conexion</returns>
    Private Function GenerateConnectionString() As String
        Dim tmpConn As String = ""

        Select Case _dbType
            Case dataBaseType.MS_ACCESS
                ' Generar cadena en el caso de MS Access
                If AccessConnectionString = "" Then
                    tmpConn = _AccessProvider
                    If _dbPath.Length <> 0 Then
                        tmpConn &= "Data Source=" & _dbPath & _dbName & ";"
                    Else
                        Return ""
                    End If
                    tmpConn &= _AccessConnTrailer
                Else
                    tmpConn = AccessConnectionString
                End If


            Case dataBaseType.SQL_SERVER
                ' Generar cadena en el caso de SQL Server
                If _dbPath.Length <> 0 Then
                    tmpConn &= "Data Source=" & _dbPath & ";"
                Else
                    Return ""
                End If

                If RequireCredentials = True Then
                    tmpConn &= _SQLServerAuthentication
                    tmpConn &= "uid=" & _dbUsername & ";"
                    tmpConn &= "Password=" & _dbPassword & ";"
                Else
                    tmpConn &= "Integrated Security=True;"
                End If

                If _dbName.Length <> 0 Then
                    tmpConn &= "Initial Catalog=" & _dbName & ";"
                Else
                    Return ""
                End If
                tmpConn &= _SQLServerConnTrailer

            Case dataBaseType.MYSQL

                ' Generar cadena en el caso de MySql
                If _dbPath.Length <> 0 Then
                    tmpConn &= "Host=" & _dbPath & ";"
                Else
                    Return ""
                End If

                If _dbUsername.Length > 0 Then
                    tmpConn &= "Uid=" & _dbUsername & ";"
                Else
                    Return ""
                End If

                If _dbPassword.Length > 0 Then
                    tmpConn &= "Pwd=" & _dbPassword & ";"
                Else
                    Return ""
                End If


                ' Puerto opcional
                If _dbPort > 0 Then
                    tmpConn &= "Port=" & _dbPort & ";"
                End If


                If _dbName.Length <> 0 Then
                    tmpConn &= "Database=" & _dbName & ";"
                Else
                    Return ""
                End If
            Case Else
                Return ""
        End Select
        _connectionString = tmpConn
        Return tmpConn
    End Function


    ''' <summary>
    ''' Inicia el motor sql, carga las variables en las distintas instancias y realiza una prueba de conexion
    ''' </summary>
    ''' <returns>True si inicio correctamente. False si hubo algun fallo</returns>
    Public Function Start() As Boolean

        If GenerateConnectionString.Length <> 0 Then
            InitializeObjects()

            Dim testSQLCore As New SQLCore
            With testSQLCore
                .dbType = _dbType
                .ConnectionString = _connectionString
                _IsStarted = .TestConnection()

                Return _IsStarted
            End With
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Inicia el motor sql sin hacer pruebas de conexion
    ''' </summary>
    ''' <returns>True si inicio correctamente. False si hubo algun fallo</returns>
    Public Function ColdBoot() As Boolean
        If GenerateConnectionString.Length <> 0 Then
            InitializeObjects()
            Return True
        Else
            Return False
        End If
    End Function



    ''' <summary>
    ''' Inicializa los objetos SQLEngine
    ''' </summary>
    Private Sub InitializeObjects()
        Delete.ConnectionString = _connectionString
        Delete.DbType = _dbType
        Delete.LogFileFullName = _LogFileFullName

        Insert.ConnectionString = _connectionString
        Insert.DbType = _dbType
        Insert.LogFileFullName = _LogFileFullName

        Update.ConnectionString = _connectionString
        Update.DbType = _dbType
        Update.LogFileFullName = _LogFileFullName

        Query.ConnectionString = _connectionString
        Query.DbType = _dbType
        Query.LogFileFullName = _LogFileFullName

        Db.ConnectionString = _connectionString
        Db.DbType = _dbType
        Db.dbPath = _dbPath
        Db.dbName = _dbName

        Builder.DatabaseType = _dbType
        Builder.DataBaseName = _dbName
        Builder.RequireCredentials = _requiereCredentials
        Builder.Username = _dbUsername
        Builder.Password = _dbPassword
    End Sub


    ''' <summary>
    ''' Envia una consulta SQL directamente sin tener que usar alguno de los subsistemas
    ''' </summary>
    ''' <param name="statement">La consulta de SQL, los parametros deben ingresarse como el caracter '?'</param>
    ''' <param name="isQuery">Flag indicando si la consulta devuelve resultados. TRUE si devuelve resultado, FALSE si no</param>
    ''' <param name="parameters">Lista de parametros de la consulta</param>
    ''' <param name="queryResult">Contenedor del resultado de la consulta</param>
    ''' <returns>TRUE si la operacion se realizo con exito, FALSE si fallo</returns>
    ''' <remarks>Uso: sqlEngine.SendSQLStatement("SELECT * FROM Tabla WHERE (ID >= ?) AND (Username = ?", True, {6, "Usuario"}, tablaResultado)</remarks>
    Public Function SendSQLStatement(ByVal statement As String, ByVal isQuery As Boolean, Optional parameters() As Object = Nothing, Optional ByRef queryResult As DataTable = Nothing) As Boolean
        Dim tmpSQLCore As New SQLCore
        Dim tmpOleParamList As New List(Of OleDbParameter)
        Dim tmpSqlParamList As New List(Of SqlParameter)

        Dim tmpProcessParam As Boolean = False

        GenerateConnectionString()

        With tmpSQLCore
            .dbType = _dbType
            .ConnectionString = _connectionString
            .QueryString = statement

            ' Transforma el array en una lista
            If (IsNothing(parameters) = False) Then
                For i = 0 To parameters.Length - 1
                    Dim tmpOleParam As New OleDbParameter
                    Dim tmpSqlParam As New SqlParameter
                    Select Case _dbType
                        Case dataBaseType.MS_ACCESS
                            tmpOleParam.ParameterName = "@p" & i
                            tmpOleParam.Value = parameters(i)
                            tmpOleParamList.Add(tmpOleParam)

                        Case dataBaseType.SQL_SERVER
                            tmpSqlParam.ParameterName = "@p" & i
                            tmpSqlParam.Value = parameters(i)
                            tmpSqlParamList.Add(tmpSqlParam)
                        Case Else
                            Return False
                    End Select
                Next
                tmpProcessParam = True
            End If



            If isQuery = True Then                                                  ' Consulta que va a devolver datos
                If IsNothing(queryResult) = False Then
                    Select Case _dbType
                        Case dataBaseType.MS_ACCESS
                            Return .ExecuteQuery(tmpProcessParam, tmpOleParamList, queryResult)
                        Case dataBaseType.SQL_SERVER
                            Return .ExecuteQuery(tmpProcessParam, tmpSqlParamList, queryResult)
                        Case Else
                            Return False
                    End Select

                Else
                    Console.Write("No hay tabla de resultados - SendSQLStatement")
                    Return False
                End If
            Else                                                                    ' Es una consulta que no va a devolver datos
                Select Case _dbType
                    Case dataBaseType.MS_ACCESS
                        Return .ExecuteNonQuery(tmpProcessParam, tmpOleParamList)
                    Case dataBaseType.SQL_SERVER
                        Return .ExecuteNonQuery(tmpProcessParam, tmpSqlParamList)
                    Case Else
                        Return False
                End Select

            End If
        End With
    End Function
End Class
