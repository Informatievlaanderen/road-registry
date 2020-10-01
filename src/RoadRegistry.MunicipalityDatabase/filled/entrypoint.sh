 #!/usr/bin/env bash
/opt/mssql-tools/bin/sqlcmd -S 'tcp:municipality-mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'IF NOT EXISTS (SELECT * FROM [SYS].[DATABASES] WHERE [Name] = N'\''municipality-registry'\'') BEGIN CREATE DATABASE [municipality-registry] END;' \
&& /opt/mssql-tools/bin/sqlcmd -S 'tcp:municipality-mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -i "/var/lib/backup/create.sql" \
&& /opt/mssql-tools/bin/bcp MunicipalityRegistryLegacy.MunicipalitySyndication in "/var/lib/backup/syndication.bcp" -n -E -S municipality-mssql,1433 -U sa -P 'E@syP@ssw0rd' -d municipality-registry
