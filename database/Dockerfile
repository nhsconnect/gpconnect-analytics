FROM flyway/flyway:7 AS base


COPY schema/* sql/schema/
COPY stored-procedures/* sql/stored-procedures/
COPY functions/* sql/functions/
COPY views/* sql/views/
COPY tests/* sql-tests/
COPY tests/test-data/* sql-tests/test-data/