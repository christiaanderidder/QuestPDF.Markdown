#!/bin/sh
apt-get -y update &&
apt-get install rename &&
dotnet test ;
cd tests/QuestPDF.Markdown.Tests/Verify &&
echo $PWD &&
rename -v -f 's/\.received\./.verified./' *.*