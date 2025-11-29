# Auth Service Database Backup & Restore

## Quick Backup

```bash
# Backup entire database
pg_dump -U postgres -h localhost -d wechat_auth > auth_backup_$(date +%Y%m%d_%H%M%S).sql

# Backup with compression
pg_dump -U postgres -h localhost -d wechat_auth | gzip > auth_backup_$(date +%Y%m%d_%H%M%S).sql.gz

# Backup schema only
pg_dump -U postgres -h localhost -d wechat_auth --schema-only > auth_schema_$(date +%Y%m%d).sql

# Backup data only
pg_dump -U postgres -h localhost -d wechat_auth --data-only > auth_data_$(date +%Y%m%d).sql
```

## Restore

```bash
# Restore from backup
psql -U postgres -h localhost -d wechat_auth < auth_backup.sql

# Restore from compressed backup
gunzip -c auth_backup.sql.gz | psql -U postgres -h localhost -d wechat_auth

# Restore specific table
pg_restore -U postgres -h localhost -d wechat_auth -t users auth_backup.sql
```

## Automated Backup Script

```bash
#!/bin/bash
# Save as: backup_auth_db.sh

BACKUP_DIR="/var/backups/postgresql/auth"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/auth_$TIMESTAMP.sql.gz"
RETENTION_DAYS=30

# Create backup directory
mkdir -p $BACKUP_DIR

# Perform backup
pg_dump -U postgres -h localhost -d wechat_auth | gzip > $BACKUP_FILE

# Delete old backups
find $BACKUP_DIR -name "auth_*.sql.gz" -mtime +$RETENTION_DAYS -delete

echo "Backup completed: $BACKUP_FILE"
```

## Schedule with Cron

```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM
0 2 * * * /path/to/backup_auth_db.sh >> /var/log/auth_backup.log 2>&1
```

## Point-in-Time Recovery

Enable WAL archiving in postgresql.conf:

```
wal_level = replica
archive_mode = on
archive_command = 'test ! -f /var/lib/postgresql/archive/%f && cp %p /var/lib/postgresql/archive/%f'
```

## Docker Backup

```bash
# Backup from Docker container
docker exec wechat-postgres pg_dump -U postgres wechat_auth > auth_backup.sql

# Restore to Docker container
docker exec -i wechat-postgres psql -U postgres wechat_auth < auth_backup.sql
```
