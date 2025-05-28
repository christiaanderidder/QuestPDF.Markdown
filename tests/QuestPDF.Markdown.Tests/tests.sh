#!/bin/sh
apt-get -y update &&
apt-get install rename &&
dotnet test ;
cd tests/QuestPDF.Markdown.Tests/Verify &&
rename 's/\.received\./.verified./' *.*