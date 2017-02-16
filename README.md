# Toolhouse Monitoring

![Travis](https://api.travis-ci.org/toolhouse/monitoring-dotnet.svg?branch=master)
[![NuGet](https://img.shields.io/nuget/v/Toolhouse.Monitoring.svg)]()

This library adds logging and monitoring capabilities to ASP.net websites. It provides:

- A [simple healthcheck endpoint](./docs/healthcheck.md).
- A [readiness endpoint](./docs/readiness.md) that dynamically checks application dependencies.
- A [metrics endpoint](./docs/metrics.md) that reports application metrics using the [Prometheus](http://prometheus.io) wire format.

## Getting Started

First, add this library to your project:

```
> Install-Package Toolhouse.Monitoring
```

From there you'll need to configure each endpoint you'd like to add:

- [Healthcheck](./docs/healthcheck.md)
- [Readiness](./docs/readiness.md)
- [Metrics](./docs/metrics.md)

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

## Packaging

To push a new version to Nuget:

1. Update the `AssemblyInformationalVersion` attribute in [`AssemblyInfo.cs`](./Toolhouse.Monitoring/Properies/AssemblyInfo.cs).
2. Commit your changes.
3. Tag your version: `git tag vX.X.X` and push your tags.
4. Run [`package.cmd`](./package.cmd) to build a new package in `Dist/`.
5. Push your package via `nuget push` (see `package.cmd` output for instructions).

## Contributing

To request a new feature or report a bug [open an issue in Github](https://github.com/toolhouse/monitoring-dotnet/issues/new) for discussion prior to submitting a pull request. If you're willing to contribute changes/fixes mention that in the issue report or discussion. If you're reporting a bug be as specific as possible about the conditions causing the bug. If possible, attach a small sample project illustrating the issue.

One of the primary objectives of this project is to keep the library as simple and small as possible. If a proposed change is not a common use case or introduces undue complexity it may not be accepted.

## License

Copyright 2017 Toolhouse, Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
