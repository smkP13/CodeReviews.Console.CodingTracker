# CODING TRACKER FOR THECSHARPACADEMY
By Zube Pierre Basali




## Project Resume

As its name states, this project is a program to keep track of our coding sessions and different values of
	those sessions(for example the duration of each session).

To do that, Sqlite was used in conjonction with C# to create a database and store our coding sessions.


## My process of work

I first made a resume of what is needed for the project from more general tasks to more precise tasks to be able
	to keep track of what I have to do and what is left to do.

I was able to determine the general structure first and code it in an abstract way, as a skecth, then take care of
	everything step by step.

The code was first made to work as intended and produce the intended results with the knowledge I had, then the
	I implemented the thing I had to learn for this project. This helped me to understand what I was learning,
	how it works and how to deal with it.

This methodollogy might take more effort at first sight, but I found that it really makes thing easier to
	memorize and made me more able to reuse the acquired knowledge. I can also use the last acquired knowledge and
	apply it to the next project to further improve.


## Requirements

	- This application has the same requirements as the previous project, except that now you'll be logging your daily coding time.

	- To show the data on the console, you should use the "Spectre.Console" library.

	- You're required to have separate classes in different files (ex. UserInput.cs, Validation.cs, CodingController.cs)

	- You should tell the user the specific format you want the date and time to be logged and not allow any other format.

	- You'll need to create a configuration file that you'll contain your database path and connection strings.

	- You'll need to create a "CodingSession" class in a separate file. It will contain the properties of your coding session: Id, StartTime, EndTime, Duration

	- The user shouldn't input the duration of the session. It should be calculated based on the Start and End times, in a separate "CalculateDuration" method.

	- The user should be able to input the start and end times manually.

	- You need to use Dapper ORM for the data access instead of ADO.NET. (This requirement was included in Feb/2024)

	- When reading from the database, you can't use an anonymous object, you have to read your table into a List of Coding Sessions.

	- Follow the DRY Principle, and avoid code repetition.


## The project structure

The user navigate through a simple console based UI named contained in MainMenu.
Each menu is subdivided in different options to complete different tasks:
	- Start/End new coding session:
		Start/End a coding session using the current date and time
	- Insert new data:
		Allows to add data to a project or add a new project
	- Delete data:
		Allows to delete datas selected in a list
	- Update data:
		Allows to update start/end date/time for a specific data
	- Delete project:
		Allows to delete a project and all its data
	- Print single project data:
		Prints diverse reports for a specific project ordered by date
	- Print all data:
		Prints every single session of every project ordered by date
	- Set/Show coding goals
		Allows to create coding time goals to achieve
	- See current session duration:
		See the duration of the current session (new coding session)
	- Exit:
		Exit the program
	- Fill database for testing purpose:
		Fill the database with a table named test_table with values to test with


## Challenges and Thoughts

There are many challenges throughout the project such as deal with datetime values, work with sqlite,
	introduction to OOP(Object Oriented Programming) or MVC(Model, View, Controller) principles.

I found out that dealing with datetime in C# and Sqlite are different and can be a bit sketchy, as computers
	have their way to interpret numbers. It forced me to see how to deal with it by searching and looking from
	different angles to solve the problems encountered during it's creation and correction.

I didn't found how to have a stable time calculation using only Sqlite as it seems to increase imprecisions with larger times
	and dates. Therefore I separated date and time and calculate the duration using C# rather than Sqlite.

The data are ordered by starting date and time, so the id must be updated if a session is added,deleted, or updated. I had
	to play around with Sqlite to make it happened, following the advise I was given, I made id as the primary key and found ways
	to change the values so it won't produce any errors and stop the process.

The project take more times as I had many things to do outside of learning programmation, so it was a bit hard to get back into it
	sporadically. I hope to be able to dedicate more time soon to dive into it.


## Lessons Learned

I learned how to work with more complex Sqlite queries. I also learned more about how the syntax work, how to circonvent around
	the language limitation to achieve the set goals.

I learned to deal with DateTime in both C# and Sqlite, that they're handled differently despite being kind of similar. I still have more
	ease to work and solve problems in C#, but I tried to solve as many things as possible using Sqlite.

I learned the concept of "Separations of Interests" at a small scale, and how it makes the coede more easy to organize, update, correct
	and read.

I learned that using objects helps the program to run faster as it does not have to constantly navigate through each files to use the
	functions and access values.

I learned how to simplify Sqlite statements using Dapper.

I learned how to protect the program from some Sql Injections, but I can't tell if it is totally safe from it.


## Areas to Improve

- Preorganize my work better.
- Lear more about Sqlite syntax and format to deal with values
- Go for an OOP to simplify readability and update of the code
- Have a better use of Separations of Interests as for my first try, I had trouble defining if something must be separated or not
	(I also think a more diverse project will help further to developp this skill)
- My english skills, especially for naming, as I am more of a self-taught in english language
- Increase confidence in my capacity to learn and apply code


## Ressources

- https://www.sqlite.org/
- https://www.sqlitetutorial.net/
- https://stackoverflow.com/
- https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/store-custom-information-config-file
- https://www.learndapper.com/
- My precendent projects and personal projects and their ressources


## Thanks

- Hason23 for the advises and the correction
- thecsharpacademy for everything they give to us
- All the developper helping newcomers for free