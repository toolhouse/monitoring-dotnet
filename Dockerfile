# This Dockerfile is used to run the test suite in a consistent Mono environment.
FROM mono:5.4.1.6

WORKDIR /usr/src/app

RUN nuget install NUnit.Console -Version 3.6.0 -OutputDirectory testrunner

RUN apt-get update \
 && apt-get -y install curl libunwind8 gettext apt-transport-https \
 && curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg \
 && mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg \
 && sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-debian-jessie-prod jessie main" > /etc/apt/sources.list.d/dotnetdev.list'
RUN apt-get update \
 && apt-get -y install dotnet-sdk-2.0.0 \
 && export PATH=$PATH:$HOME/dotnet

COPY . /usr/src/app

RUN dotnet restore Toolhouse.Monitoring.sln
RUN msbuild /p:Configuration=Release Toolhouse.Monitoring.sln
CMD [ "mono", "./testrunner/NUnit.ConsoleRunner.3.6.0/tools/nunit3-console.exe", "./Toolhouse.Monitoring.Tests/bin/Release/Toolhouse.Monitoring.Tests.dll" ]
