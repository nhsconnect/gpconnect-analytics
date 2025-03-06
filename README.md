<img src="documentation/logo.png" height=72>

# GP Connect Analytics

## Build status

[![Build Actions Status](https://github.com/nhsconnect/gpconnect-analytics/workflows/continuous-integration/badge.svg)](https://github.com/nhsconnect/gpconnect-analytics/actions?)

## End-to-end data flow

![End-to-end diagram](documentation/end-to-end-data-flow.png)

## Data extracts

1. ASID lookup data
2. SSP transactions
3. MESH send document transactions

## Splunk queries

### 1. Get the ASID lookup list

`| inputlookup asidLookup.csv | table ASID, MName, NACS, OrgName, OrgType, PName, PostCode`

### 2. Get SSP transactions

This query works on the Splunk Cloud only:

`index=spine2vfmmonitor (logReference=SSP0001 OR logReference=SSP0015 OR logReference=SSP0016) | transaction internalID maxspan=1h keepevicted=true | table _time, SspTraceId, sspFrom, sspTo, interaction, responseCode, duration, responseSize, responseErrorMessage, method | sort 0 _time`

This query works on the Spine instance of Splunk only (i.e. Live A + Live B):

`index=spinevfm* logReference IN (SSP0001, SSP0004, SSP0012) | transaction internalID startswith=SSP0001 endswith=SSP0004 keepevicted=true maxspan=1h | table _time, sspFrom, sspTo, SspTraceId, interaction, responseCode, duration, responseSize, responseErrorMessage, method | sort 0 _time`

### 3. MESH send document transactions

(TBC)

## CSV filenames

General format:

`PROJECTNAME-EXTRACTNAME-QUERYFROMDATE-QUERYTODATE-SPLUNKINSTANCE-EXTRACTDATE.csv`

Where

- PROJECTNAME is `gpcanalytics`
- EXTRACTNAME is `asidlookup`, `ssptrans` (MESH transactions TBC)
- QUERYDATEFROM and QUERYDATETO is `YYYYMMDDTHHmmss`
- SPLUNKINSTANCE is `cloud`, `spinea`, `spineb`
- EXTRACTDATE is `YYYYMMDDTHHmmss`

Examples:

- `gpcanalytics-asidlookup-20200101T000000-20200101T000000-cloud-20210301T123200.csv`
- `gpcanalytics-ssptrans-20200101T000000-20200107T000000-cloud-20210105T103000.csv`
- `gpcanalytics-ssptrans-20200107T000000-2020014T000000-spinea-20210105T103000.csv`

Note:  The QUERYDATEFROM and QUERYDATETO don't affect the output of the ASID lookup data query from Splunk, however are
included for consistency.

## Run a local SQL Server instance

To pull the image:

`docker pull mcr.microsoft.com/mssql/server`

To run the instance on the default port:

`docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=StrongP@ssword1' -p 1433:1433 -d mcr.microsoft.com/mssql/server`

## Testing

Tests were added the project on 8th Feb 2025 by Grant Riordan covering:

- Core Project
- Functions Project
- Integration test for Hierarchy repository

### How To Run Coverage Report

**Run the coverage tests**:

If you do not own a DotCover license or equivalent, you can use `coverlet` a free tool for running coverage reports.

- navigate to the `/source` directory
- open terminal and paste

```bash
dotnet test --collect:"XPlat Code Coverage" -m:1
```

or

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov

```

**Install Report Generator Globally**
Report generator allows us to build a html report of the coverage making it easier to view.

run the following to install: 
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool

```
then run the following to merge the coverage results into 1 report file

```bash
reportgenerator -reports:"../**/coverage.cobertura.xml" -reporttypes:"html" -targetdir:"./CoverageReport"
- ```
