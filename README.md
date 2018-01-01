# MSSQLTools

One of the most repetative tasks that i have to perform is to write Create, Update, Select and Delete stored procedures based on tables.
To make this a bit eaiser I normally create a small tool that puts them together, I thought this might be useful for others as well.

This script format is based on the style guide published here [Style Guide](https://github.com/jimmywyatt/SQLStyleGuide).

## Creating your scripts

Download the sourcecode and run the project, it will ask you to choose a server, database and output folder into which the scripts will be placed.

## Batch running scripts

Same as above, it will run any scripts in the sub directories specified and log the results.

## Backup

Can be useful if you need to backup a data from the command line or thought the task scheduler.

## Running

You can run the tool by calling MSSQLTools from the command prompt and following the prompts or passing --help and seeing the possible given parameters.

## Licence

This project is licened under the MIT licence.
