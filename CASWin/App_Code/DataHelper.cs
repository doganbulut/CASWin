#region Directives
using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using CASWin.Properties;
#endregion

public class DataHelper
{	
	#region Member Variables
	private DataSet ds;
	private SqlDataReader dataReader;
	#endregion

	#region ConnectionString
	private string StrCnn()
	{
        return Settings.Default.SQLConnectionString;
	}
	#endregion

	#region SetPrmArrayByPrm
	public bool SetPrmArrayByPrm(SqlParameter[] prmArray, Int16 prmOrdinal, string prmName, SqlDbType prmType, string prmValue, bool outValue)
	{
		try
		{
			if (prmOrdinal < prmArray.Length)
			{
				if (outValue == false)
				{
					prmArray[prmOrdinal] = new SqlParameter(prmName, prmType);
					prmArray[prmOrdinal].Value = prmValue;
					return true;
				}
				else
				{
					prmArray[prmOrdinal] = new SqlParameter(prmName, prmType);
					prmArray[prmOrdinal].Size = int.Parse(prmValue);
					prmArray[prmOrdinal].Value = prmValue;
					prmArray[prmOrdinal].Direction = ParameterDirection.Output;
					return true;
				}
			}
			else
			{
				return false;
			}
		}
		catch
		{
			 
			return false;
		}
	}
	#endregion

    #region SetPrmArrayByPrm
    public bool SetPrmArrayByPrm(SqlParameter[] prmArray, Int16 prmOrdinal, string prmName, SqlDbType prmType, int intSize, object prmValue, bool outValue)
    {
        try
        {
            if (prmOrdinal < prmArray.Length)
            {
                if (outValue == false)
                {
                    prmArray[prmOrdinal] = new SqlParameter(prmName, prmType);
                    prmArray[prmOrdinal].Size = intSize;
                    prmArray[prmOrdinal].Value = prmValue;
                    return true;
                }
                else
                {
                    prmArray[prmOrdinal] = new SqlParameter(prmName, prmType);                    
                    prmArray[prmOrdinal].Value = prmValue;
                    prmArray[prmOrdinal].Size = intSize;
                    prmArray[prmOrdinal].Direction = ParameterDirection.Output;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        catch
        {
            
            return false;
        }
    }
    #endregion

	#region DataTable
	public DataTable GetDataTable(SqlParameter[] prmArray, string spName)
	{
		try
		{
			ds = SqlHelper.ExecuteDataset(StrCnn(), CommandType.StoredProcedure, spName, prmArray);
			return ds.Tables[0];
		}
		catch
		{
            return null;
		}
	}

	public DataTable GetDataTable(CommandType cmdType, string spName)
	{
		try
		{
			ds = SqlHelper.ExecuteDataset(StrCnn(), cmdType, spName);
			return ds.Tables[0];
		}
		catch
		{
			return null;
		}
	}
	#endregion

	#region DataSet
	public DataSet GetDataSet(SqlParameter[] prmArray, string spName)
	{
		try
		{
			ds = SqlHelper.ExecuteDataset(StrCnn(), CommandType.StoredProcedure, spName, prmArray);
			return ds;
		}
		catch
		{
			return null;
		}
	}

	public DataSet GetDataSet(CommandType cmdType, string strCmd)
	{
		try
		{
			ds = SqlHelper.ExecuteDataset(StrCnn(), cmdType, strCmd);
			return ds;
		}
		catch
		{
			return null;
		}
	}
	#endregion

	#region ExecuteScalar
	public bool ExecuteScalar(string spName)
	{
		try
		{
			SqlHelper.ExecuteScalar(StrCnn(), CommandType.StoredProcedure, spName);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public string ExecuteScalar( SqlParameter[] prmArray,string spName)
	{
		try
		{
			return SqlHelper.ExecuteScalar(StrCnn(), CommandType.StoredProcedure, spName, prmArray).ToString();
            
		}
		catch
		{
            return null;
		}
	}
	#endregion

	#region ExecuteCommand
	public bool ExecuteCommand(string spName)
	{
		try
		{
			SqlHelper.ExecuteNonQuery(StrCnn(), CommandType.StoredProcedure, spName);
			return true;
		}
		catch
		{
            return false;
		}
	}
	
	public bool ExecuteCommand(string spName, SqlParameter[] prmArray)
	{
		try
		{
			SqlHelper.ExecuteNonQuery(StrCnn(), CommandType.StoredProcedure, spName, prmArray);
			return true;
		}
		catch(Exception e) 
		{
            return false;
		}

	} 
	#endregion
	
	#region DataReader
	public SqlDataReader GetDataReader(string spName) 
	{
		try
		{
			dataReader = SqlHelper.ExecuteReader(StrCnn(), CommandType.StoredProcedure, spName);
			return dataReader;
		}
		catch
		{
			return null;
		}
	}

	public SqlDataReader GetDataReader(SqlParameter[] prmArray, string spName)
	{
		try
		{
			dataReader = SqlHelper.ExecuteReader(StrCnn(), CommandType.StoredProcedure, spName, prmArray);
			return dataReader;
		}
		catch
		{
			return null;
		}
	}
	#endregion
		
	#region ExecuteScalar
	public object GetObject(string spName)
	{
		try
		{
			return SqlHelper.ExecuteScalar(StrCnn(), CommandType.StoredProcedure, spName);
		}
		catch
		{
			return null;
		}
	}
	#endregion	 
}