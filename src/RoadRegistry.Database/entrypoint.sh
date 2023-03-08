 #!/usr/bin/env bash
# /opt/mssql-tools/bin/sqlcmd -S 'tcp:mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'IF NOT EXISTS (SELECT * FROM [SYS].[DATABASES] WHERE [Name] = N'\''road-registry'\'') BEGIN CREATE DATABASE [road-registry] END;' \
# && /opt/mssql-tools/bin/sqlcmd -S 'tcp:mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'IF NOT EXISTS (SELECT * FROM [SYS].[DATABASES] WHERE [Name] = N'\''road-registry-events'\'') BEGIN CREATE DATABASE [road-registry-events] END;' \
