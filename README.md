this needs `configuration.json` with structure:
{
	threadCount: numberOfThreadsToUse,
	ioThreadCount: numberOfInputOutputThreadsToUse,
	targets: [
		{
			host: targetIP,
			port: targetPort,
			timeout: timeBetweenSendingEachByte ( preferably 0-10 ),
			concurrency: numberOfConcurrentConnections ( WARNING! this burns out CPU )
		},
		{
			host: ...
		}
	]
}