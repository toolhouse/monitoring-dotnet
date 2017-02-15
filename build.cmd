REM Builds the project.

nuget restore

pushd Toolhouse.Monitoring

msbuild ^
    /p:Configuration=Release ^
    || (popd && exit /B 101)

popd
