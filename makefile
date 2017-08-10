# important variables
modname = Waterbucket
version = 1.1

moddir = Scarabol/$(modname)
zipname = $(moddir)/Colony$(modname)Mod-$(version)-mods.zip
dllname = $(modname).dll

#
# actual build targets
#

default:
	mcs /target:library -r:../../../../colonyserver_Data/Managed/Assembly-CSharp.dll -r:../../Pipliz/APIProvider/APIProvider.dll -r:../../../../colonyserver_Data/Managed/UnityEngine.dll -out:"$(dllname)" -sdk:2 src/*.cs
	echo '{\n\t"assemblies" : [\n\t\t{\n\t\t\t"path" : "$(dllname)",\n\t\t\t"enabled" : true\n\t\t}\n\t]\n}' > modInfo.json

clean:
	rm -f "$(dllname)" "modInfo.json"

all: clean default

release: default
	rm -f "$(zipname)"
	cd ../../ && zip -r "$(zipname)" "$(moddir)/modInfo.json" "$(moddir)/$(dllname)"

