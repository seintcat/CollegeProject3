using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;

public class SqlAccess
{
    static Dictionary<string, SqlAccess> accessPoint = new Dictionary<string, SqlAccess>();
    //public static string gameDB { get { return Application.dataPath + "/Resources/" + "game.db"; } }
    //public static string battleDB { get { return Application.dataPath + "/Resources/" + "battle.db"; } }
    public static string gameDB { get { return Application.streamingAssetsPath + "/" + "game.db"; } }
    public static string battleDB { get { return Application.streamingAssetsPath + "/" + "battle.db"; } }

    string path;
    IDbConnection dbConnection;
    IDbCommand dbCommand;
    IDataReader _dataReader;

    public bool open { get { return dbConnection != null; } }
    public bool read { get { return _dataReader != null; } }
    public IDataReader dataReader { get { return _dataReader; } }

    private SqlAccess(string _path)
    {
        path = _path;
    }

    public static SqlAccess GetAccess(string _path)
    {
        if (accessPoint.ContainsKey(_path))
            return accessPoint[_path];

        accessPoint.Add(_path, new SqlAccess(_path));
        return accessPoint[_path];
    }

    public void Open()
    {
        try
        {
            // Open a connection to the database.
            dbConnection = new SqliteConnection("URI=file:" + path);
            dbConnection.Open();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            ShutDown();
        }
    }

    public void SqlRead(string command)
    {
        if (dbConnection == null)
            return;

        try
        {
            if (_dataReader != null)
            {
                _dataReader.Dispose();
                _dataReader = null;
            }
            if (dbCommand != null)
            {
                dbCommand.Dispose();
                dbCommand = null;
            }

            dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = command;
            _dataReader = dbCommand.ExecuteReader();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            ShutDown();
        }
    }

    public void SqlExecute(string command)
    {
        if (dbConnection == null)
            return;

        try
        {
            if (_dataReader != null)
            {
                _dataReader.Dispose();
                _dataReader = null;
            }
            if (dbCommand != null)
            {
                dbCommand.Dispose();
                dbCommand = null;
            }

            dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = command;
            dbCommand.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            ShutDown();
        }
    }

    public void ShutDown()
    {
        if (_dataReader != null)
        {
            _dataReader.Dispose();
            _dataReader = null;
        }
        if (dbCommand != null)
        {
            dbCommand.Dispose();
            dbCommand = null;
        }
        if (dbConnection != null)
        {
            dbConnection.Dispose();
            dbConnection = null;
        }
    }
}
