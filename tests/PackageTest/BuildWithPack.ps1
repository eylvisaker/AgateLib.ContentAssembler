dotnet new -i MonoGame.Template.CSharp::3.7.0.7
dotnet new mgdesktopgl -o Test --force

cp Game1.cs Test/

mkdir Test/ContentIn
cp Pointer.png Test/ContentIn
cp content.build Test

cd Test

dotnet add package AgateLib.ContentAssembler --source ../../../src/AgateLib.ContentAssembler/bin/Release