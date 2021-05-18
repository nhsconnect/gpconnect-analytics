/*
    Schema V1.7 - update Import tables
*/

ALTER TABLE Import.[File] ADD RowsInstalledPerSecond AS (RowsAdded + RowsUpdated) / (CASE WHEN InstallDuration = 0 THEN 1 ELSE InstallDuration END)