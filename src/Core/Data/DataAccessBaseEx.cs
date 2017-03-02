using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using EPiServer.Data;
using EPiServer.DataAccess;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace Knowit.NotFound.Core.Data
{
    public class DataAccessBaseEx : DataAccessBase
    {
        public DataAccessBaseEx(IDatabaseExecutor handler)
            : base(handler)
        {
            this.Executor = handler;
            Executor = handler;
        }

        public static DataAccessBaseEx GetWorker()
        {
            return ServiceLocator.Current.GetInstance<DataAccessBaseEx>();
        }
        // ReSharper disable once InconsistentNaming
        private const string REDIRECTSTABLE = "[dbo].[BVN.NotFoundMultiSiteRequests]";

        private static readonly ILogger Logger = LogManager.GetLogger();

        public DataSet ExecuteSQL(string sqlCommand, List<IDbDataParameter> parameters)
        {


            return Executor.Execute(delegate
            {
                using (DataSet ds = new DataSet())
                {
                    try
                    {
                        DbCommand command = CreateCommand(sqlCommand);
                        if (parameters != null)
                        {
                            foreach (var dbDataParameter in parameters)
                            {
                                var parameter = (SqlParameter) dbDataParameter;
                                command.Parameters.Add(parameter);
                            }
                        }
                        command.CommandType = CommandType.Text;
                        CreateDataAdapter(command).Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("An error occureding in the ExecuteSQL method with the following sql{0}. Exception:{1}", sqlCommand, ex));
                    }

                    return ds;
                }
            });

        }

        public bool ExecuteNonQuery(string sqlCommand)
        {
            return Executor.Execute(delegate
            {
                bool success = true;

                try
                {
                    IDbCommand command = CreateCommand(sqlCommand);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    success = false;
                    Logger.Error(string.Format("An error occureding in the ExecuteSQL method with the following sql{0}. Exception:{1}", sqlCommand, ex));

                }
                return success;

            });


        }

        public int ExecuteScalar(string sqlCommand)
        {
            return Executor.Execute(delegate
            {
                int result;
                try
                {
                    IDbCommand dbCommand = CreateCommand(sqlCommand);
                    dbCommand.CommandType = CommandType.Text;
                    result = (int)dbCommand.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    result = 0;
                    Logger.Error(
                        string.Format(
                            "An error occureding in the ExecuteScalar method with the following sql{0}. Exception:{1}",
                            sqlCommand,
                            ex));
                    
                }
                return result;
            });
        }

        public DataSet FindSiteIdByHost(string hostName)
        {
            string sqlCommand = "SELECT [fkSiteID] FROM [dbo].[tblHostDefinition] WHERE [Name] = @hostName";
            var hostNameParam = CreateParameter("hostName", DbType.String, 100);
            hostNameParam.Value = hostName;
            var parameters = new List<IDbDataParameter> { hostNameParam };
            var result = ExecuteSQL(sqlCommand, parameters);
            return result;


        }

        public DataSet FindSiteIds()
        {
           // string sqlCommand = "SELECT [pkId] FROM [dbo].[tblSiteDefinition]";
            string sqlCommand = "SELECT DISTINCT [fkSiteID] FROM [dbo].[tblHostDefinition]";
            return ExecuteSQL(sqlCommand, null);
        }

        public DataSet GetAllClientRequestCount(int siteId)
        {
            string sqlCommand = string.Format("SELECT [OldUrl], COUNT(*) as Requests FROM {0} WHERE [fkId] = @siteId GROUP BY [OldUrl] order by Requests desc", REDIRECTSTABLE);
            var siteIdParam = CreateParameter("siteId", DbType.Int64);
            siteIdParam.Value = siteId;
            var parameters = new List<IDbDataParameter> {siteIdParam};
            return ExecuteSQL(sqlCommand, parameters);
        }

        public void DeleteRowsForRequest(string oldUrl, int siteId)
        {
            string sqlCommand = string.Format("DELETE FROM {0} WHERE [OldUrl] = @oldurl AND [fkId] = @siteId", REDIRECTSTABLE);
            var oldUrlParam = CreateParameter("oldurl", DbType.String, 4000);
            oldUrlParam.Value = oldUrl;
            var siteIdParam = CreateParameter("siteId", DbType.Int64);
            siteIdParam.Value = siteId;

            var parameters = new List<IDbDataParameter> {oldUrlParam, siteIdParam};
            ExecuteSQL(sqlCommand, parameters);
        } 

        public void DeleteSuggestions(int maxErrors, int minimumDaysOld, int siteId)
        { 
            string sqlCommand = string.Format(@"delete from {0}
                                                where [OldUrl] in (
                                                select [OldUrl]
                                                  from (
                                                      select [OldUrl]
                                                      from {0}
                                                      Where DATEDIFF(day, [Requested], getdate()) >= {1}
                                                      and [fkId] = @siteId
                                                      group by [OldUrl]
                                                      having count(*) <= {2} 
                                                      ) t
                                                )",REDIRECTSTABLE, minimumDaysOld, maxErrors);

            var siteIdParam = CreateParameter("siteId", DbType.Int64);
            siteIdParam.Value = siteId;
            var parameters = new List<IDbDataParameter> { siteIdParam };
            ExecuteSQL(sqlCommand, parameters);
        }
        public void DeleteAllSuggestions(int siteId)
        {
            string sqlCommand = string.Format(@"delete from {0} where [fkId] = @siteId", REDIRECTSTABLE);
            var siteIdParam = CreateParameter("siteId", DbType.Int64);
            siteIdParam.Value = siteId;
            var parameters = new List<IDbDataParameter> { siteIdParam };
            ExecuteSQL(sqlCommand, parameters);
        }

        public DataSet GetRequestReferers(string url, int siteId)
        {
            string sqlCommand = string.Format("SELECT [Referer], COUNT(*) as Requests FROM {0} where [OldUrl] = @oldurl and [fkId] = @siteId GROUP BY [Referer] order by Requests desc", REDIRECTSTABLE);
            var oldUrlParam = CreateParameter("oldurl", DbType.String, 4000);
            oldUrlParam.Value = url;
            var siteIdParam = CreateParameter("siteId", DbType.Int64);
            siteIdParam.Value = siteId;

            var parameters = new List<IDbDataParameter> {oldUrlParam, siteIdParam};
            return ExecuteSQL(sqlCommand, parameters);

        }

        public DataSet GetTotalNumberOfSuggestions(int siteId)
        {

            string sqlCommand = string.Format("SELECT COUNT(DISTINCT [OldUrl]) FROM {0} WHERE [fkId] = @siteId", REDIRECTSTABLE);
            var siteIdParam = CreateParameter("siteId", DbType.Int64);
            siteIdParam.Value = siteId;
            var parameters = new List<IDbDataParameter> { siteIdParam };
            return ExecuteSQL(sqlCommand, parameters);
        }



        public int Check404Version()
        {

            return Executor.Execute(() =>
    {

        string sqlCommand = "dbo.bvn_notfoundmultisiteversion";
        int version = -1;
        try
        {
            DbCommand command = CreateCommand();
            command.Parameters.Add(CreateReturnParameter());
            command.CommandText = sqlCommand;
            command.CommandType = CommandType.StoredProcedure;
            command.ExecuteNonQuery();
            version = Convert.ToInt32(GetReturnValue(command).ToString());
        }
        catch (SqlException)
        {
            Logger.Information("Stored procedure not found. Creating it.");
            return version;
        }
        catch (Exception ex)
        {
            Logger.Error(string.Format("Error during NotFoundHandler version check:{0}", ex));
        }
        return version;
    });

        }


        public void LogRequestToDb(string oldUrl, string referer, DateTime now, int siteId)
        {
            Executor.Execute(() =>
               {
                   string sqlCommand = string.Format("INSERT INTO {0} (" +
                                       "Requested, OldUrl, " +
                                       "Referer, fkId" +
                                       ") VALUES (" +
                                       "@requested, @oldurl, " +
                                       "@referer, @siteId" +
                                       ")", REDIRECTSTABLE);
                   try
                   {
                       IDbCommand command = CreateCommand();

                       var requstedParam = CreateParameter("requested", DbType.DateTime, 0);
                       requstedParam.Value = now;
                       var refererParam = CreateParameter("referer", DbType.String, 4000);
                       refererParam.Value = referer;
                       var oldUrlParam = CreateParameter("oldurl", DbType.String, 4000);
                       oldUrlParam.Value = oldUrl;
                       var siteIdParam = CreateParameter("siteId", DbType.Int64);
                       siteIdParam.Value = siteId;
                       command.Parameters.Add(requstedParam);
                       command.Parameters.Add(refererParam);
                       command.Parameters.Add(oldUrlParam);
                       command.Parameters.Add(siteIdParam);
                       command.CommandText = sqlCommand;
                       command.CommandType = CommandType.Text;
                       command.Connection = Executor.DbFactory.CreateConnection();
                       command.ExecuteNonQuery();
                   }
                   catch (Exception ex)
                   {

                       Logger.Error("An error occured while logging a 404 handler error. Ex:" + ex);
                   }
                   return true;
               });
        }


       
    }
}