RC=0

# Remove the output folders if they exist
rm -rf test-results
rm -rf failed-approvals
rm -rf mochawesome-report

# Spin up containers
docker-compose up -d --build

# Run tests
docker exec command sh -c "NODE_ENV=ci yarn integration" || RC=$?

# Surface any approval failures
docker exec command sh -c "./surface-approval-failures.sh"

# Copy test results out of command container
docker cp command:root/test-results .
docker cp command:root/mochawesome-report .
docker cp command:root/failed-approvals .

# Spin down containers (to be tiday)
docker-compose down

# Exit with result of integration tests
exit $RC
