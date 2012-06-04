require 'rubygems'
require 'erb'
require 'fileutils'
require 'configatron'
require 'albacore'

MSBUILD_PATH = "C:/Windows/Microsoft.NET/Framework/v4.0.30319/"
BUILD_PATH = File.expand_path('build')
ARTIFACTS_PATH = File.expand_path('artifacts')
LIB_PATH = File.expand_path('lib')
SOLUTION = 'src/RabbitBus.sln'
COMPILE_TARGET = 'Release'
NUGET_CACHE= File.join(ENV['LOCALAPPDATA'], '/NuGet/Cache/')
FEEDS = ['https://go.microsoft.com/fwlink/?LinkID=206669' ]
nuget = 'nuget'
SHELL_DEPENDENCIES = ['nuget.exe']
load 'VERSION.txt'
load 'packages.rb'
if(ENV['BUILD_NUMBER']) then VERSION="#{VERSION}" + '.' + ENV['BUILD_NUMBER'] end


task :default => ["all"]

task :all => [:verify, :clean, :dependencies, :compile, :specs, :package]

task :verify do
	
	puts "Verifying dependencies ..."
	SHELL_DEPENDENCIES.each do |dep|
		verify(dep) or fail "Could not find #{dep} in the system path."
	end
end

assemblyinfo :versioning do |asm|
	asm.output_file = "src/CommonAssemblyInfo.cs"
	asm.version = "#{VERSION}"
end

task :clean do
	rm_rf "#{BUILD_PATH}"
	rm_rf "#{ARTIFACTS_PATH}"
end

task :compile => [:versioning] do

	mkdir "#{BUILD_PATH}"
	sh "#{MSBUILD_PATH}msbuild.exe /p:Configuration=#{COMPILE_TARGET} #{SOLUTION}"
	copyOutputFiles "src/RabbitBus/bin/#{COMPILE_TARGET}", "*.{dll,pdb}", "#{BUILD_PATH}"
	copyOutputFiles "src/RabbitBus.Serialization.Json/bin/#{COMPILE_TARGET}", "*.{dll,pdb}", "#{BUILD_PATH}"
end

task :specs do
	specs = FileList.new("src/RabbitBus.Specs/bin/#{COMPILE_TARGET}/*.Specs.dll")
	puts specs
	sh "lib/Machine.Specifications/tools/mspec-x86-clr4.exe -x integration #{specs}"
end

task :package do
	mkdir_p "#{ARTIFACTS_PATH}"
	rm Dir.glob("#{ARTIFACTS_PATH}/*.nupkg")
	FileList["packaging/nuget/*.nuspec"].each do |spec|
		sh "#{nuget} pack #{spec} -o #{ARTIFACTS_PATH} -Version #{VERSION} -Symbols -BasePath ."
	end
end

task :dependencies do
	feeds = FEEDS.map {|x|"-Source " + x }.join(' ')
	configatron.packages.each do | name,version |
		packageFeeds = "-Source #{NUGET_CACHE} " + feeds unless !version
		packageExists = File.directory?("#{LIB_PATH}/#{name}")
		versionInfo="#{LIB_PATH}/#{name}/version.info"
		currentVersion=IO.read(versionInfo) if File.exists?(versionInfo)
		if(!packageExists or !version or !versionInfo or currentVersion != version) then
			versionArg = "-Version #{version}" unless !version
			sh "nuget Install #{name} #{versionArg} -o #{LIB_PATH} #{packageFeeds} -ExcludeVersion" do | ok, results |
				File.open(versionInfo, 'w') {|f| f.write(version) } unless !ok
			end
		end
	end
end

task :publish => [:all] do
	FileList["#{ARTIFACTS_PATH}/*.nupkg"].gsub(File::SEPARATOR,
     File::ALT_SEPARATOR || File::SEPARATOR).each do | file |
		sh "nuget push #{file}"
	end
end

def verify(command)
	ENV['PATH'].split(/;/).each do | path |
		if(File.exists?(File.join("#{path}", command)))
			return true
		end
	end

	return false
end

def copyOutputFiles(fromDir, filePattern, outDir)
	Dir.glob(File.join(fromDir, filePattern)){|file| 		
		copy(file, outDir) if File.file?(file)
	} 
end

