FROM postgres:16

# Environment variables like POSTGRES_DB, POSTGRES_USER and POSTGRES_PASSWORD
# should be provided by docker-compose or the runtime environment.
#
# This image will initialize a SINGLE logical database that contains
# all schemas for the previous microservices (users, menu, orders,
# kitchen, payments, loyalty).
#
# The SQL files are executed in alphanumeric order on first startup.

COPY schemas/*.sql /docker-entrypoint-initdb.d/


