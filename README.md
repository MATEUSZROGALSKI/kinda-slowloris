### Slowloris with throttled network stream ###
  This tool is based on well known slowloris type of (D)DoS attack with addition of throttled network.

## How does it work ##
  It works by sending an unfinished HTTP request to the server in specified by `concurrency` amounts. This value indicates how many connections will be opened to the target.
In addition to that this implementation can also throttle network sending each byte in between intervals specified by `timout` value in configuration.

## Configuration ##
  To start this tool you need to create `configuration.json` file in the working directory and fire up `./slowloris`.
Example congiuration:

	{
	  "targets": [
	    {
	      "host": "domain.com",
	      "port": 443,
	      "isSecure": "true",
	      "timeout": 5,
	      "concurrency": 1000
	    },
	    {
	      "host": "127.0.0.1",
	      "port": 80,
	      "isSecure": false,
	      "timeout": 3,
	      "concurrency": 1000
	    }
	  ]
	}

## Statistics ##
  Statistics are currently only available inside terminal window and will become visible when used `--statistics` or `-s`. ex: `./slowloris -s`
