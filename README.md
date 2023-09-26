# VsFileMaker
My goal with this project is to make a easy to use parser, for example here is how you make a .sln file without adding any Csproj to the sln
``` C#
SLN sln = new SLN("My Project Name");
File.WriteAllText($"{projectPath}/My Project Name.sln", sln.Serialize());
```
I should note this project is still a work-in-progress, if you want something more complete i recommend [MvsSln]
(https://github.com/3F/MvsSln)
# Building
You can directly clone the project into Visual Studio and it should work out of the box
# Supported features
1. Sln file parsing and generating
2. Csproj file generating
# Plan on Adding/Supporting
1. Csproj parsing
2. Vsproj parsing and generating
3. Built-in Folders

