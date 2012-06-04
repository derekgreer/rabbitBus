RabbitBus Build Instructions
----------------------------

1. Download the latest version of the NuGet Command Line bootstrapper from
   http://nuget.codeplex.com/releases/view/58939 and add the executable to
   your PATH environment variable.

1. Download and install the latest Ruby Installer from http://rubyinstaller.org/.


2. Run the following commands to install the ruby gems used by the rake build:

	gem install albacore
	gem install configatron

	(Note: If you are behind a firewall, set the HTTP_PROXY environment variable.)

3. Run 'rake' from project root.  This will retrieve the required project dependencies and
   create the CommonAssemblyInfo.cs referenced by the project.
