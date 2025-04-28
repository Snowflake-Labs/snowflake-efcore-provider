using System.Collections;
using System.Reflection;
using log4net;
using Snowflake.Data.Client;

namespace EFCore.Tests;

public class EFCoreTestBase
{
    protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    protected DbContextBase _dbContext;
    protected ArrayList _tablesToClean = new ArrayList();

    public EFCoreTestBase()
    {
        InitializeDatabaseContext();
        InitializeTableNamesToClean();
    }

    [SetUp]
    public void BeforeEach()
    {
    }

    [TearDown]
    public void AfterEach()
    {
    }
    
    public virtual void InitializeDatabaseContext()
    {
    }

    public virtual void InitializeTableNamesToClean()
    {
    }

    protected void DropTables()
    {
        _log.Debug("Removal of database tables");
        using (var conn = new SnowflakeDbConnection(DbContextBase.GetConnectionStringFromParameters("parameters.json")))
        {
            conn.Open();
            var cmd = conn.CreateCommand();
            foreach (var tableName in _tablesToClean)
            {
                cmd.CommandText = $"drop table if exists \"{tableName}\"";
                _log.Debug(cmd.CommandText);
                cmd.ExecuteNonQuery();
            }

            conn.Close();
        }
    }
    
    protected void TruncateTables()
    {
        _log.Debug("Cleanup database tables");
        using (var conn = new SnowflakeDbConnection(DbContextBase.GetConnectionStringFromParameters("parameters.json")))
        {
            conn.Open();
            var cmd = conn.CreateCommand();
            foreach (var tableName in _tablesToClean)
            {
                cmd.CommandText = $"truncate table if exists \"{tableName}\"";
                _log.Debug(cmd.CommandText);
                cmd.ExecuteNonQuery();
            }

            conn.Close();
        }
    }
}
