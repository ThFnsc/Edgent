# ThFnsc.Edgent

## What?

This app is a background service that monitors your desktop and nukes the Microsoft Edge shortcut everytime Microsoft's abusive wisdom decides to put one in there against everybody's will.

## How?

Two ways:

First, it monitors your desktop for file changes. When a file of the name `Microsoft Edge.lnk` is created, it's immediately deleted.

Second, a script runs at startup and at every minute to seek and destroy manually, in case the first step fails for some reason.

## Why?

You'd think someone who would go to these lengths - *a couple lines of code, but still* - to get rid of a fucking shortcut must absolutely despize Edge. In my case, that's not actually true.

I quite like Edge, I think the development team did a great job making it. It's just not my favorite browser, not the one I'm the most productive in.

It's mostly because of what Microsoft is doing to push it. Is completely abusive, it's infuriating.

## I'm in

1. Download the files from the [latest release](https://github.com/ThFnsc/ThFnsc.Edgent/releases) or use the `publish.bat` script to build from source ([.NET 7 SDK required](https://dotnet.microsoft.com/en-us/download/dotnet/7.0))
2. Move the files somewhere they won't be messed with (I did not want to bother with a whole installer just for this. The executable is *THE* executable)
3. Run the `install.bat` script (just executes `ThFnsc.Edgent.exe install` for you which sets a registry key for auto startup and spins up a new instance)
4. Enjoy a clean desktop

### I'm out

1. As you could've probably guessed, run the `uninstall.bat` script (also just a shortcut for the command `ThFnsc.Edgent.exe uninstall`)
2. Delete the files, if you wish