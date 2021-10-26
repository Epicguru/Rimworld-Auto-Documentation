# Rimworld XML Auto-Documentation

### [View the latest version here](https://htmlpreview.github.io/?https://github.com/Epicguru/Rimworld-Auto-Documentation/blob/master/Latest.html).
You can save a local copy by opening the link above, then *Right clicking > Save as...*

## Info

Auto-generated 'documentation' of all vanilla and DLC defs. Intended as a tool to help modders and learners.
Should be used in conjunction with a good text editor i.e. VsCode and ideally a decompiler such as DnSpy.

This is closely based on [milon's orignal tool](https://ludeon.com/forums/index.php?topic=21440.0). See that forum post for more information about this tool, the information there will apply here too.

Compared to milon's version this is completely re-writen in C#, and apart from visual changes, has a few more features.
Notably, you can see examples (def name and file name) of values. **If you download the generator** (or compile the source yourself), you can also make
the documentation **link to the actual source files**, allowing you to open them in a single click.
Values are sorted from smallest to largest, or alphabetically if the values are not numbers.

## Using the generator
If you want more control, or more features, you can download the compiled generator from [the releases section](https://github.com/Epicguru/Rimworld-Auto-Documentation/releases).

The generator is a command-line tool. It must be given arguments:
 - **#0**: The version name, for display purposes.
 - **#1**: The path to the `Defs` folder. All sub-folders are also scanned.
 - **#2**: Optional path to another `Defs` folder, such as a mod folder.

![screen1](https://github.com/Epicguru/Rimworld-Auto-Documentation/blob/master/Images/CommandLine.png)

## Building
Simply clone repository, open solution in Visual Studio 2019 or 2022. You will need the .NET 5 build tools.

## Images and examples
![screen1](https://github.com/Epicguru/Rimworld-Auto-Documentation/blob/master/Images/Sorted.png)
![screen2](https://github.com/Epicguru/Rimworld-Auto-Documentation/blob/master/Images/ShowSourceExample2.png)
![screen3](https://github.com/Epicguru/Rimworld-Auto-Documentation/blob/master/Images/ShowSourceExample.png)
