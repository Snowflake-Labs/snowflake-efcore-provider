namespace Snowflake.EntityFrameworkCore.Update.Internal;

using Microsoft.EntityFrameworkCore.Update;

public class SnowflakeModificationCommandFactory : IModificationCommandFactory
{
    public  virtual IModificationCommand CreateModificationCommand(
        in ModificationCommandParameters modificationCommandParameters)
        => new SnowflakeModificationCommand(modificationCommandParameters);

    public  virtual INonTrackedModificationCommand CreateNonTrackedModificationCommand(
        in NonTrackedModificationCommandParameters modificationCommandParameters)
        => new SnowflakeModificationCommand(modificationCommandParameters);
}
