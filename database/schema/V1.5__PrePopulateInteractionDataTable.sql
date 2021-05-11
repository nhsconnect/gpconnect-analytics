/*
    Schema V1.5 - Pre-populate Data.Interaction with known interaction IDs up front
*/

insert into Data.Interaction
(
	InteractionId,
	InteractionName,
    ServiceName
)
values
(
	1,
	'urn:nhs:names:services:gpconnect:fhir:rest:read:metadata',
	'gpconnect'
),
(
	2,
	'urn:nhs:names:services:gpconnect:fhir:operation:gpc.getcarerecord',
	'gpconnect'
),
(
	3,
	'urn:nhs:names:services:gpconnect:fhir:rest:read:metadata-1',
	'gpconnect'
),
(
	4,
	'urn:nhs:names:services:gpconnect:fhir:rest:read:patient-1',
	'gpconnect'
),
(
	5,
	'urn:nhs:names:services:gpconnect:fhir:rest:search:patient-1',
	'gpconnect'
),
(
	6,
	'urn:nhs:names:services:gpconnect:fhir:rest:read:practitioner-1',
	'gpconnect'
),
(
	7,
	'urn:nhs:names:services:gpconnect:fhir:rest:search:practitioner-1',
	'gpconnect'
),
(
	8,
	'urn:nhs:names:services:gpconnect:fhir:rest:read:organization-1',
	'gpconnect'
),
(
	9,
	'urn:nhs:names:services:gpconnect:fhir:rest:search:organization-1',
	'gpconnect'
),
(
	10,
	'urn:nhs:names:services:gpconnect:fhir:rest:read:location-1',
	'gpconnect'
),
(
	11,
	'urn:nhs:names:services:gpconnect:fhir:operation:gpc.registerpatient-1',
	'gpconnect'
),
(
	12,
	'urn:nhs:names:services:gpconnect:fhir:rest:search:slot-1',
	'gpconnect'
),
(
	13,
	'urn:nhs:names:services:gpconnect:fhir:rest:read:appointment-1',
	'gpconnect'
),
(
	14,
	'urn:nhs:names:services:gpconnect:fhir:rest:create:appointment-1',
	'gpconnect'
),
(
	15,
	'urn:nhs:names:services:gpconnect:fhir:rest:update:appointment-1',
	'gpconnect'
),
(
	16,
	'urn:nhs:names:services:gpconnect:fhir:rest:cancel:appointment-1',
	'gpconnect'
),
(
	17,
	'urn:nhs:names:services:gpconnect:fhir:rest:search:patient_appointments-1',
	'gpconnect'
),
(
	18,
	'urn:nhs:names:services:gpconnect:structured:fhir:rest:read:metadata-1',
	'gpconnect'
),
(
	19,
	'urn:nhs:names:services:gpconnect:fhir:operation:gpc.getstructuredrecord-1',
	'gpconnect'
),
(
	20,
	'urn:nhs:names:services:gpconnect:documents:fhir:rest:read:metadata-1',
	'gpconnect'
),
(
	21,
	'urn:nhs:names:services:gpconnect:documents:fhir:rest:search:patient-1',
	'gpconnect'
),
(
	22,
	'urn:nhs:names:services:gpconnect:documents:fhir:rest:search:documentreference-1',
	'gpconnect'
),
(
	23,
	'urn:nhs:names:services:gpconnect:documents:fhir:rest:read:binary-1',
	'gpconnect'
);
