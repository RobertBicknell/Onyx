README

	Architecture

	Choices:
		Duende Identity Server was chosen as the identity provider.
		This is a well-known standard component supporting OIDC/OAuth standards, in particular, JWT tokens.
		It should be a good choice for securing an API use in a server-to-server distributed application, but can also be used from front-end clients such as SPAs
		and avoids the limitations of ASP.NET Forms authentication.
		In advanced scenarios (such as Open Banking specifications), Mutual TLS may be required, and this is also supported.

	Components:
		Servers:
			1. Identity server - supports (api) login and the issuing, refreshing and cancelling of claims/scope-based tokens used for Authentication and Authorisations by the:
			2. Products API server - this requires a valid token issued by the identity server. This is verified using the Public Key downloaded (once at startup, then cached) from Identity Server's public discovery docuemnt endpoint. The key can decrypt the encrypted JWT token payload, confirming the users claims and scopes. In this way, the API server avoids the overhead of making a separate call to the identity sever to authorise access and reduces the surface area for intercepting credentials.
			3. A test client, representing another service requiring access to the API server. This mimics a Web Application supporting interactive users, or a non-interactive system service.

		Databases, accounts, seed data:
			In this demo, one user account is hardcoded and no method is provided to create more. 
			As such, the user account information is stored in memory and no database is used. In a production scenario with interactive users, a database would be required.
			The Products are stored in a SQLEXPRESS database, with the connection string in appSettings of the ProductsAPI project.  
			The database and tables can be created by running the Migrations. No seed data is loaded by default.

	Auth 
		The authentication is not forms-based, but occurs in HTTP headers using OAuth standards, as might be the case in server-to-server scenarios.
		One scope is supported in the identity server, but more could be added, providing granular support to secure individual Controller Actions (e.g. Read-only GETs for one user, Read-Write GETs/PUT for another)

	Testing
		Unit Tests. There seemed little need to implement a repository for the requirements as they may be met with simple DbContext calls. The remaining code is app setup and configuration that cannot be usefully extracted for testing, and without any injected dependencies, there seems little to Unit Test. Therefore, the only unit tests are for the LambdaFactory.TryCreateFilter method and no mocks were needed. Otherwise, I would use NSubstitute to exercise unit/component logic with mock dependencies.

		Integration tests. A separate client Console App was used for integration / e2e testing.  A .bet file first starts the Auth and API project exes, then the client which calls those endpoints. In a production environment, these tests would use DEV/TEST appsettings to target a testing database. As this is a demo, just one database was used.


DIAGRAM

The diagram shows various microservice components interacting asynchronously via Event Bus messages.
	
Each component publishes events and subscribes to others, usually via topics. This allows one message to reach multiple receivers without the emitter needing seperate config for, or managing the connectivity with, other components. This eases scaling and makes performance behaviour more predictable, and allows for different parts of the system to fail or be updated with minimal impact to the overall system. 
	
In the microservice model, each component maintains its own persistent store of system data needed to perform its functions. Replication or updates can occur in different ways, one is the Outbox pattern whereby updates are transmitted from Database commit logs, producing a higher-degree of certainty about the integrity of the system's state. Kafka offers various levels of guarantee about message delivery, such as at-least-once, and exactly-once.

SignalR, websockets or HTTP2 are ways to avoid polling for updates between components or client sessions, meaning new messages can be acted-on in realtime, reducing latency and making user applications more responsive.

Typically, each microservice operates some form of dynamic cluster to balance load and provide resilience, such as with Kubernetes pods hosting Docker containers or Service Fabric nodes. In both, cluster health can be proactively monitored with health checks and watchdogs that can dynamically re-route requests and restart failed services/pods.
