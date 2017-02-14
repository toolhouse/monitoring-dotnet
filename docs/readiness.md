# Readiness

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
> curl -i http://mywebsite/readiness
HTTP/1.1 200 OK
Cache-Control: private
Content-Type: application/json; charset=utf-8
Content-Length: 2

{}
```

We'll add a database dependency using a connection string from `Web.config`called `MyConnection`...

```csharp
// In Application_Start...
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

Great! The database is up. But this library supports non-database dependencies as well.

### Modeling Application Dependencies

For the readiness endpoint to be able to provide a report, you must describe your dependencies. The `Toolhouse.Monitoring.Readiness` class provides static methods for adding common types of dependencies, or you can model your own by implementing [`Toolhouse.Monitoring.Dependencies.IDependency`](../Toolhouse.Monitoring/Dependencies/IDependency.cs).

Typically, you'll add your dependencies in the `Application_Start` method of your `Global.asax.cs` file.

#### Databases: `AddDatabaseDependency()`

```csharp
Toolhouse.Monitoring.Readiness.AddDatabaseDependency("MyConnection");
```

Adds a readiness check using a database connection string defined in the `<connectionStrings>` section of your `Web.config`.

`AddDatabaseDependency` will create a check that, when queried, will open a database connection and attempt to execute a trivial query (e.g., `SELECT 1+1`). It will not, by default, verify permissions to all tables.

#### SMTP Servers: `AddSmtpDependency()`

```csharp
Toolhouse.Monitoring.Readiness.AddSmtpDependency();
```

When queried, attempts to send an email to `smtptest@example.org`. If the SMTP server cannot be reached (or if the SMTP settings in your `Web.config` are invalid), this check will fail.

#### HTTP APIs: `AddHttpDependency()`

```csharp
Toolhouse.Monitoring.Readiness.AddHttpDependency("example", "http://example.org");
```

When queried, makes an HTTP `GET` request to the URL provided. If the returned status is not `200 OK`, the check fails.

#### Custom Dependencies

There are two methods for modeling custom dependencies.

##### 1. Using a Callback Function

```csharp
Toolhouse.Monitoring.Readiness.AddDependency(
    "my_custom_dependency",
    () => {
        DoSomeInvolvedCheck();
        return true;
    }
);
```

Your callback function must return `true` if the dependency is ready, or `false` otherwise. If your callback throws an exception, the dependency will be marked as **not ready** and the exception message will be reported as the `message` in the resulting JSON document.

##### 2. By Implementing `Toolhouse.Monitoring.Dependencies.IDependency`

```csharp
Toolhouse.Monitoring.Readiness.AddDependency(new MyCustomDependency());
```

See the [`IDependency`](../Toolhouse.Monitoring/Dependencies/IDependency.cs) class for more details.

