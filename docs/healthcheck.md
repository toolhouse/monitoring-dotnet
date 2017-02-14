# Adding a Healthcheck Endpoint

A healthcheck endpoint provides a means of verifying that your application is able to serve HTTP traffic. This library provides [a simple `IHttpHandler`](../Toolhouse.Monitoring/Handlers/HealthEndpointHandler.cs) implementation that returns:

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

