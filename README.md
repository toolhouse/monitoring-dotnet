# Toolhouse Monitoring

This library adds logging and monitoring capabilities to ASP.net websites. It provides:

- A [simple healthcheck endpoint](#healthcheck).
- A [readiness endpoint](#readiness) that dynamically checks application dependencies.
- A [metrics endpoint](#metrics) that reports application metrics using the [Prometheus](http://prometheus.io) wire format.

To get started, add it to your project:

```
> Install-Package Toolhouse.Monitoring
```

<a name="healthcheck"></a>
## Adding a Healthcheck Endpoint

A healthcheck endpoint provides a means of verifying that your application is able to serve HTTP traffic. This library provides a simple `IHttpHandler` implementation that returns:

- `200 OK` status
- A response body of `OK`

To wire this handler into your application at `/health`, add the following to your `Web.config`:

```xml
<location path="health">
    <system.webServer>
        <handlers>
            <add name="ToolhousHealthHandler" path="*" verb="*" type="Toolhouse.Monitoring.Handlers.HealthEndpointHandler, Toolhouse.Monitoring"/>
        </handlers>
    </system.webServer>
</location>
```

You can verify it is working via `curl`:

```
> curl -i http://localhost/health
HTTP/1.1 200 OK
Cache-Control: private
Content-Type: text/html; charset=utf-8
Content-Length: 2

OK
```

<a name="readiness"></a>
## Readiness Endpoints

Your application likely depends on external systems to function. These systems typically include:

- Databases
- Caches
- SMTP servers
- 3rd-party HTTP APIs (SOAP, REST, etc.)

A **Readiness Endpoint** provides a machine- and human-readable status report on these dependencies. The status report is formatted as JSON:

```json
{
  "database": {
    "ready": true,
    "message": "Using database MY_DB"
  },
  "smtp": {
    ready: true,
    "message": ""
  }
}
```

For each dependency:

- `ready` indicates that the status check succeeded.
- `message` provides a human-readable diagnostic message.

The response is returned with one of the following HTTP status codes:

- `200 OK` if **all** dependencies are ready.
- `503 Service Unavailable` if **any** dependencies are not ready.

To add a readiness endpoint at `/readiness`, add the following to your `Web.config`:

```xml
<location path="readiness">
    <system.webServer>
        <handlers>
            <add name="ToolhouseReadinessHandler" path="*" verb="*" type="Toolhouse.Monitoring.Handlers.ReadinessEndpointHandler, Toolhouse.Monitoring"/>
        </handlers>
    </system.webServer>
</location>
```

It should work, but not return anything interesting:

```
> curl -i http://localhost/readiness
HTTP/1.1 200 OK
Cache-Control: private
Content-Type: application/json; charset=utf-8
Content-Length: 2

{}
```

We'll add a database dependency using a connection string from `Web.config`called `MyConnection`...

```csharp
// In our Application_Start handler...
Toolhouse.Monitoring.Readiness.AddDatabaseDependency("MyConnection");
```

...and see what happens:

```
> curl -i http://localhost/readiness
HTTP/1.1 200 OK
Cache-Control: private
Content-Type: application/json; charset=utf-8

{"database":{"ready":true,"message":"Using database 'MyDatabase'"}}
```

Great! The database is up.

This library supports non-database dependencies as well. See [the Readiness docs](./docs/readiness.md) for the full dependency modeling API.

<a name="metrics"></a>
## Adding a Metrics Endpoint

To add a Prometheus metrics scraping endpoint, register the HTTP handler like so:

```xml
 <location path="metrics">
    <system.webServer>
        <handlers>
            <add name="ToolhouseMetricsHandler" path="*" verb="*" type="Toolhouse.Monitoring.MetricsEndpointHandler, Toolhouse.Monitoring"/>
        </handlers>
    </system.webServer>
</location>
```

To automatically generate metrics around HTTP request and application exceptions, add `Toolhouse.Monitoring.Modules.MetricsModule` to your `Web.config`:

```xml
<system.webServer>
    <modules runAllManagedModulesForAllRequests="true">
        <add name="ToolhouseMetricsModule" type="Toolhouse.Monitoring.Modules.MetricsModule, Toolhouse.Monitoring"/>
    </modules>
</system.webServer>
```

For details about default metrics and information about implementing custom metrics, see [the Metrics docs](./docs/metrics.md).

## Securing Endpoints

Metrics and readiness endpoints can optionally be secured via HTTP Basic Authentication. You can use the built-in `<basicAuthentication>` and related elements in your `Web.config` or add the following to your `Web.config`:

```xml
<appSettings>
    <add key="Toolhouse.Monitoring.Username" value="" />
    <add key="Toolhouse.Monitoring.PasswordSha256" value="86a9836cfd1599012ef0e164da78f0676a227c453dd5ef76abd070e0d30e289d" />
</appSettings>
```

|             Setting             |                                Description                                |
|---------------------------------|---------------------------------------------------------------------------|
| `Toolhouse.Monitoring.Username` | Basic auth username.                                                      |
| `Toolhouse.Monitoring.Password` | SHA-256 hash of basic auth password. Should be a 64-character hex string. |

## Integrating with ASP.NET MVC

In ASP.NET MVC applications, it may be necessary to explicitly ignore routes that are to be handled via the `IHttpHandler`s provided by this library:

```csharp
// In Global.asax.cs
// Ignore routes used for metrics + monitoring
RouteTable.Routes.Ignore("health");
RouteTable.Routes.IgnoreRoute("metrics");
RouteTable.Routes.IgnoreRoute("readiness");
```
