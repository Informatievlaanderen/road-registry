 #!/usr/bin/env bash
/opt/mssql-tools/bin/sqlcmd -S 'tcp:streetname-mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'IF NOT EXISTS (SELECT * FROM [SYS].[DATABASES] WHERE [Name] = N'\''streetname-registry'\'') BEGIN CREATE DATABASE [streetname-registry] END;' \
&& /opt/mssql-tools/bin/sqlcmd -S 'tcp:streetname-mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -i "/var/lib/backup/create.sql" \
&& /opt/mssql-tools/bin/bcp StreetNameRegistryLegacy.StreetNameSyndication in "/var/lib/backup/syndication.bcp" -n -E -S streetname-mssql,1433 -U sa -P 'E@syP@ssw0rd' -d streetname-registry
