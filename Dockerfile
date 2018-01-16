# This Dockerfile is used to run the test suite in a consistent Mono environment.
FROM mono:5.4.1.6

WORKDIR /usr/src/app

RUN nuget install NUnit.Console -Version 3.6.0 -OutputDirectory testrunner

COPY . /usr/src/app

RUN nuget restore Toolhouse.Monitoring.sln
RUN msbuild /p:Configuration=Release Toolhouse.Monitoring.sln
CMD [ "mono", "./testrunner/NUnit.ConsoleRunner.3.6.0/tools/nunit3-console.exe", "./Toolhouse.Monitoring.Tests/bin/Release/Toolhouse.Monitoring.Tests.dll" ]
