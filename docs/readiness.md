# Readiness


### Modeling Application Dependencies
On application startup (typically in `Application_Start`  in your `Global.asax.cs` file), you must describe your dependencies. The `Toolhouse.Monitoring.Readiness` class provides static methods for adding common types of dependencies, or you can model your own by implementing [`Toolhouse.Monitoring.Dependencies.IDependency`](TODO).

#### Databases: `AddDatabaseDependency`

```csharp
Toolhouse.Monitoring.Readiness.AddDatabaseDependency("MyConnection");
```

Adds a readiness check using a database connection string defined in the `<connectionStrings>` section of your `Web.config`.

`AddDatabaseDependency` will create a check that, when queried, will open a database connection and attempt to execute a trivial query (e.g., `SELECT 1+1`). It will not, by default, verify permissions to all tables.

#### SMTP Servers: `AddSmtpDependency()`

```csharp
Toolhouse.Monitoring.Readiness.AddSmtpDependency();
```

**TODO: Double-check this**
When queried, attempts to send an email to `smtptest@example.org`. If the SMTP server cannot be reached (or if the SMTP settings in your `Web.config` are invalid), this check will fail.

#### HTTP APIs: `AddHttpDependency()`

```csharp
Toolhouse.Monitoring.Readiness.AddHttpDependency("example", "http://example.org");
```

When queried, makes an HTTP `GET` request to the URL provided. If the returned status is not `200 OK`, the check fails.

#### Custom Dependencies

There are two methods for modeling custom dependencies.

##### via Callback Function

```csharp
Toolhouse.Monitoring.Readiness.AddDependency(
    "my_custom_dependency",
    () => {
        DoSomeInvolvedCheck();
        return true;
    }
);
```

Your callback function must return `true` if the dependency is ready, or `false` otherwise. If your callback throws an exception, the dependency will be marked as **not ready** and the exception message will be reported in the JSON document.

##### Implement `Toolhouse.Monitoring.Dependencies.IDependency`

```csharp
Toolhouse.Monitoring.Readiness.AddDependency(new MyCustomDependency());
```

[See `IDependency` for more details.](#TODO)

