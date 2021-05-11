/*
    Setup Azure Blob Storage as a data source

    Ensure you set the following variables:

		**ADD-STRONG-PASSWORD-HERE**
    	**ADD-SECRET-HERE**
    	**ADD-LOCATION-URL**

	NOTE: Do not put the first character '?' in the secret
*/

create master encryption key 
by password = '**ADD-STRONG-PASSWORD-HERE**';

create database scoped credential GPConnectAnalyticsBlobStorageCredential
with identity = 'SHARED ACCESS SIGNATURE',
secret = '**ADD-SECRET-HERE**';

create external data source GPConnectAnalyticsBlobStore
with
(
	type = blob_storage,
	location = '**ADD-LOCATION-URL**',
	credential = GPConnectAnalyticsBlobStorageCredential
);