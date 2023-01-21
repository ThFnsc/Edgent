@echo off
dotnet publish -c Release src/ThFnsc.Edgent -o publish
explorer publish
pause